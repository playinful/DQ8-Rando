using System;

namespace DQ8Rando
{
    [Serializable]
    public class Item
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Spawn { get; set; }
    }
}