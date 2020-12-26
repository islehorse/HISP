using HISP.Server;


namespace HISP.Player
{
    class Highscore
    {
        public static bool RegisterHighscore(int playerId, string gameTitle, int score, bool time)
        {
            bool isNewScore = true;

            if (!Database.PlayerHasHighscore(playerId, gameTitle))
            {
                Database.AddNewHighscore(playerId, gameTitle, score, time ? "TIME" : "SCORE");
                return isNewScore;
            }
            else
            {
                int currentScore = Database.GetHighscore(playerId, gameTitle);
                if (score < currentScore)
                {
                    score = currentScore;
                    isNewScore = false;
                }

                Database.UpdateHighscore(playerId, gameTitle, score);
                return isNewScore;
            }
        }
    }
}
