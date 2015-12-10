using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdolgozat.Model
{
    public enum Direction
    {
        UP, DOWN, LEFT, RIGHT, UNDEFINED
    }

    public enum GameState
    {
        WIN, GAME, LOSE
    }

    class GameLogic
    {
        private static GameLogic gl = null;

        public int SIZE { get { return 4; } }

        public Cell[,] GameTable { get; set; }

        public Direction MovingDir { get; set; }

        public int Score { get; set; }


        private GameLogic()
        {
            GameTable = new Cell[SIZE, SIZE];
            Score = 0;
            MovingDir = Direction.UP;

            CreateStartingState();
        }

        public static GameLogic GetInstance()
        {
            if (gl == null)
            {
                gl = new GameLogic();
            }
            return gl;
        }

        public void NewGame()
        {
            CreateStartingState();
        }
        public bool CanMoveInDir()
        {
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 1; j < SIZE; j++)
                {
                    int newi = GetRightCoordx(i, j);
                    int newj = GetRightCoordy(i, j);
                    int newi1 = GetRightCoordx(i, j - 1);
                    int newj1 = GetRightCoordy(i, j - 1);

                    if (GameTable[newi, newj].Value != 0)
                    {
                        if (GameTable[newi1, newj1].Value == 0 ||
                        GameTable[newi1, newj1].Value == GameTable[newi, newj].Value)
                            return true;
                    }
                }
            }
            return false;
        }

        public GameState CheckGameState()
        {
            for(int i = 0; i < SIZE; i++)
            {
                for(int j = 0; j < SIZE; j++)
                {
                    if(GameTable[i, j].Value == 2048)
                    {
                        return GameState.WIN;
                    }
                    for(int k = -1; k <= 1; k++)
                    {
                        for(int l = -1; l <= 1; l++)
                        {
                            if(!(k != -1 && l != -1 ||
                                k != -1 && l != 1 ||
                                k != 1 && l != -1 ||
                                k != 1 && l != 1))
                            {
                                if (i + k >= 0 && i + k < SIZE && j + l >= 0 && j + l < SIZE)
                                {
                                    if(GameTable[i+k, j+l].Value == 0 ||
                                        GameTable[i + k, j + l].Value == GameTable[i, j].Value)
                                    {
                                        return GameState.GAME;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return GameState.LOSE;
        }

        private void CreateStartingState()
        {

            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    GameTable[i, j] = new Cell();
                    GameTable[i, j].Value = 0;
                }
            }
            int starter = GetRandomStarter();
            Coordinates coords = GetRandomCoordinates();
            GameTable[coords.X, coords.Y].Value = starter;

            int starter2 = GetRandomStarter();
            Coordinates coords2 = GetRandomCoordinates();
            while (coords2.Equals(coords))
            {
                coords2 = GetRandomCoordinates();
            }
            GameTable[coords2.X, coords2.Y].Value = starter2;
        }

        private int GetRandomStarter()
        {
            Random r = new Random();
            return r.Next() % 10 == 0 ? 4 : 2;
        }

        private Coordinates GetRandomCoordinates()
        {
            Random r = new Random();
            Coordinates c = new Coordinates(r.Next() % SIZE, r.Next() % SIZE);
            return c;
        }

        public void SlideAndMerge()
        {
            for (int i = 0; i < gl.SIZE; i++)
            {
                int j = 0;
                while (j < SIZE)
                {
                    if (GameTable[GetRightCoordx(i, j), GetRightCoordy(i, j)].Value != 0)
                    {
                        LookAndMergeOrSlide(i, j);
                    }
                    j++;
                }
            }
        }

        public int GetRightCoordx(int x, int y)
        {
            if (MovingDir == Direction.UP)
            {
                return x;
            }
            if(MovingDir == Direction.DOWN)
            {
                return (SIZE - 1) - x;
            }
            if (MovingDir == Direction.RIGHT)
            {
                return (SIZE - 1) - y;
            }
            else //MovingDir == Direction.LEFT
            {
                return y;
            }
        }

        public int GetRightCoordy(int x, int y)
        {
            if (MovingDir == Direction.UP)
            {
                return y;
            }
            if (MovingDir == Direction.DOWN)
            {
                return (SIZE - 1) - y;
            }
            if (MovingDir == Direction.RIGHT)
            {
                return x;
            }
            else //MovingDir == Direction.LEFT
            {
                return (SIZE - 1) - x;
            }
        }

        private void LookAndMergeOrSlide(int x, int y)
        {
            int j = y - 1;
            
            if (j >= 0)
            {
                while (j >= 0)
                {                                            
                    
                    if (GameTable[GetRightCoordx(x, y), GetRightCoordy(x, y)].Value == GameTable[GetRightCoordx(x, j), GetRightCoordy(x, j)].Value
                            && !GameTable[GetRightCoordx(x, j), GetRightCoordy(x, j)].Merged)
                    {
                        break;
                    }
                    else
                    {
                        if (GameTable[GetRightCoordx(x, j), GetRightCoordy(x, j)].Value == 0)
                        {
                            if (j == 0)
                            {
                                break;
                            }
                            else
                            {
                                j--;
                            }
                        }
                        else
                        {
                            j++;
                            break;
                        }
                    }
                }
                if (j != y)
                {
                    int xjnew = GetRightCoordx(x, j);
                    int xynew = GetRightCoordx(x, y);
                    int jnew = GetRightCoordy(x, j);
                    int ynew = GetRightCoordy(x, y);

                    if (GameTable[xynew, ynew].Value == GameTable[xjnew, jnew].Value)
                    {
                        GameTable[xjnew, jnew].From.Add(new Coordinates(xynew, ynew));
                        GameTable[xjnew, jnew].Merged = true;
                        GameTable[xjnew, jnew].Value = 2 * GameTable[xynew, ynew].Value;
                        GameTable[xynew, ynew] = new Cell();
                    }
                    else
                    {
                        GameTable[xjnew, jnew].From.Add(new Coordinates(xynew, ynew));
                        GameTable[xjnew, jnew].Merged = false;
                        GameTable[xjnew, jnew].Value = GameTable[xynew, ynew].Value;
                        GameTable[xynew, ynew] = new Cell();
                    }
                }
                else
                {
                    int xynew = GetRightCoordx(x, y);
                    int ynew = GetRightCoordy(x, y);
                    GameTable[xynew, ynew].From.Add(new Coordinates(xynew, ynew));
                }
            }
        }

        public void ClearMerge()
        {
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    if (GameTable[i, j].Merged == true)
                    {
                        GameTable[i, j].Merged = false;
                    }
                }
            }
        }

        public void ClearFrom()
        {
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    GameTable[i, j].From = null;
                    GameTable[i, j].From = new List<Coordinates>();
                }
            }
        }

        public void ClearRoundScore()
        {

        }

        public void GenerateRandomNew()
        {
            int starter = GetRandomStarter();
            Coordinates coords = GetRandomCoordinates();
            while (GameTable[coords.X, coords.Y].Value != 0)
            {
                coords = GetRandomCoordinates();
            }
            GameTable[coords.X, coords.Y].Value = starter;
            GameTable[coords.X, coords.Y].Created = true;
        }

        public int GetValueAt(int x, int y)
        {
            return GameTable[x, y].Value;
        }

        public bool GetCreatedAt(int x, int y)
        {
            return GameTable[x, y].Created;
        }

        public List<Coordinates> GetFromAt(int x, int y)
        {
            return GameTable[x, y].From;
        }

        public void SetCreatedFalseAt(int x, int y)
        {
            GameTable[x, y].Created = false;
        }

    }
}
