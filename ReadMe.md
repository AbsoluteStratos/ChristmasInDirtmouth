# Christmas In Dirtmouth

[![Discord](https://img.shields.io/discord/879125729936298015.svg?logo=discord&logoColor=white&logoWidth=20&labelColor=7289DA&label=Discord&color=17cf48)](https://discord.gg/F6Y5TeFQ8j) ![OS](https://img.shields.io/badge/os-windows-blue) [![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE) [![Downloads](https://img.shields.io/github/downloads/AbsoluteStratos/ChristmasInDirtmouth/total
)](https://github.com/AbsoluteStratos/ChristmasInDirtmouth/releases)

<p align="center">
  <img src="https://github.com/AbsoluteStratos/ChristmasInDirtmouth/blob/main/docs/demo_small.gif" alt="Demo gif"/>
</p>

Things are getting festive in Dirtmouth.
A mod that adds a new christmas shop own by a new NPC, Merrywisp, in Dirthmouth you can by decorations from to bring some holiday chear to this gloomy kingdom.
Once fully decorated, have a listen and relax under the glow of the tree.
Happy holidays!

This is a intermediate / advanced mod that builds upon learnings from my previous [intermediate](https://github.com/AbsoluteStratos/CasinoKnight) and [beginner](https://github.com/AbsoluteStratos/FartKnight) mods which I recommend checking out if you are a new modder.
Develop of this mod occured over the course of two weeks, most of the time working on the item / shop logic.
This mod leans more heavily on Unity development than my previous with custom assets, scene, particle systems, animations, etc.
You can checkout out the unity project in my [HKWorldEdit2 Fork](https://github.com/AbsoluteStratos/HKWorldEdit2/tree/stratos/ChristmasInDirtmouth/Assets/ChristmasInDirtmouth).

This mod has the following features:

- A simple mod menu created using [Satchel BetterMenus](https://prashantmohta.github.io/ModdingDocs/Satchel/BetterMenus/better-menus.html)
- Modification of an existing scene with many new assets developed in unity and loaded via asset bundle
- Adding a new scene (shop) with transition in an existing scene
- Adding a custom shop with new items that are tracked externally from the vanilla player data
- A new interactive NPC with custom dialog

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

## Support

For issues / bugs, I probably won't fix them but feel free to open an issue.
The modding discord has a lot of very helpful and active devs there which can also answer various questions but don't bug them about this mod.