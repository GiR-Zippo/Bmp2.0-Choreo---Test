using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTier;
using ZeroTier.Core;

namespace BardMusicPlayer.Jamboree.PartyClient
{
    public class ZeroTierConnector
    {
        public Node node = null;
        private volatile bool nodeOnline = false;

        public Task<string> ZeroTierConnect(string network)
        {
            node = new Node();
            string ipAddress = "";
            ulong networkId = (ulong)Int64.Parse(network, System.Globalization.NumberStyles.HexNumber);

            Console.WriteLine("Connecting to network...");

            //node.InitFromStorage(configFilePath);
            node.InitAllowNetworkCaching(false);
            node.InitAllowPeerCaching(false);
            // node.InitAllowIdentityCaching(true);
            // node.InitAllowWorldCaching(false);
            node.InitSetEventHandler(ZeroTierEvent);
            // node.InitSetPort(0);   // Will randomly attempt ports if not specified or is set to 0
            node.InitSetRandomPortRange(40000, 50000);
            // node.InitAllowSecondaryPort(false);

            node.Start();   // Network activity only begins after calling Start()
            while (!nodeOnline)
            { Task.Delay(50); }

            Console.WriteLine("Id            : " + node.IdString);
            Console.WriteLine("Version       : " + node.Version);
            Console.WriteLine("PrimaryPort   : " + node.PrimaryPort);
            Console.WriteLine("SecondaryPort : " + node.SecondaryPort);
            Console.WriteLine("TertiaryPort  : " + node.TertiaryPort);

            node.Join(networkId);
            Console.WriteLine("Waiting for join to complete...");
            while (node.Networks.Count == 0)
            {
                Task.Delay(50);
            }

            // Wait until we've joined the network and we have routes + addresses
            Console.WriteLine("Waiting for network to become transport ready...");
            while (!node.IsNetworkTransportReady(networkId))
            {
                Task.Delay(50);
            }

            Console.WriteLine("Num of assigned addresses : " + node.GetNetworkAddresses(networkId).Count);
            if (node.GetNetworkAddresses(networkId).Count == 1)
            {
                IPAddress addr = node.GetNetworkAddresses(networkId)[0];
                ipAddress = addr.ToString();
            }
            foreach (IPAddress addr in node.GetNetworkAddresses(networkId))
            {
                Console.WriteLine(" - Address: " + addr);
            }

            Console.WriteLine("Num of routes             : " + node.GetNetworkRoutes(networkId).Count);
            foreach (RouteInfo route in node.GetNetworkRoutes(networkId))
            {
                Console.WriteLine(" -   Route: target={0} via={1} flags={2} metric={3}",
                    route.Target.ToString(),
                    route.Via.ToString(),
                    route.Flags,
                    route.Metric);
            }
            return Task.FromResult(result: ipAddress);
        }

        public void ZeroTierDisconnect()
        {
            node.Stop();
        }

        private void ZeroTierEvent(Event e)
        {
            Console.WriteLine("Event.Code = {0} ({1})", e.Code, e.Name);

            if (e.Code == Constants.EVENT_NODE_ONLINE)
            {
                nodeOnline = true;
            }
            /*
        if (e.Code == ZeroTier.Constants.EVENT_NODE_ONLINE) {
            Console.WriteLine("Node is online");
            Console.WriteLine(" - Address (NodeId): " + node.Id.ToString("x16"));
        }

        if (e.Code == ZeroTier.Constants.EVENT_NETWORK_OK) {
            Console.WriteLine(" - Network ID: " + e.NetworkInfo.Id.ToString("x16"));
        }
        */
        }

    }
}
