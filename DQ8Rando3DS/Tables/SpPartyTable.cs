using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.CodeDom;
using DQ8Rando3DS.Info;

namespace DQ8Rando3DS.Tables
{
    public class SpPartyTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public SpPartyTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + SpParty.ByteLength <= Data.Length; i += SpParty.ByteLength)
            {
                SpParty party = new SpParty(Data, i);
                Contents.Add(party.ID, party);
            }
        }
        public SpPartyTable Clone()
        {
            return new SpPartyTable(Data[..]);
        }
        public static SpPartyTable Load(string path)
        {
            return new SpPartyTable(File.ReadAllBytes(path));
        }
        public static SpPartyTable Load()
        {
            return Load("./Raw/spParty.tbl");
        }

        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Wrote group encounter table to `{path}`.");
        }

        public Dictionary<byte, SpParty> Contents { get; set; } = new Dictionary<byte, SpParty>();
        public SpParty AddParty(List<ushort> encounters)
        {
            SpParty newParty = null;
            foreach (SpParty party in Contents.Values)
            {
                if (!party.Edited && party.GetInfo() is not null && party.GetInfo().DoOverwrite)
                {
                    newParty = party;
                    break;
                }
            }

            if (newParty is null)
            {
                byte id = Contents.Values.Last().ID;
                if (id >= 255)
                    return Contents.Values.Last();

                Data = Data.ToList().Concat(new byte[SpParty.ByteLength]).ToArray();
                newParty = new SpParty(Data, Data.Length - SpParty.ByteLength);
                newParty.ID = (byte)(id + 1);
                Contents.Add(newParty.ID, newParty);
            }

            for (int i = 0; i < encounters.Count / 2; i++)
            {
                newParty.Members[i].Symbol = encounters[i * 2];
                newParty.Members[i].Number = encounters[i * 2 + 1];
            }

            newParty.Edited = true;
            return newParty;
        }
    }
    public class SpParty
    {
        public const int ByteLength = 0x24;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset + ByteLength));

        public SpParty(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;

            for (int i = 0; i < Members.Length; i++)
            {
                Members[i] = new SpPartyMember(_Data, Offset + 4 + (i * SpPartyMember.ByteLength));
            }
        }

        public void CopyTo(SpParty target)
        {
            for (int i = 2; i < Data.Length; i++)
            {
                target.Data[i] = Data[i];
            }
        }

        public byte ID { get => Data[0]; set => Data[0] = value; }
        public SpPartyMember[] Members { get; set; } = new SpPartyMember[4];
        public bool Edited { get; set; } = false;

        public Info.SpPartyInfo GetInfo() => SpPartyInfo.Get(ID);
        //public bool IsBoss() => Members.Any(m => m.GetMonsterInfo() is not null && m.GetMonsterInfo().IsBoss);

        public int SymbolCount() => Members.Count(m => m.IsEmpty());
    }
    public class SpPartyMember
    {
        public const int ByteLength = 8;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset + ByteLength));

        public SpPartyMember(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public ushort Symbol
        {
            get => BitConverter.ToUInt16(Data[0..2]);
            set
            {
                if (value == 0)
                    Extensions.SetBytes(Data[4..8], new byte[] { 0, 0, 0, 0 });
                else
                    Extensions.SetBytes(Data[4..8], new byte[] { 0, 0, 0x80, 0x3F });

                Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value));
            }
        }
        public ushort Number { get => BitConverter.ToUInt16(Data[2..4]); set => Extensions.SetBytes(Data[2..4], BitConverter.GetBytes(value)); }

        public SymbolInfo GetSymbolInfo() => SymbolInfo.Data.ContainsKey(Symbol) ? SymbolInfo.Get(Symbol) : null;
        //public MonsterInfo GetMonsterInfo() => GetSymbolInfo() is not null ? GetSymbolInfo().GetMonsterInfo() : null;

        public bool IsEmpty() => Symbol <= 0;
    }
}