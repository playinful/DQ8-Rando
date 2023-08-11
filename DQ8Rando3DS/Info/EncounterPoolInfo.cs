using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

namespace DQ8Rando3DS.Info
{
    public class EncounterPoolInfo
    {
        private static Dictionary<ushort, EncounterPoolInfo> _Data;
        public static Dictionary<ushort, EncounterPoolInfo> Data { 
            get
            {
                if (_Data is null)
                    Load();
                return _Data; 
            } 
        }

        public static void Load()
        {
            _Data = JsonSerializer.Deserialize<Dictionary<ushort, EncounterPoolInfo>>(File.ReadAllText("./Json/Encount.json"));
        }
        public static EncounterPoolInfo Get(ushort id)
        {
            return Data[id];
        }
        public static IEnumerable<EncounterPoolInfo> GetWhere(Func<EncounterPoolInfo, bool> predicate)
        {
            return Data.Values.Where(predicate);
        }

        public ushort ID { get; set; }
        public string Name { get; set; }
        public ushort? Parent { get; set; }
        public double PowerLevel { get; set; }
        public bool DoEdit { get; set; }
        public bool Missable { get; set; }
        public bool NeedsModel { get; set; } = true;
        public bool IsBoss { get; set; }
        public bool IsArena { get; set; }
        public bool IsPostgame { get; set; }
        public bool IsMemoriam { get; set; }
        public bool IsSpecial { get; set; }
        public ushort? SpecialSymbol { get; set; }
        public int SortIndex { get; set; } = int.MaxValue;
    }
}
