using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Model
{
    public class SnakeScore
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public SnakeScore(string name, int score)
        {
            Name = name;
            Score = score;
        }
    }
}
