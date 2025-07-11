using Android.Media;
using Application = Android.App.Application;
using TunerApp.Services;

[assembly: Dependency(typeof(TunerApp.Platforms.Android.SoundPlayer))]

namespace TunerApp.Platforms.Android
{
    public class SoundPlayer : ISoundPlayer
    {
        private MediaPlayer? _player;

        public SoundPlayer()
        {
            int resId = Application.Context.Resources.GetIdentifier("tuned", "raw", Application.Context.PackageName);
            if (resId != 0)
            {
                _player = MediaPlayer.Create(Application.Context, resId);
            }
        }

        public void PlayTunedSound()
        {
            if (_player != null)
            {
                if (_player.IsPlaying)
                {
                    _player.Stop();
                    _player.Prepare();
                }

                _player.SeekTo(0);
                _player.Start();
            }
        }
    }

}
