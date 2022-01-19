using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BardMusicPlayer.Jamboree.Events
{
    public sealed class PartyJoinedEvent : JamboreeEvent
    {
        internal PartyJoinedEvent(string token) : base(0, false)
        {
            EventType = GetType();
            Token = token;
        }

        public string Token { get; }

        public override bool IsValid() => true;
    }

}
