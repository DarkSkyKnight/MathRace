using MathRace.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MathRace.IO
{
    internal class ParseBuffer
    {

        private List<byte> dataToParse;

        public List<Payload> payloads = new List<Payload>();

        private NetworkStream stream;

        public byte[] buf = new byte[1024];

        public ParseBuffer()
        {
            dataToParse = new List<byte>();
        }

        public void setstream(NetworkStream str)
        {
            stream = str;
        }

        public void Append(List<byte> data)
        {
            if(data != null && data.Count > 0)
            {
                dataToParse.AddRange(data);
            }
        }

        public void beginread()
        {
            stream.BeginRead(buf, 0, 1024, new AsyncCallback(endread), stream);
        }

        public void endread(IAsyncResult ar)
        {
            Append(new List<byte>(buf));

            Payload payload;
            while(readNext(out payload))
            {
                if (payloads.Any() && payload.Equals(payloads.Last())) // use payloads.Any() to prevent payloads.Last() from throwing an exception
                {

                }
                else
                {
                    payloads.Add(payload);
                }
                
            }
            beginread();
        }

        public bool readNext(out Payload pl)
        {

            pl = null;

            var x = Parser.searchMark(this.dataToParse);
            if (x.Item1)
            {
                int markIndex = x.Item2;
                Tuple<int, int> size_end = Parser.getSizeandEnd(this.dataToParse, markIndex);

                if(size_end == null)
                {
                    Debug.Print("remove range in readNext");

                    dataToParse.RemoveRange(0, markIndex + 4 - 1);
                }

                else if(dataToParse.Count >= size_end.Item2)
                {
                    var a = Parser.fromEV3(dataToParse.GetRange(markIndex - 2, size_end.Item1 + 2));

                    dataToParse.RemoveRange(markIndex - 2, size_end.Item1 + 2);

                    pl = new Payload(a.Item1, a.Item2);

                    

                    return true;
                }

            }
            else
            {
                dataToParse.Clear();
            }
            return false;
        }

        public void clear()
        {
            dataToParse.Clear();
        }

    }
}
