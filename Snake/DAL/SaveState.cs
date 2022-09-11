using Microsoft.Win32;
using Snake.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Snake.MainWindow;

namespace Snake.DAL
{
    public class SaveState
    {
        public void WriteScore(string filePath, Clusterfuck clusterfuck)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine($"Snakelength: {clusterfuck.SnakeLength}");
                sw.WriteLine($"Milliseconds: {clusterfuck.GameTimer.Interval.TotalMilliseconds}");
                clusterfuck.Snakeparts.ForEach(part => sw.WriteLine($"PosX: {part.Position.X}\nPosY: {part.Position.Y}\nIsHead: {part.IsHead}"));
                sw.WriteLine($"Direction: {clusterfuck.SnakeDirection}");
                sw.WriteLine($"Score: {clusterfuck.CurrentScore}");
                sw.WriteLine($"FoodPosX: {clusterfuck.SnakeFood.Position.X}\nFoodPosY: {clusterfuck.SnakeFood.Position.Y}");
            }
        }

        public Clusterfuck? ReadScore(string filePath, int snakeSquareSize, Brush brush)
        {
            Clusterfuck clusterfuck = new Clusterfuck();
            
                using (StreamReader sr = new StreamReader(filePath))
                {
                    clusterfuck.SnakeLength = Convert.ToInt32(sr.ReadLine().Split(": ")[1]);
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(Convert.ToDouble(sr.ReadLine().Split(": ")[1]));
                    clusterfuck.GameTimer = new() { Interval = timeSpan };
                    for (int i = 0; i < clusterfuck.SnakeLength; i++)
                    {
                        clusterfuck.Snakeparts.Add(new()
                        {
                            Position = new(Convert.ToInt32(sr.ReadLine().Split(": ")[1]), Convert.ToInt32(sr.ReadLine().Split(": ")[1])),
                            IsHead = Convert.ToBoolean(sr.ReadLine().Split(": ")[1]),
                            UiElement = new Rectangle() { Width = snakeSquareSize, Height = snakeSquareSize }
                        });
                    }
                    string direction = sr.ReadLine().Split(": ")[1];
                    if (direction == "Left")
                    {
                        clusterfuck.SnakeDirection = SnakeDirection.Left;
                    }
                    else if (direction == "Right")
                    {
                        clusterfuck.SnakeDirection = SnakeDirection.Right;
                    }
                    else if (direction == "Up")
                    {
                        clusterfuck.SnakeDirection = SnakeDirection.Up;
                    }
                    else
                    {
                        clusterfuck.SnakeDirection = SnakeDirection.Down;
                    }
                    clusterfuck.CurrentScore = Convert.ToInt32(sr.ReadLine().Split(": ")[1]);
                    clusterfuck.SnakeFood = new()
                    {
                        Position = new(Convert.ToInt32(sr.ReadLine().Split(": ")[1]), Convert.ToInt32(sr.ReadLine().Split(": ")[1])),
                        UiElement = new Ellipse()
                        {
                            Height = snakeSquareSize,
                            Width = snakeSquareSize,
                            Fill = brush
                        }
                    };
                }
            
            return clusterfuck;
        }
    }
}
