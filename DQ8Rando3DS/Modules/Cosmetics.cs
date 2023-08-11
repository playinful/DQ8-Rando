using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace DQ8Rando3DS.Modules
{
    public class Cosmetics : RandomizerModule
    {
        // TODO
        public override void Start()
        {
            if (Options.Cosmetics.Hero.CostumePack == "Random")
                RandomizeCostumePack(Options.Cosmetics.Hero, "./Skins/Hero/");
            if (Options.Cosmetics.Yangus.CostumePack == "Random")
                RandomizeCostumePack(Options.Cosmetics.Yangus, "./Skins/Yangus/");
            if (Options.Cosmetics.Jessica.CostumePack == "Random")
                RandomizeCostumePack(Options.Cosmetics.Jessica, "./Skins/Jessica/");
            if (Options.Cosmetics.Angelo.CostumePack == "Random")
                RandomizeCostumePack(Options.Cosmetics.Angelo, "./Skins/Angelo/");
            if (Options.Cosmetics.Red.CostumePack == "Random")
                RandomizeCostumePack(Options.Cosmetics.Red, "./Skins/Red/");
            if (Options.Cosmetics.Morrie.CostumePack == "Random")
                RandomizeCostumePack(Options.Cosmetics.Morrie, "./Skins/Morrie/");

            RandomizeCostumes(Options.Cosmetics.Hero,    "./Skins/Hero/");
            RandomizeCostumes(Options.Cosmetics.Yangus,  "./Skins/Yangus/");
            RandomizeCostumes(Options.Cosmetics.Jessica, "./Skins/Jessica/");
            RandomizeCostumes(Options.Cosmetics.Angelo,  "./Skins/Angelo/");
            RandomizeCostumes(Options.Cosmetics.Red,     "./Skins/Red/");
            RandomizeCostumes(Options.Cosmetics.Morrie,  "./Skins/Morrie/");

            SetCostumes(Options.Cosmetics.Hero,    "./Skins/Hero/",    $"{Options.Path}/romfs/data/Model/player/c001_");
            SetCostumes(Options.Cosmetics.Yangus,  "./Skins/Yangus/",  $"{Options.Path}/romfs/data/Model/player/c002_");
            SetCostumes(Options.Cosmetics.Angelo,  "./Skins/Angelo/",  $"{Options.Path}/romfs/data/Model/player/c003_");
            SetCostumes(Options.Cosmetics.Jessica, "./Skins/Jessica/", $"{Options.Path}/romfs/data/Model/player/c004_");
            SetCostumes(Options.Cosmetics.Red,     "./Skins/Red/",     $"{Options.Path}/romfs/data/Model/player/c0011_");
            SetCostumes(Options.Cosmetics.Morrie,  "./Skins/Morrie/",  $"{Options.Path}/romfs/data/Model/player/c0012_");
        }

        public void RandomizeCostumePack(RandomizerOptions.CosmeticOptions.CostumeSet costumeSet, string path)
        {
            List<string> costumePacks = Directory.GetDirectories(path)
                .Where(p =>
                    Path.GetDirectoryName(p).ToLower() != "random" &&
                    Path.GetDirectoryName(p).ToLower() != "none"
                ).Select(p => Path.GetDirectoryName(p)).ToList();

            if (costumePacks.Count > 0 && RNG.NextBool())
            {
                costumeSet.CostumePack = RNG.Choice(costumePacks);
            }
            else
            {
                costumeSet.CostumePack = "None";
                for (int i = 0; i < costumeSet.Costumes.Length; i++)
                {
                    costumeSet.Costumes[i] = "Random";
                }
            }
        }
        public void RandomizeCostumes(RandomizerOptions.CosmeticOptions.CostumeSet costumeSet, string path)
        {
            if (!costumeSet.Costumes.Any(c => c == "Random" || c == "Random Vanilla"))
                return;

            List<string> costumes = Directory.GetFiles(path)
                .Where(c =>
                    c.ToLower().EndsWith(".bch") &&
                    Path.GetFileNameWithoutExtension(c).ToLower() != "random" &&
                    Path.GetFileNameWithoutExtension(c).ToLower() != "random vanilla"
                    && !c.ToLower().EndsWith("_h.bch"))
                .Select(c => Path.GetFileNameWithoutExtension(c)).ToList();

            List<string> vanillaCostumes = new List<string>();
            if (costumeSet == Options.Cosmetics.Hero)
                vanillaCostumes = new List<string>{
                    "Classic",
                    "Soldier Uniform",
                    "Dragovian Armour",
                    "Gothic Vestment",
                    "Metal King Armour",
                };
            if (costumeSet == Options.Cosmetics.Yangus)
                vanillaCostumes = new List<string>{
                    "Classic",
                    "Dogsbody's Vest",
                };
            if (costumeSet == Options.Cosmetics.Jessica)
                vanillaCostumes = new List<string>{
                    "Classic",
                    "Jessica's Outfit",
                    "Magic Bikini",
                    "Bunny Suit",
                    "Dancer's Costume",
                    "Dangerous Bustier",
                    "Divine Bustier",
                    "Hexlet's Skirt",
                    "Abiding Blazer",
                    "Nitid Tutu",
                };
            if (costumeSet == Options.Cosmetics.Angelo)
                vanillaCostumes = new List<string>{
                    "Classic",
                    "Oriental Warrior Wear",
                    "Butler's Best",
                };
            if (costumeSet == Options.Cosmetics.Red)
                vanillaCostumes = new List<string>{
                    "Classic",
                    "Mandarin Dress",
                    "Cap'n's Clothes",
                    "Kunoichi Costume",
                };
            if (costumeSet == Options.Cosmetics.Morrie)
                vanillaCostumes = new List<string>{
                    "Classic",
                    "Jester's Outfit",
                };

            for (int i = 0; i < costumeSet.Costumes.Length; i++)
            {
                if (costumeSet.Costumes[i] == "Random")
                {
                    costumeSet.Costumes[i] = RNG.Choice(costumes);
                    costumes.Remove(costumeSet.Costumes[i]);
                    if (vanillaCostumes.Contains(costumeSet.Costumes[i]))
                        vanillaCostumes.Remove(costumeSet.Costumes[i]);
                }
                if (costumeSet.Costumes[i] == "Random Vanilla")
                {
                    costumeSet.Costumes[i] = RNG.Choice(vanillaCostumes);
                    vanillaCostumes.Remove(costumeSet.Costumes[i]);
                    if (costumes.Contains(costumeSet.Costumes[i]))
                        costumes.Remove(costumeSet.Costumes[i]);
                }
            }
        }
        public void SetCostumes(RandomizerOptions.CosmeticOptions.CostumeSet costumeSet, string path, string outPath)
        {
            if (costumeSet.CostumePack != "None" && costumeSet.CostumePack is not null)
                return; // TODO

            for (int i = 0; i < costumeSet.Costumes.Length; i++)
            {
                string costume = costumeSet.Costumes[i];

                // TODO skip if default

                string modelPath = Path.Combine(path, $"{costume}.bch");
                string hiModelPath = Path.Combine(path, $"{costume}_H.bch");
                if (!File.Exists(modelPath))
                    continue;
                if (!File.Exists(hiModelPath))
                    hiModelPath = modelPath;

                Directory.CreateDirectory( Path.GetDirectoryName(outPath) );
                File.Copy(modelPath, $"{outPath}{i+1}.bch", overwrite: true);
                File.Copy(hiModelPath, $"{outPath}h{i+1}.bch", overwrite: true);
            }
        }
    }
}
