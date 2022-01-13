using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BardMusicPlayer.Grunt;
using BardMusicPlayer.Quotidian.Enums;
using BardMusicPlayer.Quotidian.Structs;
using BardMusicPlayer.Seer;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;

namespace BardMusicPlayer.Choreograph
{
    public class Sequencer
    {
        private Game _game;
        private Playback _playback;
        private bool _playbackStart = false;
        private int _EventsIndex = 0;
        private List<PerformerData> _eventsData;


        private ConcurrentQueue<(Game, MidiEventPlayedEventArgs, int)> _eventQueue;
        public delegate void Del(Game game, MidiEventPlayedEventArgs e, int tracknumber);

        private int _tracknumber = 0;
        ITimeSpan _startingpoint;

        public Sequencer(Game game, MidiFile container, List<PerformerData>performerDatas, int tracknr = 1)
        {
            _eventQueue = new ConcurrentQueue<(Game, MidiEventPlayedEventArgs, int)>();

            _game = game;
            _playback = container.GetPlayback();

            //Start the melanchall sequencer
            PlaybackCurrentTimeWatcher.Instance.AddPlayback(_playback, TimeSpanType.Metric);
            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += OnTick;
            PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeSpan.FromMilliseconds(10);  //Not sure, but seems to affect OnNoteEvent polling too
            PlaybackCurrentTimeWatcher.Instance.Start();

            _playback.Speed = 1;                    //Yep that's the playback speed and we'll set it
            _playback.Started += _playback_Started;
            _playback.Stopped += _playback_Stopped;
            _playback.Finished    += OnPlaybackStopped;
            _tracknumber = tracknr;

            _playbackStart = false;
            _EventsIndex = 0;
            _eventsData = new List<PerformerData>();
            foreach (var t in performerDatas)
            {
                if(Convert.ToInt32(t.Performer) == tracknr)
                    _eventsData.Add(t);
                if (Convert.ToInt32(t.Performer) == 0)
                    _eventsData.Add(t);
            }

//BmpChoreograph.Instance.PublishEvent(new MaxPlayTimeEvent(_playback.GetDuration(TimeSpanType.Metric)));
        }

        private void _playback_Stopped(object sender, EventArgs e)
        {
            _playbackStart = false;
        }

        private void _playback_Started(object sender, EventArgs e)
        {
            _playbackStart = true;
        }

        public void SetPlaybackStart(double f)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(f/1000); //We have microseconds and want some milis....
            _startingpoint = new MetricTimeSpan(hours: time.Hours, minutes: time.Minutes, seconds: time.Seconds, milliseconds: time.Milliseconds);

            _playback.MoveToTime(_startingpoint);
        }

        public void OnTick(object sender, PlaybackCurrentTimeChangedEventArgs e)
        {
            if (!_playbackStart)
                return;

            var p = _playback.GetCurrentTime(TimeSpanType.Metric);
            var z = p.ToString().Split(':');
            TimeSpan time = new TimeSpan(0, Convert.ToInt32(z[0]), Convert.ToInt32(z[1]), Convert.ToInt32(z[2]), Convert.ToInt32(z[3]));
            Console.WriteLine(time.TotalMilliseconds);

            if (_EventsIndex == _eventsData.Count())
                return;

            PerformerData d = _eventsData.ElementAt(_EventsIndex);
            if (Enumerable.Range(Convert.ToInt32(d.Timestamp), Convert.ToInt32(d.Timestamp) + 200).Contains((int)time.TotalMilliseconds))
            {
                _EventsIndex++;

                if (d.Modifier.Contains("TEXT"))
                {
                    _ = GameExtensions.SendLyricLine(_game, d.Key);
                    return;
                }

                BardMusicPlayer.Quotidian.Enums.Keys key = KeyTranslation.ASCIIToGame[d.Key];
                if (d.Modifier.Contains("SHIFT"))
                    key = (int)BardMusicPlayer.Quotidian.Enums.Keys.Shift + key;
                else if (d.Modifier.Contains("CTRL"))
                    key = (int)BardMusicPlayer.Quotidian.Enums.Keys.Control + key;

                _ = GameExtensions.SyncTapKey(_game, (BardMusicPlayer.Quotidian.Enums.Keys)key);
            }

            //BmpChoreograph.Instance.PublishEvent(new CurrentPlayPositionEvent(_playback.GetCurrentTime(TimeSpanType.Metric)));
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            _playbackStart = false;
            //BmpChoreograph.Instance.PublishEvent(new PlaybackStoppedEvent());
        }

        public void ChangeTracknumer(int track)
        { _tracknumber = track; }

        public void Start()
        {
            if (!BmpGrunt.Instance.Started)
                return;
            _playback.Start();      //start from the point you stopped
        }

        public void Pause()
        {
            _playback.Stop();       //missleading, it only pauses
        }

        public void Stop()
        {
            _playback.Stop();        //Stop
            _playback.MoveToStart(); //To the beginning of da song
            _EventsIndex = 0;
        }

        public void Destroy()
        {
            _playback.Stop();
            _playback.Dispose();
        }

    }
}
