using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace DQ8Rando3DS.Tables
{
    public class BookList
    {
        public string[] Data { get; set; }

        public BookList(string data)
        {
            Data = data.Split("\n");
            Contents = new Book[Data.Length];

            for (int i = 0; i < Contents.Length; i++)
            {
                Contents[i] = new Book(Data, i);
            }
        }
        public static BookList Load(string path)
        {
            return new BookList(File.ReadAllText(path));
        }
        public static BookList Load()
        {
            return Load("./Raw/bookList.cfg");
        }
        public void Save(string path)
        {
            Extensions.WriteAllText(path, string.Join("\n", Data));
            MainWindow.UpdateStatus($"Saved book list to `{path}`.");
        }

        public Book[] Contents { get; set; }
    }
    public class Book
    {
        public string[] Data { get; set; }
        public int LineIndex { get; set; }
        public string String { get => Data[LineIndex]; set => Data[LineIndex] = value; }

        public Book(string[] data, int lineIndex)
        {
            Data = data;
            LineIndex = lineIndex;
        }

        public string Name {
            get => !IsEmpty() ? Name_Pattern.Match(String).Value : null;
            set => String = Name_Pattern.Replace(String, value);
        }
        protected static readonly Regex Name_Pattern = new("(?<=\").*(?=\")");

        public int? ID {
            get => !IsEmpty() ? int.Parse(ID_Pattern.Match(String).Value) : null;
            set => String = ID_Pattern.Replace(String, value.ToString());
        }
        protected static readonly Regex ID_Pattern = new("(?<=1,)\\d+(?=,0,-1,0,-1,\\d+,0;$)");

        public bool IsEmpty() => !Data[LineIndex].StartsWith("TREASURE");
        public bool DoEdit() => !IsEmpty() && ID != 10000; // 10000 is the Trodain ship book
    }
}
