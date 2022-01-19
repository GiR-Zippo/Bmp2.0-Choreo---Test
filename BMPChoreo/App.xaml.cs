using BardMusicPlayer.Choreograph;
using BardMusicPlayer.Coffer;
using BardMusicPlayer.Grunt;
using BardMusicPlayer.Jamboree;
using BardMusicPlayer.Pigeonhole;
using BardMusicPlayer.Seer;
using BardMusicPlayer.Siren;
using System.Diagnostics;
using System.Windows;

namespace BMPChoreo
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string path = @"data\";

            BmpPigeonhole.Initialize(path + @"\Configuration.json");

            // var view = (MainView)View;
            // LogManager.Initialize(new(view.Log));

            BmpCoffer.Initialize(path + @"\MusicCatalog.db");
            BmpSeer.Instance.SetupFirewall("BMPChoreo");
            BmpSeer.Instance.Start();
            BmpGrunt.Instance.Start();
            BmpChoreograph.Instance.Start();

            BmpSiren.Instance.Setup();

            BmpJamboree.Instance.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //LogManager.Shutdown();
            BmpJamboree.Instance.Stop();

            if (BmpSiren.Instance.IsReadyForPlayback)
                BmpSiren.Instance.Stop();
            BmpSiren.Instance.ShutDown();

            BmpChoreograph.Instance.Stop();

            BmpGrunt.Instance.Stop();
            BmpSeer.Instance.Stop();
            BmpSeer.Instance.DestroyFirewall("BardMusicPlayer");
            BmpCoffer.Instance.Dispose();
            BmpPigeonhole.Instance.Dispose();

            //Wasabi hangs kill it with fire
            Process.GetCurrentProcess().Kill();
        }
    }
}
