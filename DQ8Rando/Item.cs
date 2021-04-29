using System;

namespace DQ8Rando
{
    [Serializable]
    public class Item
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Spawn { get; set; }
        public string Unk1 { get; set; }
        public string Unk2 { get; set; }
        public string Unk3 { get; set; }
        public string Unk4 { get; set; }
        public string Unk5 { get; set; }
        public string Icon { get; set; }
        public string Price { get; set; }
        public string SellPrice { get; set; }
        public string Attack { get; set; }
        public string Defence { get; set; }
        public string Agility { get; set; }
        public string Wisdom { get; set; }
        public string Footer { get; set; }
    }
    public class ItemFile
    {
        public string Header { get; set; }
        public Item[] Contents { get; set; }
    }
}