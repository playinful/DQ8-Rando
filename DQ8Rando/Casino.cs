namespace DQ8Rando
{
    public class ItemCasinoFile
    {
        public string Header { get; set; }
        public Casino[] Contents { get; set; }
    }
    public class Casino
    {
        public string ID { get; set; }
        public string Location { get; set; }
        public string[] Items { get; set; }
        public string[] Prices { get; set; }
    }
}
