using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

namespace DQ8Rando3DS.Info
{
    public class ActionInfo
    {
        private static Dictionary<ushort, ActionInfo> _Data;
        public static Dictionary<ushort, ActionInfo> Data
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
            _Data = JsonSerializer.Deserialize<Dictionary<ushort, ActionInfo>>(File.ReadAllText("./Json/Actions.json"));
        }
        public static ActionInfo Get(ushort id)
        {
            return Data[id];
        }
        public static IEnumerable<ActionInfo> GetWhere(Func<ActionInfo, bool> predicate)
        {
            return Data.Values.Where(predicate);
        }

        public ushort ID { get; set; }
        public string Name { get; set; }
    }
}
