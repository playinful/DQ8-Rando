using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Printing.IndexedProperties;
using System.Windows.Controls.Primitives;
using DQ8Rando3DS.Info;

namespace DQ8Rando3DS.Tables
{
    public class CasinoItemTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public CasinoItemTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + CasinoInventory.ByteLength <= Data.Length; i += CasinoInventory.ByteLength)
            {
                CasinoInventory casino = new CasinoInventory(Data, i);
                Contents.Add(casino.ID, casino);
            }
        }
        public static CasinoItemTable Load(string path)
        {
            return new CasinoItemTable(File.ReadAllBytes(path));
        }
        public static CasinoItemTable Load()
        {
            return Load("./Raw/item_casino.tbl");
        }
        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Saved casino item table to `{path}`.");
        }

        public void CreateSpoilerLog(string path)
        {
            List<string> spoilerLog = new List<string>();

            foreach (CasinoInventory casino in Contents.Values)
            {
                if (casino.ID == 0)
                    spoilerLog.Add("Pickham Casino");
                if (casino.ID == 1)
                    spoilerLog.Add("Baccarat Casino");
                spoilerLog.Add(string.Empty.PadLeft(32, '-'));

                foreach (CasinoItem item in casino.Items.Where(i => !i.IsEmpty()))
                {
                    spoilerLog.Add($"{ItemInfo.Get(item.Item).Name} ({item.Price} token{(item.Price == 1 ? "" : "s")})");
                }

                if (casino.ID == 0)
                    spoilerLog.Add("");
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, spoilerLog));
            MainWindow.UpdateStatus($"Wrote casino spoiler log to `{path}`.");
        }

        public Dictionary<byte, CasinoInventory> Contents { get; set; } = new Dictionary<byte, CasinoInventory>();
    }
    public class CasinoInventory
    {
        public const int ByteLength = 0x4C;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset+ByteLength));

        public CasinoInventory(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;

            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = new CasinoItem(_Data, Offset + 4 + (CasinoItem.ItemLength * i), Offset + 4 + (CasinoItem.ItemLength * Items.Length) + (i * CasinoItem.PriceLength));
            }
        }

        public void Clear()
        {
            for (int i = 4; i < Data.Length; i++)
            {
                Data[i] = 0;
            }
        }

        public byte ID { get => Data[0]; set => Data[0] = value; }
        public CasinoItem[] Items { get; set; } = new CasinoItem[12];
        
    }
    public class CasinoItem
    {
        public const int ItemLength = 0x2;
        public const int PriceLength = 0x4;

        private byte[] _Data { get; set; }
        public int ItemOffset { get; set; }
        public int PriceOffset { get; set; }
        public Span<byte> ItemData => _Data.AsSpan(ItemOffset..(ItemOffset+ItemLength));
        public Span<byte> PriceData => _Data.AsSpan(PriceOffset..(PriceOffset+PriceLength));

        public CasinoItem(byte[] data, int itemOffset, int priceOffset)
        {
            _Data = data;
            ItemOffset = itemOffset;
            PriceOffset = priceOffset;
        }

        public ushort Item { get => BitConverter.ToUInt16(ItemData); set => Extensions.SetBytes(ItemData, BitConverter.GetBytes(value)); }
        public int Price { get => BitConverter.ToInt32(PriceData); set => Extensions.SetBytes(PriceData, BitConverter.GetBytes(value)); }

        public bool IsEmpty() => Item == 0;
    }
}
