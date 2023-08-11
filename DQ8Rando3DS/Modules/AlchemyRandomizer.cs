using DQ8Rando3DS.Info;
using DQ8Rando3DS.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQ8Rando3DS.Modules
{
    public class AlchemyRandomizer : RandomizerModule
    {
        public AlchemyTable AlchemyTable { get; set; }
        
        public override void Start()
        {
            if (Options.Alchemy.NoChange)
                return;

            MainWindow.UpdateStatus("Randomizing alchemy recipes...");

            AlchemyTable = AlchemyTable.Load("./Raw/renkinRecipe.tbl");

            if (Options.Alchemy.ResultRandom || Options.Alchemy.AllRandom)
                RandomizeResults();
            if (Options.Alchemy.IngredientRandom || Options.Alchemy.AllRandom)
                RandomizeIngredients();

            if (Options.Alchemy.Shuffle)
                ShuffleRecipes();

            Save();
        }

        public void RandomizeResults()
        {
            foreach (AlchemyRecipe recipe in AlchemyTable.Contents.Values)
            {
                recipe.Result = ItemInfo.GetRandomItem(RNG).ID;
            }
        }
        public void RandomizeIngredients()
        {
            foreach (AlchemyRecipe recipe in AlchemyTable.Contents.Values)
            {
                recipe.Ingredient1 = ItemInfo.GetRandomItem(RNG).ID;
                recipe.Ingredient2 = ItemInfo.GetRandomItem(RNG).ID;
                recipe.Ingredient3 = RNG.NextBool() ? ItemInfo.GetRandomItem(RNG).ID : (ushort)0;
            }
        }

        public void ShuffleRecipes()
        {
            List<ushort> results = new List<ushort>();
            foreach (AlchemyRecipe recipe in AlchemyTable.Contents.Values)
            {
                results.Add(recipe.Result);
            }
            results = results.Shuffle(RNG).ToList();

            foreach (AlchemyRecipe recipe in AlchemyTable.Contents.Values)
            {
                recipe.Result = results[0];
                results.RemoveAt(0);
            }
        }

        public void Save()
        {
            AlchemyTable.Save($"{Options.Path}/romfs/data/Params/renkinRecipe.tbl");
            AlchemyTable.CreateSpoilerLog($"{Options.Path}/spoiler/AlchemyRecipe.txt");
        }
    }
}
