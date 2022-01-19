using BardMusicPlayer.Jamboree.Events;
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
    public class ZeroTierPartyClient
    {
        private SocketClient svcClient { get; set; } = null;
        public ZeroTierPartyClient(IPEndPoint iPEndPoint)
        {
            BackgroundWorker objWorkerServerDiscovery = new BackgroundWorker();
            objWorkerServerDiscovery.WorkerReportsProgress = true;
            objWorkerServerDiscovery.WorkerSupportsCancellation = true;

            svcClient = new SocketClient(ref objWorkerServerDiscovery, iPEndPoint);
            objWorkerServerDiscovery.DoWork += new DoWorkEventHandler(svcClient.Start);
            objWorkerServerDiscovery.ProgressChanged += new ProgressChangedEventHandler(logWorkers_ProgressChanged);
            objWorkerServerDiscovery.RunWorkerAsync();
        }

        public void SendPacket(PartyOpcodes.OpcodeEnum opcode, string data)
        {
            if (!svcClient.SendMessage(opcode, data))
                svcClient.Stop();
        }

        public void Close()
        {
            svcClient.SendMessage(PartyOpcodes.OpcodeEnum.CMSG_TERM_SESSION, "");
            svcClient.Stop();
        }
        private void logWorkers_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Report thread messages to Console
            Console.WriteLine(e.UserState.ToString());
        }
    }

    public class SocketClient
    {
        public bool disposing = false;
        public IPEndPoint remoteServerEndPoint;
        public int ServerPort = 0;
        private Socket sender;
        private BackgroundWorker worker;

        public SocketClient(ref BackgroundWorker worker, IPEndPoint localEndPoint)
        {
            this.worker = worker;
            this.remoteServerEndPoint = localEndPoint;
            worker.ReportProgress(1, "Client");
        }

        public void Start(object s, DoWorkEventArgs e)
        {
            byte[] bytes = new byte[1024];
            //try
            {
                sender = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                //try
                {
                    sender.Connect(remoteServerEndPoint);
                    //da loop
                    while (this.disposing == false)
                    {
                        if (sender.Poll(100, System.Net.Sockets.SelectMode.SelectRead))
                        {
                            int bytesRec = sender.Receive(bytes);
                            PartyOpcodes.OpcodeEnum opcode = (PartyOpcodes.OpcodeEnum)bytes[0];
                            string data = Encoding.ASCII.GetString(bytes, 1, bytesRec);
                            switch (opcode)
                            {
                                case PartyOpcodes.OpcodeEnum.SMSG_PERFORMANCE_START:
                                    BmpJamboree.Instance.PublishEvent(new PerformanceStartEvent(Convert.ToInt64(data)));
                                    break;
                            };
                            Console.WriteLine("Client: Recv: {0}", data);
                        }
                    }
                    // Release the socket.
                    sender.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    sender.Close();
                }
                /*catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException err)
                {
                    Console.WriteLine(err);
                    Console.WriteLine("SocketErrorCode={0}", err.SocketErrorCode);
                }*/
            }
            /*catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }*/
        }

        public bool SendMessage(PartyOpcodes.OpcodeEnum opcode, string data)
        {
            if (sender.Available == -1)
                return false;
            data = " " + data;
            byte[] msg = Encoding.ASCII.GetBytes(data);
            msg[0] = (byte)opcode;
            try
            {
                int bytesSent = sender.Send(msg);
            }
            catch { }
            return true;
        }
        public void SendMessage(string data)
        {
            byte[] msg = Encoding.ASCII.GetBytes("This is a test");
            int bytesSent = sender.Send(msg);
        }

        public void Stop()
        {
            this.disposing = true;
        }
    }
}
