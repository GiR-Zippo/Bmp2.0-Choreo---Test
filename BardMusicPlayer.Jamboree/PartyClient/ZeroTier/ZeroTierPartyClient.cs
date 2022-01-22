using BardMusicPlayer.Jamboree.Events;
using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZeroTier.Sockets;

namespace BardMusicPlayer.Jamboree.ZeroTier
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

        public void SetPlayerData(byte type, string name)
        {
            svcClient.SetPlayerData(type, name);
        }

        public void SendPacket(ZeroTierPartyOpcodes.OpcodeEnum opcode, string data)
        {
            if (!svcClient.SendMessage(opcode, data))
                svcClient.Stop();
        }

        public void SendPacket(byte[] pck)
        {
            if (!svcClient.SendMessage(pck))
                svcClient.Stop();
        }

        public void Close()
        {
            svcClient.SendMessage(ZeroTierPartyOpcodes.OpcodeEnum.CMSG_TERM_SESSION, "");
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
        private PartyGame session = null;
        private BackgroundWorker worker;

        public SocketClient(ref BackgroundWorker w, IPEndPoint localEndPoint)
        {
            worker = w;
            this.remoteServerEndPoint = localEndPoint;
            worker.ReportProgress(1, "Client");
        }

        public void Start(object s, DoWorkEventArgs e)
        {
            byte[] bytes = new byte[1024];
            Socket sender = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            //Connect to the server
            sender.Connect(remoteServerEndPoint);
            //Wait til connected
            while(!sender.Connected)
            { Task.Delay(10); }
            //Create the session
            session = new PartyGame(sender, false);
            //Inform we are connected
            BmpJamboree.Instance.PublishEvent(new PartyConnectionChangedEvent(PartyConnectionChangedEvent.ResponseCode.OK, "Connected"));

            //da loop
            while (this.disposing == false)
            {
                if (!session.Update())
                {
                    BmpJamboree.Instance.PublishEvent(new PartyConnectionChangedEvent(PartyConnectionChangedEvent.ResponseCode.ERROR, "Disconnected"));
                    break;
                }
                Task.Delay(5);
            }
            // Remove this session and exit
            session.CloseConnection();
            BmpJamboree.Instance.PublishEvent(new PartyConnectionChangedEvent(PartyConnectionChangedEvent.ResponseCode.MESSAGE, "Disconnected"));
        }

        public bool SendMessage(ZeroTierPartyOpcodes.OpcodeEnum opcode, string data)
        {
            data = " " + data;
            byte[] msg = Encoding.ASCII.GetBytes(data);
            msg[0] = (byte)opcode;
            return session.SendPacket(msg);
        }

        public bool SendMessage(byte[] pck)
        {
            return session.SendPacket(pck);
        }

        public void Stop()
        {
            this.disposing = true;
        }

        public void SetPlayerData(byte type, string name)
        {
            var t = session.PartyClient;
            t.Performer_Type = type;
            t.Performer_Name = name;
        }
    }
}
