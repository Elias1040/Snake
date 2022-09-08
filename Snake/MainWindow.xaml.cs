using Snake.Model;
using System;
using System.Collections.Generic;
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

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int SnakeSquareSize = 20;
        private List<Snakepart> snakeparts;

        public MainWindow()
        {
            InitializeComponent();
            snakeparts = new List<Snakepart>();
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawArena();
            DrawSnake();
        }

        private void DrawSnake()
        {
            foreach (Snakepart snakepart in snakeparts)
            {
                if (snakepart == null)
                {
                    snakepart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakepart.IsHead ? Brushes.Red : Brushes.LightYellow)
                    };
                    Arena.Children.Add(snakepart.UiElement);
                    Canvas.SetTop(snakepart.UiElement, snakepart.Position.Y);
                    Canvas.SetLeft(snakepart.UiElement, snakepart.Position.X);
                }
            }
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
                    Fill = isOdd ? Brushes.YellowGreen : Brushes.Green,
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
    }
}
