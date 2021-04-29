using System;

namespace DQ8Rando
{
    [Serializable]
    public class HotelCharges
    {
        public string Header { get; set; }
        public Hotel[] Contents { get; set; }
    }
    public class Hotel
    {
        public string ID { get; set; }
        public int Price { get; set; }
        public string Location { get; set; }
        public int Edit { get; set; }
    }
}