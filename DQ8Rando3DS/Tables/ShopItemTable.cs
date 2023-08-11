using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DQ8Rando3DS.Info;

namespace DQ8Rando3DS.Tables
{
    public class ShopItemTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public ShopItemTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + ShopInventory.ByteLength <= Data.Length; i += ShopInventory.ByteLength)
            {
                ShopInventory inventory = new ShopInventory(Data, i);
                Contents.Add(inventory.ID, inventory);
            }
        }
        public static ShopItemTable Load(string path)
        {
            return new ShopItemTable(File.ReadAllBytes(path));
        }
        public static ShopItemTable Load()
        {
            return Load("./Raw/shopItem.tbl");
        }
        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Wrote shop item spoiler log to `{path}`.");
        }
        public ShopItemTable Clone()
        {
            return new ShopItemTable(Data[..]);
        }

        public void CreateSpoilerLog(string path)
        {
            List<string> spoilerLog = new List<string>();

            foreach (ShopInventory shop in Contents.Values.Where(sh => sh.GetInfo().DoEdit && sh.GetInfo().Parent is null))
            {
                string spoiler = shop.GetInfo().Name;

                if (shop.Markup < 100)
                    spoiler += $" ({100-shop.Markup}% discount)";
                if (shop.Markup > 100)
                    spoiler += $" ({shop.Markup-100}% markup)";

                List<string> items = new List<string>();
                foreach (ShopInventoryItem item in shop.Items.Where(i => i.Item != 0))
                {
                    items.Add(ItemInfo.Get(item.Item).Name);
                }

                spoiler += ": " + string.Join(", ", items);

                spoilerLog.Add(spoiler);
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, spoilerLog));
        }

        public Dictionary<byte, ShopInventory> Contents { get; set; } = new Dictionary<byte, ShopInventory>();
    }
    public class ShopInventory
    {
        public const int ByteLength = 0x30;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset+ByteLength));

        public ShopInventory(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;

            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = new ShopInventoryItem(_Data, Offset + 8 + (i * ShopInventoryItem.ByteLength));
            }
        }

        public void Clear()
        {
            for (int i = 8; i < Data.Length; i++)
            {
                Data[i] = 0;
            }
        }

        public void PushItem(ushort item)
        {
            if (ItemCount() >= 10) return;
            Items.First(i => i.IsEmpty()).Item = item;
        }

        public byte ID { get => Data[0]; set => Data[0] = value; }
        public int Markup { get => BitConverter.ToInt32(Data[4..8]); set => Extensions.SetBytes(Data[4..8], BitConverter.GetBytes(value)); }
        public ShopInventoryItem[] Items { get; set; } = new ShopInventoryItem[10];

        public Info.ShopInfo GetInfo() => ShopInfo.Get(ID);
        public int ItemCount() => Items.Count(x => !x.IsEmpty());
    }
    public class ShopInventoryItem
    {
        public const int ByteLength = 0x4;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset + ByteLength));

        public ShopInventoryItem(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public ushort Item { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }

        public bool IsEmpty() => Item == 0;
    }
}
