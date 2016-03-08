using MathRace.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRace.Protocol
{
    internal class Payload
    {

        public byte[] data_parsed
        {
            get;
            private set;
        }

        public string title
        {
            get;
            private set;
        }

        public bool data_bool
        {
            get;
            private set;
        }

        public float data_float
        {
            get;
            private set;
        }

        public string data_string
        {
            get;
            private set;
        }

        public int type
        {
            get;
            private set;
        }

        public Payload(byte[] data)
        {
            if(data == null)
            {
                throw new Exception("MathRace.Protocol.Payload: Null data");
            }

            this.data_parsed = data;

        }

        public Payload(string title, bool data) : this(Parser.toEV3(title, data))
        {

        }

        public Payload(string title, float data) : this(Parser.toEV3(title, data))
        {

        }

        public Payload(string title, string data) : this(Parser.toEV3(title, data))
        {

        }

        public Payload(string title, List<byte> rawdata)
        {
            var x = Titles.checkType(title);
            if (x.Item1)
            {
                if(x.Item2 == 0)
                {
                    data_bool = Parser.convertToLogic(rawdata);
                    type = x.Item2;
                }
                else if(x.Item2 == 1)
                {
                    data_float = Parser.convertToNumber(rawdata);
                    type = x.Item2;
                }
                else if(x.Item2 == 2)
                {
                    data_string = Parser.convertTostring(rawdata);
                    type = x.Item2;
                }
            }
        }

    }
}
