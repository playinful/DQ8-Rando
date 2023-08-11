using DQ8Rando3DS.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Media.Imaging;

namespace DQ8Rando3DS.Modules
{
    public class TreasureRandomizer : RandomizerModule
    {
        private TreasureBoxTable TreasureTable { get; set; }
        private TbContentsTable TbContentsTable { get; set; }

        public override void Start()
        {
            if (Options.Treasure.NoChange && Options.Treasure.BlueNoChange &&
                !Options.Treasure.LockThief && !Options.Treasure.LockMagic)
                return;

            MainWindow.UpdateStatus("Randomizing treasures...");

            TreasureTable = TreasureBoxTable.Load("./Raw/tbTreasureBox.tbl");
            TbContentsTable = TbContentsTable.Load("./Raw/tbContents.tbl");

            if (Options.Treasure.Randomize)
                RandomizeTreasureBoxContents();
            if (Options.Treasure.Shuffle)
                ShuffleTreasureBoxContents();

            if (Options.Treasure.RandomizeBlueChestPools || (Options.Treasure.SwapBlueAndRedChests && Options.Treasure.Randomize))
                RandomizeBlueChests();
            if (Options.Treasure.ShuffleBlueChests || (Options.Treasure.SwapBlueAndRedChests && Options.Treasure.Shuffle))
                ShuffleBlueChests();

            if (Options.Treasure.LockThief || Options.Treasure.LockMagic)
                RandomizeLocks();

            UpdateChildren();
            UpdateChestModels();
            UpdateBreakables();

            Save();
        }

        private void RandomizeTreasureBoxContents()
        {
            List<TreasureBox> boxesToEdit = new List<TreasureBox>();
            List<TreasureBox> chests = new List<TreasureBox>();
            foreach (TreasureBox box in TreasureTable.Contents.Values)
            {
                if (IsEditable(box) && box.Behaviour != TreasureBox.TBoxBehaviour.Chest && box.Behaviour != TreasureBox.TBoxBehaviour.BlueChest)
                {
                    boxesToEdit.Add(box);
                }
                if (IsEditable(box) && (box.Behaviour == TreasureBox.TBoxBehaviour.Chest || box.Behaviour == TreasureBox.TBoxBehaviour.BlueChest))
                {
                    chests.Add(box);
                }
            }

            // swap blue and red chests
            chests = chests.Shuffle(RNG).ToList();
            List<TreasureBox> redChests = new List<TreasureBox>();
            List<TreasureBox> blueChests = new List<TreasureBox>();
            if (Options.Treasure.SwapBlueAndRedChests)
            {
                int blueAmount = (int)Math.Round(chests.Count * Options.Treasure.BlueAmount);
                blueChests.AddRange(chests.ToArray()[..blueAmount]);
                redChests.AddRange(chests.ToArray()[blueAmount..]);
                boxesToEdit.AddRange(chests);

                foreach (TreasureBox box in blueChests)
                    box.Lock = TreasureBox.LockType.None;
            }
            else
            {
                foreach (TreasureBox box in chests)
                {
                    if (box.Behaviour == TreasureBox.TBoxBehaviour.Chest)
                    {
                        boxesToEdit.Add(box);
                        redChests.Add(box);
                    }
                }
            }

            // determine amounts
            boxesToEdit = boxesToEdit.Shuffle(RNG).ToList();
            int[] c = Extensions.Chunkify(boxesToEdit.Count, Options.Treasure.ItemAmount, Options.Treasure.GoldAmount, Options.Treasure.TrapAmount, Options.Treasure.EmptyAmount);
            int itemAmount = c[0];
            int goldAmount = c[1];
            int trapAmount = c[2];
            int emptyAmount = c[3]; // this isn't used but ... i don't really care that much
            List<TreasureBox> itemBoxes = new List<TreasureBox>();
            List<TreasureBox> goldBoxes = new List<TreasureBox>();
            List<TreasureBox> trapBoxes = new List<TreasureBox>();
            List<TreasureBox> emptyBoxes = new List<TreasureBox>();
            if (!Options.Treasure.TrapNonChests)
            {
                redChests = redChests.Shuffle(RNG).ToList();

                trapAmount = Math.Min(redChests.Count, trapAmount);

                trapBoxes.AddRange(redChests.ToArray()[..trapAmount]);
                foreach (TreasureBox box in trapBoxes)
                {
                    boxesToEdit.Remove(box);
                }

                c = Extensions.Chunkify(boxesToEdit.Count, Options.Treasure.ItemAmount, Options.Treasure.GoldAmount, Options.Treasure.EmptyAmount);
                itemAmount = c[0];
                goldAmount = c[1];
                emptyAmount = c[2];
                trapAmount = 0;
            }

            itemBoxes.AddRange(boxesToEdit.ToArray()[..itemAmount]);
            goldBoxes.AddRange(boxesToEdit.ToArray()[itemAmount..(itemAmount + goldAmount)]);
            trapBoxes.AddRange(boxesToEdit.ToArray()[(itemAmount + goldAmount)..(itemAmount + goldAmount + trapAmount)]);
            emptyBoxes.AddRange(boxesToEdit.ToArray()[(itemAmount + goldAmount + trapAmount)..]);

            foreach (TreasureBox box in redChests)
            {
                box.Behaviour = TreasureBox.TBoxBehaviour.Chest;
            }
            foreach (TreasureBox box in blueChests)
            {
                box.Behaviour = TreasureBox.TBoxBehaviour.BlueChest;
                box.Lock = TreasureBox.LockType.None;
            }

            foreach (TreasureBox box in itemBoxes)
            {
                box.Value = 0;
                box.Pool = 0;

                box.Item = Info.ItemInfo.GetRandomItem(RNG).ID;
            }
            foreach (TreasureBox box in goldBoxes)
            {
                box.Item = 0xFFFF;
                box.Pool = 0;

                box.Value = RandomGoldAmount();
            }
            foreach (TreasureBox box in trapBoxes)
            {
                box.Behaviour = TreasureBox.TBoxBehaviour.Chest;

                box.Item = 0xFFFD;
                box.Pool = 0;

                box.Value = GetRandomTrap();
            }
            foreach (TreasureBox box in emptyBoxes)
            {
                box.Value = 0;
                box.Item = 0;
                box.Pool = 0;
            }
        }
        private void RandomizeBlueChests()
        {
            // randomize pools
            int poolCount = RNG.Next(10, 27);
            List<BlueChestItem> items = new List<BlueChestItem>();
            for (int i = 0; i < poolCount; i++)
            {
                int itemCount = RNG.Next(2, 9);
                BlueChestPool pool = TbContentsTable.Contents.Values.ElementAt(i + 1);
                for (int j = 0; j < pool.Items.Length; j++)
                {
                    BlueChestItem item = pool.Items[j];

                    item.Item = 0;
                    item.Value = 0;
                    item.Footer = 0;

                    if (j <= itemCount)
                        items.Add(item);
                }
            }
            items = items.Shuffle(RNG).ToList();
            IEnumerable<BlueChestItem>[] c = items.Chunkify(Options.Treasure.ItemAmount, Options.Treasure.GoldAmount, Options.Treasure.TrapAmount);
            List<BlueChestItem> itemItems = c[0].ToList();
            List<BlueChestItem> goldItems = c[1].ToList();
            List<BlueChestItem> trapItems = c[2].ToList();
            foreach (BlueChestItem item in itemItems)
            {
                item.Item = Info.ItemInfo.GetRandomItem(RNG).ID;
                item.Value = 1;
                item.Footer = 0x41200000;
            }
            foreach (BlueChestItem item in goldItems)
            {
                item.Item = 0xFFFF;
                item.Value = RandomGoldAmount();
                item.Footer = 0x41200000;
            }
            foreach (BlueChestItem item in trapItems)
            {
                item.Item = 0xFFFD;
                item.Value = GetRandomTrap();
                item.Footer = 0x41200000;
            }

            List<TreasureBox> blueChests = new List<TreasureBox>();
            blueChests.AddRange(TreasureTable.Contents.Values.Where(box => IsEditable(box) && box.Behaviour == TreasureBox.TBoxBehaviour.BlueChest));
            foreach (TreasureBox box in blueChests)
            {
                box.Pool = (ushort)RNG.Next(1, poolCount);
            }
        }

        private void ShuffleTreasureBoxContents()
        {
            // swap blue and red chests
            if (Options.Treasure.SwapBlueAndRedChests)
            {
                List<TreasureBox> chests = new List<TreasureBox>();
                chests.AddRange(TreasureTable.Contents.Values.Where(box => IsEditable(box) && (box.Behaviour == TreasureBox.TBoxBehaviour.BlueChest || box.Behaviour == TreasureBox.TBoxBehaviour.Chest)));
                for (int i = chests.Count - 1; i > 0; i--)
                {
                    TreasureBox box = chests[i];
                    int swapIndex = RNG.Next(0, i + 1);
                    TreasureBox swapBox = chests[swapIndex];

                    ushort tempItem = swapBox.Item;
                    short tempValue = swapBox.Value;
                    ushort tempPool = swapBox.Pool;
                    TreasureBox.TBoxBehaviour tempBehaviour = swapBox.Behaviour;
                    TreasureBox.LockType tempLock = swapBox.Lock;

                    swapBox.Item = box.Item;
                    swapBox.Value = box.Value;
                    swapBox.Pool = box.Pool;
                    swapBox.Behaviour = box.Behaviour;
                    swapBox.Lock = box.Lock;
                    box.Item = tempItem;
                    box.Value = tempValue;
                    box.Pool = tempPool;
                    box.Behaviour = tempBehaviour;
                    box.Lock = tempLock;
                }
            }

            List<TreasureBox> boxesToEdit = new List<TreasureBox>();
            boxesToEdit.AddRange(TreasureTable.Contents.Values.Where(box => IsEditable(box) && box.Behaviour != TreasureBox.TBoxBehaviour.BlueChest));
            for (int i = boxesToEdit.Count - 1; i > 0; i--)
            {
                TreasureBox box = boxesToEdit[i];
                int swapIndex = RNG.Next(0, i+1);
                TreasureBox swapBox = boxesToEdit[swapIndex];

                ushort tempItem = swapBox.Item;
                short tempValue = swapBox.Value;

                swapBox.Item = box.Item;
                swapBox.Value = box.Value;
                box.Item = tempItem;
                box.Value = tempValue;
            }
        }
        private void ShuffleBlueChests()
        {
            List<TreasureBox> blueChests = new List<TreasureBox>();
            blueChests.AddRange(TreasureTable.Contents.Values.Where(box => box.Behaviour == TreasureBox.TBoxBehaviour.BlueChest));

            for (int i = blueChests.Count - 1; i > 0; i--)
            {
                TreasureBox box = blueChests[i];
                int swapIndex = RNG.Next(0, i + 1);
                TreasureBox swapBox = blueChests[swapIndex];

                ushort tempPool = swapBox.Pool;

                swapBox.Pool = box.Pool;
                box.Pool = tempPool;
            }
        }

        private void RandomizeLocks()
        {
            // find red chests
            List<TreasureBox> redChests = new List<TreasureBox>();
            redChests.AddRange(TreasureTable.Contents.Values.Where(box => IsEditable(box) && box.Behaviour == TreasureBox.TBoxBehaviour.Chest));

            // lock chests
            if (Options.Treasure.LockMagic || Options.Treasure.LockThief)
            {
                List<TreasureBox> lockedChests = new List<TreasureBox>();
                List<TreasureBox> unlockedChests = new List<TreasureBox>();

                redChests = redChests.Shuffle(RNG).ToList();

                int lockAmount = (int)Math.Round(redChests.Count * Options.Treasure.LockAmount);

                lockedChests.AddRange(redChests.ToArray()[..lockAmount]);
                unlockedChests.AddRange(redChests.ToArray()[lockAmount..]);

                List<TreasureBox.LockType> locks = new List<TreasureBox.LockType>();
                if (Options.Treasure.LockThief) locks.Add(TreasureBox.LockType.ThiefKey);
                if (Options.Treasure.LockMagic) locks.Add(TreasureBox.LockType.MagicKey);

                foreach (TreasureBox box in lockedChests)
                {
                    box.Lock = RNG.Choice(locks);
                }
                foreach (TreasureBox box in unlockedChests)
                {
                    box.Lock = TreasureBox.LockType.None;
                }
            }
        }

        private void UpdateChildren()
        {
            foreach (TreasureBox box in TreasureTable.Contents.Values)
            {
                if (box.Parent is not null && box.Parent != string.Empty && box.Parent != "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
                {
                    TreasureBox parent = TreasureTable.GetByName(box.Parent);

                    if (parent.Behaviour == TreasureBox.TBoxBehaviour.BlueChest && box.Behaviour == TreasureBox.TBoxBehaviour.Chest)
                        box.Behaviour = parent.Behaviour;
                    if (parent.Behaviour == TreasureBox.TBoxBehaviour.Chest && box.Behaviour == TreasureBox.TBoxBehaviour.BlueChest)
                        box.Behaviour = parent.Behaviour;

                    box.Item = parent.Item;
                    box.Value = parent.Value;
                    box.Lock = parent.Lock;
                    box.Pool = parent.Pool;
                }
            }
        }
        private void UpdateChestModels()
        {
            foreach (TreasureBox box in  TreasureTable.Contents.Values)
            {
                // this would not recolor parented chests. we don't want that
                //if (!IsEditable(box))
                //    continue;

                if (box.Behaviour == TreasureBox.TBoxBehaviour.Chest)
                {
                    if (box.Model == TreasureBox.TBoxModel.BlueChest || box.Model == TreasureBox.TBoxModel.PurpleChest)
                        box.Model = TreasureBox.TBoxModel.RedChest;
                    if (box.Model == TreasureBox.TBoxModel.SnowyBlueChest || box.Model == TreasureBox.TBoxModel.SnowyPurpleChest)
                        box.Model = TreasureBox.TBoxModel.SnowyRedChest;
                }
                if (box.Behaviour == TreasureBox.TBoxBehaviour.BlueChest)
                {
                    if (Options.Treasure.RandomizeBlueChestPools || (Options.Treasure.SwapBlueAndRedChests && Options.Treasure.Randomize))
                    {
                        if (box.Model == TreasureBox.TBoxModel.DefaultChest || box.Model == TreasureBox.TBoxModel.RedChest || box.Model == TreasureBox.TBoxModel.DarkRedChest || box.Model == TreasureBox.TBoxModel.PurpleChest)
                            box.Model = TreasureBox.TBoxModel.BlueChest;
                        if (box.Model == TreasureBox.TBoxModel.SnowyRedChest || box.Model == TreasureBox.TBoxModel.SnowyPurpleChest)
                            box.Model = TreasureBox.TBoxModel.SnowyBlueChest;
                    }
                    else
                    {
                        if (box.Model == TreasureBox.TBoxModel.BlueChest || box.Model == TreasureBox.TBoxModel.DefaultChest || box.Model == TreasureBox.TBoxModel.RedChest || box.Model == TreasureBox.TBoxModel.DarkRedChest || box.Model == TreasureBox.TBoxModel.PurpleChest)
                            box.Model = box.Pool == 0x6 ? TreasureBox.TBoxModel.PurpleChest : TreasureBox.TBoxModel.BlueChest;
                        if (box.Model == TreasureBox.TBoxModel.SnowyRedChest || box.Model == TreasureBox.TBoxModel.SnowyBlueChest || box.Model == TreasureBox.TBoxModel.SnowyPurpleChest)
                            box.Model = box.Pool == 0x6 ? TreasureBox.TBoxModel.SnowyPurpleChest : TreasureBox.TBoxModel.SnowyBlueChest;
                    }
                }
            }

            // Deprecated; blue chests can be trapped anyway, this is unnecessary
            // allow trapped chests to appear blue
            /*if (!Options.Treasure.SwapBlueAndRedChests)
                return;
            List<TreasureBox> trapChests = new List<TreasureBox>();
            trapChests.AddRange(TreasureTable.Contents.Values.Where(box => IsEditable(box) && box.Item == 0xFFFD));
            foreach (TreasureBox box in trapChests)
            {
                if (RNG.NextBool())
                    continue;

                bool purple = Options.Treasure.Shuffle && RNG.Next(0, 10) == 0;

                if (box.Model == TreasureBox.TBoxModel.DefaultChest || box.Model == TreasureBox.TBoxModel.RedChest || box.Model == TreasureBox.TBoxModel.DarkRedChest || box.Model == TreasureBox.TBoxModel.BlueChest || box.Model == TreasureBox.TBoxModel.PurpleChest)
                    box.Model = purple ? TreasureBox.TBoxModel.PurpleChest : TreasureBox.TBoxModel.BlueChest;
                if (box.Model == TreasureBox.TBoxModel.SnowyRedChest || box.Model == TreasureBox.TBoxModel.SnowyBlueChest || box.Model == TreasureBox.TBoxModel.SnowyBlueChest)
                    box.Model = purple ? TreasureBox.TBoxModel.SnowyPurpleChest : TreasureBox.TBoxModel.SnowyBlueChest;
            }*/
        }
        private void UpdateBreakables()
        {
            foreach (TreasureBox box in TreasureTable.Contents.Values)
            {
                if (box.Behaviour == TreasureBox.TBoxBehaviour.EmptyBreakable && !box.IsEmpty())
                {
                    box.Behaviour = TreasureBox.TBoxBehaviour.Breakable;
                }
                if (box.Behaviour == TreasureBox.TBoxBehaviour.Breakable && box.IsEmpty())
                {
                    box.Behaviour = TreasureBox.TBoxBehaviour.EmptyBreakable;
                }
            }
        }

        private bool IsEditable(TreasureBox box)
        {
            return box.GetInfo().Type == Info.TreasureBoxInfo.EditType.Edit && !box.HasParent() &&
            (
                box.Behaviour == TreasureBox.TBoxBehaviour.Chest          ||
                box.Behaviour == TreasureBox.TBoxBehaviour.BlueChest      ||
                box.Behaviour == TreasureBox.TBoxBehaviour.Breakable      ||
                box.Behaviour == TreasureBox.TBoxBehaviour.EmptyBreakable ||
                box.Behaviour == TreasureBox.TBoxBehaviour.HangingBag     ||
                box.Behaviour == TreasureBox.TBoxBehaviour.Wardrobe       ||
                box.Behaviour == TreasureBox.TBoxBehaviour.SparklySpot
            );
        }

        private short RandomGoldAmount()
        {
            return (short)RNG.Next(Options.Treasure.MinGoldValue, Options.Treasure.MaxGoldValue+1);
        }
        private short GetRandomTrap()
        {
            short[] validTraps = new short[] { 
                0x3D,  // cannibox
                0x72,  // mimic
                0xDE,  // pandora box
                0x012F // coffer of death
            };
            return RNG.Choice(validTraps);
        }

        public void Save()
        {
            TreasureTable.Save($"{Options.Path}/romfs/data/Params/tbTreasureBox.tbl");
            TbContentsTable.Save($"{Options.Path}/romfs/data/Params/tbContents.tbl");
            TreasureTable.CreateSpoilerLog($"{Options.Path}/spoiler/Treasure.txt");
            CreateTbContentsSpoilerLog($"{Options.Path}/spoiler/BlueChests.txt");
        }
        public void CreateTbContentsSpoilerLog(string path)
        {
            List<string> chestLines = new List<string>();
            List<string> poolLines = new List<string>();

            Dictionary<ushort, char> poolAlphabeticKey = new Dictionary<ushort, char>();

            List<TreasureBox> blueChests = new List<TreasureBox>();
            blueChests.AddRange(TreasureTable.Contents.Values.Where(box => IsEditable(box) && box.Behaviour == TreasureBox.TBoxBehaviour.BlueChest));
            foreach (TreasureBox box in blueChests)
            {
                string spoiler = "";

                string name = Info.TreasureBoxInfo.Get(box.ID).Name;

                if (box.Model == TreasureBox.TBoxModel.DefaultChest || box.Model == TreasureBox.TBoxModel.RedChest || box.Model == TreasureBox.TBoxModel.SnowyRedChest || box.Model == TreasureBox.TBoxModel.DarkRedChest)
                    name = name.Replace("%c", "Red Chest");
                if (box.Model == TreasureBox.TBoxModel.BlueChest || box.Model == TreasureBox.TBoxModel.SnowyBlueChest)
                    name = name.Replace("%c", "Blue Chest");
                if (box.Model == TreasureBox.TBoxModel.PurpleChest || box.Model == TreasureBox.TBoxModel.SnowyPurpleChest)
                    name = name.Replace("%c", "Purple Chest");

                spoiler += $"{name}: Pool ";

                if (!poolAlphabeticKey.ContainsKey(box.Pool))
                {
                    poolAlphabeticKey[box.Pool] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[poolAlphabeticKey.Count()];
                }

                spoiler += poolAlphabeticKey[box.Pool];

                chestLines.Add(spoiler);
            }

            foreach (BlueChestPool pool in TbContentsTable.Contents.Values)
            {
                if (poolAlphabeticKey.ContainsKey(pool.ID))
                {
                    string spoiler = $"Pool {poolAlphabeticKey[pool.ID]}: ";

                    List<string> items = new List<string>();

                    List<BlueChestItem> sortedItems = pool.Items.OrderBy(item =>
                    {
                        if (item.Item == 0xFFFF)
                            return item.Value - 1000000;
                        if (item.Item == 0xFFFD)
                            return item.Value + 1000;
                        else
                            return item.Item;
                    }).ToList();
                    foreach (BlueChestItem item in sortedItems)
                    {
                        if (item.Footer == 0)
                            continue;

                        string itemstr = "";

                        if (item.Item == 0xFFFF)
                        {
                            itemstr = $"{item.Value} gold coin{(item.Value != 1 ? "s" : "")}";
                        }
                        else if (item.Item == 0xFFFD)
                        {
                            itemstr = $"Trap ({Info.MonsterInfo.Get((ushort)item.Value).Name})";
                        }
                        else
                        {
                            itemstr = Info.ItemInfo.Get(item.Item).Name;
                        }

                        items.Add(itemstr);
                    }

                    spoiler += string.Join(", ", items);

                    poolLines.Add(spoiler);
                }
            }
            poolLines.Sort();

            List<string> spoilerLines = new List<string>();
            spoilerLines.AddRange(chestLines);
            spoilerLines.Add("------------------------------------------------");
            spoilerLines.AddRange(poolLines);

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, spoilerLines));
            MainWindow.UpdateStatus($"Wrote blue chest spoiler log to `{path}`.");
        }
    }
}
