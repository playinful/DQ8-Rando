using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Markup;
using System.Configuration;
using DQ8Rando3DS.Info;

namespace DQ8Rando3DS.Tables
{
    public class TreasureBoxTable
    {
        public TreasureBoxTable(byte[] data)
        {
            Data = data;

            for (int i = 0; i + TreasureBox.ByteLength < Data.Length; i += TreasureBox.ByteLength)
            {
                TreasureBox treasureBox = new TreasureBox(data, i);
                Contents.Add(treasureBox.ID, treasureBox);
            }
        }
        public static TreasureBoxTable Load(string path)
        {
            return new TreasureBoxTable(File.ReadAllBytes(path));
        }
        public static TreasureBoxTable Load()
        {
            return Load("./Raw/tbTreasureBox.tbl");
        }
        public TreasureBoxTable Clone()
        {
            return new TreasureBoxTable(Data[..]);
        }

        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Wrote treasure box table to `{path}`.");
        }
        public void CreateSpoilerLog(string path)
        {
            List<string> outLines = new List<string>();
            foreach (TreasureBox box in Contents.Values)
            {
                string spoiler = "";

                if (box.HasParent() || // box has parent
                    box.Item == 0 || // box is empty
                    box.GetInfo().Type == 0) // box shouldn't be edited
                    continue;

                if (box.Behaviour != TreasureBox.TBoxBehaviour.Chest      &&
                    box.Behaviour != TreasureBox.TBoxBehaviour.Breakable  &&
                    box.Behaviour != TreasureBox.TBoxBehaviour.HangingBag &&
                    box.Behaviour != TreasureBox.TBoxBehaviour.Wardrobe   &&
                    box.Behaviour != TreasureBox.TBoxBehaviour.SparklySpot)
                    continue;

                string name = TreasureBoxInfo.Get(box.ID).Name;

                if (box.Item == 65535)
                {
                    spoiler = $"{name}: {box.Value} gold coin{(box.Value != 1 ? "s" : "")}";
                }
                else if (box.Item == 65533)
                {
                    spoiler = $"{name}: Trap ({Info.MonsterInfo.Get((ushort)box.Value).Name})";
                }
                else
                {
                    string item = ItemInfo.Get(box.Item).Name;
                    spoiler = $"{name}: {item}";
                }

                switch(box.Model)
                {
                    case TreasureBox.TBoxModel.DefaultChest:
                    case TreasureBox.TBoxModel.RedChest:
                    case TreasureBox.TBoxModel.SnowyRedChest:
                    case TreasureBox.TBoxModel.DarkRedChest:
                        spoiler = spoiler.Replace("%c", "Red Chest");
                        break;
                    case TreasureBox.TBoxModel.PurpleChest:
                    case TreasureBox.TBoxModel.SnowyPurpleChest:
                        spoiler = spoiler.Replace("%c", "Purple Chest");
                        break;
                    case TreasureBox.TBoxModel.BlueChest:
                    case TreasureBox.TBoxModel.SnowyBlueChest:
                        spoiler = spoiler.Replace("%c", "Blue Chest");
                        break;
                }

                switch (box.Lock)
                {
                    case TreasureBox.LockType.ThiefKey:
                        spoiler += " [needs Thief's Key]";
                        break;
                    case TreasureBox.LockType.MagicKey:
                        spoiler += " [needs Magic Key]";
                        break;
                }

                outLines.Add(spoiler);
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, outLines));
            MainWindow.UpdateStatus($"Wrote treasure spoiler log to `{path}`.");
        }

        public TreasureBox GetById(ushort id)
        {
            return Contents[id];
        }
        public TreasureBox GetByName(string name)
        {
            foreach (TreasureBox box in Contents.Values)
            {
                if (box.Name == name)
                    return box;
            }
            return null;
        }

        public byte[] Data { get; set; }
        public Dictionary<ushort, TreasureBox> Contents { get; set; } = new Dictionary<ushort, TreasureBox>();
    }
    public class TreasureBox
    {
        public const int ByteLength = 0x50;

        private byte[] _Data;
        public int Offset { get; set; }
        public Span<byte> Data { get => _Data.AsSpan(Offset..(Offset + ByteLength)); }

        public TreasureBox(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        // unique ID; 0x0
        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
        // internal string name; 0x14 ~ 0x24
        public string Name { get => Encoding.UTF8.GetString(Data[0x14..0x24]); set => Extensions.SetBytes(Data[0x14..0x24], Encoding.UTF8.GetBytes(value)); }
        // internal string name of parent box; 0x40 ~ 0x50
        public string Parent { get => Encoding.UTF8.GetString(Data[0x40..0x50]); set => Extensions.SetBytes(Data[0x40..0x50], Encoding.UTF8.GetBytes(value)); }
        public bool HasParent() => Parent is not null && Parent != string.Empty && Parent != "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

        // Item ID; 0x2A (65535 = gold, 65533 = trap)
        public ushort Item { get => BitConverter.ToUInt16(Data[0x2A..0x2C]); set => Extensions.SetBytes(Data[0x2A..0x2C], BitConverter.GetBytes(value)); }
        // amount of gold contained within (or Group Encounter ID); 0x2C
        public short Value { get => BitConverter.ToInt16(Data[0x2C..0x2E]); set => Extensions.SetBytes(Data[0x2C..0x2E], BitConverter.GetBytes(value)); }
        // pool for blue chest; 0x2E
        public ushort Pool { get => BitConverter.ToUInt16(Data[0x2E..0x30]); set => Extensions.SetBytes(Data[0x2E..0x30], BitConverter.GetBytes(value)); }
        // item ID of key needed to open; 0x28
        // 0x0115 = Thief's Key, 0x0116 = Magic Key, 0x0117 = ultimate key
        public LockType Lock { get => (LockType)BitConverter.ToUInt16(Data[0x28..0x2A]); set => Extensions.SetBytes(Data[0x28..0x2A], BitConverter.GetBytes((ushort)value)); }
        public enum LockType: ushort
        {
            None     = 0x0,
            ThiefKey = 0x0115,
            MagicKey = 0x0116
        }

        // model for chest; 0x24
        public TBoxModel Model { get => (TBoxModel)Data[0x24]; set => Data[0x24] = (byte)value; }
        public enum TBoxModel: byte
        {
            None             = 0x0,
            DefaultChest     = 0x1,
            RedChest         = 0x44,
            PurpleChest      = 0x45,
            BlueChest        = 0x46,
            SnowyRedChest    = 0x47,
            SnowyPurpleChest = 0x48,
            SnowyBlueChest   = 0x49,
            DarkRedChest     = 0x4B
        }

        // type of object this box behaves as; 0x26
        public TBoxBehaviour Behaviour { get => (TBoxBehaviour)Data[0x26]; set => Data[0x26] = (byte)value; }
        public enum TBoxBehaviour: byte
        {
            None           = 0x0,
            Chest          = 0x1,
            BlueChest      = 0x2,
            Breakable      = 0x3,
            EmptyBreakable = 0x4,
            Salamango      = 0x5,
            HangingBag     = 0x6,
            Wardrobe       = 0x7,
            Book           = 0x8,
            SparklySpot    = 0xA
        }

        public TreasureBoxInfo GetInfo() => TreasureBoxInfo.Get(ID);
        public bool IsEmpty()
        {
            return Item == 0 && Pool == 0;
        }
    }
}
