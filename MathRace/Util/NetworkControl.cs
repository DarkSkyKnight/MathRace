using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace MathRace.Util
{
    class NetworkControl
    {

        public static byte[] getMacAddressofCurrentWin32Device()
        {

            ManagementClass mgmt = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection objCol = mgmt.GetInstances();

            string address = string.Empty;

            foreach (ManagementObject obj in objCol)
            {
                if (address == string.Empty)
                {
                    if ((bool)obj["IPEnabled"] == true)
                    {
                        address = obj["MacAddress"].ToString();
                    }

                    obj.Dispose();
                }

            }

            address = address.Replace(":", "");

            Debug.Print("NetworkControl.getMacAddressofCurrentWin32Device: This device's MAC address:" + address);

            return Converter.HexadecimalStringToByteArray(address);

        }

        public static BluetoothAddress getBluetoothMacAddressofCurrentWin32Device()
        {
            BluetoothRadio myRadio = BluetoothRadio.PrimaryRadio;
            if (myRadio == null)
            {
                Debug.Print("No radio hardware or unsupported software stack");
                return null;
            }
            RadioMode mode = myRadio.Mode;
            // Warning: LocalAddress is null if the radio is powered-off.
            Debug.Print("* Radio, address: {0:C}", myRadio.LocalAddress);

            return myRadio.LocalAddress;

        } 



    }
}
