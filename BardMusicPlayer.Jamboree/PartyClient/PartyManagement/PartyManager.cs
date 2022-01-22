using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BardMusicPlayer.Jamboree.PartyManagement
{
    class PartyManager
    {
        public List<PartyClientInfo> GetPartyMembers() { return _partyGames; }

        private List<PartyClientInfo> _partyGames = new List<PartyClientInfo>();
#region Instance Constructor/Destructor
        private static readonly Lazy<PartyManager> LazyInstance = new(() => new PartyManager());

        /// <summary>
        /// 
        /// </summary>
        public bool Started { get; private set; }


        private PartyManager()
        {
            _partyGames.Clear();
        }

        public static PartyManager Instance => LazyInstance.Value;

        ~PartyManager() { Dispose(); }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Add(PartyClientInfo client)
        {
            foreach(PartyClientInfo info in _partyGames)
                if (info.Performer_Name == client.Performer_Name)
                    return;
            _partyGames.Add(client);
        }

        public void Remove(PartyClientInfo client)
        {
            _partyGames.Remove(client);
        }

#endregion



    }
}
