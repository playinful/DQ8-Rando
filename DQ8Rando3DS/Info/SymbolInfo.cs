using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQ8Rando3DS.Info
{
    public class SymbolInfo
    {
        private static Dictionary<ushort, SymbolInfo> _Data;
        public static Dictionary<ushort, SymbolInfo> Data {
            get
            {
                if (_Data == null)
                    Load();
                return _Data;
            }
        }

        public static void Load()
        {
            _Data = JsonSerializer.Deserialize<Dictionary<ushort, SymbolInfo>>(File.ReadAllText("./Json/Symbol.json"));
        }
        public static SymbolInfo Get(ushort id)
        {
            return Data.ContainsKey(id) ? Data[id] : null;
        }
        public static IEnumerable<SymbolInfo> GetWhere(Func<SymbolInfo, bool> predicate)
        {
            return Data.Values.Where(predicate);
        }

        public ushort ID { get; set; }
        public bool CanSpawn { get; set; }
    }
}
