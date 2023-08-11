using DQ8Rando3DS.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQ8Rando3DS.Modules
{
    public class BookRandomizer : RandomizerModule
    {
        public BookList BookList { get; set; }
        
        public override void Start()
        {
            if (!Options.Alchemy.BookShuffle)
                return;

            MainWindow.UpdateStatus("Shuffling bookshelves...");

            BookList = BookList.Load("./Raw/BookList.cfg");

            if (Options.Alchemy.BookShuffle)
                ShuffleBooks();

            Save();
        }

        public void ShuffleBooks()
        {
            List<int> sourceIds = new List<int>();
            foreach (Book book in BookList.Contents.Where(b => b.DoEdit()))
            {
                if (book.ID is not null && !sourceIds.Contains((int)book.ID))
                    sourceIds.Add((int)book.ID);
            }
            IEnumerable<int> targetIds = sourceIds.Shuffle(RNG);
            Dictionary<int, int> idPairs = sourceIds.Zip(targetIds, (k,v) => new {k,v}).ToDictionary(x => x.k, x => x.v); // more shit copied from stackoverflow lol i should use this anonymous thing more often

            foreach (Book book in BookList.Contents.Where(b => b.DoEdit()))
            {
                book.ID = idPairs[(int)book.ID];
            }
        }

        public void Save()
        {
            BookList.Save($"{Options.Path}/romfs/data/Script/field/BookList/BookList.cfg");
        }
    }
}
