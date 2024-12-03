using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogCore;
using UnityEngine;

namespace ChristmasInDirtmouth
{
    public class ModItems
    {
        public static int NUM_ITEMS = ChristmasInDirtmouth.GlobalData.HeroInventory.Length;

        public static CustomItemStats[] ChristmasItemStats = new CustomItemStats[NUM_ITEMS];
        
        public ModItems()
        {
            for (int i = 0; i < NUM_ITEMS; i++)
            {
                // Boot strap relic number to be an ider
                ChristmasItemStats[i] = CustomItemStats.CreateNormalItem(
                    String.Format("shopSprite_{0:D}", i),
                    String.Format("INV_NAME_CHRISTMAS_ITEM_{0:D}", i),
                    String.Format("SHOP_DESC_CHRISTMAS_ITEM_{0:D}", i),
                    String.Format("CHRISTMAS_ITEM_{0:D}", i),
                    i
               );
            }
        }

        public static Dictionary<string, string> ShopMap = new Dictionary<string, string> {
            { "INV_NAME_CHRISTMAS_ITEM_0", "Garland and Wreaths" },
            { "INV_NAME_CHRISTMAS_ITEM_1", "Town Presents" },
            { "INV_NAME_CHRISTMAS_ITEM_2", "Candles" },
            { "INV_NAME_CHRISTMAS_ITEM_3", "Holiday Tree" },
            { "INV_NAME_CHRISTMAS_ITEM_4", "String Lights" },
            { "SHOP_DESC_CHRISTMAS_ITEM_0", "A collection of festive garland thats sure to bring a cherry mood to the town." },
            { "SHOP_DESC_CHRISTMAS_ITEM_1", "Present decorations scatter across town to reflect the season of giving." },
            { "SHOP_DESC_CHRISTMAS_ITEM_2", "Peaceful candles and decorations around the towns buildings." },
            { "SHOP_DESC_CHRISTMAS_ITEM_3", "A large decorated tree in the center of Dirtmouth. Certainly a good way to bring cheer to all bugs." },
            { "SHOP_DESC_CHRISTMAS_ITEM_4", "Light up the town with an abundance of festive lights" },
        };

        public static Dictionary<string, string> PriceMap = new Dictionary<string, string> {
            { "CHRISTMAS_ITEM_0", "100" },
            { "CHRISTMAS_ITEM_1", "200" },
            { "CHRISTMAS_ITEM_2", "250" },
            { "CHRISTMAS_ITEM_3", "500" },
            { "CHRISTMAS_ITEM_4", "1000" },
        };

        public static Dictionary<string, string> NPCMap = new Dictionary<string, string> {
            { "SLY_SHOP_INTRO", "Ahhh, little wanderer! It is you! The tiny hero I've heard so much about, skipping from shadow to light. What a treat!<page>Heehee... I’ve been decking my wares for a season of joy! You’ve braved many a deep and dreary place, haven’t you? Perhaps a little sparkle and cheer would warm that quiet shell of yours, hmm?" },
            { "SLY_NOSTOCK_1", "Oh my, you've cleared me out! Not a single bauble or bell left to spare. Such enthusiasm warms my little heart, dear wanderer.<page>For now, take your treasures and spread some cheer, won’t you? The world could always use a touch of light. Farewell, my festive friend!"},
            { "SLY_MAIN", "Merrywisp" },
            { "SLY_SUB", "" },
            { "SLY_SUPER", "Festive Knight" },
        };
    }


    public struct CustomItemStats
    {
        // https://github.com/RedFrog6002/FrogCore/blob/master/FrogCore/CustomItemStats.cs#L28C19-L28C34
        public static readonly Color defaultActiveColor = new Color(1f, 1f, 1f, 1f);
        public static readonly Color defaultInactiveColor = new Color(0.551f, 0.551f, 0.551f, 1f);

        public static CustomItemStats CreateNormalItem(string sprite, string nameConvo, string descConvo, string priceConvo, int relicNumber = 0, int charmsRequired = 0, string requiredPlayerDataBool = "", string removalPlayerDataBool = "", bool dungDiscount = false, bool canBuy = true)
        {
            CustomItemStats item = new CustomItemStats();
            item.playerDataBoolName = "";
            item.nameConvo = nameConvo;
            item.descConvo = descConvo;
            item.priceConvo = priceConvo;
            item.requiredPlayerDataBool = requiredPlayerDataBool;
            item.removalPlayerDataBool = removalPlayerDataBool;
            item.specialType = 18;
            item.relicNumber = 0;
            item.charmsRequired = charmsRequired;
            item.activeColour = defaultActiveColor;
            item.inactiveColour = defaultInactiveColor;
            item.dungDiscount = dungDiscount;
            item.relic = false;
            item.relicPDInt = "";
            return item;
        }

        public string playerDataBoolName;

        public string nameConvo;

        public string descConvo;

        public string priceConvo;

        public string requiredPlayerDataBool;

        public string removalPlayerDataBool;

        public int specialType;

        public int relicNumber;

        public int charmsRequired;

        public Color activeColour;

        public Color inactiveColour;

        public bool dungDiscount;

        public bool relic;

        public string relicPDInt;

        public string spriteId;
    }
}
