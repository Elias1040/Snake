using Microsoft.Win32;
using Snake.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Snake.MainWindow;

namespace Snake.DAL
{
    public class SaveState
    {
        /// <summary>
        /// Writes Save data to text file
        /// </summary>
        /// <param name="clusterfuck"></param>
        public void WriteState(Clusterfuck clusterfuck)
        {
            SaveFileDialog fileDialog = new()
            {
                Filter = "Text Files (*.txt)|*.txt",
                DefaultExt = "txt",
                AddExtension = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                using (StreamWriter sw = new(fileDialog.FileName))
                {
                    sw.WriteLine($"Snakelength: {clusterfuck.SnakeLength}");
                    sw.WriteLine($"Milliseconds: {clusterfuck.GameTimer.Interval.TotalMilliseconds}");
                    clusterfuck.Snakeparts.ForEach(part => sw.WriteLine($"PosX: {part.Position.X}\nPosY: {part.Position.Y}\nIsHead: {part.IsHead}"));
                    sw.WriteLine($"Direction: {clusterfuck.SnakeDirection}");
                    sw.WriteLine($"Score: {clusterfuck.CurrentScore}");
                    sw.WriteLine($"FoodPosX: {clusterfuck.SnakeFood.Position.X}\nFoodPosY: {clusterfuck.SnakeFood.Position.Y}");
                }
            }
        }

        /// <summary>
        /// Reads save data from a save file
        /// </summary>
        /// <param name="snakeSquareSize"></param>
        /// <param name="brush"></param>
        /// <returns>The save if the file is correctly formatted</returns>
        public Clusterfuck? ReadState(int snakeSquareSize, Brush brush)
        {
            OpenFileDialog fileDialog = new();
            if (fileDialog.ShowDialog() == true)
            {
                using (StreamReader sr = new(fileDialog.FileName))
                {
                    try
                    {
                        Clusterfuck clusterfuck = new();
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
                        return clusterfuck;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Save file is corrupted");
                    }
                }
            }
            return null;
        }
    }
}
