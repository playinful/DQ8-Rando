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
using System.Windows.Navigation;
using System.CodeDom;

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

        private bool CopyMonsterNum { get; set; } = false;
        private bool CreateMonsterSpoilerLog { get; set; } = false;

        private Dictionary<ushort, double> PowerScalingKey { get; set; } = new();

        private int _RetryCount { get; set; }

        public override void Start()
        {
            MonsterTable ??= MonsterTable.Load();

            EncounterStep();
            MonsterStep();

            Save();
        }

        public void EncounterStep()
        {
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
                if (Options.Monsters.GuaranteeFullBestiary)
                {
                    MainWindow.UpdateStatus("Checking if the bestiary is completable...");

                    if (BestiaryAllWithin())
                    {
                        MainWindow.UpdateStatus("Bestiary check successful.");
                    }
                    else
                    {
                        if (_RetryCount < 5)
                        {
                            MainWindow.UpdateStatus("Found some monsters which can't be defeated. Retrying...");

                            _RetryCount++;
                            EncounterTable = EncounterTable.Load();
                            EncounterStep();
                            return;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }

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
        public void MonsterStep()
        {
            MainWindow.UpdateStatus("Randomizing monsters...");

            if (!Options.Monsters.MonsterStatsNoChange || Options.Monsters.RandomizeMonsterActions ||
                Options.Monsters.RandomizeMonsterExperience || Options.Monsters.RandomizeMonsterGold ||
                Options.Monsters.RandomizeMonsterLoot || Options.Monsters.RandomizeMonsterResistances)
                RandomizeMonsterStats();
        }

        public void RandomizeEncounterPools()
        {
            EncounterTable ??= EncounterTable.Load();

            List<EncounterPool> pools = new();
            if (Options.Monsters.OverworldRandomEnemy || Options.Monsters.OverworldRandomEnemyOrBoss)
                pools.AddRange(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && !pool.GetInfo().IsBoss && !pool.GetInfo().IsArena && !pool.GetInfo().IsSpecial));
            if (Options.Monsters.BossRandomEnemy || Options.Monsters.BossRandomGroup)
                pools.AddRange(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsBoss));
            if (Options.Monsters.ArenaRandomEnemy || Options.Monsters.ArenaRandomEnemyOrBoss)
                pools.AddRange(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsArena));
            if (Options.Monsters.ArenaRandomEnemy || Options.Monsters.ArenaRandomEnemyOrBoss)
                pools.AddRange(EncounterTable.Contents.Values.Where(pool => pool.GetInfo().DoEdit && pool.GetInfo().IsSpecial));

            pools.ForEach(p => p.Clear());

            if (pools.Count <= 0)
                return;

            // TODO bestiary step
            if (Options.Monsters.GuaranteeFullBestiary)
            {
                MainWindow.UpdateStatus("Making sure the bestiary can be completed...");

                IEnumerable<BestiaryInfo> bestiary = GetUnusedBestiary();

                foreach (BestiaryInfo best in bestiary)
                {
                    if (!best.Required || BestiaryWithin(best)) continue;

                    IEnumerable<object> encounters = GetBestiaryEncounters(best);

                    if (!encounters.Any())
                        throw new Exception();

                    bool success = false;

                    foreach (object encounter in encounters.Shuffle(RNG))
                    {
                        IEnumerable<EncounterPool> validPools = Array.Empty<EncounterPool>();

                        if (encounter is Symbol symbol)
                        {
                            validPools = pools.Where(p => CanTakeSymbol(symbol, p) && !p.GetInfo().Missable && !p.GetInfo().IsArena);

                            if (!validPools.Any())
                                continue;

                            success = RNG.Choice(validPools).PushSymbol(symbol.ID, GetRandomWeight(), GetRandomWeight());
                        }
                        if (encounter is SpParty party)
                        {
                            validPools = pools.Where(p => CanTakeParty(party, p) && !p.GetInfo().Missable && !p.GetInfo().IsArena);

                            if (!validPools.Any())
                                continue;

                            success = RNG.Choice(validPools).PushParty(party.ID, 7);
                        }

                        if (success)
                            break;
                    }

                    if (!success)
                    {
                        RandomizeEncounterPools();
                        return; // FIXME really lazy contingency
                    }
                }
            }

            MainWindow.UpdateStatus("Randomizing encounters...");

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

                while (pool.SymbolCount() < symbolCount)
                {
                    pool.PushSymbol(GetRandomSymbol(pool), GetRandomWeight(), GetRandomWeight());
                }
                while (pool.PartyCount() < partyCount)
                {
                    pool.PushParty(GetRandomParty(pool), 7);
                }
            }
        }
        public void ShuffleEncounterPools()
        {
            EncounterTable ??= EncounterTable.Load();
            MainWindow.UpdateStatus("Shuffling encounters...");

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
            
            List<EncounterPoolSymbol> symbols = new();
            List<EncounterPoolParty> parties = new();
            foreach (EncounterPool pool in pools)
            {
                foreach (EncounterPoolSymbol symbol in pool.Symbols.Where(sym => !sym.IsEmpty()))
                    symbols.Add(symbol.Clone());
                foreach (EncounterPoolParty party in pool.Parties.Where(par => !par.IsEmpty()))
                    parties.Add(party.Clone());

                pool.Clear();
            }

            List<object> symbolsAndParties = symbols.Cast<object>().Concat(parties).Shuffle(RNG).ToList();

            // bestiary step
            if (Options.Monsters.GuaranteeFullBestiary && pools.Any(p => !p.GetInfo().Missable))
            {
                MonsterTable ??= MonsterTable.Load();
                IEnumerable<EncounterPool> unmissablePools = pools.Where(pool => !pool.GetInfo().Missable);

                HashSet<BestiaryInfo> bestiary = new();

                foreach (EncounterPoolSymbol symbol in symbols)
                    bestiary.Add(GetMonster(symbol).GetBestiaryInfo());
                foreach (EncounterPoolParty party in parties)
                    foreach (Monster monster in GetMonsters(party))
                        bestiary.Add(monster.GetBestiaryInfo());

                if (bestiary.Contains(null)) bestiary.Remove(null);

                foreach (BestiaryInfo best in bestiary)
                {
                    if (!best.Required || BestiaryWithin(best)) continue;

                    int selIndex = symbolsAndParties.FindIndex(o =>
                    {
                        if (o is EncounterPoolSymbol symbol) return GetMonster(symbol).Bestiary == best.ID;
                        if (o is EncounterPoolParty party) return GetMonsters(party).Any(m => m.Bestiary == best.ID);

                        return false;
                    });
                    object sel = symbolsAndParties[selIndex];

                    while (true)
                    {
                        if (sel is EncounterPoolSymbol symbol)
                        {
                            if (RNG.Choice(unmissablePools, pool => PoolHasRoomForSymbol(pool)).PushSymbol(symbol))
                                break;
                        }
                        else if (sel is EncounterPoolParty party)
                        {
                            if (RNG.Choice(unmissablePools, pool => PoolHasRoomForParty(pool)).PushParty(party))
                                break;
                        }
                        else throw new Exception();
                    }

                    symbolsAndParties.RemoveAt(selIndex);
                }
            }

            // make sure each table contains at least one encounter
            foreach (EncounterPool pool in pools.Where(p => p.IsEmpty()))
            {
                if (symbolsAndParties.First() is EncounterPoolSymbol symbol)
                {
                    pool.PushSymbol(symbol);
                    symbolsAndParties.RemoveAt(0);
                }
                else if (symbolsAndParties.First() is EncounterPoolParty party)
                {
                    pool.PushParty(party);
                    symbolsAndParties.RemoveAt(0);
                }
            }

            // distribute remaining encounters
            while (symbolsAndParties.Count > 0)
            {
                EncounterPool pool = RNG.Choice(pools);

                if (symbolsAndParties.First() is EncounterPoolSymbol symbol)
                {
                    if (pool.PushSymbol(symbol))
                        symbolsAndParties.RemoveAt(0);
                }
                else if (symbolsAndParties.First() is EncounterPoolParty party)
                {
                    if (pool.PushParty(party))
                        symbolsAndParties.RemoveAt(0);
                }
            }
        }
        public void GenericShuffle(IEnumerable<EncounterPool> pools)
        {
            List<EncounterPool> clones = pools.Select(pool => pool.Clone()).ToList();
            List<EncounterPool> remainingPools = pools.ToList();

            if (Options.Monsters.GuaranteeFullBestiary)
            {
                MonsterTable ??= MonsterTable.Load();
                SymbolTable ??= SymbolTable.Load();
                SpPartyTable ??= SpPartyTable.Load();

                HashSet<BestiaryInfo> bestiary = new();
                foreach (EncounterPool pool in pools)
                {
                    foreach (EncounterPoolSymbol symbol in pool.GetNonEmptySymbols())
                    {
                        bestiary.Add(GetMonster(symbol).GetBestiaryInfo());
                    }
                    foreach (EncounterPoolParty party in pool.GetNonEmptyParties())
                    {
                        foreach (Monster monster in GetMonsters(party)) bestiary.Add(monster.GetBestiaryInfo());
                    }

                    pool.Clear();
                }
                if (bestiary.Contains(null)) bestiary.Remove(null);

                foreach (BestiaryInfo best in bestiary)
                {
                    if (!best.Required || BestiaryWithin(best)) continue;

                    EncounterPool source = RNG.Choice(clones, pool => BestiaryWithin(best, pool));
                    EncounterPool target = RNG.Choice(remainingPools, pool => !pool.GetInfo().Missable);

                    source.CopyTo(target);

                    clones.Remove(source);
                    remainingPools.Remove(target);
                }
            }

            foreach (EncounterPool pool in clones.Shuffle(RNG))
            {
                pool.CopyTo(remainingPools[0]);
                remainingPools.RemoveAt(0);
            }

            //for (int i = pools.Count() - 1; i > 0; i--)
            //{
            //    EncounterPool source = pools.ElementAt(i);
            //    EncounterPool target = pools.ElementAt(RNG.Next(i+1));

            //    if (source != target)
            //        source.SwapWith(target);
            //}
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
        public bool PoolHasRoomForSymbol(EncounterPool pool)
        {
            if (pool.SymbolCount() >= 10) return false;

            if (pool.GetInfo().IsBoss)
            {
                if (Options.Monsters.BossRandomEnemy) return pool.SymbolCount() < 1;
                if (Options.Monsters.BossRandomGroup) return pool.SymbolCount() < 4;
                return false;
            }
            if (pool.GetInfo().IsArena)
            {
                return pool.SymbolCount() < 3 && pool.PartyCount() < 1;
            }
            if (pool.GetInfo().IsSpecial)
            {
                return pool.SymbolCount() < 1;
            }

            return pool.SymbolCount() < 10;
        }
        public bool PoolHasRoomForParty(EncounterPool pool)
        {
            if (pool.PartyCount() >= 2) return false;

            if (pool.GetInfo().IsBoss)
            {
                if (Options.Monsters.BossShuffle) return pool.SymbolCount() < 1;
                return false;
            }
            if (pool.GetInfo().IsArena)
            {
                return pool.SymbolCount() < 1 && pool.PartyCount() < 1;
            }
            if (pool.GetInfo().IsSpecial) return false;

            return pool.PartyCount() < 2; // TODO
        }
        public bool CanTakeSymbol(Symbol symbol, EncounterPool pool)
        {
            return SymbolIsValid(symbol, pool) && PoolHasRoomForSymbol(pool);
        }
        public bool CanTakeParty(SpParty party, EncounterPool pool)
        {
            return PartyIsValid(party, pool) && PoolHasRoomForParty(pool);
        }

        public IEnumerable<BestiaryInfo> GetUnusedBestiary()
        {
            return BestiaryInfo.GetRequired().Where(best => !BestiaryWithin(best));
        }
        public bool BestiaryWithin(BestiaryInfo best)
        {
            EncounterTable ??= EncounterTable.Load();
            SymbolTable ??= SymbolTable.Load();
            SpPartyTable ??= SpPartyTable.Load();
            MonsterTable ??= MonsterTable.Load();

            return EncounterTable.Contents.Values.Where(pool => !pool.GetInfo().Missable && pool.GetInfo().Parent is null && !pool.GetInfo().IsArena).Any(pool => BestiaryWithin(best, pool));
        }
        public bool BestiaryWithin(BestiaryInfo best, EncounterPool pool)
        {
            foreach (EncounterPoolSymbol sym in pool.GetNonEmptySymbols())
            {
                if (GetMonster(sym).Bestiary == best.ID) return true;
            }
            foreach (EncounterPoolParty par in pool.GetNonEmptyParties())
            {
                if (GetMonsters(par).Any(m => m.Bestiary == best.ID)) return true;
            }
            return false;
        }
        public bool BestiaryAllWithin()
        {
            return BestiaryInfo.GetRequired().All(best => BestiaryWithin(best));
        }
        public IEnumerable<object> GetBestiaryEncounters(BestiaryInfo best)
        {
            return GetBestiarySymbols(best).Cast<object>().Concat(GetBestiaryParties(best));
        }
        public IEnumerable<Symbol> GetBestiarySymbols(BestiaryInfo best)
        {
            SymbolTable ??= SymbolTable.Load();
            MonsterTable ??= MonsterTable.Load();

            return SymbolTable.Contents.Values.Where(sym => MonsterTable.Contents[sym.Monster].Bestiary == best.ID);
        }
        public IEnumerable<SpParty> GetBestiaryParties(BestiaryInfo best)
        {
            SymbolTable ??= SymbolTable.Load();
            MonsterTable ??= MonsterTable.Load();
            SpPartyTable ??= SpPartyTable.Load();

            return SpPartyTable.Contents.Values.Where(par => par.Members.Any(m => MonsterTable.Contents[SymbolTable.Contents[m.Symbol].Monster].Bestiary == best.ID));
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

        public void RandomizeMonsterStats()
        {
            CreateMonsterSpoilerLog = true;

            foreach (Monster monster in MonsterTable.Contents.Values.Where(mon => mon.GetInfo().DoEdit))
            {
                // MHP, MMP, Attack, Defence, Agility
                if (!Options.Monsters.MonsterStatsNoChange)
                {
                    // TODO
                }

                // Resistances
                if (Options.Monsters.RandomizeMonsterResistances)
                {
                    // TODO
                    if (Options.Monsters.MonsterStatsBalanced)
                    {
                        // TODO
                    }
                    else
                    {

                    }
                }
                // Actions
                if (Options.Monsters.RandomizeMonsterActions)
                {
                    // TODO
                }

                // EXP
                if (Options.Monsters.RandomizeMonsterExperience)
                {
                    // TODO
                }
                // Gold
                if (Options.Monsters.RandomizeMonsterGold)
                {
                    // TODO
                }
                // Loot
                if (Options.Monsters.RandomizeMonsterLoot)
                {
                    // TODO
                }
            }

            UpdateChildMonsters();
        }
        public void UpdateChildMonsters()
        {
            foreach (Monster monster in MonsterTable.Contents.Values.Where(mon => mon.GetInfo().Parent is not null))
            {
                Monster parent = MonsterTable.Contents[(ushort)monster.GetInfo().Parent];

                // Stats
                if (monster.GetInfo().CopyAll || monster.GetInfo().CopyStats)
                {
                    monster.MaximumHP = parent.MaximumHP;
                    monster.MaximumMP = parent.MaximumMP;
                    monster.Attack = parent.Attack;
                    monster.Defence = parent.Defence;
                    monster.Agility = parent.Agility;
                    monster.Evasion = parent.Evasion;

                    for (int i = 0; i < 24; i++)
                        monster.SetResistance(i, parent.GetResistance(i));
                }
                // Loot
                if (monster.GetInfo().CopyAll || monster.GetInfo().CopyLoot)
                {
                    monster.Experience = parent.Experience;
                    monster.Gold = parent.Gold;
                    monster.Item1 = parent.Item1;
                    monster.Item1DropRate = parent.Item1DropRate;
                    monster.Item2 = parent.Item2;
                    monster.Item2DropRate = parent.Item2DropRate;
                }
                // Actions
                if (monster.GetInfo().CopyAll || monster.GetInfo().CopyActions)
                {
                    for (int i = 0; i < 6; i++)
                        monster.SetAction(i, parent.GetAction(i));
                }
            }
        }

        public Monster GetMonster(EncounterPoolSymbol sym)
        {
            SymbolTable ??= SymbolTable.Load();
            MonsterTable ??= MonsterTable.Load();

            return MonsterTable.Contents[SymbolTable.Contents[sym.ID].Monster];
        }
        public List<Monster> GetMonsters(EncounterPoolParty par)
        {
            List<Monster> monsters = new();

            SymbolTable ??= SymbolTable.Load();
            SpPartyTable ??= SpPartyTable.Load();
            MonsterTable ??= MonsterTable.Load();

            foreach (SpPartyMember member in SpPartyTable.Contents[par.ID].Members.Where(mem => !mem.IsEmpty()))
            {
                monsters.Add(MonsterTable.Contents[SymbolTable.Contents[member.Symbol].Monster]);
            }

            return monsters;
        }

        public void Save()
        {
            MonsterTable?.Save($"{Options.Path}/romfs/data/Params/monster.tbl");
            if (CreateMonsterSpoilerLog) MonsterTable?.CreateSpoilerLog($"{Options.Path}/spoiler/Monsters.txt");

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
