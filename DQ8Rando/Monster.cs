using System;
using System.Collections.Generic;

namespace DQ8Rando
{
    public class MonsterFile
    {
        public string Header { get; set; }
        public Dictionary<string,Monster> Contents { get; set; }
        public MonsterFile Clone()
        {
            MonsterFile newFile = new MonsterFile();
            newFile.Header = Header;
            newFile.Contents = new Dictionary<string, Monster>();
            foreach (KeyValuePair<string, Monster> kv in Contents)
            {
                Monster add = kv.Value.Clone();
                newFile.Contents.Add(kv.Key, add);
            }
            return newFile;
        }
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

        public int PowerLevel { get; set; }
        public double AdjustedLevel { get; set; } = -1;

        public Monster Clone()
        {
            Monster newMonster = new Monster();
            newMonster.Agility = Agility;
            newMonster.Attack = Attack;
            newMonster.Defence = Defence;
            newMonster.Experience = Experience;
            newMonster.Footer = Footer;
            newMonster.Gold = Gold;
            newMonster.HP = HP;
            newMonster.ID = ID;
            newMonster.MP = MP;
            newMonster.Name = Name;
            newMonster.Unk1 = Unk1;
            newMonster.Unk2 = Unk2;
            newMonster.Unk3 = Unk3;
            newMonster.Unk4 = Unk4;
            newMonster.Unk5 = Unk5;
            newMonster.Unk6 = Unk6;
            newMonster.Edit = Edit;
            newMonster.Parent = Parent;
            newMonster.DoNotAverage = DoNotAverage;
            newMonster.PowerLevel = PowerLevel;
            newMonster.AdjustedLevel = AdjustedLevel;
            newMonster.Actions = new string[6];
            for (int i = 0; i < newMonster.Actions.Length; i++)
            {
                newMonster.Actions[i] = Actions[i];
            }
            newMonster.Items = new string[2];
            for (int i = 0; i < newMonster.Items.Length; i++)
            {
                newMonster.Items[i] = Items[i];
            }
            newMonster.Resistances = new string[22];
            for (int i = 0; i < newMonster.Resistances.Length; i++)
            {
                newMonster.Resistances[i] = Resistances[i];
            }
            foreach (Monster child in Children)
            {
                newMonster.Children.Add(child);
            }

            return newMonster;
        }
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
        public double PowerLevel { get; set; }
        public EncountTableEntry[] Contents { get; set; } = new EncountTableEntry[10];
        public EncountTableSetEntry[] SetEncounters { get; set; } = new EncountTableSetEntry[2];
        public EncountTable Clone()
        {
            EncountTable copy = new EncountTable();

            copy.ID = ID;
            copy.Region = Region;
            copy.Area = Area;
            copy.Edit = Edit;
            copy.Parent = Parent;
            copy.Header = Header;
            copy.AvgHP = AvgHP;
            copy.AvgMP = AvgMP;
            copy.AvgATK = AvgATK;
            copy.AvgDEF = AvgDEF;
            copy.AvgAGI = AvgAGI;
            copy.AvgWIS = AvgWIS;
            copy.AvgBST = AvgBST;
            copy.AvgEXP = AvgEXP;
            copy.PowerLevel = PowerLevel;
            copy.Shuffled = Shuffled;

            for (int i = 0; i < 10; i++)
            {
                copy.Contents[i] = Contents[i].Clone();
            }
            for (int i = 0; i < 2; i++)
            {
                copy.SetEncounters[i] = SetEncounters[i].Clone();
            }

            return copy;
        }

        public bool Shuffled { get; set; } = false;
    }
    [Serializable]
    public class EncountTableEntry
    {
        public string Arg1 { get; set; } = "00";
        public string Arg2 { get; set; } = "00";
        public string ID { get; set; } = "0000";
        public string Footer { get; set; } = "00000000";
        public EncountTableEntry Clone()
        {
            EncountTableEntry copy = new EncountTableEntry();

            copy.Arg1 = Arg1;
            copy.Arg2 = Arg2;
            copy.ID = ID;
            copy.Footer = Footer;

            return copy;
        }

        public bool Shuffled { get; set; } = false;
    }
    [Serializable]
    public class EncountTableSetEntry
    {
        public string Weight { get; set; } = "00";
        public string ID { get; set; } = "86";
        public EncountTableSetEntry Clone()
        {
            EncountTableSetEntry copy = new EncountTableSetEntry();

            copy.ID = ID;
            copy.Weight = Weight;

            return copy;
        }

        public bool Shuffled { get; set; } = false;
    }
    public class GospelEntry
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Required { get; set; }
        public string[] Monsters { get; set; }
        public bool Successful { get; set; } = false;
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
