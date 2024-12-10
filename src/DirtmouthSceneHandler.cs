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
            //On.AudioManager.BeginApplyMusicCue += OnAudioManagerBeginApplyMusicCue;
            //On.AudioManager.BeginApplyAtmosCue += OnAudioManagerBeginApplyAtmosCue;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
        }

        public void OnSceneChange(Scene from, Scene to)
        {
            if (to.name == "Town")
            {

                SceneManager sm = GameManager.instance.sm;
                //sm.AmbientIntesityMix = 0.1f;
                sm.isWindy = false;

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

                // Tree
                GameObject tree = objab.transform.Find("_Decor/tree/listen").gameObject;
                tree.AddComponent<TreeListenHandler>();
                GameObject credits = tree.transform.Find("credits").gameObject;
                credits.SetActive(false);
                Logger.Info("Setup Christmas Dirtmouth complete");
            }
            else if (bundle != null)
            {
                //Log.Info("Unloading asset");
                bundle.Unload(true);
            }
        }

        public void OnHeroUpdate()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                SceneManager sm = GameManager.instance.sm;
                FieldInfo gmField = sm.GetType().GetField("gm", BindingFlags.NonPublic | BindingFlags.Instance);
                GameManager gm = gmField.GetValue(sm) as GameManager;

                AtmosCue atmosCue = ScriptableObject.CreateInstance<AtmosCue>();
                gm.AudioManager.ApplyAtmosCue(atmosCue, 0.1f);

                MusicCue musicCue = ScriptableObject.CreateInstance<MusicCue>();
                gm.AudioManager.ApplyMusicCue(musicCue, 0f, 0.1f, false);
            }
        }
        private IEnumerator OnAudioManagerBeginApplyMusicCue(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
        {
            // Mute the background music in the scene to replace with our own
            if (!ChristmasInDirtmouth.GlobalData.ShopIntro)
            {
                musicCue = ScriptableObject.CreateInstance<MusicCue>();
            }
            yield return orig(self, musicCue, delayTime, transitionTime, applySnapshot);
        }

        private IEnumerator OnAudioManagerBeginApplyAtmosCue(On.AudioManager.orig_BeginApplyAtmosCue orig, AudioManager self, AtmosCue musicCue, float transitionTime)
        {
            if (!ChristmasInDirtmouth.GlobalData.ShopIntro)
            {
                musicCue = ScriptableObject.CreateInstance<AtmosCue>();
            }
            yield return orig(self, musicCue, transitionTime);
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

    public class TreeListenHandler : MonoBehaviour
    {
        private GameObject doorArrowPrompt;
        private bool door_active = false;

        private void Awake()
        {
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            doorArrowPrompt = CreatePromptPrehab();
            doorArrowPrompt.SetActive(true);
        }
        private GameObject CreatePromptPrehab(string text = "LISTEN")
        {
            GameObject prefab = new GameObject("Tree Arrow");
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
                SceneManager sm = GameManager.instance.sm;
                FieldInfo gmField = sm.GetType().GetField("gm", BindingFlags.NonPublic | BindingFlags.Instance);
                GameManager gm = gmField.GetValue(sm) as GameManager;

                AtmosCue atmosCue = ScriptableObject.CreateInstance<AtmosCue>();
                gm.AudioManager.ApplyAtmosCue(atmosCue, 0.1f);

                MusicCue musicCue = ScriptableObject.CreateInstance<MusicCue>();
                gm.AudioManager.ApplyMusicCue(musicCue, 0f, 0.1f, false);

                // Back ground music
                AudioSource audio = transform.Find("audio").GetComponent<AudioSource>();
                audio.Play();

                // Enable Credits
                GameObject credits = transform.Find("credits").gameObject;
                Animator animator = credits.GetComponent<Animator>();
                animator.Play("CreditsAnimation", -1, 0f);
                credits.SetActive(true);
                Logger.Info("here!");

            }
        }
    }
}
