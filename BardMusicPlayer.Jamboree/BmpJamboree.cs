/*
 * Copyright(c) 2021 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTier.Core;

namespace BardMusicPlayer.Jamboree
{
    public partial class BmpJamboree : IDisposable
    {
        private Pydna _pydna = null;

#region Instance Constructor/Destructor
        private static readonly Lazy<BmpJamboree> LazyInstance = new(() => new BmpJamboree());

        /// <summary>
        /// 
        /// </summary>
        public bool Started { get; private set; }


        private BmpJamboree()
        {
            _pydna = new Pydna();
        }

        public static BmpJamboree Instance => LazyInstance.Value;

        /// <summary>
        /// Start the eventhandler
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            if (Started) return;
            StartEventsHandler();
            Started = true;
        }

        /// <summary>
        /// Stop the eventhandler
        /// </summary>
        /// <returns></returns>
        public void Stop()
        {
            if (!Started) return;
            StopEventsHandler();
            Started = false;
        }

        ~BmpJamboree() { Dispose(); }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
#endregion

        public void JoinParty(string partycode)
        {
            if(_pydna == null)
                _pydna = new Pydna();
            Task.Run(() => _pydna.JoinParty(partycode));
        }

        public void CreateParty(string networkId)
        {
            if (_pydna == null)
                _pydna = new Pydna();
            Task.Run(() => _pydna.CreateParty(networkId));
        }

        public void LeaveParty()
        {
            if (_pydna == null)
                _pydna = new Pydna();
            _pydna.LeaveParty();
        }

        public void SendPerformanceStart()
        {
            if (_pydna == null)
                _pydna = new Pydna();
            _pydna.SendPerformanceStart();
        }

    }


    /* */
}
