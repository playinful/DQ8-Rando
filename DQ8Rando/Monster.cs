using System;
using System.Collections.Generic;

namespace DQ8Rando
{
    public class MonsterFile
    {
        public string Header { get; set; }
        public Dictionary<string,Monster> Contents { get; set; }
    }
    public class Monster
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Edit { get; set; }
        public string Parent { get; set; }
        public string Unk1 { get; set; }
        public int HP { get; set; }
        public int MP { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Agility { get; set; }
        public int Wisdom { get; set; }
        public int Experience { get; set; }
        public int Gold { get; set; }
        public string[] Items { get; set; }
        public string Unk2 { get; set; }
        public string[] Resistances { get; set; }
        public string Unk3 { get; set; }
        public string[] Actions { get; set; }
        public string Unk4 { get; set; }
        public string Unk5 { get; set; }
        public string Unk6 { get; set; }
        public string Footer { get; set; }
        public bool DoNotAverage { get; set; }
        public List<Monster> Children = new List<Monster>();
    }
    [Serializable]
    public class Encounter
    {
        public string ID { get; set; }
        public string Monster { get; set; }
        public int Spawn { get; set; }
        public bool HasOverworldModel { get; set; }
    }
    [Serializable]
    public class SetEncounter
    {
        public string ID { get; set; }
        public string[] Contents { get; set; }
        public int Spawn { get; set; }
        public double Chance { get; set; }
        public int Groups { get; set; }
        public int Count { get; set; }
        public bool HasOverworldModel { get; set; }
        public string TeamName { get; set; }
    }
    [Serializable]
    public class EncountFile
    {
        public string Header { get; set; }
        public Dictionary<string, EncountTable> Contents { get; set; }
    }
    [Serializable]
    public class EncountTable
    {
        public string ID { get; set; }
        public string Region { get; set; }
        public string Area { get; set; }
        public int Edit { get; set; }
        public string Parent { get; set; }
        public string Header { get; set; }
        public double AvgHP { get; set; }
        public double AvgMP { get; set; }
        public double AvgATK { get; set; }
        public double AvgDEF { get; set; }
        public double AvgAGI { get; set; }
        public double AvgWIS { get; set; }
        public double AvgBST { get; set; }
        public double AvgEXP { get; set; }
        public EncountTableEntry[] Contents { get; set; }
        public EncountTableSetEntry[] SetEncounters { get; set; }
    }
    [Serializable]
    public class EncountTableEntry
    {
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }
        public string ID { get; set; }
        public string Footer { get; set; }
    }
    [Serializable]
    public class EncountTableSetEntry
    {
        public string Weight { get; set; }
        public string ID { get; set; }
    }
    public class GospelEntry
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Required { get; set; }
        public string[] Monsters { get; set; }
    }
    public class ScoutMonsterFile
    {
        public string Header { get; set; }
        public Dictionary<string, ScoutMonster> Contents { get; set; }
    }
    public class ScoutMonster
    {
        public string ID { get; set; }
        public string Monster { get; set; }
        public string Unk1 { get; set; }
        public string Unk2 { get; set; }
        public bool Shuffle { get; set; }

        /*public string OldMonster { get; set; }
        public void init()
        {
            OldMonster = Monster;
        }*/
    }
}
