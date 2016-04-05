using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRace.Util
{
    static class CommonStringReader
    {

        public static bool isAlphaNumerical(string x)
        {
            return x.All(char.IsLetterOrDigit);
        }

        public static bool isDigit(string x)
        {
            return x.All(char.IsDigit);
        }



    }
}
