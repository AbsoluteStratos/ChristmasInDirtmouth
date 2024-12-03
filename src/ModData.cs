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
        // Saved Inventory only set during bench save (need to set a hook for this)
        // The combo of these two allow us to one keep the updates after saving like other items
        public bool[] HeroInventorySaved = new bool[] { false, false, false, false, false };

        public ModData() {
            // Assign the active inventory to saved one
            HeroInventorySaved.CopyTo(HeroInventory, 0);
        }

    }
}
