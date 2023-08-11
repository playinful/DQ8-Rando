using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Windows.Shapes;
using System.Net.Http.Headers;
using System.Windows;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace DQ8Rando3DS
{
    public class RandomizerOptions
    {
        public string Version { get; set; }

        public int? Seed { get; set; }
        [JsonIgnore] public string Path { get; set; }

        // MONSTERS
        public MonsterOptions Monsters { get; set; } = new MonsterOptions();
        public class MonsterOptions
        {
            // ENCOUNTERS
            public bool OverworldNoChange { get; set; } = true;
            public bool OverworldRandomEnemy { get; set; } = false;
            public bool OverworldRandomEnemyOrBoss { get; set; } = false;
            public bool OverworldShuffle { get; set; } = false;
            public bool OverworldShuffleByArea { get; set; } = false;

            public bool BossNoChange { get; set; } = true;
            public bool BossRandomEnemy { get; set; } = false;
            public bool BossRandomGroup { get; set; } = false;
            public bool BossShuffle { get; set;} = false;

            public bool ArenaNoChange { get; set; } = true;
            public bool ArenaRandomEnemy { get; set; } = false;
            public bool ArenaRandomEnemyOrBoss { get; set; } = false;
            public bool ArenaShuffle { get; set;} = false;

            public bool SpecialNoChange { get; set; } = true;
            public bool SpecialRandomEnemy { get; set; } = false;
            public bool SpecialRandomEnemyOrBoss { get; set; } = false;
            public bool SpecialShuffle { get; set; } = false;

            public bool ShuffleInfamous { get; set; } = false;

            public bool MixInInfamous { get; set; } = false;
            public bool MixInArena { get; set; } = false;
            public bool MixInPostgame { get; set; } = false;
            public bool MixInMemoriam { get; set; } = false;
            public bool MixInSpecial { get; set; } = false;
            public bool MixInExtra { get; set; } = false;

            public bool GuaranteeFullBestiary { get; set; } = false;

            public bool ShuffleBattlebacks { get; set; } = false;

            // MONSTERS
            public bool MonsterStatsNoChange { get; set; } = true;
            public bool MonsterStatsBalanced { get; set; } = false;
            public bool MonsterStatsChaos { get; set; } = false;

            public bool RandomizeMonsterExperience { get; set; } = false;
            public bool RandomizeMonsterGold { get; set; } = false;
            public bool RandomizeMonsterLoot { get; set; } = false;
            public bool RandomizeMonsterResistances { get; set; } = false;
            public bool RandomizeMonsterActions { get; set; } = false;

            public Visibility ShowMixIns => OverworldRandomEnemy || BossRandomEnemy || ArenaRandomEnemy ? Visibility.Visible : Visibility.Hidden;

            public MonsterOptions Clone()
            {
                return (MonsterOptions)MemberwiseClone();
            }
        }

        // TREASURE
        public TreasureOptions Treasure { get; set; } = new TreasureOptions();
        public class TreasureOptions
        {
            public bool NoChange { get; set; } = true;
            public bool Randomize { get; set; } = false;
            public bool Shuffle { get; set; } = false;

            public double ItemAmount { get; set; } = 35;
            public double GoldAmount { get; set; } = 32;
            public double TrapAmount { get; set; } = 3;
            public double EmptyAmount { get; set; } = 30;

            public short MaxGoldValue { get; set; } = 4000;
            public short MinGoldValue { get; set; } = 1;

            public bool BlueNoChange { get;set; } = true;
            public bool RandomizeBlueChestPools { get; set; } = false;
            public bool ShuffleBlueChests { get; set; } = false;
            public bool SwapBlueAndRedChests { get; set; } = false;

            public double BlueAmount { get; set; } = 0.5;

            public bool TrapNonChests { get; set; } = false;

            public bool LockThief { get; set; } = false;
            public bool LockMagic { get; set; } = false;
            public double LockAmount { get; set; } = 0.3;

            public TreasureOptions Clone()
            {
                return (TreasureOptions)MemberwiseClone();
            }
        }

        // SHOPPING
        public ShopOptions Shopping { get; set; } = new ShopOptions();
        public class ShopOptions
        {
            public bool ShopNoChange { get; set; } = true;
            public bool ShopItemShuffle { get; set; } = false;
            public bool ShopInventoryShuffle { get; set; } = false;
            public bool ShopRandom { get; set; } = false;

            public bool RandomPrices { get; set; } = false;
            public bool RandomMarkup { get; set; } = false;

            public int MinimumPrice { get; set; } = 1;
            public int MaximumPrice { get; set; } = 500000;

            public int MinimumMarkup { get; set; } = 50;
            public int MaximumMarkup { get; set; } = 300;

            public bool CasinoNoChange { get; set; } = true;
            public bool CasinoShuffle { get; set; } = false;
            public bool CasinoRandom { get; set; } = false;

            public bool CasinoRandomPrices { get; set; } = false;
            public int CasinoMinimumPrice { get; set; } = 300;
            public int CasinoMaximumPrice { get; set; } = 1000000;

            public bool HotelRandomPrices { get; set; } = false;
            public int HotelMinimumPrice { get; set; } = 1;
            public int HotelMaximumPrice { get; set; } = 50;

            public ShopOptions Clone()
            {
                return (ShopOptions)MemberwiseClone();
            }
        }

        // ALCHEMY
        public AlchemyOptions Alchemy { get; set; } = new AlchemyOptions();
        public class AlchemyOptions
        {
            public bool NoChange { get; set; } = true;
            public bool Shuffle { get; set; } = false;
            public bool IngredientRandom { get; set; } = false;
            public bool ResultRandom { get; set; } = false;
            public bool AllRandom { get; set; } = false;

            public bool BookShuffle { get; set; } = false;

            public AlchemyOptions Clone()
            {
                return (AlchemyOptions)MemberwiseClone();
            }
        }

        // COSMETICS
        public CosmeticOptions Cosmetics { get; set; } = new CosmeticOptions();
        public class CosmeticOptions
        {
            public CostumeSet Hero { get; set; } = new CostumeSet(5);
            public CostumeSet Yangus { get; set; } = new CostumeSet(2);
            public CostumeSet Jessica { get; set; } = new CostumeSet(10);
            public CostumeSet Angelo { get; set; } = new CostumeSet(3);
            public CostumeSet Red { get; set; } = new CostumeSet(4);
            public CostumeSet Morrie { get; set; } = new CostumeSet(2);

            public class CostumeSet
            {
                public CostumeSet(int count)
                {
                    Costumes = new string[count];
                }
                public CostumeSet Clone()
                {
                    CostumeSet set = new CostumeSet(Costumes.Length);
                    Costumes.CopyTo(set.Costumes.AsSpan());
                    set.CostumePack = CostumePack;
                    return set;
                }

                public string CostumePack { get; set; }
                public string[] Costumes { get; set; }
            }

            public CosmeticOptions Clone()
            {
                CosmeticOptions cosmeticOptions = new CosmeticOptions();
                cosmeticOptions.Hero    = Hero   .Clone();
                cosmeticOptions.Yangus  = Yangus .Clone();
                cosmeticOptions.Jessica = Jessica.Clone();
                cosmeticOptions.Angelo  = Angelo .Clone();
                cosmeticOptions.Red     = Red    .Clone();
                cosmeticOptions.Morrie  = Morrie .Clone();
                return cosmeticOptions;
            }
        }

        public RandomizerOptions Clone()
        {
            RandomizerOptions options = (RandomizerOptions)MemberwiseClone();

            options.Monsters = Monsters.Clone();
            options.Treasure = Treasure.Clone();
            options.Shopping = Shopping.Clone();
            options.Alchemy = Alchemy.Clone();
            options.Cosmetics = Cosmetics.Clone();

            return options;
        }

        public void Save(string path)
        {
            Version = "Development Version";
            Extensions.WriteAllText(path, JsonSerializer.Serialize(this));
        }
        public static RandomizerOptions Load(string path)
        {
            RandomizerOptions options = JsonSerializer.Deserialize<RandomizerOptions>(File.ReadAllText(path));
            options.Version = new RandomizerOptions().Version;
            return options;
        }
    }
}
