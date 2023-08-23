using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DQ8Rando3DS.Info;

namespace DQ8Rando3DS.Tables
{
    public class EncounterTable
    {
        public const int HeaderLength = 0x10;
        public byte[] Data { get; set; }

        public EncounterTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + EncounterPool.ByteLength <= Data.Length; i += EncounterPool.ByteLength)
            {
                EncounterPool pool = new EncounterPool(Data, i);
                Contents.Add(pool.ID, pool);
            }
        }
        public static EncounterTable Load(string path)
        {
            return new EncounterTable(File.ReadAllBytes(path));
        }
        public static EncounterTable Load()
        {
            return Load("./Raw/encount.tbl");
        }
        public EncounterTable Clone()
        {
            return new EncounterTable(Data[..]);
        }

        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Saved encounter table to `{path}`.");
        }
        public void CreateSpoilerLog(string path, SymbolTable symbolTable = null, SpPartyTable spPartyTable = null)
        {
            symbolTable ??= SymbolTable.Load();
            spPartyTable ??= SpPartyTable.Load();

            List<string> spoilerLog = new List<string>();

            foreach (EncounterPool pool in Contents.Values.OrderBy(p => p.GetInfo().SortIndex).ThenBy(p => p.ID))
            {
                if (pool.IsEmpty() || !pool.GetInfo().DoEdit || pool.GetInfo().Parent is not null)
                    continue;

                string spoiler = pool.GetInfo().Name;

                List<ushort> symbols = new List<ushort>();
                foreach (EncounterPoolSymbol symbol in pool.Symbols)
                {
                    if (!symbol.IsEmpty() && !symbols.Contains(symbol.ID))
                        symbols.Add(symbol.ID);
                }
                foreach (EncounterPoolParty eParty in pool.Parties)
                {
                    SpParty party = spPartyTable.Contents[eParty.ID];
                    foreach (SpPartyMember member in party.Members.Where(mem => !mem.IsEmpty()))
                    {
                        if (!symbols.Contains(member.Symbol))
                            symbols.Add(member.Symbol);
                    }
                }

                List<MonsterInfo> monsters = new List<MonsterInfo>();
                foreach (ushort symbol in symbols)
                {
                    MonsterInfo monster = MonsterInfo.Get(symbolTable.Contents[symbol].Monster);
                    if (!monsters.Contains(monster))
                        monsters.Add(monster);
                }

                monsters = monsters.OrderBy(m => m.ID).ToList();

                List<string> monsterNames = new List<string>();
                foreach (MonsterInfo monster in monsters)
                {
                    if (!monsterNames.Contains(monster.Name) && monster.Name != "none" && monster.Name != "")
                        monsterNames.Add(monster.Name);
                }

                spoiler += ": " + string.Join(", ", monsterNames);

                spoilerLog.Add(spoiler);
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, spoilerLog));
            MainWindow.UpdateStatus($"Created encounter spoiler log at `{path}`.");
        }

        public Dictionary<ushort, EncounterPool> Contents { get; set; } = new Dictionary<ushort, EncounterPool>();
    }
    public class EncounterPool
    {
        public const int ByteLength = 0x60;
        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data { get => _Data.AsSpan(Offset..(Offset+ByteLength)); }

        public EncounterPool(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;

            for (int i = 0; i < Symbols.Length; i++)
            {
                Symbols[i] = new EncounterPoolSymbol(_Data, Offset + 0xC + (EncounterPoolSymbol.ByteLength * i));
            }
            for (int i = 0; i < Parties.Length; i++)
            {
                Parties[i] = new EncounterPoolParty(_Data, Offset + 0x5C + (EncounterPoolParty.ByteLength * i));
            }
        }
        public EncounterPool Clone()
        {
            return new EncounterPool(Data[..].ToArray(), 0);
        }

        public void CopyTo(EncounterPool target)
        {
            for (int i = 0xC; i < Data.Length; i++)
            {
                target.Data[i] = Data[i];
            }
        }
        public void SwapWith(EncounterPool target)
        {
            for (int i = 0xC; i < Data.Length; i++)
            {
                (target.Data[i], Data[i]) = (Data[i], target.Data[i]);
            }
        }
        public void Clear()
        {
            for (int i = 0xC; i < Data.Length; i++)
            {
                Data[i] = 0;
            }
        }
        public bool PushSymbol(ushort id, byte weight1 = 1, byte weight2 = 1)
        {
            if (SymbolCount() >= 10)
                return false;

            EncounterPoolSymbol encounter = Symbols.First(enc => enc.IsEmpty());
            encounter.ID = id;
            encounter.Weight1 = weight1;
            encounter.Weight2 = weight2;
            encounter.Footer = 0x3F800000;
            return true;
        }
        public bool PushSymbol(EncounterPoolSymbol symbol) => PushSymbol(symbol.ID, symbol.Weight1, symbol.Weight2);
        public bool PushParty(byte id, byte weight = 7)
        {
            if (PartyCount() >= 2)
                return false;

            EncounterPoolParty encounter = Parties.First(enc => enc.IsEmpty());
            encounter.ID = id;
            encounter.Weight = weight;
            return true;
        }
        public bool PushParty(EncounterPoolParty party) => PushParty(party.ID, party.Weight);
        public bool IsEmpty()
        {
            return Symbols.All(enc => enc.IsEmpty()) && Parties.All(enc => enc.IsEmpty());
        }

        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
        public EncounterPoolSymbol[] Symbols { get; set; } = new EncounterPoolSymbol[10];
        public EncounterPoolParty[] Parties { get; set; } = new EncounterPoolParty[2];

        public EncounterPoolInfo GetInfo() => EncounterPoolInfo.Get(ID);
        public int SymbolCount() => Symbols.Where(enc => !enc.IsEmpty()).Count();
        public int PartyCount() => Parties.Where(enc => !enc.IsEmpty()).Count();
        public IEnumerable<EncounterPoolSymbol> GetNonEmptySymbols() => Symbols.Where(sym => !sym.IsEmpty());
        public IEnumerable<EncounterPoolParty> GetNonEmptyParties() => Parties.Where(sym => !sym.IsEmpty());
    }
    public class EncounterPoolSymbol
    {
        public const int ByteLength = 0x8;
        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data { get => _Data.AsSpan(Offset..(Offset + ByteLength)); }

        public EncounterPoolSymbol(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }
        public EncounterPoolSymbol Clone()
        {
            return new EncounterPoolSymbol(Data.ToArray(), 0);
        }

        public void CopyTo(EncounterPoolSymbol target)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                target.Data[i] = Data[i];
            }
        }
        public void Clear()
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = 0;
            }
        }
        public bool IsEmpty()
        {
            return Weight1 == 0 && Weight2 == 0 && ID == 0 && Footer == 0;
        }

        public byte Weight1 { get => Data[0]; set => Data[0] = value; }
        public byte Weight2 { get => Data[1]; set => Data[1] = value; }
        public ushort ID { get => BitConverter.ToUInt16(Data[2..4]); set => Extensions.SetBytes(Data[2..4], BitConverter.GetBytes(value)); }
        public int Footer { get => BitConverter.ToInt32(Data[4..8]); set => Extensions.SetBytes(Data[4..8], BitConverter.GetBytes(value)); }

        public SymbolInfo GetInfo() => SymbolInfo.Get(ID);
        //public MonsterInfo GetMonster() => MonsterInfo.Get(SymbolInfo.Get(ID).Monster);
    }
    public class EncounterPoolParty
    {
        public const int ByteLength = 0x2;
        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data { get => _Data.AsSpan(Offset..(Offset + ByteLength)); }

        public EncounterPoolParty(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }
        public EncounterPoolParty Clone()
        {
            return new EncounterPoolParty(Data.ToArray(), 0);
        }

        public void CopyTo(EncounterPoolParty target)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                target.Data[i] = Data[i];
            }
        }
        public void Clear()
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = 0;
            }
        }
        public bool IsEmpty()
        {
            return Weight == 0 && (ID == 0 || ID == 0x86);
        }

        public byte ID { get => Data[1]; set => Data[1] = value; }
        public byte Weight { get => Data[0]; set => Data[0] = value; }

        public SpPartyInfo GetInfo() => SpPartyInfo.Get(ID);
        //public IEnumerable<MonsterInfo> GetMonsters() => SpPartyInfo.Get(ID).Monsters.Select(m => MonsterInfo.Get(m)); //TODO
    }
}
