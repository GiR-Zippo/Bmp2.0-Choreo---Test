/*
 * Copyright(c) 2021 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using BardMusicPlayer.Jamboree.Events;
using BardMusicPlayer.Jamboree.PartyManagement;
using BardMusicPlayer.Jamboree.ZeroTier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ZeroTier.Sockets;

namespace BardMusicPlayer.Jamboree
{
    public class PartyGame
    {
        private Socket _socket = null;
        private bool _server = false; //created from server or client side
        private PartyClientInfo _clientInfo = new PartyClientInfo();

        public PartyClientInfo PartyClient {get{return _clientInfo;} }
        public Socket Socket { get { return _socket; } }


        internal PartyGame(Socket socket, bool server)
        {
            _socket = socket;
            _server = server;
            PartyManager.Instance.Add(_clientInfo);
        }

        public bool Update()
        {
            byte[] bytes = new byte[1024];
            if (_socket.Available == -1)
                return false;
            if (_socket.Poll(100, System.Net.Sockets.SelectMode.SelectRead))
            {
                int bytesRec = 0;
                try
                {
                    bytesRec = _socket.Receive(bytes);
                    if (bytesRec == -1)
                    {
                        CloseConnection();
                        return false;
                    }
                    else
                    {
                        if (_server)
                            serverOpcodeHandling(bytes, bytesRec);
                        else
                            clientOpcodeHandling(bytes, bytesRec);
                    }
                }
                catch (SocketException err)
                {
                    Console.WriteLine(
                            "ServiceErrorCode={0} SocketErrorCode={1}",
                            err.ServiceErrorCode,
                            err.SocketErrorCode);
                    return false;
                }
            }

            return true;
        }

        public bool SendPacket(byte[] pck)
        {
            if (_socket.Available == -1)
                return false;

            try { _socket.Send(pck); }
            catch { return false; }
            return true;
        }

        private void serverOpcodeHandling(byte[] bytes, int bytesRec)
        {
            Packet packet = new Packet(bytes);
            switch (packet.Opcode)
            {
                case ZeroTierPartyOpcodes.OpcodeEnum.CMSG_JOIN_PARTY:
                    _clientInfo.Performer_Type = packet.ReadUInt8();
                    _clientInfo.Performer_Name = packet.ReadCString();
                    BmpJamboree.Instance.SendPerformerJoin(_clientInfo.Performer_Type, _clientInfo.Performer_Name);
                    sendPartyMemberList();
                    break;
                default:
                    break;
            };
        }

        private void clientOpcodeHandling(byte[] bytes, int bytesRec)
        {
            Packet packet = new Packet(bytes);
            switch (packet.Opcode)
            {
                case ZeroTierPartyOpcodes.OpcodeEnum.SMSG_PERFORMANCE_START:
                    BmpJamboree.Instance.PublishEvent(new PerformanceStartEvent(packet.ReadInt64()));
                    break;
                case ZeroTierPartyOpcodes.OpcodeEnum.SMSG_JOIN_PARTY:
                    _clientInfo.Performer_Type = packet.ReadUInt8();
                    _clientInfo.Performer_Name = packet.ReadCString();
                    break;
                case ZeroTierPartyOpcodes.OpcodeEnum.SMSG_PARTY_MEMBERS:
                    int count = packet.ReadInt32();
                    for (int index = 0; index != count; index ++)
                    {
                        PartyClientInfo clientInfo = new PartyClientInfo();
                        clientInfo.Performer_Type = packet.ReadUInt8();
                        clientInfo.Performer_Name = packet.ReadCString();
                        PartyManager.Instance.Add(clientInfo);
                    }
                    BmpJamboree.Instance.PublishEvent(new PartyChangedEvent());
                    break;
                default:
                    break;
            };
        }

        public void CloseConnection()
        {
            _socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            _socket.Close();
        }

        private void sendPartyMemberList()
        {
            List<PartyClientInfo> members = PartyManager.Instance.GetPartyMembers();
            if (members.Count == 0)
                return;
            SendPacket(ZeroTierPacketBuilder.SMSG_PARTY_MEMBERS(members));
        }
    }
}
