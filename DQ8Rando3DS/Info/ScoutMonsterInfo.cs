using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace DQ8Rando3DS.Info
{
    public class ScoutMonsterInfo
    {
        private static Dictionary<ushort, ScoutMonsterInfo> _Data;
        public static Dictionary<ushort, ScoutMonsterInfo> Data
        {
            get
            {
                if (_Data == null)
                    Load();
                return _Data;
            }
        }

        public static void Load()
        {
            _Data = JsonSerializer.Deserialize<Dictionary<ushort, ScoutMonsterInfo>>(File.ReadAllText("./Json/ScoutMonster.json"));
        }
        public static ScoutMonsterInfo Get(ushort id)
        {
            return Data[id];
        }

        public ushort ID { get; set; }
        public ushort Encounter { get; set; }
        public bool DoShuffle { get; set; }
    }
}
