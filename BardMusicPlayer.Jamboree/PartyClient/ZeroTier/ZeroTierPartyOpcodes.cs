using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BardMusicPlayer.Jamboree.ZeroTier
{
    public static class ZeroTierPartyOpcodes
    {
        public enum OpcodeEnum : byte
        {
            NULL_OPCODE = 0x00,
            SMSG_PERFORMANCE_START = 0x01,
            CMSG_TERM_SESSION = 0x02,
            CMSG_JOIN_PARTY = 0x03,
            SMSG_JOIN_PARTY = 0x04,
            SMSG_PARTY_MEMBERS = 0x05
        }
    }
}
