using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace DQ8Rando3DS.Info
{
    public class HotelInfo
    {
        private static Dictionary<byte, HotelInfo> _Data { get; set; }
        public static Dictionary<byte, HotelInfo> Data
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
            _Data = JsonSerializer.Deserialize<Dictionary<byte, HotelInfo>>(File.ReadAllText("./Json/HotelCharges.json"));
        }

        public static HotelInfo Get(byte id)
        {
            return Data[id];
        }

        public byte ID { get; set; }
        public string Name { get; set; }
        public bool DoEdit { get; set; }
    }
}
