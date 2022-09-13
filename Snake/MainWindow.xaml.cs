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

        #region Fields
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
        private Highscore highscore;
        private bool Muted;
        #endregion

        #region Constants
        const int SnakeSquareSize = 20;
        const int SnakeStartLength = 3;
        const int SnakeStartSpeed = 200;
        const int SnakeSpeedThreshold = 100; 
        #endregion
        public ViewModel ViewModel { get; set; }

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
            Muted = false;
            backgroundSound = new(@"C:\Users\elias\source\repos\Snake\Snake\BackgroundMusic.wav");
            respawnSound = new(@"C:\Users\elias\source\repos\Snake\Snake\ResetSound.wav");
            deadSound = new(@"C:\Users\elias\source\repos\Snake\Snake\DeadSound.wav");
            turnSound = new(@"C:\Users\elias\source\repos\Snake\Snake\TurnSound.wav");
            eatSound = new(@"C:\Users\elias\source\repos\Snake\Snake\EatSound.wav");
            gameTimer.Tick += GameTimer_Tick;
        }
        public enum SnakeDirection { Left, Right, Up, Down }

        /// <summary>
        /// Calls DrawArena
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawArena();
        }

        /// <summary>
        /// Moves snake every tick and updates game status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
            UpdateGameStatus();
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
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
                PlaySound(respawnSound, backgroundSound);
                DrawSnake();
                DrawSnakeFood();
            }
        }

        /// <summary>
        /// Moves the snake in a given direction
        /// </summary>
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

        /// <summary>
        /// Checks for collisions with food, body and walls
        /// </summary>
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



        /// <summary>
        /// Ends the game
        /// </summary>
        private void EndGame()
        {
            gameTimer.IsEnabled = false;
            PlaySound(deadSound);
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

        /// <summary>
        /// Gets the food position
        /// </summary>
        /// <returns>A Point with food position</returns>
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

        /// <summary>
        /// Removes the eaten food and adds to score
        /// </summary>
        private void EatSnakeFood()
        {
            snakeLength++;
            if (currentScore >= 20)
            {
                currentScore += (int)(currentScore * 0.1);
            }
            else
            {
                currentScore++;
            }
            int interval = Math.Max(SnakeSpeedThreshold, (int)gameTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTimer.Interval = TimeSpan.FromMilliseconds(interval);
            PlaySound(eatSound, backgroundSound);
            Arena.Children.Remove(snakeFood.UiElement);
            DrawSnakeFood();
            UpdateGameStatus();
        }

        /// <summary>
        /// Hides all screen menus
        /// </summary>
        private void HideAllScreens()
        {
            Menu.Visibility = Visibility.Hidden;
            HighScoreScreen.Visibility = Visibility.Hidden;
            EndGameScreen.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Hudes the end screen menu
        /// </summary>
        private void HideEndScreen()
        {
            EndGameScreen.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
        }


        #region Events
        /// <summary>
        /// Changes the direction, pauses the game or restarts the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    HideAllScreens();
                    StartNewGame();
                    gameTimer.IsEnabled = true;
                    break;
                case Key.Escape:
                    if (gameTimer.IsEnabled)
                    {
                        gameTimer.IsEnabled = false;
                        if (!Muted)
                        {
                            backgroundSound.Stop();
                        }
                        Menu.Visibility = Visibility.Visible;
                        MainMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Menu.Visibility = Visibility.Collapsed;
                        HighScoreScreen.Visibility = Visibility.Collapsed;
                        if (!Muted)
                        {
                            backgroundSound.Play();
                        }
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

        /// <summary>
        /// Calls StartNewGame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            Menu.Visibility = Visibility.Collapsed;
            StartNewGame();
            gameTimer.IsEnabled = true;
        }

        /// <summary>
        /// Shows the high score menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowHighscore_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Collapsed;
            HighScoreScreen.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the high score menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideHighScoreScreen_Click(object sender, RoutedEventArgs e)
        {
            HighScoreScreen.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Calls HideEndScreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideEndScreen_Click(object sender, RoutedEventArgs e)
        {
            HideEndScreen();
        }

        /// <summary>
        /// Saves the score
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveScore_Click(object sender, RoutedEventArgs e)
        {
            highscore.SaveScore(scoreName.Text, currentScore);
            ViewModel.SnakeScores.Add(new(scoreName.Text, currentScore));
            HideEndScreen();
        }

        /// <summary>
        /// Loads a save file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadState_Click(object sender, RoutedEventArgs e)
        {

            SaveState saveState = new();
            Clusterfuck? clusterfuck = saveState.ReadState(SnakeSquareSize, foodBrush);
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
                PlaySound(respawnSound, backgroundSound);
                gameTimer.IsEnabled = true;
            }
            //else
            //{
            //    newGame = true;
            //}
        }

        /// <summary>
        /// Saves state to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveState_Click(object sender, RoutedEventArgs e)
        {
            if (!newGame)
            {
                bool gameState = gameTimer.IsEnabled;
                gameTimer.IsEnabled = false;
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
                save.WriteState(clusterfuck);
                if (gameState)
                {
                    gameTimer.IsEnabled = true;
                }
            }
        } 
   
        
        /// <summary>
        /// Mutes the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => backgroundSound.Stop());
            Muted = true;
            Mute.Visibility = Visibility.Visible;
            Unmute.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Unmutes the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Unmute_Click(object sender, RoutedEventArgs e)
        {
            if (gameTimer.IsEnabled)
            {
                backgroundSound.Play();
            }
            Muted = false;
            Unmute.Visibility = Visibility.Visible;
            Mute.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Clears the high score
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearHighScore_Click(object sender, RoutedEventArgs e)
        {
            highscore.ClearScores();
            ViewModel.SnakeScores.Clear();
        }
        #endregion

        /// <summary>
        /// Plays one sound
        /// </summary>
        /// <param name="sound"></param>
        private void PlaySound(SoundPlayer sound)
        {
            if (!Muted)
            {
                sound.Play();
            }
        }

        /// <summary>
        /// Plays one sound and backgroundsound
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="background"></param>
        private void PlaySound(SoundPlayer sound, SoundPlayer background)
        {
            if (!Muted)
            {
                Task.Run(() =>
                {
                    sound.PlaySync();
                    if (gameTimer.IsEnabled && !Muted)
                    {
                        background.PlayLooping();
                    }
                });
            }
        }


        #region Canvas
        /// <summary>
        /// Draws the arena on canvas
        /// </summary>
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

        /// <summary>
        /// Draws the snake on canvas
        /// </summary>
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

        /// <summary>
        /// Draws the food on canvas
        /// </summary>
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

        /// <summary>
        /// Clears the arena for snake and food
        /// </summary>
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
        #endregion



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
