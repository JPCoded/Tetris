#region

using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualBasic;

#endregion

namespace Tetris
{
    internal sealed class Board : IBoard
    {
        public delegate void FullRowsEventHandler(object sender, FullRowsEventArgs e);

        public delegate void GameOverEventHandler(object sender, EventArgs e);

        public delegate void GotNewBlockEventHandler(object sender, NewBlockEventArgs e);

        private int _nextBlock;


        public Board(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Cells = new Dictionary<string, Cell>();
            for (var row = 0; row <= Rows - 1; row++)
            {
                for (var column = 0; column <= Columns - 1; column++)
                {
                    Cells.Add(row + "," + column, new Cell(row, column));
                }
            }
        }

        public int Rows { get; }

        public int Columns { get; }

        public Dictionary<string, Cell> Cells { get; }

        public Block FallingBlock { get; set; }

        public Color Block1Color { get; set; }

        public Color Block2Color { get; set; }

        public Color Block3Color { get; set; }

        public Color Block4Color { get; set; }

        public Color Block5Color { get; set; }

        public Color Block6Color { get; set; }

        public Color Block7Color { get; set; }

        public event FullRowsEventHandler FullRows;

        public event GameOverEventHandler GameOver;

        public event GotNewBlockEventHandler GotNewBlock;

        public bool Rotate()
        {
            if (CanRotate())
            {
                FallingBlock.OffsetRotation();
                return true;
            }
            return false;
        }

        public bool CanRotate()
        {
            if (FallingBlock != null)
            {
                var nextRotation = FallingBlock.NextRotationMatrix;
                for (var row = 0; row <= 3; row++)
                {
                    for (var column = 0; column <= 3; column++)
                    {
                        if (nextRotation[row].Substring(column, 1).Equals("1"))
                        {
                            var pt = BlockToBoard(new CellPoint(row, column));

                            if (pt.Column < 0 || pt.Column >= Columns || pt.Row < 0 || pt.Row >= Rows ||
                                Cells[new CellPoint(pt.Row, pt.Column).ToString()].Fixed)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public bool MoveLeft()
        {
            if (CanMoveLeft())
            {
                FallingBlock.X -= 1;
                return true;
            }
            return false;
        }

        public bool CanMoveLeft()
        {
            if (FallingBlock != null)
            {
                for (var row = 0; row <= 3; row++)
                {
                    for (var column = 0; column <= 3; column++)
                    {
                        if (FallingBlock.FilledCell(column, row))
                        {
                            var pt = BlockToBoard(new CellPoint(row, column));

                            if (pt.Column.Equals(0) || Cells[new CellPoint(pt.Row, pt.Column - 1).ToString()].Fixed)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public bool CanMoveRight()
        {
            if (FallingBlock != null)
            {
                for (var row = 0; row <= 3; row++)
                {
                    for (var column = 3; column >= 0; column += -1)
                    {
                        if (FallingBlock.FilledCell(column, row))
                        {
                            var pt = BlockToBoard(new CellPoint(row, column));
                            if (pt.Column.Equals(Columns - 1) ||
                                Cells[new CellPoint(pt.Row, pt.Column + 1).ToString()].Fixed)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public bool MoveRight()
        {
            if (CanMoveRight())
            {
                FallingBlock.X += 1;
                return true;
            }
            return false;
        }

        public void NewBlock()
        {
            FallingBlock = _nextBlock.Equals(0) ? new Block(GetRandomNumber(1, 7)) : new Block(_nextBlock);

            switch (FallingBlock.Type)
            {
                case 1:
                    FallingBlock.Color = Block1Color;
                    break;
                case 2:
                    FallingBlock.Color = Block2Color;
                    break;
                case 3:
                    FallingBlock.Color = Block3Color;
                    break;
                case 4:
                    FallingBlock.Color = Block4Color;
                    break;
                case 5:
                    FallingBlock.Color = Block5Color;
                    break;
                case 6:
                    FallingBlock.Color = Block6Color;
                    break;
                case 7:
                    FallingBlock.Color = Block7Color;
                    break;
            }
            _nextBlock = GetRandomNumber(1, 7);
            FallingBlock.X = (Columns - 4) / 2;
            FallingBlock.Y = 0;
            GotNewBlock?.Invoke(this, new NewBlockEventArgs(FallingBlock.Type, _nextBlock));
        }

        public void CheckBlock()
        {
            if (FallingBlock != null)
            {
                dynamic overlapBlock = false;

                for (var row = 0; row <= 3; row++)
                {
                    for (var column = 0; column <= 3; column++)
                    {
                        if (FallingBlock.FilledCell(column, row))
                        {
                            var pt = BlockToBoard(new CellPoint(row, column));
                            if (Cells[pt.ToString()].Fixed)
                            {
                                overlapBlock = true;
                                break;
                            }
                        }
                    }
                    if (overlapBlock)
                    {
                        break;
                    }
                }
                if (overlapBlock)
                {
                    GameOver?.Invoke(this, new EventArgs());
                }
                else
                {
                    dynamic fixBlock = false;

                    for (var column = 0; column <= 3; column++)
                    {
                        for (var row = 3; row >= 0; row += -1)
                        {
                            if (FallingBlock.FilledCell(column, row))
                            {
                                var pt = BlockToBoard(new CellPoint(row, column));
                                if (pt.Row.Equals(Rows - 1) ||
                                    Cells[new CellPoint(pt.Row + 1, pt.Column).ToString()].Fixed)
                                {
                                    fixBlock = true;
                                }
                                break;
                            }
                        }
                        if (fixBlock)
                        {
                            break;
                        }
                    }

                    if (fixBlock)
                    {
                        for (var row = 0; row <= 3; row++)
                        {
                            for (var column = 0; column <= 3; column++)
                            {
                                if (FallingBlock.FilledCell(column, row))
                                {
                                    var pt = BlockToBoard(new CellPoint(row, column));
                                    Cells[pt.ToString()].Fixed = true;
                                    Cells[pt.ToString()].Color = FallingBlock.Color;
                                }
                            }
                        }
                        FallingBlock = null;
                        CheckFullRows();
                    }
                }
            }
        }

        public void CheckFullRows()
        {
            dynamic fullRows = new List<int>();

            for (var row = Rows - 1; row >= 0; row += -1)
            {
                dynamic fullRow = true;

                for (var column = 0; column <= Columns - 1; column++)
                {
                    if (!Cells[row + "," + column].Fixed)
                    {
                        fullRow = false;
                        break;
                    }
                }
                if (fullRow)
                {
                    fullRows.Add(row);
                }
            }
            if (fullRows.Count > 0)
            {
                foreach (int row in fullRows)
                {
                    DeleteRow(row);
                }

                FullRows?.Invoke(this, new FullRowsEventArgs(fullRows.Count));
            }
        }

        public void DeleteRow(int row)
        {
            for (var r = row; r >= 1; r += -1)
            {
                for (var col = 0; col <= Columns - 1; col++)
                {
                    Cells[r + "," + col].Fixed = Cells[r - 1 + "," + col].Fixed;
                    Cells[r + "," + col].Color = Cells[r - 1 + "," + col].Color;
                }
            }
            for (var col = 0; col <= Columns - 1; col++)
            {
                Cells["0," + col].Fixed = false;
            }
        }

        public CellPoint BlockToBoard(CellPoint p) => new CellPoint(p.Row + FallingBlock.Y, p.Column + FallingBlock.X);

        public CellPoint BoardToBlock(CellPoint p) => new CellPoint(p.Row - FallingBlock.Y, p.Column - FallingBlock.X);

        public Color GetCellColor(CellPoint p)
        {
            var output = Color.Transparent;

            if (Cells[p.Row + "," + p.Column].Fixed)
            {
                output = Cells[p.Row + "," + p.Column].Color;
            }
            else
            {
                if (FallingBlock != null && CellIsInsideBlock(p.Row, p.Column))
                {
                    var pt = BoardToBlock(p);

                    if (FallingBlock.FilledCell(pt.Column, pt.Row))
                    {
                        output = FallingBlock.Color;
                    }
                }
            }

            return output;
        }

        public bool CellIsInsideBlock(int row, int column) => row >= FallingBlock.Y && row <= FallingBlock.Y + 3 &&
                                                              column >= FallingBlock.X && column <= FallingBlock.X + 3;

        private static int GetRandomNumber(int lowerbound, int upperbound) =>
            Convert.ToInt32(Math.Floor((upperbound - lowerbound + 1) * VBMath.Rnd())) + lowerbound;
    }
}