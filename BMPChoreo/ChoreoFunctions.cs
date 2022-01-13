using BardMusicPlayer.Choreograph;
using BardMusicPlayer.Transmogrify.Song;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMPChoreo
{
    public static class ChoreoFunctions
    {
        /// <summary>
        /// The playback states
        /// </summary>
        public enum ChoreoState_Enum
        {
            CHOREO_STATE_STOPPED = 0,
            CHOREO_STATE_PLAYING,
            CHOREO_STATE_PAUSE,
            CHOREO_STATE_PLAYNEXT //indicates the next song should be played
        };
        public static ChoreoState_Enum ChoreoState;

        private static BmpSong currentSong;

        /// <summary>
        /// Loads a midi file into the sequencer
        /// </summary>
        /// <returns>true if success</returns>
        public static bool LoadChoreo(string filename, List<PerformerData> perfData, int performer)
        {
            ChoreoState = ChoreoState_Enum.CHOREO_STATE_STOPPED;

            currentSong = BmpSong.OpenMidiFile(filename).Result;
            BmpChoreograph.Instance.LoadPerfomance(currentSong, perfData, performer );
            return true;
        }

        public static void Start()
        {
            ChoreoState = ChoreoState_Enum.CHOREO_STATE_PLAYING;
            BmpChoreograph.Instance.StartPerformance();
        }

        public static void Pause()
        {
            ChoreoState = ChoreoState_Enum.CHOREO_STATE_PAUSE;
            BmpChoreograph.Instance.PausePerformance();
        }

        public static void Stop()
        {
            ChoreoState = ChoreoState_Enum.CHOREO_STATE_STOPPED;
            BmpChoreograph.Instance.StopPerformance();
        }

        /// <summary>
        /// Gets the song name from the current song
        /// </summary>
        /// <returns>song name as string</returns>
        public static string GetSongName()
        {
            if (currentSong == null)
                return "please load a song";
            return currentSong.Title;
        }

        public static void SetClientsGo()
        {
            BmpChoreograph.Instance.SendChatChoreoStart();
        }
    }
}
