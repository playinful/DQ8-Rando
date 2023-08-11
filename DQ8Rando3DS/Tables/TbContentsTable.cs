using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DQ8Rando3DS.Tables
{
    public class TbContentsTable
    {
        public const int HeaderLength = 0x4;

        public TbContentsTable(byte[] data) 
        {
            Data = data;

            for (int i = HeaderLength; i + BlueChestPool.ByteLength < data.Length; i += BlueChestPool.ByteLength)
            {
                BlueChestPool pool = new BlueChestPool(data, i);
                Contents.Add(pool.ID, pool);
            }
        }
        public static TbContentsTable Load(string path)
        {
            return new TbContentsTable(File.ReadAllBytes(path));
        }
        public static TbContentsTable Load()
        {
            return Load("./Raw/tbContents.tbl");
        }
        public TbContentsTable Clone()
        {
            return new TbContentsTable(Data[..]);
        }

        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Wrote blue chest pool table to `{path}`.");
        }

        public byte[] Data { get; set; }
        public Dictionary<ushort, BlueChestPool> Contents { get; set; } = new Dictionary<ushort, BlueChestPool>();
    }
    public class BlueChestPool
    {
        public const int ByteLength = 0x44;
        public const int HeaderLength = 0x4;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data { get => _Data.AsSpan(Offset..(Offset + ByteLength)); }

        public BlueChestPool(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;

            for (int i = 0; i < 8; i++)
            {
                Items[i] = new BlueChestItem(_Data, Offset + HeaderLength + (i * BlueChestItem.ByteLength));
            }
        }

        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
        public BlueChestItem[] Items { get; set; } = new BlueChestItem[8];
    }
    public class BlueChestItem
    {
        public const int ByteLength = 0x8;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data { get => _Data.AsSpan(Offset..(Offset + ByteLength)); }

        public BlueChestItem(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public ushort Item { get => BitConverter.ToUInt16(Data[0x0..0x2]); set => Extensions.SetBytes(Data[0x0..0x2], BitConverter.GetBytes(value)); }
        public short Value { get => BitConverter.ToInt16(Data[0x2..0x4]); set => Extensions.SetBytes(Data[0x2..0x4], BitConverter.GetBytes(value)); }
        public int Footer { get => BitConverter.ToInt32(Data[0x4..0x8]); set => Extensions.SetBytes(Data[0x4..0x8], BitConverter.GetBytes(value)); }
    }
}
