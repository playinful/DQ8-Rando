using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Windows.Navigation;
using DQ8Rando3DS.Tables;

namespace DQ8Rando3DS.Info
{
    public class BestiaryInfo
    {
        private static Dictionary<ushort, BestiaryInfo> _Data;
        public static Dictionary<ushort, BestiaryInfo> Data
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
            _Data = JsonSerializer.Deserialize<Dictionary<ushort, BestiaryInfo>>(File.ReadAllText("./Json/Bestiary.json"));
        }
        public static BestiaryInfo GetById(ushort id)
        {
            return Data.ContainsKey(id) ? Data[id] : null;
        }
        public static IEnumerable<BestiaryInfo> GetWhere(Func<BestiaryInfo, bool> predicate)
        {
            return Data.Values.Where(predicate);
        }
        public static IEnumerable<BestiaryInfo> GetRequired()
        {
            return GetWhere(x => x.Required);
        }

        public int? ID { get; set;}
        public bool Required { get; set; }
    }
}
