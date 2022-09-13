using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snake.Model;

namespace Snake.DAL
{
    public class Highscore
    {
        private string path;
        public Highscore()
        {
            path = "C:\\Users\\elias\\Downloads\\Snake scores\\Scores.txt";
        }

        /// <summary>
        /// Saves the score to a auto created file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="score"></param>
        public void SaveScore(string name, int score)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            using (StreamWriter sw = new(path, true))
            {
                sw.WriteLine(name);
                sw.WriteLine(score);
            }
        }

        /// <summary>
        /// Reads scores from file
        /// </summary>
        /// <returns>List of names and scores</returns>
        public List<SnakeScore> GetScores()
        {
            List<SnakeScore> scores = new List<SnakeScore>();
            try
            {
                using (StreamReader sr = new("C:\\Users\\elias\\Downloads\\Snake scores\\Scores.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string name = sr.ReadLine();
                        int.TryParse(sr.ReadLine(), out int score);
                        scores.Add(new(name, score));
                    }
                }
            }
            catch (Exception)
            {

            }
            return scores;
        }

        /// <summary>
        /// Overwrites the data the file with an empty string
        /// </summary>
        public void ClearScores()
        {
            if (File.Exists(path))
            {
                File.WriteAllText(path, string.Empty);
            }
            else
            {
                File.Create(path);
            }
        }
    }
}
