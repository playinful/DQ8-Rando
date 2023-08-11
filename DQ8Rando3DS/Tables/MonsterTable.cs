using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Printing.IndexedProperties;

namespace DQ8Rando3DS.Tables
{
    public class MonsterTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public MonsterTable(byte[] data)
        {
            Data = data;
            
            for (int i = HeaderLength; i + Monster.ByteLength <= Data.Length; i += Monster.ByteLength)
            {
                Monster monster = new Monster(Data, i);
                Contents.Add(monster.ID, monster);
            }
        }
        public MonsterTable Clone()
        {
            return new MonsterTable(Data[..]);
        }
        public static MonsterTable Load(string path)
        {
            return new MonsterTable(File.ReadAllBytes(path));
        }
        public static MonsterTable Load()
        {
            return Load("./Raw/monster.tbl");
        }

        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Saved monster table to `{path}`.");
        }
        public void CreateSpoilerLog(string path)
        {
            // TODO
        }

        public Dictionary<ushort, Monster> Contents { get; set; } = new Dictionary<ushort, Monster>();
    }
    public class Monster
    {
        public const int ByteLength = 0xE0;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset+ByteLength));

        public Monster(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
        public ushort MaximumHP { get => BitConverter.ToUInt16(Data[8..10]); set => Extensions.SetBytes(Data[8..10], BitConverter.GetBytes(value)); }
        public ushort MaximumMP { get => BitConverter.ToUInt16(Data[10..12]); set => Extensions.SetBytes(Data[10..12], BitConverter.GetBytes(value)); }
        public ushort Attack { get => BitConverter.ToUInt16(Data[12..14]); set => Extensions.SetBytes(Data[12..14], BitConverter.GetBytes(value)); }
        public ushort Defence { get => BitConverter.ToUInt16(Data[14..16]); set => Extensions.SetBytes(Data[14..16], BitConverter.GetBytes(value)); }
        public ushort Agility { get => BitConverter.ToUInt16(Data[16..18]); set {
                Extensions.SetBytes(Data[16..18], BitConverter.GetBytes(value));
                Extensions.SetBytes(Data[18..20], BitConverter.GetBytes(value));
            }
        }
        public ushort Wisdom => Agility;

        public int Experience { get => BitConverter.ToInt32(Data[0x14..0x18]); set => Extensions.SetBytes(Data[0x14..0x18], BitConverter.GetBytes(value)); }
        public int Gold { get => BitConverter.ToInt32(Data[0x18..0x1C]); set => Extensions.SetBytes(Data[0x18..0x1C], BitConverter.GetBytes(value)); }
        public ushort Item1 { get => BitConverter.ToUInt16(Data[0x1C..0x1E]); set => Extensions.SetBytes(Data[0x1C..0x1E], BitConverter.GetBytes(value)); }
        public ushort Item2 { get => BitConverter.ToUInt16(Data[0x1E..0x20]); set => Extensions.SetBytes(Data[0x1E..0x20], BitConverter.GetBytes(value)); }

        public Span<byte> Resistances => Data[0x33..0x49];
        public void SetResistance(int index, byte res)
        {
            if (index < 0 || index >= 22)
                return;

            Data[0x33 + index] = res;
        }
        public byte GetResistance(int index)
        {
            if (index < 0 || index >= 22)
                return 255;

            return Data[0x33 + index];
        }

        public void SetAction(int index, ushort act)
        {
            if (index < 0 || index >= 6)
                return;

            Extensions.SetBytes(Data[(0x3E + (index * 2))..(0x40 + (index * 2))], BitConverter.GetBytes(act));
        }
        public ushort GetAction(int index)
        {
            if (index < 0 || index >= 6)
                return 0xFFFF;

            return BitConverter.ToUInt16(Data[(0x3E+(index*2))..(0x40+(index*2))]);
        }
    }
}
