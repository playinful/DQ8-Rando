using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using DQ8Rando3DS.Modules;
using System.Timers;
using System.Threading;
using System.Diagnostics;

namespace DQ8Rando3DS
{
    public class RandomizerMain
    {
        public RandomizerMain(RandomizerOptions options)
        {
            Options = options.Clone();
        }

        public RandomizerOptions Options { get; set; }
        public Random RNG { get; set; }

        public void Start()
        {
            try
            {
                if (Directory.Exists($"{Options.Path}/romfs"))
                    Directory.Delete($"{Options.Path}/romfs", true);
                if (Directory.Exists($"{Options.Path}/spoiler"))
                    Directory.Delete($"{Options.Path}/spoiler", true);
                MainWindow.UpdateStatus("Cleared output directory.");
            }
            catch
            {
                MainWindow.UpdateStatus("Failed to clear output directory.");
            }

            MainWindow.UpdateStatus("Starting randomizer...");

            // seed RNG
            if (Options.Seed is null)
            {
                MainWindow.UpdateStatus("Generating seed...");
                Options.Seed = new Random().Next(int.MinValue, int.MaxValue);
            }
            RNG = new Random((int)Options.Seed);

            SaveOptionsFile();

            RandomizerModule[] modules = {
                new MonsterRandomizer(), // Not done
                new TreasureRandomizer(), // Not done
                new ShopRandomizer(), // Not done
                new CasinoRandomizer(), // Not done
                new HotelChargesRandomizer(), // DONE
                new AlchemyRandomizer(), // DONE
                new BookRandomizer(), // DONE
                new PlayerRandomizer(), // not started
                new TextRandomizer(), // not started
                new MusicRandomizer(), // not started
                new DispositionRandomizer(), // not started
                new Cosmetics(), // almost done
                new Miscellaneous(), // not done
            };

            foreach (RandomizerModule module in modules)
            {
                module.Initialize(Options, RNG.NextRandom());
                module.Start();
            }

            MainWindow.UpdateStatus($"Finished creating patch with seed {Options.Seed}.");

            Process.Start("explorer.exe", Options.Path);

            MainWindow.Enable();
        }

        public void SaveOptionsFile()
        {
            Options.Save($"{Options.Path}/options.json");
            MainWindow.UpdateStatus($"Saved options file to `{Options.Path}/options.json`.");
        }

    }
}
