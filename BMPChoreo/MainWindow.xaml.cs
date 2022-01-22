using BardMusicPlayer.Choreograph;
using BardMusicPlayer.Grunt;
using BardMusicPlayer.Jamboree;
using BardMusicPlayer.Jamboree.Events;
using BardMusicPlayer.Seer;
using BardMusicPlayer.Siren;
using BardMusicPlayer.Transmogrify.Song;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace BMPChoreo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum PlaybackState_Enum
        {
            PLAYBACK_STATE_STOPPED = 0,
            PLAYBACK_STATE_PLAYING,
            PLAYBACK_STATE_PAUSE,
            PLAYBACK_STATE_PLAYNEXT //indicates the next song should be played
        };
        private static PlaybackState_Enum PlaybackState;
        private BmpSong _currentSong;
        private double MaxTime = 0;
        private Game game = null;
        private List<PerformerData> _eventsData;
        private int _EventsIndex = 0;
        private bool _Edit = false;
        private string _xmlFilename = "Performance";
        public MainWindow()
        {
            InitializeComponent();
            BmpSeer.Instance.GameStarted += e => GameStarted(e.Game);
            BmpSeer.Instance.ChatLog += Instance_ChatLog;

            BmpSiren.Instance.SynthTimePositionChanged += Instance_SynthTimePositionChanged;

            BmpJamboree.Instance.OnPartyCreated += Instance_OnPartyCreated;
            BmpJamboree.Instance.OnPartyConnectionChanged += Instance_OnPartyConnectionChanged;
            BmpJamboree.Instance.OnPartyChanged += Instance_OnPartyChanged;
            BmpJamboree.Instance.OnPerformanceStart += Instance_OnPerformanceStart;
        }

        private void Instance_ChatLog(BardMusicPlayer.Seer.Events.ChatLog seerEvent)
        {
            //if (seerEvent.ChatLogLine.Contains(@"<1234567890>GO!"))
            //    this.Dispatcher.BeginInvoke(new Action(() => _ChatLog(seerEvent)));
        }

        private void _ChatLog(BardMusicPlayer.Seer.Events.ChatLog seerEvent)
        {
            if (!_Edit)
            {
                Edit.Background = Brushes.Yellow;
                Edit.Content = "REMO";
                Thread.Sleep(3400);
                BmpChoreograph.Instance.StartPerformance();
            }
        }

        private void GameStarted(Game g)
        {
            game = g;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "MIDI file|*.mid;*.midi|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            if (!openFileDialog.CheckFileExists)
                return;

            PlaybackState = PlaybackState_Enum.PLAYBACK_STATE_STOPPED;

            _currentSong = BmpSong.OpenMidiFile(openFileDialog.FileName).Result;
            _ = BmpSiren.Instance.Load(_currentSong);

            string xmlfilename = Path.ChangeExtension(openFileDialog.FileName, "xml");
            _xmlFilename = xmlfilename;
            BmpChoreograph.Instance.DestroySongFromLocalPerformer();

            if (File.Exists(xmlfilename))
            {
                loadPerformanceData(xmlfilename);
                BmpChoreograph.Instance.LoadPerfomance(BmpSong.OpenMidiFile(openFileDialog.FileName).Result, _eventsData, 1);
            }
            else
            {
                _eventsData = new List<PerformerData>();
                Events.ItemsSource = _eventsData;
            }
            MaxTime = 0;
            _EventsIndex = 0;            
        }

        private void Instance_SynthTimePositionChanged(string songTitle, double currentTime, double endTime, int activeVoices)
        {
            this.Dispatcher.BeginInvoke(new Action(() => _SynthTimePositionChanged(songTitle, currentTime, endTime, activeVoices)));
        }

        private void _SynthTimePositionChanged(string songTitle, double currentTime, double endTime, int activeVoices)
        {
            //Timer
            if (MaxTime == 0)
            {
                MaxTime = 1;
                MaxTime = endTime;

                this.timebar.Minimum = 0;
                this.timebar.Maximum = MaxTime;

                List<string> times = new List<string>();
                for (double i = 0; i != MaxTime; i++)
                    times.Add(i.ToString());
                this.EventTimes.ItemsSource = times;
            }
            else
            {
                this.SetEventTime(currentTime);
            }
        }

        private void SetEventTime(double current_time)
        {
            PushTheButton(current_time);

            this.timebar.Value = current_time;
            EventTimes.SelectedIndex = (int)current_time;
            EventTimes.ScrollIntoView(EventTimes.SelectedItem);
        }

        /// <summary>
        /// taps the keys in edit mode
        /// </summary>
        /// <param name="current_time"></param>
        private void PushTheButton(double current_time)
        {
            if (game == null)
                return;

            if (PlaybackState == PlaybackState_Enum.PLAYBACK_STATE_STOPPED)
                return;

            if (_EventsIndex == _eventsData.Count())
                return;

            PerformerData d = _eventsData.ElementAt(_EventsIndex);
            if (Enumerable.Range(Convert.ToInt32(d.Timestamp), Convert.ToInt32(d.Timestamp)+200).Contains((int)current_time))
            {
                Events.SelectedIndex = (int)_EventsIndex;
                Events.ScrollIntoView(Events.SelectedItem);
                _EventsIndex++;

                if (d.Modifier == null)
                    d.Modifier = "";

                if (d.Modifier.Contains("TEXT"))
                {
                    _ = GameExtensions.SendLyricLine(game, d.Key);
                    return;
                }

                BardMusicPlayer.Quotidian.Enums.Keys key = KeyTranslation.ASCIIToGame[d.Key];
                if (d.Modifier.Contains("SHIFT"))
                    key = (int)BardMusicPlayer.Quotidian.Enums.Keys.Shift + key;
                else if (d.Modifier.Contains("CTRL"))
                    key = (int)BardMusicPlayer.Quotidian.Enums.Keys.Control + key;

                _ = GameExtensions.SyncTapKey(game, (BardMusicPlayer.Quotidian.Enums.Keys)key);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if(!_Edit)
            {
                BmpChoreograph.Instance.StartPerformance();
                return;
            }

            if (!BmpSiren.Instance.IsReadyForPlayback)
                return;
            PlaybackState = PlaybackState_Enum.PLAYBACK_STATE_PLAYING;
            BmpSiren.Instance.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (!_Edit)
            {
                BmpChoreograph.Instance.PausePerformance();
                return;
            }

            if (!BmpSiren.Instance.IsReadyForPlayback)
                return;
            PlaybackState = PlaybackState_Enum.PLAYBACK_STATE_PAUSE;
            BmpSiren.Instance.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Edit.Content = "Edit";
            Edit.Background = _Edit ? Brushes.Red : Brushes.AliceBlue;
            if (!_Edit)
            {
                BmpChoreograph.Instance.StopPerformance();
                return;
            }

            if (!BmpSiren.Instance.IsReadyForPlayback)
                return;
            PlaybackState = PlaybackState_Enum.PLAYBACK_STATE_STOPPED;
            BmpSiren.Instance.Stop();
            _EventsIndex = 0;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (!_Edit)
            {
                _Edit = true;
                Edit.Background = Brushes.Red;
            }
            else
            {
                _Edit = false;
                Edit.Background = Brushes.AliceBlue;
            }
        }

        private void LoadPerf_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Performance"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Perfomer documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                string filename = dlg.FileName;
                loadPerformanceData(filename);
            }
        }

        private void SavePerf_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = _xmlFilename; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Perfomer documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                //Sort
                var events = Events.ItemsSource as List<PerformerData>;
                events.Sort(delegate (PerformerData c1, PerformerData c2) { return Convert.ToInt32(c1.Timestamp).CompareTo(Convert.ToInt32(c2.Timestamp)); });
                Events.Items.Refresh();

                // Save document
                string filename = dlg.FileName;
                string d = StaticHelpers.ToXML(_eventsData);
                StreamWriter stream = new StreamWriter(filename);
                stream.Write(d);
                stream.Close();
            }
        }

        /// <summary>
        /// load the performance data from the xml
        /// </summary>
        /// <param name="filename"></param>
        private void loadPerformanceData(string filename)
        {
            //Load the xml
            var doc = XDocument.Load(new StreamReader(@filename));

            //output a list of PerformanceData
            var xmlList = doc.Root
                .Descendants("PerformerData")
                .Select(node => new PerformerData
                {
                    Timestamp = node.Element("Timestamp").Value,
                    Performer = node.Element("Performer").Value,
                    Modifier = node.Element("Modifier").Value,
                    Key = node.Element("Key").Value
                })
                .ToList();

            List<PerformerData> dataSource = Events.ItemsSource as List<PerformerData>;
            if (dataSource == null)
            {
                _eventsData = new List<PerformerData>();
                Events.ItemsSource = _eventsData;
            }
            else
                dataSource.Clear();

            dataSource = Events.ItemsSource as List<PerformerData>;
            foreach (PerformerData perfData in xmlList)
                dataSource.Add(perfData);

            Events.Items.Refresh();
        }
    }
}
