using BardMusicPlayer.Choreograph;
using BardMusicPlayer.Jamboree;
using BardMusicPlayer.Jamboree.Events;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace BMPChoreo
{
    /// <summary>
    /// The remote routines
    /// </summary>
    public partial class MainWindow : Window
    {
        private void Instance_OnPerformanceStart(object sender, PerformanceStartEvent e)
        {
            this.Dispatcher.BeginInvoke(new Action(() => _OnPerformanceStart(e)));
        }

        private void _OnPerformanceStart(PerformanceStartEvent e)
        {
            long current = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long server = e.TimeStamp;
            int delay = (int)current - (int)server;

            Thread.Sleep(3500 - delay);
            Edit.Background = Brushes.Yellow;
            Edit.Content = "REMO";
            BmpChoreograph.Instance.StartPerformance();
        }

        private void Instance_OnPartyCreated(object sender, PartyCreatedEvent e)
        {
            string Token = e.Token;
            this.Dispatcher.BeginInvoke(new Action(() => this.PartyToken_TextBox.Text = Token));
        }


        private void Instance_OnPartyConnectionChanged(object sender, PartyConnectionChangedEvent e)
        {
            PartyConnectionChangedEvent.ResponseCode code = e.Code;
            string Message = e.Message;
            this.Dispatcher.BeginInvoke(new Action(() => this.PartyToken_TextBox.Text = Message));

            //tell the server we are here and a dancer
            if (code == PartyConnectionChangedEvent.ResponseCode.OK)
                BmpJamboree.Instance.SendPerformerJoin(1, "test Player");// game.PlayerName);
        }

        private void Instance_OnPartyChanged(object sender, PartyChangedEvent e)
        {
            var t = BmpJamboree.Instance.GetPartyMembers();
            this.Dispatcher.BeginInvoke(new Action(() => this.PerformerList.ItemsSource = t));
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string token = PartyToken_TextBox.Text;
            PartyToken_TextBox.Text = "Please wait...";
            BmpJamboree.Instance.CreateParty(token);
        }

        private void Join_Click(object sender, RoutedEventArgs e)
        {
            string token = PartyToken_TextBox.Text;
            PartyToken_TextBox.Text = "Please wait...";
            BmpJamboree.Instance.JoinParty(token);
        }
        private void Leave_Click(object sender, RoutedEventArgs e)
        {
            BmpJamboree.Instance.LeaveParty();
        }

    }
}
