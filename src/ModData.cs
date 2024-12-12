using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChristmasInDirtmouth
{
    // https://prashantmohta.github.io/ModdingDocs/saving-mod-data.html
    public class ModData
    {
        public bool ShopIntro = true;

        // Active Inventory
        public bool[] HeroInventory = new bool[] { false, false, false, false, false };

    }
}
