using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.DAL
{
    public class Score
    {
        public void WriteScore(string filePath, int score)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine(score);
            }
        }

        public int ReadScore(string filePath)
        {
            int score = 0;
            using (StreamReader sr = new StreamReader(filePath))
            {
                int.TryParse(sr.ReadLine(), out score);
            }
            return score;
        }
    }
}
