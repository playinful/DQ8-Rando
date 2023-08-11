using DQ8Rando3DS.Info;
using DQ8Rando3DS.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQ8Rando3DS.Modules
{
    public class CasinoRandomizer : RandomizerModule
    {
        public CasinoItemTable CasinoItemTable { get; set; }
        
        public override void Start()
        {
            if (Options.Shopping.CasinoNoChange && !Options.Shopping.CasinoRandomPrices)
                return;

            CasinoItemTable = CasinoItemTable.Load("./Raw/item_casino.tbl");

            MainWindow.UpdateStatus("Randomizing casino items...");

            if (Options.Shopping.CasinoRandom)
                RandomizeCasinoItems();
            if (Options.Shopping.CasinoShuffle)
                ShuffleCasinoItems();

            if (Options.Shopping.CasinoRandomPrices)
                RandomizeCasinoPrices();

            Save();
        }

        public void RandomizeCasinoItems()
        {
            foreach (CasinoInventory casino in CasinoItemTable.Contents.Values)
            {
                int itemCount = RNG.Next(1,7) + RNG.Next(0,7);

                casino.Clear();

                for (int i = 0; i < itemCount; i++)
                {
                    casino.Items[i].Item = ItemInfo.GetRandomItem(RNG).ID;
                    casino.Items[i].Price = 1; // TODO
                }
            }
        }

        public void ShuffleCasinoItems()
        {
            List<ushort> items = new List<ushort>();
            foreach (CasinoInventory casino in CasinoItemTable.Contents.Values)
            {
                foreach (CasinoItem item in casino.Items.Where(i => !i.IsEmpty()))
                {
                    items.Add(item.Item);
                }
            }
            items = items.Shuffle(RNG).ToList();

            foreach (CasinoInventory casino in CasinoItemTable.Contents.Values)
            {
                foreach (CasinoItem item in casino.Items.Where(i => !i.IsEmpty()))
                {
                    item.Item = items[0];
                    items.RemoveAt(0);
                }
            }
        }

        public void RandomizeCasinoPrices()
        {
            foreach (CasinoInventory casino in CasinoItemTable.Contents.Values)
            {
                foreach (CasinoItem item in casino.Items.Where(i => !i.IsEmpty()))
                {
                    item.Price = RNG.Next(Options.Shopping.CasinoMinimumPrice, Options.Shopping.CasinoMaximumPrice+1);
                }
            }
        }

        public void Save()
        {
            CasinoItemTable.Save($"{Options.Path}/romfs/data/Params/item_casino.tbl");
            CasinoItemTable.CreateSpoilerLog($"{Options.Path}/spoiler/Casino.txt");
        }
    }
}
