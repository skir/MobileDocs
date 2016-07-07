using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MobileDocs
{
    public class WebService
    {
        static NetworkStream stream;
        Socket socket;

        static Stopwatch stopWatch = new Stopwatch();
        static int bytesReceived = 0;

        static OnDataReceived mOnDataReceived;

        public WebService()
        {
        }

        public void initStream(string hostAdress, int port)
        {
            Console.WriteLine("************************");
            closeConnection();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(hostAdress), port);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;
            args.Completed += connectionCompleted;
            socket.ConnectAsync(args);
        }

        public void closeConnection()
        {
            if (stream != null)
            {
                stream.Close();
            }
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private static void connectionCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.ConnectSocket != null)
            {
                stream = new NetworkStream(args.ConnectSocket);
                
                Console.WriteLine("Connection Established : " + args.RemoteEndPoint);
                readData();
            }
            else
            {
                Console.WriteLine("Connection Failed : " + args.RemoteEndPoint);
            }
        }
        
        private static void readData()
        {
            Byte[] bytes = new Byte[1001];
            int i;
            while (true)
            {
                if ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    //Console.WriteLine("reading");
                    if (bytesReceived == 0)
                    {
                        stopWatch.Start();
                    }
                    bytesReceived += bytes.Length;

                    if (mOnDataReceived != null)
                    {
                        //Console.WriteLine("data " + bytes[0]);
                        //if (bytes[0] == 1)
                        //{
                        //    //mOnDataReceived.TogglePlayingState();
                        //}
                        //if (bytes[0] == 0)
                        //{
                            mOnDataReceived.OnReceived(bytes);
                        //}
                        
                    }
                    //if (stopWatch.ElapsedMilliseconds > 5000 && stopWatch.IsRunning)
                    //{
                    //    stopWatch.Stop();
                    //    Console.WriteLine("SPEED " + bytesReceived / (stopWatch.ElapsedMilliseconds / 1000));
                    //}
                }
            }
        }

        public void SetOnDataReceivedListener(OnDataReceived onDataReceived)
        {
            mOnDataReceived = onDataReceived;
        }

        public interface OnDataReceived
        {
            void OnReceived(byte[] data);
            void TogglePlayingState();
        }
    }
}
