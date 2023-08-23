using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using DQ8Rando3DS.Tables;

namespace DQ8Rando3DS.Info
{
    public class MonsterInfo
    {
        private static Dictionary<ushort, MonsterInfo> _Data;
        public static Dictionary<ushort, MonsterInfo> Data
        {
            get
            {
                if (_Data is null)
                    Load();
                return _Data;
            }
        }

        public static void Load()
        {
            _Data = JsonSerializer.Deserialize<Dictionary<ushort, MonsterInfo>>(File.ReadAllText("./Json/Monsters.json"));
        }
        public static MonsterInfo Get(ushort id)
        {
            return Data[id];
        }

        public ushort ID { get; set; }
        public string Name { get; set; }
        public bool CanSpawn { get; set; }
        public bool DoEdit { get; set; }

        public ushort? Parent { get; set; }
        public bool CopyStats { get; set; }
        public bool CopyActions { get; set; }
        public bool CopyLoot { get; set; }
        public bool CopyAll { get; set; }

        public bool IsBoss { get; set; }
        public bool IsPostgame { get; set; }
        public bool IsMemoriam { get; set; }
        public bool IsArena { get; set; }
        public bool IsInfamous { get; set; }
        public bool IsSpecial { get; set; }
        public bool IsExtra { get; set; }

        public double PowerLevel { get; set; }

        public int SortIndex { get; set; } = int.MaxValue;
        public bool GroupWithParent { get; set; } = false;
    }
}
