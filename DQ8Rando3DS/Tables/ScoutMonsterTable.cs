using DQ8Rando3DS.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Documents;

namespace DQ8Rando3DS.Tables
{
    public class ScoutMonsterTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public ScoutMonsterTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + ScoutMonster.ByteLength <= Data.Length; i += ScoutMonster.ByteLength)
            {
                ScoutMonster monster = new ScoutMonster(Data, i);
                Contents.Add(monster.ID, monster);
            }
        }
        public static ScoutMonsterTable Load(string path)
        {
            return new ScoutMonsterTable(File.ReadAllBytes(path));
        }
        public static ScoutMonsterTable Load()
        {
            return Load("./Raw/scoutMonster.tbl");
        }
        public ScoutMonsterTable Clone()
        {
            return new ScoutMonsterTable(Data[..]);
        }

        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Wrote scout monster table to `{path}`.");
        }
        public void CreateSpoilerLog(string path)
        {
            List<string> spoilerLog = new List<string>();

            foreach (ScoutMonster monster in Contents.Values.Where(mon => mon.GetInfo().DoShuffle))
            {
                //spoilerLog.Add($"{MonsterInfo.Get(SymbolInfo.Get(monster.GetInfo().Encounter).Monster).Name} -> {MonsterInfo.Get(monster.GetEncounterInfo().Monster).Name}"); TODO
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, spoilerLog));
            MainWindow.UpdateStatus($"Wrote scout monster spoiler log to `{path}`.");
        }

        public Dictionary<ushort, ScoutMonster> Contents { get; set; } = new Dictionary<ushort, ScoutMonster>();
    }
    public class ScoutMonster
    {
        public const int ByteLength = 0x6;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset+ByteLength));

        public ScoutMonster(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
        public ushort Monster { get => BitConverter.ToUInt16(Data[2..4]); set => Extensions.SetBytes(Data[2..4], BitConverter.GetBytes(value)); }

        public ScoutMonsterInfo GetInfo() => ScoutMonsterInfo.Get(ID);
        public SymbolInfo GetEncounterInfo() => SymbolInfo.Get(Monster);
    }
}
