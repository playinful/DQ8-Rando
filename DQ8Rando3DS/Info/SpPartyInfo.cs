using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DQ8Rando3DS.Info
{
    public class SpPartyInfo
    {
        private static Dictionary<ushort, SpPartyInfo> _Data;
        public static Dictionary<ushort, SpPartyInfo> Data { 
            get 
            {
                if (_Data is null)
                    Load();
                return _Data;
            }
        }

        public static void Load()
        {
            _Data = JsonSerializer.Deserialize<Dictionary<ushort, SpPartyInfo>>(File.ReadAllText("./Json/SpParty.json"));
        }
        public static SpPartyInfo Get(byte id)
        {
            return Data.ContainsKey(id) ? Data[id] : new SpPartyInfo(id);
        }
        public static IEnumerable<SpPartyInfo> GetWhere(Func<SpPartyInfo, bool> predicate)
        {
            return Data.Values.Where(predicate);
        }

        public SpPartyInfo(byte id) => ID = id;

        public byte ID { get; set; }
        public bool CanSpawn { get; set; }
        public bool DoOverwrite { get; set; }
        public string TeamName { get; set; }
    }
}
