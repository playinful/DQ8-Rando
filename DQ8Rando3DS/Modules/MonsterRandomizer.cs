using DQ8Rando3DS.Info;
using DQ8Rando3DS.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.SymbolStore;
using System.Xml.Serialization;

namespace DQ8Rando3DS.Modules
{
    class MonsterRandomizer : RandomizerModule
    {
        public MonsterTable MonsterTable { get; set; }
        public EncounterTable EncounterTable { get; set; }
        public SymbolTable SymbolTable { get; set; }
        public SpPartyTable SpPartyTable { get; set; }
        public ScoutMonsterTable ScoutMonsterTable { get; set; }
        public BattleMapTable BattleMapTable { get; set; }

        public byte[] BattleRoadRaw { get; set; }

        private bool CopyMonsterNum { get; set; }

        public override void Start()
        {
            if (!Options.Monsters.MonsterStatsNoChange || Options.Monsters.RandomizeMonsterExperience || Options.Monsters.RandomizeMonsterGold ||
                Options.Monsters.RandomizeMonsterLoot || Options.Monsters.RandomizeMonsterResistances)
                MonsterStep();

            if (!Options.Monsters.OverworldNoChange || !Options.Monsters.BossNoChange || !Options.Monsters.ArenaNoChange || !Options.Monsters.SpecialNoChange || 
                Options.Monsters.ShuffleInfamous)
                EncounterStep();

            Save();
        }

        public void MonsterStep()
        {
            MainWindow.UpdateStatus("Randomizing monsters...");

            MonsterTable = MonsterTable.Load("./Raw/monster.tbl");
        }
        public void EncounterStep()
        {
            MainWindow.UpdateStatus("Randomizing encounters...");

            if (Options.Monsters.OverworldRandomEnemy || Options.Monsters.OverworldRandomEnemyOrBoss ||
                Options.Monsters.BossRandomEnemy || Options.Monsters.BossRandomGroup ||
                Options.Monsters.ArenaRandomEnemy || Options.Monsters.ArenaRandomEnemyOrBoss ||
                Options.Monsters.SpecialRandomEnemy || Options.Monsters.SpecialRandomEnemyOrBoss)
                RandomizeEncounterPools();

            if (Options.Monsters.OverworldShuffleByArea ||
                Options.Monsters.BossShuffle ||
                Options.Monsters.ArenaShuffle ||
                Options.Monsters.SpecialShuffle)
                ShuffleEncounterPools();

            if (Options.Monsters.OverworldShuffle)
                ShuffleOverworldEncounters();

            if (Options.Monsters.ShuffleInfamous)
                ShuffleInfamousMonsters();



            if (EncounterTable is not null)
            {
                MainWindow.UpdateStatus("Cleaning up encounters...");

                FinalizeEncounters();
                UpdateChildPools();
            }

            if (Options.Monsters.ArenaRandomEnemy || Options.Monsters.ArenaRandomEnemyOrBoss)
            {
                MainWindow.UpdateStatus("Randomizing arena models...");

                RandomizeArenaModels();
            }

            if (Options.Monsters.ShuffleBattlebacks)
            {
                MainWindow.UpdateStatus("Shuffling battle backgrounds...");

                ShuffleBattlebacks();
            }
        }

        public void RandomizeEncounterPools()
        {
            EncounterTable = EncounterTable.Load();

            List<EncounterPool> pools = new List<EncounterPool>();
            if (Options.Monsters.OverworldRandomEnemy || Options.Monsters.OverworldRandomEnemyOrBoss)
                pools.AddRange(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && !pool.GetInfo().IsBoss && !pool.GetInfo().IsArena && !pool.GetInfo().IsSpecial));
            if (Options.Monsters.BossRandomEnemy || Options.Monsters.BossRandomGroup)
                pools.AddRange(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsBoss));
            if (Options.Monsters.ArenaRandomEnemy || Options.Monsters.ArenaRandomEnemyOrBoss)
                pools.AddRange(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsArena));
            if (Options.Monsters.ArenaRandomEnemy || Options.Monsters.ArenaRandomEnemyOrBoss)
                pools.AddRange(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsSpecial));

            pools.ForEach(p => p.Clear());

            // TODO bestiary step

            foreach (EncounterPool pool in pools)
            {
                int symbolCount = 0;
                int partyCount = 0;
                if (pool.GetInfo().IsBoss)
                {
                    if (Options.Monsters.BossRandomEnemy)
                        symbolCount = 1;
                    else
                        symbolCount = RNG.Next(1, 5);
                }
                else if (pool.GetInfo().IsArena)
                {
                    if (Options.Monsters.ArenaRandomEnemyOrBoss && RNG.Chance(1/5))
                        partyCount = 1;
                    else
                        symbolCount = 3;
                }
                else if (pool.GetInfo().IsSpecial)
                {
                    symbolCount = 1;
                }
                else
                {
                    int totalCount = RNG.Next(1, 7) + RNG.Next(7);
                    while (totalCount > symbolCount + partyCount)
                    {
                        if (symbolCount >= 10)
                            partyCount = totalCount - symbolCount;
                        else if (partyCount >= 2)
                            symbolCount = totalCount - partyCount;
                        else if (RNG.Next(6) == 0)
                            partyCount++;
                        else
                            symbolCount++;
                    }
                }

                for (int i = 0; i < symbolCount; i++)
                {
                    pool.PushSymbol(GetRandomSymbol(pool), GetRandomWeight(), GetRandomWeight());
                }
                for (int i = 0; i < partyCount; i++)
                {
                    pool.PushParty(GetRandomParty(pool), 7);
                }
            }
        }
        public void ShuffleEncounterPools()
        {
            if (Options.Monsters.OverworldShuffleByArea)
                GenericShuffle(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && !pool.GetInfo().IsBoss && !pool.GetInfo().IsArena && !pool.GetInfo().IsSpecial));
            if (Options.Monsters.ArenaShuffle)
                GenericShuffle(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsArena));
            if (Options.Monsters.SpecialShuffle)
                GenericShuffle(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsSpecial));
            
            if (Options.Monsters.BossShuffle)
            {
                List<EncounterPool> bossPools = EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsBoss).ToList();
                List<EncounterPool> postPools = EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsBoss && pool.GetInfo().IsPostgame).ToList();
                List<EncounterPool> memoPools = EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsBoss && pool.GetInfo().IsMemoriam).ToList();

                if (Options.Monsters.MixInPostgame)
                {
                    bossPools.AddRange(postPools);
                    postPools.Clear();
                }
                if (Options.Monsters.MixInMemoriam)
                {
                    bossPools.AddRange(memoPools);
                    memoPools.Clear();
                }

                GenericShuffle(bossPools);
                GenericShuffle(postPools);
                GenericShuffle(memoPools);
            }
        }
        public void ShuffleOverworldEncounters()
        {
            EncounterTable ??= EncounterTable.Load();

            List<EncounterPool> pools = EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && !pool.GetInfo().IsBoss && !pool.GetInfo().IsArena && !pool.GetInfo().IsSpecial).ToList();
            
            List<Tuple<ushort, byte, byte>> symbols = new List<Tuple<ushort, byte, byte>>();
            List<Tuple<byte, byte>> parties = new List<Tuple<byte, byte>>();
            foreach (EncounterPool pool in pools)
            {
                foreach (EncounterPoolSymbol symbol in pool.Symbols.Where(sym => !sym.IsEmpty()))
                    symbols.Add(new Tuple<ushort, byte, byte>(symbol.ID, symbol.Weight1, symbol.Weight2));
                foreach (EncounterPoolParty party in pool.Parties.Where(par => !par.IsEmpty()))
                    parties.Add(new Tuple<byte, byte>(party.ID, party.Weight));

                pool.Clear();
            }

            symbols = symbols.Shuffle(RNG).ToList();
            parties = parties.Shuffle(RNG).ToList();

            // make sure each table contains at least one encounter
            foreach (EncounterPool pool in pools)
            {
                if (symbols.Count <= 0 || RNG.Next(6) == 0)
                {
                    pool.PushParty(parties[0].Item1, parties[0].Item2);
                    parties.RemoveAt(0);
                }
                else
                {
                    pool.PushSymbol(symbols[0].Item1, symbols[0].Item2, symbols[0].Item3);
                    symbols.RemoveAt(0);
                }
            }

            // distribute remaining encounters
            while (symbols.Count > 0)
            {
                EncounterPool pool = RNG.Choice(pools);

                if (pool.PushSymbol(symbols[0].Item1, symbols[0].Item2, symbols[0].Item3))
                    symbols.RemoveAt(0);
            }
            while (parties.Count > 0)
            {
                EncounterPool pool = RNG.Choice(pools);

                if (pool.PushParty(parties[0].Item1, parties[0].Item2))
                    parties.RemoveAt(0);
            }
        }
        public void GenericShuffle(IEnumerable<EncounterPool> pools)
        {
            for (int i = pools.Count() - 1; i > 0; i--)
            {
                EncounterPool source = pools.ElementAt(i);
                EncounterPool target = pools.ElementAt(RNG.Next(i+1));

                if (source != target)
                    source.SwapWith(target);
            }
        }

        public ushort GetRandomSymbol(EncounterPool pool)
        {
            SymbolTable ??= SymbolTable.Load();

            return RNG.Choice(SymbolTable.Contents.Values, symbol => SymbolIsValid(symbol, pool)).ID;
        }
        public byte GetRandomParty(EncounterPool pool)
        {
            SymbolTable ??= SymbolTable.Load();
            SpPartyTable ??= SpPartyTable.Load();

            return RNG.Choice(SpPartyTable.Contents.Values, party => PartyIsValid(party, pool)).ID;
        }
        public byte GetRandomWeight()
        {
            return (byte)RNG.Next(1, 12); // TODO
        }
        public bool SymbolIsValid(Symbol symbol, EncounterPool pool, bool isParty = false)
        {
            SymbolInfo symbolInfo = symbol.GetInfo();
            if (!symbolInfo.CanSpawn)
                return false;

            MonsterInfo monsterInfo = symbol.GetMonsterInfo();
            if (!monsterInfo.CanSpawn)
                return false;

            EncounterPoolInfo poolInfo = pool.GetInfo();

            if (monsterInfo.IsBoss)
            {
                if (!poolInfo.IsBoss && !poolInfo.IsArena && !poolInfo.IsSpecial && !Options.Monsters.OverworldRandomEnemyOrBoss)
                    return false;
                if (poolInfo.IsArena && (!Options.Monsters.ArenaRandomEnemyOrBoss || !isParty))
                    return false;
                if (poolInfo.IsSpecial && !Options.Monsters.SpecialRandomEnemyOrBoss)
                    return false;
            }
            else if (poolInfo.IsArena && Options.Monsters.ArenaRandomEnemyOrBoss && isParty)
                return false;

            if (monsterInfo.IsPostgame && !poolInfo.IsPostgame && !Options.Monsters.MixInPostgame)
                return false;
            if (monsterInfo.IsMemoriam && !poolInfo.IsMemoriam && !Options.Monsters.MixInMemoriam)
                return false;
            if (monsterInfo.IsArena && !poolInfo.IsArena && !Options.Monsters.MixInArena)
                return false;
            if (monsterInfo.IsSpecial && !poolInfo.IsSpecial && !Options.Monsters.MixInSpecial)
                return false;

            if (monsterInfo.IsInfamous && !Options.Monsters.MixInInfamous)
                return false;
            if (monsterInfo.IsExtra && !Options.Monsters.MixInExtra)
                return false;

            return true;
        }
        public bool PartyIsValid(SpParty party, EncounterPool pool)
        {
            if (!party.GetInfo().CanSpawn) return false;

            return party.Members.All(m =>
            {
                if (m.Symbol == 0) return true;
                
                return SymbolIsValid(SymbolTable.Contents[m.Symbol], pool, isParty:true);
            });
        }

        public void ShuffleInfamousMonsters()
        {
            ScoutMonsterTable = ScoutMonsterTable.Load("./Raw/scoutMonster.tbl");
            ScoutMonster[] monsters = ScoutMonsterTable.Contents.Values.Where(mon => mon.GetInfo().DoShuffle).ToArray();

            for (int i = monsters.Length - 1; i > 0; i--)
            {
                ScoutMonster source = monsters[i];
                ScoutMonster target = monsters[RNG.Next(i + 1)];

                (source.Monster, target.Monster) = (target.Monster, source.Monster);
            }
            // STUDY spawn requirements
        }

        public void FinalizeEncounters()
        {
            if (EncounterTable is null)
                return;

            int x = 0;
            foreach (EncounterPool pool in EncounterTable.Contents.Values.Where(p => p.GetInfo().DoEdit))
            {
                if ((pool.GetInfo().IsBoss || pool.GetInfo().IsArena) && pool.SymbolCount() > 0)
                {
                    x++;
                    SpPartyTable ??= SpPartyTable.Load();

                    List<ushort> symbols = new List<ushort>();
                    foreach (EncounterPoolSymbol symbol in pool.Symbols.Where(sym => !sym.IsEmpty()))
                    {
                        symbols.Add(symbol.ID);
                        symbols.Add(0xB); // 1 monster
                    }

                    pool.Clear();
                    pool.PushParty(SpPartyTable.AddParty(symbols).ID, 0x7);
                }
                if (pool.GetInfo().IsArena && (Options.Monsters.ArenaRandomEnemy || Options.Monsters.ArenaRandomEnemyOrBoss))
                {
                    SpPartyTable ??= SpPartyTable.Load();

                    SpParty party = SpPartyTable.Contents[pool.Parties[0].ID];

                    if (party.SymbolCount() < 3)
                    {
                        int count = party.SymbolCount();

                        if (!party.Edited)
                        {
                            SpParty newParty = SpPartyTable.AddParty(new List<ushort>());
                            party.CopyTo(newParty);
                            pool.Parties[0].ID = newParty.ID;
                            party = newParty;
                        }

                        foreach (SpPartyMember member in party.Members[..3])
                        {
                            if (member.IsEmpty())
                            {
                                MonsterTable ??= MonsterTable.Load();
                                SymbolTable ??= SymbolTable.Load();
                                member.Symbol = 0x126;
                                member.Number = 0xC;
                                CopyMonsterNum = true;
                            }
                        }

                        if (count == 2)
                            (party.Members[1].Symbol, party.Members[1].Number, party.Members[2].Symbol, party.Members[2].Number) = (party.Members[2].Symbol, party.Members[2].Number, party.Members[1].Symbol, party.Members[1].Number);
                        if (count == 1)
                            (party.Members[1].Symbol, party.Members[1].Number, party.Members[0].Symbol, party.Members[0].Number) = (party.Members[0].Symbol, party.Members[0].Number, party.Members[1].Symbol, party.Members[1].Number);
                    }
                }
                if (pool.GetInfo().IsSpecial && pool.SymbolCount() > 0)
                {
                    SymbolTable ??= SymbolTable.Load();

                    ushort targetSymId = (ushort)pool.GetInfo().SpecialSymbol;
                    Symbol targetSymbol = SymbolTable.Contents[targetSymId];
                    Symbol sourceSymbol = SymbolTable.Contents[pool.Symbols[0].ID];

                    sourceSymbol.CopyTo(targetSymbol);
                }
            }
        }
        public void UpdateChildPools()
        {
            foreach (EncounterPool pool in EncounterTable.Contents.Values)
            {
                if (pool.GetInfo().Parent is not null)
                {
                    EncounterTable.Contents[(ushort)pool.GetInfo().Parent].CopyTo(pool);
                }
            }
        }

        public void RandomizeArenaModels()
        {
            List<string> models = new Regex("[\n\r]+").Split(File.ReadAllText("./Json/ArenaModels.txt")).ToList();
            models = models.Shuffle(RNG).ToList();

            BattleRoadRaw = File.ReadAllBytes("./Raw/btl_road.tbl");
            List<byte> brr = BattleRoadRaw.ToList();
            for (int i = 0; i < brr.Count; i++)
            {
                if (brr[i] == 0xFF)
                {
                    brr.RemoveAt(i);
                    brr.InsertRange(i, Encoding.UTF8.GetBytes(models[0]));
                    models.RemoveAt(0);
                }
            }
            BattleRoadRaw = brr.ToArray();
        }
        public void ShuffleBattlebacks()
        {
            BattleMapTable = BattleMapTable.Load();

            IEnumerable<BattleMap> maps = BattleMapTable.Contents.Values.Where(map => map.ID != 0);

            for (int i = maps.Count() - 1; i > 0; i--)
            {
                BattleMap source = maps.ElementAt(i);
                BattleMap target = maps.ElementAt(RNG.Next(i+1));

                if (source != target)
                    source.SwapWith(target);
            }
        }

        public void Save()
        {
            MonsterTable?.Save($"{Options.Path}/romfs/data/Params/monster.tbl");
            MonsterTable?.CreateSpoilerLog($"{Options.Path}/spoiler/Monsters.txt");

            SymbolTable?.Save($"{Options.Path}/romfs/data/Params/symbol.tbl");
            if (SpPartyTable is not null && !SpPartyTable.Data.SequenceEqual(SpPartyTable.Load("./Raw/spParty.tbl").Data))
                SpPartyTable?.Save($"{Options.Path}/romfs/data/Params/spParty.tbl");

            EncounterTable?.Save($"{Options.Path}/romfs/data/Params/encount.tbl");
            EncounterTable?.CreateSpoilerLog($"{Options.Path}/spoiler/Encounters.txt", SymbolTable, SpPartyTable);

            ScoutMonsterTable?.Save($"{Options.Path}/romfs/data/Params/scoutMonster.tbl");
            ScoutMonsterTable?.CreateSpoilerLog($"{Options.Path}/spoiler/ScoutMonster.txt");

            BattleMapTable?.Save($"{Options.Path}/romfs/data/Battle/battlemaplist.tbl");

            if (BattleRoadRaw is not null)
            {
                Extensions.WriteAllBytes($"{Options.Path}/romfs/data/Params/btl_road.tbl", BattleRoadRaw);
                MainWindow.UpdateStatus($"Saved arena NPC table to `{Options.Path}/romfs/data/Params/btl_road.tbl`.");
            }

            if (CopyMonsterNum)
            {
                Extensions.Copy("./Raw/monsterNum.tbl", $"{Options.Path}/romfs/data/Params/monsterNum.tbl", overwrite:true);
                MainWindow.UpdateStatus($"Copied monster number table to `{Options.Path}/romfs/data/Params/monsterNum.tbl`.");
            }
        }
    }
}
