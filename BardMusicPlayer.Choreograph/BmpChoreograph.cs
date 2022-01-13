using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BardMusicPlayer.Grunt;
using BardMusicPlayer.Pigeonhole;
using BardMusicPlayer.Seer;
using BardMusicPlayer.Transmogrify.Song;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace BardMusicPlayer.Choreograph
{
    public partial class BmpChoreograph : IDisposable
    {
        private static readonly Lazy<BmpChoreograph> LazyInstance = new(() => new BmpChoreograph());

        public IEnumerable<Game> Bards { get; private set; }
        public Game SelectedBard { get; set; }
        private int _NoteKeyDelay;

        private Sequencer _sequencer;
        /// <summary>
        /// 
        /// </summary>
        public bool Started { get; private set; }

        private BmpChoreograph()
        {
            Bards = BmpSeer.Instance.Games.Values;
            BmpSeer.Instance.GameStarted += e => GameStarted(e.Game);
        }

        private void GameStarted(Game game)
        {
            if (!Bards.Contains(game))
            {
                Bards.Append(game);
                SelectedBard ??= game;
            }
        }

        public static BmpChoreograph Instance => LazyInstance.Value;

        public void SendChatChoreoStart()
        {
            /*int pid = BmpMaestro.Instance.GetHostPid();
            foreach (Game game in BmpSeer.Instance.Games.Values)
            {
                if (game.Pid == pid)
                {
                    _ = GameExtensions.SendLyricLine(game, "<1234567890>GO!");
                    return;
                }
            }*/
        }

        /// <summary>
        /// Sets a new song for the sequencer
        /// </summary>
        /// <param name="bmpSong"></param>
        /// <param name="track">the tracknumber which should be played; -1 all tracks</param>
        /// <returns></returns>
        public void LoadPerfomance(BmpSong bmpSong, List<PerformerData> performerDatas, int track)
        {
            var index = 0;
            //create a midifile
            var midiFile = new MidiFile();
            //add the chunks
            foreach (var data in bmpSong.TrackContainers)
            {
                //Set the channel for notes and progchanges
                using (var manager = data.Value.SourceTrackChunk.ManageNotes())
                {
                    foreach (Note note in manager.Notes)
                        note.Channel = Melanchall.DryWetMidi.Common.FourBitNumber.Parse(index.ToString());
                }
                using (var manager = data.Value.SourceTrackChunk.ManageTimedEvents())
                {
                    foreach (var e in manager.Events)
                    {
                        var programChangeEvent = e.Event as ProgramChangeEvent;
                        if (programChangeEvent == null)
                            continue;
                        programChangeEvent.Channel = Melanchall.DryWetMidi.Common.FourBitNumber.Parse(index.ToString());
                    }
                }
                midiFile.Chunks.Add(data.Value.SourceTrackChunk);

                if (data.Value.SourceTrackChunk.ManageNotes().Notes.Count() > 0)
                    index++;
            }
            //and set the tempo map
            midiFile.ReplaceTempoMap(bmpSong.SourceTempoMap);

            //prepare the performer data
            List<PerformerData> datas = new List<PerformerData>();
            foreach (var t in performerDatas)
                datas.Add(t);

            _sequencer = new Sequencer(SelectedBard, midiFile, datas, track);

            
        }

        /// <summary>
        /// Starts the performance
        /// </summary>
        /// <returns></returns>
        public void StartPerformance()
        {
            if (_sequencer != null)
            {
                _NoteKeyDelay = BmpPigeonhole.Instance.NoteKeyDelay;
                BmpPigeonhole.Instance.NoteKeyDelay = 1;
                _sequencer.Start();
            }
        }

        /// <summary>
        /// Pause the performance
        /// </summary>
        /// <returns></returns>
        public void PausePerformance()
        {
            if (_sequencer != null)
            {
                BmpPigeonhole.Instance.NoteKeyDelay = _NoteKeyDelay;
                _sequencer.Pause();
            }
        }

        /// <summary>
        /// Stops the perfomance
        /// </summary>
        /// <returns></returns>
        public void StopPerformance()
        {
            if (_sequencer != null)
            {
                BmpPigeonhole.Instance.NoteKeyDelay = _NoteKeyDelay;
                _sequencer.Stop();
            }
        }

        /// <summary>
        /// Change the tracknumber (-1 all tracks)
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public void ChangeTracknumber(int track)
        {
            if (_sequencer != null)
                _sequencer.ChangeTracknumer(track);
        }

        /// <summary>
        /// Destroys the sequencer
        /// </summary>
        /// <returns></returns>
        public void DestroySongFromLocalPerformer()
        {
            if (_sequencer != null)
                _sequencer.Destroy();
        }

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

        /// <summary>
        /// Sets the playback at position (timeindex in microseconds)
        /// </summary>
        /// <param name="timeindex"></param>
        /// <returns></returns>
        public void SetPlaybackStart(double timeindex)
        {
            if (_sequencer != null)
                _sequencer.SetPlaybackStart(timeindex);
        }

        ~BmpChoreograph() { Dispose(); }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}