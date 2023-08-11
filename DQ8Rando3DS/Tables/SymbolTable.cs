using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DQ8Rando3DS.Info;

namespace DQ8Rando3DS.Tables
{
    public class SymbolTable
    {
        public const int HeaderLength = 0x10;
        
        public byte[] Data { get; set; }

        public SymbolTable(byte[] data)
        {
            Data = data;

            for (int i = HeaderLength; i + Symbol.ByteLength <= Data.Length; i += Symbol.ByteLength)
            {
                Symbol symbol = new Symbol(Data, i);
                Contents.Add(symbol.ID, symbol);
            }
        }
        public SymbolTable Clone()
        {
            return new SymbolTable(Data[..]);
        }
        public static SymbolTable Load(string path)
        {
            return new SymbolTable(File.ReadAllBytes(path));
        }
        public static SymbolTable Load()
        {
            return Load("./Raw/symbol.tbl");
        }

        public void Save(string path)
        {
            Extensions.WriteAllBytes(path, Data);
            MainWindow.UpdateStatus($"Saved symbol table to `{path}`.");
        }
        public void CreateSpoilerLog(string path)
        {
            // We're actually not going to do that.
        }

        public Dictionary<ushort, Symbol> Contents { get; set; } = new Dictionary<ushort, Symbol>();
    }
    public class Symbol
    {
        public const int ByteLength = 0x2C;

        private byte[] _Data { get; set; }
        public int Offset { get; set; }
        public Span<byte> Data => _Data.AsSpan(Offset..(Offset+ByteLength));

        public Symbol(byte[] data, int offset)
        {
            _Data = data;
            Offset = offset;
        }

        public void CopyTo(Symbol target)
        {
            for (int i = 2; i < Data.Length; i++)
            {
                target.Data[i] = Data[i];
            }
        }
        public void Swap(Symbol target)
        {
            for (int i = 2; i < Data.Length; i++)
            {
                (Data[i], target.Data[i]) = (target.Data[i], Data[i]);
            }
        }

        public ushort ID { get => BitConverter.ToUInt16(Data[0..2]); set => Extensions.SetBytes(Data[0..2], BitConverter.GetBytes(value)); }
        public ushort Monster { get => BitConverter.ToUInt16(Data[4..6]); set => Extensions.SetBytes(Data[4..6], BitConverter.GetBytes(value)); }

        public SymbolInfo GetInfo() => SymbolInfo.Get(ID);
        public MonsterInfo GetMonsterInfo() => MonsterInfo.Get(Monster);
    }
}
