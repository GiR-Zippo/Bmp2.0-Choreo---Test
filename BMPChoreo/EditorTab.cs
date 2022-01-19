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
    /// Interaktionslogik für MainWindow Edit Tab
    /// </summary>
    public partial class MainWindow : Window
    {

        private void timebar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            BmpSiren.Instance.SetPosition((int)this.timebar.Value);
        }


        /// <summary>
        /// Add the current timestamp to the "Events" list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTimeStamp_Click(object sender, RoutedEventArgs e)
        {
            var d = Events.ItemsSource as List<PerformerData>;
            d.Add(new PerformerData { Timestamp = (string)EventTimes.SelectedItem, Key = "", Modifier = "NULL" });
            Console.WriteLine(d);
            Events.Items.Refresh();

        }

        /// <summary>
        /// Removes a selected item from the "Events" list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveTimeStamp_Click(object sender, RoutedEventArgs e)
        {
            var d = Events.ItemsSource as List<PerformerData>;
            d.RemoveAt(Events.SelectedIndex);
            Events.Items.Refresh();

        }

        /// <summary>
        /// sorts the "Events" list with lowest timestamp first
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortTimeStamp_Click(object sender, RoutedEventArgs e)
        {
            var dataSource = Events.ItemsSource as List<PerformerData>;
            dataSource.Sort(delegate (PerformerData c1, PerformerData c2) { return Convert.ToInt32(c1.Timestamp).CompareTo(Convert.ToInt32(c2.Timestamp)); });
            Events.Items.Refresh();
        }
    }
}