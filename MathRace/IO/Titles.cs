using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRace.IO
{
    internal class Titles
    {

        ///<summary>
        /// type 0: bool
        /// type 1: int
        /// type 2: string
        ///</summary>

        public static Tuple<string, int> SPEED = new Tuple<string, int>("speed", 1);
        public static Tuple<string, int> DISPLAYMESSAGE = new Tuple<string, int>("displaymessage", 2);
        public static Tuple<string, int> HASBUMPED = new Tuple<string, int>("hasbumped", 0);
        public static Tuple<string, int> ISCOLORED = new Tuple<string, int>("iscolored", 0);
        public static Tuple<string, int> DISTTOOBSTACLE = new Tuple<string, int>("disttoobstacle", 1);
        public static Tuple<string, int> STEERLEFT = new Tuple<string, int>("steerleft", 1);
        public static Tuple<string, int> STEERRIGHT = new Tuple<string, int>("steerright", 1);

        public static List<Tuple<string, int>> TITLES = new List<Tuple<string, int>> { SPEED, DISPLAYMESSAGE,
        HASBUMPED, ISCOLORED, DISTTOOBSTACLE, STEERLEFT, STEERRIGHT };

        public static Tuple<bool, int> checkType(string x)
        {
            for( int i = 0; i < TITLES.Count; i++ )
            {
                if(x == TITLES[i].Item1)
                {
                    return new Tuple<bool, int>(true, TITLES[i].Item2);
                }
            }

            return new Tuple<bool, int>(false, -1);
        }

    }
}
