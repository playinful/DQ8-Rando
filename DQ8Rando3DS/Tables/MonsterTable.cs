using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Printing.IndexedProperties;
using DQ8Rando3DS.Info;
using System.Text.RegularExpressions;

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
            List<string> spoilerLog = new();

            string template = File.ReadAllText("./Json/monster_spoiler_template.txt");
            foreach (Monster monster in Contents.Values.Where(mon => mon.GetInfo().DoEdit && !mon.GetInfo().GroupWithParent).OrderBy(mon => mon.GetInfo().SortIndex))
            {
                string spoiler = template;

                spoiler = new Regex("%{73}").Replace(spoiler, monster.GetInfo().Name.PadRight(73, '═'), 1);

                spoiler = new Regex("#{5}").Replace(spoiler, monster.MaximumHP.ToString().PadLeft(5), 1);
                spoiler = new Regex("#{5}").Replace(spoiler, monster.MaximumMP.ToString().PadLeft(5), 1);
                spoiler = new Regex("#{5}").Replace(spoiler, monster.Attack.ToString().PadLeft(5), 1);
                spoiler = new Regex("#{5}").Replace(spoiler, monster.Defence.ToString().PadLeft(5), 1);
                spoiler = new Regex("#{5}").Replace(spoiler, monster.Agility.ToString().PadLeft(5), 1);
                spoiler = new Regex("#{5}").Replace(spoiler, monster.Wisdom.ToString().PadLeft(5), 1);

                spoiler = new Regex("#{10}").Replace(spoiler, monster.Experience.ToString().PadLeft(10), 1);
                spoiler = new Regex("#{10}").Replace(spoiler, monster.Gold.ToString().PadLeft(10), 1);

                string evasion = monster.Evasion == 0 ? "0/1" : $"1/{256 * Math.Pow(0.5, monster.Evasion - 1)}";
                spoiler = new Regex("#{5}").Replace(spoiler, evasion.PadLeft(5), 1);
                spoiler = new Regex("#{5}").Replace(spoiler, RESIST_VALUES[23][monster.GetResistance(23)].PadLeft(5), 1);

                for (int i = 0; i < 22; i++)
                {
                    spoiler = new Regex("#{5}").Replace(spoiler, RESIST_VALUES[i][monster.GetResistance(i)].PadLeft(5), 1);
                }

                List<string> items = new();
                if (monster.Item1 > 0 && monster.Item1DropRate > 0)
                {
                    items.Add($"║ - {ItemInfo.Get(monster.Item1).Name} (1/{256 * Math.Pow(0.5, monster.Item1DropRate - 1)})".PadRight(74) + "║");
                }
                if (monster.Item2 > 0 && monster.Item2DropRate > 0)
                {
                    items.Add($"║ - {ItemInfo.Get(monster.Item2).Name} (1/{256 * Math.Pow(0.5, monster.Item2DropRate - 1)})".PadRight(74) + "║");
                }
                spoiler = new Regex("(?<=[\n\r])\\$(?=[\n\r])").Replace(spoiler, string.Join(Environment.NewLine, items));

                HashSet<string> actions = new();
                foreach (Monster child in Contents.Values.Where(mon => mon == monster || (mon.GetInfo().Parent == monster.ID && mon.GetInfo().GroupWithParent)))
                {
                    for (int i = 0; i < 6; i++)
                        actions.Add("║ - " + ActionInfo.Get(child.GetAction(i)).Name.PadRight(70) + "║");
                }
                spoiler = new Regex("(?<=[\n\r])!(?=[\n\r])").Replace(spoiler, string.Join(Environment.NewLine, actions));

                spoilerLog.Add(spoiler);
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine + Environment.NewLine, spoilerLog));
        }

        public Dictionary<ushort, Monster> Contents { get; set; } = new Dictionary<ushort, Monster>();

        public static string[][] RESIST_VALUES { get; } =
        {
            new string[]{ "0%", "25%", "50%", "100%" }, // Frizz
            new string[]{ "0%", "25%", "50%", "100%" }, // Sizz
            new string[]{ "0%", "25%", "50%", "100%" }, // Bang
            new string[]{ "0%", "25%", "50%", "100%" }, // Woosh
            new string[]{ "0%", "25%", "50%", "100%" }, // Zap
            new string[]{ "0%", "25%", "50%", "100%" }, // Crack
            new string[]{ "0%", "15%", "50%", "100%" }, // Dazzle
            new string[]{ "0%", "15%", "50%", "100%" }, // Snooze
            new string[]{ "0%", "15%", "50%", "100%" }, // Whack
            new string[]{ "0%", "15%", "50%", "100%" }, // Drain Magic
            new string[]{ "0%", "15%", "50%", "100%" }, // Fizzle
            new string[]{ "0%", "15%", "50%", "100%" }, // Fuddle
            new string[]{ "0%", "15%", "50%", "100%" }, // Sap
            new string[]{ "0%", "15%", "50%", "100%" }, // Deceleratle
            new string[]{ "0%", "15%", "50%", "100%" }, // Poison
            new string[]{ "0%", "15%", "50%", "100%" }, // Paralysis
            new string[]{ "0%", "15%", "50%", "100%" }, // Stun
            new string[]{ "0%", "15%", "50%", "100%" }, // Ban Dance
            new string[]{ "0%", "25%", "50%", "100%" }, // Fire Breath
            new string[]{ "0%", "25%", "50%", "100%" }, // Cool Breath
            new string[]{ "0%", "25%", "50%", "100%" }, // Strike/Rock
            new string[]{ "0%", "25%", "50%", "100%" }, // Army
            null, // None
            new string[]{ "0%", "30%", "60%", "90%" }, // Attack
        };
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
        public byte Item1DropRate { get => Data[0x20]; set => Data[0x20] = value; }
        public byte Item2DropRate { get => Data[0x21]; set => Data[0x21] = value; }

        public byte Evasion { get => Data[0x4E]; set => Data[0x4E] = value; }

        public ushort Bestiary { get => BitConverter.ToUInt16(Data[0x50..0x52]); set => Extensions.SetBytes(Data[0x50..0x52], BitConverter.GetBytes(value)); }

        //public Span<byte> Resistances => Data[0x33..0x4B];
        public void SetResistance(int index, byte res)
        {
            if (index < 0 || index >= 24)
                return;

            Data[0x23 + index] = res;
        }
        public byte GetResistance(int index)
        {
            if (index < 0 || index >= 24)
                return 255;

            return Data[0x23 + index];
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

        public MonsterInfo GetInfo() => MonsterInfo.Get(ID);
        public BestiaryInfo GetBestiaryInfo() => BestiaryInfo.GetById(Bestiary);
    }
}
