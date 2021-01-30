using HISP.Server;
using System.Collections.Generic;

namespace HISP.Player
{
    public class Highscore
    {
        public class HighscoreTableEntry
        {
            public int UserId;
            public string GameName;
            public int Wins;
            public int Looses;
            public int TimesPlayed;
            public int Score;
            public string Type;
        }
        public HighscoreTableEntry[] HighscoreList
        {
            get
            {
                return highScoreList.ToArray();
            }
        }

        private User baseUser;
        private List<HighscoreTableEntry> highScoreList = new List<HighscoreTableEntry>();

        public Highscore(User user)
        {
            baseUser = user;
            HighscoreTableEntry[] highscores = Database.GetPlayerHighScores(user.Id);
            foreach (HighscoreTableEntry highscore in highscores)
                highScoreList.Add(highscore);
        }
        public HighscoreTableEntry GetHighscore(string gameTitle)
        {
            foreach (HighscoreTableEntry highscore in HighscoreList)
            {
                if (highscore.GameName == gameTitle)
                {
                    return highscore;
                }
            }
            throw new KeyNotFoundException("Highscore for " + gameTitle + " Not found.");
        }
        public bool HasHighscore(string gameTitle)
        {
            foreach(HighscoreTableEntry highscore in HighscoreList)
            {
                if(highscore.GameName == gameTitle)
                {
                    return true;
                }
            }
            return false;
        }

        public bool UpdateHighscore(string gameTitle, int score, bool time)
        {
            bool isNewScore = true;

            if (!HasHighscore(gameTitle))
            {
                string type = time ? "TIME" : "SCORE";
                Database.AddNewHighscore(baseUser.Id, gameTitle, score, type);

                HighscoreTableEntry newHighscore = new HighscoreTableEntry();
                newHighscore.UserId = baseUser.Id;
                newHighscore.GameName = gameTitle;
                newHighscore.Wins = 0;
                newHighscore.Looses = 0;
                newHighscore.TimesPlayed = 1;
                newHighscore.Score = score;
                newHighscore.Type = type;
                highScoreList.Add(newHighscore);

                return isNewScore;
            }
            else
            {
                int currentScore = GetHighscore(gameTitle).Score;
                if (score < currentScore)
                {
                    score = currentScore;
                    isNewScore = false;
                }

                Database.UpdateHighscore(baseUser.Id, gameTitle, score);
                
                for(int i = 0; i < highScoreList.Count; i++)
                {
                       
                    if(highScoreList[i].GameName == gameTitle)
                    {
                        highScoreList[i].TimesPlayed += 1;
                        highScoreList[i].Score = score;
                    }
                }

                return isNewScore;
            }

        }
    }
}
