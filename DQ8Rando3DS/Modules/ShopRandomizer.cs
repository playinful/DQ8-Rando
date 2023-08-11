using DQ8Rando3DS.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace DQ8Rando3DS.Modules
{
    public class ShopRandomizer : RandomizerModule
    {
        public ShopItemTable ShopItemTable { get; set; }
        public ItemTable ItemTable { get; set; }

        // TODO
        public override void Start()
        {
            if (Options.Shopping.ShopNoChange && !Options.Shopping.RandomPrices && !Options.Shopping.RandomMarkup)
                return;

            ShopItemTable = ShopItemTable.Load("./Raw/shopItem.tbl");
            ItemTable = ItemTable.Load("./Raw/item.tbl");

            MainWindow.UpdateStatus("Randomizing shops...");

            if (Options.Shopping.RandomPrices)
                RandomizeItemPrices();

            if (Options.Shopping.ShopRandom)
                RandomizeShops();
            if (Options.Shopping.ShopItemShuffle)
                ShuffleShopItems();
            if (Options.Shopping.ShopInventoryShuffle)
                ShuffleShopInventories();

            if (Options.Shopping.RandomMarkup)
                RandomizeMarkups();

            if (!Options.Shopping.ShopNoChange)
                UpdateChildren();

            Save();
        }

        public void RandomizeShops()
        {
            foreach (ShopInventory shop in ShopItemTable.Contents.Values.Where(sh => sh.GetInfo().DoEdit && sh.GetInfo().Parent is null))
            {
                shop.Clear();

                int itemAmount = RNG.Next(1, 6) + RNG.Next(1, 6); // to get us an average nearing the middle

                for (int i = 0; i < itemAmount; i++)
                {
                    shop.Items[i].Item = Info.ItemInfo.GetRandomItem(RNG).ID;
                }
            }
        }
        public void RandomizeItemPrices()
        {
            foreach (ItemData item in ItemTable.Contents.Values.Where(it => it.GetInfo().Type == Info.ItemInfo.ItemType.Item))
            {
                item.BuyPrice = RNG.Next(Options.Shopping.MinimumPrice, Options.Shopping.MaximumPrice+1);
                item.SellPrice = (int)Math.Floor(item.BuyPrice / 2.0);
                if (item.SellPrice == 0 && item.BuyPrice > 0) item.SellPrice = 1;
            }
        }
        public void RandomizeMarkups()
        {
            foreach (ShopInventory shop in ShopItemTable.Contents.Values.Where(sh => sh.GetInfo().DoEdit))
            {
                shop.Markup = RNG.Next(Options.Shopping.MinimumMarkup, Options.Shopping.MaximumMarkup+1);
            }
        }

        public void ShuffleShopItems()
        {
            List<ushort> items = new List<ushort>();
            List<ShopInventory> shops = ShopItemTable.Contents.Values.Where(sh => sh.GetInfo().DoEdit && sh.GetInfo().Parent is null).ToList();
            foreach (ShopInventory shop in shops)
            {
                foreach (ShopInventoryItem item in shop.Items)
                {
                    items.Add(item.Item);
                }
                shop.Clear();
            }

            // make sure each shop has at least 1 item
            foreach (ShopInventory shop in shops)
            {
                int index = RNG.Next(items.Count);
                shop.PushItem(items[index]);
                items.RemoveAt(index);
            }

            // distribute remaining items
            foreach (ushort item in items)
            {
                ShopInventory shop = RNG.Choice(shops);
                while (shop.ItemCount() == 10)
                    shop = RNG.Choice(shops);

                shop.PushItem(item);
            }
        }
        public void ShuffleShopInventories()
        {
            List<ShopInventory> shops = ShopItemTable.Contents.Values.Where(sh => sh.GetInfo().DoEdit && sh.GetInfo().Parent is null).ToList();
            for (int i = shops.Count-1; i > 0; i--)
            {
                ShopInventory source = shops[i];
                ShopInventory target = shops[RNG.Next(i+1)];

                if (source == target)
                    continue;

                for (int j = 0; j < source.Items.Length; j++)
                {
                    (source.Items[j].Item, target.Items[j].Item) = (target.Items[j].Item, source.Items[j].Item);
                }
            }
        }

        public void UpdateChildren()
        {
            foreach(ShopInventory shop in ShopItemTable.Contents.Values.Where(sh => sh.GetInfo().Parent is not null))
            {
                ShopInventory parent = ShopItemTable.Contents[(byte)shop.GetInfo().Parent];

                shop.Markup = parent.Markup;
                for (int i = 0; i < shop.Items.Length; i++)
                {
                    shop.Items[i].Item = parent.Items[i].Item;
                }
            }
        }

        public void Save()
        {
            if (!Options.Shopping.ShopNoChange || Options.Shopping.RandomMarkup)
            {
                ShopItemTable.Save($"{Options.Path}/romfs/data/Params/shopItem.tbl");
                ShopItemTable.CreateSpoilerLog($"{Options.Path}/spoiler/ShopItems.txt");
            }
            if (Options.Shopping.RandomPrices)
            {
                ItemTable.Save($"{Options.Path}/romfs/data/Params/item.tbl");
                ItemTable.CreatePriceSpoilerLog($"{Options.Path}/spoiler/ItemPrices.txt");
            }
        }
    }
}
