using BardMusicPlayer.Jamboree.PartyClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTier.Sockets;

namespace BardMusicPlayer.Jamboree.PartyServer
{
    public class ZeroTierPartyServer
    {
        private SocketServer svcServer { get; set; } = null;
        public ZeroTierPartyServer(IPEndPoint iPEndPoint)
        {
            BackgroundWorker objWorkerServerDiscovery = new BackgroundWorker();
            objWorkerServerDiscovery.WorkerReportsProgress = true;
            objWorkerServerDiscovery.WorkerSupportsCancellation = true;

            svcServer = new SocketServer(ref objWorkerServerDiscovery, iPEndPoint);
            objWorkerServerDiscovery.DoWork += new DoWorkEventHandler(svcServer.Start);
            objWorkerServerDiscovery.ProgressChanged += new ProgressChangedEventHandler(logWorkers_ProgressChanged);
            objWorkerServerDiscovery.RunWorkerAsync();
        }

        public void Close()
        {
            svcServer.Stop();
        }

        public void SendToAll(PartyOpcodes.OpcodeEnum opcode, string data)
        {
            svcServer.SendToAll(opcode, data);
        }

        private void logWorkers_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.UserState.ToString());
        }
    }

    public class SocketServer
    {
        public bool disposing = false;
        byte[] packetBytes = new byte[] { 0x1, 0x2, 0x3 };
        public IPEndPoint iPEndPoint;
        public int ServerPort = 0;

        private BackgroundWorker worker;
        private List<Socket> sockets;
        public SocketServer(ref BackgroundWorker worker, IPEndPoint localEndPoint)
        {
            sockets = new List<Socket>();
            this.worker = worker;
            this.iPEndPoint = localEndPoint;
            worker.ReportProgress(1, "Server");
        }

        public void Start(object sender, DoWorkEventArgs e)
        {
            string data = null;

            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            Console.WriteLine(iPEndPoint.ToString());
            Socket listener = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
 
            listener.Bind(iPEndPoint);
            listener.Listen(10);
            
            while (this.disposing == false)
            {
                //try
                {
                    if (listener.Poll(100, System.Net.Sockets.SelectMode.SelectRead))
                    {
                        //Incomming connection
                        Socket handler = listener.Accept();
                        if (!sockets.Contains(handler))
                            sockets.Add(handler);
                    }

                    List<Socket> tempsock = sockets;
                    //Clean the socket list
                    foreach (var sock in tempsock)
                    {
                        if (sock.Available == -1)
                            sockets.Remove(sock);
                    }

                    tempsock = sockets;
                    foreach (var sock in tempsock)
                    {
                        if (sock.Available == -1)
                            continue;
                        if (sock.Poll(1, System.Net.Sockets.SelectMode.SelectRead))
                        {
                            int bytesRec = 0;
                            try
                            {
                                bytesRec = sock.Receive(bytes);
                                if (bytesRec == -1)
                                {
                                    removeSocket(sock);
                                    break;
                                }
                                else
                                {
                                    PartyOpcodes.OpcodeEnum opcode = (PartyOpcodes.OpcodeEnum)bytes[0];
                                    string trunk = Encoding.ASCII.GetString(bytes, 1, bytesRec);
                                    switch (opcode)
                                    {
                                        default:
                                            break;
                                    };
                                    Console.WriteLine("Client: Recv: {0}", data);
                                }
                            }
                            catch (SocketException err)
                            {
                                Console.WriteLine(
                                        "ServiceErrorCode={0} SocketErrorCode={1}",
                                        err.ServiceErrorCode,
                                        err.SocketErrorCode);
                                sockets.Remove(sock);
                            }
                        }
                    }
                }
                    /*catch (SocketException err)
                    {
                        Console.WriteLine(err);
                        Console.WriteLine("ServiceErrorCode={0} SocketErrorCode={1}", err.ServiceErrorCode, err.SocketErrorCode);
                    }*/
                Thread.Sleep(10);
            }
            foreach (Socket s in sockets)
            {
                // Release the socket.
                s.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                s.Close();
            }
            listener.Close();
            return;
        }

        public void SendToAll(PartyOpcodes.OpcodeEnum opcode, string data)
        {
            foreach (var sock in sockets)
            {
                if (sock.Available == -1)
                    continue;
                data = " " + data;
                byte[] msg = Encoding.ASCII.GetBytes(data);
                msg[0] = (byte)opcode;
                try
                {
                    sock.Send(msg);
                }
                catch
                {
                }
            }
        }

        public void Stop()
        {
            this.disposing = true;
        }

        private void removeSocket(Socket socket)
        {
            foreach (Socket sock in sockets)
            {
                if (sock.RemoteEndPoint == socket.RemoteEndPoint)
                {
                    sockets.Remove(sock);
                    sock.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    sock.Close();
                    return;
                }
            }
        }

    }
}
