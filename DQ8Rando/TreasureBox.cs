using System;

namespace DQ8Rando
{
    [Serializable]
    public class TreasureBox
    {
        public string ID { get; set; }
        public string EngRegion { get; set; }
        public string EngMap { get; set; }
        public string Container { get; set; }
        public int Edit { get; set; }
        public string Unk1 { get; set; }
        public string Region { get; set; }
        public string Map { get; set; }
        public string Code { get; set; }
        public string Model { get; set; }
        public string Type { get; set; }
        public string Lock { get; set; }
        public string Content { get; set; }
        public string Value { get; set; }
        public string Pool { get; set; }
        public string XPos { get; set; }
        public string YPos { get; set; }
        public string ZPos { get; set; }
        public string Rot { get; set; }
        public string Parent { get; set; }
    }
    [Serializable]
    public class TreasureBoxFile
    { 
        public TreasureBox[] Contents { get; set; }
        public string Footer { get; set; }
    }
    [Serializable]
    public class BlueChestPool
    {
        public string ID { get; set; }
        public string Unk1 { get; set; }
        public BlueChestItem[] Contents { get; set; }
    }
    [Serializable]
    public class BlueChestItem
    {
        public string ID { get; set; }
        public string Value { get; set; }
        public string Footer { get; set; }
    }
    [Serializable]
    public class TbContentsFile
    {
        public string Header { get; set; }
        public BlueChestPool[] Contents { get; set; }
    }
}