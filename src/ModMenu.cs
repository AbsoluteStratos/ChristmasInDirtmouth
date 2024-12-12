using Modding;
using Satchel.BetterMenus;

namespace ChristmasInDirtmouth
{
    public static class ModMenu
    {
        public static Menu PrepareMenu(ModToggleDelegates modtoggledelegates)
        {
            //Create a new MenuRef if it's not null
            Menu MenuRef = new Menu(
                name: "Christmas in Dirtmouth", //the title of the menu screen, it will appear on the top center of the screen 
                elements: new Element[]
                {
                    new TextPanel("A Mod by Stratos",600f),

                    new TextPanel("Options",800f),

                    new MenuButton("Reset Shop", "Resets the shop and decorations.", (setting) => { ChristmasInDirtmouth.GlobalData.Reset(); }),

                    new MenuButton("Cheat", "Give hollow knight enough geo to buy all the items.", (setting) => { HeroController.instance.AddGeo(1800); }),
                }
            );

            //uses the GetMenuScreen function to return a menuscreen that MAPI can use. 
            //The "modlistmenu" that is passed into the parameter can be any menuScreen that you want to return to when "Back" button or "esc" key is pressed 
            return MenuRef;
        }
        
    }
}
