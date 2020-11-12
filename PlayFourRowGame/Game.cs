using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayFourRowGame
{
    /// <summary>
    /// this class represent the game logic/model behind the display (gui).
    /// here is the real game but with simple implements.
    /// </summary>
    public class Game
    {
        private const int NumberOfRows = 6; // const 6 rows.
        private const int NumberOfColumns = 7; // const 7 columns.
        private const int NumberOfTieDisk = 21; //const 21 disk per tie game.

        private bool[] _bonusForAllColumns; // array that will save all player column visit for future bonus.

        private int _diskCounter = 0; // player disk counter.

        private readonly bool _isStartFirst; // rue when player start first.

        public bool IsWin { get; private set; } = false; // winner flag.

        public bool IsTie { get; private set; } = false; // tie flag.

        public char[,] GameBoard { get; private set; } // real game board.

        public char DiskColor { get; set; } // char disk color.

        public List<Point> WinningRowDisks { get; private set; } // array of winning row points.

        public char OpponentDiskColor { get; set; } // char opponent disk color.

        public bool IsMyTurn { get; set; } // my turn flag.

        public int Score { get; set; } // score counter.

        //c'tor.
        public Game(char playerDiskColor, char opponentDiskColor, bool isStatFirst)
        {
            _isStartFirst = isStatFirst;
            GameBoard = new char[NumberOfRows, NumberOfColumns];

            for (int i = 0; i < NumberOfRows; i++)
                for (int j = 0; j < NumberOfColumns; j++)
                    GameBoard[i, j] = 'N';

            _bonusForAllColumns = Enumerable.Repeat(false, 7).ToArray();

            WinningRowDisks = new List<Point>();

            DiskColor = playerDiskColor;
            OpponentDiskColor = opponentDiskColor;

        }

        public bool Move(int columnIndex)
        {
            if (IsLegalMove(columnIndex))
            {
                for (int rowIndex = NumberOfRows - 1; rowIndex >= 0; rowIndex--)
                {
                    if (GameBoard[rowIndex, columnIndex] == 'N')
                    {
                        GameBoard[rowIndex, columnIndex] = DiskColor;
                        _diskCounter++;
                        IsMyTurn = !IsMyTurn;

                        if (!_bonusForAllColumns[columnIndex])
                            _bonusForAllColumns[columnIndex] = true;

                        if (!IsWinGame())
                            IsTie = IsTieGame();

                        CalculateScore();
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsWinGame()
        {
            // horizontalCheck 
            for (int i = 0; i < NumberOfRows ; i++)
            {
                for (int j = 0; j < NumberOfColumns - 3; j++)
                {
                    if (GameBoard[i, j] == DiskColor && GameBoard[i, j + 1] == DiskColor &&
                        GameBoard[i, j + 2] == DiskColor && GameBoard[i, j + 3] == DiskColor)
                    {
                        WinningRowDisks.Add(new Point(i, j));
                        WinningRowDisks.Add(new Point(i, j + 1));
                        WinningRowDisks.Add(new Point(i, j + 2));
                        WinningRowDisks.Add(new Point(i, j + 3));
                        IsWin = true;
                        return true;
                    }
                }
            }

            // verticalCheck
            for (int i = 0; i < NumberOfRows - 3; i++)
            {
                for (int j = 0; j < NumberOfColumns; j++)
                {
                    if (GameBoard[i, j] == DiskColor && GameBoard[i + 1, j] == DiskColor &&
                        GameBoard[i + 2, j] == DiskColor && GameBoard[i + 3, j] == DiskColor)
                    {
                        WinningRowDisks.Add(new Point(i, j));
                        WinningRowDisks.Add(new Point(i + 1, j));
                        WinningRowDisks.Add(new Point(i + 2, j));
                        WinningRowDisks.Add(new Point(i + 3, j));
                        IsWin = true;
                        return true;
                    }
                }
            }

            // ascendingDiagonalCheck 
            for (int i = 3; i < NumberOfColumns; i++)
            {
                for (int j = 0; j < NumberOfRows - 3; j++)
                {
                    if (GameBoard[j, i] == DiskColor && GameBoard[j + 1, i - 1] == DiskColor &&
                        GameBoard[j + 2, i - 2] == DiskColor && GameBoard[j + 3, i - 3] == DiskColor)
                    {
                        WinningRowDisks.Add(new Point(j, i));
                        WinningRowDisks.Add(new Point(j + 1, i - 1));
                        WinningRowDisks.Add(new Point(j + 2, i - 2));
                        WinningRowDisks.Add(new Point(j + 3, i - 3));
                        IsWin = true;
                        return true;
                    }
                }
            }

            // descendingDiagonalCheck
            for (int i = 3; i < NumberOfColumns; i++)
            {
                for (int j = 3; j < NumberOfRows; j++)
                {
                    if (GameBoard[j, i] == DiskColor && GameBoard[j - 1, i - 1] == DiskColor &&
                        GameBoard[j - 2, i - 2] == DiskColor && GameBoard[j - 3, i - 3] == DiskColor)
                    {
                        WinningRowDisks.Add(new Point(j, i));
                        WinningRowDisks.Add(new Point(j - 1, i - 1));
                        WinningRowDisks.Add(new Point(j - 2, i - 2));
                        WinningRowDisks.Add(new Point(j - 3, i - 3));
                        IsWin = true;
                        return true;
                    }
                }
            }
            IsWin = false;
            return false;
        }

        private bool IsTieGame()
        {
            return _diskCounter == NumberOfTieDisk && !_isStartFirst;
        }

        public bool IsEmptyCell(int row, int column)
        {
            return GameBoard[row, column] == 'N';
        }

        public void SetEmptyCell(int row, int column)
        {
            if (IsMyTurn)
                GameBoard[row, column] = DiskColor;
            else
                GameBoard[row, column] = OpponentDiskColor;
        }

        private void CalculateScore()
        {
            if (IsWin)
                Score = 1000;
            else
                Score = _diskCounter * 10;

            Func<bool, bool> ifTrue = x => x;
            var count = _bonusForAllColumns.Where(ifTrue).ToList().Count();

            if (count == NumberOfColumns)
                Score += 100;
        }

        private bool IsLegalMove(int columnIndex)
        {
            for (int rowIndex = 0; rowIndex < NumberOfRows; rowIndex++)
            {
                if (GameBoard[rowIndex, columnIndex] == 'N')
                {
                    return true;
                }
            }
            return false;
        }
    }
}
