using BardMusicPlayer.Jamboree.PartyManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BardMusicPlayer.Jamboree.ZeroTier
{
    public static class ZeroTierPacketBuilder
    {
        public static byte[] PerformanceStart()
        {
            Packet buffer = new Packet(ZeroTierPartyOpcodes.OpcodeEnum.SMSG_PERFORMANCE_START);
            buffer.WriteInt64(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            return buffer.GetData();
        }

        /// <summary>
        /// Send we joined the party
        /// | type 0 = bard
        /// | type 1 = dancer
        /// </summary>
        /// <param name="type"></param>
        /// <param name="performer_name"></param>
        /// <returns>data as byte[]</returns>
        public static byte[] CMSG_JOIN_PARTY(byte type, string performer_name)
        {
            Packet buffer = new Packet(ZeroTierPartyOpcodes.OpcodeEnum.CMSG_JOIN_PARTY);
            buffer.WriteUInt8(type);
            buffer.WriteCString(performer_name);
            return buffer.GetData();
        }

        public static byte[] SMSG_PARTY_MEMBERS(List<PartyClientInfo> clients)
        {
            Packet buffer = new Packet(ZeroTierPartyOpcodes.OpcodeEnum.SMSG_PARTY_MEMBERS);
            buffer.WriteInt32(clients.Count);
            foreach(var member in clients)
            {
                buffer.WriteUInt8(member.Performer_Type);
                buffer.WriteCString(member.Performer_Name);
            }
            return buffer.GetData();
        }
    }
}
