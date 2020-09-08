using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Framework.NetWork;

namespace Client
{
    class Client
    {
        static private NetClient m_NetClient;

        static void Main(string[] args)
        {
            ////////////// example 1.
            //await Task.Run(() => Connect());
            //Connect();

            //await Task.Delay(3000);


            ///////////// example 2.
            Console.WriteLine("Press 'F1' to connect server...");
            ConsoleKeyInfo key = Console.ReadKey();
            if (key.Key == ConsoleKey.F1)
            {
                m_NetClient = new NetClient("192.168.6.91", 11000);
                Console.WriteLine("\ncreate net client successfully");
            }
            else
            {
                return;
            }

            Console.WriteLine("\nPress 'Enter' to send data");
            string data = Console.ReadLine();
            key = Console.ReadKey();
            if(key.Key == ConsoleKey.Enter)
            {
                byte[] byteData = Encoding.ASCII.GetBytes(data);
                m_NetClient.Send(byteData, 0, 10);
                m_NetClient.Tick();
            }




            Console.ReadLine();
        }



        async static void Connect()
        {
            string host = "webcode.me";
            int port = 80;
            
            var client = new TcpClient();
            try
            {
                await client.ConnectAsync(host, port);
                Console.WriteLine("Connect successful");

                Write();
                Receive();
                int ii = 0;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
            }
            // https://stackoverflow.com/questions/41718342/async-socket-client-using-tcpclient-class-c-sharp
            //Task.Run(() => Receive());
            //Task.Run(() => Write());

            //await Receive();
            //await Write();

            
        }

        async static void Receive()
        {
            while (true)
            {
                await Task.Delay(1000);
                Console.WriteLine("Receive");
            }
        }

        async static void Write()
        {
            while(true)
            {
                //await Task.Delay(500);
                await Foo();
                Console.WriteLine("Write");
            }
        }

        async static Task Foo()
        {
            await Task.Delay(1);
            return;
        }
    }
}
