using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BardMusicPlayer.Jamboree.PartyClient
{
    public static class PartyOpcodes
    {
        public enum OpcodeEnum : byte
        {
            NULL_OPCODE = 0x00,
            SMSG_PERFORMANCE_START = 0x01,
            CMSG_TERM_SESSION = 0x02
        }
    }
}
