using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK4_Lab1.Algorithms
{
    static class Helpers
    {
        public static byte ToByte(this int i)
        {
            return (byte)(i > 255 ? 255 : i < 0 ? 0 : i);
        }
    }
}
