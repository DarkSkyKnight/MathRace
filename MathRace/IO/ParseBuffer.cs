using MathRace.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRace.IO
{
    internal class ParseBuffer
    {

        private List<byte> dataToParse;

        public ParseBuffer()
        {
            dataToParse = new List<byte>();
        }

        public void Append(List<byte> data)
        {
            if(data != null && data.Count > 0)
            {
                dataToParse.AddRange(data);
            }
        }

        public Payload readNext()
        {
            var x = Parser.searchMark(this.dataToParse);
            if (x.Item1)
            {
                int markIndex = x.Item2;
                Tuple<int, int> size_end = Parser.getSizeandEnd(this.dataToParse, markIndex);

                if(dataToParse.Count >= size_end.Item2)
                {
                    var a = Parser.fromEV3(dataToParse.GetRange(markIndex - 2, size_end.Item1));

                    dataToParse.RemoveRange(markIndex - 2, size_end.Item1);

                    return new Payload(a.Item1, a.Item2);
                }

            }
            else
            {
                dataToParse.Clear();
            }
            return null;
        }

        public void clear()
        {
            dataToParse.Clear();
        }

    }
}
