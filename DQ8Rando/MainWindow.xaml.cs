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

using System.Text.RegularExpressions;

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
            initialized = true;
            loadWindow();
        }

        // initialize variables here
        public bool initialized = false;

        public System.Random superRand = new System.Random();
        public System.Random rand;

        public string jsonString = "";

        //public List<OptionTab> optionOutputList;
        public List<Option> optionOutputList;
        public List<string> elementList;
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
        public System.Windows.Forms.OpenFileDialog optionSelectDialog = new System.Windows.Forms.OpenFileDialog { DefaultExt = "json", Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*" };
        public Dictionary<int, List<GospelEntry>> validEncounterHandler;
        public Dictionary<int, List<SetEncounter>> validSetEncounterHandler;

        // parameters
        public double balance_variance = 50;

        //click functions
        private void button_finish_Click(object sender, RoutedEventArgs e)
        {
            int seed = generateNewRandom(tbox_seed.Text);

            bool error_handler = true;
            error_handler = false;

            if (error_handler)
            {
                try
                {
                    button_finish_Click_func(sender, e, seed);
                }
                catch
                {
                    string message = "The program has encountered an unexpected error.\nPlease create a new issue at https://github.com/pIayinful/DQ8-Rando/issues, and please submit your 'options.txt' file as well.\n\nIn the meantime, feel free to try another seed.";
                    createOutputFolder(outputFolder);
                    generateOptionLog(outputFolder, seed);
                    generateOptionJson(outputFolder, seed);

                    MessageBoxResult result = System.Windows.MessageBox.Show(message);
                }
            } else
            {
                button_finish_Click_func(sender, e, seed);
            }
        }
        private void button_finish_Click_func(object sender, RoutedEventArgs e, int seed)
        {
            initializeOptionOutputList();

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
            Exception x = null;
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
                check_monsters_enemySpecial.IsChecked == true ||
                check_monsters_stats_balance.IsChecked == true)
                randomizeEncounters();
            if (check_monsters_infamous.IsChecked == true)
                shuffleInfamousMonsters();
            if (radio_shop_noChange.IsChecked == false || check_shop_itemPrice.IsChecked == true || check_shop_shopPrice.IsChecked == true)
                randomizeShops();
            if (radio_shop_casino_noChange.IsChecked == false || check_shop_casino_Price.IsChecked == true)
                randomizeCasino();
            if (check_shop_hotelCharges.IsChecked == true)
                randomizeHotelCharges();

            x = createOutputFolder(outputFolder);

            if (x == null)
            {
                outputDataToFile(outputFolder);
                generateSpoilerLog(outputFolder);
                generateOptionLog(outputFolder, seed);
                generateOptionJson(outputFolder, seed);
                copyAssets(outputFolder);
                // getBSTAs();
            }
            else
            {
                message = "Error: Unable to create output directory.";
            }

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
        private void button_loadOptions_Click(object sender, RoutedEventArgs e)
        {
            initializeOptionOutputList();
            optionSelectDialog.ShowDialog();
            loadOptions(optionSelectDialog.FileName);
        }
        private void button_saveDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int seed = generateNewRandom(tbox_seed.Text);
                if (seed.ToString() != tbox_seed.Text)
                    seed = -1;

                generateOptionJson(".", seed);
                System.Windows.MessageBox.Show("Saved.");
            } catch
            {
                System.Windows.MessageBox.Show("Error: Unable to save configuration.");
            }
        }

        // get BSTAs
        private void getBSTAs()
        {
            loadEncounterFile();
            List<string> outputList = new List<string>();
            foreach (EncountTable table in encountFile.Contents.Values)
            {
                string addStr = table.ID + " - ";

                List<Monster> monsterList = new List<Monster>();
                foreach (EncountTableEntry entry in table.Contents)
                {
                    var mon = getMonsterByEncounterID(entry.ID);
                    if (mon != null && mon.Edit.Contains("N") != true && monsterList.Contains(mon) != true && mon.DoNotAverage != true)
                        monsterList.Add(mon);
                }
                foreach (EncountTableSetEntry entry in table.SetEncounters)
                {
                    foreach (string id in getSetEncounterById(entry.ID).Contents)
                    {
                        var mon = getMonsterById(id);
                        if (mon != null && mon.Edit.Contains("N") != true && monsterList.Contains(mon) != true && mon.DoNotAverage != true)
                            monsterList.Add(mon);
                    }
                }

                if (monsterList.Count > 0)
                {
                    /*double avgHP = monsterList.Select(m => m.HP).Average();
                    double avgMP = monsterList.Select(m => m.MP).Average();
                    double avgAttack = monsterList.Select(m => m.Attack).Average();
                    double avgDefence = monsterList.Select(m => m.Defence).Average();
                    double avgAgility = monsterList.Select(m => m.Agility).Average();
                    double avgWisdom = monsterList.Select(m => m.Wisdom).Average(); */
                    double avgExp = monsterList.Select(m => m.Experience).Average();

                    /*addStr += "HP: " + avgHP.ToString() + ", "
                        + "MP: " + avgMP.ToString() + ", "
                        + "Attack: " + avgAttack.ToString() + ", "
                        + "Defence: " + avgDefence.ToString() + ", "
                        + "Agility: " + avgAgility.ToString() + ", "
                        + "Wisdom: " + avgWisdom.ToString() + ", ";*/
                    addStr += avgExp.ToString();

                    outputList.Add(addStr);
                }
                else
                {
                    addStr += "null";
                    outputList.Add(addStr);
                }
            }
            File.WriteAllText(outputFolder + "/bstAverage.txt", string.Join("\n", outputList));
        }

        // Load options and stuff
        private void loadWindow()
        {
            browseDialog.SelectedPath = Environment.CurrentDirectory;
            tbox_directory.Text = Environment.CurrentDirectory;
            browseDialog.Description = "Select the output folder.";
            loadOptions("./options.json");
        }
        private void loadOptions(string filename = "./options.json")
        {
            try
            {
                if (File.Exists(filename))
                {
                    jsonString = File.ReadAllText(filename);
                    OutputOptionFile optionFile = JsonSerializer.Deserialize<OutputOptionFile>(jsonString);

                    if (optionFile.Seed != -1)
                    {
                        tbox_seed.Text = optionFile.Seed.ToString();
                    }

                    if (optionFile.Path != null && optionFile.Path != "")
                    {
                        tbox_directory.Text = optionFile.Path;
                        browseDialog.SelectedPath = optionFile.Path;
                    }

                    foreach (OutputOption option in optionFile.Elements)
                    {
                        object element = FindName(option.Element);

                        if (element != null)
                        {
                            System.Windows.Controls.Primitives.ToggleButton toggle = element as System.Windows.Controls.Primitives.ToggleButton;
                            Xceed.Wpf.Toolkit.DoubleUpDown dupdown = element as Xceed.Wpf.Toolkit.DoubleUpDown;
                            Xceed.Wpf.Toolkit.SingleUpDown supdown = element as Xceed.Wpf.Toolkit.SingleUpDown;
                            Xceed.Wpf.Toolkit.ShortUpDown shupdown = element as Xceed.Wpf.Toolkit.ShortUpDown;
                            Xceed.Wpf.Toolkit.LongUpDown lupdown = element as Xceed.Wpf.Toolkit.LongUpDown;

                            if (toggle != null)
                                toggle.IsChecked = option.BoolValue;
                            else if (dupdown != null)
                                dupdown.Value = option.DoubleValue;
                            else if (supdown != null)
                                supdown.Value = (float)option.DoubleValue;
                            else if (shupdown != null)
                                shupdown.Value = (short)option.IntValue;
                            else if (lupdown != null)
                                lupdown.Value = option.IntValue;
                        }
                    }
                }
            } catch
            {
                System.Windows.MessageBox.Show("Error: unable to load options from file.");
            }
        }

        // IsEnabled check functions

        private void monsters_EnableCheck(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                check_monsters_mixInInfamous.IsEnabled = radio_monsters_overworld_random.IsChecked == true;
                check_monsters_includeArena.IsEnabled = radio_monsters_overworld_random.IsChecked == true || radio_monsters_boss_random.IsChecked == true;
                check_monsters_includePostgame.IsEnabled = radio_monsters_overworld_random.IsChecked == true || radio_monsters_boss_noChange.IsChecked != true || radio_monsters_arena_random.IsChecked == true;
                check_monsters_includeMemoriam.IsEnabled = check_monsters_includePostgame.IsEnabled;
                check_monsters_boss_allowMultiple.IsEnabled = radio_monsters_boss_random.IsChecked == true || radio_monsters_arena_random.IsChecked == true;
            }
        }
        private void monsters_stats_EnableCheck(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                check_monsters_stats_all.IsChecked = check_monsters_stats_hp.IsChecked == true && check_monsters_stats_mp.IsChecked == true && check_monsters_stats_attack.IsChecked == true &&
                    check_monsters_stats_defence.IsChecked == true && check_monsters_stats_agility.IsChecked == true;

                tbox_monster_statMax.IsEnabled = check_monsters_stats_hp.IsChecked == true || check_monsters_stats_mp.IsChecked == true || check_monsters_stats_attack.IsChecked == true || check_monsters_stats_defence.IsChecked == true ||
                    check_monsters_stats_agility.IsChecked == true || check_monsters_stats_exp.IsChecked == true || check_monsters_stats_gold.IsChecked == true || check_monsters_stats_balance.IsChecked == true;
                tbox_monster_statMin.IsEnabled = tbox_monster_statMax.IsEnabled;
            }
        }
        private void monsters_stats_all_click(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                bool all_checked = check_monsters_stats_all.IsChecked == true;

                check_monsters_stats_hp.IsChecked = all_checked;
                check_monsters_stats_mp.IsChecked = all_checked;
                check_monsters_stats_attack.IsChecked = all_checked;
                check_monsters_stats_defence.IsChecked = all_checked;
                check_monsters_stats_agility.IsChecked = all_checked;
            }
        }
        //treasure
        private void treasure_EnableCheck(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                tbox_treasure_emptyPercentMax.IsEnabled = check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_blueRandom.IsChecked == true || check_treasure_forceFill.IsChecked == true;
                tbox_treasure_emptyPercentMin.IsEnabled = tbox_treasure_emptyPercentMax.IsEnabled;
                tbox_treasure_goldPercentMax.IsEnabled = tbox_treasure_emptyPercentMax.IsEnabled;
                tbox_treasure_goldPercentMin.IsEnabled = tbox_treasure_emptyPercentMax.IsEnabled;
                tbox_treasure_goldAmountMax.IsEnabled = tbox_treasure_emptyPercentMax.IsEnabled;
                tbox_treasure_goldAmountMin.IsEnabled = tbox_treasure_emptyPercentMax.IsEnabled;

                tbox_treasure_trapPercentMax.IsEnabled = check_treasure_tbRandom.IsChecked == true || check_treasure_swapBlueRed.IsChecked == true || check_treasure_forceFill.IsChecked == true;
                tbox_treasure_trapPercentMin.IsEnabled = tbox_treasure_trapPercentMax.IsEnabled;

                tbox_treasure_bluePercentMax.IsEnabled = check_treasure_swapBlueRed.IsChecked == true;
                tbox_treasure_bluePercentMin.IsEnabled = tbox_treasure_bluePercentMax.IsEnabled;
            }
        }
        private void treasure_locks_EnableCheck(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                check_treasure_locks_thiefKey.IsEnabled = check_treasure_locks_randomizeLocks.IsChecked == true;
                check_treasure_locks_magicKey.IsEnabled = check_treasure_locks_thiefKey.IsEnabled;

                tbox_treasure_locks_max.IsEnabled = check_treasure_locks_randomizeLocks.IsChecked == true && (check_treasure_locks_thiefKey.IsChecked == true || check_treasure_locks_magicKey.IsChecked == true);
                tbox_treasure_locks_min.IsEnabled = tbox_treasure_locks_max.IsEnabled;
            }
        }
        //shopping
        private void shopping_EnableCheck(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                tbox_shop_itemPriceMax.IsEnabled = check_shop_itemPrice.IsChecked == true;
                tbox_shop_itemPriceMin.IsEnabled = tbox_shop_itemPriceMax.IsEnabled;

                tbox_shop_shopPriceMax.IsEnabled = check_shop_shopPrice.IsChecked == true;
                tbox_shop_shopPriceMin.IsEnabled = tbox_shop_shopPriceMax.IsEnabled;
            }
        }
        private void shopping_casino_EnableCheck(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                tbox_shop_casino_price_max.IsEnabled = check_shop_casino_Price.IsChecked == true;
                tbox_shop_casino_price_min.IsEnabled = tbox_shop_casino_price_max.IsEnabled;
            }
        }
        private void shopping_hotel_EnableCheck(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                tbox_shop_hotelChargesMax.IsEnabled = check_shop_hotelCharges.IsChecked == true;
                tbox_shop_hotelChargesMin.IsEnabled = tbox_shop_hotelChargesMax.IsEnabled;
            }
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
                optionOutputList = JsonSerializer.Deserialize<List<Option>>(jsonString);
            }
        }
        private void legacy_initializeOptionOutputList()
        {
            /*if (optionOutputList == null)
            {
                jsonString = File.ReadAllText("Data/Options.json");
                optionOutputList = JsonSerializer.Deserialize<List<OptionTab>>(jsonString);
                foreach (OptionTab tab in optionOutputList)
                {
                    tab.Control = FindName(tab.Element) as System.Windows.Controls.Control;
                    if (tab.Parent != null)
                        tab.ParentControl = FindName(tab.Parent) as System.Windows.Controls.Control;
                    foreach (OptionLegacy opt in tab.Contents)
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
            }*/
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
        private void loadElementList()
        {
            if (elementList == null)
            {
                jsonString = File.ReadAllText("Data/ElementList.json");
                elementList = JsonSerializer.Deserialize<List<string>>(jsonString);
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
            loadMonsterFile();
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
            loadEncounters();
            loadSetEncounters();
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
                loadEncounterFile();

                customEncountFile = new EncountFile();
                customEncountFile.Header = encountFile.Header;
                customEncountFile.Contents = new Dictionary<string, EncountTable>();
                foreach (KeyValuePair<string, EncountTable> kv in encountFile.Contents)
                {
                    EncountTable newTable = kv.Value.Clone();

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
                for (int i = 0; i < tbTreasureBox.Contents.Length; i++)
                {
                    var src = tbTreasureBox.Contents[i];
                    TreasureBox box = new TreasureBox();
                    customTreasureBoxFile.Contents[i] = box;
                    box.Edit = src.Edit;
                    box.Code = src.Code;
                    box.Container = src.Container;
                    box.Content = src.Content;
                    box.EngMap = src.EngMap;
                    box.EngRegion = src.EngRegion;
                    box.ID = src.ID;
                    box.Lock = src.Lock;
                    box.Map = src.Map;
                    box.Model = src.Model;
                    box.Parent = src.Parent;
                    box.Pool = src.Pool;
                    box.Region = src.Region;
                    box.Rot = src.Rot;
                    box.Type = src.Type;
                    box.Unk1 = src.Unk1;
                    box.Value = src.Value;
                    box.XPos = src.XPos;
                    box.YPos = src.YPos;
                    box.ZPos = src.ZPos;
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
                for (int i = 0; i < tbContents.Contents.Length; i++)
                {
                    var src = tbContents.Contents[i];
                    BlueChestPool pool = new BlueChestPool();
                    customTbContentsFile.Contents[i] = pool;
                    pool.ID = src.ID;
                    pool.Unk1 = src.Unk1;
                    pool.Contents = new BlueChestItem[src.Contents.Length];
                    for (int j = 0; j < pool.Contents.Length; j++)
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
                    cust.ID = src.ID;
                    cust.Name = src.Name;
                    cust.Spawn = src.Spawn;
                    cust.Unk1 = src.Unk1;
                    cust.Unk2 = src.Unk2;
                    cust.Unk3 = src.Unk3;
                    cust.Unk4 = src.Unk4;
                    cust.Unk5 = src.Unk5;
                    cust.Icon = src.Icon;
                    cust.Price = src.Price;
                    cust.SellPrice = src.SellPrice;
                    cust.Attack = src.Attack;
                    cust.Defence = src.Defence;
                    cust.Agility = src.Agility;
                    cust.Wisdom = src.Wisdom;
                    cust.Footer = src.Footer;
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
                customMonsterFile = monsterFile.Clone();
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
            for (int i = hex.Length - 2; i >= 0; i -= 2)
            {
                reversedHex += hex.Substring(i, 2);
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
        private Encounter getRandomEncounter(List<GospelEntry> l, EncountTable table)
        {
            Encounter encount = new Encounter();

            GospelEntry ge = l[rand.Next(l.Count)];

            var encs = ge.Monsters.Select(e => findEncounterByMonster(e));

            var validEncs = new List<Encounter>(encs).FindAll(e => encounterIsValid(e, table));

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
        private void randomizeEncountTable(EncountTable table, bool overwrite = true, bool balance = false)
        {
            // we find our list if it's already in memory
            List<GospelEntry> validEncounters;
            List<SetEncounter> validSetEncounters;
            // if it's not, we create it
            if (validEncounterHandler.TryGetValue(table.Edit, out validEncounters) == false)
            {
                validEncounters = gospelList.FindAll(e => encounterIsValid(e, table));
                validEncounterHandler.Add(table.Edit, validEncounters);
            }
            if (validSetEncounterHandler.TryGetValue(table.Edit, out validSetEncounters) == false)
            {
                validSetEncounters = new List<SetEncounter>(setEncountList.Values).FindAll(e => encounterIsValid(e, table));
                validSetEncounterHandler.Add(table.Edit, validSetEncounters);
            }

            // if we do balancing, we trim our valid encounters to those that can balance
            if (balance)
            {
                validEncounters = validEncounters.FindAll(e => encounterIsValid(e, table, balance: balance));
                validSetEncounters = validSetEncounters.FindAll(e => encounterIsValid(e, table, balance: balance));
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
                    if (singleCount >= 10 || singleCount >= validEncounters.Count)
                    {
                        singleCount = validEncounters.Count > 10 ? 10 : validEncounters.Count;
                        setCount = validSetEncounters.Count > (setCount + enemyCount - i) ? (setCount + enemyCount - i) : validSetEncounters.Count;
                        break;
                    }
                    else if (setCount >= 2 || setCount >= validSetEncounters.Count)
                    {
                        setCount = validSetEncounters.Count > 2 ? 2 : validSetEncounters.Count;
                        singleCount = validEncounters.Count > (singleCount + enemyCount - i) ? (singleCount + enemyCount - i) : validEncounters.Count;
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
                EncountTableEntry entry = table.Contents[i];
                if (overwrite == true || entry == null)
                {
                    entry = new EncountTableEntry();
                    table.Contents[i] = entry;
                }
                if (i < singleCount && (overwrite == true || entry.ID == "0000" || entry.ID == null))
                {
                    var randomEnemy = getRandomEncounter(validEncounters, table);
                    entry.Arg1 = "03";
                    entry.Arg2 = "03";
                    entry.ID = randomEnemy.ID;
                    entry.Footer = "0000803F";
                    if (balance)
                    {
                        Monster mon = getMonsterByEncounterID(randomEnemy.ID, customMonsterFile);
                        if (mon.AdjustedLevel == -1)
                        {
                            balanceMonsterToTable(mon, table);
                        }
                    }
                }
            }
            for (int i = 0; i < 2; i++)
            {
                EncountTableSetEntry entry = table.SetEncounters[i];
                if (overwrite == true || entry == null)
                {
                    entry = new EncountTableSetEntry();
                    table.SetEncounters[i] = entry;
                }
                if (i < setCount && (overwrite == true || entry.ID == "00" || entry.ID == "86" || entry.ID == null))
                {
                    var randomEnemy = getRandomSetEncounter(validSetEncounters);
                    entry.Weight = "03";
                    entry.ID = randomEnemy.ID;
                    if (balance)
                    {
                        foreach (string id in randomEnemy.Contents)
                        {
                            Monster mon = getMonsterById(id, customMonsterFile);
                            if (mon.AdjustedLevel == -1)
                            {
                                balanceMonsterToTable(mon, table);
                            }
                        }
                    }
                }
            }

            // chance of adding double boss
            if (table.Edit > 3 && table.Edit != 10 && table.Edit != 5 && check_monsters_boss_allowMultiple.IsChecked == true)
            {
                var boss = setEncountList[table.SetEncounters[0].ID];
                if (boss.Count < 10 && boss.Groups < 4 && rand.Next(0, 2) == 1)
                {
                    var validDoubleEncounters = getValidDoubleEncounters(validSetEncounters, boss);
                    if (validDoubleEncounters.Count > 0)
                    {
                        var randomEnemy = getRandomSetEncounter(validDoubleEncounters);
                        table.SetEncounters[1].Weight = "03";
                        table.SetEncounters[1].ID = randomEnemy.ID;
                        if (balance)
                        {
                            foreach (string id in randomEnemy.Contents)
                            {
                                Monster mon = getMonsterById(id, customMonsterFile);
                                if (mon.PowerLevel == -1)
                                {
                                    balanceMonsterToTable(mon, table);
                                }
                            }
                        }
                    }
                }
            }
        }
        private List<SetEncounter> getValidDoubleEncounters(List<SetEncounter> setEncounters, SetEncounter boss)
        {
            return setEncounters.FindAll(e => doubleEncounterIsValid(e, boss));
        }
        private bool doubleEncounterIsValid(SetEncounter e, SetEncounter boss)
        {
            return e.Count <= (10 - boss.Count) && e.Groups <= (4 - boss.Groups) && e.Spawn != 8;
        }
        private bool encounterIsValid(object enc, EncountTable table, bool gospel = false, bool balance = false)
        {
            if (enc == null)
                return false;

            var edit = table.Edit;

            var e = enc as Encounter;
            var se = enc as SetEncounter;
            var g = enc as GospelEntry;
            var e1 = enc as EncountTableEntry;
            var se1 = enc as EncountTableSetEntry;
            var m = enc as Monster;

            if (m != null)
            {
                e = findEncounterByMonster(m.ID);
            }

            if (e1 != null)
            {
                e = getEncounterById(e1.ID);
            } else if (se1 != null)
            {
                se = getSetEncounterById(se1.ID);
            }

            if (g != null)
            {
                bool result = g.Monsters.Any(f => encounterIsValid(findEncounterByMonster(f), table, gospel, balance)) ||
                    (gospel && getGospelSetEncounters(g).Any(f => encounterIsValid(f, table, gospel, balance)));

                return result;
            }

            int spawn = 0;
            bool hasModel = false;
            if (e != null)
            {
                hasModel = e.HasOverworldModel || edit == 4 || edit > 5;
                spawn = e.Spawn;
                if (((edit >= 1 && edit <= 3) || edit == 10) == false)
                    return false;
            } else if (se != null)
            {
                hasModel = se.HasOverworldModel || edit == 4 || edit > 5;
                spawn = se.Spawn;
            }

            double power_level = -1;
            if (e != null && balance)
            {
                Monster mon = getMonsterByEncounterID(e.ID, customMonsterFile);
                if (mon.DoNotAverage)
                    power_level = -1;
                else
                    power_level = mon.AdjustedLevel;
            } else if (se != null && balance)
            {
                List<double> mons = new List<double>();
                foreach (string id in se.Contents)
                {
                    Monster mon = getMonsterById(id, customMonsterFile);
                    if (mon.AdjustedLevel > -1 && !mon.DoNotAverage)
                        mons.Add(mon.AdjustedLevel);
                }
                if (mons.Count > 0)
                    power_level = mons.Average();
                else
                    power_level = -1;
            }

            bool result1 = hasModel;
            bool result2 = (spawn >= 1 && spawn <= 3) ||
                (spawn == 4 && check_monsters_mixInInfamous.IsChecked == true) ||
                (spawn == 5 && (check_monsters_includeArena.IsChecked == true ||
                    edit == 4)) ||
                (spawn == 6 && (check_monsters_includePostgame.IsChecked == true ||
                    edit == 8 || edit == 7)) ||
                (spawn == 7 && (check_monsters_includeMemoriam.IsChecked == true ||
                    edit == 8)) ||
                (spawn == 8);
            bool result3 = (edit != 2 && edit != 11) || gospel != true;
            //bool result4 = gospel != true || tableHasRoom(table, enc);
            bool result5 = balance != true || power_level == -1 || (power_level >= table.PowerLevel - balance_variance && power_level <= table.PowerLevel + balance_variance);

            return result1
                && result2
                && result3
                //&& result4
                && result5
                ;
        }
        private bool tableContentsAreValid(EncountTable table, EncountTable target, bool gospel = false)
        {
            return table.Contents.All(e =>
            {
                bool x = e.ID == "0000" || encounterIsValid(getEncounterById(e.ID), target, gospel);
                return x;
            }) && table.SetEncounters.All(e => {
                bool x = e.ID == "00" || e.ID == "86" || encounterIsValid(getSetEncounterById(e.ID), target, gospel);
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
            var ep = enc as Encounter;
            var sep = enc as SetEncounter;

            if (ep != null)
            {
                e = new EncountTableEntry();
                e.ID = ep.ID;
                e.Arg1 = "03";
                e.Arg2 = "03";
                e.Footer = "0000803F";
            } else if (sep != null)
            {
                se = new EncountTableSetEntry();
                se.ID = sep.ID;
                se.Weight = "07";
            }

            if (e != null && encounterIsValid(getEncounterById(e.ID), table))
                return addValueToArray(table.Contents, e);
            else if (se != null && encounterIsValid(getSetEncounterById(se.ID), table))
                return addValueToArray(table.SetEncounters, se);
            else
                return false;
        }
        private bool addValueToArray(object[] arr, object add)
        {
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

            if (e != null || e1 != null)
            {
                return table.Contents.Any(x => x == null) && encounterIsValid(e ?? getEncounterById(e1.ID), table);
            } else if (se != null || se1 != null)
            {
                se = se ?? getSetEncounterById(se1.ID);
                return tableHasRoomForSetEncounter(table, se) && encounterIsValid(se, table);
            } else if (g != null)
            {
                return g.Monsters.Any(mon => tableHasRoom(table, findEncounterByMonster(mon))) ||
                    getGospelSetEncounters(g).Any(mon => tableHasRoom(table, mon));
            }

            return false;
        }
        private bool tableHasRoomForSetEncounter(EncountTable table, SetEncounter e)
        {
            if ((table.Edit >= 1 && table.Edit <= 3) || table.Edit == 10)
                return table.SetEncounters.Any(x => x == null);
            else
                return table.SetEncounters[0] == null ||
                    (check_monsters_boss_allowMultiple.IsChecked == true && table.SetEncounters[1] == null && doubleEncounterIsValid(getSetEncounterById(table.SetEncounters[0].ID), e));
        }
        private int randomizeStat(int stat, double min, double max, bool? condition, string flags)
        {
            // flags: "Z" = non-zero, "M" = respect 255
            if (condition != true)
                return stat;
            else
            {
                if (flags.Contains("M") && stat == 255)
                    return stat;

                var xValue = getRandomDoubleBetweenTwoValues(rand, min, max);

                var result = stat * xValue / 100;
                result = result > 65535 ? 65535 : result;
                if (flags.Contains("Z"))
                    result = result < 1 ? 1 : result;
                else
                    result = result < 0 ? 0 : result;

                if (flags.Contains("M") && (int)result == 255)
                {
                    if (result >= 255)
                        return 256;
                    else
                        return 254;
                }

                return (int)result;
            }
        }
        private int randomizeStat(int stat, double min, double max, System.Windows.Controls.CheckBox condition, string flags)
        {
            return randomizeStat(stat, min, max, condition.IsChecked, flags);
        }
        private int randomizeStat(int stat, double min, double max, System.Windows.Controls.CheckBox condition)
        {
            return randomizeStat(stat, min, max, condition.IsChecked, "");
        }
        private int randomizeStat(int stat, double min, double max, bool? condition)
        {
            return randomizeStat(stat, min, max, condition, "");
        }
        private int randomizeStat(int stat, double min, double max)
        {
            return randomizeStat(stat, min, max, true, "");
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
        private bool trapChestExists(TreasureBoxFile data, string id)
        {
            return data.Contents.Any(e => e.Content == "FFFD" && e.Value == id);
        }
        private void addTrapChests(List<EncountTable> tgt)
        {
            // now add the existing chests
            loadTreasureFile();
            var treasureFile = customTreasureBoxFile != null ? customTreasureBoxFile : tbTreasureBox;

            Dictionary<string, string> trapChestIDs = new Dictionary<string, string>{
                {"003D" , "01D1"},
                {"0072" , "01D2"},
                {"00DE" , "01D3"},
                {"012F" , "0174"} };
            foreach (KeyValuePair<string, string> kv in trapChestIDs)
            {
                if (trapChestExists(treasureFile, kv.Key))
                    tgt.Add(getEncountTableByID(customEncountFile, kv.Value));
            }
        }
        private void flushEncounterTables(List<EncountTable> l)
        {
            foreach (EncountTable table in l)
            {
                flushEncounterTable(table);
            }
        }
        private void flushEncounterTable(EncountTable table)
        {
            table.Contents = new EncountTableEntry[10];
            table.SetEncounters = new EncountTableSetEntry[2];
        }
        private bool gospelIsSatisfied(List<EncountTable> l, List<GospelEntry> g_list = null)
        {
            g_list = g_list ?? gospelList;

            foreach (EncountTable table in l.FindAll(t => (t.Parent == null || t.Parent == "") && t.Edit != 1 && t.Edit != 2 && t.Edit != 11))
            {
                gospelCheckProcessTable(table, g_list);
            }

            //var console_output = g_list.FindAll(gosp => gosp.Required && !gosp.Successful);
            //Console.WriteLine(console_output);

            return g_list.All(gosp => gosp.Successful || !gosp.Required);
        }
        private bool gospelIsSatisfied(EncountFile f, List<GospelEntry> g_list = null)
        {
            return gospelIsSatisfied(new List<EncountTable>(f.Contents.Values), g_list);
        }
        private bool tryAddGospel(EncountTable table, GospelEntry g, out SetEncounter outvar, bool balance = false)
        {
            List<Encounter> encounters = new List<Encounter>(g.Monsters.Select(mon => findEncounterByMonster(mon)));
            List<SetEncounter> setEncounters = getGospelSetEncounters(g);

            List<object> fullList = new List<object>(encounters);
            fullList.AddRange(setEncounters);

            fullList = fullList.FindAll(enc => tableHasRoom(table, enc) && encounterIsValid(enc, table, true, balance));

            var randomEnc = fullList[rand.Next(fullList.Count)];

            bool succ = tryAddEncounter(table, randomEnc);
            SetEncounter se = randomEnc as SetEncounter;

            outvar = succ ? se : null;

            if (balance)
            {
                Encounter e = randomEnc as Encounter;
                if (e != null)
                {
                    balanceMonsterToTable(getMonsterByEncounterID(e.ID, customMonsterFile), table);
                }
                else if (se != null)
                {
                    foreach (string id in se.Contents)
                    {
                        balanceMonsterToTable(getMonsterById(id, customMonsterFile), table);
                    }
                }
            }

            return succ;
        }
        private void resetGospel(List<GospelEntry> g_list = null)
        {
            g_list = g_list ?? gospelList;

            foreach (GospelEntry gospel in g_list)
            {
                gospel.Successful = false;
            }
        }
        private bool doGospelRandom(List<EncountTable> l, bool balance = false, bool flush = true)
        {
            if (l.Count > 0)
            {
                // first, flush all of the encounter tables
                if (flush)
                    flushEncounterTables(l);

                gospelIsSatisfied(customEncountFile);

                // next, assign each gospel monster to a random table
                foreach (GospelEntry g in gospelList)
                {
                    if (g.Required && !g.Successful)
                    {
                        var validTables = l.FindAll(table => tableHasRoom(table, g) && encounterIsValid(g, table, true, balance));

                        if (validTables.Count > 0)
                        {
                            var randomTable = validTables[rand.Next(validTables.Count)];

                            SetEncounter se;
                            g.Successful = tryAddGospel(randomTable, g, out se, balance);

                            if (se != null && se.Contents.Length > 1)
                            {
                                foreach (string mon in se.Contents)
                                {
                                    GospelEntry otherEntry = getGospelByMonsterID(mon);
                                    if (otherEntry != null)
                                        otherEntry.Successful = true;
                                }
                            }
                        }
                    }
                }

                if (gospelIsSatisfied(customEncountFile))
                {
                    resetGospel();
                    // now, pad out the tables with random monsters
                    foreach (EncountTable table in l)
                    {
                        randomizeEncountTable(table, false, balance);
                    }
                }
                else
                {
                    resetGospel();
                    if (!balance)
                    {
                        doGospelRandom(l, balance);
                    }
                    else
                        return false;
                }
            }

            return true;
        }
        private GospelEntry getGospelByEncounterID(string id)
        {
            Monster mon = getMonsterByEncounterID(id);

            if (mon != null)
                return getGospelByMonsterID(mon.ID);

            return null;
        }
        private List<GospelEntry> getGospelBySetEncounterID(string id)
        {
            List<GospelEntry> returnList = new List<GospelEntry>();

            SetEncounter se = getSetEncounterById(id);

            foreach (string monsterID in se.Contents)
            {
                GospelEntry g = getGospelByMonsterID(monsterID);
                if (g != null)
                    returnList.Add(g);
            }

            return returnList;
        }
        private void gospelCheckProcessTable(EncountTable table, List<GospelEntry> g_list)
        {
            g_list = g_list ?? gospelList;

            foreach (EncountTableEntry entry in table.Contents)
            {
                if (entry != null && entry.ID != null)
                {
                    Monster mon = getMonsterByEncounterID(entry.ID);
                    GospelEntry gospel = mon != null ? getGospelByMonsterID(mon.ID, g_list) : null;
                    if (gospel != null)
                        gospel.Successful = true;
                }
            }
            foreach (EncountTableSetEntry entry in table.SetEncounters)
            {
                if (entry != null && entry.ID != null)
                {
                    SetEncounter se = getSetEncounterById(entry.ID) ?? new SetEncounter { Contents = new string[0] };
                    foreach (string id in se.Contents)
                    {
                        Monster mon = getMonsterById(id);
                        GospelEntry gospel = mon != null ? getGospelByMonsterID(mon.ID, g_list) : null;
                        if (gospel != null)
                            gospel.Successful = true;
                    }
                }
            }
        }
        private void shuffleEncounterTables(List<EncountTable> shuffleList)
        {
            // make sure all the tables' shuffle values are set to false
            foreach (EncountTable t in shuffleList)
            {
                t.Shuffled = false;
            }
            // make a backup copy of the tables to draw from
            List<EncountTable> copies = new List<EncountTable>(shuffleList.Select(table => table.Clone()));

            // flush the tables, then check for gospels
            flushEncounterTables(shuffleList);

            //create the source-to-destination dictionary
            Dictionary<EncountTable, EncountTable> swapDic = new Dictionary<EncountTable, EncountTable>();

            if (check_monsters_gospel.IsChecked == true)
            // create a list of gospel entries that need to be accounted for
            {
                resetGospel();
                gospelIsSatisfied(customEncountFile);

                Dictionary<GospelEntry, List<EncountTable>> shuffleGospel = new Dictionary<GospelEntry, List<EncountTable>>();
                foreach (EncountTable table in copies)
                {
                    foreach (EncountTableEntry entry in table.Contents)
                    {
                        GospelEntry g = getGospelByEncounterID(entry.ID);
                        if (g != null && shuffleGospel.ContainsKey(g) != true && g.Required && !g.Successful)
                        {
                            List<EncountTable> newList = new List<EncountTable>();
                            newList.Add(table);
                            shuffleGospel.Add(g, newList);
                        }
                        else if (g != null && shuffleGospel.ContainsKey(g))
                        {
                            if (shuffleGospel[g].Contains(table) != true)
                                shuffleGospel[g].Add(table);
                        }
                    }
                    foreach (EncountTableSetEntry entry in table.SetEncounters)
                    {
                        List<GospelEntry> gList = getGospelBySetEncounterID(entry.ID);
                        foreach (GospelEntry g in gList)
                        {
                            if (g != null && shuffleGospel.ContainsKey(g) != true && g.Required && !g.Successful)
                            {
                                List<EncountTable> newList = new List<EncountTable>();
                                newList.Add(table);
                                shuffleGospel.Add(g, newList);
                            }
                            else if (g != null && shuffleGospel.ContainsKey(g))
                            {
                                if (shuffleGospel[g].Contains(table) != true)
                                    shuffleGospel[g].Add(table);
                            }
                        }
                    }
                }
                
                while (shuffleGospel.Keys.Any(gospel => !gospel.Successful))
                // first shuffle: put the gospel tables in their place
                {
                    foreach (KeyValuePair<GospelEntry, List<EncountTable>> kv in shuffleGospel)
                    {
                        if (!kv.Key.Successful) // if it's already successful, do nothing
                        {
                            if (!kv.Value.Any(table => table.Shuffled)) // if it has a table that's successful, skip to the end and...
                            {
                                placeShuffleTable(kv.Value, shuffleList, swapDic, true);

                                /*List<EncountTable> validTables = kv.Value.FindAll(t => !t.Shuffled);
                                if (validTables.Count > 0) // if there are valid source tables, proceed to next step
                                {
                                    EncountTable srcTable = validTables[rand.Next(validTables.Count)];
                                    selectAndPlaceShuffleTable(srcTable, shuffleList, true);
                                }
                                else // if there are no valid tables, do this instead
                                {
                                    EncountTable srcTable = kv.Value[rand.Next(kv.Value.Count)];
                                    selectAndBounceGospelShuffleTable(srcTable, shuffleList, copies);
                                }*/

                                // TODO: write code to reset the gospel set if there's a failure
                            }
                            kv.Key.Successful = true; // set the value to successful
                        }
                    }
                }
            }

            //default protocol
            // go through each table and swap them with tables that fit
            foreach (EncountTable table in copies)
            {
                if (!table.Shuffled)
                    placeShuffleTable(table, shuffleList, swapDic);
            }

            // now, take our swap dictionary and apply our changes
            foreach (KeyValuePair<EncountTable,EncountTable> kv in swapDic)
            {
                kv.Value.Contents = kv.Key.Contents;
                kv.Value.SetEncounters = kv.Key.SetEncounters;
            }

            /*List<EncountTable> copies = new List<EncountTable>(shuffleList.Select(table => table.Clone()));
            // create a catalogue of "occupied" tables
            Dictionary<EncountTable, bool> occupied = new Dictionary<EncountTable, bool>();
            foreach (EncountTable table in shuffleList)
                occupied.Add(table, false);
            // create a catalogue of "used" tables
            Dictionary<EncountTable, bool> shuffled = new Dictionary<EncountTable, bool>();
            foreach (EncountTable table in copies)
                shuffled.Add(table, false);

            flushEncounterTables(shuffleList);

            // start by preparing a gospel list
            Dictionary<GospelEntry, List<EncountTable>> shuffleGospel = new Dictionary<GospelEntry, List<EncountTable>>();
            foreach (EncountTable table in copies)
            {
                foreach (EncountTableEntry entry in table.Contents)
                {
                    GospelEntry g = getGospelByEncounterID(entry.ID);
                    if (g != null && shuffleGospel.ContainsKey(g) != true)
                    {
                        List<EncountTable> newList = new List<EncountTable>();
                        newList.Add(table);
                        shuffleGospel.Add(g, newList);
                    }
                    else if (g != null)
                    {
                        if (shuffleGospel[g].Contains(table) != true)
                            shuffleGospel[g].Add(table);
                    }
                }
                foreach (EncountTableSetEntry entry in table.SetEncounters)
                {
                    List<GospelEntry> gList = getGospelBySetEncounterID(entry.ID);
                    foreach (GospelEntry g in gList)
                    {
                        if (g != null && shuffleGospel.ContainsKey(g) != true)
                        {
                            List<EncountTable> newList = new List<EncountTable>();
                            newList.Add(table);
                            shuffleGospel.Add(g, newList);
                        }
                        else if (g != null)
                        {
                            if (shuffleGospel[g].Contains(table) != true)
                                shuffleGospel[g].Add(table);
                        }
                    }
                }
            }

            //reset and test if these gospel entries are satisfied
            resetGospel(new List<GospelEntry>(shuffleGospel.Keys));
            gospelIsSatisfied(customEncountFile, new List<GospelEntry>(shuffleGospel.Keys));

            // start placing gospel tables randomly
            foreach (KeyValuePair<GospelEntry, List<EncountTable>> kv in shuffleGospel)
            {
                placeGospelTable(kv, shuffleList, shuffled, occupied, shuffleGospel, copies);
            }

            // place the rest of the tables
            foreach (EncountTable table in copies)
            {
                placeShuffleTable(table, shuffleList, shuffled, occupied, shuffleGospel, copies);
            }*/
        }
        private EncountTable selectAndSwapTable(EncountTable table, List<EncountTable> shuffleList, bool gospel = false)
        {
            List<EncountTable> candidates = new List<EncountTable>();
            foreach (EncountTable potential in shuffleList)
            {
                if (table.Edit == potential.Edit || (tableContentsAreValid(table, potential, gospel) && tableContentsAreValid(potential, table)))
                    candidates.Add(potential);
            }

            EncountTable target = candidates[rand.Next(candidates.Count)];

            swapTables(table, target);

            return target;
        }
        private void swapTables(EncountTable table1, EncountTable table2)
        {
            EncountTableEntry[] t1c = table1.Contents;
            EncountTableSetEntry[] t1se = table1.SetEncounters;

            table1.Contents = table2.Contents;
            table1.SetEncounters = table2.SetEncounters;

            table2.Contents = t1c;
            table2.SetEncounters = t1se;
        }
        private EncountTable selectAndPlaceShuffleTable(EncountTable srcTable, List<EncountTable> targets, bool gospel = false, bool ignoreShuffle = false)
        {
            List<EncountTable> candidates = new List<EncountTable>();
            foreach (EncountTable potential in targets)
            {
                if ((srcTable.Edit == potential.Edit || tableContentsAreValid(srcTable, potential, gospel)) && ((!potential.Shuffled) || ignoreShuffle))
                    candidates.Add(potential);
            }

            EncountTable target = candidates[rand.Next(candidates.Count)];

            //placeShuffleTable(srcTable, target);

            return target;
        }
        /*private void placeShuffleTable(EncountTable srcTable, EncountTable target)
        {
            target.Contents = srcTable.Contents;
            target.SetEncounters = srcTable.SetEncounters;
            srcTable.Shuffled = true;
            target.Shuffled = true;
        }*/
        private EncountTable selectAndBounceGospelShuffleTable(EncountTable srcTable, List<EncountTable> targets, List<EncountTable> copies)
        {
            EncountTable target = selectAndPlaceShuffleTable(srcTable, targets, true, true);

            // process of "bouncing back" the table
            

            return target;
        }

        //new 1/18/2022
        private bool placeShuffleTable(List<EncountTable> sources, List<EncountTable> targets, Dictionary<EncountTable,EncountTable> swapDic, bool gospel = false)
        {
            // keep track of what failed, we'll need this later
            bool srcFailed = false;
            bool tgtFailed = false;

            // gather our source candidates
            List<EncountTable> srcCandidates = sources.FindAll(t => !t.Shuffled);
            if (srcCandidates.Count <= 0)
            // expand scope to all candidates if there are none available
            {
                srcCandidates = sources;
                srcFailed = true;
            }

            //shuffle our candidates (we will use the rest as backup if necessary)
            srcCandidates = new List<EncountTable>(srcCandidates.OrderBy(r => rand.Next()));

            EncountTable target = null;
            EncountTable source = null;
            foreach (EncountTable src in srcCandidates)
            {
                // gather our target candidates
                List<EncountTable> tgtCandidates = targets.FindAll(t => (!gospel && src.Edit == t.Edit) || tableContentsAreValid(src, t, gospel));// we can't use the Edit comparison if Gospel is true because we need to make sure the table is viable for gospel inclusion

                if (tgtCandidates.Count <= 0)
                    continue; // skip to next source if there are no available targets

                if (tgtCandidates.Any(t => !t.Shuffled))
                    tgtCandidates = new List<EncountTable>(tgtCandidates.FindAll(t => !t.Shuffled));
                else
                    tgtFailed = true;// we could skip to the next source here, but it would fail some of the time. maybe work on this?

                //now we select a target and break
                target = tgtCandidates[rand.Next(tgtCandidates.Count)];
                source = src;
                break;
            }

            if (target != null && source != null)
            {
                if (srcFailed)
                {
                    swapDic[source].Shuffled = false;
                    swapDic.Remove(source);
                }
                if (tgtFailed)
                {
                    KeyValuePair<EncountTable, EncountTable> tgtKvp = new List<KeyValuePair<EncountTable, EncountTable>>(swapDic).Find(kv => kv.Value == target);
                    tgtKvp.Key.Shuffled = false;
                    swapDic.Remove(tgtKvp.Key);
                }

                swapDic.Add(source, target);
                source.Shuffled = true;
                target.Shuffled = true;
            }
            else
                Console.WriteLine("fucked up");

            return srcFailed == false && tgtFailed == false;
            
        }
        private void placeShuffleTable(EncountTable source, List<EncountTable> targets, Dictionary<EncountTable, EncountTable> swapDic, bool gospel = false)
        {
            List<EncountTable> sources = new List<EncountTable>();
            sources.Add(source);

            placeShuffleTable(sources, targets, swapDic, gospel);
        }

        /*private bool tableHasGospel(EncountTable table, GospelEntry gospel)
        {
            return table.Contents.Any(entry =>
            {
                return getGospelByEncounterID(entry.ID) == gospel;
            }) || table.SetEncounters.Any(entry =>
            {
                return getGospelSetEncounters(gospel).Any(ggse => ggse.ID == entry.ID);
            });
        }
        private void placeShuffleTable(EncountTable table, List<EncountTable> shuffleList, Dictionary<EncountTable, bool> shuffled, Dictionary<EncountTable, bool> occupied, Dictionary<GospelEntry, List<EncountTable>> shuffleGospel, List<EncountTable> copies, bool gospel = false)
        {
            if (shuffled[table] != true)
            {
                var validTables = shuffleList.FindAll(t => tableContentsAreValid(table, t, gospel) && occupied[t] != true);

                if (validTables.Count > 0)
                {
                    placeTableContents(table, validTables, shuffled, occupied, shuffleGospel);
                }
                else
                {
                    // do the same thing as done with the gospel tables
                    validTables = shuffleList.FindAll(t => tableContentsAreValid(table, t, gospel));

                    displaceTableContents(table, validTables, shuffled, occupied, shuffleGospel, shuffleList, copies);
                }
            }
        }
        private void placeGospelTable(KeyValuePair<GospelEntry, List<EncountTable>> kv, List<EncountTable> shuffleList, Dictionary<EncountTable, bool> shuffled, Dictionary<EncountTable, bool> occupied, Dictionary<GospelEntry, List<EncountTable>> shuffleGospel, List<EncountTable> copies)
        {
            GospelEntry gospel = kv.Key;
            if (gospel.Required && !gospel.Successful)
            {
                var openTables = kv.Value.FindAll(t => shuffled[t] != true);

                if (openTables.Count > 0)
                {
                    EncountTable sourceTable = openTables[rand.Next(openTables.Count)];
                    var validTables = shuffleList.FindAll(t => tableContentsAreValid(sourceTable, t, check_monsters_gospel.IsChecked == true) && occupied[t] != true);

                    if (validTables.Count > 0)
                    {
                        placeTableContents(sourceTable, validTables, shuffled, occupied, shuffleGospel);
                    }
                    else
                    {
                        // forcefully replace an already placed table, then relocate the displaced table
                        validTables = shuffleList.FindAll(t => tableContentsAreValid(sourceTable, t, check_monsters_gospel.IsChecked == true));

                        displaceTableContents(sourceTable, validTables, shuffled, occupied, shuffleGospel, shuffleList, copies);
                    }
                }
            }
        }
        private void placeTableContents(EncountTable sourceTable, List<EncountTable> validTables, Dictionary<EncountTable, bool> shuffled, Dictionary<EncountTable, bool> occupied, Dictionary<GospelEntry, List<EncountTable>> shuffleGospel)
        {
            EncountTable targetTable = validTables[rand.Next(validTables.Count)];
            targetTable.Contents = sourceTable.Contents;
            targetTable.SetEncounters = sourceTable.SetEncounters;
            shuffled[sourceTable] = true;
            occupied[targetTable] = true;
            
            gospelCheckProcessTable(targetTable, new List<GospelEntry>(shuffleGospel.Keys));
        }
        private void displaceTableContents(EncountTable sourceTable, List<EncountTable> validTables, Dictionary<EncountTable, bool> shuffled, Dictionary<EncountTable, bool> occupied, Dictionary<GospelEntry, List<EncountTable>> shuffleGospel, List<EncountTable> shuffleList, List<EncountTable> copies)
        {
            if (validTables.Count > 0)
            {
                EncountTable targetTable = validTables[rand.Next(validTables.Count)];
                KeyValuePair<GospelEntry, List<EncountTable>> gospelRedo = new List<KeyValuePair<GospelEntry, List<EncountTable>>>(shuffleGospel).Find(gl => gl.Value.Any(table => table.Contents == targetTable.Contents));
                EncountTable dispTable = copies.Find(c => c.Contents == targetTable.Contents);

                placeTableContents(sourceTable, new List<EncountTable> { targetTable }, shuffled, occupied, shuffleGospel);

                resetGospel(new List<GospelEntry>(shuffleGospel.Keys));
                if (gospelRedo.Key != null)
                {
                    gospelRedo.Key.Successful = false;
                    gospelIsSatisfied(customEncountFile, new List<GospelEntry>(shuffleGospel.Keys));
                    placeShuffleTable(dispTable, shuffleList, shuffled, occupied, shuffleGospel, copies, gospelRedo.Key.Required && check_monsters_gospel.IsChecked == true && !gospelRedo.Key.Successful);
                    gospelRedo.Key.Successful = true;
                } else if (dispTable != null)
                {
                    gospelIsSatisfied(customEncountFile, new List<GospelEntry>(shuffleGospel.Keys));
                    placeShuffleTable(dispTable, shuffleList, shuffled, occupied, shuffleGospel, copies);
                }
            }
        }
        private void placeShuffleEntry(object entry, List<EncountTable> monsterShuffleTables, Dictionary<GospelEntry, List<object>> gospel_dic, bool gospel = false)
        {
            List<EncountTable> valid_encount_tables = monsterShuffleTables.FindAll(table => encounterIsValid(entry, table, gospel, check_monsters_stats_balance.IsChecked == true) && tableHasRoom(table, entry));

            if (valid_encount_tables.Count > 0)
            {
                EncountTable random_table = valid_encount_tables[rand.Next(valid_encount_tables.Count)];
                tryAddEncounter(random_table, entry);
            }
            else
            {
                valid_encount_tables = monsterShuffleTables.FindAll(table => encounterIsValid(entry, table, gospel, check_monsters_stats_balance.IsChecked == true));
                EncountTable random_table = valid_encount_tables[rand.Next(valid_encount_tables.Count)];
                displaceTableEntry(entry, random_table, gospel_dic, monsterShuffleTables);
            }
        }
        private void placeGospelShuffleEntry(KeyValuePair<GospelEntry, List<object>> kv, List<object> combined_pool, List<EncountTable> monsterShuffleTables, Dictionary<GospelEntry, List<object>> gospel_dic)
        {
            if (kv.Key.Required && !kv.Key.Successful)
            {
                List<object> valid_entries = kv.Value.FindAll(entry => combined_pool.Contains(entry));
                object random_entry = valid_entries[rand.Next(valid_entries.Count)];

                List<EncountTable> valid_encount_tables = monsterShuffleTables.FindAll(table => encounterIsValid(random_entry, table, check_monsters_gospel.IsChecked == true, check_monsters_stats_balance.IsChecked == true) && tableHasRoom(table, random_entry));

                if (valid_encount_tables.Count > 0)
                {
                    EncountTable random_table = valid_encount_tables[rand.Next(valid_encount_tables.Count)];
                    tryAddEncounter(random_table, random_entry);
                    combined_pool.Remove(random_entry);
                    kv.Key.Successful = true;
                    EncountTableSetEntry se = random_entry as EncountTableSetEntry;
                    if (se != null)
                    {
                        foreach (GospelEntry g in getGospelBySetEncounterID(se.ID))
                        {
                            if (g != null)
                                g.Successful = true;
                        }
                    }
                }
                else
                {
                    valid_encount_tables = monsterShuffleTables.FindAll(table => encounterIsValid(random_entry, table, check_monsters_gospel.IsChecked == true, check_monsters_stats_balance.IsChecked == true));
                    EncountTable random_table = valid_encount_tables[rand.Next(valid_encount_tables.Count)];
                    displaceTableEntry(random_entry, random_table, gospel_dic, monsterShuffleTables);

                    combined_pool.Remove(random_entry);
                    kv.Key.Successful = true;
                    EncountTableSetEntry se = random_entry as EncountTableSetEntry;
                    if (se != null)
                    {
                        foreach (GospelEntry g in getGospelBySetEncounterID(se.ID))
                        {
                            if (g != null)
                                g.Successful = true;
                        }
                    }
                }
            }
        }
        private void displaceTableEntry(object entry, EncountTable table, Dictionary<GospelEntry, List<object>> gospel_dic, List<EncountTable> monsterShuffleTables)
        {
            EncountTableEntry e = entry as EncountTableEntry;
            EncountTableSetEntry se = entry as EncountTableSetEntry;

            if (e != null)
            {
                int rand_index = rand.Next(10);
                EncountTableEntry displaced = table.Contents[rand_index];
                table.Contents[rand_index] = table.Contents[9];
                table.Contents[9] = null;
                tryAddEncounter(table, e);
                replaceTableEntry(displaced, gospel_dic, monsterShuffleTables);
            } else if (se != null)
            {
                int rand_index = rand.Next(2);
                EncountTableSetEntry displaced = table.SetEncounters[rand_index];
                table.SetEncounters[rand_index] = table.SetEncounters[2];
                table.SetEncounters[2] = null;
                tryAddEncounter(table, se);
                replaceTableEntry(displaced, gospel_dic, monsterShuffleTables);
            }
        }
        private void replaceTableEntry(object displaced, Dictionary<GospelEntry, List<object>> gospel_dic, List<EncountTable> monsterShuffleTables)
        {
            resetGospel(new List<GospelEntry>(gospel_dic.Keys));
            gospelIsSatisfied(customEncountFile, new List<GospelEntry>(gospel_dic.Keys));

            List<KeyValuePair<GospelEntry, List<object>>> ge = new List<KeyValuePair<GospelEntry, List<object>>>(gospel_dic).FindAll(kv => kv.Value.Contains(displaced));

            bool gospel = ge.Any(g => g.Key.Required && !g.Key.Successful) && check_monsters_gospel.IsChecked == true;

            placeShuffleEntry(displaced, monsterShuffleTables, gospel_dic, gospel);

            foreach (KeyValuePair<GospelEntry, List<object>> kv in ge)
            {
                kv.Key.Successful = true;
            }
        }*/
        private double balanceMonsterToTable(Monster mon, EncountTable table)
        {
            return balanceMonsterToLevel(mon, table.PowerLevel);
        }
        private double balanceMonsterToLevel(Monster mon, double power_level)
        {
            if (mon.AdjustedLevel == -1 && !mon.DoNotAverage)
            {
                double lowest_level = power_level - balance_variance;
                lowest_level = lowest_level > 0 ? lowest_level : 0;
                double highest_level = power_level + balance_variance;

                double new_power_level;

                if (rand.NextDouble() > 0.5)
                {
                    new_power_level = getRandomDoubleBetweenTwoValues(rand, power_level, highest_level);
                }
                else
                {
                    new_power_level = getRandomDoubleBetweenTwoValues(rand, lowest_level, power_level);
                }

                mon.AdjustedLevel = new_power_level;
            }

            return mon.AdjustedLevel;
        }
        private double randomizeMonsterPowerLevel(EncountTable table, double cumulative_highest, List<object> masterList, List<object> shuffleEncList = null/*, List<object> edited_monsters, List<double> power_range*/)
        {
            // get a single encounter from the list
            object sel_monster = masterList[rand.Next(masterList.Count)];
            //edited_monsters.Add(sel_monster);
            Encounter e = sel_monster as Encounter;
            SetEncounter se = sel_monster as SetEncounter;
            EncountTableEntry ete = sel_monster as EncountTableEntry;
            EncountTableSetEntry etse = sel_monster as EncountTableSetEntry;

            if (ete != null)
                e = getEncounterById(ete.ID);
            else if (etse != null)
                se = getSetEncounterById(etse.ID);

            List<Monster> monsters_to_change = new List<Monster>();

            // figure out what type of encounter it is and add its monsters to a temporary list
            if (e != null)
            {
                monsters_to_change.Add(getMonsterByEncounterID(e.ID, customMonsterFile));
            } else if (se != null)
            {
                foreach (string id in se.Contents)
                {
                    Monster mon = getMonsterById(id, customMonsterFile);
                    if (monsters_to_change.Contains(mon) != true)
                    {
                        monsters_to_change.Add(mon);
                    }
                }
            }

            tryAddEncounter(table, sel_monster);
            if (shuffleEncList != null)
                shuffleEncList.Remove(sel_monster);

            // prepare to get the new power level
            List<double> new_highest = new List<double>();

            // we have either a monster or a set of monsters, so let's adjust their power level to a reasonable amount
            foreach (Monster mon in monsters_to_change)
            {
                double new_power_level = balanceMonsterToLevel(mon, table.PowerLevel);

                new_highest.Add(new_power_level);
            }

            return new_highest.Average();
        }
        private List<object> removeAllAdjustedMonsters (List<object> enc_list)
        {
            enc_list = enc_list.FindAll(enc =>
            {
                Encounter e = enc as Encounter;
                SetEncounter se = enc as SetEncounter;
                EncountTableEntry ete = enc as EncountTableEntry;
                EncountTableSetEntry etse = enc as EncountTableSetEntry;

                if (ete != null)
                    e = getEncounterById(ete.ID);
                else if (etse != null)
                    se = getSetEncounterById(etse.ID);

                if (e != null)
                {
                    return getMonsterById(e.Monster, customMonsterFile).AdjustedLevel == -1;
                } else if (se != null)
                {
                    return se.Contents.All(id => getMonsterById(id).AdjustedLevel == -1);
                }
                
                return false;
            });

            return enc_list;
        }
        private void randomizeEncountersBalanced(List<EncountTable> randomTables, MonsterFile mons)
        {
            // flush the encounter tables
            flushEncounterTables(randomTables);

            // create a list of each "type" of table, we'll make balanced monsters for all of them
            List<int> edit_range = new List<int>(Enumerable.Range(1, 11));

            // now, create a list of all possible encounters and set encounters
            List<Encounter> available_encounters = new List<Encounter>();
            foreach (Encounter enc in encountList.Values)
            {
                if (available_encounters.All(e => e.Monster != enc.Monster) && getMonsterById(enc.Monster, mons).DoNotAverage != true)
                    available_encounters.Add(enc);
            }
            // concatenate the encounter list with the set encounter list
            List<object> concat_available_encounters = new List<object>(available_encounters);
            concat_available_encounters.AddRange(setEncountList.Values);

            // create a dictionary, assigning the table types to their respective lists of power levels
            Dictionary<int, List<EncountTable>> power_ranges = new Dictionary<int, List<EncountTable>>();
            foreach (int i in edit_range)
            {
                power_ranges.Add(i, new List<EncountTable>(randomTables.FindAll(table => table.Edit == i).OrderBy(table => table.PowerLevel)));
            }

            // gather a list of adjusted encounters
            List<object> adjusted_encounters = new List<object>();

            // balance the monsters
            foreach (KeyValuePair<int, List<EncountTable>> kv in power_ranges)
            {
                if (kv.Value.Count > 0)
                {
                    // create a dummy table for comparisons
                    EncountTable dummy_table = new EncountTable();
                    dummy_table.Edit = kv.Key;

                    // test each monster against the dummy table to get the list of available monsters
                    List<object> suitable_encounters = concat_available_encounters.FindAll(enc => encounterIsValid(enc, dummy_table));

                    // start with the first
                    EncountTable first_table = kv.Value[0];
                    double cum_highest = randomizeMonsterPowerLevel(first_table, -1, suitable_encounters);
                    kv.Value.RemoveAt(0);
                    suitable_encounters = removeAllAdjustedMonsters(suitable_encounters);

                    // then do the rest
                    foreach (EncountTable table in kv.Value)
                    {
                        if (table.PowerLevel > cum_highest + balance_variance)
                        {
                            cum_highest = randomizeMonsterPowerLevel(table, cum_highest, suitable_encounters);
                            suitable_encounters = removeAllAdjustedMonsters(suitable_encounters);
                        }
                    }
                }
            }

            //var debug = new List<Monster>(mons.Contents.Values).FindAll(mon => mon.AdjustedLevel != -1);
            //Console.WriteLine(debug);

            // so now that we've decided the power levels for the first batch of monsters... we now have to do the rest... and this is where we do the gospel randomization

            if (check_monsters_gospel.IsChecked == true)
            {
                bool succ = doGospelRandom(randomTables, true, false);
                if (!succ)
                {
                    unbalanceMonsters(mons);
                    randomizeEncountersBalanced(randomTables, mons);
                }
            }
            else
            {
                foreach (EncountTable table in randomTables)
                {
                    randomizeEncountTable(table, true, true);
                }
            }

            // after setting the adjusted power levels, we now need to adjust the monsters' attributes accordingly
        }
        private void unbalanceMonsters(MonsterFile mons)
        {
            foreach (Monster mon in mons.Contents.Values)
            {
                mon.AdjustedLevel = -1;
            }
        }
        private void shuffleMonsters()
        {
            // generate our list of tables to be shuffled
            List<EncountTable> monsterShuffleTables = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (e.Edit >= 1 && e.Edit <= 3) || e.Edit == 10);

            // create our encounter pool
            List<EncountTableEntry> encounter_pool = new List<EncountTableEntry>();
            List<EncountTableSetEntry> setEncounter_pool = new List<EncountTableSetEntry>();

            // old code // Dictionary<GospelEntry, List<object>> gospel_dic = new Dictionary<GospelEntry, List<object>>();

            // populate our encounter pool
            foreach (EncountTable table in monsterShuffleTables)
            {
                foreach (EncountTableEntry entry in table.Contents)
                {
                    if (entry != null && entry.ID != null && entry.ID != "0000")
                    {
                        encounter_pool.Add(entry);
                    }
                }
                foreach (EncountTableSetEntry entry in table.SetEncounters)
                {
                    if (entry != null && entry.ID != null && entry.ID != "00" && entry.ID != "86")
                    {
                        setEncounter_pool.Add(entry);
                    }
                }
            }

            List<object> combined_pool = new List<object>(encounter_pool);
            combined_pool.AddRange(setEncounter_pool); // our pool of encounters and set encounters

            flushEncounterTables(monsterShuffleTables); // flush tables; this gives us a set of fresh tables to insert monsters into, and also removes them from the gospel check

            // create our dictionary of gospel entries
            Dictionary<GospelEntry, List<object>> gospel_to_encounters = new Dictionary<GospelEntry, List<object>>();
            Dictionary<object, List<GospelEntry>> encounter_to_gospels = new Dictionary<object, List<GospelEntry>>();
            if (check_monsters_gospel.IsChecked == true) // we ignore this if gospel is not turned on
            {
                foreach (EncountTableEntry enc in encounter_pool)
                {
                    GospelEntry gospel = getGospelByEncounterID(enc.ID);
                    if (enc.ID != "0000" && gospel != null)
                    {
                        if (!gospel_to_encounters.ContainsKey(gospel))
                        {
                            gospel_to_encounters.Add(gospel, new List<object>());
                        }
                        gospel_to_encounters[gospel].Add(enc);
                    }
                }
                foreach (EncountTableSetEntry setEntry in setEncounter_pool)
                {
                    List<GospelEntry> gospels = getGospelBySetEncounterID(setEntry.ID);
                    if (setEntry.ID == "00" && setEntry.ID == "86")
                    {
                        foreach (GospelEntry gospel in gospels)
                        {
                            if (!gospel_to_encounters.ContainsKey(gospel))
                            {
                                gospel_to_encounters.Add(gospel, new List<object>());
                            }
                            gospel_to_encounters[gospel].Add(setEntry); // since we're only processing each set encounter once, we shouldn't have any duplicates, right...?
                        }
                    }
                }
            }
            // now do the reverse dictionary
            foreach (KeyValuePair<GospelEntry, List<object>> kv in gospel_to_encounters) {
                foreach (object enc in kv.Value)
                {
                    if (!encounter_to_gospels.ContainsKey(enc))
                    {
                        encounter_to_gospels.Add(enc, new List<GospelEntry>());
                    }
                    encounter_to_gospels[enc].Add(kv.Key);
                }
            }
            List<GospelEntry> mon_shuffle_gospel_list = new List<GospelEntry>(gospel_to_encounters.Keys);

            Dictionary<GospelEntry, int> gospel_amounts = new Dictionary<GospelEntry, int>(); // here is where we will keep track of how many gospel entries have been placed in satisfactory locations
            foreach (GospelEntry gospel in gospel_to_encounters.Keys)
                gospel_amounts.Add(gospel, 0);

            // before we move onto the distribution, assign shuffle values to each encounter
            foreach (object obj in combined_pool)
            {
                EncountTableEntry enc = obj as EncountTableEntry;
                EncountTableSetEntry setEntry = obj as EncountTableSetEntry;

                if (enc != null)
                {
                    enc.Shuffled = false;
                } else if (setEntry != null)
                {
                    setEntry.Shuffled = false;
                }
            }

            resetGospel(mon_shuffle_gospel_list);
            gospelIsSatisfied(customEncountFile, mon_shuffle_gospel_list); // we're just checking to see if any of our gospels are satisfied elsewhere

            // start by shuffling in gospels
            foreach (KeyValuePair<GospelEntry, List<object>> kv in gospel_to_encounters)
            {
                GospelEntry gospel = kv.Key;
                List<object> encounts = kv.Value;
                if (gospel.Required && !gospel.Successful)
                {
                    encounts = new List<object>(encounts.OrderBy(x => rand.Next()));

                    foreach (object e in encounts)
                    {
                        bool succ = tryPlaceShuffleMonster(e, monsterShuffleTables, true);

                        if (succ)
                            break; // TODO: update gospel thing
                    }
                }
            }

            foreach (object entry in combined_pool)
            {
                //placeShuffleMonster(entry, monsterShuffleTables);
                // TODO: this doesn't really work because we don't have a way to update our list of monsters... we don't know which ones were already shuffled!!!
            }

            /*************************************\
            |                                     |
            |    |||||||   OLD CODE   |||||||     |
            |    VVVVVVV              VVVVVVV     |
            |                                     |
            \*************************************/
            /*// now do the balancing
            if (check_monsters_stats_balance.IsChecked == true)
            {
                int[] table_types = new int[] { 1, 2, 3, 10 };

                List<EncountTableEntry> available_encounters = new List<EncountTableEntry>();
                List<EncountTableSetEntry> available_setEncounters = new List<EncountTableSetEntry>();
                foreach (object enc in combined_pool)
                {
                    EncountTableEntry e = enc as EncountTableEntry;
                    EncountTableSetEntry se = enc as EncountTableSetEntry;

                    if (e != null && available_encounters.All(item => getEncounterById(item.ID).Monster != getEncounterById(e.ID).Monster) && getMonsterByEncounterID(e.ID, customMonsterFile).DoNotAverage != true)
                    {
                        available_encounters.Add(e);
                    }
                    else if (se != null && available_setEncounters.All(item => item.ID != se.ID) && getSetEncounterById(se.ID).Contents.Any(id => getMonsterById(id, customMonsterFile).DoNotAverage != true))
                    {
                        available_setEncounters.Add(se);
                    }
                }
                List<object> concat_available_encounters = new List<object>(available_encounters);
                concat_available_encounters.AddRange(available_setEncounters);

                // create a dictionary of power levels
                Dictionary<int, List<EncountTable>> power_ranges = new Dictionary<int, List<EncountTable>>();
                foreach (int i in table_types)
                {
                    power_ranges.Add(i, new List<EncountTable>(monsterShuffleTables.FindAll(table => table.Edit == i).OrderBy(table => table.PowerLevel)));
                }

                // balance the monsters
                foreach (KeyValuePair<int, List<EncountTable>> kv in power_ranges)
                {
                    if (kv.Value.Count > 0)
                    {
                        // create a dummy table for comparisons
                        EncountTable dummy_table = new EncountTable();
                        dummy_table.Edit = kv.Key;

                        // test each monster against the dummy table to get the list of available monsters
                        List<object> suitable_encounters = concat_available_encounters.FindAll(enc => encounterIsValid(enc, dummy_table));

                        // start with the first
                        EncountTable first_table = kv.Value[0];
                        double cum_highest = randomizeMonsterPowerLevel(first_table, -1, suitable_encounters, combined_pool);
                        kv.Value.RemoveAt(0);
                        suitable_encounters = removeAllAdjustedMonsters(suitable_encounters);

                        // then do the rest
                        foreach (EncountTable table in kv.Value)
                        {
                            if (table.PowerLevel > cum_highest + balance_variance)
                            {
                                cum_highest = randomizeMonsterPowerLevel(table, cum_highest, suitable_encounters, combined_pool);
                                suitable_encounters = removeAllAdjustedMonsters(suitable_encounters);
                            }
                        }
                    }
                }
            }*/
            // ^ old code contained within

            // old code // there should be a gospel check here
            //foreach (KeyValuePair<GospelEntry, List<object>> kv in gospel_dic)
            //{
            //    placeGospelShuffleEntry(kv, combined_pool, monsterShuffleTables, gospel_dic);
            //}

        }
        private bool tryPlaceShuffleMonster(object entry, List<EncountTable> monsterShuffleTables, bool gospel = false)
        {
            EncountTableEntry enc = entry as EncountTableEntry;
            EncountTableSetEntry se = entry as EncountTableSetEntry;

            List<EncountTable> valid_tables = new List<EncountTable>(getValidTables(entry, monsterShuffleTables, gospel).OrderBy(x => rand.Next()));
            
            foreach (EncountTable table in valid_tables)
            {
                bool succ = tryAddEncounter(table, entry);

                if (succ)
                    return succ;
            }
            
            return false;
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
        private Monster getMonsterById(string id, MonsterFile data)
        {
            return data.Contents[id];
        }
        private Monster getMonsterById(string id)
        {
            return getMonsterById(id, monsterFile);
        }
        private Monster getMonsterByEncounterID(string id)
        {
            return getMonsterById(getEncounterById(id).Monster);
        }
        private Monster getMonsterByEncounterID(string id, MonsterFile data)
        {
            return getMonsterById(getEncounterById(id).Monster, data);
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
        private List<SetEncounter> getGospelSetEncounters(GospelEntry g)
        {
            List<SetEncounter> result = new List<SetEncounter>();

            // get everything for each ID
            foreach (string id in g.Monsters)
            {
                foreach (SetEncounter se in new List<SetEncounter>(setEncountList.Values).FindAll(se => se.Contents.Contains(id)))
                {
                    if (result.Contains(se) != true)
                        result.Add(se);
                }
            }

            return result;
        }
        private GospelEntry getGospelByMonsterID(string id, List<GospelEntry> g_list = null)
        {
            g_list = g_list ?? gospelList;

            return gospelList.Find(g => g.Monsters.Contains(id));
        }
        // Find all tables an encounter / monster can fit in.
        private List<EncountTable> getValidTables(object subject, List<EncountTable> targets, bool gospel = false, bool balance = false, bool check_for_room = false)
        {
            // subject: our encounter
            // targets: the list of tables we want to test
            // gospel : if true, also check for gospel validity
            // balance: if true, also check for balance validity
            // check_for_room: if true, also check if table has room

            return new List<EncountTable>(targets.FindAll(t => {
                return encounterIsValid(subject, t, gospel, balance) && 
                    (tableHasRoom(t, subject) || !check_for_room);
            }));
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

            if (check_monsters_stats_balance.IsChecked != true)
            {
                foreach (Monster mon in customMonsterFile.Contents.Values)
                {
                    if (mon.Edit.ToUpper().Contains("N") != true && mon.Edit.ToUpper().Contains("C") != true && mon.Edit.ToUpper().Contains("I") != true) {
                        if (mon.Edit.ToUpper().Contains("P") != true)
                        {
                            mon.HP = randomizeStat(mon.HP, minStat, maxStat, check_monsters_stats_hp, "Z");
                            mon.MP = randomizeStat(mon.MP, minStat, maxStat, check_monsters_stats_mp, "M");
                            mon.Gold = randomizeStat(mon.Gold, minStat, maxStat, check_monsters_stats_gold);
                            mon.Experience = randomizeStat(mon.Experience, minStat, maxStat, check_monsters_stats_exp);
                        }
                        mon.Attack = randomizeStat(mon.Attack, minStat, maxStat, check_monsters_stats_attack);
                        mon.Defence = randomizeStat(mon.Defence, minStat, maxStat, check_monsters_stats_defence);
                        mon.Agility = randomizeStat(mon.Agility, minStat, maxStat, check_monsters_stats_agility);
                        //mon.Wisdom = randomizeStat(mon.Wisdom, minStat, maxStat, check_monsters_stats_wisdom);
                        if (check_monsters_stats_resist.IsChecked == true)
                        {
                            for (int i = 0; i < mon.Resistances.Length; i++)
                            {
                                mon.Resistances[i] = "0" + rand.Next(4).ToString();
                            }
                        }
                    }
                    if (check_monsters_stats_actions.IsChecked == true && mon.Edit.ToUpper().Contains("I") != true)
                    {
                        // to be added
                    }
                }
            }
            if (check_monsters_stats_items.IsChecked == true)
            {
                loadItems();
                Item[] validItems = Array.FindAll<Item>(itemFile.Contents, e => e.Spawn == 1);

                foreach (Monster mon in customMonsterFile.Contents.Values)
                {
                    if (mon.Edit.ToUpper().Contains("L") != true && mon.Edit.ToUpper().Contains("C") != true && mon.Edit.ToUpper().Contains("I") != true && mon.Edit.ToUpper().Contains("N") != true && mon.Edit.ToUpper().Contains("P") != true)
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
            // now do parents
            foreach (Monster mon in customMonsterFile.Contents.Values)
            {
                if (mon.Parent != null && mon.Parent.Length > 0)
                {
                    var parent = getMonsterById(mon.Parent, customMonsterFile);
                    parent.Children.Add(mon);
                    if (mon.Edit.ToUpper().Contains("P") || mon.Edit.ToUpper().Contains("I") || mon.Edit.ToUpper().Contains("C"))
                    {
                        mon.HP = parent.HP;
                        mon.MP = parent.MP;
                        mon.Experience = parent.Experience;
                        mon.Gold = parent.Gold;
                        mon.Items = parent.Items;
                    }
                    if (mon.Edit.ToUpper().Contains("I") || mon.Edit.ToUpper().Contains("C"))
                    {
                        mon.Attack = parent.Attack;
                        mon.Defence = parent.Defence;
                        mon.Agility = parent.Agility;
                        mon.Wisdom = parent.Wisdom;
                        mon.Resistances = parent.Resistances;
                    }
                    if (mon.Edit.ToUpper().Contains("I"))
                    {
                        mon.Actions = parent.Actions;
                    }
                }
            }
        }
        private void randomizeEncounters()
        {
            loadEncounters();
            loadSetEncounters();
            prepareCustomEncountFile();

            if (check_monsters_stats_balance.IsChecked == true)
                prepareCustomMonsterFile();

            // we find our working monster file
            MonsterFile mons = (customMonsterFile != null) ? customMonsterFile : monsterFile;

            // let's gather the encounter tables we'll randomize
            var randomTables = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (
                (((e.Edit >= 1 && e.Edit <= 3) || e.Edit == 10) && radio_monsters_overworld_random.IsChecked == true) ||
                (e.Edit == 4 && radio_monsters_arena_random.IsChecked == true) || 
                /*(e.Edit == 5 && check_monsters_enemySpecial.IsChecked == true) ||*/
                (((e.Edit >= 6 && e.Edit <= 8) || e.Edit == 11) && radio_monsters_boss_random.IsChecked == true) /*|| 
                (e.Edit == 9 && there are chests that exist) */) &&
                (e.Parent == null || e.Parent == ""));
            if (radio_monsters_boss_random.IsChecked == true)
                addTrapChests(randomTables);

            // shuffle monsters
            List<List<EncountTable>> masterShuffleList = new List<List<EncountTable>>();
            // shuffle overworld tables
            if (radio_monsters_overworld_tables.IsChecked == true)
            {
                masterShuffleList.Add(new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => (e.Edit >= 1 && e.Edit <= 3) || e.Edit == 10));
            }
            // shuffle scripted encounters
            if (radio_monsters_boss_shuffle.IsChecked == true)
            {
                List<EncountTable> scriptedList = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => e.Edit == 6 || e.Edit == 11 || e.Edit == 9);
                List<EncountTable> postgameList = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => e.Edit == 7);
                List<EncountTable> memoriamList = new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => e.Edit == 8);
                if (check_monsters_includePostgame.IsChecked == true)
                    scriptedList.AddRange(postgameList);
                else
                    masterShuffleList.Add(postgameList);
                if (check_monsters_includeMemoriam.IsChecked == true)
                    scriptedList.AddRange(memoriamList);
                else
                    masterShuffleList.Add(memoriamList);
                masterShuffleList.Add(scriptedList);
            }
            // shuffle arena teams
            if (radio_monsters_arena_shuffle.IsChecked == true)
            {
                masterShuffleList.Add(new List<EncountTable>(customEncountFile.Contents.Values).FindAll(e => e.Edit == 4));
            }

            // now do the actual shuffling process
            foreach (List<EncountTable> shuffleList in masterShuffleList)
            {
                shuffleEncounterTables(shuffleList);
            }

            // shuffle individual encounters
            if (radio_monsters_overworld_shuffle.IsChecked == true)
            {
                shuffleMonsters();
            }

            // do the randoms
            if (check_monsters_stats_balance.IsChecked == true)
            {
                randomizeEncountersBalanced(randomTables, mons); // balancing takes priority
            }
            else if (check_monsters_gospel.IsChecked == true) {

                doGospelRandom(randomTables); // then randomize by gospel

            }
            else
            {
                foreach (EncountTable table in randomTables)
                {
                    randomizeEncountTable(table); // otherwise randomize randomly
                }
            }

            // finally, fill the remaining empty spaces in all of the tables with blank entries
            foreach (EncountTable table in customEncountFile.Contents.Values)
            {
                EncountTableEntry defaultEntry = new EncountTableEntry();
                EncountTableSetEntry defaultSetEntry = new EncountTableSetEntry();

                for (int i = 0; i < 10; i++)
                {
                    var content = table.Contents[i];
                    if (content == null || content.ID == null)
                    {
                        table.Contents[i] = defaultEntry;
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    var content = table.SetEncounters[i];
                    if (content == null || content.ID == null)
                    {
                        table.SetEncounters[i] = defaultSetEntry;
                    }
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

            int trapAmt = 0;

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
                    if (trapAmt < 4)
                        box.Value = validTraps[trapAmt];
                    else
                        box.Value = validTraps[rand.Next(validTraps.Length)];
                    y++;
                    trapAmt++;
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
                    if (mon.Edit.ToUpper().Contains("N") != true && mon.Edit.ToUpper().Contains("C") != true && mon.Edit.ToUpper().Contains("I") != true)
                    {
                        string addStr = "╔" + stringFillToLength(capFirstLetter(mon.Name), 73, '═') + "╗\n";
                        addStr += mon.AdjustedLevel.ToString();
                        addStr += "║┌Stats──────────────────┬───────────────────────┬───────────────────────┐║\n" +
                            "║│HP:" + stringFillToLength(mon.HP.ToString(), 20, ' ', 1) +
                            "│MP:" + stringFillToLength(mon.MP == 255 ? "Infinite" : mon.MP.ToString(), 20, ' ', 1) +
                            "│Attack:" + stringFillToLength(mon.Attack.ToString(), 16, ' ', 1) + "│║\n" +
                            "║├───────────────────────┼───────────────────────┼───────────────────────┤║\n" +
                            "║│Defence:" + stringFillToLength(mon.Defence.ToString(), 15, ' ', 1) +
                            "│Agility:" + stringFillToLength(mon.Agility.ToString(), 15, ' ', 1) +
                            "│Wisdom:" + stringFillToLength(mon.Wisdom.ToString(), 16, ' ', 1) + "│║\n" +
                            "║├───────────────────────┴───────────┬───────────┴───────────────────────┤║\n" +
                            "║│Experience:" + stringFillToLength(mon.Experience.ToString(), 24, ' ', 1) +
                            "│Gold:" + stringFillToLength(mon.Gold.ToString(), 30, ' ', 1) + "│║\n" +
                            "║└───────────────────────────────────┴───────────────────────────────────┘║\n" +
                            "║Items Dropped: ";
                        string itemStr = findItemByID(mon.Items[0]).Name;
                        if (mon.Items[1] != "0000")
                        {
                            itemStr += ", " + findItemByID(mon.Items[1]).Name;
                        }
                        addStr += stringFillToLength(itemStr, 58, ' ') + "║\n";
                        addStr += "║Actions:                                                                 ║\n";

                        var concatActions = new List<string>(mon.Actions);
                        foreach (Monster child in mon.Children)
                        {
                            if (child.Edit.ToUpper().Contains("C"))
                                concatActions.AddRange(child.Actions);
                        }

                        List<Action> monActions = new List<Action>();
                        foreach (string act in concatActions)
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
                        for (int i = 2; i < 24; i++)
                        {
                            resStr[i] = stringFillToLength(resistName.Keys.ElementAt(i) + ":", 12, ' ') + " " + stringFillToLength(getResistPercent(mon.Resistances[i - 2], resistName.Values.ElementAt(i)) + "%", 4, ' ', 1);
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
                }

                File.WriteAllText(path, string.Join("\n", outputArr));
            }
        }

        // option log
        private void generateOptionLog(string path, int seed)
        {
            List<string> option_string_list = new List<string>();

            foreach (Option opt in optionOutputList)
            {
                if (opt.Show)
                {
                    string add_string;
                    string prefix = opt.Indent > 0 ? stringFillToLength(" - ", opt.Indent * 3, ' ', 1) : "";
                    switch (opt.Type)
                    {
                        case "none":
                            break;
                        case "blank":
                            option_string_list.Add("");
                            break;
                        case "label":
                            option_string_list.Add(prefix + opt.Text);
                            break;

                        case "version":
                            option_string_list.Add(prefix + opt.Text);
                            break;
                        case "seed":
                            option_string_list.Add(prefix + opt.Text.Replace("$1",seed.ToString()));
                            break;

                        case "string":
                        case "number":
                        case "bool":
                            List<object> controls = new List<object>(opt.Controls.Select(o => FindName(o)));
                            if (controls.All(c =>
                            {
                                System.Windows.Controls.Control ctrl = c as System.Windows.Controls.Control;
                                return ctrl != null && ctrl.IsEnabled == true;
                            }))
                            {
                                add_string = opt.Text;
                                int i = 0;
                                foreach (object c in controls)
                                {
                                    i++;
                                    System.Windows.Controls.Primitives.ToggleButton toggle = c as System.Windows.Controls.Primitives.ToggleButton;
                                    Xceed.Wpf.Toolkit.DoubleUpDown dupdown  = c as Xceed.Wpf.Toolkit.DoubleUpDown;
                                    Xceed.Wpf.Toolkit.SingleUpDown supdown  = c as Xceed.Wpf.Toolkit.SingleUpDown;
                                    Xceed.Wpf.Toolkit.ShortUpDown  shupdown = c as Xceed.Wpf.Toolkit.ShortUpDown;
                                    Xceed.Wpf.Toolkit.LongUpDown   lupdown  = c as Xceed.Wpf.Toolkit.LongUpDown;

                                    if (toggle != null)
                                        add_string = add_string.Replace("$"+i.ToString(), toggle.IsChecked == true ? "True" : "False");
                                    else if (dupdown != null)
                                        add_string = add_string.Replace("$" + i.ToString(), dupdown.Value.ToString());
                                    else if (supdown != null)
                                        add_string = add_string.Replace("$" + i.ToString(), supdown.Value.ToString());
                                    else if (shupdown != null)
                                        add_string = add_string.Replace("$" + i.ToString(), shupdown.Value.ToString());
                                    else if (lupdown != null)
                                        add_string = add_string.Replace("$" + i.ToString(), lupdown.Value.ToString());
                                }
                                option_string_list.Add(prefix + add_string);
                            }
                            break;
                        case "radio":
                            List<object> radios = new List<object>(opt.Controls.Select(o => FindName(o)));
                            if (radios.All(c =>
                            {
                                System.Windows.Controls.Control ctrl = c as System.Windows.Controls.Control;
                                return ctrl != null && ctrl.IsEnabled == true;
                            }))
                            {
                                System.Windows.Controls.Primitives.ToggleButton ctrl = radios.Find(c =>
                                {
                                    System.Windows.Controls.Primitives.ToggleButton toggle = c as System.Windows.Controls.Primitives.ToggleButton;

                                    return toggle.IsChecked == true;
                                }) as System.Windows.Controls.Primitives.ToggleButton;

                                if (ctrl != null)
                                    add_string = opt.Text.Replace("$1", ctrl.Content.ToString());
                                else
                                    add_string = opt.Text.Replace("$1", "None");

                                option_string_list.Add(prefix + add_string);
                            }
                            break;
                    }
                }
            }

            File.WriteAllText(path + "/options.txt", string.Join("\n", option_string_list));
        }
        private void generateOptionJson(string path, int seed)
        {
            loadElementList();

            OutputOptionFile outputOptionFile = new OutputOptionFile();
            outputOptionFile.Seed = seed;
            outputOptionFile.Path = tbox_directory.Text != Environment.CurrentDirectory ? tbox_directory.Text : null;

            foreach (string name in elementList)
            {
                object element = FindName(name);

                if (element != null)
                {
                    OutputOption outOpt = new OutputOption();
                    outOpt.Element = name;

                    System.Windows.Controls.Primitives.ToggleButton toggle = element as System.Windows.Controls.Primitives.ToggleButton;
                    Xceed.Wpf.Toolkit.DoubleUpDown dupdown                 = element as Xceed.Wpf.Toolkit.DoubleUpDown;
                    Xceed.Wpf.Toolkit.SingleUpDown supdown                 = element as Xceed.Wpf.Toolkit.SingleUpDown;
                    Xceed.Wpf.Toolkit.ShortUpDown shupdown                 = element as Xceed.Wpf.Toolkit.ShortUpDown;
                    Xceed.Wpf.Toolkit.LongUpDown lupdown                   = element as Xceed.Wpf.Toolkit.LongUpDown;

                    if (toggle != null)
                        outOpt.BoolValue = toggle.IsChecked == true;
                    else if (dupdown != null)
                        outOpt.DoubleValue = (double)dupdown.Value;
                    else if (supdown != null)
                        outOpt.DoubleValue = (double)supdown.Value;
                    else if (shupdown != null)
                        outOpt.IntValue = (int)shupdown.Value;
                    else if (lupdown != null)
                        outOpt.IntValue = (int)lupdown.Value;

                    outputOptionFile.Elements.Add(outOpt);
                }
            }

            string jsonOutString = JsonSerializer.Serialize(outputOptionFile, new JsonSerializerOptions { DefaultIgnoreCondition = (JsonIgnoreCondition)2 });
            File.WriteAllText(path + "/options.json", jsonOutString);
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
