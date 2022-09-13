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
