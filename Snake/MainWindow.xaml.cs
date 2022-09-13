﻿using Microsoft.Win32;
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
using System.Xml;
using System.Runtime.CompilerServices;
using System.Data;
using System.Collections.ObjectModel;
using System.Media;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SolidColorBrush snakeBodyBrush;
        private SolidColorBrush snakeHeadBrush;
        private SolidColorBrush foodBrush;
        private Random random;
        private List<Snakepart> snakeparts;
        private SnakeDirection snakeDirection;
        private DispatcherTimer gameTimer;
        private int snakeLength;
        private Snakefood snakeFood;
        private int currentScore;
        private bool newGame;
        private SoundPlayer backgroundSound;
        private SoundPlayer respawnSound;
        private SoundPlayer deadSound;
        private SoundPlayer turnSound;
        private SoundPlayer eatSound;

        public ViewModel ViewModel { get; set; }
        private Highscore highscore;

        const int SnakeSquareSize = 20;
        const int SnakeStartLength = 3;
        const int SnakeStartSpeed = 200;
        const int SnakeSpeedThreshold = 100;

        public MainWindow()
        {
            InitializeComponent();
            highscore = new();
            ViewModel = new ViewModel(highscore.GetScores());
            this.DataContext = ViewModel;
            snakeparts = new();
            gameTimer = new();
            snakeFood = new();
            random = new();
            newGame = true;
            snakeDirection = SnakeDirection.Right;
            snakeBodyBrush = Brushes.LightBlue;
            snakeHeadBrush = Brushes.Yellow;
            foodBrush = Brushes.Red;
            currentScore = 0;
            backgroundSound = new(@"C:\Users\elias\source\repos\Snake\Snake\BackgroundMusic.wav");
            respawnSound = new(@"C:\Users\elias\source\repos\Snake\Snake\ResetSound.wav");
            deadSound = new(@"C:\Users\elias\source\repos\Snake\Snake\DeadSound.wav");
            turnSound = new(@"C:\Users\elias\source\repos\Snake\Snake\TurnSound.wav");
            eatSound = new(@"C:\Users\elias\source\repos\Snake\Snake\EatSound.wav");
            gameTimer.Tick += GameTimer_Tick;
        }
        public enum SnakeDirection { Left, Right, Up, Down }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawArena();
        }
        private void StartNewGame()
        {
            if (newGame)
            {
                ClearArena();
                newGame = false;
                snakeLength = SnakeStartLength;
                snakeDirection = SnakeDirection.Right;
                snakeparts.Add(new()
                {
                    Position = new(SnakeSquareSize * 5, SnakeSquareSize * 5),
                });
                gameTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);
                PlaySounds(respawnSound);
                DrawSnake();
                DrawSnakeFood();
            }
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
            UpdateGameStatus();
        }
        private void EndGame()
        {
            gameTimer.IsEnabled = false;
            PlaySounds(deadSound);
            foreach (Snakepart snakepart in snakeparts)
            {
                if (snakepart.UiElement != null)
                {
                    Arena.Children.Remove(snakepart.UiElement);
                }
            }
            newGame = true;
            HighScore.Text = currentScore.ToString();
            Menu.Visibility = Visibility.Visible;
            EndGameScreen.Visibility = Visibility.Visible;
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
            PlaySounds(eatSound);
            Arena.Children.Remove(snakeFood.UiElement);
            DrawSnakeFood();
            UpdateGameStatus();
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
                case Key.R:
                    gameTimer.IsEnabled = false;
                    newGame = true;
                    StartNewGame();
                    gameTimer.IsEnabled = true;
                    break;
                case Key.Escape:
                    if (gameTimer.IsEnabled)
                    {
                        gameTimer.IsEnabled = false;
                        backgroundSound.Stop();
                        Menu.Visibility = Visibility.Visible;
                        MainMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Menu.Visibility = Visibility.Collapsed;
                        HighScoreScreen.Visibility = Visibility.Collapsed;
                        backgroundSound.Play();
                        gameTimer.IsEnabled = true;
                    }
                    break;
            }
            if (direction != snakeDirection && gameTimer.IsEnabled)
            {
                MoveSnake();
                //PlaySounds(turnSound);
            }

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
        private void CollisionCheck()
        {
            Snakepart snakeHead = snakeparts.Last();
            if ((snakeHead.Position.X == snakeFood.Position.X) && (snakeHead.Position.Y == snakeFood.Position.Y))
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
        private void UpdateGameStatus()
        {
            Time.Text = gameTimer.Interval.TotalMilliseconds.ToString();
            Score.Text = currentScore.ToString();
        }


        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            Menu.Visibility = Visibility.Collapsed;
            StartNewGame();
            gameTimer.IsEnabled = true;
        }
        private void ShowHighscore_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Collapsed;
            HighScoreScreen.Visibility = Visibility.Visible;
        }
        private void HideHighScoreScreen_Click(object sender, RoutedEventArgs e)
        {
            HighScoreScreen.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
        }
        private void HideEndScreen_Click(object sender, RoutedEventArgs e)
        {
            HideEndScreen();
        }
        private void HideEndScreen()
        {
            EndGameScreen.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
        }


        private void SaveScore_Click(object sender, RoutedEventArgs e)
        {
            highscore.SaveScore(scoreName.Text, currentScore);
            ViewModel.SnakeScores.Add(new(scoreName.Text, currentScore));
            //Scores = new(Scores.OrderByDescending(score => score.Score));
            //HighScoreGrid.ItemsSource = Scores;
            HideEndScreen();
        }
        private void LoadState_Click(object sender, RoutedEventArgs e)
        {

            SaveState saveState = new();
            Clusterfuck? clusterfuck = saveState.ReadScore(SnakeSquareSize, foodBrush);
            if (clusterfuck != null)
            {
                ClearArena();
                snakeparts = clusterfuck.Snakeparts;
                snakeLength = clusterfuck.SnakeLength;
                snakeFood = clusterfuck.SnakeFood;
                gameTimer.Interval = clusterfuck.GameTimer.Interval;
                currentScore = clusterfuck.CurrentScore;
                snakeDirection = clusterfuck.SnakeDirection;
                DrawSnakeFood();
                DrawSnake();
                newGame = false;
                Menu.Visibility = Visibility.Collapsed;
                PlaySounds(respawnSound);
                gameTimer.IsEnabled = true;
            }
            //else
            //{
            //    newGame = true;
            //}
        }
        private void SaveState_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            Clusterfuck clusterfuck = new()
            {
                CurrentScore = currentScore,
                GameTimer = gameTimer,
                SnakeDirection = snakeDirection,
                SnakeFood = snakeFood,
                SnakeLength = snakeLength,
                Snakeparts = snakeparts
            };
            SaveState save = new();
            save.WriteScore(clusterfuck);
            gameTimer.Start();
        }


        private void PlaySounds(SoundPlayer sound)
        {
            sound.LoadAsync();
            backgroundSound.LoadAsync();
            Task.Run(() => { sound.PlaySync(); backgroundSound.PlayLooping(); });
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
            snakeFood = new()
            {
                UiElement = new Ellipse() { Width = SnakeSquareSize, Height = SnakeSquareSize, Fill = foodBrush },
                Position = GetFoodPosition()
            };
            Arena.Children.Add(snakeFood.UiElement);
            Canvas.SetTop(snakeFood.UiElement, snakeFood.Position.Y);
            Canvas.SetLeft(snakeFood.UiElement, snakeFood.Position.X);
        }
        private void ClearArena()
        {
            foreach (Snakepart snakepart in snakeparts)
            {
                if (snakepart.UiElement != null)
                {
                    Arena.Children.Remove(snakepart.UiElement);
                }
            }
            if (snakeFood != null)
            {
                Arena.Children.Remove(snakeFood.UiElement);
            }
            snakeparts.Clear();
            snakeFood = null;
            currentScore = 0;
        }


    }

    public class ViewModel
    {
        public ObservableCollection<SnakeScore> SnakeScores { get; set; }
        public ViewModel(List<SnakeScore> scores)
        {
            SnakeScores = new ObservableCollection<SnakeScore>(scores);
        }
    }
}
