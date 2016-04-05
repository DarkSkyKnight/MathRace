using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRace.IO;
using MathRace.Protocol;
using MathRace.Util;
using System.Diagnostics;

namespace MathRace.Engine
{
    class MathRace_test
    {

        private Communicator com;

        public bool IsConnected
        {
            get
            {
                return com.IsConnected;
            }
        }

        public MathRace_test()
        {
            com = new Communicator();
        }

        public bool Disconnect()
        {
            return com.Disconnect();
        }

        public bool Send(string title, bool message)
        {
            if (!checkTitle(title))
            {
                throw new Exception("title error");
            }
            Payload payload = new Payload(title, message);
            return com.Send(payload);
        }

        public bool Send(string title, float message)
        {
            if (!checkTitle(title))
            {
                throw new Exception("title error");
            }
            Payload payload = new Payload(title, message);
            return com.Send(payload);
        }

        public bool Send(string title, int message)
        {
            return Send(title, (float)message);
        }

        public bool Send(string title, string message)
        {
            if (!checkTitle(title))
            {
                throw new Exception("title error");
            }
            if (!checkTitle(message))
            {
                throw new Exception("message error");
            }
            Payload payload = new Payload(title, message);
            return com.Send(payload);
        }

        public void ReadandPrint()
        {
            Payload payload = com.ReadNext();
            if(payload != null)
            {
                Debug.Print("Title: " + payload.title);
                if(payload.type == 0)
                {
                    Debug.Print("Message: " + payload.data_bool);
                }
                else if(payload.type == 1)
                {
                    Debug.Print("Message: " + payload.data_float);
                }
                else if(payload.type == 2)
                {
                    Debug.Print("Message: " + payload.data_string);
                }
            }
        }

        private bool checkTitle(string title)
        {
            if(title == null)
            {
                return false;
            }
            if(title.Length > 254)
            {
                return false;
            }
            return true;
        }

    }
}
