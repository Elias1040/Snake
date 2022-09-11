using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Snake.MainWindow;
using System.Windows.Threading;
using System.Windows;

namespace Snake.Model
{
    public class Clusterfuck
    {
        public List<Snakepart> Snakeparts { get; set; }
        public SnakeDirection SnakeDirection { get; set; }
        public DispatcherTimer GameTimer { get; set; }
        public int SnakeLength { get; set; }
        public Snakefood SnakeFood { get; set; }
        public int CurrentScore { get; set; }
        public Clusterfuck()
        {
            Snakeparts = new List<Snakepart>();
            GameTimer = new DispatcherTimer();
        }
    }
}
