﻿namespace ChristmasInDirtmouth
{
    // https://prashantmohta.github.io/ModdingDocs/saving-mod-data.html
    public class ModData
    {
        public bool ShopIntro = true;

        // Active Inventory
        public bool[] HeroInventory = new bool[] { false, false, false, false, false };


        public void Reset()
        {
            ShopIntro = true;
            HeroInventory = new bool[] { false, false, false, false, false };
        }
    }
}
