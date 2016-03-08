using MathRace.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MathRace.Protocol
{

    internal class Parser
    {

        private static int NUMBER = 0;
        private static int BOOLEAN = 1;
        private static int STRING = 2;

        private static int NUMBER_MESSAGE_SIZE_LIMIT = 4;
        private static int BOOLEAN_MESSAGE_SIZE_LIMIT = 1;

        private static int eightbit = 0;
        private static int sixteenbit = 1;
        private static int thirtytwobit = 2;

        /// <summary>
        /// Stored static functions and fields contain information on the fields that are contained within the payload of 
        /// an EV3 bluetooth message.
        /// 
        /// Each EV3 message contains a Title (mailbox title) and a Value.
        /// 
        /// Example payload (actually captured from an EV3)
        /// -----------------------------------------------          
        ///            
        ///                 Field names:
        ///                 -------------
        /// Byte:  0  0x12  PayloadSize : Byte 0 and 1 contain the size (bytes) of the payload.
        ///        1  0x00                The size counting starts at byte 2. Here: 00 12 (hex) = 18 (dec)
        ///        -------
        ///        2  0x01  SecretHeader: Byte 2..5 contain a header (just a guess)
        ///        3  0x00                The contents are unknown, but every message has these values here.
        ///        4  0x81                 
        ///        5  0x9E
        ///        -------
        ///        6  0x05  TitleSize   : Byte 6 contains the size (bytes) of the Title field, which follows.
        ///        -------
        ///        7  0x70  Title       : The title text string (ascii). Here: "ping"
        ///        8  0x69                The last character is always 0x00.
        ///        9  0x6E               
        ///       10  0x67
        ///       11  0x00
        ///       --------
        ///       12  0x06  ValueSize   : Byte (6 + TitleSize + 1) contains the size (bytes) of the Value field which follows.
        ///       13  0x00                Here: 00 06 (hex) = 6 (dec)
        ///       --------
        ///       14  0x68  Value       : The value. Here the text: "hello"
        ///       15  0x65                The value field can contain:
        ///       16  0x6C                - Text (length: variable, ends with 0x00) ---> string                 
        ///       17  0x6C                - Number (length: 4 bytes)                ---> float (Single)
        ///       18  0x6F                - Logic (length: 1 byte)                  ---> bool
        ///       19  0x00
        ///       
        /// Special thanks ot Joeri van Belle for deciphering this information.
        ///
        /// </summary>

        private static bool isEV3_LittleEndian = true;
        private static byte[] mark = { 0x01, 0x00, 0x81, 0x9E };
        private static List<byte> mark_read = new List<byte> { 0x01, 0x00, 0x81, 0x9E };
        private static int PayloadSizeIndex = 0;
        private static int MarkIndex = 2;
        private static int TitleSizeIndex = 6;
        private static int TitleIndex = 7;
        private static int MessageSizeIndex(int titlesize)
        {
            return 1 + TitleIndex + titlesize;
        }
        private static int MessageIndex(int titlesize)
        {
            return MessageSizeIndex(titlesize) + 2;
        }


        public static byte[] toEV3(string title, float message)
        {

            byte[] x = convertToBytes(title);
            byte[] y = convertToBytes(message);
            return combine(x, y, NUMBER);

        }

        public static byte[] toEV3(string title, int message)
        {

            float x = (float)message;
            return toEV3(title, x);

        }

        public static byte[] toEV3(string title, string message)
        {

            byte[] x = convertToBytes(title);
            byte[] y = convertToBytes(message);
            return combine(x, y, STRING);

        }

        public static byte[] toEV3(string title, bool message)
        {

            byte[] x = convertToBytes(title);
            byte[] y = convertToBytes(message);
            return combine(x, y, BOOLEAN);

        }

        /// <summary>
        /// The function receives raw data from the buffer handler and returns a Tuple containing the title and the message in byte[] form.
        /// </summary>

        public static Tuple<string, List<byte>> fromEV3(List<byte> raw)
        {

            Tuple<bool, int> x;

            // -----
            // Searches for whether there is a mark in the raw data. If not, throws an exception.

            x = searchMark(raw);


            if (!x.Item1)
            {
                throw new Exception("MathRace.Protocol.Parser.fromEV3: no Mark found.");
            }

            if (x.Item2 < 2)
            {
                throw new Exception("MathRace.Protocol.Parser.fromEV3: raw data contains no payload size info.");
            }

            Debug.Print("Mark is searched.");

            // -----
            // Read the raw data and extracts title and message from a payload.

            // a = x.Item2 - 2;
            // b = x.Item2;
            int c = x.Item2 + 4;
            // d = x.Item2 + 5;

            int titlesize = (int)raw[c];

            int e = x.Item2 + 6 + titlesize;
            // f = x.Item2 + 6 + titlesize + 2;

            Debug.Print("c is " + c + " titlesize is " + titlesize + " e is " + e);
            Debug.Print("raw[e] is " + raw[e] + " raw [e + 1] is " + raw[e + 1]);

            int messagesize = convertToInteger(raw.GetRange(e, 2));

            string title = convertTostring(raw.GetRange(c+1, titlesize));
            List<byte> message = raw.GetRange(e+2, messagesize);

            return new Tuple<string, List<byte>>(title, message);

        }

        /// <summary>
        /// Searches raw data for mark, and returns a Tuple containing whether a mark is present, and the index of the mark within the raw data.
        /// </summary>

        public static Tuple<bool, int> searchMark(List<byte> rawdata)
        {

            var x = mark_read.Count;
            var y = rawdata.Count - x;

            for (var i = 0; i <= y; i++)
            {

                var k = 0;
                for (; k < x; k++)
                {

                    if (mark[k] != rawdata[i + k])
                    {
                        break;
                    }

                }

                if (k == x)
                {

                    return new Tuple<bool, int>(true, i);

                }

            }

            return new Tuple<bool, int>(false, -1);

        }

        /// <summary>
        /// Returns where the raw data ends, to be used to split arrays in the buffer. Use searchMark to confirm there is a Mark; this function
        /// checks for a Mark and throws an exception if none are present.
        /// </summary>

        public static Tuple<int, int> getSizeandEnd(List<byte> rawdata, int markindex)
        {

            int x = markindex;

            if (rawdata[x] == mark[0])
            {
                if (rawdata[x + 1] == mark[1])
                {
                    if (rawdata[x + 2] == mark[2])
                    {
                        if (rawdata[x + 3] == mark[3])
                        {

                            try
                            {

                                var y = convertToInteger(rawdata.GetRange(x - 2, 2));

                                return new Tuple<int, int>(y, y + markindex - 2);

                            } catch (Exception e)
                            {
                                Debug.Print("MathRace.Protocol.Parser.searchEnd: raw data is missing payloadsize header.");
                                Debug.Print(e.Message);
                            }

                        }
                    }
                }
            }
            return null;

        }

        /// <summary>
        /// Combine data into payload suitable to send to EV3.
        /// </summary>

        private static byte[] combine(byte[] title, byte[] message, int type)
        {

            if (title == null)
            {
                throw new Exception("MathRace.Protocol.Parser.combine: title == null");
            }
            if (message == null)
            {
                throw new Exception("MathRace.Protocol.Parser.combine: message == null");
            }

            if (type == Parser.NUMBER && (message.Length > Parser.NUMBER_MESSAGE_SIZE_LIMIT))
            {
                throw new Exception("MathRace.Protocol.Parser.combine: Input message as NUMBER exceeded byte limit");
            }
            if (type == Parser.BOOLEAN && (message.Length != Parser.BOOLEAN_MESSAGE_SIZE_LIMIT))
            {
                throw new Exception("MathRace.Protocol.Parser.combine: Input message as BOOLEAN not 1 byte");
            }

            byte[] titlesize = new byte[0];
            byte[] messagesize = new byte[0];
            byte[] payloadsize = new byte[0];

            int x = 0;
            int y = 0;

            bool isstring = false;

            x = title.Length;
            titlesize = convertToBytes(title.Length, eightbit);

            if (type == Parser.NUMBER)
            {
                y = NUMBER_MESSAGE_SIZE_LIMIT;
                messagesize = convertToBytes(NUMBER_MESSAGE_SIZE_LIMIT, sixteenbit);
            }
            else if (type == Parser.BOOLEAN)
            {
                y = BOOLEAN_MESSAGE_SIZE_LIMIT;
                messagesize = convertToBytes(BOOLEAN_MESSAGE_SIZE_LIMIT, sixteenbit);
            }
            else if (type == Parser.STRING)
            {
                y = message.Length + 1;
                // + 1: 0x00 tail on Message if string
                messagesize = convertToBytes(message.Length, sixteenbit);
                isstring = true;
            }

            // + 1: Title header
            // + 2: Message header
            // + 4: Mark header
            // + 1: 0x00 tail on Title

            int z = x + y + 1 + 2 + 4 + 1;

            Debug.Print("MathRace.Protocol.Parser.combine: " + "current raw title data is: " + BitConverter.ToString(title));
            Debug.Print("MathRace.Protocol.Parser.combine: " + "current raw message data is: " + BitConverter.ToString(message));
            Debug.Print("MathRace.Protocol.Parser.combine: " + "current x-value is: " + x);
            Debug.Print("MathRace.Protocol.Parser.combine: " + "current y-value is: " + y);
            Debug.Print("MathRace.Protocol.Parser.combine: " + "current z-value is: " + z);

            payloadsize = convertToBytes(z, sixteenbit);

            byte[] a = new byte[4];
            a = mark;

            byte[] blank = { 0x00 };

            var outcome = new byte[2 + z];
            payloadsize.CopyTo(outcome, PayloadSizeIndex);
            a.CopyTo(outcome, MarkIndex);
            titlesize.CopyTo(outcome, TitleSizeIndex);
            title.CopyTo(outcome, TitleIndex);
            blank.CopyTo(outcome, MessageSizeIndex(x) - 1);
            messagesize.CopyTo(outcome, MessageSizeIndex(x));
            message.CopyTo(outcome, MessageIndex(x));
            if (isstring)
            {
                blank.CopyTo(outcome, z + 1);
            }

            Debug.Print("MathRace.Protocol.Parser.combine: " + "current outcome: " + BitConverter.ToString(outcome));

            return outcome;

        }

        public static float convertToNumber(List<byte> data)
        {

            if (data == null)
            {
                throw new Exception("MathRace.Protocol.Parser.convertToNumber: data == null.");

            }

            float x = 0;

            if (data.Count == 4)
            {
                x = BitConverter.ToSingle(data.ToArray(), 0);
                Debug.Print("MathRace.Protocol.Parser.convertToNumber: " + x);
            }
            else {
                throw new Exception("MathRace.Protocol.Parser.convertToNumber: " + "Length /= 4");
            }

            return x;

        }

        public static int convertToInteger(List<byte> data)
        {

            if (data == null)
            {
                throw new Exception("MathRace.Protocol.Parser.convertToInteger: data == null.");

            }

            int x = 0;

            if (data.Count == 4)
            {
                x = BitConverter.ToInt32(data.ToArray(), 0);
            }
            else if (data.Count == 2)
            {
                x = BitConverter.ToInt16(data.ToArray(), 0);
            }
            else
            {
                throw new Exception("MathRace.Protocol.Parser.convertToInteger: Not Int32 or Int16.");
            }
            Debug.Print("MathRace.Protocol.Parser.convertToInteger: " + x);

            return x;

        }

        public static bool convertToLogic(List<byte> data)
        {

            if (data == null)
            {
                throw new Exception("MathRace.Protocol.Parser.convertToLogic: data == null.");

            }

            bool x = false;

            if (data.Count == 1)
            {
                x = BitConverter.ToBoolean(data.ToArray(), 0);
                Debug.Print("MathRace.Protocol.Parser.convertToLogic: " + x);
            }
            else {
                Debug.Print("MathRace.Protocol.Parser.convertToLogic: " + "Length /= 1");
            }

            return x;

        }

        public static string convertTostring(List<byte> data)
        {

            if (data == null)
            {
                throw new Exception("MathRace.Protocol.Parser.convertTostring: data == null.");

            }

            string x = "";

            UTF8Encoding y = new UTF8Encoding();

            x = y.GetString(data.ToArray());
            Debug.Print("MathRace.Protocol.Parser.convertTostring: " + x);

            return x;

        }

        private static byte[] convertToBytes(float data)
        {

            byte[] x = new byte[0];
            x = BitConverter.GetBytes(data);

            Debug.Print("MathRace.Protocol.Parser.convertToBytes(float): " + BitConverter.ToString(x));

            return x;

        }

        private static byte[] convertToBytes(int data, int type)
        {

            byte[] x = new byte[1] { 0 };

            if (type == eightbit)
            {
                if (data < 0 || data > 255)
                {
                    throw new Exception("MathRace.Protocol.Parser.convertToBytes(int): data input exceeds 8-bit limit. ");
                }
                x = new byte[] { (byte)data };
            }
            else if (type == sixteenbit)
            {
                if (data < (-32768) || data > 32767)
                {
                    throw new Exception("MathRace.Protocol.Parser.convertToBytes(int): data input exceeds 16-bit limit. ");
                }
                x = BitConverter.GetBytes((short)data);
            }
            else if (type == thirtytwobit)
            {
                x = BitConverter.GetBytes(data);
            }

            Debug.Print("MathRace.Protocol.Parser.convertToBytes(int): the bit size is " + type);
            Debug.Print("MathRace.Protocol.Parser.convertToBytes(int): " + BitConverter.ToString(x));

            return x;

        }

        private static byte[] convertToBytes(bool data)
        {

            byte[] x;
            x = BitConverter.GetBytes(data);

            Debug.Print("MathRace.Protocol.Parser.convertToBytes(bool): " + BitConverter.ToString(x));

            return x;
        }

        // EV3 chars are encoded in UTF-8

        private static byte[] convertToBytes(string data)
        {

            if (data == null)
            {
                throw new Exception("MathRace.Protocol.Parser.convertToBytes: string data == null.");

            }

            UTF8Encoding y = new UTF8Encoding();

            byte[] x = new byte[0];
            x = y.GetBytes(data);

            Debug.Print("MathRace.Protocol.Parser.convertToBytes(String): " + BitConverter.ToString(x));

            return x;

        }

    }

}
