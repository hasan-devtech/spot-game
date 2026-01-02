using NAudio.Wave;
using System.IO;

namespace SpotTheDifferencesGame
{
    public class SoundManager
    {
        private readonly AudioFileReader correctAudioFile;
        private readonly AudioFileReader wrongAudioFile;
        private readonly AudioFileReader winAudioFile;
        private readonly AudioFileReader loseAudioFile;
        private readonly WaveOutEvent soundPlayer;

        public SoundManager()
        {
            soundPlayer = new WaveOutEvent();
            string[] searchPaths = { Path.Combine(Application.StartupPath, "Sounds") };
            foreach (var path in searchPaths)
            {
                string correctPath = Path.Combine(path, "correct.wav");
                if (File.Exists(correctPath) && correctAudioFile == null)
                {
                    correctAudioFile = new AudioFileReader(correctPath);
                }
                string wrongPath = Path.Combine(path, "wrong.wav");
                if (File.Exists(wrongPath) && wrongAudioFile == null)
                {
                    wrongAudioFile = new AudioFileReader(wrongPath);
                }
                string winPath = Path.Combine(path, "goodResult.mp3");
                if (File.Exists(winPath) && winAudioFile == null)
                {
                    winAudioFile = new AudioFileReader(winPath);
                }
                string losePath = Path.Combine(path, "lose.mp3");
                if (File.Exists(losePath) && loseAudioFile == null)
                {
                    loseAudioFile = new AudioFileReader(losePath);
                }
            }
        }

        public void PlaySound(string soundType)
        {
            try
            {
                soundPlayer.Stop();
                AudioFileReader? audioFile = null;
                switch (soundType)
                {
                    case "correct":
                        audioFile = correctAudioFile;
                        break;
                    case "wrong":
                        audioFile = wrongAudioFile;
                        break;
                    case "win":
                        audioFile = winAudioFile;
                        break;
                    case "lose":
                        audioFile = loseAudioFile;
                        break;
                    default:
                        return; 
                }
                if (audioFile == null) return;
                audioFile.Position = 0;
                soundPlayer.Init(audioFile);
                soundPlayer.Play();
            }
            catch{}
        }
        
        public void Dispose()
        {
            soundPlayer?.Dispose();
            correctAudioFile?.Dispose();
            wrongAudioFile?.Dispose();
        }
    }
}