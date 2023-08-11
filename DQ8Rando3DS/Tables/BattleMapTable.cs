using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace DQ8Rando3DS.Tables
{
    public class BattleMapTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public BattleMapTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + BattleMap.ByteLength <= Data.Length; i += BattleMap.ByteLength)
            {
                BattleMap map = new BattleMap(Data, i);
                Contents.Add(map.ID, map);
            }
        }
        public static BattleMapTable Load(string path)
        {
            return new BattleMapTable(File.ReadAllBytes(path));
        }
        public static BattleMapTable Load()
        {
            return Load("./Raw/battlemaplist.tbl");
        }

        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Saved battle map list to `{path}`.");
        }

        public Dictionary<ushort, BattleMap> Contents { get; set; } = new Dictionary<ushort, BattleMap>();
    }
    public class BattleMap
    {
        public const int ByteLength = 0x80;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset+ByteLength));

        public BattleMap(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }
        public void SwapWith(BattleMap target)
        {
            for (int i = 2; i < Data.Length; i++)
            {
                (target.Data[i], Data[i]) = (Data[i], target.Data[i]);
            }
        }

        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
    }
}
