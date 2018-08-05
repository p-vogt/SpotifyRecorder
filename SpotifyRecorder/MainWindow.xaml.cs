using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyRecorder.src;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Navigation;

namespace SpotifyRecorder
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AudioRecorder recorder = new AudioRecorder();
        string accessToken = "";


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

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            var clientID = File.ReadAllText("config.txt");
            var spotifyURL = "https://accounts.spotify.com/authorize";
            var redirectURI = "http://localhost:8888/callback";
            var request = $"{spotifyURL}?response_type=token&client_id={clientID}&redirect_uri={redirectURI}&show_dialog=false&scope=user-read-currently-playing";
            browser.Navigated += browser_navigated;
            browser.Navigate(request);
        }

        private void browser_navigated(object sender, NavigationEventArgs e)
        {
            var code = "";
            var error = "";
            if (!String.IsNullOrEmpty(e.Uri.Query))
            {
                //since we are looking for code for authorization that will be exchanged for request token from the server
                if (e.Uri.Query.StartsWith("?code="))
                {
                    code = e.Uri.Query.Replace("?code=", "");
                    var postData = new byte[0];
                    var header = "Content-Type=application/x-www-form-urlencoded";
                    var request = "https://accounts.spotify.com/authorize?client_id=fbf1ab5527d44179afffb024aef711a9&response_type=token&redirect_uri=http://localhost:8888/callback";
                    browser.Navigate(request);
                }

                if (e.Uri.Query.StartsWith("?error=") || e.Uri.Query.Contains("error="))
                {
                    error = e.Uri.Query.Replace("error=", "");
                }
            }
            if (e.Uri.Fragment.StartsWith("#access_token="))
            {
                var parameters = e.Uri.Fragment.Split('&');
                accessToken = parameters[0].Replace("#access_token=", "");
            }
        }
        WebRequest currentTrackRequest;
        private void btnGetCurrentTrack_Click(object sender, RoutedEventArgs e)
        {
            currentTrackRequest = WebRequest.CreateHttp("https://api.spotify.com/v1/me/player/currently-playing");
            currentTrackRequest.Headers.Add("Authorization", "Bearer " + accessToken);
            currentTrackRequest.Method = "GET";
            currentTrackRequest.BeginGetResponse(GetResponseCallback, currentTrackRequest);
           
        }

        private void GetResponseCallback(IAsyncResult ar)
        {
            var resp = currentTrackRequest.EndGetResponse(ar);
            var response = (HttpWebResponse)resp;
            var streamResponse = response.GetResponseStream();
            var responseString = "";
            using (var streamRead = new StreamReader(streamResponse))
            {
                responseString = streamRead.ReadToEnd();
            }
            response.Close();
            dynamic result = JObject.Parse(responseString);
            string trackname = result.item.name.Value;
            int progress_ms = (int)result.progress_ms.Value;
            int duration_ms = (int) (result.item.duration_ms.Value);
            int remaining_ms = duration_ms - progress_ms;

            dynamic artists = result.item.artists;
            var artistsString = "";
            foreach (var artist in artists)
            {
                artistsString += artist.name.Value + ", ";
            }
            artistsString = artistsString.Substring(0, artistsString.Length - 3); // remove last ", "

            Dispatcher.Invoke(() =>
            {
                labelTrackName.Content = artistsString + " - " + trackname;
            });
        }
    }
}
