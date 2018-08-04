using SpotifyRecorder.src;
using System.Windows;

namespace SpotifyRecorder
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AudioRecorder recorder = new AudioRecorder();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            recorder.StartRecording();
            btnStop.IsEnabled = true;

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            recorder.StopRecording();


            btnStart.IsEnabled = true;

        }
    }
}
