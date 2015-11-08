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

        bool pressed = false;

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

            if (!pressed)
            {
                pressed = true;
                base.OnKeyDown(e);
                if (e.Key == Key.Up)
                    GameLogic.GetInstance().MovingDir = Direction.UP;

                if (e.Key == Key.Down)
                    GameLogic.GetInstance().MovingDir = Direction.DOWN;

                if (e.Key == Key.Left)
                    GameLogic.GetInstance().MovingDir = Direction.LEFT;

                if (e.Key == Key.Right)
                    GameLogic.GetInstance().MovingDir = Direction.RIGHT;

                if (gl.CanMoveInDir())
                    ControlAll();
                else
                    pressed = false;
            }
        }

        private void ControlAll()
        {
            gl.SlideAndMerge();
            AnimateSlideAndMerge();

            gl.ClearMerge();
            gl.ClearFrom();

            CreateRandomNew();

            //gl.CheckGameState();
        }

        public void AnimateSlideAndMerge()
        {
            for (int i = 0; i < gl.SIZE; i++)
            {
                for (int j = 0; j < gl.SIZE; j++)
                {
                    Coordinates t = new Coordinates(gl.GetRightCoordx(i, j), gl.GetRightCoordy(i, j));
                    //ha olyan mezőbe ütközünk amiben nem 0 van és van honnan jött
                    //akkor végigmegyünk a honnan mezőkön és mindegyiket meganimáljuk
                    //és átadjuk a first paramétert ami azért kell, hogy abban az esetben
                    //be tudjuk állítani a to mezőt a végén
                    if (gl.GetValueAt(t.X, t.Y) != 0 && gl.GetFromAt(t.X, t.Y).Count == 1)
                    {
                        Animation(gl.GetFromAt(t.X, t.Y).ElementAt(0), t, true);
                    }
                    else if (gl.GetValueAt(t.X, t.Y) != 0 && gl.GetFromAt(t.X, t.Y).Count == 2)
                    {
                        Animation(gl.GetFromAt(t.X, t.Y).ElementAt(0), t, false);
                        Animation(gl.GetFromAt(t.X, t.Y).ElementAt(1), t, true);
                    }
                }
            }
        }

        public void Animation(Coordinates from, Coordinates to, bool last)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation slide = new DoubleAnimation();

            // Beállítjuk az animáció paramétereit
            if (gl.MovingDir == Direction.UP || gl.MovingDir == Direction.DOWN)
            {
                slide.To = to.Y * 125;
                slide.From = from.Y * 125;
            }
            else
            {
                slide.To = to.X * 125;
                slide.From = from.X * 125;
            }
            slide.Duration = new Duration(TimeSpan.FromMilliseconds(150));
            if (gl.MovingDir == Direction.UP || gl.MovingDir == Direction.DOWN)
                Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.(TranslateTransform.Y)"));
            else
                Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.(TranslateTransform.X)"));

            Storyboard.SetTarget(slide, GameGridCells[from.X, from.Y]);

            // Elindítjuk az animációkat és átadjuk a paramétereket
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
            //megvárjuk amígy az összes animációs esemény lezajlik
            BackgroundWorker barInvoker = new BackgroundWorker();
            barInvoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(180));
            };

            barInvoker.RunWorkerAsync();

            //majd a várás után generálunk egy új kezdőértéket
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
                pressed = false;
            };
        }
    }
}
