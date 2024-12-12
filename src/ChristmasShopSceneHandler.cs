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
using System.Security.AccessControl;
using System.ComponentModel;
using System.Collections;
using HutongGames.PlayMaker;
using GlobalEnums;
using static System.TimeZoneInfo;


namespace ChristmasInDirtmouth
{
    public class ChristmasShopSceneHandler
    {
        public static string Name = "ChristmasShopScene";
        public static string Gate = "left_01";
        public static string Path;
        public static bool SceneActive = false;

        private CustomScene sceneObj;
        private AssetBundle bundle;
        private const string RESOURCE_PATH = "ChristmasInDirtmouth.Resources.christmasshopscene";
        private static Satchel.Core satchelCore = new Satchel.Core();
        private static GameObject ShopPrefab, ShopMenu, SpritePrefab;

        private bool boughtTest = false;

        public ChristmasShopSceneHandler(GameObject refTileMap, GameObject refSceneManager, GameObject shopRegion, GameObject shopMenu, GameObject corniferCard)
        {
            ModHooks.LanguageGetHook += OnLanguageGet;
            On.ShopMenuStock.SpawnStock += SpawnStock;
            On.AudioManager.BeginApplyMusicCue += OnAudioManagerBeginApplyMusicCue;
            //On.AudioManager.BeginApplyAtmosCue += OnAudioManagerBeginApplyAtmosCue;

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
            settings.saturation = 1.0f;
            settings.heroLightColor = new Color(1.0f, 0.95f, 0.55f, 0.25f);
            settings.isWindy = false;
            settings.defaultColor = new Color(1.0f, 0.95f, 0.55f, 0.8f);
            // 0 = Dust, 1 = Grass, 2 = Bone, 3 = Spa, 4 = Metal, 5 = No Effect, 6 = Wet
            settings.environmentType = 1;
            sceneObj.Config(40, 25, settings);
            // Satchel will handle set up of the scene manager, but we need our own call back for modifying objects
            // Add call back to load the scene on change
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;

            // Store preloaded fabs
            ShopPrefab = shopRegion;
            ShopMenu = shopMenu;
        }

        private void OnSceneChange(Scene from, Scene to)
        {
            var currentScene = to.name;
            SceneActive = (to.name == ChristmasShopSceneHandler.Name);
            if (SceneActive)
            {
                // Back ground music
                AudioSource audio = GameObject.Find("root/audio").GetComponent<AudioSource>();
                audio.Play();

                Logger.Debug("Christmas shop interior scene change");
                // Manually replicating because I already have game objects in the scene
                // https://github.com/PrashantMohta/Satchel/blob/master/Utils/SceneUtils.cs#L144
                GameObject gate = GameObject.Find(ChristmasShopSceneHandler.Gate).gameObject;
                var tp = gate.AddComponent<TransitionPoint>();

                // Set up gate back to town
                tp.isADoor = false;
                tp.SetTargetScene("Town");
                tp.entryPoint = "door_christmas_shop";
                tp.alwaysEnterLeft = true;
                tp.alwaysEnterRight = false;

                GameObject rm = gate.transform.Find("Hazard Respawn Marker").gameObject;
                tp.respawnMarker = rm.GetAddComponent<HazardRespawnMarker>();
                tp.respawnMarker.respawnFacingRight = true;
                tp.sceneLoadVisualization = GameManager.SceneLoadVisualizations.Default;


                ChristmasInDirtmouth.ResetPrefabMaterials(GameObject.Find("root"));
                SpritePrefab = GameObject.Find("root/sprites");
                SpritePrefab.SetActive(false);

                GameObject npc = GameObject.Find("root/npc").gameObject;
                npc.AddComponent<NPCDialog>();

                // Initialize shop object
                var go = GameObject.Instantiate(ShopPrefab);
                go.transform.position = GameObject.Find("root/shop").gameObject.transform.position;
                go.SetActive(true);

                // Initilize shop UI menu object
                // https://github.com/homothetyhk/HollowKnight.ItemChanger/blob/master/ItemChanger/Locations/CustomShopLocation.cs#L96
                GameObject menu = GameObject.Instantiate(ShopMenu);
                menu.SetActive(true);

                var itemList = menu.Find("Item List").gameObject;
                ShopMenuStock stock = itemList.GetComponent<ShopMenuStock>();
                PlayMakerFSM fsmItemControl = itemList.LocateMyFSM("Item List Control");

                // Modify the buy FSM to add a specific type
                GameObject uilist = menu.Find("Confirm").gameObject.Find("UI List").gameObject;
                PlayMakerFSM confirmFSM = uilist.LocateMyFSM("Confirm Control");
                FsmState state = confirmFSM.GetValidState("Special Type?");
                IntSwitch switchaction = (IntSwitch)state.GetAction(1);
                //FsmInt[] compareTo = new FsmInt[switchaction.compareTo.Length+1];
                FsmEvent[] sendEvent = new FsmEvent[switchaction.sendEvent.Length + 1];
                Satchel.FsmUtil.AddCustomAction(state, () =>
                    {
                        if (confirmFSM.FsmVariables.IntVariables[4].Value == 18)
                        {

                            int cost = confirmFSM.FsmVariables.IntVariables[0].Value;
                            // Item ID in the "Confirm Control" FSM is always 0 for some reason
                            int itemMenuId = fsmItemControl.FsmVariables.IntVariables[3].Value;

                            Logger.Info(String.Format("Christmas Item Bought: {0:D} for {0:D} geo", itemMenuId, cost));
                            // Reuse, normal event in switch action to trigger a complete
                            switchaction.Event(sendEvent[0]);
                            // Set item in our inventory to true that we just bought. Index is based on position in shop
                            int j = 0;
                            for (int i = 0; i < ModItems.NUM_ITEMS; i++)
                            {
                                if (!ChristmasInDirtmouth.GlobalData.HeroInventory[i])
                                {
                                    if(j == itemMenuId)
                                    {
                                        ChristmasInDirtmouth.GlobalData.HeroInventory[i] = true;
                                        i = ModItems.NUM_ITEMS;
                                    }
                                    else
                                    {
                                        j += 1;
                                    }
                                }
                            }
                            // Used for inspiration
                            // https://github.com/homothetyhk/HollowKnight.ItemChanger/blob/a2bcdd59284ed6aa4e82ca308b4dc23105ccc72c/ItemChanger/Util/ShopUtil.cs#L98
                            MethodInfo dynMethod = stock.GetType().GetMethod("SpawnStock", BindingFlags.NonPublic | BindingFlags.Instance);
                            dynMethod.Invoke(stock, new object[] { });
                        }
                    }
                );

                // Interesting FSM editting reference
                // https://github.com/SFGrenade/TestOfTeamwork/blob/f2fb8212fa4cc2a725c29e57ca5e2675383adc82/src/MonoBehaviours/WeaverPrincessBossIntro.cs#L45
                PlayMakerFSM fsm = go.LocateMyFSM("Shop Region");
                // Skip and remove the edits to the player data variable "metSlyShop"
                Satchel.FsmUtil.ChangeTransition(fsm, "Intro Convo?", "YES", "Title Up");
                Satchel.FsmUtil.RemoveAction(fsm.GetValidState("Box Up"), 0);
                // Skip any voice lines
                Satchel.FsmUtil.ChangeTransition(fsm, "Box Up", "FINISHED", "Convo");
                // Handle intro message (modify the bool based on the ShopIntro)
                fsm.FsmVariables.GetFsmBool("Intro Msg").Value = ChristmasInDirtmouth.GlobalData.ShopIntro;
                state = fsm.GetValidState("Intro Convo?");
                Satchel.FsmUtil.InsertCustomAction(state, () =>
                    {
                        if (!ChristmasInDirtmouth.GlobalData.ShopIntro)
                        {
                            // For some reason works, trying to set the value breaks stuff?
                            go.LocateMyFSM("Shop Region").FsmVariables.GetFsmBool("Intro Msg").Clear();
                        }
                        ChristmasInDirtmouth.GlobalData.ShopIntro = false;
                    }
                , 0);

                // Modify out of stock FSM to not play any audio (could replace)
                fsm = menu.LocateMyFSM("shop_control");
                state = fsm.GetValidState("Sly");
                Satchel.FsmUtil.RemoveAction(state, 6);

                state = fsm.GetValidState("Sly 2");
                Satchel.FsmUtil.RemoveAction(state, 5);

                Logger.Success("Setup Christmas shop scene complete");
            }
        }

        private void SceneOnload(object sender, SceneLoadedEventArgs e)
        {
            Logger.Info("Christmas Shop Scene loading complete");
        }

        // Hook for replacing text in pop-up menus / title cards
        // Instead of creating new, we will intercept existing
        // https://github.com/ToboterXP/HollowKnight.TheGlimmeringRealm/blob/5183853ec31ece532549a6cf88d0b303a8d0fd7e/TextChanger.cs
        // https://github.com/ToboterXP/HollowKnight.TheGlimmeringRealm/blob/5183853ec31ece532549a6cf88d0b303a8d0fd7e/Rooms/Village1/Village1.cs#L24
        public string OnLanguageGet(string key, string sheetTitle, string orig)
        {
            if (!SceneActive) { return orig; }
            //Debug.Log("==== " + key + "  " + sheetTitle + ": ");
            // Replacing NPC title (pulled from the FSM of the sly shop)
            if ((sheetTitle == "Sly" || sheetTitle == "Titles" || sheetTitle == "SatchelCustomDialogue") && ModItems.NPCMap.ContainsKey(key))
            {
                return ModItems.NPCMap[key];
            }
            // Replacing UI / Price values if present
            if (sheetTitle == "UI" && ModItems.ShopMap.ContainsKey(key)){
                return ModItems.ShopMap[key];
            }
            if (sheetTitle == "Prices" && ModItems.PriceMap.ContainsKey(key))
            {
                return ModItems.PriceMap[key];
            }
            return orig;
        }

        private void SpawnStock(On.ShopMenuStock.orig_SpawnStock orig, ShopMenuStock self)
        {
            if (self.stock.Length == 0) { orig(self); return; }
            if (!SceneActive) { orig(self); return; }

            self.itemCount = -1;
            self.stockInv = new GameObject[self.stock.Length];

            Logger.Debug("Rebuilding custom shop");

            GameObject prefab = self.stock[0];
            // Initialize shop items
            int itemCount = ChristmasInDirtmouth.GlobalData.HeroInventory.Where(c => !c).Count();
            self.stock = new GameObject[itemCount];
            int j = 0;
            for (int i = 0; i < ModItems.NUM_ITEMS; i++)
            {
                // If item is not in the heros inventory
                if (!ChristmasInDirtmouth.GlobalData.HeroInventory[i] && j < self.stock.Length)
                {
                    Sprite itemSprite = SpritePrefab.Find(String.Format("{0:D}", i)).GetComponent<SpriteRenderer>().sprite;
                    CustomItemStats item = ModItems.ChristmasItemStats[i];
                    self.stock[j] = GameObject.Instantiate(prefab, prefab.transform.parent);
                    self.stock[j].name = String.Format("ShopItem_{0:D}", i);
                    self.stock[j].GetComponent<ShopItemStats>().nameConvo = item.nameConvo;
                    self.stock[j].GetComponent<ShopItemStats>().descConvo = item.descConvo;
                    self.stock[j].GetComponent<ShopItemStats>().priceConvo = item.priceConvo;
                    self.stock[j].GetComponent<ShopItemStats>().requiredPlayerDataBool = item.requiredPlayerDataBool;
                    self.stock[j].GetComponent<ShopItemStats>().playerDataBoolName = item.playerDataBoolName;
                    self.stock[j].GetComponent<ShopItemStats>().specialType = item.specialType;
                    self.stock[j].GetComponent<ShopItemStats>().charmsRequired = item.charmsRequired;
                    self.stock[j].GetComponent<ShopItemStats>().relicNumber = item.relicNumber;
                    self.stock[j].transform.Find("Item Sprite").GetComponent<SpriteRenderer>().sprite = itemSprite;
                    self.stock[j].GetComponent<ShopItemStats>().itemNumber = i;
                    self.stock[j].SetActive(false);
                    j += 1;
                }
            }

            var spawnedStock = new Dictionary<GameObject, GameObject>(self.stock.Length);
            GameObject[] array = self.stock;
            foreach (GameObject gameObject in array)
            {
                GameObject gameObject2 = GameObject.Instantiate(gameObject);
                gameObject2.SetActive(value: false);
                spawnedStock.Add(gameObject, gameObject2);
            }

            // Edit the master component
            // Can be some odd null reference here, but trying to catch is cases some error?
            if (self.masterList.GetType() == typeof(GameObject))
            {
                var masterStock = self.masterList.GetComponent<ShopMenuStock>();
                masterStock.stock = self.stock;
                orig(self);
            }
        }

        // https://github.com/SFGrenade/CustomBgm/blob/master/src/CustomBgm.cs
        private IEnumerator OnAudioManagerBeginApplyMusicCue(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
        {
            // Mute the background music in the scene to replace with our own
            if (SceneActive)
            {
                musicCue = ScriptableObject.CreateInstance<MusicCue>();
            }
            yield return orig(self, musicCue, delayTime, transitionTime, applySnapshot);
        }

        private IEnumerator OnAudioManagerBeginApplyAtmosCue(On.AudioManager.orig_BeginApplyAtmosCue orig, AudioManager self, AtmosCue musicCue, float transitionTime)
        {
            if (SceneActive)
            {
                musicCue = ScriptableObject.CreateInstance<AtmosCue>();
            }
            yield return orig(self, musicCue, transitionTime);
        }
    }

    public class NPCDialog : MonoBehaviour
    {
        private GameManager gm;
        private GameObject doorArrowPrompt;
        private CustomDialogueManager dialogManager;

        private bool active = false;
        private int prev_convo = -1;

        private void Awake()
        {
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            gm = GameManager.instance;
            // Needs preloads["Cliffs_01"]["Cornifer Card"]; on mod init
            // Assumes CustomArrowPrompt.Prepare(CardPrefab); has been called
            doorArrowPrompt = CreatePromptPrehab();
            doorArrowPrompt.SetActive(true);
        }

        private GameObject CreatePromptPrehab(string text = "LISTEN")
        {
            // Needs preloads["Cliffs_01"]["Cornifer Card"]; on mod init
            // Assumes CustomArrowPrompt.Prepare(CardPrefab); has been called
            // https://github.com/PrashantMohta/Satchel/blob/2e922c2939ae35af0a256b5edd1792db0dbf0c92/Custom/CustomArrowPrompt.cs
            GameObject prefab = new GameObject("NPC Arrow");
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
                active = true;
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.name == "Knight" && Mathf.Abs(other.attachedRigidbody.velocity.x) < 0.1 && active)
            {
                doorArrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Show();
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.name == "Knight")
            {
                active = false;
                doorArrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Hide();
            }
        }

        public void OnHeroUpdate()
        {
            // In the modding tutorial you will see the use of Input.GetKeyDown
            // We dont use that here because we want to be agnostic to the key bind for up and also support controllers
            // User the inputHanlder instead
            if (GameManager.instance.inputHandler.inputActions.up.IsPressed && active)
            {
                active = false;
                doorArrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Hide();
                if (ChristmasInDirtmouth.GlobalData.ShopIntro)
                {
                    ChristmasInDirtmouth.MerryDialogueManager.ShowDialogue("SLY_SHOP_INTRO");
                    //ChristmasInDirtmouth.GlobalData.ShopIntro = false;
                }
                else
                {
                    // Random dialog
                    System.Random rnd = new System.Random();
                    int index = rnd.Next(1, 4);
                    // Ensure we dont get the same message twice in a row
                    if (prev_convo == index)
                    {
                        index += 1;
                        if (index == 4) { index = 1; }
                    }
                    ChristmasInDirtmouth.MerryDialogueManager.ShowDialogue(String.Format("MERRY_DIALOG_{0:d}", index));
                    prev_convo = index;
                }
                
            }
        }
    }
}
