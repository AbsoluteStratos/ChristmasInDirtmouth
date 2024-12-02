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
using FrogCore;
using HutongGames.PlayMaker;


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
        private static GameObject ShopPrefab, ShopMenu, SpritePrefab;
        private static bool SceneActive = false;


        private bool boughtTest = false;

        public ChristmasShopSceneHandler(GameObject refTileMap, GameObject refSceneManager, GameObject shopRegion, GameObject shopMenu)
        {
            ModHooks.LanguageGetHook += OnLanguageGet;
            //On.ShopMenuStock.BuildItemList += BuildItemList;
            On.ShopMenuStock.SpawnStock += SpawnStock;

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
            ShopMenu = shopMenu;
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

                

                SpritePrefab = GameObject.Find("root/sprites");
                ChristmasInDirtmouth.ResetPrefabMaterials(SpritePrefab);

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

                // Modify the buy FSM to add a specific type
                GameObject uilist = menu.Find("Confirm").gameObject.Find("UI List").gameObject;
                PlayMakerFSM confirmFSM = uilist.LocateMyFSM("Confirm Control");
                FsmState state = confirmFSM.GetValidState("Special Type?");
                IntSwitch switchaction = (IntSwitch)state.GetAction(1);
                //FsmInt[] compareTo = new FsmInt[switchaction.compareTo.Length+1];
                FsmEvent[] sendEvent = new FsmEvent[switchaction.sendEvent.Length + 1];
                Satchel.FsmUtil.AddCustomAction(state, () =>
                    {
                        Logger.Info(go.name.ToString() + " hit!");
                        if (confirmFSM.FsmVariables.IntVariables[4].Value == 18)
                        {
                            Logger.Info("Christmas Item Bought!");
                            Logger.Info("Item: " + confirmFSM.FsmVariables.IntVariables[1].Value.ToString() + " Cost: " + confirmFSM.FsmVariables.IntVariables[0].Value.ToString());
                            // Reuse, normal event in switch action to trigger a complete
                            switchaction.Event(sendEvent[0]);

                            boughtTest = true;

                            // Used for inspiration
                            // https://github.com/homothetyhk/HollowKnight.ItemChanger/blob/a2bcdd59284ed6aa4e82ca308b4dc23105ccc72c/ItemChanger/Util/ShopUtil.cs#L98
                            MethodInfo dynMethod = stock.GetType().GetMethod("SpawnStock", BindingFlags.NonPublic | BindingFlags.Instance);
                            dynMethod.Invoke(stock, new object[] { });
                        }
                    }
                );

                
                // Need to trigger initialization to inverstigate item list
                //itemList.SetActive(true);

                // Interesting FSM editting reference
                // https://github.com/SFGrenade/TestOfTeamwork/blob/f2fb8212fa4cc2a725c29e57ca5e2675383adc82/src/MonoBehaviours/WeaverPrincessBossIntro.cs#L45
                PlayMakerFSM fsm = go.LocateMyFSM("Shop Region");

                //fsm.FsmVariables.GetFsmBool("Relic Dealer").Value = false;

                fsm.FsmVariables.GetFsmBool("Intro Msg").Value = true; // Edit here based on mod settings
                // Skip and remove the edits to the player data variable "metSlyShop"
                Satchel.FsmUtil.ChangeTransition(fsm, "Intro Convo?", "YES", "Title Up");
                Satchel.FsmUtil.RemoveAction(fsm.GetValidState("Box Up"), 0);
                // Skip any voice lines
                Satchel.FsmUtil.ChangeTransition(fsm, "Box Up", "FINISHED", "Convo");
                // Modify the main shop state "Shop Up"
                var shopUp = fsm.GetValidState("Shop Up");
                if (shopUp != null)
                {
                    return; // already edited
                }
                Satchel.FsmUtil.ChangeTransition(fsm, "Relic Dealer?", "NO", "Regain Control");


                // Adding shop behavior
                //GameObject root = GameObject.Find("root/shop").gameObject;
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
        // https://github.com/ToboterXP/HollowKnight.TheGlimmeringRealm/blob/5183853ec31ece532549a6cf88d0b303a8d0fd7e/Rooms/Village1/Village1.cs#L24
        public string OnLanguageGet(string key, string sheetTitle, string orig)
        {
            if (!SceneActive) { return orig; }

             //Logger.Info("==== " + key + "  " + sheetTitle + ": ");
            Debug.Log("==== " + key + "  " + sheetTitle + ": ");
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

            else if (key == "INV_NAME_CHRISTMAS_ITEM_1" && sheetTitle == "UI")
            {
                return "Garland and Wreaths";
            }
            else if (key == "SHOP_DESC_CHRISTMAS_ITEM_1" && sheetTitle == "UI")
            {
                return "A collection of festive garland thats sure to bring a cherry mood to the town.";
            }

            else if (key == "CHRISTMAS_ITEM_1" && sheetTitle == "Prices")
            {
                return "5";
            }
            else if (key == "CHRISTMAS_ITEM_2" && sheetTitle == "Prices")
            {
                return "10";
            }
            else if (key == "CHRISTMAS_ITEM_3" && sheetTitle == "Prices")
            {
                return "5";
            }
            return orig;
        }

        private void SpawnStock(On.ShopMenuStock.orig_SpawnStock orig, ShopMenuStock self)
        {
            if (self.stock.Length == 0) { orig(self); return; }
            if (!SceneActive) { orig(self); return; }

            self.itemCount = -1;
            float num = 0f;
            self.stockInv = new GameObject[self.stock.Length];

            Logger.Error("rebuilding shop");

            Sprite itemSprite = SpritePrefab.Find("test").GetComponent<SpriteRenderer>().sprite;
            CustomItemStats item1 = CustomItemStats.CreateNormalItem(itemSprite, "", "INV_NAME_HEARTPIECE_1", "SHOP_DESC_HEARTPIECE_1", 20);
            CustomItemStats item2 = CustomItemStats.CreateNormalItem(itemSprite, "", "INV_NAME_HEARTPIECE_1", "SHOP_DESC_HEARTPIECE_1", 20);
            CustomItemStats item3 = CustomItemStats.CreateNormalItem(itemSprite, "", "INV_NAME_HEARTPIECE_1", "SHOP_DESC_HEARTPIECE_1", 20);
            CustomItemStats[] items = { item1, item2, item3 };

            //stats.priceConvo = "CHRISTMAS_ITEM_1";

            GameObject prefab = self.stock[0];
            if (!boughtTest)
            {
                self.stock = new GameObject[2];

                self.stock[0] = GameObject.Instantiate(prefab, prefab.transform.parent);
                self.stock[0].GetComponent<ShopItemStats>().charmsRequired = 0;
                self.stock[0].GetComponent<ShopItemStats>().nameConvo = "INV_NAME_CHRISTMAS_ITEM_1";
                self.stock[0].GetComponent<ShopItemStats>().descConvo = "SHOP_DESC_CHRISTMAS_ITEM_1";
                self.stock[0].GetComponent<ShopItemStats>().priceConvo = "CHRISTMAS_ITEM_1";
                self.stock[0].GetComponent<ShopItemStats>().specialType = 18;
                self.stock[0].GetComponent<ShopItemStats>().requiredPlayerDataBool = "";
                self.stock[0].GetComponent<ShopItemStats>().playerDataBoolName = "";
                self.stock[0].transform.Find("Item Sprite").GetComponent<SpriteRenderer>().sprite = itemSprite;
                self.stock[0].SetActive(false);
                self.stock[0].name = "Test_obj";


                self.stock[1] = GameObject.Instantiate(prefab, prefab.transform.parent);
                self.stock[1].GetComponent<ShopItemStats>().charmsRequired = 0;
                self.stock[1].GetComponent<ShopItemStats>().nameConvo = "INV_NAME_HEARTPIECE_1";
                self.stock[1].GetComponent<ShopItemStats>().descConvo = "INV_NAME_HEARTPIECE_1";
                self.stock[1].GetComponent<ShopItemStats>().priceConvo = "CHRISTMAS_ITEM_2";
                self.stock[1].GetComponent<ShopItemStats>().specialType = 18;
                self.stock[1].GetComponent<ShopItemStats>().requiredPlayerDataBool = "";
                self.stock[1].GetComponent<ShopItemStats>().playerDataBoolName = "";
                self.stock[1].transform.Find("Item Sprite").GetComponent<SpriteRenderer>().sprite = itemSprite;
                self.stock[1].SetActive(false);
                self.stock[1].name = "Test_obj_2";
            }
            else
            {
                self.stock = new GameObject[1];

                self.stock[0] = GameObject.Instantiate(prefab, prefab.transform.parent);
                self.stock[0].GetComponent<ShopItemStats>().charmsRequired = 0;
                self.stock[0].GetComponent<ShopItemStats>().nameConvo = "INV_NAME_CHRISTMAS_ITEM_1";
                self.stock[0].GetComponent<ShopItemStats>().descConvo = "SHOP_DESC_CHRISTMAS_ITEM_1";
                self.stock[0].GetComponent<ShopItemStats>().priceConvo = "CHRISTMAS_ITEM_1";
                self.stock[0].GetComponent<ShopItemStats>().specialType = 18;
                self.stock[0].GetComponent<ShopItemStats>().requiredPlayerDataBool = "";
                self.stock[0].GetComponent<ShopItemStats>().playerDataBoolName = "";
                self.stock[0].transform.Find("Item Sprite").GetComponent<SpriteRenderer>().sprite = itemSprite;
                self.stock[0].SetActive(false);
                self.stock[0].name = "Test_obj";
            }

            var spawnedStock = new Dictionary<GameObject, GameObject>(self.stock.Length);
            GameObject[] array = self.stock;
            foreach (GameObject gameObject in array)
            {
                Logger.Error("Setting go");
                GameObject gameObject2 = GameObject.Instantiate(gameObject);
                gameObject2.SetActive(value: false);
                spawnedStock.Add(gameObject, gameObject2);
            }

            // Edit the master component
            // Can be some odd null reference here, but trying to catch is cases some error?
            var masterStock = self.masterList.GetComponent<ShopMenuStock>();
            masterStock.stock = self.stock;
            orig(self);
        }
    }
}
