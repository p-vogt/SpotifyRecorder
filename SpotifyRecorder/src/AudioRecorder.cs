using NAudio.Lame;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading;

namespace SpotifyRecorder.src
{
    class AudioRecorder
    {
        private WasapiLoopbackCapture capture;
        private WaveFileWriter writer;
        string outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NAudio");
        string outputFileName = "recorded";
        string outputFilePathWav => Path.Combine(outputFolder, $"{outputFileName}.wav");
        string outputFilePathMP3 => Path.Combine(outputFolder, $"{outputFileName}.mp3");

        public void StartRecording()
        {
            Directory.CreateDirectory(outputFolder);

            capture = new WasapiLoopbackCapture();
            writer = new WaveFileWriter(outputFilePathWav, capture.WaveFormat);
            capture.DataAvailable += (s, a) =>
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);
                if (writer.Position > capture.WaveFormat.AverageBytesPerSecond * 20)
                {
                    capture.StopRecording();
                }
            };
            capture.RecordingStopped += (s, a) =>
            {
                writer.Dispose();
                writer = null;
                capture.Dispose();
            };
            capture.StartRecording();
            new Thread(() =>
            {
                while (capture.CaptureState != NAudio.CoreAudioApi.CaptureState.Stopped)
                {
                    Thread.Sleep(500);
                }
            }).Start();
        }

        public void StopRecording()
        {
            capture?.StopRecording();
            new Thread(() =>
            {
                // TODO check if file is in use
                while (writer != null)
                {
                    Thread.Sleep(50);
                }
                WaveToMP3(outputFilePathWav, outputFilePathMP3);
            }).Start();
        }

        // Convert WAV to MP3 using libmp3lame library
        public static void WaveToMP3(string waveFileName, string mp3FileName, int bitRate = 320)
        {
            using (var reader = new AudioFileReader(waveFileName))
            {
                using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
                {
                    reader.CopyTo(writer);
                }
            }

        }

        // Convert MP3 file to WAV using NAudio classes only
        public static void MP3ToWave(string mp3FileName, string waveFileName)
        {
            using (var reader = new Mp3FileReader(mp3FileName))
            {
                using (var writer = new WaveFileWriter(waveFileName, reader.WaveFormat))
                {
                    reader.CopyTo(writer);
                }
            }
        }
    }
}
