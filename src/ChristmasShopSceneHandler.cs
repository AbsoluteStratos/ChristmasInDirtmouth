﻿using System;
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
                Logger.Info(fsm.FsmVariables.GetFsmString("No Stock Event").Value);
                Logger.Info(fsm.FsmVariables.GetFsmBool("Stock Left").Value.ToString());

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
            if ((sheetTitle == "Sly" || sheetTitle == "Titles") && ModItems.NPCMap.ContainsKey(key))
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

            Logger.Error("rebuilding shop");

            Sprite itemSprite = SpritePrefab.Find("test").GetComponent<SpriteRenderer>().sprite;

            GameObject prefab = self.stock[0];
            // Initialize shop items
            int itemCount = ChristmasInDirtmouth.GlobalData.HeroInventory.Where(c => !c).Count();
            Logger.Warning(itemCount.ToString());
            self.stock = new GameObject[itemCount];
            int j = 0;
            for (int i = 0; i < ModItems.NUM_ITEMS; i++)
            {
                // If item is not in the heros inventory
                if (!ChristmasInDirtmouth.GlobalData.HeroInventory[i] && j < self.stock.Length)
                {
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
            var masterStock = self.masterList.GetComponent<ShopMenuStock>();
            masterStock.stock = self.stock;
            orig(self);
        }
    }
}
