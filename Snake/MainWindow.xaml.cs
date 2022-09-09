using Microsoft.Win32;
using Snake.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Snake.DAL;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Snakepart> snakeparts;
        private SolidColorBrush snakeBodyBrush;
        private SolidColorBrush snakeHeadBrush;
        private SnakeDirection snakeDirection;
        private DispatcherTimer gameTimer;
        private int snakeLength;
        private Random random;
        private UIElement snakeFood;
        private SolidColorBrush foodBrush;
        private int currentScore;

        const int SnakeSquareSize = 20;
        const int SnakeStartLength = 3;
        const int SnakeStartSpeed = 400;
        const int SnakeSpeedThreshold = 100;

        public MainWindow()
        {
            InitializeComponent();
            snakeparts = new List<Snakepart>();
            snakeDirection = SnakeDirection.Right;
            snakeBodyBrush = Brushes.LightBlue;
            snakeHeadBrush = Brushes.Yellow;
            foodBrush = Brushes.Red;
            gameTimer = new DispatcherTimer();
            random = new Random();
            currentScore = 0;
            gameTimer.Tick += GameTimer_Tick;
        }
        public enum SnakeDirection { Left, Right, Up, Down }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawArena();
            StartNewGame();
        }
        private void StartNewGame()
        {
            gameTimer.IsEnabled = false;
            foreach (Snakepart snakepart in snakeparts)
            {
                if (snakepart.UiElement != null)
                {
                    Arena.Children.Remove(snakepart.UiElement);
                }
            }
            if (snakeFood != null)
            {
                Arena.Children.Remove(snakeFood);
            }
            snakeparts.Clear();
            snakeFood = null;
            currentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeparts.Add(new()
            {
                Position = new(SnakeSquareSize * 5, SnakeSquareSize * 5),
            });
            gameTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);
            DrawSnake();
            DrawSnakeFood();
            gameTimer.IsEnabled = true;
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }
        private void EndGame()
        {
            gameTimer.IsEnabled = false;
            foreach (Snakepart snakepart in snakeparts)
            {
                if (snakepart.UiElement != null)
                {
                    Arena.Children.Remove(snakepart.UiElement);
                }
            }
            MessageBox.Show($"You died!!!\nScore: {currentScore}");
        }




        private Point GetFoodPosition()
        {
            int maxX = (int)(Arena.ActualWidth / SnakeSquareSize);
            int maxY = (int)(Arena.ActualHeight / SnakeSquareSize);
            int foodX = random.Next(0, maxX) * SnakeSquareSize;
            int foodY = random.Next(0, maxY) * SnakeSquareSize;

            foreach (Snakepart snakepart in snakeparts)
            {
                if ((snakepart.Position.X == foodX) && (snakepart.Position.Y == foodY))
                {
                    return GetFoodPosition();
                }
            }

            return new(foodX, foodY);
        }
        private void EatSnakeFood()
        {
            snakeLength++;
            currentScore++;
            int interval = Math.Max(SnakeSpeedThreshold, (int)gameTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTimer.Interval = TimeSpan.FromMilliseconds(interval);
            Arena.Children.Remove(snakeFood);
            DrawSnakeFood();
            UpdateGameStatus();
        }
        private void UpdateGameStatus()
        {
            this.Title = "Snake - Score: " + currentScore + "Game speed: " + gameTimer.Interval.TotalMilliseconds;
        }


        private void MoveSnake()
        {
            while (snakeparts.Count >= snakeLength)
            {
                Arena.Children.Remove(snakeparts[0].UiElement);
                snakeparts.RemoveAt(0);
            }

            foreach (Snakepart snakepart in snakeparts)
            {
                (snakepart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakepart.IsHead = false;
            }

            Snakepart snakeHead = snakeparts.Last();
            double x = snakeHead.Position.X;
            double y = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    x -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    x += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    y -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    y += SnakeSquareSize;
                    break;
            }
            snakeparts.Add(new()
            {
                IsHead = true,
                Position = new(x, y),
            });
            DrawSnake();
            CollisionCheck();
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection direction = snakeDirection;
            switch (e.Key)
            {
                case Key.Up or Key.W:
                    if (snakeDirection != SnakeDirection.Down)
                    {
                        snakeDirection = SnakeDirection.Up;
                    }
                    break;
                case Key.Down or Key.S:
                    if (snakeDirection != SnakeDirection.Up)
                    {
                        snakeDirection = SnakeDirection.Down;
                    }
                    break;
                case Key.Left or Key.A:
                    if (snakeDirection != SnakeDirection.Right)
                    {
                        snakeDirection = SnakeDirection.Left;
                    }
                    break;
                case Key.Right or Key.D:
                    if (snakeDirection != SnakeDirection.Left)
                    {
                        snakeDirection = SnakeDirection.Right;
                    }
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }
            if (direction != snakeDirection && gameTimer.IsEnabled)
            {
                MoveSnake();
            }

        }
        private void CollisionCheck()
        {
            Snakepart snakeHead = snakeparts.Last();
            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
            }

            if ((snakeHead.Position.Y < Arena.MinHeight) || (snakeHead.Position.Y >= Arena.ActualHeight) ||
                (snakeHead.Position.X < Arena.MinWidth) || (snakeHead.Position.X >= Arena.ActualWidth))
            {
                EndGame();
            }

            foreach (Snakepart snakepart in snakeparts.Take(snakeparts.Count - 1))
            {
                if ((snakeHead.Position.X == snakepart.Position.X) && (snakeHead.Position.Y == snakepart.Position.Y))
                {
                    EndGame();
                }
            }
        }

        private void SaveScore_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                Score score = new();
                score.WriteScore(fileDialog.FileName, currentScore);
            }
            gameTimer.Start();
        }
        private void LoadScore_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            OpenFileDialog fileDialog = new();
            if (fileDialog.ShowDialog() == true)
            {
                Score score = new();
                currentScore = score.ReadScore(fileDialog.FileName);
            }
            gameTimer.Start();
        }
        
        
        private void DrawArena()
        {
            bool isOdd = false;
            int x = 0, y = 0;
            int rowCounter = 0;
            bool isFull = false;
            while (!isFull)
            {
                Rectangle rect = new Rectangle()
                {
                    Fill = isOdd ? Brushes.YellowGreen : Brushes.GreenYellow,
                    Height = SnakeSquareSize,
                    Width = SnakeSquareSize,
                };
                Arena.Children.Add(rect);
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                x += SnakeSquareSize;
                isOdd = !isOdd;
                if (x >= Arena.ActualWidth)
                {
                    y += SnakeSquareSize;
                    x = 0;
                    rowCounter++;
                    isOdd = (rowCounter % 2 != 0);
                }
                if (y >= Arena.ActualHeight)
                {
                    isFull = true;
                }
            }
        }
        private void DrawSnake()
        {
            foreach (Snakepart snakepart in snakeparts)
            {
                if (snakepart.UiElement == null)
                {
                    snakepart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakepart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    Arena.Children.Add(snakepart.UiElement);
                    Canvas.SetTop(snakepart.UiElement, snakepart.Position.Y);
                    Canvas.SetLeft(snakepart.UiElement, snakepart.Position.X);
                }
            }
        }
        private void DrawSnakeFood()
        {
            Point foodPosition = GetFoodPosition();
            snakeFood = new Ellipse()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodBrush
            };
            Arena.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }

    }
}
