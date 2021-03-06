using BardMusicPlayer.Jamboree.Events;
using BardMusicPlayer.Jamboree.ZeroTier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BardMusicPlayer.Jamboree
{
    /// <summary>
    /// The manager class for the festival
    /// </summary>
    public class Pydna
    {
        private bool _servermode { get; set; } = false;
        private ZeroTierPartyServer server = null;
        private ZeroTierPartyClient client = null;
        private ZeroTierConnector zeroTierConnector = null;

        public void CreateParty(string networkId)
        {
            zeroTierConnector = new ZeroTierConnector();
            string id = networkId+"-";
            string data = zeroTierConnector.ZeroTierConnect(networkId).Result;
            id = id+data.Split('.')[3];
            _servermode = true;
            var plainTextBytes = Encoding.UTF8.GetBytes(id);

            server = new ZeroTierPartyServer(new IPEndPoint(IPAddress.Parse(data), 12345));
            BmpJamboree.Instance.PublishEvent(new PartyCreatedEvent(Convert.ToBase64String(plainTextBytes)));
        }

        public void JoinParty(string partycode)
        {
            zeroTierConnector = new ZeroTierConnector();
            var base64EncodedBytes = Convert.FromBase64String(partycode);
            string p = Encoding.UTF8.GetString(base64EncodedBytes);
            string networkId = p.Split('-')[0];
            string host = p.Split('-')[1];
            string data = zeroTierConnector.ZeroTierConnect(networkId).Result;
            _servermode = false;
            data = data.Split('.')[0] + "." + data.Split('.')[1] + "." + data.Split('.')[2] + "." + host;
            client = new ZeroTierPartyClient(new IPEndPoint(IPAddress.Parse(data), 12345));
        }

        public void LeaveParty()
        {
            if (!_servermode)
            {
                if (client == null)
                    return;
                client.Close();
                client = null;
            }
            else
            {
                if (server == null)
                    return;
                server.Close();
                server = null;
            }
            zeroTierConnector.ZeroTierDisconnect();
        }

#region NetworkSendFunctions
        public void SendPerformanceStart()
        {
            if (_servermode)
                server.SendToAll(ZeroTierPacketBuilder.PerformanceStart());
        }

        /// <summary>
        /// Send we joined the party
        /// | type 0 = bard
        /// | type 1 = dancer
        /// </summary>
        /// <param name="type"></param>
        /// <param name="performer_name"></param>
        public void SendPerformerJoin(byte type, string performername)
        {
            if (!_servermode)
            {
                client.SetPlayerData(type, performername);
                client.SendPacket(ZeroTierPacketBuilder.CMSG_JOIN_PARTY(type, performername));
            }
            else
                server.SendToAll(ZeroTierPacketBuilder.CMSG_JOIN_PARTY(type, performername));
        }

#endregion

    }
}
