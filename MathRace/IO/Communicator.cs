using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Collections.Concurrent;
using MathRace.Protocol;

namespace MathRace.IO
{
    class Communicator
    {

        private ParseBuffer buffer;
        private ConcurrentQueue<Payload> queue;
        private SerialPort port;
        private Object connectlock;

        public bool IsConnected
        {
            get
            {
                return port.IsOpen;
            }
        }

        public string portinfo
        {
            get
            {
                try
                {
                    if (port.IsOpen)
                    {
                        return port.PortName;
                    }
                }
                catch(Exception e)
                {
                    Debug.Print(e.Message);
                }
                return null;
            }
        }

        public Communicator()
        {
            buffer = new ParseBuffer();
            queue = new ConcurrentQueue<Payload>();

            port = new SerialPort();
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            connectlock = new Object();
        }

        public bool connect(string portname)
        {
            lock (connectlock)
            {
                if(!port.IsOpen && IsValidPort(portname))
                {
                    buffer.clear();
                    ClearQueue();
                    try
                    {
                        port.PortName = portname;
                        port.Open();
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                        return true;
                    }
                    catch(Exception e)
                    {
                        Debug.Print(e.Message);
                    }
                }
                return false;
            }
        }

        public bool Send(Payload payload)
        {
            if(payload == null)
            {
                throw new Exception("MathRace.IO.Communicator: payload is null when sending");
            }

            if (port.IsOpen)
            {
                try
                {
                    port.Write(payload.data_parsed, 0, payload.data_parsed.Length);
                }
                catch(Exception e)
                {
                    Debug.Print(e.Message);
                }
            }

            return false;
        }

        public bool Disconnect()
        {
            lock (connectlock)
            {
                if (port.IsOpen)
                {
                    try
                    {
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                        port.Close();

                        buffer.clear();
                        ClearQueue();
                        return true;
                    }
                    catch(Exception e)
                    {
                        Debug.Print(e.Message);
                    }
                }
            }
            return false;
        }

        private bool IsValidPort(string portname)
        {
            if(portname != null)
            {
                string[] portNames = SerialPort.GetPortNames();
                foreach(string name in portNames)
                {
                    if(name.ToUpper() == portname.ToUpper())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Payload ReadNext()
        {
            Payload payload;
            if(queue.TryDequeue(out payload)){
                return payload;
            }
            return null;
        }

        private void ClearQueue()
        {
            while(queue.Count > 0)
            {
                Payload payload;
                queue.TryDequeue(out payload);
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sPort = (SerialPort)sender;
            if (sPort.IsOpen)
            {
                try
                {

                    if(sPort.BytesToRead > 0)
                    {
                        List<byte> rawdata = new List<byte>();
                        buffer.Append(rawdata);
                        Payload payload = buffer.readNext();
                        while(payload != null)
                        {
                            queue.Enqueue(payload);
                            payload = buffer.readNext();
                        }
                    }
                }
                catch(Exception xe)
                {
                    Debug.Print(xe.Message);
                }
            }
        }

    }
}
