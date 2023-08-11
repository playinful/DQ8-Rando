using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQ8Rando3DS
{
    public class RandomizerModule
    {
        public RandomizerOptions Options;
        public Random RNG { get; set; }

        public virtual void Start()
        {
        }
        public void Initialize(RandomizerOptions options, Random rng)
        {
            Options = options;
            RNG = rng;
        }
    }
}
