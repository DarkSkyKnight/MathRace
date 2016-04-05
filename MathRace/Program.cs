using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathRace.Protocol;
using System.IO.Ports;
using System.Diagnostics;
using MathRace.IO;
using MathRace.Util;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Threading;

namespace MathRace
{
    static class Program
    {


        static Thread tr1;

        static Thread tr2;

        static Communicator a;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            a = new Communicator();

            Console.WriteLine("Searching for devices.... Please wait....");

            while (!a.foundanEV3)
            {
                
            }

            Console.WriteLine("found an EV3!");

            Thread.Sleep(300);

            if (!a.isEV3paired)
            {
                Console.WriteLine("EV3 is not paired. Do you want to pair now? YES/NO");
                string x = Console.ReadLine();
                while (!x.In("YES", "NO"))
                {
                    Console.WriteLine("Please enter only YES or NO");
                    x = Console.ReadLine();
                }
                
                if(x == "YES")
                {
                    if (a.pair())
                    {
                        Console.WriteLine("Pairing successful!");
                        Thread.Sleep(300);
                    }
                    else
                    {
                        Console.WriteLine("Paring unsuccessful!");
                    }
                }

                else
                {

                }

            }
            else
            {
                Console.WriteLine("EV3 is already paired.");
            }

            //Console.WriteLine("Do you want to connect to the EV3 now? YES/NO");

            //var b = Console.ReadLine();

            //while (!b.In("YES", "NO"))
            //{
            //    Console.WriteLine("Please enter only YES or NO");
            //    b = Console.ReadLine();
            //}

            //if(b == "YES")
            //{
                a.startconnect();
            while (!a.IsConnected)
            {

            }
            //}
            //else
            //{
            //
            //}

            tr1 = new Thread(new ThreadStart(ReadandPrint));

            tr2 = new Thread(new ThreadStart(SendData));

            tr2.Start();

            tr1.Start();

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MathRace());
        }

        public static void SendData()
        {

            while (true)
            {

                Console.WriteLine("Input the title");
                var c = Console.ReadLine();

                if (c == "END")
                {
                    break;
                }

                Console.WriteLine("Input the message");
                var d = Console.ReadLine();

                if (d == "END")
                {
                    break;
                }

                bool x;
                float y;

                if (bool.TryParse(d, out x))
                {
                    a.Send(new Payload(c, x));
                }
                if (float.TryParse(d, out y))
                {
                    a.Send(new Payload(c, y));
                }
                else
                {
                    a.Send(new Payload(c, d));
                }

                tr2.Join(5000);

            }
        }

        public static void ReadandPrint()
        {
            a.Read();

            Payload pl;

            while (a.getbufferpayload(out pl))
            {
                Console.WriteLine("The title is: " + pl.title + " the message is: " + pl.message);
            }

            tr1.Join(5000);

            ReadandPrint();

        }
    }
}
