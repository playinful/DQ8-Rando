using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DQ8Rando3DS.Tables;

namespace DQ8Rando3DS.Modules
{
    public class HotelChargesRandomizer : RandomizerModule
    {
        public HotelChargesTable HotelCharges { get; set; }
        // TODO
        public override void Start()
        {
            if (!Options.Shopping.HotelRandomPrices)
                return;

            MainWindow.UpdateStatus("Randomizing inn fares...");

            HotelCharges = HotelChargesTable.Load("./Raw/hotelCharges.tbl");

            RandomizeHotelPrices();

            Save();
        }

        public void RandomizeHotelPrices()
        {
            foreach (Hotel hotel in HotelCharges.Contents.Values.Where(h => h.GetInfo().DoEdit))
            {
                hotel.Price = (short)RNG.Next(Options.Shopping.HotelMinimumPrice, Options.Shopping.HotelMaximumPrice+1);
            }
        }

        public void Save()
        {
            // TODO
            HotelCharges.Save($"{Options.Path}/romfs/data/Params/hotelCharges.tbl");
            HotelCharges.CreateSpoilerLog($"{Options.Path}/spoiler/HotelCharges.txt");
        }
    }
}
