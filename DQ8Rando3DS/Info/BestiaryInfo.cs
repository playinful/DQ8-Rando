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
        private static BestiaryInfo[] _Data;
        public static BestiaryInfo[] Data
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
            _Data = JsonSerializer.Deserialize<BestiaryInfo[]>(File.ReadAllText("./Json/Bestiary.json"));
        }
        public static BestiaryInfo GetById(int id)
        {
            return Data.First(info => info.ID == id);
        }
        public static BestiaryInfo GetByMonsterId(ushort id)
        {
            return Data.First(info => info.Monsters.Contains(id));
        }
        public static IEnumerable<BestiaryInfo> GetWhere(Func<BestiaryInfo, bool> predicate)
        {
            return Data.Where(predicate);
        }

        public int? ID { get; set;}
        public string Name { get; set; }
        public bool Required { get; set; }
        public ushort[] Monsters { get; set; }

        public bool IsWithin(EncounterTable table)
        {
            //foreach (EncounterPool pool in table.Contents.Values.Where(p => p.GetInfo().Parent is null && !p.GetInfo().Missable))
            //{
            //    foreach (Encounter encounter in pool.Symbols)
            //    {
            //        if (Monsters.Contains(encounter.GetInfo().Monster))
            //            return true;
            //    }
            //    foreach (GroupEncounter encounter in pool.Parties)
            //    {
            //        if (encounter.GetInfo().Monsters.Any(mon => Monsters.Contains(mon)))
            //            return true;
            //    }
            //}
            return false;
        }
        public static bool AllWithin(EncounterTable table)
        {
            return Data.All(best => !best.Required || best.IsWithin(table));
        }

        //public List<SymbolInfo> GetEncounters()
        //{
        //    List<SymbolInfo> encounters = new List<SymbolInfo>();

        //    encounters.AddRange(SymbolInfo.GetWhere(enc => enc.CanSpawn && Monsters.Contains(enc.Monster)));

        //    return encounters;
        //}
        //public List<SpPartyInfo> GetGroupEncounters()
        //{
        //    List<SpPartyInfo> encounters = new List<SpPartyInfo>();

        //    //encounters.AddRange(SpPartyInfo.GetWhere(enc => enc.CanSpawn && enc.Monsters.Any(mon => Monsters.Contains(mon)))); //TODO

        //    return encounters;
        //}
    }
}
