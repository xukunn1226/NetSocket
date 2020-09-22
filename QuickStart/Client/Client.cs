using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Framework.NetWork;
using System.Threading;
using System.IO;

namespace Client
{
    class Client
    {
        static private NetManager<string> m_NetManager;

        static async Task Main()
        {
            var tokenSource2 = new CancellationTokenSource();
            CancellationToken ct = tokenSource2.Token;

            var task = Task.Run(() =>
            {
                // Were we already canceled?
                ct.ThrowIfCancellationRequested();

                bool moreToDo = true;
                while (moreToDo)
                {
                    // Poll on this property if you have to do
                    // other cleanup before throwing.
                    if (ct.IsCancellationRequested)
                    {
                        // Clean up here, then...
                        ct.ThrowIfCancellationRequested();
                        //break;
                    }

                    ConsoleKeyInfo key = Console.ReadKey();
                    if(key.Key == ConsoleKey.Enter)
                    {
                        tokenSource2.Cancel();
                    }
                }
            }, tokenSource2.Token); // Pass same token to Task.Run.

            //tokenSource2.Cancel();

            // Just continue on this thread, or await with try-catch:
            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
            }
            finally
            {
                tokenSource2.Dispose();
            }

            Console.ReadKey();
        }


        static async Task Main1(string[] args)
        {
            //TestRef();

            Console.WriteLine($"Main            {Thread.CurrentThread.ManagedThreadId}");

            ///////////// example 2.
            Console.WriteLine("Press 'F1' to connect server...");
            m_NetManager = new NetManager<string>(new PacketString());
            ConsoleKeyInfo key;
            while (true)
            {
                if(m_NetManager.state == ConnectState.Disconnected)
                {
                    Console.WriteLine("F1: Connect;     F2: Reconnect");
                    key = Console.ReadKey();
                    if (key.Key == ConsoleKey.F1)
                    {
                        await m_NetManager.Connect("127.0.0.1", 11000);
                        if (m_NetManager.state != ConnectState.Connected)
                            Console.WriteLine("Press 'F2' to retry connect server...");
                    }
                    else if(key.Key == ConsoleKey.F2)
                    {
                        await m_NetManager.Reconnect();
                        if (m_NetManager.state == ConnectState.Connected)
                            break;
                        else
                            Console.WriteLine("Press 'F2' to retry connect server...");
                    }
                    else
                    {
                        Console.WriteLine("Error: Press 'F2' to retry connect server...");
                    }
                }

                await AutoSending();

                // test close socket
                if (m_NetManager.state == ConnectState.Connected)
                {
                    Console.WriteLine("\nPress 'C' to quit");
                    key = Console.ReadKey();
                    if (key.Key == ConsoleKey.C)
                    {
                        m_NetManager.Close();
                    }
                }
            }

            // connect successfully

            //ManualSending();

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
            while (true && m_NetManager.state == ConnectState.Connected)
            {
                string data = "Hello world..." + index++;
                Console.WriteLine("\n Sending...:" + data);
                m_NetManager.SetData(data);
                m_NetManager.Update();

                if (index == 300)
                    m_NetManager.Close();

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
                    //byte[] byteData = Encoding.ASCII.GetBytes(data);
                    m_NetManager.SetData(data, true);
                }
                else if (key.Key == ConsoleKey.Q)
                    break;
            }
        }

        static void CloseTest()
        {
            while (true)
            {
                Console.WriteLine("\nPress 'C' to quit");
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.C)
                {
                    m_NetManager.Close();
                }
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
