namespace DQ8Rando
{
    public class ShopItemFile
    {
        public string Header { get; set; }
        public Shop[] Contents { get; set; }
    }
    public class Shop
    {
        public string ID { get; set; }
        public string Unk1 { get; set; }
        public string Unk2 { get; set; }
        public string Unk3 { get; set; }
        public string Location { get; set; }
        public string Parent { get; set; }
        public string Store { get; set; }
        public int Sale { get; set; }
        public ShopItem[] Items { get; set; }
        public int Edit { get; set; }
    }
    public class ShopItem
    {
        public string ID { get; set; }
        public string Footer { get; set; }
    }
}
