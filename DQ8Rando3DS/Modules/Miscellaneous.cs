using DQ8Rando3DS.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Printing.IndexedProperties;

namespace DQ8Rando3DS.Modules
{
    public class Miscellaneous : RandomizerModule
    {
        public override void Start()
        {
            // write seed string
            MessageFile msg_record = MessageFile.Load("./Raw/msg_record.binE");
            msg_record.Strings[17] = "Battle Log                        Seed: " + Options.Seed.ToString();
            msg_record.Save($"{Options.Path}/romfs/data/Message/eng/msg_record.binE");
            MainWindow.UpdateStatus("Wrote seed information to message table.");

            // patch WordTable
            byte[] wordTable = File.ReadAllBytes("./Raw/word.txt");
            List<string> words = Encoding.UTF8.GetString(wordTable[3..]).Split("\r\n").ToList();
            for (int i = 0; i < words.Count; i++)
            {
                string word = words[i];
                if (word.StartsWith(";insertDummy"))
                {
                    words.RemoveAt(i);
                    for (int count = int.Parse(new Regex("(?<=^;insertDummy\\()\\d*(?=\\))").Match(word).Value); count > 0; count--)
                    {
                        words.Insert(i, "\t\t0\t0\t0\t0\t2\t0");
                    }
                }
            }
            wordTable = wordTable[..3].Concat(Encoding.UTF8.GetBytes(string.Join("\r\n", words))).ToArray();
            Extensions.WriteAllBytes($"{Options.Path}/romfs/data/Params/WordTable/eng/word.txt", wordTable);

            // TODO
        }
    }
}
