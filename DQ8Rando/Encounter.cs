using System;
using System.Collections.Generic;

namespace DQ8Rando
{
    [Serializable]
    public class Encounter
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Spawn { get; set; }
        public double Chance { get; set; }
    }
    [Serializable]
    public class SetEncounter
    {
        public string ID { get; set; }
        public string[] Contents { get; set; }
        public int Spawn { get; set; }
        public double Chance { get; set; }
    }
    [Serializable]
    public class EncountFile
    {
        public string Header { get; set; }
        public List<EncountTable> Contents { get; set; }
    }
    [Serializable]
    public class EncountTable
    {
        public string ID { get; set; }
        public string Region { get; set; }
        public string Area { get; set; }
        public int Edit { get; set; }
        public string Header { get; set; }
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
}
