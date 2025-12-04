// StatsManager.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Morskoy_Battel
{
    public class StatsManager
    {
        private static readonly Lazy<StatsManager> _instance = new Lazy<StatsManager>(() => new StatsManager());
        public static StatsManager Instance => _instance.Value;

        private const string FilePath = "game_stats.txt";
        private List<GameRecord> _records = new List<GameRecord>();

        private StatsManager()
        {
            LoadStats();
        }

        public IReadOnlyList<GameRecord> GetHumanGameRecords()
        {
            // Только PvP-режимы
            var filtered = _records.FindAll(r =>
                r.Mode == "PvP_afk" || r.Mode == "PvP_on");
            filtered.Sort((a, b) => b.Date.CompareTo(a.Date)); 
            return filtered.AsReadOnly();
        }

        public void AddGameResult(
            string mode,
            string opponentName,
            int opponentRating,
            bool isWin,
            int ratingChange)
        {
            if (mode != "PvP_afk" && mode != "PvP_on")
                return; 

            var record = new GameRecord
            {
                Mode = mode,
                OpponentName = opponentName ?? "Неизвестно",
                OpponentRating = opponentRating,
                IsWin = isWin,
                RatingChange = ratingChange,
                Date = DateTime.Now
            };

            _records.Add(record);
            SaveStats(); 
        }

        private void SaveStats()
        {
            try
            {
                using (var writer = new StreamWriter(FilePath, false, System.Text.Encoding.UTF8))
                {
                    foreach (var r in _records)
                    {
                                                string line = string.Join("|",
                            r.Date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            r.Mode,
                            r.OpponentName.Replace("|", ""), 
                            r.OpponentRating,
                            r.IsWin ? "1" : "0",
                            r.RatingChange
                        );
                        writer.WriteLine(line);
                    }
                }
            }
            catch
            {
                
            }
        }

        private void LoadStats()
        {
            _records.Clear();
            if (!File.Exists(FilePath))
                return;

            try
            {
                using (var reader = new StreamReader(FilePath, System.Text.Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('|');
                        if (parts.Length != 6) continue;

                        if (!DateTime.TryParseExact(parts[0], "yyyy-MM-dd HH:mm:ss",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            continue;

                        string mode = parts[1];
                        string opponentName = parts[2];
                        if (!int.TryParse(parts[3], out int opponentRating)) continue;
                        bool isWin = parts[4] == "1";
                        if (!int.TryParse(parts[5], out int ratingChange)) continue;

                        _records.Add(new GameRecord
                        {
                            Date = date,
                            Mode = mode,
                            OpponentName = opponentName,
                            OpponentRating = opponentRating,
                            IsWin = isWin,
                            RatingChange = ratingChange
                        });
                    }
                }
            }
            catch
            {
                _records.Clear();
            }
        }
    }
}