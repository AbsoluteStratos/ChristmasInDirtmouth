using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GlobalEnums;
using Modding;
using Satchel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChristmasInDirtmouth
{
    public class DirtmouthSceneHandler
    {
        private AssetBundle bundle;
        private const string RESOURCE_PATH = "ChristmasInDirtmouth.Resources.christmasindirtmouthanchor";
        public DirtmouthSceneHandler()
        {
            // Hook onto unity scene change
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        }

        public void OnSceneChange(Scene from, Scene to)
        {
            if (to.name == "Town")
            {
                if (bundle == null)
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    using (Stream s = asm.GetManifestResourceStream(RESOURCE_PATH))
                    {
                        byte[] buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Dispose();
                        // Log.Info("Loading bundle: " + bundle_name);
                        bundle = AssetBundle.LoadFromMemory(buffer);
                    }
                }
                // Note the path is based on where you saved the prefab
                GameObject objab = GameObject.Instantiate(bundle.LoadAsset<GameObject>("Assets/ChristmasInDirtmouth/ChristmasInDirtmouthAnchor.prefab"));
                objab.transform.position = UnityEngine.Vector3.zero;
                objab.SetActive(true);
                ChristmasInDirtmouth.ResetPrefabMaterials(objab);

                // Add a transition hook door collider
                // Note that doors are a little more tricky than other gates, other gates will run the animation for you but not doors
                // So instead what we will do is listen for the up / enter key press then trigger the transition and animation ourselves
                // See CasinoTownDoorHandler
                GameObject gate = objab.transform.Find("door_christmas_shop").gameObject;
                gate.AddComponent<ChristmasShopDoorHandler>();
                var tp = gate.AddComponent<TransitionPoint>();
                tp.isADoor = true;
                tp.alwaysEnterLeft = false;
                tp.alwaysEnterRight = false;

                GameObject rm = objab.transform.Find("door_christmas_shop/Hazard Respawn Marker").gameObject;
                tp.respawnMarker = rm.AddComponent<HazardRespawnMarker>();
                tp.respawnMarker.respawnFacingRight = true;
                tp.sceneLoadVisualization = GameManager.SceneLoadVisualizations.Default;
            }
            else if (bundle != null)
            {
                //Log.Info("Unloading asset");
                bundle.Unload(true);
            }
        }
    }

    public class ChristmasShopDoorHandler : MonoBehaviour
    {
        private GameManager gm;
        private tk2dSpriteAnimator heroAnim;
        private GameObject doorArrowPrompt;

        private bool door_active = false;

        private void Awake()
        {
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            gm = GameManager.instance;
            heroAnim = HeroController.instance.GetComponent<tk2dSpriteAnimator>();
            doorArrowPrompt = CreatePromptPrehab();
            doorArrowPrompt.SetActive(true);
        }

        private GameObject CreatePromptPrehab(string text = "ENTER")
        {
            // Needs preloads["Cliffs_01"]["Cornifer Card"]; on mod init
            // Assumes CustomArrowPrompt.Prepare(CardPrefab); has been called
            // https://github.com/PrashantMohta/Satchel/blob/2e922c2939ae35af0a256b5edd1792db0dbf0c92/Custom/CustomArrowPrompt.cs
            GameObject prefab = new GameObject("Door Arrow");
            prefab.transform.position = transform.Find("Prompt Marker").position;
            CustomArrowPrompt.GetAddCustomArrowPrompt(prefab, text, null);
            prefab.SetActive(false);
            DontDestroyOnLoad(prefab);

            return prefab;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.name == "Knight")
            {
                door_active = true;
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.name == "Knight" && Mathf.Abs(other.attachedRigidbody.velocity.x) < 0.1)
            {
                doorArrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Show();
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.name == "Knight")
            {
                door_active = false;
                doorArrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Hide();
            }
        }

        public void OnHeroUpdate()
        {
            // In the modding tutorial you will see the use of Input.GetKeyDown
            // We dont use that here because we want to be agnostic to the key bind for up and also support controllers
            // User the inputHanlder instead
            if (GameManager.instance.inputHandler.inputActions.up.IsPressed && door_active)
            {
                door_active = false;
                // Trigger a scene transition
                gm.BeginSceneTransition(new GameManager.SceneLoadInfo
                {
                    SceneName = ChristmasShopSceneHandler.Name,
                    EntryGateName = ChristmasShopSceneHandler.Gate,
                    HeroLeaveDirection = GatePosition.bottom,
                    EntryDelay = 0.2f,
                    WaitForSceneTransitionCameraFade = true,
                    PreventCameraFadeOut = false,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = false,
                    forceWaitFetch = false
                });
                // Must be after, the Begin transition will cancel animation if before
                HeroController.instance.StartCoroutine(PlayExitAnimation());
            }
        }

        private IEnumerator PlayExitAnimation()
        {
            HeroController.instance.StopAnimationControl();
            // Used door_bretta FSM in Town scene to figure out the name
            yield return heroAnim.PlayAnimWait("Enter");

            HeroController.instance.StartAnimationControl();

            yield break;
        }
    }
}
