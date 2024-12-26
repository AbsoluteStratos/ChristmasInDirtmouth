# Christmas In Dirtmouth

[![Discord](https://img.shields.io/discord/879125729936298015.svg?logo=discord&logoColor=white&logoWidth=20&labelColor=7289DA&label=Discord&color=17cf48)](https://discord.gg/F6Y5TeFQ8j) ![OS](https://img.shields.io/badge/os-windows%20%7C%20mac-blue?label=os) [![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE) [![Downloads](https://img.shields.io/github/downloads/AbsoluteStratos/ChristmasInDirtmouth/total
)](https://github.com/AbsoluteStratos/ChristmasInDirtmouth/releases)

<p align="center">
  <img src="https://github.com/AbsoluteStratos/ChristmasInDirtmouth/blob/main/docs/demo_small.gif" alt="Demo gif"/>
</p>

Things are getting festive in Dirtmouth!
This mod adds a new Christmas shop in Dirtmouth owned by a new NPC, Merrywisp.
Our hero can buy various decorations for geo to bring some holiday cheer to this gloomy kingdom.
Once fully decorated, take a moment to listen and relax under the glow of the tree.
Happy holidays!

This is an intermediate/advanced mod that builds upon lessons from my previous [intermediate](https://github.com/AbsoluteStratos/CasinoKnight) and [beginner](https://github.com/AbsoluteStratos/FartKnight) mods, which I recommend checking out if you are new to modding.
Development of this mod occurred over the course of two weeks, with most of the time spent working on the item/shop logic.
This mod leans more heavily on Unity development than my previous projects, featuring custom assets, scenes, particle systems, animations, and more.
You can check out the Unity project in my [HKWorldEdit2 Fork](https://github.com/AbsoluteStratos/HKWorldEdit2/tree/stratos/ChristmasInDirtmouth/Assets/ChristmasInDirtmouth).

This mod has the following features:

- A simple mod menu created using [Satchel BetterMenus](https://prashantmohta.github.io/ModdingDocs/Satchel/BetterMenus/better-menus.html)
- Modification of an existing scene with many new assets developed in Unity and loaded via an asset bundle
- Addition of a new scene (shop) with a transition from an existing scene
- A custom shop menu UI with new items tracked externally from the vanilla player data
- A new interactive NPC with custom dialogue

To show logs:
https://hk-modding.github.io/api/articles/logs.html#in-game-console

## Repository Layout

```
CasinoKnight
├── bin                                 # Compiled project files
├── docs                                # Documentation files
├── src                                 # Source folder
│   ├── Resources                       # Packed asset bundles
│   ├── ChristmasShopSceneHandler.cs    # Merrywisp's shop scene handler
│   ├── DirtmouthSceneHandler.cs        # Dirtmouth scene modifier handler
│   ├── EasterEggHandler.cs             # Easter egg handler
│   ├── Logger.cs                       # Logging utils
│   ├── ModClass.cs                     # Core mod class for hooking on Modding API
│   ├── ModData.cs                      # Mod data associated with a game save
│   ├── ModItems.cs                     # Custom item information and constants
│   ├── ModMenu.cs                      # Building function for Custom Mod Menu
│   └── ChristmasInDirtmouth.csproj     # C# project file
└── ChristmasInDirtmouth.sln            # Visual Studio solution file
```

## Resources

- [Modding Docs](https://prashantmohta.github.io/ModdingDocs/)
- [Hollow Knight Scene Names](https://drive.google.com/drive/folders/1VwVbCjU8uPV4V3cDu_Tr1TgEs01hMSFr)
- [Hollow Knight Sprite Database](https://drive.google.com/drive/folders/1lx02_w9TFTYdR3aggI1gbXcLr69roaNV)
- [OG NewScene Docs](https://radiance.synthagen.net/apidocs/_images/NewScene.html)
- [Unity 2020.2.2f1](https://unity.com/releases/editor/archive)
- [HKWorldEdit2](https://github.com/nesrak1/HKWorldEdit2)
- [Unity Asset Bundler Browser](https://github.com/Unity-Technologies/AssetBundles-Browser)
- [PlayMaker FSM Viewer Avalonia](https://github.com/nesrak1/FSMViewAvalonia)

## Dependencies

- [Satchel](https://github.com/PrashantMohta/Satchel/)

> [!WARNING]  
> This mod is presently not fully compatable with the following known mods:
>  - `HKVocalize` [[Issue](https://github.com/AbsoluteStratos/ChristmasInDirtmouth/issues/2)]

## Support

For issues / bugs, I probably won't fix them but feel free to open an issue.
The modding discord has a lot of very helpful and active devs there which can also answer various questions but don't bug them about this mod.
