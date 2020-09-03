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
            await Task.Run(() => Connect());

            await Task.Delay(10000);

            //Console.WriteLine("-----------");
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
            //await Task.Run(() => Receive());
            //await Task.Run(() => Write());

            await Receive();
            await Write();
        }

        async static Task Receive()
        {
            while (true)
            {
                await Task.Delay(1000);
                Console.WriteLine("Receive");
            }
        }

        async static Task Write()
        {
            while(true)
            {
                await Task.Delay(500);
                Console.WriteLine("Write");
            }
        }
    }
}
