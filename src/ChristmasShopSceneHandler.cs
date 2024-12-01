using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Satchel;
using static Satchel.SceneUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Modding;
using static Mono.Security.X509.X520;
using InControl;
using static CinematicSkipPopup;
using HutongGames.PlayMaker.Actions;

namespace ChristmasInDirtmouth
{
    public class ChristmasShopSceneHandler
    {
        public static string Name = "ChristmasShopScene";
        public static string Gate = "left_01";
        public static string Path;

        private CustomScene sceneObj;
        private AssetBundle bundle;
        private const string RESOURCE_PATH = "ChristmasInDirtmouth.Resources.christmasshopscene";
        private static Satchel.Core satchelCore = new Satchel.Core();
        private static GameObject ShopPrefab;
        private static bool SceneActive = false;

        public ChristmasShopSceneHandler(GameObject refTileMap, GameObject refSceneManager, GameObject shopRegion)
        {
            ModHooks.LanguageGetHook += OnLanguageGet;
            // Load scene bundle
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream(RESOURCE_PATH))
            {
                byte[] buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                s.Dispose();
                Logger.Debug("Loading bundle: " + RESOURCE_PATH);
                bundle = AssetBundle.LoadFromMemory(buffer);
            }
            Path = bundle.GetAllScenePaths()[0];

            // Use Satchel to create a custome scene
            // https://github.com/PrashantMohta/Satchel/blob/master/Core.cs#L177
            sceneObj = satchelCore.GetCustomScene(ChristmasShopSceneHandler.Name, refTileMap, refSceneManager);
            sceneObj.OnLoaded += SceneOnload;

            // Can edit properties in the settings object, will by default use the value in the reference scene manager
            // which is the mappers store room in this case
            // https://github.com/PrashantMohta/Satchel/blob/master/Utils/SceneUtils.cs#L36
            CustomSceneManagerSettings settings = new SceneUtils.CustomSceneManagerSettings(refSceneManager.GetComponent<SceneManager>());
            settings.saturation = 1.2f;
            settings.heroLightColor = new Color(1.0f, 0.95f, 0.55f, 0.05f);
            sceneObj.Config(40, 25, settings);
            // Satchel will handle set up of the scene manager, but we need our own call back for modifying objects
            // Add call back to load the scene on change
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;

            // Store preloaded fabs
            Logger.Info(shopRegion.ToString());
            ShopPrefab = shopRegion;
        }

        private void OnSceneChange(Scene from, Scene to)
        {
            var currentScene = to.name;
            SceneActive = (to.name == ChristmasShopSceneHandler.Name);
            if (SceneActive)
            {
                
                //ChristmasInDirtmouth.ResetPrefabMaterials(GameObject.Find("root"));
                Logger.Info("Casino interior scene change");
                // Manually replicating because I already have game objects in the scene
                // https://github.com/PrashantMohta/Satchel/blob/master/Utils/SceneUtils.cs#L144
                GameObject gate = GameObject.Find(ChristmasShopSceneHandler.Gate).gameObject;
                var tp = gate.AddComponent<TransitionPoint>();

                // See Casino Exterior for linked Gate
                tp.isADoor = false;
                tp.SetTargetScene("Town");
                tp.entryPoint = "door_christmas_shop";
                tp.alwaysEnterLeft = true;
                tp.alwaysEnterRight = false;

                GameObject rm = gate.transform.Find("Hazard Respawn Marker").gameObject;
                tp.respawnMarker = rm.GetAddComponent<HazardRespawnMarker>();
                tp.respawnMarker.respawnFacingRight = true;
                tp.sceneLoadVisualization = GameManager.SceneLoadVisualizations.Default;

                var go = GameObject.Instantiate(ShopPrefab);
                go.transform.position = GameObject.Find("root/shop").gameObject.transform.position;
                go.SetActive(true);

                // Interesting FSM editting reference
                // https://github.com/SFGrenade/TestOfTeamwork/blob/f2fb8212fa4cc2a725c29e57ca5e2675383adc82/src/MonoBehaviours/WeaverPrincessBossIntro.cs#L45
                PlayMakerFSM fsm = go.LocateMyFSM("Shop Region");

                fsm.FsmVariables.GetFsmBool("Intro Msg").Value = true;
                fsm.FsmVariables.GetFsmBool("Relic Dealer").Value = false;

                //Satchel.Futils.FsmVariables.SetVariables(fsm, "NPC Title", "Test");
                //fsm.FsmVariables.GetFsmString("NPC Title").Value = "Test";
                //fsm.FsmVariables.Get
                //var call_state = fsm.GetValidState("Title Up");
                //call_state



                Satchel.FsmUtil.ChangeTransition(fsm, "Box Up", "FINISHED", "Convo");

                Satchel.FsmUtil.ChangeTransition(fsm, "Relic Dealer?", "NO", "Regain Control");

                //GameObject cm = GameObject.Find("CameraLockArea").gameObject;
                //var cl = cm.AddComponent<CameraLockArea>();
                //cl.cameraXMin = 15;
                //cl.cameraXMax = 17;
                //cl.cameraYMin = 10;
                //cl.cameraYMax = 22;
                //cl.preventLookDown = true;
                //cl.preventLookUp = true;

                // Adding shop behavior
                GameObject root = GameObject.Find("root/shop").gameObject;
                //root.AddComponent<ChristmasShopHandler>();

                Logger.Warning("Gate Set up");
            }
        }

        private void SceneOnload(object sender, SceneLoadedEventArgs e)
        {
            Logger.Info("Casino Scene loading complete");
        }


        // Hook for replacing text in pop-up menus / title cards
        // Instead of creating new, we will intercept existing
        // https://github.com/ToboterXP/HollowKnight.TheGlimmeringRealm/blob/5183853ec31ece532549a6cf88d0b303a8d0fd7e/TextChanger.cs
        public string OnLanguageGet(string key, string sheetTitle, string orig)
        {
            Logger.Info("Hit");
            if (!SceneActive) { return orig; }

            Logger.Info("==== " + key + "  " + sheetTitle + ": ");
            // Replacing NPC title (pulled from the FSM of the sly shop)
            if (key == "SLY_SHOP_INTRO" && sheetTitle == "Sly")
            {
                return "This is a test message <page> Test Message 2!";
            }
            else if (key == "SLY_MAIN" && sheetTitle == "Titles")
            {
                return "Merrywisp";
            }
            else if (key == "SLY_SUB" && sheetTitle == "Titles")
            {
                return "";
            }
            else if (key == "SLY_SUPER" && sheetTitle == "Titles")
            {
                return "Festive Knight";
            }
            return orig;
        }

    }


    public class ChristmasShopHandler : MonoBehaviour
    {
        private GameManager gm;
        private GameObject doorArrowPrompt;

        private bool door_active = false;

        private void Awake()
        {
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            gm = GameManager.instance;
            doorArrowPrompt = CreatePromptPrehab();
            doorArrowPrompt.SetActive(true);
        }

        private GameObject CreatePromptPrehab(string text = "SHOP")
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
                // Trigger a scene transition
                
            }
        }
    }
}
