using System.Windows.Controls;

namespace DQ8Rando
{
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
        public Option[] Contents { get; set; }
    }
    public class Option : OptionBase
    {
        public string Text { get; set; }
        public int Indent { get; set; }
    }
    public class OptionChoice : Option
    {

    }
}
