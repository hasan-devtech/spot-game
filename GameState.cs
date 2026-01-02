using System.Collections.Generic;
using System.Drawing;

namespace SpotTheDifferencesGame
{
    public class GameState
    {
        public Bitmap LeftImage { get; set; }
        public Bitmap RightImage { get; set; }
        public List<Rectangle> FoundDifferences { get; } = new List<Rectangle>();
        public int RemainingDifferences { get; set; }
        public int TimeRemaining { get; set; }
        public int AttemptsLeft { get; set; }
        public DifficultyLevel CurrentDifficulty { get; set; } = DifficultyLevel.Medium;
        public GameMode CurrentGameMode { get; set; } = GameMode.TimeBased;
        public ImageSource ImageSource { get; set; } = ImageSource.GenerateModified;
        public void Reset()
        {
            LeftImage?.Dispose();
            LeftImage = null;
            RightImage?.Dispose();
            RightImage = null;
            FoundDifferences.Clear();
            RemainingDifferences = 0;
            TimeRemaining = 0;
            AttemptsLeft = 0;
        }
    }
}