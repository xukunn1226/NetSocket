using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        async static Task Main(string[] args)
        {
            //await Task.Run(() => Connect());
            Connect();

            await Task.Delay(3000);

            Console.WriteLine("-----------");
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

            Receive();
            Write();
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
                await Task.Delay(500);
                Console.WriteLine("Write");
            }
        }
    }
}
