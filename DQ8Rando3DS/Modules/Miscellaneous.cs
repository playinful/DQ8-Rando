using DQ8Rando3DS.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // TODO
        }
    }
}
