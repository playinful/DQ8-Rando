using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace DQ8Rando3DS.Info
{
    public class TreasureBoxInfo
    {
        public static Dictionary<ushort, TreasureBoxInfo> Data { get; set; } = default!;

        public static void Load()
        {
            Data = JsonSerializer.Deserialize<Dictionary<ushort, TreasureBoxInfo>>(File.ReadAllText("Json/TreasureBox.json"));
        }
        public static TreasureBoxInfo Get(ushort id)
        {
            if (Data is null)
                Load();
            return Data[id];
        }

        public EditType Type { get; set; }
        public string Name { get; set; }

        public enum EditType : int
        {
            DontEdit = 0,
            Edit = 1,
            KeyItem = 2,
            MagicKey = 3
        }
    }
}
