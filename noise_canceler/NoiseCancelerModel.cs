using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace noise_canceler
{
    class NoiseCancelerModel
    {
        public int Delay { get; set; }
        public double LeftWeight { get; set; }
        public double RightWeight { get; set; }
        public double[] FreqWeight { get; set; }
    }
}
