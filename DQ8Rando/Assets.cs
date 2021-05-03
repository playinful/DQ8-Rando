using System.Windows.Controls;

namespace DQ8Rando
{
    public class AssetFile
    {
        public string Source { get; set; }
        public AssetPath[] Output { get; set; }
        public string Condition { get; set; }
        public Control ConditionControl { get; set; }
    }
    public class AssetPath
    {
        public string Path { get; set; }
        public string Filename { get; set; }
    }
}
