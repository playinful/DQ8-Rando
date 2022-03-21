using System.Windows.Controls;
using System.Collections.Generic;

namespace DQ8Rando
{
    public class Option
    {
        public string Type { get; set; } = "none";
        public string Text { get; set; } = "";
        public int Indent { get; set; } = 0;
        public string[] Controls { get; set; } = new string[0];
        public bool Show { get; set; } = true;
    }

    public class OptionBase
    {
        public string Element { get; set; }
        public string Parent { get; set; }
        public OptionChoice[] Elements { get; set; }
        public string Type { get; set; }
        public Control Control { get; set; }
        public Control ParentControl { get; set; }
    }
    public class OptionTab : OptionBase
    {
        public string Header { get; set; }
        public OptionLegacy[] Contents { get; set; }
    }
    public class OptionLegacy : OptionBase
    {
        public string Text { get; set; }
        public int Indent { get; set; }
    }
    public class OptionChoice : OptionLegacy
    {

    }
    public class OutputOption
    {
        public string Element { get; set; }
        public string StringValue { get; set; }
        public bool BoolValue { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
    }
    public class OutputOptionFile
    {
        public int Seed { get; set; } = -1;
        public string Path { get; set; }
        public List<OutputOption> Elements { get; set; } = new List<OutputOption>();
    }
}
