using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Szakdolgozat.Model;

namespace Szakdolgozat
{
    /// <summary>
    /// Interaction logic for GameGrid.xaml
    /// </summary>
    public partial class GameGrid : UserControl
    {
        private static GameLogic gl;
        private static GameCell[,] GameGridCells { get; set; }

        private bool _pressed = false;

        public GameGrid()
        {
            InitializeComponent();

            gl = GameLogic.GetInstance();
            GameGridCells = new GameCell[gl.SIZE, gl.SIZE];

            NewGame();
        }

        public void NewGame()
        {
            for (int i = 0; i < gl.SIZE; i++)
            {
                for (int j = 0; j < gl.SIZE; j++)
                {
                    if (gl.GetValueAt(i, j) != 0)
                    {
                        AddNewGameCell(i, j);
                    }
                    else
                    {
                        RemoveOldGameCell(i, j);
                    }
                }
            }
        }

        public void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            Direction dir = Direction.UNDEFINED;
            base.OnKeyDown(e);
            if (e.Key == Key.Up)
                dir = Direction.UP;

            if (e.Key == Key.Down)
                dir = Direction.DOWN;

            if (e.Key == Key.Left)
                dir = Direction.LEFT;

            if (e.Key == Key.Right)
                dir = Direction.RIGHT;


            SetMovingDirAndControl(dir);
        }

        public void SetMovingDirAndControl(Direction dir)
        {
            GameLogic.GetInstance().MovingDir = dir;

            if (!_pressed)
            {
                _pressed = true;

                if (gl.CanMoveInDir())
                    ControlAll();
                else
                    _pressed = false;
            }
        }

        private void ControlAll()
        {
            gl.SlideAndMerge();
            AnimateSlideAndMerge();

            gl.ClearMerge();
            gl.ClearFrom();

            CreateRandomNew();
        }

        public void AnimateSlideAndMerge()
        {
            for (int i = 0; i < gl.SIZE; i++)
            {
                for (int j = 0; j < gl.SIZE; j++)
                {
                    Coordinates t = new Coordinates(gl.GetRightCoordx(i, j), gl.GetRightCoordy(i, j));
                    if (gl.GetValueAt(t.X, t.Y) != 0)
                    {
                        if (gl.GetFromAt(t.X, t.Y).Count == 1)
                        {
                            Animation(gl.GetFromAt(t.X, t.Y).ElementAt(0), t, true);
                        }
                        else if (gl.GetFromAt(t.X, t.Y).Count == 2)
                        {
                            Animation(gl.GetFromAt(t.X, t.Y).ElementAt(0), t, false);
                            Animation(gl.GetFromAt(t.X, t.Y).ElementAt(1), t, true);
                        }
                    }
                }
            }
        }

        public void Animation(Coordinates from, Coordinates to, bool last)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation slide = new DoubleAnimation();

            slide.Duration = new Duration(TimeSpan.FromMilliseconds(150));
            if (gl.MovingDir == Direction.UP || gl.MovingDir == Direction.DOWN)
            {
                slide.To = to.Y * 125;
                slide.From = from.Y * 125;
                Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.(TranslateTransform.Y)"));
            }
            else
            {
                slide.To = to.X * 125;
                slide.From = from.X * 125;
                Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
            }

            Storyboard.SetTarget(slide, GameGridCells[from.X, from.Y]);
            
            sb.Completed += (sender, e) => AnimationEnds(from, to, last);

            sb.Children.Add(slide);
            sb.Begin();
        }

        public void AnimationEnds(Coordinates from, Coordinates to, bool last)
        {
            RemoveOldGameCell(from.X, from.Y);

            if (last)
            {
                AddNewGameCell(to.X, to.Y);
            }
        }

        public void AddNewGameCell(int x, int y)
        {
            RemoveOldGameCell(x, y);
            GameGridCells[x, y] = new GameCell();
            GameGridCells[x, y].Value.Text = gl.GetValueAt(x, y).ToString();
            GameGridCells[x, y].Cell.Background = new SolidColorBrush(Colors.Gold);
            GameGridCells[x, y].RenderTransformOrigin = new Point(0.5, 0.5);
            GameGridCells[x, y].RenderTransform = new TranslateTransform(x * 125, y * 125);
            GameCanvas.Children.Add(GameGridCells[x, y]);
        }

        public void RemoveOldGameCell(int x, int y)
        {
            GameCanvas.Children.Remove(GameGridCells[x, y]);
            GameGridCells[x, y] = null;
        }

        public void CreateRandomNew()
        {
            BackgroundWorker barInvoker = new BackgroundWorker();
            barInvoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(180));
            };

            barInvoker.RunWorkerAsync();
            
            barInvoker.RunWorkerCompleted += delegate
            {
                gl.GenerateRandomNew();
                for (int i = 0; i < gl.SIZE; i++)
                {
                    for (int j = 0; j < gl.SIZE; j++)
                    {
                        if (gl.GetCreatedAt(i, j))
                        {
                            AddNewGameCell(i, j);
                            gl.SetCreatedFalseAt(i, j);
                        }
                    }
                }
                _pressed = false;
            };
        }
    }
}
