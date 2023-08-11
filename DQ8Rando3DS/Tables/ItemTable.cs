using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DQ8Rando3DS.Info;
using System.Security.Cryptography.X509Certificates;

namespace DQ8Rando3DS.Tables
{
    public class ItemTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public ItemTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + ItemData.ByteLength <= Data.Length; i += ItemData.ByteLength)
            {
                ItemData item = new ItemData(Data, i);
                Contents.Add(item.ID, item);
            }
        }
        public static ItemTable Load(string path)
        {
            return new ItemTable(File.ReadAllBytes(path));
        }
        public static ItemTable Load()
        {
            return Load("./Raw/item.tbl");
        }
        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Saved item table to `{path}`.");
        }

        public void CreatePriceSpoilerLog(string path)
        {
            List<string> spoilerLog = new List<string>();

            foreach (ItemData item in Contents.Values.Where(i => i.GetInfo().Type == ItemInfo.ItemType.Item))
            {
                spoilerLog.Add($"{item.GetInfo().Name}: {item.BuyPrice} G");
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, spoilerLog));
            MainWindow.UpdateStatus($"Wrote item price spoiler log to `{path}`.");
        }

        public Dictionary<ushort, ItemData> Contents { get; set; } = new Dictionary<ushort, ItemData>();
    }
    public class ItemData
    {
        public const int ByteLength = 0x84;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset+ByteLength));

        public ItemData(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
        public int BuyPrice { get => BitConverter.ToInt32(Data[0xC..0x10]); set => Extensions.SetBytes(Data[0xC..0x10], BitConverter.GetBytes(value)); }
        public int SellPrice { get => BitConverter.ToInt32(Data[0x10..0x14]); set => Extensions.SetBytes(Data[0x10..0x14], BitConverter.GetBytes(value)); }
        public byte EquippableBy { get => Data[0x1C]; set => Data[0x1C] = value; }
        public byte Attack { get => Data[0x1F]; set => Data[0x1F] = value; }
        public byte Defence { get => Data[0x20]; set => Data[0x20] = value; }
        public byte Agility { get => Data[0x21]; set => Data[0x21] = value; }
        public byte Wisdom { get => Data[0x22]; set => Data[0x22] = value; }

        public ItemInfo GetInfo() => ItemInfo.Get(ID);

        public enum EquipCharacter: byte
        {
            None = 0,
            Hero = 1,
            Yangus = 2,
            Jessica = 4,
            Angelo = 8,
            Red = 32,
            Morrie = 128
        }
    }
}
