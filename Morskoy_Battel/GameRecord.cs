using System;

namespace Morskoy_Battel
{
    public class GameRecord
    {
        public string OpponentName { get; set; }
        public int OpponentRating { get; set; }
        public bool IsWin { get; set; }
        public int RatingChange { get; set; } 
        public string Mode { get; set; }
        public DateTime Date { get; set; }

        public string Result => IsWin ? "Победа" : "Поражение";
    }
}