using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Collections.Concurrent;
using MathRace.Protocol;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using System.Management;
using MathRace.Util;
using System.Net.Sockets;
using System.Threading;

namespace MathRace.IO
{
    class Communicator
    {


        private BluetoothEndPoint lep;
        private BluetoothClient client;
        private BluetoothComponent lc;

        private BluetoothDeviceInfo theev3;

        private ParseBuffer buffer;

        public bool getbufferpayload(out Payload payload)
        {
            if (buffer.payloads.Any())
            {
                payload = buffer.payloads.First();
                buffer.payloads.RemoveAt(0);
                return true;
            }
            else
            {
                payload = null;
                return false;
            }
        }

        private NetworkStream stream;
        private SerialPort port;
        private Object connectlock;

        public readonly List<BluetoothDeviceInfo> devicelist;
        public bool foundanEV3 = false;

        public bool isEV3paired
        {
            get
            {
                if (foundanEV3)
                {

                    Debug.Print("Found an EV3 in isEV3paired");

                    if(devicelist.Count > 0)
                    {
                        return devicelist.Any(x => (x.DeviceName == "EV3") && (x.Authenticated));
                    }
                }

                return false;

            }
        }

        public bool IsConnected
        {
            get
            {
                return client.Connected;
            }
        }

        public BluetoothDeviceInfo EV3info
        {
            get
            {
                return theev3;
            }
        }

        public NetworkStream getstream()
        {
            return stream;
        }

        public Communicator()
        {

            buffer = new ParseBuffer();

            Debug.Print("ParseBuffer and ConcurrentQueue initialized in Communicator.");

            lep = new BluetoothEndPoint(NetworkControl.getBluetoothMacAddressofCurrentWin32Device(), BluetoothService.SerialPort);

            Debug.Print("BluetoothEndPoint initialized in Communicator.");

            client = new BluetoothClient(lep);

            Debug.Print("BluetoothClient initialized in Communicator.");

            lc = new BluetoothComponent(client);

            Debug.Print("BluetoothComponent initialized in Communicator.");

            devicelist = new List<BluetoothDeviceInfo>();


            lc.DiscoverDevicesAsync(255, true, true, true, true, null);
            lc.DiscoverDevicesProgress += new EventHandler<DiscoverDevicesEventArgs>(ddp);
            lc.DiscoverDevicesComplete += new EventHandler<DiscoverDevicesEventArgs>(ddc);
            
        }

        

        private void ddp(object sender, DiscoverDevicesEventArgs e)
        {

            Debug.Print("Starting to discover Bluetooth devices...");

            for (int i = 0; i < e.Devices.Length; i++)
            {

                string x = "Bluetooth Device found: "
                    + e.Devices[i].DeviceName
                    + "\nDevice Address: "
                    + e.Devices[i].DeviceAddress.ToString()
                    + "\nRemembered: "
                    + e.Devices[i].Remembered.ToString()
                    + "\nAuthenticated: "
                    + e.Devices[i].Authenticated.ToString();

                Debug.Print(x);

                if(e.Devices[i].DeviceName == "EV3")
                {
                    theev3 = e.Devices[i];
                    foundanEV3 = true;
                }

                devicelist.Add(e.Devices[i]);

            }

        }

        private void ddc(object sender, DiscoverDevicesEventArgs e)
        {

            Console.WriteLine("Searching for new devices has ended.");

            if(!devicelist.Any(x => x.DeviceName == "EV3"))
            {
                Console.WriteLine("Searching for EV3 again.... Please wait....");
                lc.DiscoverDevicesAsync(255, true, true, true, true, null);

            }

        }

        public bool pair()
        {

            BluetoothAddress ev3 = null;

            try {
                ev3 = devicelist.Single(x => x.DeviceName == "EV3").DeviceAddress;
            }
            catch(Exception e)
            {
                Console.WriteLine("More than one EV3 seems to exist!");
            }

            Console.WriteLine("Confirm the connection to this computer on your EV3. (Select the tick twice.)");

            var y = BluetoothSecurity.PairRequest(ev3, "1234");

            if (y)
            {
                devicelist.Remove(theev3);
                theev3.Refresh();
                devicelist.Add(theev3);
            }

            return y;

        }

        public void startconnect()
        {

            if (!isEV3paired)
            {
                Console.WriteLine("You need to pair with the EV3 first!");
                return;
            }

            BluetoothAddress ev3 = null;

            try
            {
                ev3 = devicelist.Single(x => x.DeviceName == "EV3").DeviceAddress;
            }
            catch (Exception e)
            {
                Console.WriteLine("More than one EV3 seems to exist!");
            }

            Console.WriteLine("Beginning connection...");

            client.SetPin("1234");
            client.BeginConnect(ev3, BluetoothService.SerialPort, new AsyncCallback(endconnect), theev3);

        }

        private void endconnect(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                Console.WriteLine("Connection successful!");
            }

            stream = client.GetStream();

            buffer.setstream(stream);

        }

        public bool Send(Payload payload)
        {
            if (stream.CanWrite)
            {

                if(payload.data_parsed != null)
                {
                    try
                    { stream.Write(payload.data_parsed, 0, payload.size); }
                    catch
                    {
                        return false;
                    }

                    Debug.Print("Successfuly sent onto the stream!");

                    return true;
                }

            }

            return false;
        }

        public void Read()
        {
            if (stream.CanRead)
            {
                buffer.beginread();
            }
            else
            {
                throw new Exception("MathRace.IO.Communicator: Can't read yet!");
            }
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
            return null;
        }

        private void ClearQueue()
        {

        }

        

        

    }
}
