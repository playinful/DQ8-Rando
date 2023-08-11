using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DQ8Rando3DS.Tables
{
    public class AlchemyTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public AlchemyTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + AlchemyRecipe.ByteLength <= Data.Length; i += AlchemyRecipe.ByteLength)
            {
                AlchemyRecipe recipe = new AlchemyRecipe(Data, i);
                Contents.Add(recipe.ID, recipe);
            }
        }
        public static AlchemyTable Load(string path)
        {
            return new AlchemyTable(File.ReadAllBytes(path));
        }
        public static AlchemyTable Load()
        {
            return Load("./Raw/renkinRecipe.bin");
        }
        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Saved alchemy recipe table to `{path}`.");
        }

        public void CreateSpoilerLog(string path)
        {
            List<string> spoilerLog = new List<string>();

            foreach (AlchemyRecipe recipe in Contents.Values)
            {
                List<string> ingredients = new List<string>{ Info.ItemInfo.Get(recipe.Ingredient1).Name, Info.ItemInfo.Get(recipe.Ingredient2).Name };
                if (recipe.Ingredient3 != 0)
                    ingredients.Add(Info.ItemInfo.Get(recipe.Ingredient3).Name);

                spoilerLog.Add($"{string.Join(" + ", ingredients)} = {Info.ItemInfo.Get(recipe.Result).Name}");
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, spoilerLog));
            MainWindow.UpdateStatus($"Wrote alchemy spoiler log to `{path}`.");
        }

        public Dictionary<ushort, AlchemyRecipe> Contents { get; set; } = new Dictionary<ushort, AlchemyRecipe>();
    }
    public class AlchemyRecipe
    {
        public const int ByteLength = 0xC;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset + ByteLength));

        public AlchemyRecipe(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
        public ushort Result { get => BitConverter.ToUInt16(Data[2..4]); set => Extensions.SetBytes(Data[2..4], BitConverter.GetBytes(value)); }
        public ushort Ingredient1 { get => BitConverter.ToUInt16(Data[4..6]); set => Extensions.SetBytes(Data[4..6], BitConverter.GetBytes(value)); }
        public ushort Ingredient2 { get => BitConverter.ToUInt16(Data[6..8]); set => Extensions.SetBytes(Data[6..8], BitConverter.GetBytes(value)); }
        public ushort Ingredient3 { get => BitConverter.ToUInt16(Data[8..0xA]); set => Extensions.SetBytes(Data[8..0xA], BitConverter.GetBytes(value)); }
        public ushort Sort { get => BitConverter.ToUInt16(Data[0xA..0xC]); set => Extensions.SetBytes(Data[0xA..0xC], BitConverter.GetBytes(value)); }
    }
}
