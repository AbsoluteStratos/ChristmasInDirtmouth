using System;
using System.Collections;
using GlobalEnums;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ChristmasInDirtmouth
{
    public class ChristmasInDirtmouth : Mod
    {
        new public static string GetName() => "Christmas In Dirtmouth";
        public override string GetVersion() => "0.1.1";

        internal static ChristmasInDirtmouth Instance;

        public Dictionary<string, Dictionary<string, GameObject>> preloads;

        private DirtmouthSceneHandler dirtmouthHandler;
        private ChristmasShopSceneHandler shopHandler;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Logger.Info("Initializing");

            Instance = this;

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ShopUtil.HookShops();

            // Prepare classes from preloaded objects
            // https://github.com/PrashantMohta/Smolknight/blob/6a6253ca3ea6549cc17bff47c33ade2ac28054e7/Smolknight.cs#L134
            // Arrow prompt
            Satchel.CustomArrowPrompt.Prepare(preloadedObjects["Cliffs_01"]["Cornifer Card"]);

            // Create scene handlers
            dirtmouthHandler = new DirtmouthSceneHandler();
            shopHandler = new ChristmasShopSceneHandler(
                preloadedObjects["Room_mapper"]["TileMap"],
                preloadedObjects["Room_mapper"]["_SceneManager"],
                preloadedObjects["Room_shop"]["Basement Closed/Shop Region"],
                preloadedObjects["Room_shop"]["Shop Menu"]
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
                ("Room_shop", "Shop Menu"),
                ("Ruins1_23", "Lift Call Lever")
            };
        }

        public void OnHeroUpdate()
        {
            /*if (Input.GetKeyDown(KeyCode.J))
            {
                HeroController.instance.AddGeo(100);
            }*/
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