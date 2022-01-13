using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BardMusicPlayer.Choreograph
{
    public class PerformerData
    {
        public string Timestamp { set; get; }
        public string Modifier { set; get; }
        public string Key { set; get; }
        public string Performer { get; set; }
    }

    public class KeyTranslation
    {
        public static Dictionary<string, BardMusicPlayer.Quotidian.Enums.Keys> ASCIIToGame = new Dictionary<string, BardMusicPlayer.Quotidian.Enums.Keys>
        {
            { "1", BardMusicPlayer.Quotidian.Enums.Keys.D1 },
            { "2", BardMusicPlayer.Quotidian.Enums.Keys.D2 },
            { "3", BardMusicPlayer.Quotidian.Enums.Keys.D3 },
            { "4", BardMusicPlayer.Quotidian.Enums.Keys.D4 },
            { "5", BardMusicPlayer.Quotidian.Enums.Keys.D5 },
            { "6", BardMusicPlayer.Quotidian.Enums.Keys.D6 },
            { "7", BardMusicPlayer.Quotidian.Enums.Keys.D7 },
            { "8", BardMusicPlayer.Quotidian.Enums.Keys.D8 },
            { "9", BardMusicPlayer.Quotidian.Enums.Keys.D9 },
            { "0", BardMusicPlayer.Quotidian.Enums.Keys.D0 },
            { "A", BardMusicPlayer.Quotidian.Enums.Keys.A },
            { "D", BardMusicPlayer.Quotidian.Enums.Keys.D },
            { "E", BardMusicPlayer.Quotidian.Enums.Keys.E },
            { "Q", BardMusicPlayer.Quotidian.Enums.Keys.Q },
            { "S", BardMusicPlayer.Quotidian.Enums.Keys.S },
            { "W", BardMusicPlayer.Quotidian.Enums.Keys.W },
            { " ", BardMusicPlayer.Quotidian.Enums.Keys.Space }
        };
    }

}
