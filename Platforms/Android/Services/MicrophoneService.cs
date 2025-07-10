using Android.Media;
using System.Linq;
using TunerApp.Services;
using Application = Android.App.Application;
using Microsoft.Maui.ApplicationModel;

[assembly: Dependency(typeof(TunerApp.Platforms.Android.Services.MicrophoneService))]
namespace TunerApp.Platforms.Android.Services
{
    public class MicrophoneService : IMicrophoneService
    {
        private AudioRecord _audioRecord;
        private volatile bool _isRecording;
        private Thread _recordingThread;
        private const int SampleRate = 44100; // Match your AudioCaptureManager's rate

        public async Task StartRecordingAsync(Action<byte[]> onDataAvailable)
        {
            var status = await Permissions.RequestAsync<Permissions.Microphone>();

            if (status != PermissionStatus.Granted)
            {
                Console.WriteLine("Permissão negada para o microfone.");
                return;
            }


            var _bufferSize = AudioRecord.GetMinBufferSize(
                44100,
                ChannelIn.Mono,
                Encoding.Pcm16bit);

            _audioRecord = new AudioRecord(
                AudioSource.Mic,
                44100,
                ChannelIn.Mono,
                Encoding.Pcm16bit,
                _bufferSize);

            _audioRecord.StartRecording();
            _isRecording = true;

            _recordingThread = new Thread(() =>
            {
                byte[] buffer = new byte[_bufferSize];
                while (_isRecording)
                {
                    int read = _audioRecord.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        onDataAvailable?.Invoke(buffer.Take(read).ToArray());
                    }
                }
            });

            _recordingThread.Start();
        }

        public void StopRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;

            try
            {
                _audioRecord?.Stop();
            }
            catch { /* Ignore stop errors */ }

            try
            {
                _audioRecord?.Release();
            }
            catch { /* Ignore release errors */ }

            _audioRecord = null;
            _recordingThread?.Join(200); // Wait a bit for thread to finish
            _recordingThread = null;
        }
    }
}