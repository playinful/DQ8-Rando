using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;

using static System.Random;

namespace DQ8Rando
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public System.Random rand = new System.Random();

        public List<Option> optionOutputList;

        public string jsonString = "";
        public List<Encounter> encountList;
        public List<SetEncounter> setEncountList;
        public List<Item> itemList;
        public EncountFile encountFile;
        public TreasureBoxFile tbTreasureBox;
        public TbContentsFile tbContents;

        public EncountFile customEncountFile;
        public TreasureBoxFile customTreasureBoxFile;
        public TbContentsFile customTbContentsFile;

        private void button_finish_Click(object sender, RoutedEventArgs e)
        {
            initializeOptionOutputList();

            customEncountFile = null;
            customTreasureBoxFile = null;
            customTbContentsFile = null;

            string message = "Patch created successfully.";

            if (check_monsters_enemyOverworld.IsChecked == true || check_monsters_enemySpecial.IsChecked == true)
                randomizeOverworldEnemies();
            if (check_monsters_enemyBoss.IsChecked == true)
                randomizeBossEnemies();
            if (check_treasure_tbRandom.IsChecked == true  || check_treasure_blueRandom.IsChecked == true        || check_treasure_blueShuffle.IsChecked == true ||
                check_treasure_forceFill.IsChecked == true || check_treasure_locks_keyLocation.IsChecked == true || check_treasure_locks_randomizeLocks.IsChecked == true ||
                check_treasure_swapBlueRed.IsChecked == true)
                randomizeTreasure();

            if (customEncountFile != null)
                outputEncounterTableToFile(customEncountFile,"encount.tbl");
            if (customTreasureBoxFile != null)
                outputTreasureBoxFileToFile(customTreasureBoxFile, "tbTreasureBox.tbl");
            if (customTbContentsFile != null)
                outputTbContentsFileToFile(customTbContentsFile, "tbContents.tbl");

            generateSpoilerLog();
            generateOptionLog();

            MessageBoxResult result = MessageBox.Show(message);

        }
        private void monsters_mixInInfamous_EnableCheck(object sender, RoutedEventArgs e)
        {
            check_monsters_mixInInfamous.IsEnabled = (check_monsters_enemyOverworld.IsChecked == true);
        }
        private void treasure_goldPercent_EnableCheck(object sender, RoutedEventArgs e)
        {
            tbox_treasure_goldPercentMin.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_goldPercentMax.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_goldAmountMin.IsEnabled  = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_goldAmountMax.IsEnabled  = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_emptyPercentMin.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true);
            tbox_treasure_emptyPercentMax.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true);
            tbox_treasure_trapPercentMin.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_trapPercentMax.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_bluePercentMin.IsEnabled = (check_treasure_swapBlueRed.IsChecked == true);
            tbox_treasure_bluePercentMax.IsEnabled = (check_treasure_swapBlueRed.IsChecked == true);
        }
        private void treasure_locks_EnableCheck(object sender, RoutedEventArgs e)
        {
            check_treasure_locks_thiefKey.IsEnabled = (check_treasure_locks_randomizeLocks.IsChecked == true);
            check_treasure_locks_magicKey.IsEnabled = (check_treasure_locks_randomizeLocks.IsChecked == true);
            tbox_treasure_locks_min.IsEnabled       = (check_treasure_locks_randomizeLocks.IsChecked == true && (check_treasure_locks_thiefKey.IsChecked == true || check_treasure_locks_magicKey.IsChecked == true));
            tbox_treasure_locks_max.IsEnabled       = (check_treasure_locks_randomizeLocks.IsChecked == true && (check_treasure_locks_thiefKey.IsChecked == true || check_treasure_locks_magicKey.IsChecked == true));
        }
        private void tbox_treasure_goldPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_treasure_goldPercentMin, tbox_treasure_goldPercentMax);
        }
        private void tbox_treasure_goldAmount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_treasure_goldAmountMin, tbox_treasure_goldAmountMax);
        }
        private void tbox_treasure_emptyPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_treasure_emptyPercentMin, tbox_treasure_emptyPercentMax);
        }
        private void tbox_treasure_trapPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_treasure_trapPercentMin, tbox_treasure_trapPercentMax);
        }
        private void tbox_treasure_bluePercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_treasure_bluePercentMin, tbox_treasure_bluePercentMax);
        }
        private void tbox_treasure_lockPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_treasure_locks_min, tbox_treasure_locks_max);
        }
        private void minMaxCheck(object sender, Xceed.Wpf.Toolkit.DoubleUpDown min, Xceed.Wpf.Toolkit.DoubleUpDown max)
        {
            if (sender != null && max != null && min != null)
            {
                if (sender == max && max.Value < min.Value)
                    min.Value = max.Value;
                else if (max.Value < min.Value)
                    max.Value = min.Value;
                else if (sender == max && max.Value == null)
                    max.Value = min.Value;
                else if (sender == min && min.Value == null)
                    min.Value = min.Minimum;
            }
        }
        private void minMaxCheck(object sender, Xceed.Wpf.Toolkit.ShortUpDown min, Xceed.Wpf.Toolkit.ShortUpDown max)
        {
            if (sender != null && max != null && min != null)
            {
                if (sender == max && max.Value < min.Value)
                    min.Value = max.Value;
                else if (max.Value < min.Value)
                    max.Value = min.Value;
                else if (sender == max && max.Value == null)
                    max.Value = min.Value;
                else if (sender == min && min.Value == null)
                    min.Value = min.Minimum;
            }
        }
        public void initializeOptionOutputList()
        {
            if (optionOutputList == null)
            {
                
            }
        }
        private void loadEncounters()
        {
            if (encountList == null)
            {
                jsonString = File.ReadAllText("Encounter.json");
                encountList = JsonSerializer.Deserialize<List<Encounter>>(jsonString);
                loadEncounterFile();
            }
        }
        private void loadSetEncounters()
        {
            if (setEncountList == null)
            {
                jsonString = File.ReadAllText("SetEncounter.json");
                setEncountList = JsonSerializer.Deserialize<List<SetEncounter>>(jsonString);
                loadEncounterFile();
            }
        }
        private void loadEncounterFile()
        {
            if (encountFile == null)
            {
                jsonString = File.ReadAllText("EncountTbl.json");
                encountFile = JsonSerializer.Deserialize<EncountFile>(jsonString);
            }
        }
        private void loadItems()
        {
            if (itemList == null)
            {
                jsonString = File.ReadAllText("Item.json");
                itemList = JsonSerializer.Deserialize<List<Item>>(jsonString);
            }
        }
        private void loadTreasureFile()
        {
            if (tbTreasureBox == null)
            {
                jsonString = File.ReadAllText("TreasureBox.json");
                tbTreasureBox = JsonSerializer.Deserialize<TreasureBoxFile>(jsonString);
                loadItems();
            }
        }
        private void loadBlueChestFile()
        {
            if (tbContents == null)
            {
                jsonString = File.ReadAllText("BlueChest.json");
                tbContents = JsonSerializer.Deserialize<TbContentsFile>(jsonString);
            }
        }
        private void prepareCustomEncountFile() 
        { 
            if (customEncountFile == null)
            {
                customEncountFile = new EncountFile();
                customEncountFile.Header = encountFile.Header;
                customEncountFile.Contents = new List<EncountTable>();
                foreach (EncountTable table in encountFile.Contents)
                {
                    EncountTable newTable = new EncountTable();
                    newTable.ID = table.ID;
                    newTable.Region = table.Region;
                    newTable.Area = table.Area;
                    newTable.Edit = table.Edit;
                    newTable.Header = table.Header;
                    newTable.Contents = new EncountTableEntry[10];
                    newTable.SetEncounters = new EncountTableSetEntry[2];
                    int count = 0;
                    foreach (EncountTableEntry entry in table.Contents)
                    {
                        EncountTableEntry newEntry = new EncountTableEntry();
                        newEntry.Arg1 = entry.Arg1;
                        newEntry.Arg2 = entry.Arg2;
                        newEntry.ID = entry.ID;
                        newEntry.Footer = entry.Footer;
                        newTable.Contents[count] = newEntry;
                        count++;
                    }
                    count = 0;
                    foreach (EncountTableSetEntry entry in table.SetEncounters)
                    {
                        EncountTableSetEntry newEntry = new EncountTableSetEntry();
                        newEntry.Weight = entry.Weight;
                        newEntry.ID = entry.ID;
                        newTable.SetEncounters[count] = newEntry;
                        count++;
                    }
                    customEncountFile.Contents.Add(newTable);
                }
            }
        }
        private void prepareCustomTreasureBoxFile()
        {
            if (customTreasureBoxFile == null)
            {
                loadTreasureFile();
                customTreasureBoxFile = new TreasureBoxFile();
                customTreasureBoxFile.Footer = tbTreasureBox.Footer;
                customTreasureBoxFile.Contents = new TreasureBox[tbTreasureBox.Contents.Length];
                for (int i = 0; i < tbTreasureBox.Contents.Length;i++)
                {
                    var src = tbTreasureBox.Contents[i];
                    TreasureBox box = new TreasureBox();
                    customTreasureBoxFile.Contents[i] = box;
                    box.Edit =      src.Edit;
                    box.Code =      src.Code;
                    box.Container = src.Container;
                    box.Content =   src.Content;
                    box.EngMap =    src.EngMap;
                    box.EngRegion = src.EngRegion;
                    box.ID =        src.ID;
                    box.Lock =      src.Lock;
                    box.Map =       src.Map;
                    box.Model =     src.Model;
                    box.Parent =    src.Parent;
                    box.Pool =      src.Pool;
                    box.Region =    src.Region;
                    box.Rot =       src.Rot;
                    box.Type =      src.Type;
                    box.Unk1 =      src.Unk1;
                    box.Value =     src.Value;
                    box.XPos =      src.XPos;
                    box.YPos =      src.YPos;
                    box.ZPos =      src.ZPos;
                }
            }
        }
        private void prepareCustomTbContentsFile()
        {
            if (customTbContentsFile == null)
            {
                customTbContentsFile = new TbContentsFile();
                customTbContentsFile.Header = tbContents.Header;
                customTbContentsFile.Contents = new BlueChestPool[tbContents.Contents.Length];
                for (int i = 0;i < tbContents.Contents.Length;i++)
                {
                    var src = tbContents.Contents[i];
                    BlueChestPool pool = new BlueChestPool();
                    customTbContentsFile.Contents[i] = pool;
                    pool.ID = src.ID;
                    pool.Unk1 = src.Unk1;
                    pool.Contents = new BlueChestItem[src.Contents.Length];
                    for (int j = 0;j < pool.Contents.Length;j++)
                    {
                        var srcItem = src.Contents[j];
                        BlueChestItem item = new BlueChestItem();
                        pool.Contents[j] = item;
                        item.ID = srcItem.ID;
                        item.Value = srcItem.Value;
                        item.Footer = srcItem.Footer;
                    }
                }
            }
        }
        private string reverseHex(string hex)
        {
            if (hex.Length % 2 != 0)
                hex = "0" + hex;
            string reversedHex = "";
            for (int i = hex.Length-2;i >= 0; i -= 2)
            {
                reversedHex += hex.Substring(i,2);
            }
            return reversedHex;
        }
        private double getRandomDoubleBetweenTwoValues(System.Random r, double min, double max)
        {
            double randVal = r.NextDouble();
            var diff = max - min;
            return min + (randVal * diff);
        }
        private double getRandomDoubleBetweenTwoValues(System.Random r, int min, int max)
        {
            double randVal = r.NextDouble();
            var diff = max - min;
            return min + (randVal * diff);
        }
        private void randomizeOverworldEnemies()
        {
            loadEncounters();
            loadSetEncounters();
            prepareCustomEncountFile();
            List<int> valid = new List<int>();
            valid.Add(1);
            if (check_monsters_mixInInfamous.IsChecked == true) {
                valid.Add(4);
            }
            List<Encounter> validEncounters = encountList.FindAll(e => valid.Contains(e.Spawn));
            List<SetEncounter> validSetEncounters = setEncountList.FindAll(e => valid.Contains(e.Spawn));
            foreach (EncountTable table in customEncountFile.Contents)
            {
                if (table.Edit == 1 && check_monsters_enemyOverworld.IsChecked == true)
                {
                    table.Contents = getTenRandomEncounters(validEncounters);
                    table.SetEncounters = getTwoRandomSetEncounters(validSetEncounters);
                } else if (table.Edit == 3 && check_monsters_enemySpecial.IsChecked == true)
                {
                    table.SetEncounters = getOneRandomSetEncounter(validSetEncounters);
                }
            }
        }
        private EncountTableEntry[] getTenRandomEncounters(List<Encounter> l)
        {
            EncountTableEntry[] entryArr = new EncountTableEntry[10];
            int enemyCount = rand.Next(1,10);
            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                EncountTableEntry entry = new EncountTableEntry();
                entryArr[i] = entry;
                if (count < enemyCount)
                {
                    var randomEnemy = l[rand.Next(l.Count)];
                    entry.Arg1 = "03";
                    entry.Arg2 = "03";
                    entry.ID = randomEnemy.ID;
                    entry.Footer = "0000803F";
                }
                else
                {
                    entry.Arg1 = "00";
                    entry.Arg2 = "00";
                    entry.ID = "0000";
                    entry.Footer = "00000000";
                }
                count++;
            }
            return entryArr;
        }
        private EncountTableSetEntry[] getTwoRandomSetEncounters(List<SetEncounter> l)
        {
            EncountTableSetEntry[] entryArr = new EncountTableSetEntry[2];
            int enemyCount = rand.Next(0, 2);
            int count = 0;
            for (int i = 0; i < 2; i++)
            {
                EncountTableSetEntry entry = new EncountTableSetEntry();
                entryArr[i] = entry;
                if (count < enemyCount)
                {
                    var randomEnemy = l[rand.Next(l.Count)];
                    entry.Weight = "03";
                    entry.ID = randomEnemy.ID;
                }
                else
                {
                    entry.Weight = "00";
                    entry.ID = "86";
                }
                count++;
            }
            return entryArr;
        }
        private EncountTableSetEntry[] getOneRandomSetEncounter(List<SetEncounter> l)
        {
            EncountTableSetEntry[] entryArr = new EncountTableSetEntry[2];
            int enemyCount = 1;
            int count = 0;
            for (int i = 0; i < 2; i++)
            {
                EncountTableSetEntry entry = new EncountTableSetEntry();
                entryArr[i] = entry;
                if (count < enemyCount)
                {
                    var randomEnemy = l[rand.Next(l.Count)];
                    entry.Weight = "03";
                    entry.ID = randomEnemy.ID;
                }
                else
                {
                    entry.Weight = "00";
                    entry.ID = "86";
                }
                count++;
            }
            return entryArr;
        }
        private Encounter getEncounterById(string id)
        {
            loadEncounters();
            Encounter target = null;
            foreach (Encounter encount in encountList)
            {
                if (encount.ID.ToUpper() == id.ToUpper()) 
                { 
                    target = encount;
                    break;
                }
            }
            return target;
        }
        private SetEncounter getSetEncounterById(string id)
        {
            loadSetEncounters();
            SetEncounter target = null;
            foreach (SetEncounter encount in setEncountList)
            {
                if (encount.ID.ToUpper() == id.ToUpper())
                {
                    target = encount;
                    break;
                }
            }
            return target;
        }
        private void randomizeBossEnemies()
        {
            loadSetEncounters();
            prepareCustomEncountFile();
            List<SetEncounter> validSetEncounters = setEncountList.FindAll(e => e.Spawn > 0);
            foreach (EncountTable table in customEncountFile.Contents)
            {
                if (table.Edit == 2)
                {
                    table.SetEncounters = getOneRandomSetEncounter(setEncountList);
                }
            }
        }
        private TreasureBox findTreasureBoxByCode(TreasureBox[] contents, string code) {
            foreach (TreasureBox b in contents)
            {
                if (b.Code.ToUpper() == code.ToUpper())
                    return b;
            }
            return null;
        }
        private TreasureBox findTreasureBoxByID(TreasureBox[] contents, string id)
        {
            foreach (TreasureBox b in contents)
            {
                if (b.ID.ToUpper() == id.ToUpper())
                    return b;
            }
            return null;
        }
        private Item findItemByID(string id)
        {
            loadItems();
            foreach (Item item in itemList)
            {
                if (item.ID.ToUpper() == id.ToUpper())
                    return item;
            }
            return null;
        }
        private BlueChestPool findBlueChestPoolByID(string id, TbContentsFile tbc)
        {
            if (tbc == null)
            {
                loadBlueChestFile();
                tbc = tbContents;
            }
            foreach(BlueChestPool pool in tbc.Contents)
            {
                if (pool.ID == id)
                    return pool;
            }
            return null;
        }
        private void generateSpoilerLog()
        {
            generateEncounterSpoilerLog("Encounters.txt");
            generateTreasureSpoilerLog("Treasure.txt");
            generateBlueChestSpoilerLog("BlueChests.txt");
        }
        private void generateEncounterSpoilerLog(string path)
        {
            if (customEncountFile != null)
            {
                List<string> spoiler = new List<string>();
                foreach (EncountTable table in customEncountFile.Contents)
                {
                    if (table.Edit > 0)
                    {
                        spoiler.Add(table.Region);
                        if (table.Area != "" && table.Area != ".")
                        {
                            spoiler.Add(" (");
                            spoiler.Add(table.Area);
                            spoiler.Add(")");
                        }
                        spoiler.Add(": ");
                        List<string> monsters = new List<string>();
                        foreach (EncountTableEntry entry in table.Contents)
                        {
                            var monster = getEncounterById(entry.ID);
                            if (monster != null && monster.Spawn > 0)
                                monsters.Add(monster.Name);
                        }
                        foreach (EncountTableSetEntry entry in table.SetEncounters)
                        {
                            var monster = getSetEncounterById(entry.ID);
                            if (monster != null && monster.Spawn > 0)
                                monsters.AddRange(monster.Contents);
                        }
                        spoiler.Add(string.Join(", ", monsters));
                        spoiler.Add("\n");
                    }
                }
                File.WriteAllText(path, string.Join("", spoiler));
            }
        }
        private void generateTreasureSpoilerLog(string path)
        {
            if (customTreasureBoxFile != null)
            {
                List<string> spoiler = new List<string>();
                foreach (TreasureBox box in customTreasureBoxFile.Contents)
                {
                    if (box.Parent == "00000000000000000000000000000000" && box.Type != "0008" && box.Type != "0002" && (box.Content == "0000" && box.Value == "0000" && box.Pool == "0000") == false)
                    {
                        string spoilerStr = box.EngRegion;
                        if (box.EngMap != "" && box.EngMap != ".")
                            spoilerStr += " (" + box.EngMap + ")";
                        spoilerStr += " - " + box.Container;
                        if (box.Lock == "0115")
                            spoilerStr += " [Thief's Key]";
                        if (box.Lock == "0116")
                            spoilerStr += " [Magic Key]";
                        spoilerStr += ": ";
                        if (box.Content == "FFFF")
                        {
                            if (box.Value == "0001")
                                spoilerStr += "1 gold coin";
                            else
                                spoilerStr += int.Parse(box.Value, System.Globalization.NumberStyles.HexNumber).ToString() + " gold coins";
                        }
                        else if (box.Content == "FFFD")
                        {
                            switch (box.Value.ToUpper())
                            {
                                case "003D":
                                    spoilerStr += "cannibox";
                                    break;
                                case "0072":
                                    spoilerStr += "mimic";
                                    break;
                                case "00DE":
                                    spoilerStr += "Pandora's box";
                                    break;
                                case "012F":
                                    spoilerStr += "coffer of death";
                                    break;
                            }
                        }
                        else
                            spoilerStr += findItemByID(box.Content).Name;
                        spoiler.Add(spoilerStr);

                        File.WriteAllText(path, string.Join("\n", spoiler));
                    }
                }
            }
        }
        private void generateBlueChestSpoilerLog(string path)
        {
            if (customTreasureBoxFile != null || customTbContentsFile != null)
            {
                TreasureBoxFile file;
                if (customTreasureBoxFile != null)
                    file = customTreasureBoxFile;
                else
                {
                    loadBlueChestFile();
                    file = tbTreasureBox;
                }
                TbContentsFile tbFile;
                if (customTbContentsFile != null)
                    tbFile = customTbContentsFile;
                else
                {
                    loadBlueChestFile();
                    tbFile = tbContents;
                }
                List<string> spoilerPools = new List<string>();
                List<string> spoilerChests = new List<string>();
                List<string> usedPools = new List<string>();
                foreach (TreasureBox box in file.Contents)
                {
                    if (box.Parent == "00000000000000000000000000000000" && box.Type == "0002")
                    {
                        string spoilerStr = box.EngRegion;
                        if (box.EngMap != "" && box.EngMap != ".")
                            spoilerStr += " (" + box.EngMap + ")";
                        spoilerStr += " - " + box.Container + ": Pool ";
                        int pool;
                        if (usedPools.Contains(box.Pool))
                        {
                            pool = usedPools.IndexOf(box.Pool) + 1;
                        } else
                        {
                            usedPools.Add(box.Pool);
                            pool = usedPools.Count;
                        }
                        spoilerStr += pool.ToString();
                        spoilerChests.Add(spoilerStr);
                    }
                }
                for (int i = 0; i < usedPools.Count; i++)
                {
                    var pool = findBlueChestPoolByID(usedPools[i], tbFile);
                    string poolStr = "Pool " + (i+1).ToString() + ": ";
                    List<string> poolContent = new List<string>();
                    foreach (BlueChestItem item in pool.Contents)
                    {
                        if (item.Footer != "00000000")
                        {
                            if (item.ID != "FFFF")
                                poolContent.Add(findItemByID(item.ID).Name);
                            else
                            {
                                if (item.Value == "0001")
                                    poolContent.Add("1 gold coin");
                                else
                                    poolContent.Add(int.Parse(item.Value, System.Globalization.NumberStyles.HexNumber).ToString() + " gold coins");
                            }
                        }    
                    }
                    poolStr += string.Join(", ", poolContent);
                    spoilerPools.Add(poolStr);
                }
                string finalString =
                    "POOLS --------------------------------------------------------------\n"
                  + string.Join("\n",spoilerPools) + "\n"
                  + "CHESTS -------------------------------------------------------------\n"
                  + string.Join("\n", spoilerChests);

                File.WriteAllText(path, finalString);
            }
        }
        private void generateOptionLog()
        {

        }
        private void writeHexStringToFile (string byteString, string path)
        {
            // now we convert byteString to an array of bytes
            byte[] bytes = new byte[byteString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0; // just in case
                string hex = byteString.Substring(i * 2, 2);
                int intValue = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                bytes[i] = (byte)intValue;
            }
            File.WriteAllBytes(path, bytes);
        }
        private void outputEncounterTableToFile(EncountFile data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (EncountTable table in data.Contents)
                {
                    string id = table.ID;
                    id = reverseHex(id);
                    byteString += id + table.Header;
                    foreach (EncountTableEntry entry in table.Contents)
                    {
                        id = entry.ID;
                        id = reverseHex(id);
                        byteString += entry.Arg1 + entry.Arg2 + id + entry.Footer;
                    }
                    foreach (EncountTableSetEntry entry in table.SetEncounters)
                    {
                        byteString += entry.Weight + entry.ID;
                    }
                }
                writeHexStringToFile(byteString, path);
            }
        }
        private void outputTreasureBoxFileToFile(TreasureBoxFile data, string path)
        {
            if (data != null)
            {
                string byteString = "";
                foreach (TreasureBox box in data.Contents)
                {
                    byteString += reverseHex(box.ID) 
                      + box.Unk1 + box.Region + box.Map + box.Code
                      + reverseHex(box.Model)
                      + reverseHex(box.Type)
                      + reverseHex(box.Lock)
                      + reverseHex(box.Content)
                      + reverseHex(box.Value)
                      + reverseHex(box.Pool)
                      + reverseHex(box.XPos) + reverseHex(box.YPos) + reverseHex(box.ZPos) + reverseHex(box.Rot) + box.Parent;
                }
                byteString += data.Footer;

                writeHexStringToFile(byteString, path);
            }
        }
        private void outputTbContentsFileToFile(TbContentsFile data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (BlueChestPool pool in data.Contents)
                {
                    byteString += reverseHex(pool.ID)
                        + pool.Unk1;
                    foreach (BlueChestItem item in pool.Contents)
                    {
                        byteString += reverseHex(item.ID)
                          + reverseHex(item.Value)
                          + item.Footer;
                    }
                }

                writeHexStringToFile(byteString, path);
            }
        }
        private void randomizeTreasure()
        {
            loadTreasureFile();
            prepareCustomTreasureBoxFile();
            if (check_treasure_blueRandom.IsChecked == true)
            {
                loadBlueChestFile();
                prepareCustomTbContentsFile();
            }

            double goldPercent  = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_goldPercentMin.Value,  (double)tbox_treasure_goldPercentMax.Value) / 100;
            double emptyPercent = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_emptyPercentMin.Value, (double)tbox_treasure_emptyPercentMax.Value) / 100;
            double lockPercent  = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_locks_min.Value,       (double)tbox_treasure_locks_max.Value) / 100;
            double bluePercent  = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_bluePercentMin.Value,  (double)tbox_treasure_bluePercentMax.Value) / 100;
            double trapPercent  = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_trapPercentMin.Value,  (double)tbox_treasure_trapPercentMax.Value) / 100;
            int minGold = (int)tbox_treasure_goldAmountMin.Value;
            int maxGold = (int)tbox_treasure_goldAmountMax.Value;
            double[] test = { goldPercent,emptyPercent,lockPercent,bluePercent,trapPercent };
            test = test;
            // chests to reassign
            List<TreasureBox> chestsToReassign = new List<TreasureBox>();

            // start by doing swap chests
            if (check_treasure_swapBlueRed.IsChecked == true)
            {
                // determine which chests are valid
                string[] validChestTypes = { "Red Chest", "Blue Chest", "Purple Chest" };
                var validChests = Array.FindAll(customTreasureBoxFile.Contents, e => validChestTypes.Contains(e.Container) && e.Parent == "00000000000000000000000000000000" && e.Edit == 1);
                // sort chests randomly
                var sortedChests = validChests.OrderBy(e => rand.NextDouble());
                // assign chests in that random order
                int i = 0;
                foreach (TreasureBox box in sortedChests)
                {
                    if (i <= sortedChests.Count() * bluePercent && bluePercent > 0)
                    {
                        if (box.Container == "Red Chest")
                        {
                            box.Container = "Blue Chest";
                            if (box.Model == "0047")
                                box.Model = "0049";
                            else
                                box.Model = "0046";
                            box.Lock = "0000";
                            box.Content = "0000";
                            box.Value = "0000";
                            box.Type = "0002";
                            chestsToReassign.Add(box);
                        }
                    } else
                    {
                        if (box.Container != "Red Chest")
                        {
                            box.Container = "Red Chest";
                            if (box.Model == "0048" || box.Model == "0049")
                                box.Model = "0047";
                            else
                                box.Model = "0001";
                            box.Pool = "0000";
                            box.Type = "0001";
                            chestsToReassign.Add(box);
                        }
                    }
                    i++;
                }
            }
            // then assign normal chest values
            var validItems = itemList.FindAll(e => e.Spawn == 1);
            // if "Randomize All" is checked, we just add all chests to the pool
            if (check_treasure_tbRandom.IsChecked == true)
            {
                foreach (TreasureBox box in customTreasureBoxFile.Contents)
                {
                    if (box.Edit == 1 && box.Type != "0008" && box.Parent == "00000000000000000000000000000000" && chestsToReassign.Contains(box) == false && box.Type != "0002")
                        chestsToReassign.Add(box);
                }
            }
            if (check_treasure_blueShuffle.IsChecked == true)
            {
                foreach (TreasureBox box in customTreasureBoxFile.Contents)
                {
                    if (box.Edit == 1 && box.Type == "0002" && box.Parent == "00000000000000000000000000000000" && chestsToReassign.Contains(box) == false)
                        chestsToReassign.Add(box);
                }
            }
            if (check_treasure_forceFill.IsChecked == true)
            {
                foreach (TreasureBox box in customTreasureBoxFile.Contents)
                {
                    if (box.Content == "0000" && box.Value == "0000" && box.Edit == 1 && box.Type != "0008" && box.Type != "0002" && box.Parent == "00000000000000000000000000000000" && chestsToReassign.Contains(box) == false)
                        chestsToReassign.Add(box);
                }
            }
            // randomize the order
            chestsToReassign = new List<TreasureBox>(chestsToReassign.OrderBy(e => rand.NextDouble()));
            // separate the chests
            var redChests = chestsToReassign.FindAll(e => e.Type == "0001");
            var notRedChests = chestsToReassign.FindAll(e => e.Type != "0001" && e.Type != "0002");
            var blueChests = chestsToReassign.FindAll(e => e.Type == "0002");

            string[] validTraps = { "003d", "0072", "00de", "012f" };

            int x = 0;
            int y = 0;
            int z = 0;
            foreach (TreasureBox box in redChests)
            {
                if (x <= redChests.Count() * goldPercent && goldPercent > 0)
                {
                    box.Content = "FFFF";
                    int gold = rand.Next(minGold,maxGold+1);
                    box.Value = gold.ToString("X4");
                    x++;
                } else if (y <= redChests.Count() * trapPercent && trapPercent > 0)
                {
                    box.Content = "FFFD";
                    box.Value = validTraps[rand.Next(validTraps.Length)];
                    y++;
                } else if (z <= redChests.Count() * emptyPercent && check_treasure_forceFill.IsChecked != true && emptyPercent > 0)
                {
                    box.Content = "0000";
                    box.Value = "0000";
                    z++;
                } else
                {
                    var randomItem = validItems[rand.Next(validItems.Count)];
                    box.Content = randomItem.ID;
                    box.Pool = "0000";
                    box.Value = "0001";
                }
            }
            x = 0; z = 0;
            foreach (TreasureBox box in notRedChests)
            {
                if (x <= notRedChests.Count() * goldPercent && goldPercent > 0)
                {
                    box.Content = "FFFF";
                    int gold = rand.Next(minGold, maxGold + 1);
                    box.Value = gold.ToString("X4");
                    x++;
                }
                else if (z <= notRedChests.Count() * emptyPercent && emptyPercent > 0)
                {
                    box.Content = "0000";
                    box.Value = "0000";
                    z++;
                }
                else
                {
                    var randomItem = validItems[rand.Next(validItems.Count)];
                    box.Content = randomItem.ID;
                    box.Value = "0001";
                    box.Pool = "0000";
                }
            }

            // assign blue chest pools
            int blueChestPoolAmount = 10;
            if (check_treasure_blueRandom.IsChecked == true)
            {
                List<BlueChestItem> entriesToRandomize = new List<BlueChestItem>();
                if (check_treasure_blueShuffle.IsChecked == true)
                    blueChestPoolAmount = rand.Next(1,33);
                for (int i = 1;i <= blueChestPoolAmount;i++)
                {
                    var pool = customTbContentsFile.Contents[i];
                    int amountInPool = rand.Next(1, 9);
                    int j = 0;
                    foreach (BlueChestItem item in pool.Contents)
                    {
                        item.ID = "0000";
                        item.Value = "0000";
                        item.Footer = "00000000";
                        if (j < amountInPool)
                            entriesToRandomize.Add(item);
                        j++;
                    }
                }
                var sortedEntries = entriesToRandomize.OrderBy(e => rand.NextDouble());
                x = 0;
                foreach (BlueChestItem item in sortedEntries)
                {
                    if (x <= sortedEntries.Count() * goldPercent && goldPercent > 0)
                    {
                        item.ID = "FFFF";
                        item.Footer = "00002041";
                        int goldValue = rand.Next(minGold,maxGold+1);
                        item.Value = goldValue.ToString("X4");  
                    } else
                    {
                        item.Value = "0001";
                        item.Footer = "00002041";
                        item.ID = validItems[rand.Next(validItems.Count)].ID;
                    }
                    x++;
                }
            } 
            // then assign blue chest values
            foreach (TreasureBox box in blueChests)
            {
                box.Pool = rand.Next(1,blueChestPoolAmount+1).ToString("X4");
                if (check_treasure_blueRandom.IsChecked == false)
                {
                    if (box.Pool == "0006")
                    {
                        if (box.Model == "0049" || box.Model == "0048" || box.Model == "0047")
                            box.Model = "0048";
                        else
                            box.Model = "0045";
                        box.Container = "Purple Chest";
                    } else
                    {
                        if (box.Model == "0049" || box.Model == "0048" || box.Model == "0047")
                            box.Model = "0049";
                        else
                            box.Model = "0046";
                        box.Container = "Blue Chest";
                    }
                }
            }
            // then lock;unlock chests
            List<string> validLocks = new List<string>();
            if (check_treasure_locks_thiefKey.IsChecked == true)
                validLocks.Add("0115");
            if (check_treasure_locks_magicKey.IsChecked == true)
                validLocks.Add("0116");
            var chestsToLock = Array.FindAll(customTreasureBoxFile.Contents, e => e.Edit == 1 && e.Parent == "00000000000000000000000000000000" && e.Type == "0001").OrderBy(e => rand.NextDouble());
            x = 0;
            if (check_treasure_locks_randomizeLocks.IsChecked == true && validLocks.Count > 0)
            {
                foreach (TreasureBox box in chestsToLock)
                {
                    if (x <= chestsToLock.Count() * lockPercent && lockPercent > 0)
                        box.Lock = validLocks[rand.Next(validLocks.Count)];
                    else
                        box.Lock = "0000";
                    x++;
                }
            }
            
            // then shuffle the magic key
            if (check_treasure_locks_keyLocation.IsChecked == true)
            {
                List<TreasureBox> validShuffleChests = new List<TreasureBox>();
                foreach (TreasureBox box in customTreasureBoxFile.Contents)
                {
                    if ((box.Edit == 1 || box.Edit == 3) && box.Type != "0008" && box.Parent == "00000000000000000000000000000000" &&
                        (box.Type != "0002" || check_treasure_blueRandom.IsChecked == true))
                        validShuffleChests.Add(box);
                }
                var selBox = validShuffleChests[rand.Next(validShuffleChests.Count)];
                var magicKeyBox = findTreasureBoxByID(customTreasureBoxFile.Contents, "0180");
                if (selBox != magicKeyBox)
                {
                    magicKeyBox.Lock = selBox.Lock;
                    magicKeyBox.Content = selBox.Content;
                    magicKeyBox.Value = selBox.Value;
                    magicKeyBox.Pool = selBox.Pool;
                    if (selBox.Type == "0002")
                    {
                        magicKeyBox.Type = "0002";
                        magicKeyBox.Container = selBox.Container;
                        selBox.Type = "0001";
                        selBox.Container = "Red Chest";
                        if (selBox.Model == "0049")
                        {
                            magicKeyBox.Model = "0046";
                            selBox.Model = "0047";
                        }
                        else if (selBox.Model == "0048")
                        {
                            magicKeyBox.Model = "0045";
                            selBox.Model = "0047";
                        } else
                        {
                            magicKeyBox.Model = selBox.Model;
                            selBox.Model = "0001";
                        }
                    }
                    selBox.Lock = "0000";
                    selBox.Content = "0116";
                    selBox.Value = "0001";
                    selBox.Pool = "0000";
                }
            }
            // then assign parents
            var childChests = Array.FindAll(customTreasureBoxFile.Contents, e => e.Parent != "00000000000000000000000000000000");
            foreach (TreasureBox box in childChests)
            {
                TreasureBox parent = findTreasureBoxByCode(customTreasureBoxFile.Contents, box.Parent);
                
                box.Lock = parent.Lock;
                box.Content = parent.Content;
                box.Value = parent.Value;
                box.Pool = parent.Pool;
                if (box.Type == "0001" || box.Type == "0002")
                {
                    box.Model = parent.Model;
                    box.Type = parent.Type;
                }
            }
        }
    }
}
