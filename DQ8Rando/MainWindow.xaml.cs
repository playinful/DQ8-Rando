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
using Microsoft.Win32;
using System.Windows.Forms;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;

using static System.Random;

using Xceed.Wpf.Toolkit;

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
            browseDialog.SelectedPath = Environment.CurrentDirectory;
            tbox_directory.Text = Environment.CurrentDirectory;
            browseDialog.Description = "Select the output folder.";
        }

        // initialize variables here

        public System.Random superRand = new System.Random();
        public System.Random rand;

        public string jsonString = "";

        public List<OptionTab> optionOutputList;
        public AssetFile[] gameAssets;

        Dictionary<string, int> resistName = new Dictionary<string, int>{
            {"Evasion"    , 0}, {"Attack"     , 0}, {"Frizz"      , 0}, {"Sizz"       , 0},
            {"Bang"       , 0}, {"Woosh"      , 0}, {"Zap"        , 0}, {"Crack"      , 0},
            {"Dazzle"     , 1}, {"Snooze"     , 1}, {"Whack"      , 1}, {"Drain Magic", 1},
            {"Fizzle"     , 1}, {"Fuddle"     , 1}, {"Sap"        , 1}, {"Decelerate" , 1},
            {"Poison"     , 1}, {"Paralysis"  , 1}, {"Stun"       , 1}, {"Ban Dance"  , 1},
            {"Fire Breath", 0}, {"Ice Breath" , 0}, {"Strike/Rock", 0}, {"Army"       , 0} };
        
        public Dictionary<string, Encounter> encountList;
        public Dictionary<string, SetEncounter> setEncountList;
        public List<GospelEntry> gospelList;
        public ItemFile itemFile;
        public EncountFile encountFile;
        public TreasureBoxFile tbTreasureBox;
        public TbContentsFile tbContents;
        public HotelCharges hotelCharges;
        public ShopItemFile shopItem;
        public ItemCasinoFile itemCasino;
        public MonsterFile monsterFile;
        public ScoutMonsterFile scoutMonster;
        public Dictionary<string, Action> actionFile;

        public ItemFile customItemFile;
        public EncountFile customEncountFile;
        public TreasureBoxFile customTreasureBoxFile;
        public TbContentsFile customTbContentsFile;
        public HotelCharges customHotelCharges;
        public ShopItemFile customShopItem;
        public ItemCasinoFile customItemCasino;
        public MonsterFile customMonsterFile;
        public ScoutMonsterFile customScoutMonster;

        public string outputFolder;
        public FolderBrowserDialog browseDialog = new FolderBrowserDialog();
        public Dictionary<int, List<GospelEntry>> validEncounterHandler;
        public Dictionary<int, List<SetEncounter>> validSetEncounterHandler;

        //click functions
        private void button_finish_Click(object sender, RoutedEventArgs e)
        {
            initializeOptionOutputList();

            int seed = generateNewRandom(tbox_seed.Text);

            customItemFile = null;
            customEncountFile = null;
            customTreasureBoxFile = null;
            customTbContentsFile = null;
            customHotelCharges = null;
            customShopItem = null;
            customItemCasino = null;
            customMonsterFile = null;
            customScoutMonster = null;

            validEncounterHandler = new Dictionary<int, List<GospelEntry>>();
            validSetEncounterHandler = new Dictionary<int, List<SetEncounter>>();

            outputFolder = tbox_directory.Text + "\\output";

            string message = "Patch successfully built to " + outputFolder + ".\nSeed: " + seed.ToString();

            if (check_treasure_tbRandom.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_blueShuffle.IsChecked == true ||
                check_treasure_forceFill.IsChecked == true || check_treasure_locks_keyLocation.IsChecked == true || check_treasure_locks_randomizeLocks.IsChecked == true ||
                check_treasure_swapBlueRed.IsChecked == true)
                randomizeTreasure();
            if (check_monsters_stats_gold.IsChecked == true || check_monsters_stats_exp.IsChecked == true || check_monsters_stats_hp.IsChecked == true || check_monsters_stats_mp.IsChecked == true ||
                check_monsters_stats_attack.IsChecked == true || check_monsters_stats_defence.IsChecked == true || check_monsters_stats_agility.IsChecked == true /*|| check_monsters_stats_wisdom.IsChecked == true*/ ||
                check_monsters_stats_items.IsChecked == true || check_monsters_stats_resist.IsChecked == true || check_monsters_stats_balance.IsChecked == true || check_monsters_stats_actions.IsChecked == true)
                randomizeMonsters();
            if (radio_monsters_overworld_noChange.IsChecked == false ||
                radio_monsters_arena_noChange.IsChecked == false ||
                radio_monsters_boss_noChange.IsChecked == false ||
                check_monsters_enemySpecial.IsChecked == true)
                randomizeEncounters();
            if (check_monsters_infamous.IsChecked == true)
                shuffleInfamousMonsters();
            if (radio_shop_noChange.IsChecked == false || check_shop_itemPrice.IsChecked == true || check_shop_shopPrice.IsChecked == true)
                randomizeShops();
            if (radio_shop_casino_noChange.IsChecked == false || check_shop_casino_Price.IsChecked == true)
                randomizeCasino();
            if (check_shop_hotelCharges.IsChecked == true)
                randomizeHotelCharges();

            var x = createOutputFolder(outputFolder);

            if (x == null)
            {
                outputDataToFile(outputFolder);
                generateSpoilerLog(outputFolder);
                generateOptionLog(outputFolder, seed);
            }
            else
            {
                message = "Error: Unable to create output directory.";
            }

            copyAssets(outputFolder);

            MessageBoxResult result = System.Windows.MessageBox.Show(message);

            if (x != null)
                tbox_directory.Text = Environment.CurrentDirectory;

        }
        private void button_browse_Click(object sender, RoutedEventArgs e)
        {
            browseDialog.SelectedPath = tbox_directory.Text;
            browseDialog.ShowDialog();
            tbox_directory.Text = browseDialog.SelectedPath;
        }

        // IsEnabled check functions
        private void monsters_EnableCheck(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton[] radios = { radio_monsters_arena_noChange, radio_monsters_arena_random, radio_monsters_arena_shuffle, radio_monsters_boss_noChange, radio_monsters_boss_shuffle, radio_monsters_boss_random,
                radio_monsters_overworld_noChange, radio_monsters_overworld_random, radio_monsters_overworld_shuffle, radio_monsters_overworld_tables};
            System.Windows.Controls.CheckBox[] checks = { check_monsters_boss_allowMultiple, check_monsters_enemySpecial, check_monsters_gospel, check_monsters_includeArena, check_monsters_includeMemoriam, check_monsters_includePostgame,
                check_monsters_infamous, check_monsters_mixInInfamous};

            if (radios.All<System.Windows.Controls.RadioButton>(i => i != null) && checks.All<System.Windows.Controls.CheckBox>(i => i != null))
            {
                radios = null;
                checks = null;

                bool isRandom = (radio_monsters_overworld_random.IsChecked == true || radio_monsters_boss_random.IsChecked == true || radio_monsters_arena_random.IsChecked == true || check_monsters_enemySpecial.IsChecked == true);
                
                check_monsters_mixInInfamous.IsEnabled = isRandom;
                check_monsters_includeArena.IsEnabled = (radio_monsters_overworld_random.IsChecked == true || radio_monsters_boss_random.IsChecked == true || check_monsters_enemySpecial.IsChecked == true);

                check_monsters_includePostgame.IsEnabled = isRandom || radio_monsters_boss_shuffle.IsChecked == true;
                check_monsters_includeMemoriam.IsEnabled = isRandom || radio_monsters_boss_shuffle.IsChecked == true;

                //check_monsters_gospel.IsEnabled = (radio_monsters_overworld_noChange.IsChecked == false || radio_monsters_boss_noChange.IsChecked == false);
                check_monsters_boss_allowMultiple.IsEnabled = (radio_monsters_boss_random.IsChecked == true || radio_monsters_arena_random.IsChecked == true);
            }
        }
        private void monsters_stats_enableCheck(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox[] checkList = { check_monsters_stats_attack, check_monsters_stats_defence, check_monsters_stats_hp, check_monsters_stats_mp, /*check_monsters_stats_wisdom,*/ check_monsters_stats_agility };
            check_monsters_stats_all.IsChecked = checkList.All(b => b.IsChecked == true);

            tbox_monster_statMax.IsEnabled = checkList.Any(b => b.IsChecked == true) || check_monsters_stats_exp.IsChecked == true || check_monsters_stats_gold.IsChecked == true;
            tbox_monster_statMin.IsEnabled = tbox_monster_statMax.IsEnabled;
        }
        private void monsters_stats_all(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox[] checkList = { check_monsters_stats_attack, check_monsters_stats_defence, check_monsters_stats_hp, check_monsters_stats_mp, /*check_monsters_stats_wisdom,*/ check_monsters_stats_agility };
            foreach (System.Windows.Controls.CheckBox box in checkList)
            {
                box.IsChecked = check_monsters_stats_all.IsChecked;
            }

            monsters_stats_enableCheck(sender, e);
        }
        private void treasure_goldPercent_EnableCheck(object sender, RoutedEventArgs e)
        {
            tbox_treasure_goldPercentMin.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_goldPercentMax.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_goldAmountMin.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true);
            tbox_treasure_goldAmountMax.IsEnabled = (check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true);
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
            tbox_treasure_locks_min.IsEnabled = (check_treasure_locks_randomizeLocks.IsChecked == true && (check_treasure_locks_thiefKey.IsChecked == true || check_treasure_locks_magicKey.IsChecked == true));
            tbox_treasure_locks_max.IsEnabled = (check_treasure_locks_randomizeLocks.IsChecked == true && (check_treasure_locks_thiefKey.IsChecked == true || check_treasure_locks_magicKey.IsChecked == true));
        }
        private void shop_hotelCharges_EnableCheck(object sender, RoutedEventArgs e)
        {
            tbox_shop_hotelChargesMax.IsEnabled = check_shop_hotelCharges.IsChecked == true;
            tbox_shop_hotelChargesMin.IsEnabled = check_shop_hotelCharges.IsChecked == true;
        }
        private void shop_itemPrice_EnableCheck(object sender, RoutedEventArgs e)
        {
            tbox_shop_itemPriceMin.IsEnabled = check_shop_itemPrice.IsChecked == true;
            tbox_shop_itemPriceMax.IsEnabled = check_shop_itemPrice.IsChecked == true;
        }
        private void shop_shopPrice_EnableCheck(object sender, RoutedEventArgs e)
        {
            tbox_shop_shopPriceMin.IsEnabled = check_shop_shopPrice.IsChecked == true;
            tbox_shop_shopPriceMax.IsEnabled = check_shop_shopPrice.IsChecked == true;
        }
        private void shop_casino_price_EnableCheck(object sender, RoutedEventArgs e)
        {
            tbox_shop_casino_price_max.IsEnabled = check_shop_casino_Price.IsChecked == true;
            tbox_shop_casino_price_min.IsEnabled = check_shop_casino_Price.IsChecked == true;
        }

        // min/max checks
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
        private void minMaxCheck(object sender, Xceed.Wpf.Toolkit.LongUpDown min, Xceed.Wpf.Toolkit.LongUpDown max)
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
        private void shop_hotelCharges_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_shop_hotelChargesMin, tbox_shop_hotelChargesMax);
        }
        private void shop_itemPrice_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_shop_itemPriceMin, tbox_shop_itemPriceMax);
        }
        private void shop_shopPrice_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_shop_shopPriceMin, tbox_shop_shopPriceMax);
        }
        private void shop_casino_Price_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            minMaxCheck(sender, tbox_shop_casino_price_min, tbox_shop_casino_price_max);
        }

        // watermark box function
        private void WatermarkBoxUpdate(System.Windows.Controls.TextBox box, System.Windows.Controls.Label watermark)
        {
            if (box.Text == "" || box.Text.All<char>(e => e == ' ') || box.Text == null)
                watermark.Visibility = Visibility.Visible;
            else
                watermark.Visibility = Visibility.Hidden;
        }

        // seed box functionality
        private void tbox_seed_TextChanged(object sender, TextChangedEventArgs e)
        {
            WatermarkBoxUpdate(tbox_seed, label_seed_watermark);
        }

        // text box checkers
        private void tbox_IsNumericOnly(object sender, TextCompositionEventArgs e)
        {
            int dummy;
            var obj = e.OriginalSource as System.Windows.Controls.TextBox;
            bool success = int.TryParse(obj.Text + e.Text, out dummy);
            e.Handled = !(success || int.TryParse(obj.Text, out dummy) == false);
        }
        private void tbox_PreventSpace(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        // loading and intitializing data
        private void initializeOptionOutputList()
        {
            if (optionOutputList == null)
            {
                jsonString = File.ReadAllText("Data/Options.json");
                optionOutputList = JsonSerializer.Deserialize<List<OptionTab>>(jsonString);
                foreach (OptionTab tab in optionOutputList)
                {
                    tab.Control = FindName(tab.Element) as System.Windows.Controls.Control;
                    if (tab.Parent != null)
                        tab.ParentControl = FindName(tab.Parent) as System.Windows.Controls.Control;
                    foreach (Option opt in tab.Contents)
                    {
                        opt.Control = FindName(opt.Element) as System.Windows.Controls.Control;
                        if (opt.Parent != null)
                            opt.ParentControl = FindName(opt.Parent) as System.Windows.Controls.Control;
                        if (opt.Elements != null && opt.Type == "Choice")
                        {
                            foreach (OptionChoice element in opt.Elements)
                            {
                                element.Control = FindName(element.Element) as System.Windows.Controls.Control;
                                if (element.Parent != null)
                                    element.ParentControl = FindName(element.Parent) as System.Windows.Controls.Control;
                            }
                        }
                    }
                }
            }
        }
        private void loadAssetData()
        {
            if (gameAssets == null)
            {
                jsonString = File.ReadAllText("Data/Assets.json");
                gameAssets = JsonSerializer.Deserialize<AssetFile[]>(jsonString);
                foreach (AssetFile asset in gameAssets)
                {
                    if (asset.Condition != null)
                        asset.ConditionControl = FindName(asset.Condition) as System.Windows.Controls.Control;
                }
            }
        }
        private void loadMonsterFile()
        {
            if (monsterFile == null)
            {
                jsonString = File.ReadAllText("Data/Monsters.json");
                monsterFile = JsonSerializer.Deserialize<MonsterFile>(jsonString);
            }
        }
        private void loadEncounters()
        {
            if (encountList == null)
            {
                jsonString = File.ReadAllText("Data/Encounter.json");
                encountList = JsonSerializer.Deserialize<Dictionary<string, Encounter>>(jsonString);
            }
            loadEncounterFile();
            loadMonsterFile();
            loadGospel();
        }
        private void loadSetEncounters()
        {
            if (setEncountList == null)
            {
                jsonString = File.ReadAllText("Data/SetEncounter.json");
                setEncountList = JsonSerializer.Deserialize<Dictionary<string, SetEncounter>>(jsonString);
            }
            loadEncounterFile();
            loadMonsterFile();
            loadGospel();
        }
        private void loadGospel()
        {
            if (gospelList == null)
            {
                jsonString = File.ReadAllText("Data/Gospel.json");
                gospelList = JsonSerializer.Deserialize<List<GospelEntry>>(jsonString);
            }
        }
        private void loadEncounterFile()
        {
            if (encountFile == null)
            {
                jsonString = File.ReadAllText("Data/EncountTbl.json");
                encountFile = JsonSerializer.Deserialize<EncountFile>(jsonString);
            }
        }
        private void loadItems()
        {
            if (itemFile == null)
            {
                jsonString = File.ReadAllText("Data/Item.json");
                itemFile = JsonSerializer.Deserialize<ItemFile>(jsonString);
            }
        }
        private void loadTreasureFile()
        {
            if (tbTreasureBox == null)
            {
                jsonString = File.ReadAllText("Data/TreasureBox.json");
                tbTreasureBox = JsonSerializer.Deserialize<TreasureBoxFile>(jsonString);
            }
            loadItems();
        }
        private void loadBlueChestFile()
        {
            if (tbContents == null)
            {
                jsonString = File.ReadAllText("Data/BlueChest.json");
                tbContents = JsonSerializer.Deserialize<TbContentsFile>(jsonString);
            }
            loadItems();
        }
        private void loadHotelCharges()
        {
            if (hotelCharges == null)
            {
                jsonString = File.ReadAllText("Data/HotelCharges.json");
                hotelCharges = JsonSerializer.Deserialize<HotelCharges>(jsonString);
            }
        }
        private void loadShopItemFile()
        {
            if (shopItem == null)
            {
                jsonString = File.ReadAllText("Data/ShopItem.json");
                shopItem = JsonSerializer.Deserialize<ShopItemFile>(jsonString);
            }
            loadItems();
        }
        private void loadItemCasinoFile()
        {
            if (itemCasino == null)
            {
                jsonString = File.ReadAllText("Data/Item_Casino.json");
                itemCasino = JsonSerializer.Deserialize<ItemCasinoFile>(jsonString);
            }
            loadItems();
        }
        private void loadScoutMonsterFile()
        {
            if (scoutMonster == null)
            {
                jsonString = File.ReadAllText("Data/ScoutMonster.json");
                scoutMonster = JsonSerializer.Deserialize<ScoutMonsterFile>(jsonString);
            }
            loadMonsterFile();
            loadEncounters();
        }
        private void loadActions()
        {
            if (actionFile == null)
            {
                jsonString = File.ReadAllText("Data/Actions.json");
                actionFile = JsonSerializer.Deserialize<Dictionary<string, Action>>(jsonString);
            }
        }

        // prepare custom data
        private void prepareCustomEncountFile() 
        { 
            if (customEncountFile == null)
            {
                customEncountFile = new EncountFile();
                customEncountFile.Header = encountFile.Header;
                customEncountFile.Contents = new Dictionary<string, EncountTable>();
                foreach (KeyValuePair<string, EncountTable> kv in encountFile.Contents)
                {
                    EncountTable newTable = new EncountTable();
                    newTable.ID = kv.Value.ID;
                    newTable.Region = kv.Value.Region;
                    newTable.Area = kv.Value.Area;
                    newTable.Edit = kv.Value.Edit;
                    newTable.Parent = kv.Value.Parent;
                    newTable.Header = kv.Value.Header;
                    newTable.Contents = new EncountTableEntry[10];
                    newTable.SetEncounters = new EncountTableSetEntry[2];
                    int count = 0;
                    foreach (EncountTableEntry entry in kv.Value.Contents)
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
                    foreach (EncountTableSetEntry entry in kv.Value.SetEncounters)
                    {
                        EncountTableSetEntry newEntry = new EncountTableSetEntry();
                        newEntry.Weight = entry.Weight;
                        newEntry.ID = entry.ID;
                        newTable.SetEncounters[count] = newEntry;
                        count++;
                    }
                    customEncountFile.Contents.Add(kv.Key, newTable);
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
        private void prepareCustomHotelCharges()
        {
            loadHotelCharges();
            if (customHotelCharges == null)
            {
                customHotelCharges = new HotelCharges();
                customHotelCharges.Header = hotelCharges.Header;
                customHotelCharges.Contents = new Hotel[hotelCharges.Contents.Length];
                for (int i = 0; i < hotelCharges.Contents.Length; i++)
                {
                    Hotel cust = new Hotel();
                    var src = hotelCharges.Contents[i];
                    cust.ID = src.ID;
                    cust.Price = src.Price;
                    cust.Location = src.Location;
                    cust.Edit = src.Edit;
                    customHotelCharges.Contents[i] = cust;
                }
            }
        }
        private void prepareCustomShopItemFile()
        {
            loadShopItemFile();
            loadItems();
            if (customShopItem == null)
            {
                customShopItem = new ShopItemFile();
                customShopItem.Header = shopItem.Header;
                customShopItem.Contents = new Shop[shopItem.Contents.Length];
                for (int i = 0; i < shopItem.Contents.Length; i++)
                {
                    Shop cust = new Shop();
                    var src = shopItem.Contents[i];
                    cust.ID = src.ID;
                    cust.Unk1 = src.Unk1;
                    cust.Unk2 = src.Unk2;
                    cust.Unk3 = src.Unk3;
                    cust.Location = src.Location;
                    cust.Parent = src.Parent;
                    cust.Store = src.Store;
                    cust.Sale = src.Sale;
                    cust.Items = new ShopItem[src.Items.Length];
                    cust.Edit = src.Edit;
                    for (int j = 0; j < src.Items.Length; j++)
                    {
                        ShopItem cust2 = new ShopItem();
                        var src2 = src.Items[j];
                        cust2.ID = src2.ID;
                        cust2.Footer = src2.Footer;
                        cust.Items[j] = cust2;
                    }
                    customShopItem.Contents[i] = cust;
                }
            }
        }
        private void prepareCustomItemCasinoFile()
        {
            loadItemCasinoFile();
            loadItems();
            if (customItemCasino == null)
            {
                customItemCasino = new ItemCasinoFile();
                customItemCasino.Header = itemCasino.Header;
                customItemCasino.Contents = new Casino[itemCasino.Contents.Length];
                for (int i = 0; i < itemCasino.Contents.Length; i++)
                {
                    Casino cust = new Casino();
                    var src = itemCasino.Contents[i];
                    cust.ID = src.ID;
                    cust.Location = src.Location;
                    cust.Items = new string[src.Items.Length];
                    for (int j = 0; j < src.Items.Length; j++)
                    {
                        cust.Items[j] = src.Items[j];
                    }
                    cust.Prices = new string[src.Prices.Length];
                    for (int j = 0; j < src.Prices.Length; j++)
                    {
                        cust.Prices[j] = src.Prices[j];
                    }
                    customItemCasino.Contents[i] = cust;
                }
            }
        }
        private void prepareCustomItemFile()
        {
            loadItems();
            if (customItemFile == null)
            {
                customItemFile = new ItemFile();
                customItemFile.Header = itemFile.Header;
                customItemFile.Contents = new Item[itemFile.Contents.Length];
                for (int i = 0; i < itemFile.Contents.Length; i++)
                {
                    Item cust = new Item();
                    var src = itemFile.Contents[i];
                    cust.ID        = src.ID;
                    cust.Name      = src.Name;
                    cust.Spawn     = src.Spawn;
                    cust.Unk1      = src.Unk1;
                    cust.Unk2      = src.Unk2;
                    cust.Unk3      = src.Unk3;
                    cust.Unk4      = src.Unk4;
                    cust.Unk5      = src.Unk5;
                    cust.Icon      = src.Icon;
                    cust.Price     = src.Price;
                    cust.SellPrice = src.SellPrice;
                    cust.Attack    = src.Attack;
                    cust.Defence   = src.Defence;
                    cust.Agility   = src.Agility;
                    cust.Wisdom    = src.Wisdom;
                    cust.Footer    = src.Footer;
                    customItemFile.Contents[i] = cust;
                }
            }
        }
        private void prepareCustomScoutMonster()
        {
            loadScoutMonsterFile();
            if (customScoutMonster == null)
            {
                customScoutMonster = new ScoutMonsterFile();
                customScoutMonster.Header = scoutMonster.Header;
                customScoutMonster.Contents = new Dictionary<string, ScoutMonster>();
                foreach (KeyValuePair<string, ScoutMonster> kv in scoutMonster.Contents)
                {
                    ScoutMonster push = new ScoutMonster();
                    push.ID = kv.Value.ID;
                    push.Monster = kv.Value.Monster;
                    push.Unk1 = kv.Value.Unk1;
                    push.Unk2 = kv.Value.Unk2;
                    push.Shuffle = kv.Value.Shuffle;
                    //push.init();
                    customScoutMonster.Contents.Add(kv.Key, push);
                }
            }
        }
        private void prepareCustomMonsterFile()
        {
            if (customMonsterFile == null)
            {
                loadMonsterFile();
                customMonsterFile = new MonsterFile();
                customMonsterFile.Header = monsterFile.Header;
                customMonsterFile.Contents = new Dictionary<string, Monster>();
                foreach (KeyValuePair<string,Monster> kv in monsterFile.Contents)
                {
                    Monster add = new Monster();
                    var src = kv.Value;
                    add.Agility = src.Agility;
                    add.Attack = src.Attack;
                    add.Defence = src.Defence;
                    add.Experience = src.Experience;
                    add.Footer = src.Footer;
                    add.Gold = src.Gold;
                    add.HP = src.HP;
                    add.ID = src.ID;
                    add.MP = src.MP;
                    add.Name = src.Name;
                    add.Unk1 = src.Unk1;
                    add.Unk2 = src.Unk2;
                    add.Unk3 = src.Unk3;
                    add.Unk4 = src.Unk4;
                    add.Unk5 = src.Unk5;
                    add.Unk6 = src.Unk6;
                    add.Actions = new string[6];
                    for (int i = 0; i < add.Actions.Length; i++)
                    {
                        add.Actions[i] = src.Actions[i];
                    }
                    add.Items = new string[2];
                    for (int i = 0; i < add.Items.Length; i++)
                    {
                        add.Items[i] = src.Items[i];
                    }
                    add.Resistances = new string[22];
                    for (int i = 0; i < add.Resistances.Length; i++)
                    {
                        add.Resistances[i] = src.Resistances[i];
                    }

                    customMonsterFile.Contents.Add(kv.Key, add);
                }
            }
        }

        // initialize RNG
        private int generateNewRandom(string textSeed)
        {
            int seed;
            bool success = int.TryParse(textSeed, out seed);
            if (success == false)
                seed = superRand.Next();
            rand = new System.Random(seed);
            return seed;
        }

        // helper functions
        private string reverseHex(string hex)
        {
            if (hex == null)
                return null;
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
        private Encounter getRandomEncounter(List<GospelEntry> l, int edit)
        {
            Encounter encount = new Encounter();

            GospelEntry ge = l[rand.Next(l.Count)];

            var encs = ge.Monsters.Select(e => findEncounterByMonster(e));

            var validEncs = new List<Encounter>(encs).FindAll(e => encounterIsValid(e,edit));

            encount = validEncs[rand.Next(validEncs.Count)];

            return encount;
        }
        private SetEncounter getRandomSetEncounter(List<SetEncounter> l)
        {
            List<double> chanceList = new List<double>();
            foreach (SetEncounter mon in l)
            {
                if (chanceList.Count == 0)
                    chanceList.Add(mon.Chance);
                else
                    chanceList.Add(mon.Chance + chanceList[chanceList.Count - 1]);
            }
            double result = getRandomDoubleBetweenTwoValues(rand, 0, chanceList[chanceList.Count - 1]);
            SetEncounter encount = new SetEncounter();
            for (var i = 0; i < chanceList.Count; i++)
            {
                if (chanceList[i] > result)
                {
                    encount = l[i];
                    break;
                }
            }

            return encount;
        }
        /*private EncountTableEntry[] getTenRandomEncounters(List<Encounter> l)
        {
            EncountTableEntry[] entryArr = new EncountTableEntry[10];
            int enemyCount = rand.Next(1, 10);
            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                EncountTableEntry entry = new EncountTableEntry();
                entryArr[i] = entry;
                if (count < enemyCount)
                {
                    var randomEnemy = getRandomEncounter(l);
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
        }*/
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
                    var randomEnemy = getRandomSetEncounter(l);
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
                    var randomEnemy = getRandomSetEncounter(l);
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
        private void randomizeEncountTable(EncountTable table)
        {
            // we find our list if it's already in memory
            List<GospelEntry> validEncounters;
            List<SetEncounter> validSetEncounters;
            // if it's not, we create it
            if (validEncounterHandler.TryGetValue(table.Edit, out validEncounters) == false)
            {
                validEncounters = gospelList.FindAll(e => encounterIsValid(e, table.Edit));
                validEncounterHandler.Add(table.Edit, validEncounters);
            }
            if (validSetEncounterHandler.TryGetValue(table.Edit, out validSetEncounters) == false)
            {
                validSetEncounters = new List<SetEncounter>(setEncountList.Values).FindAll(e => encounterIsValid(e, table.Edit));
                validSetEncounterHandler.Add(table.Edit, validSetEncounters);
            }

            // determining enemy counts
            int singleCount = 0;
            int setCount = 0;

            if ((table.Edit >= 1 && table.Edit <= 3) || table.Edit == 10)
            {
                int enemyCount = rand.Next(1, 13);
                singleCount = 0;
                setCount = 0;
                for (int i = 0; i < enemyCount; i++)
                {
                    if (singleCount >= 10)
                    {
                        singleCount = 10;
                        setCount += (enemyCount - i);
                        break;
                    }
                    else if (setCount >= 2)
                    {
                        setCount = 2;
                        singleCount += (enemyCount - i);
                        break;
                    }
                    if (rand.Next(0, 12) < 10)
                    {
                        singleCount++;
                    }
                    else
                    {
                        setCount++;
                    }
                }
            } else
            {
                setCount = 1;
            }
            
            // actually assigning enemies randomly
            for (int i = 0; i < 10; i++)
            {
                EncountTableEntry entry = new EncountTableEntry();
                table.Contents[i] = entry;
                if (i < singleCount)
                {
                    var randomEnemy = getRandomEncounter(validEncounters, table.Edit);
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
            }
            for (int i = 0; i < 2; i++)
            {
                EncountTableSetEntry entry = new EncountTableSetEntry();
                table.SetEncounters[i] = entry;
                if (i < setCount)
                {
                    var randomEnemy = getRandomSetEncounter(validSetEncounters);
                    entry.Weight = "03";
                    entry.ID = randomEnemy.ID;
                }
                else
                {
                    entry.Weight = "00";
                    entry.ID = "86";
                }
            }

            // chance of adding double boss
            if (table.Edit > 3 && table.Edit != 10 && table.Edit != 5 && check_monsters_boss_allowMultiple.IsChecked == true)
            {
                var boss = setEncountList[table.SetEncounters[0].ID];
                if (boss.Count < 10 && boss.Groups < 4 && rand.Next(0,2) == 1)
                {
                    var validDoubleEncounters = validSetEncounters.FindAll(e => e.Count <= (10 - boss.Count) && e.Groups <= (4 - boss.Groups));
                    if (validDoubleEncounters.Count > 0)
                    {
                        var randomEnemy = getRandomSetEncounter(validDoubleEncounters);
                        table.SetEncounters[1].Weight = "03";
                        table.SetEncounters[1].ID = randomEnemy.ID;
                    }
                }
            }
        }
        private bool encounterIsValid(object enc, int edit)
        {
            var e = enc as Encounter;
            var se = enc as SetEncounter;
            var g = enc as GospelEntry;
            var e1 = enc as EncountTableEntry;
            var se1 = enc as EncountTableSetEntry;

            if (e1 != null)
            {
                e = getEncounterById(e1.ID);
            } else if (se1 != null)
            {
                se = getSetEncounterById(se1.ID);
            }

            if (g != null)
            {
                return g.Monsters.Any(f => encounterIsValid(findEncounterByMonster(f), edit));
            }

            int spawn = 0;
            bool hasModel = false;
            if (e != null)
            {
                hasModel = e.HasOverworldModel || edit == 4 || edit > 5;
                spawn = e.Spawn;
            } else if (se != null)
            {
                hasModel = se.HasOverworldModel || edit == 4 || edit > 5;
                spawn = se.Spawn;
            }
            return hasModel && ((spawn >= 1 && spawn <= 3) ||
                (spawn == 4 && check_monsters_mixInInfamous   .IsChecked == true) ||
                (spawn == 5 && (check_monsters_includeArena   .IsChecked == true ||
                    edit == 4)) ||
                (spawn == 6 && (check_monsters_includePostgame.IsChecked == true ||
                    edit == 8 || edit == 7)) ||
                (spawn == 7 && (check_monsters_includeMemoriam.IsChecked == true ||
                    edit == 8)));
        }
        private bool tableContentsAreValid(EncountTable table, EncountTable target)
        {
            return table.Contents.All(e =>
            {
                bool x = e.ID == "0000" || encounterIsValid(getEncounterById(e.ID), target.Edit);
                return x;
            }) && table.SetEncounters.All(e => {
                bool x = e.ID == "00" || e.ID == "86" || encounterIsValid(getSetEncounterById(e.ID), target.Edit);
                return x;
            });
        }
        private bool tableContentsAreValid(EncountTable table)
        {
            return tableContentsAreValid(table, table);
        }
        private void encounterSwapRandom(EncountTable table, List<EncountTable> tables)
        {
            var target = tables[rand.Next(tables.Count)];
            encounterSwap(table, target);
        }
        private void encounterSwap(EncountTable table, EncountTable target)
        {
            var tc1 = table.Contents;
            var tse1 = table.SetEncounters;
            table.Contents = target.Contents;
            table.SetEncounters = target.SetEncounters;
            target.Contents = tc1;
            target.SetEncounters = tse1;
        }
        private bool tryAddEncounter(EncountTable table, object enc)
        {
            var e = enc as EncountTableEntry;
            var se = enc as EncountTableSetEntry;

            if ((e != null && encounterIsValid(getEncounterById(e.ID), table.Edit)))
                return addValueToArray(table.Contents, e);
            else if ((se != null && encounterIsValid(getSetEncounterById(se.ID), table.Edit)))
                return addValueToArray(table.SetEncounters, se);
            else
                return false;
        }
        private bool addValueToArray(object[] arr, object add)
        {
            Console.WriteLine(arr.GetType().GetElementType());
            Console.WriteLine(add.GetType());
            if (add.GetType() != arr.GetType().GetElementType())
                return false;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == null)
                {
                    arr[i] = add;
                    return true;
                }
            }

            return false;
        }
        private bool tableHasRoom(EncountTable table, object enc)
        {
            var e = enc as Encounter;
            var se = enc as SetEncounter;
            var g = enc as GospelEntry;
            var e1 = enc as EncountTableEntry;
            var se1 = enc as EncountTableSetEntry;

            if (e != null || g != null || e1 != null)
            {
                return table.Contents.Any(x => x == null);
            } else if (se != null || se1 != null)
            {
                return table.SetEncounters.Any(x => x == null);
            }

            return false;
        }
        private int randomizeStat(int stat, double min, double max, bool? condition, bool nonZero)
        {
            if (condition != true)
                return stat;
            else
            {
                var xValue = getRandomDoubleBetweenTwoValues(rand, min, max);

                var result = stat * xValue / 100;
                result = result > 65535 ? 65535 : result;
                if (nonZero)
                    result = result < 1 ? 1 : result;
                else
                    result = result < 0 ? 0 : result;

                return (int)result;
            }
        }
        private int randomizeStat(int stat, double min, double max, System.Windows.Controls.CheckBox condition, bool nonZero)
        {
            return randomizeStat(stat, min, max, condition.IsChecked, nonZero);
        }
        private int randomizeStat(int stat, double min, double max, System.Windows.Controls.CheckBox condition)
        {
            return randomizeStat(stat, min, max, condition.IsChecked, false);
        }
        private int randomizeStat(int stat, double min, double max, bool? condition)
        {
            return randomizeStat(stat, min, max, condition, false);
        }
        private int randomizeStat(int stat, double min, double max)
        {
            return randomizeStat(stat, min, max, true, false);
        }
        private string capFirstLetter(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
        private string stringFillToLength(string str, int length, char fill, int align)
        {
            if (str.Length < length)
            {
                string filler = string.Concat(Enumerable.Repeat(fill, length - str.Length));
                if (align == 1)
                    return filler + str;
                else
                    return str + filler;
            }
            else
                return str;
        }
        private string stringFillToLength(string str, int length, char fill)
        {
            return stringFillToLength(str, length, fill, 0);
        }
        private string getResistPercent(string value, int type)
        {
            switch (value)
            {
                case "00":
                    return "0";
                case "01":
                    if (type == 0)
                        return "25";
                    else
                        return "15";
                case "02":
                    return "50";
                case "03":
                    return "100";
                default:
                    return value;
            }
        }

        // data search functions
        private Encounter getEncounterById(string id)
        {
            loadEncounters();
            return id == null ? null : encountList[id];
        }
        private SetEncounter getSetEncounterById(string id)
        {
            loadSetEncounters();
            return setEncountList[id];
        }
        private TreasureBox findTreasureBoxByCode(TreasureBox[] contents, string code)
        {
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
            foreach (Item item in itemFile.Contents)
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
            foreach (BlueChestPool pool in tbc.Contents)
            {
                if (pool.ID == id)
                    return pool;
            }
            return null;
        }
        private Monster getMonsterById(string id)
        {
            return monsterFile.Contents[id];
        }
        private Monster getMonsterByEncounterID(string id)
        {
            return getMonsterById(getEncounterById(id).Monster);
        }
        private Encounter findEncounterByMonster(string id)
        {
            Encounter res = null;

            foreach (Encounter enc in encountList.Values)
            {
                if (id == enc.Monster)
                    return enc;
            }

            return res;
        }
        private EncountTable getEncountTableByID(EncountFile file, string id)
        {
            return file.Contents[id];
        }
        private Action getActionById(string id)
        {
            return actionFile[id];
        }

        // main randomizer functions
        private void randomizeMonsters()
        {
            loadMonsterFile();
            prepareCustomMonsterFile();
            loadItems();
            loadActions();

            double minStat = (double)tbox_monster_statMin.Value;
            double maxStat = (double)tbox_monster_statMax.Value;

            if (check_monsters_stats_balance.IsChecked == true)
            {
                // to be added
            } else
            {
                foreach (Monster mon in customMonsterFile.Contents.Values)
                {
                    mon.HP = randomizeStat(mon.HP, minStat, maxStat, check_monsters_stats_hp, true);
                    mon.MP = randomizeStat(mon.MP, minStat, maxStat, check_monsters_stats_mp);
                    mon.Attack = randomizeStat(mon.Attack, minStat, maxStat, check_monsters_stats_attack);
                    mon.Defence = randomizeStat(mon.Defence, minStat, maxStat, check_monsters_stats_defence);
                    mon.Agility = randomizeStat(mon.Agility, minStat, maxStat, check_monsters_stats_agility);
                    //mon.Wisdom = randomizeStat(mon.Wisdom, minStat, maxStat, check_monsters_stats_wisdom);
                    mon.Gold = randomizeStat(mon.Gold, minStat, maxStat, check_monsters_stats_gold);
                    mon.Experience = randomizeStat(mon.Experience, minStat, maxStat, check_monsters_stats_exp);
                    if (check_monsters_stats_resist.IsChecked == true)
                    {
                        for (int i = 0; i < mon.Resistances.Length; i++)
                        {
                            mon.Resistances[i] = "0" + rand.Next(4).ToString();
                        }
                    }
                    if (check_monsters_stats_actions.IsChecked == true)
                    {
                        // to be added
                    }
                }
            }
            if (check_monsters_stats_items.IsChecked == true)
            {
                loadItems();
                Item[] validItems = Array.FindAll<Item>(itemFile.Contents, e => e.Spawn == 1);

                foreach (Monster mon in monsterFile.Contents.Values)
                {
                    var randVal = rand.NextDouble();
                    int itemAmount = 0;
                    if (randVal > 0.05)
                        itemAmount++;
                    if (randVal > 0.5)
                        itemAmount++;

                    for (int i = 0; i < mon.Items.Length; i++)
                    {
                        if (i < itemAmount)
                        {
                            var randomItem = validItems[rand.Next(validItems.Length)];
                            mon.Items[i] = randomItem.ID;
                        }
                        else
                            mon.Items[i] = "0000";
                    }
                }
            }
        }
        private void randomizeEncounters()
        {
            loadEncounters();
            loadSetEncounters();
            prepareCustomEncountFile();

            // we find our working monster file
            MonsterFile mons = (customMonsterFile != null) ? customMonsterFile : monsterFile;

            // let's gather the encounter tables we'll randomize
            var randomTables = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (
                (((e.Edit >= 1 && e.Edit <= 3) || e.Edit == 10) && radio_monsters_overworld_random.IsChecked == true) ||
                (e.Edit == 4 && radio_monsters_arena_random.IsChecked == true) || 
                /*(e.Edit == 5 && check_monsters_enemySpecial.IsChecked == true) ||*/
                (((e.Edit >= 6 && e.Edit <= 8) || e.Edit == 11) && radio_monsters_boss_random.IsChecked == true) /*|| 
                (e.Edit == 9 && there are chests that exist) */));

            // shuffle monsters
            var shuffleTables = new List<List<EncountTable>>();
            if (radio_monsters_overworld_tables.IsChecked == true)
            {
                shuffleTables.Add(new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (e.Edit >= 1 && e.Edit <= 3) || e.Edit == 10));
            }
            if (radio_monsters_boss_shuffle.IsChecked == true)
            {
                var shuff1 = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (e.Edit == 6 || e.Edit == 11));
                var shuff2 = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (e.Edit == 7));
                var shuff3 = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (e.Edit == 8));
                if (check_monsters_includePostgame.IsChecked == true)
                    shuff1.AddRange(shuff2);
                else
                    shuffleTables.Add(shuff2);
                if (check_monsters_includeMemoriam.IsChecked == true)
                    shuff1.AddRange(shuff3);
                else
                    shuffleTables.Add(shuff3);
                shuffleTables.Add(shuff1);
            }
            if (radio_monsters_arena_shuffle.IsChecked == true)
            {
                shuffleTables.Add(new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => e.Edit == 4));
            }
            foreach (List<EncountTable> tables in shuffleTables)
            {
                // start with a simple indiscriminate shuffle
                var shuffledValues = new List<EncountTable>(tables.OrderBy(e => rand.NextDouble()).Select(e => {
                    var x = new EncountTable();
                    x.Contents = e.Contents;
                    x.SetEncounters = e.SetEncounters;
                    return x;
                }));
                for (int i = 0; i < shuffledValues.Count; i++)
                {
                    tables[i].Contents = shuffledValues[i].Contents;
                    tables[i].SetEncounters = shuffledValues[i].SetEncounters;
                }

                // re-sort the tables so that they're in correct positions
                while (tables.Any(e => tableContentsAreValid(e) != true))
                {
                    foreach (EncountTable table in tables)
                    {
                        if (tableContentsAreValid(table) != true)
                        {
                            var validTables = tables.FindAll(e => tableContentsAreValid(table, e));
                            var validReplacementTables = tables.FindAll(e => tableContentsAreValid(e, table));
                            if (validReplacementTables.Count > 0)
                            {
                                encounterSwapRandom(table, validReplacementTables);
                            }
                            else
                            {
                                encounterSwapRandom(table, validTables);
                            }
                        }
                    }
                }
            }
            // shuffle individual encounters
            List<EncountTable> monsterShuffleTables;
            if (radio_monsters_overworld_shuffle.IsChecked == true)
            {
                monsterShuffleTables = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (e.Edit >= 1 && e.Edit <= 3) || e.Edit == 10);
                List<object> shuffleMonsters = new List<object>();
                foreach (EncountTable table in monsterShuffleTables)
                {
                    shuffleMonsters.AddRange(Array.FindAll<EncountTableEntry>(table.Contents, e => e.ID != "0000"));
                    shuffleMonsters.AddRange(Array.FindAll<EncountTableSetEntry>(table.SetEncounters, e => e.ID != "00" && e.ID != "86"));
                    table.Contents = new EncountTableEntry[10];
                    table.SetEncounters = new EncountTableSetEntry[2];
                }
                var validMonsters = new Dictionary<int, List<object>>();
                // assign one monster to each table
                foreach (EncountTable table in monsterShuffleTables)
                {
                    List<object> valid;
                    if (validMonsters.TryGetValue(table.Edit, out valid) == false)
                    {
                        valid = shuffleMonsters.FindAll(e => encounterIsValid(e, table.Edit));
                        validMonsters.Add(table.Edit, valid);
                    }
                    var randomEncounter = valid[rand.Next(valid.Count)];
                    bool succ = tryAddEncounter(table, randomEncounter);
                    if (succ)
                    {
                        valid.Remove(randomEncounter);
                        shuffleMonsters.Remove(randomEncounter);
                    } else
                    {
                        Console.WriteLine("Error");
                    }
                }
                // then assign remaining monsters to random tables
                foreach (object enc in shuffleMonsters)
                {
                    var validTables = monsterShuffleTables.FindAll(e => tableHasRoom(e, enc) && encounterIsValid(enc, e.Edit));
                    var randomTable = validTables[rand.Next(validTables.Count)];
                    bool succ = tryAddEncounter(randomTable, enc);
                    if (succ) { } else
                    {
                        Console.WriteLine("Error");
                    }
                }
                // finally, fill the remaining empty spaces in all of the tables with blank entries
                foreach (EncountTable table in monsterShuffleTables)
                {
                    EncountTableEntry defaultEntry = new EncountTableEntry();
                    defaultEntry.ID = "0000";
                    defaultEntry.Footer = "00000000";
                    defaultEntry.Arg1 = "00";
                    defaultEntry.Arg2 = "00";
                    EncountTableSetEntry defaultSetEntry = new EncountTableSetEntry();
                    defaultSetEntry.ID = "86";
                    defaultSetEntry.Weight = "00";

                    for (int i = 0; i < 10; i++)
                    {
                        if (table.Contents[i] == null)
                        {
                            table.Contents[i] = defaultEntry;
                        }
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        if (table.SetEncounters[i] == null)
                        {
                            table.SetEncounters[i] = defaultSetEntry;
                        }
                    }
                }
            }

            // do the randoms
            if (check_monsters_gospel.IsChecked == true) { } else
            {
                foreach (EncountTable table in randomTables)
                {
                    randomizeEncountTable(table);
                }
            }

            // set parents
            foreach (EncountTable table in customEncountFile.Contents.Values)
            {
                if (table.Parent != null && table.Parent != "")
                    table.Contents = getEncountTableByID(customEncountFile, table.Parent).Contents;
            }

            
        }
        private void shuffleInfamousMonsters()
        {
            loadScoutMonsterFile();
            prepareCustomScoutMonster();

            var scoutMonsters = new List<ScoutMonster>(customScoutMonster.Contents.Values).FindAll(e => e.Shuffle);
            var randomlyOrderedMonsters = new List<string>(scoutMonsters.OrderBy(e => rand.NextDouble())
                .Select(e => e.Monster));

            for (int i = 0; i < scoutMonsters.Count; i++)
            {
                scoutMonsters[i].Monster = randomlyOrderedMonsters[i];
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

            double goldPercent = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_goldPercentMin.Value, (double)tbox_treasure_goldPercentMax.Value) / 100;
            double emptyPercent = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_emptyPercentMin.Value, (double)tbox_treasure_emptyPercentMax.Value) / 100;
            double lockPercent = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_locks_min.Value, (double)tbox_treasure_locks_max.Value) / 100;
            double bluePercent = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_bluePercentMin.Value, (double)tbox_treasure_bluePercentMax.Value) / 100;
            double trapPercent = getRandomDoubleBetweenTwoValues(rand, (double)tbox_treasure_trapPercentMin.Value, (double)tbox_treasure_trapPercentMax.Value) / 100;
            int minGold = (int)tbox_treasure_goldAmountMin.Value;
            int maxGold = (int)tbox_treasure_goldAmountMax.Value;

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
                    }
                    else
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
            Item[] validItems = Array.FindAll<Item>(itemFile.Contents, e => e.Spawn == 1);
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
                    int gold = rand.Next(minGold, maxGold + 1);
                    box.Value = gold.ToString("X4");
                    x++;
                }
                else if (y <= redChests.Count() * trapPercent && trapPercent > 0)
                {
                    box.Content = "FFFD";
                    box.Value = validTraps[rand.Next(validTraps.Length)];
                    y++;
                }
                else if (z <= redChests.Count() * emptyPercent && check_treasure_forceFill.IsChecked != true && emptyPercent > 0)
                {
                    box.Content = "0000";
                    box.Value = "0000";
                    z++;
                }
                else
                {
                    var randomItem = validItems[rand.Next(validItems.Count())];
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
                    var randomItem = validItems[rand.Next(validItems.Count())];
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
                    blueChestPoolAmount = rand.Next(1, 33);
                for (int i = 1; i <= blueChestPoolAmount; i++)
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
                        int goldValue = rand.Next(minGold, maxGold + 1);
                        item.Value = goldValue.ToString("X4");
                    }
                    else
                    {
                        item.Value = "0001";
                        item.Footer = "00002041";
                        item.ID = validItems[rand.Next(validItems.Count())].ID;
                    }
                    x++;
                }
            }
            // then assign blue chest values
            foreach (TreasureBox box in blueChests)
            {
                box.Pool = rand.Next(1, blueChestPoolAmount + 1).ToString("X4");
                if (check_treasure_blueRandom.IsChecked == false)
                {
                    if (box.Pool == "0006")
                    {
                        if (box.Model == "0049" || box.Model == "0048" || box.Model == "0047")
                            box.Model = "0048";
                        else
                            box.Model = "0045";
                        box.Container = "Purple Chest";
                    }
                    else
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
                        }
                        else
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
        private void randomizeHotelCharges()
        {
            prepareCustomHotelCharges();
            int hotelChargeMin = (int)tbox_shop_hotelChargesMin.Value;
            int hotelChargeMax = (int)tbox_shop_hotelChargesMax.Value;

            foreach (Hotel hotel in customHotelCharges.Contents)
            {
                if (hotel.Edit == 1)
                    hotel.Price = rand.Next(hotelChargeMin, hotelChargeMax + 1);
            }
        }
        private void randomizeShops()
        {
            Shop[] validShops = new Shop[0];
            Shop[] validCustomShops = new Shop[0];

            if (radio_shop_noChange.IsChecked == false || check_shop_shopPrice.IsChecked == true)
            {
                prepareCustomShopItemFile();
                validShops = Array.FindAll<Shop>(shopItem.Contents, e => e.Edit == 1 && e.Parent == null);
                validCustomShops = Array.FindAll<Shop>(customShopItem.Contents, e => e.Edit == 1 && e.Parent == null);
            }
            if (check_shop_itemPrice.IsChecked == true)
                prepareCustomItemFile();

            int itemPriceMin = (int)tbox_shop_itemPriceMin.Value;
            int itemPriceMax = (int)tbox_shop_itemPriceMax.Value;
            int shopPriceMin = (int)tbox_shop_shopPriceMin.Value;
            int shopPriceMax = (int)tbox_shop_shopPriceMax.Value;

            // start by shuffling or randomizing
            if (radio_shop_shuffleShop.IsChecked == true)
            {
                var shuffledShops = new List<Shop>(validCustomShops.OrderBy(e => rand.NextDouble()));
                for (int i = 0; i < validCustomShops.Length; i++)
                {
                    Shop cust = validCustomShops[i];
                    Shop src = shuffledShops[i];

                    cust.Items = src.Items;
                    cust.Sale = src.Sale;
                }
            }
            else if (radio_shop_shuffleItem.IsChecked == true)
            {
                List<ShopItem> itemList = new List<ShopItem>();
                foreach (Shop shop in validCustomShops)
                {
                    foreach (ShopItem item in shop.Items)
                    {
                        if (item.ID != "0000")
                            itemList.Add(item);
                    }
                }
                itemList = new List<ShopItem>(itemList.OrderBy(e => rand.NextDouble()));
                int j = 0;
                foreach (Shop shop in validCustomShops)
                {
                    for (int i = 0; i < shop.Items.Length; i++)
                    {
                        ShopItem item = shop.Items[i];
                        if (item.ID != "0000")
                        {
                            shop.Items[i] = itemList[j];
                            j++;
                        }
                    }
                }

            }
            else if (radio_shop_randomItem.IsChecked == true)
            {
                Item[] validItems = Array.FindAll<Item>(itemFile.Contents, e => e.Spawn == 1);
                foreach (Shop shop in validCustomShops)
                {
                    int itemCount = rand.Next(1, 11);
                    for (int i = 0; i < shop.Items.Length; i++)
                    {
                        ShopItem item = shop.Items[i];
                        if (i < itemCount)
                        {
                            Item randItem = validItems[rand.Next(validItems.Length)];
                            item.ID = randItem.ID;
                        }
                        else
                            item.ID = "0000";
                    }
                }
            }

            // now assign shop sales
            if (check_shop_shopPrice.IsChecked == true)
            {
                foreach (Shop shop in validCustomShops)
                {
                    shop.Sale = rand.Next(shopPriceMin, shopPriceMax + 1);
                }
            }

            // assign parents
            if (radio_shop_noChange.IsChecked == false || check_shop_shopPrice.IsChecked == true)
            {
                foreach (Shop shop in customShopItem.Contents)
                {
                    if (shop.Parent != null)
                    {
                        Shop parent = Array.Find(customShopItem.Contents, e => e.ID == shop.Parent);
                        shop.Items = parent.Items;
                        shop.Sale = parent.Sale;
                    }
                }
            }

            // now assign item prices
            if (check_shop_itemPrice.IsChecked == true)
            {
                foreach (Item item in customItemFile.Contents)
                {
                    if (item.Spawn != 0 && item.Spawn != 3)
                    {
                        int price = rand.Next(itemPriceMin, itemPriceMax + 1);
                        item.Price = price.ToString("X8");
                        int sellPrice = (int)Math.Ceiling(price / 2.0);
                        item.SellPrice = sellPrice.ToString("X8");
                    }
                }
            }
        }
        private void randomizeCasino()
        {
            if (radio_shop_casino_noChange.IsChecked == false || check_shop_casino_Price.IsChecked == true)
                prepareCustomItemCasinoFile();

            int casinoPriceMin = (int)tbox_shop_casino_price_min.Value;
            int casinoPriceMax = (int)tbox_shop_casino_price_max.Value;

            Item[] validItems = Array.FindAll<Item>(itemFile.Contents, e => e.Spawn == 1);

            // randomize prizes
            if (radio_shop_casino_shufflePrize.IsChecked == true)
            {
                List<string> shuffleItems = new List<string>();
                foreach (Casino casino in itemCasino.Contents)
                {
                    foreach (string item in casino.Items)
                    {
                        if (item != "0000")
                            shuffleItems.Add(item);
                    }
                }
                shuffleItems = new List<string>(shuffleItems.OrderBy(e => rand.NextDouble()));
                int j = 0;
                foreach (Casino casino in customItemCasino.Contents)
                {
                    for (int i = 0; i < casino.Items.Length; i++)
                    {
                        if (casino.Items[i] != "0000")
                        {
                            casino.Items[i] = shuffleItems[j];
                            j++;
                        }
                    }
                }
            }
            else if (radio_shop_casino_randomPrize.IsChecked == true)
            {
                foreach (Casino casino in customItemCasino.Contents)
                {
                    int itemAmount = rand.Next(1, 13);
                    for (int i = 0; i < 12; i++)
                    {
                        if (check_shop_casino_Price.IsChecked == true)
                        {
                            if (i < itemAmount)
                                casino.Items[i] = validItems[rand.Next(validItems.Length)].ID;
                            else
                                casino.Items[i] = "0000";
                        }
                        else
                        {
                            if (casino.Items[i] != "0000")
                                casino.Items[i] = validItems[rand.Next(validItems.Length)].ID;
                            else
                                casino.Items[i] = "0000";
                        }
                    }
                }
            }

            // randomize prices
            if (check_shop_casino_Price.IsChecked == true)
            {
                foreach (Casino casino in customItemCasino.Contents)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (casino.Items[i] != "0000")
                            casino.Prices[i] = rand.Next(casinoPriceMin, casinoPriceMax + 1).ToString("X8");
                        else
                            casino.Prices[i] = "00000000";
                    }
                }
            }
        }

        // output functions
        private Exception createOutputFolder(string path)
        {
            try
            {
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
                return null;
            }
            catch (Exception x)
            {
                return x;
            }
        }

        // spoiler logs
        private void generateSpoilerLog(string path)
        {
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            if (Directory.Exists(path + "/spoiler") == false)
                Directory.CreateDirectory(path + "/spoiler");
            generateEncounterSpoilerLog(path + "/spoiler/" + "Encounters.txt");
            generateTreasureSpoilerLog (path + "/spoiler/" + "Treasure.txt");
            generateBlueChestSpoilerLog(path + "/spoiler/" + "BlueChests.txt");
            generateHotelChargesSpoilerLog(path + "/spoiler/" + "Inns.txt");
            generateShopSpoilerLog(path + "/spoiler/" + "Shops.txt");
            generateItemPriceSpoilerLog(path + "/spoiler/" + "ItemPrices.txt");
            generateItemCasinoSpoilerLog(path + "/spoiler/" + "Casino.txt");
            generateScoutMonsterSpoilerLog(path + "/spoiler/" + "InfamousMonsters.txt");
            generateMonsterSpoilerLog(path + "/spoiler/" + "Monsters.txt");
        }
        private void generateEncounterSpoilerLog(string path)
        {
            if (customEncountFile != null)
            {
                List<string> spoiler = new List<string>();
                foreach (EncountTable table in customEncountFile.Contents.Values)
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
                            var monster = getMonsterByEncounterID(entry.ID);
                            if (monster != null && monster.ID != "0000" && monsters.Contains(monster.Name) == false)
                                monsters.Add(monster.Name);
                        }
                        foreach (EncountTableSetEntry entry in table.SetEncounters)
                        {
                            var troop = getSetEncounterById(entry.ID);
                            if (troop.TeamName != null)
                            {
                                if (monsters.Contains(troop.TeamName) == false)
                                    monsters.Add(troop.TeamName);
                            } else if (troop != null && troop.Spawn > 0)
                            {
                                foreach (string id in troop.Contents)
                                {
                                    var monster = getMonsterById(id);
                                    if (monster != null && monster.ID != "0000" && monsters.Contains(monster.Name) == false)
                                        monsters.Add(monster.Name);
                                }
                            }
                                
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
        private void generateHotelChargesSpoilerLog(string path)
        {
            if (customHotelCharges != null)
            {
                List<string> outputArr = new List<string>();

                foreach (Hotel hotel in customHotelCharges.Contents)
                {
                    if (hotel.Edit == 1)
                        outputArr.Add(hotel.Location + ": " + hotel.Price.ToString() + " G");
                }

                File.WriteAllText(path, string.Join("\n", outputArr));
            }
        }
        private void generateShopSpoilerLog(string path)
        {
            if (customShopItem != null)
            {
                List<string> outputArr = new List<string>();

                foreach (Shop shop in customShopItem.Contents)
                {
                    if (shop.Edit == 1 && shop.Parent == null)
                    {
                        string push = shop.Location + " - " + shop.Store;
                        if (shop.Sale < 100)
                            push += " (" + (100 - shop.Sale).ToString() + "% off)";
                        else if (shop.Sale > 100)
                            push += " (" + (shop.Sale - 100).ToString() + "% markup)";
                        push += ": ";
                        List<string> pushArr = new List<string>();
                        foreach (ShopItem item in shop.Items)
                        {
                            if (item.ID != "0000")
                            {
                                Item srcItem = findItemByID(item.ID);
                                pushArr.Add(srcItem.Name);
                            }
                        }
                        push += string.Join(", ", pushArr);
                        outputArr.Add(push);
                    }
                }

                File.WriteAllText(path, string.Join("\n", outputArr));
            }
        }
        private void generateItemPriceSpoilerLog(string path)
        {
            if (customItemFile != null && check_shop_itemPrice.IsChecked == true)
            {
                List<string> outputArr = new List<string>();

                foreach (Item item in customItemFile.Contents)
                {
                    if (item.Spawn != 0 && item.Spawn != 3)
                        outputArr.Add(item.Name + ": " + int.Parse(item.Price, System.Globalization.NumberStyles.HexNumber).ToString() + " G");
                }

                File.WriteAllText(path, string.Join("\n", outputArr));
            }
        }
        private void generateItemCasinoSpoilerLog(string path)
        {
            if (customItemCasino != null)
            {
                List<string> outputArr = new List<string>();

                foreach (Casino casino in customItemCasino.Contents)
                {
                    List<string> casinoArr = new List<string>();
                    casinoArr.Add(casino.Location + " Casino");
                    casinoArr.Add("-------------------------------------");
                    for (int i = 0; i < 12; i++)
                    {
                        if (casino.Items[i] != "0000")
                            casinoArr.Add(findItemByID(casino.Items[i]).Name + " - " + int.Parse(casino.Prices[i], System.Globalization.NumberStyles.HexNumber).ToString() + " tokens");
                    }
                    outputArr.Add(string.Join("\n", casinoArr));
                }

                File.WriteAllText(path, string.Join("\n\n", outputArr));
            }
        }
        private void generateScoutMonsterSpoilerLog(string path)
        {
            if (customScoutMonster != null)
            {
                List<string> outputArr = new List<string>();

                for (var i = 0; i < customScoutMonster.Contents.Count; i++)
                {
                    outputArr.Add(getMonsterByEncounterID(scoutMonster.Contents.Values.ElementAt(i).Monster).Name + " -> "
                        + getMonsterByEncounterID(customScoutMonster.Contents.Values.ElementAt(i).Monster).Name);
                }
                /*foreach (ScoutMonster sm in customScoutMonster.Contents.Values)
                {
                    outputArr.Add(getMonsterByEncounterID(sm.OldMonster).Name + " -> " + getMonsterByEncounterID(sm.Monster).Name);
                }*/

                File.WriteAllText(path, string.Join("\n", outputArr));
            }
        }
        private void generateMonsterSpoilerLog(string path)
        {
            if (customMonsterFile != null)
            {
                List<string> outputArr = new List<string>();
                outputArr.Add("If this file is displaying incorrectly, try switching to a fixed-width font.\n");

                foreach (Monster mon in customMonsterFile.Contents.Values)
                {
                    string addStr = "╔" + stringFillToLength(capFirstLetter(mon.Name), 73, '═') + "╗\n";
                    addStr += "║┌Stats──────────────────┬───────────────────────┬───────────────────────┐║\n" +
                        "║│HP:               " + stringFillToLength(mon.HP.ToString(), 5, ' ', 1) +
                        "│MP:               "  + stringFillToLength(mon.MP.ToString(), 5, ' ', 1) +
                        "│Attack:           "  + stringFillToLength(mon.Attack.ToString(), 5, ' ', 1) + "│║\n" +
                        "║├───────────────────────┼───────────────────────┼───────────────────────┤║\n" +
                        "║│Defence:          " + stringFillToLength(mon.Defence.ToString(), 5, ' ', 1) +
                        "│Agility:          "  + stringFillToLength(mon.Agility.ToString(), 5, ' ', 1) +
                        "│Wisdom:           "  + stringFillToLength(mon.Wisdom.ToString(), 5, ' ', 1) + "│║\n" +
                        "║├───────────────────────┴───────────┬───────────┴───────────────────────┤║\n" +
                        "║│Experience:              " + stringFillToLength(mon.Experience.ToString(), 10, ' ', 1) +
                        "│Gold:                    "  + stringFillToLength(mon.Gold.ToString(), 10, ' ', 1) + "│║\n" +
                        "║└───────────────────────────────────┴───────────────────────────────────┘║\n" +
                        "║Items Dropped: ";
                    string itemStr = findItemByID(mon.Items[0]).Name;
                    if (mon.Items[1] != "0000")
                    {
                        itemStr += ", " + findItemByID(mon.Items[1]).Name;
                    }
                    addStr += stringFillToLength(itemStr, 58, ' ') + "║\n";
                    addStr += "║Actions:                                                                 ║\n";

                    List<Action> monActions = new List<Action>();
                    foreach (string act in mon.Actions)
                    {
                        var a = getActionById(act);
                        if (monActions.Contains(a) != true)
                            monActions.Add(a);
                    }
                    List<string> monActionStrings = new List<string>();
                    foreach (Action act in monActions)
                    {
                        monActionStrings.Add("║ - " + stringFillToLength(act.Name, 70, ' ') + "║");
                    }
                    addStr += string.Join("\n", monActionStrings) + "\n";

                    addStr += "║┌Resistances──────┬─────────────────┬─────────────────┬─────────────────┐║\n";

                    string[] resStr = new string[24];
                    resStr[0] = stringFillToLength(resistName.Keys.ElementAt(0) + ":", 12, ' ') + " " + stringFillToLength("???" + "%", 4, ' ', 1);
                    resStr[1] = stringFillToLength(resistName.Keys.ElementAt(1) + ":", 12, ' ') + " " + stringFillToLength("???" + "%", 4, ' ', 1);
                    for (int i = 2; i < 24; i ++)
                    {
                        resStr[i] = stringFillToLength(resistName.Keys.ElementAt(i) + ":", 12, ' ') + " " + stringFillToLength(getResistPercent(mon.Resistances[i-2], resistName.Values.ElementAt(i))+"%", 4, ' ', 1);
                    }
                    List<string> resRows = new List<string>();
                    for (int i = 0; i < 24; i += 4)
                    {
                        resRows.Add(string.Join("│", resStr.Skip(i).Take(4)));
                    }
                    addStr += "║│" + string.Join("│║\n║├─────────────────┼─────────────────┼─────────────────┼─────────────────┤║\n║│", resRows) + 
                        "│║\n";

                    addStr += "║└─────────────────┴─────────────────┴─────────────────┴─────────────────┘║\n" +
                        "╚═════════════════════════════════════════════════════════════════════════╝";

                    outputArr.Add(addStr);
                }

                File.WriteAllText(path, string.Join("\n", outputArr));
            }
        }

        // option log
        private void generateOptionLog(string path, int seed)
        {
            string optionString = "DQ8-Rando v0.4p\nDragon Quest VIII 3DS Randomizer Version 0.4 Prerelease\n\n";
            optionString += "Seed: " + seed.ToString() + "\n\n";
            foreach (OptionTab tab in optionOutputList)
            {
                if (tab.Control != null)
                {
                    optionString += tab.Header + ":\n";
                    foreach (Option opt in tab.Contents)
                    {
                        if ((opt.Control != null && opt.Control.IsEnabled) || opt.Elements != null)
                        {
                            if (opt.Indent != 0)
                            {
                                for (int i = 0; i < opt.Indent; i++)
                                    optionString += "   ";
                            }
                            optionString += " - " + opt.Text + ": ";
                            var check = opt.Control as System.Windows.Controls.CheckBox;
                            if (check != null)
                            {
                                if (check.IsChecked == true)
                                    optionString += "True";
                                else
                                    optionString += "False";
                            }
                            var doubleUpDown = opt.Control as Xceed.Wpf.Toolkit.DoubleUpDown;
                            if (doubleUpDown != null)
                            {
                                optionString += doubleUpDown.Value.ToString();
                            }
                            var shortUpDown = opt.Control as Xceed.Wpf.Toolkit.ShortUpDown;
                            if (shortUpDown != null)
                            {
                                optionString += shortUpDown.Value.ToString();
                            }
                            var longUpDown = opt.Control as Xceed.Wpf.Toolkit.LongUpDown;
                            if (longUpDown != null)
                            {
                                optionString += longUpDown.Value.ToString();
                            }
                            if (opt.Elements != null && opt.Type == "Choice")
                            {
                                var element = Array.Find(opt.Elements, e => {
                                    var radioButton = e.Control as System.Windows.Controls.RadioButton;
                                    if (radioButton != null && radioButton.IsChecked == true)
                                        return true;
                                    else
                                        return false;
                                });
                                if (element != null)
                                    optionString += element.Text;
                            }
                            optionString += "\n";
                        }
                    }
                }
            }
            File.WriteAllText(path + "/options.txt", optionString);
        }

        // copy graphics
        private void copyAssets(string path)
        {
            loadAssetData();
            foreach (AssetFile asset in gameAssets)
            {
                bool doAsset = false;
                if (asset.Condition != null)
                {
                    var checkBox = asset.ConditionControl as System.Windows.Controls.CheckBox;
                    if (checkBox != null && checkBox.IsChecked == true)
                    {
                        doAsset = true;
                    }
                }
                else
                    doAsset = true;

                if (doAsset)
                {
                    foreach (AssetPath asspath in asset.Output)
                    {
                        if (Directory.Exists(path + "/" + asspath.Path) == false)
                            Directory.CreateDirectory(path + "/" + asspath.Path);
                        File.Copy("Resources/" + asset.Source, path + "/" + asspath.Path + "/" + asspath.Filename, true);
                    }
                }
            }
        }

        // write data to file
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
        private void outputDataToFile(string path)
        {
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            if (Directory.Exists(path + "/romfs") == false)
                Directory.CreateDirectory(path + "/romfs");
            if (Directory.Exists(path + "/romfs/data") == false)
                Directory.CreateDirectory(path + "/romfs/data");
            if (Directory.Exists(path + "/romfs/data/Params") == false)
                Directory.CreateDirectory(path + "/romfs/data/Params");
            string fullPath = path + "/romfs/data/Params";
            if (customEncountFile != null)
                outputEncounterTableToFile(customEncountFile, fullPath + "/" + "encount.tbl");
            if (customTreasureBoxFile != null)
                outputTreasureBoxFileToFile(customTreasureBoxFile, fullPath + "/" + "tbTreasureBox.tbl");
            if (customTbContentsFile != null)
                outputTbContentsFileToFile(customTbContentsFile, fullPath + "/" + "tbContents.tbl");
            if (customHotelCharges != null)
                outputHotelChargesToFile(customHotelCharges, fullPath + "/" + "hotelCharges.tbl");
            if (customShopItem != null)
                outputShopItemToFile(customShopItem, fullPath + "/" + "shopItem.tbl");
            if (customItemFile != null)
                outputItemFileToFile(customItemFile, fullPath + "/" + "item.tbl");
            if (customItemCasino != null)
                outputItemCasinoToFile(customItemCasino, fullPath + "/" + "item_casino.tbl");
            if (customScoutMonster != null)
                outputScoutMonsterToFile(customScoutMonster, fullPath + "/" + "scoutMonster.tbl");
            if (customMonsterFile != null)
                outputMonsterFileToFile(customMonsterFile, fullPath + "/" + "monster.tbl");
        }
        private void outputEncounterTableToFile(EncountFile data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (EncountTable table in data.Contents.Values)
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
        private void outputHotelChargesToFile(HotelCharges data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (Hotel hotel in data.Contents)
                {
                    byteString += hotel.ID + hotel.ID;
                    byteString += reverseHex(hotel.Price.ToString("X4"));
                }

                writeHexStringToFile(byteString, path);
            }
        }
        private void outputShopItemToFile(ShopItemFile data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (Shop shop in data.Contents)
                {
                    byteString += shop.ID + shop.Unk1 + shop.Unk2 + shop.Unk3;
                    byteString += reverseHex(shop.Sale.ToString("X8"));
                    foreach (ShopItem item in shop.Items)
                    {
                        byteString += reverseHex(item.ID);
                        byteString += item.Footer;
                    }
                }

                writeHexStringToFile(byteString, path);
            }
        }
        private void outputItemFileToFile(ItemFile data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (Item item in data.Contents)
                {
                    byteString += reverseHex(item.ID);
                    byteString += item.Unk1 + item.Unk2;
                    byteString += reverseHex(item.Icon);
                    byteString += item.Unk3;
                    byteString += reverseHex(item.Price);
                    byteString += reverseHex(item.SellPrice);
                    byteString += item.Unk4;
                    byteString += item.Attack + item.Defence + item.Agility + item.Wisdom;
                    byteString += item.Unk5 + item.Footer;
                }

                writeHexStringToFile(byteString, path);
            }
        }
        private void outputItemCasinoToFile(ItemCasinoFile data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (Casino casino in data.Contents)
                {
                    byteString += casino.ID;
                    foreach (string item in casino.Items)
                    {
                        byteString += reverseHex(item);
                    }
                    foreach (string price in casino.Prices)
                    {
                        byteString += reverseHex(price);
                    }
                }

                writeHexStringToFile(byteString, path);
            }
        }
        private void outputScoutMonsterToFile(ScoutMonsterFile data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (ScoutMonster sm in data.Contents.Values)
                {
                    byteString += reverseHex(sm.ID)
                        + reverseHex(sm.Monster)
                        + sm.Unk1 + sm.Unk2;
                }

                writeHexStringToFile(byteString, path);
            }
        }
        private void outputMonsterFileToFile (MonsterFile data, string path)
        {
            if (data != null)
            {
                string byteString = data.Header;
                foreach (Monster mon in data.Contents.Values)
                {
                    var addString = "";
                    addString += reverseHex(mon.ID)
                        + mon.Unk1
                        + reverseHex(mon.HP.ToString("X4")) + reverseHex(mon.MP.ToString("X4")) + reverseHex(mon.Attack.ToString("X4"))
                        + reverseHex(mon.Defence.ToString("X4")) + reverseHex(mon.Agility.ToString("X4")) + reverseHex(mon.Wisdom.ToString("X4"))
                        + reverseHex(mon.Experience.ToString("X8")) + reverseHex(mon.Gold.ToString("X8"));
                    foreach (string itemid in mon.Items)
                        addString += reverseHex(itemid);
                    addString += mon.Unk2 + string.Join("", mon.Resistances) + mon.Unk3;
                    foreach (string actid in mon.Actions)
                        addString += reverseHex(actid);
                    addString += mon.Unk4 + mon.Unk5 + mon.Unk6 + mon.Footer;

                    byteString += addString;
                }

                writeHexStringToFile(byteString, path);
            }
        }
    }

}
