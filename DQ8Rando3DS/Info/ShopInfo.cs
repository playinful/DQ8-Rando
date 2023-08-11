using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace DQ8Rando3DS.Info
{
    public class ShopInfo
    {
        private static Dictionary<byte, ShopInfo> _Data { get; set; }
        public static Dictionary<byte, ShopInfo> Data
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
            _Data = JsonSerializer.Deserialize<Dictionary<byte, ShopInfo>>(File.ReadAllText("./Json/ShopItem.json"));
        }
        public static ShopInfo Get(byte id)
        {
            return Data[id];
        }

        public byte ID { get; set; }
        public byte? Parent { get; set; }
        public string Name { get; set; }
        public bool DoEdit { get; set; }
    }
}
