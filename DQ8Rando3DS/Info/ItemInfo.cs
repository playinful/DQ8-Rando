using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace DQ8Rando3DS.Info
{
    public class ItemInfo
    {
        private static Dictionary<ushort, ItemInfo> _Data;
        public static Dictionary<ushort, ItemInfo> Data {
            get {
                if (_Data is null)
                    Load();
                return _Data;
            }
        }

        public static void Load()
        {
            _Data = JsonSerializer.Deserialize<Dictionary<ushort, ItemInfo>>(File.ReadAllText("Json/Item.json"));
        }
        public static ItemInfo Get(ushort id)
        {
            if (_Data is null)
                Load();
            return _Data[id];
        }

        public static IEnumerable<ItemInfo> GetNormalItems()
        {
            return Data.Values.Where(item => item.Type == ItemInfo.ItemType.Item);
        }
        public static ItemInfo GetRandomItem(Random rng)
        {
            return rng.Choice(GetNormalItems());
        }

        public ushort ID { get; set; }
        public ItemType Type { get; set; }
        public string Name { get; set; }

        public enum ItemType
        {
            None = 0,
            Item = 1,
            KeyItem = 2,
            Dummy = 3
        }
    }
}
