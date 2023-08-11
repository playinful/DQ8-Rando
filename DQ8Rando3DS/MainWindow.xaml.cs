using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit.Core.Converters;
using Ookii.Dialogs.Wpf;
using System.Threading;
using System.Windows.Threading;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Markup;
using DQ8Rando3DS.Tables;

namespace DQ8Rando3DS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<MainWindow> Instances = new List<MainWindow>();
        public MainWindow()
        {
            Instances.Add(this);
            InitializeComponent();
            DataContext = new RandomizerOptions();
            UpdateStatus("Program initialized");
            if (File.Exists("default.json"))
                LoadPreset("default.json");
        }
        public RandomizerOptions Options => (RandomizerOptions)DataContext;

        private Thread RandomizerThread;

        public void LoadPreset(string path)
        {
            DataContext = RandomizerOptions.Load(path);
            UpdateStatus($"Loaded preset from `{path}`.");
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSeed())
            {
                if (new Regex("^-?[0-9]+$").IsMatch(SeedBox.Text))
                {
                    MessageBox.Show(
                        "Seed value is too large. Please enter a different seed or leave it blank.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(
                    "Please enter a valid seed or leave it blank.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
            folderDialog.Multiselect = false;
            bool? result = folderDialog.ShowDialog();
            
            if (result != true)
            {
                return;
            }
            else
            {
                Options.Path = folderDialog.SelectedPath;
            }

            TopMenu.IsEnabled = MainTabControl.IsEnabled = BottomMenu.IsEnabled = false;
            StartRandomizer();
        }

        private void StartRandomizer()
        {
            RandomizerThread = new Thread(new RandomizerMain(Options).Start);
            RandomizerThread.Start();
        }

        private bool ValidateSeed()
        {
            if (SeedBox.Text.Length == 0)
            {
                Options.Seed = null;
                return true;
            }

            bool b = int.TryParse(SeedBox.Text, out int seed);
            if (b) Options.Seed = seed;
            return b;
        }
        private bool SeedBox_IsValidText(string text)
        {
            Regex accepted = new Regex("^-?[0-9]*$");
            return accepted.IsMatch(text);
        }
        private void SeedBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!SeedBox_IsValidText(e.Text))
                e.Handled = true;
        }
        private void SeedBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
                e.Handled = true;
        }
        private void SeedBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text) && SeedBox_IsValidText((string)e.Data.GetData(DataFormats.Text)))
                e.Effects = DragDropEffects.Copy;
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
        private void Menu_Presets_Save_Click(object sender, RoutedEventArgs e)
        {
            ValidateSeed();

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".json";
            dialog.Filter = "JSON file (.json)|*.json";
            bool? result = dialog.ShowDialog();

            if (result != true)
                return;

            try
            {
                Options.Save(dialog.FileName);
                UpdateStatus($"Wrote preset to `{dialog.FileName.Replace("\\\\", "/")}`.");
            }
            catch
            {
                MessageBox.Show(
                    "Something went wrong. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                UpdateStatus("Failed to write preset to file.");
            }
        }
        private void Menu_Presets_Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".json";
            dialog.Filter = "JSON file (.json)|*.json";
            dialog.Multiselect = false;
            bool? result = dialog.ShowDialog();

            if (result != true)
                return;

            try
            {
                LoadPreset(dialog.FileName);
            }
            catch
            {
                MessageBox.Show(
                    "Something went wrong. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                UpdateStatus($"Failed to load preset from file `{dialog.FileName.Replace("\\\\", "/")}`.");
            }
        }
        private void Menu_Presets_SetDefault_Click(object sender, RoutedEventArgs e)
        {
            ValidateSeed();
            Options.Save("default.json");
            UpdateStatus("Saved default settings.");
        }
        private void Menu_Presets_ClearDefault_Click(object sender, RoutedEventArgs e)
        {
            File.Delete("default.json");
            UpdateStatus("Reset default settings.");
        }

        public static void UpdateStatus(string text)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Console.WriteLine(text);
                foreach (MainWindow window in Instances)
                {
                    if (window is not null)
                        window.StatusBarText.Text = text;
                }
            }));
        }
        public static void Enable()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (MainWindow window in Instances)
                {
                    if (window is not null)
                        window.TopMenu.IsEnabled = window.MainTabControl.IsEnabled = window.BottomMenu.IsEnabled = true;
                }
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (RandomizerThread is not null && RandomizerThread.IsAlive)
                RandomizerThread.Interrupt();
        }

        // -- COSMETICS -- //

        public static readonly string[] CosmeticSortKey = new string[]
        {
            "random",
            "random vanilla",
            "classic",
            "soldier uniform",
            "dragovian armour",
            "gothic vestment",
            "metal king armour",
            "dogsbody's vest",
            "oriental warrior wear",
            "butler's best",
            "jessica's outfit",
            "magic bikini",
            "bunny suit",
            "dancer's costume",
            "dangerous bustier",
            "divine bustier",
            "hexlet's skirt",
            "abiding blazer",
            "nitid tutu",
            "mandarin dress",
            "cap'n's clothes",
            "kunoichi costume",
            "jester's outfit",
            "basic black",
            "basic blue",
            "basic green",
            "basic grey",
            "basic orange",
            "basic pink",
            "basic purple",
            "basic red",
            "basic white",
            "basic yellow",
            "smash bros. alternate",
            "in-vince-ible",
            "trans pride",
            "serena",
            "partner in crime",
            "yakuza",
        };
        private string[] Characters;
        private ComboBox[][] CostumeBoxes;
        private ComboBox[] CostumePackBoxes;
        private void LoadCosmeticOptions()
        {
            if (Characters is not null)
                return;

            Characters ??= new string[] { "Hero", "Yangus", "Jessica", "Angelo", "Red", "Morrie" };
            CostumeBoxes ??= new ComboBox[][]
            {
                new ComboBox[]{
                    ComboBox_Cosmetics_Hero_Costume1,
                    ComboBox_Cosmetics_Hero_Costume2,
                    ComboBox_Cosmetics_Hero_Costume3,
                    ComboBox_Cosmetics_Hero_Costume4,
                    ComboBox_Cosmetics_Hero_Costume5,
                },
                new ComboBox[]{
                    ComboBox_Cosmetics_Yangus_Costume1,
                    ComboBox_Cosmetics_Yangus_Costume2,
                },
                new ComboBox[]{
                    ComboBox_Cosmetics_Jessica_Costume1,
                    ComboBox_Cosmetics_Jessica_Costume2,
                    ComboBox_Cosmetics_Jessica_Costume3,
                    ComboBox_Cosmetics_Jessica_Costume4,
                    ComboBox_Cosmetics_Jessica_Costume5,
                    ComboBox_Cosmetics_Jessica_Costume6,
                    ComboBox_Cosmetics_Jessica_Costume7,
                    ComboBox_Cosmetics_Jessica_Costume8,
                    ComboBox_Cosmetics_Jessica_Costume9,
                    ComboBox_Cosmetics_Jessica_Costume10,
                },
                new ComboBox[]{
                    ComboBox_Cosmetics_Angelo_Costume1,
                    ComboBox_Cosmetics_Angelo_Costume2,
                    ComboBox_Cosmetics_Angelo_Costume3,
                },
                new ComboBox[]{
                    ComboBox_Cosmetics_Red_Costume1,
                    ComboBox_Cosmetics_Red_Costume2,
                    ComboBox_Cosmetics_Red_Costume3,
                    ComboBox_Cosmetics_Red_Costume4,
                },
                new ComboBox[]{
                    ComboBox_Cosmetics_Morrie_Costume1,
                    ComboBox_Cosmetics_Morrie_Costume2,
                }
            };
            CostumePackBoxes ??= new ComboBox[]
            {
                ComboBox_Cosmetics_Hero,
                ComboBox_Cosmetics_Yangus,
                ComboBox_Cosmetics_Jessica,
                ComboBox_Cosmetics_Angelo,
                ComboBox_Cosmetics_Red,
                ComboBox_Cosmetics_Morrie,
            };

            for (int i = 0; i < Characters.Length; i++)
            {
                string character = Characters[i];
                ComboBox[] boxes = CostumeBoxes[i];
                ComboBox packBox = CostumePackBoxes[i];

                List<string> skins = Directory.GetFiles($"./Skins/{character}/").Where(f => f.ToLower().EndsWith(".bch")).Select
                (f => Path.GetFileNameWithoutExtension(f)).Where(f =>
                    !f.ToLower().EndsWith("_h") &&
                    f.ToLower() != "random" &&
                    f.ToLower() != "random vanilla")
                .ToList();

                skins.Sort((a, b) =>
                {
                    if (CosmeticSortKey.Contains(a.ToLower()))
                    {
                        if (CosmeticSortKey.Contains(b.ToLower()))
                        {
                            return Array.IndexOf(CosmeticSortKey, a.ToLower()) - Array.IndexOf(CosmeticSortKey, b.ToLower());
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (CosmeticSortKey.Contains(b.ToLower()))
                        {
                            return 1;
                        }
                        else
                        {
                            return a.CompareTo(b);
                        }
                    }
                });

                skins = skins.Prepend("Random Vanilla").Prepend("Random").ToList();

                foreach (ComboBox box in boxes)
                {
                    foreach (string skin in skins)
                    {
                        box.Items.Add(skin);
                    }
                }

                List<string> skinPacks = Directory.GetDirectories($"./Skins/{character}/")
                    .Select(d => Path.GetDirectoryName(d))
                    .Where(d => d.ToLower() != "none" && d.ToLower() != "random")
                    .ToList();

                skinPacks = skinPacks.Prepend("Random").Prepend("None").ToList();

                foreach (string skinPack in skinPacks)
                {
                    packBox.Items.Add(skinPack);
                }
            }
        }
        private void UpdateCosmeticOptions()
        {
            if (CostumeBoxes is null || CostumePackBoxes is null)
                return;

            for (int i = 0; i < CostumeBoxes[0].Length; i++)
            {
                ComboBox box = CostumeBoxes[0][i];
                Options.Cosmetics.Hero.Costumes[i] = box.Text;
            }
            for (int i = 0; i < CostumeBoxes[1].Length; i++)
            {
                ComboBox box = CostumeBoxes[1][i];
                Options.Cosmetics.Yangus.Costumes[i] = box.Text;
            }
            for (int i = 0; i < CostumeBoxes[2].Length; i++)
            {
                ComboBox box = CostumeBoxes[2][i];
                Options.Cosmetics.Jessica.Costumes[i] = box.Text;
            }
            for (int i = 0; i < CostumeBoxes[3].Length; i++)
            {
                ComboBox box = CostumeBoxes[3][i];
                Options.Cosmetics.Angelo.Costumes[i] = box.Text;
            }
            for (int i = 0; i < CostumeBoxes[4].Length; i++)
            {
                ComboBox box = CostumeBoxes[4][i];
                Options.Cosmetics.Red.Costumes[i] = box.Text;
            }
            for (int i = 0; i < CostumeBoxes[5].Length; i++)
            {
                ComboBox box = CostumeBoxes[5][i];
                Options.Cosmetics.Morrie.Costumes[i] = box.Text;
            }

            Options.Cosmetics.Hero.CostumePack    = CostumePackBoxes[0].Text;
            Options.Cosmetics.Yangus.CostumePack  = CostumePackBoxes[1].Text;
            Options.Cosmetics.Jessica.CostumePack = CostumePackBoxes[2].Text;
            Options.Cosmetics.Angelo.CostumePack  = CostumePackBoxes[3].Text;
            Options.Cosmetics.Red.CostumePack     = CostumePackBoxes[4].Text;
            Options.Cosmetics.Morrie.CostumePack  = CostumePackBoxes[5].Text;

            Panel_Cosmetics_Hero   .IsEnabled = ComboBox_Cosmetics_Hero   .SelectedIndex <= 0;
            Panel_Cosmetics_Yangus .IsEnabled = ComboBox_Cosmetics_Yangus .SelectedIndex <= 0;
            Panel_Cosmetics_Jessica.IsEnabled = ComboBox_Cosmetics_Jessica.SelectedIndex <= 0;
            Panel_Cosmetics_Angelo .IsEnabled = ComboBox_Cosmetics_Angelo .SelectedIndex <= 0;
            Panel_Cosmetics_Red    .IsEnabled = ComboBox_Cosmetics_Red    .SelectedIndex <= 0;
            Panel_Cosmetics_Morrie .IsEnabled = ComboBox_Cosmetics_Morrie .SelectedIndex <= 0;
        }

        private void Tab_Cosmetics_Selected(object sender, RoutedEventArgs e)
        {
            LoadCosmeticOptions();
        }
        private void ComboBox_Cosmetics_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateCosmeticOptions();
        }
        private void ComboBox_Cosmetics_DropDownClosed(object sender, EventArgs e)
        {
            UpdateCosmeticOptions();
        }
    }
}
