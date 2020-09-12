﻿using System;
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

        static async Task Main(string[] args)
        {
            ////////////// example 1.
            //await Task.Run(() => Connect());
            //Connect();

            //await Task.Delay(3000);


            ///////////// example 2.
            Console.WriteLine("Press 'F1' to connect server...");
            m_NetClient = new NetClient("127.0.0.1", 11000, OnConnected, OnDisconnected);
            ConsoleKeyInfo key;
            while (m_NetClient.state != NetClient.ConnectState.Connected)
            {
                if(m_NetClient.state == NetClient.ConnectState.Disconnected)
                {
                    key = Console.ReadKey();
                    if (key.Key == ConsoleKey.F1)
                    {
                        await m_NetClient.Connect();
                        if (m_NetClient.state == NetClient.ConnectState.Connected)
                            break;
                        else
                            Console.WriteLine("Press 'F1' to retry connect server...");
                    }
                    else
                    {
                        Console.WriteLine("Error: Press 'F1' to retry connect server...");
                    }
                }
            }

            // connect successfully
            await AutoSending();

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        static void OnConnected()
        {
            Console.WriteLine("\n连接成功");
        }

        static void OnDisconnected(int ret)
        {
            if(ret != 0)
            {
                Console.WriteLine("\n连接失败，请重试");
            }
        }

        static async Task AutoSending()
        {
            int index = 0;
            while (true)
            {
                string data = "Hello world..." + index++;
                byte[] byteData = Encoding.ASCII.GetBytes(data);
                Console.WriteLine("\n" + data);
                m_NetClient.Send(byteData);
                m_NetClient.FlushSending();

                //if (index == 3)
                //    break;
                await Task.Delay(10);
            }
        }

        static void ManualSending()
        {
            while (true)
            {
                Console.WriteLine("\nPress 'Enter' to send data\n");
                string data = Console.ReadLine();
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter)
                {
                    byte[] byteData = Encoding.ASCII.GetBytes(data);
                    m_NetClient.Send(byteData);
                    m_NetClient.FlushSending();
                }
                else if (key.Key == ConsoleKey.Q)
                    break;
            }
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
