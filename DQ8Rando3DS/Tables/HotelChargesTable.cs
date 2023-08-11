using DQ8Rando3DS.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DQ8Rando3DS.Tables
{
    public class HotelChargesTable
    {
        public const int HeaderLength = 0x10;

        public byte[] Data { get; set; }

        public HotelChargesTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + Hotel.ByteLength <= Data.Length; i += Hotel.ByteLength)
            {
                Hotel hotel = new Hotel(Data, i);
                Contents.Add(hotel.ID, hotel);
            }
        }
        public static HotelChargesTable Load(string path)
        {
            return new HotelChargesTable(File.ReadAllBytes(path));
        }
        public static HotelChargesTable Load()
        {
            return Load("./Raw/hotelCharges.tbl");
        }
        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Saved inn price table to `{path}`.");
        }

        public void CreateSpoilerLog(string path)
        {
            List<string> spoilerLog = new List<string>();
            
            foreach (Hotel hotel in Contents.Values.Where(h => h.GetInfo().DoEdit))
            {
                spoilerLog.Add($"{hotel.GetInfo().Name}: {hotel.Price} G");
            }

            Extensions.WriteAllText(path, string.Join(Environment.NewLine, spoilerLog));
            MainWindow.UpdateStatus($"Wrote hotel charges spoiler log to `{path}`.");
        }

        public Dictionary<byte, Hotel> Contents { get; set; } = new Dictionary<byte, Hotel>();
    }
    public class Hotel
    {
        public const int ByteLength = 0x4;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset+ByteLength));

        public Hotel(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public byte ID { get => Data[0]; set => Data[0] = Data[1] = value; }
        public short Price { get => BitConverter.ToInt16(Data[2..4]); set => Extensions.SetBytes(Data[2..4], BitConverter.GetBytes(value)); }

        public HotelInfo GetInfo() => HotelInfo.Get(ID);
    }
}
