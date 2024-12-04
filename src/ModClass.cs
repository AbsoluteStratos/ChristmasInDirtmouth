﻿using System;
using System.Collections;
using GlobalEnums;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using UObject = UnityEngine.Object;
using Satchel;

namespace ChristmasInDirtmouth
{
    public class ChristmasInDirtmouth : Mod, ILocalSettings<ModData>
    {
        new public static string GetName() => "Christmas In Dirtmouth";
        public override string GetVersion() => "0.0.1";

        public static CustomDialogueManager MerryDialogueManager;
        internal static ModData GlobalData = new ModData();
        internal static ModItems GlobalItems = new ModItems(); // Dont really need to save this but oh well
        internal static ChristmasInDirtmouth Instance;

        public Dictionary<string, Dictionary<string, GameObject>> preloads;

        private DirtmouthSceneHandler dirtmouthHandler;
        private ChristmasShopSceneHandler shopHandler;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Logger.Info("Initializing");

            Instance = this;

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.SavegameSaveHook += OnSaveGame;

            // Prepare classes from preloaded objects
            // https://github.com/PrashantMohta/Smolknight/blob/6a6253ca3ea6549cc17bff47c33ade2ac28054e7/Smolknight.cs#L134
            // Arrow prompt
            Satchel.CustomArrowPrompt.Prepare(preloadedObjects["Cliffs_01"]["Cornifer Card"]);
            // Dialog manager
            MerryDialogueManager = new Satchel.CustomDialogueManager(preloadedObjects["Cliffs_01"]["Cornifer Card"]);
            ModItems.AddManagerConversations(MerryDialogueManager);

            // Create scene handlers
            dirtmouthHandler = new DirtmouthSceneHandler();
            shopHandler = new ChristmasShopSceneHandler(
                preloadedObjects["Room_mapper"]["TileMap"],
                preloadedObjects["Room_mapper"]["_SceneManager"],
                preloadedObjects["Room_shop"]["Basement Closed/Shop Region"],
                preloadedObjects["Room_shop"]["Shop Menu"],
                preloadedObjects["Cliffs_01"]["Cornifer Card"]
            );

            Logger.Info("Initialized");
        }

        // https://prashantmohta.github.io/ModdingDocs/preloads.html#how-to-preload-an-object
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Cliffs_01","Cornifer Card"),
                ("Room_mapper","TileMap"),
                ("Room_mapper","TileMap Render Data"),
                ("Room_mapper","_SceneManager"),
                ("Town", "_Scenery/point_light/HeroLight 3"),
                ("Town","_Scenery/lamp_flys/flys"),
                ("Room_shop", "Basement Closed/Shop Region"),
                ("Room_shop", "Shop Menu")
            };
        }

        public void OnHeroUpdate()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                HeroController.instance.AddGeo(1000);
            }
            // Debugging / Dev util for jumping to new scene
            if (Input.GetKeyDown(KeyCode.O))
            {
                // Quick jump to casino for testing, remove
                GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
                {
                    SceneName = ChristmasShopSceneHandler.Name,
                    EntryGateName = ChristmasShopSceneHandler.Gate,
                    HeroLeaveDirection = GatePosition.right,
                    EntryDelay = 0.2f,
                    WaitForSceneTransitionCameraFade = true,
                    PreventCameraFadeOut = false,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = false,
                    forceWaitFetch = false
                });
            }
        }

        public void OnSaveGame(int e)
        {
            // On save updated the saved inventory
            GlobalData.HeroInventory.CopyTo(GlobalData.HeroInventorySaved, 0);
        }

        void ILocalSettings<ModData>.OnLoadLocal(ModData s)
        {
            //GlobalData = s;
            // Overwrite current inventory with current saved one
            GlobalData.HeroInventorySaved.CopyTo(GlobalData.HeroInventory, 0);
        }

        ModData ILocalSettings<ModData>.OnSaveLocal()
        {
            return GlobalData;
        }

        // https://radiance.synthagen.net/apidocs/_images/Assets.html?highlight=getexecutingassembly#using-our-loaded-stuff
        public static void ResetPrefabMaterials(GameObject obj)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                if (child.GetComponent<SpriteRenderer>() != null)
                {
                    child.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                }
                ResetPrefabMaterials(child);
            }
        }
    }
}