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

            //Set the number of rows
            Rows = rows;
            //Set the number of columns
            Columns = columns;
            //Initialize the Cell collection
            Cells = new Dictionary<string, Cell>();
            for (var row = 0; row <= Rows - 1; row++)
            {
                for (var column = 0; column <= Columns - 1; column++)
                {
                    Cells.Add(row + "," + column, new Cell(row, column));
                }
            }
        }

        //This class represents the game board. Many of the game logic is implemented here.
        public int Rows { get; }

        //Defines the number of rows
        public int Columns { get; }

        //Defines the number of columns
        public Dictionary<string, Cell> Cells { get; }

        //Single-cell collection
        public Block FallingBlock { get; set; }

        //The current falling block, if there's any
        public Color Block1Color { get; set; }

        //Allows to customize the color of the type 1 blocks
        public Color Block2Color { get; set; }

        //Allows to customize the color of the type 2 blocks
        public Color Block3Color { get; set; }

        //Allows to customize the color of the type 3 blocks
        public Color Block4Color { get; set; }

        //Allows to customize the color of the type 4 blocks
        public Color Block5Color { get; set; }

        //Allows to customize the color of the type 5 blocks
        public Color Block6Color { get; set; }

        //Allows to customize the color of the type 6 blocks
        public Color Block7Color { get; set; }

        public event FullRowsEventHandler FullRows;

        //Fires when the user completes any number of full rows. The completed rows will disappear.
        public event GameOverEventHandler GameOver;

        //Fires when a block reaches the top of the game board.
        public event GotNewBlockEventHandler GotNewBlock;

        private static int GetRandomNumber(int lowerbound, int upperbound) => Convert.ToInt32(
                                                                           Math.Floor((upperbound - lowerbound + 1) *
                                                                                      VBMath.Rnd())) + lowerbound;

        public bool Rotate()
        {
            //Tries to rotate the current falling block, returning True if the rotation has been done
            if (CanRotate())
            {
                FallingBlock.OffsetRotation();
                return true;
            }
            return false;
        }

        public bool CanRotate()
        {
            //Ensures that the current falling block can rotate. It can rotate if, once rotated, keeps inside board margins and doesn't overlap with existing fixed cells.
            if (FallingBlock != null)
            {
                //Get the 4x4 matrix corresponding to the next rotation of the block
                var nextRotation = FallingBlock.NextRotationMatrix;
                //Check each filled cell of the block. It must be inside board margins and not overlap with fixed cells.
                for (var row = 0; row <= 3; row++)
                {
                    for (var column = 0; column <= 3; column++)
                    {
                        if (nextRotation[row].Substring(column, 1).Equals("1"))
                        {
                            //This is a filled cell.
                            //Translate its coordinates to board-coordinates.
                            var pt = BlockToBoard(new CellPoint(row, column));
                            //Check if the cell is inside board margins and doesn't overlap with fixed cells.
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
            //Tries to move the current block 1 column to the left, returning True if it has been moved
            if (CanMoveLeft())
            {
                FallingBlock.X -= 1;
                return true;
            }
            return false;
        }

        public bool CanMoveLeft()
        {
            //Ensures that the current falling block can move to the left. It can move if, once moved, keeps inside board margins and doesn't overlap with existing fixed cells.
            if (FallingBlock != null)
            {
                //Check each filled cell of the block. It must be inside board margins and not overlap with fixed cells.
                for (var row = 0; row <= 3; row++)
                {
                    for (var column = 0; column <= 3; column++)
                    {
                        if (FallingBlock.FilledCell(column, row))
                        {
                            //This is a filled cell.
                            //Translate its coordinates to board-coordinates.
                            var pt = BlockToBoard(new CellPoint(row, column));
                            //Check if the cell is inside board margins and doesn't overlap with fixed cells.
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
            //Ensures that the current falling block can move to the right. It can move if, once moved, keeps inside board margins and doesn't overlap with existing fixed cells.
            if (FallingBlock != null)
            {
                //Check each filled cell of the block. It must be inside board margins and not overlap with fixed cells.
                for (var row = 0; row <= 3; row++)
                {
                    for (var column = 3; column >= 0; column += -1)
                    {
                        if (FallingBlock.FilledCell(column, row))
                        {
                            //This is a filled cell.
                            //Translate its coordinates to board-coordinates.
                            var pt = BlockToBoard(new CellPoint(row, column));
                            //Check if the cell is inside board margins and doesn't overlap with fixed cells.
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
            //Tries to move the current block 1 column to the right, returning True if it has been moved
            if (CanMoveRight())
            {
                FallingBlock.X += 1;
                return true;
            }
            return false;
        }

        public void NewBlock()
        {
            //Creates a new block.

            if (_nextBlock.Equals(0))
            {
                //This is the first block requested, so get its block type randomly
                FallingBlock = new Block(GetRandomNumber(1, 7));
            }
            else
            {
                //This is not the first block, so use the _nextBlock variable
                FallingBlock = new Block(_nextBlock);
            }
            //Set the block color
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
            //Select the next block to fall (after the one just chosen)
            _nextBlock = GetRandomNumber(1, 7);
            //Position the new block inside the board
            FallingBlock.X = (Columns - 4) / 2;
            FallingBlock.Y = 0;
            //Notify user that a new block has been created
            GotNewBlock?.Invoke(this, new NewBlockEventArgs(FallingBlock.Type, _nextBlock));
        }

        public void CheckBlock()
        {
            //Checks game logic with the falling block. Concretely, checks that:
            //1) The falling block has reached the top of the board (it overlaps with existing fixed cells)
            //2) The falling block has reached its ultimate position and, therefore, it cannot be moved anymore (for example, it has been dropped to the bottom of the board)

            if (FallingBlock != null)
            {
                //Let's see if the falling block overlaps with existing fixed cells. If so, the game is over
                dynamic overlapBlock = false;
                //Check each existing filled cell on the current falling block
                for (var row = 0; row <= 3; row++)
                {
                    for (var column = 0; column <= 3; column++)
                    {
                        if (FallingBlock.FilledCell(column, row))
                        {
                            //This is a filled cell
                            //Translate its coordinates to board-coordinates
                            var pt = BlockToBoard(new CellPoint(row, column));
                            if (Cells[pt.ToString()].Fixed)
                            {
                                overlapBlock = true;
                                break; // TODO: might not be correct. Was : Exit For
                            }
                        }
                    }
                    if (overlapBlock)
                    {
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
                if (overlapBlock)
                {
                    //An overlap exists. This is game over!
                    GameOver?.Invoke(this, new EventArgs());
                }
                else
                {
                    //Now check if the current falling block has been dropped to its ultimate position. If so, transform each filled cell of the block into a fixed cell on the board.
                    dynamic fixBlock = false;

                    for (var column = 0; column <= 3; column++)
                    {
                        for (var row = 3; row >= 0; row += -1)
                        {
                            if (FallingBlock.FilledCell(column, row))
                            {
                                var pt = BlockToBoard(new CellPoint(row, column));
                                //If the falling block has a filled cell just over the board's bottom, or just over a fixed cell in the next line, it must be fixed
                                if (pt.Row.Equals(Rows - 1) ||
                                    Cells[new CellPoint(pt.Row + 1, pt.Column).ToString()].Fixed)
                                {
                                    fixBlock = true;
                                }
                                break; // TODO: might not be correct. Was : Exit For
                            }
                        }
                        if (fixBlock)
                        {
                            break; // TODO: might not be correct. Was : Exit For
                        }
                    }

                    if (fixBlock)
                    {
                        //Transform each filled cell on the block into a fixed cell on the board
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
                        //As a block has been dropped and fixed, check if the player has completed full rows
                        CheckFullRows();
                    }
                }
            }
        }

        public void CheckFullRows()
        {
            //Checks if the player has completed full rows (this is the main Tetris objective!)

            dynamic fullRows = new List<int>();

            //Check each row from bottom to top
            for (var row = Rows - 1; row >= 0; row += -1)
            {
                dynamic fullRow = true;
                //Check if all columns are fixed in the row
                for (var column = 0; column <= Columns - 1; column++)
                {
                    if (!Cells[row + "," + column].Fixed)
                    {
                        fullRow = false;
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
                //The row is full filled
                if (fullRow)
                {
                    fullRows.Add(row);
                }
            }
            if (fullRows.Count > 0)
            {
                //Delete the full rows
                foreach (int row in fullRows)
                {
                    DeleteRow(row);
                }
                //Notify the user (you should probably reward the player)
                FullRows?.Invoke(this, new FullRowsEventArgs(fullRows.Count));
            }
        }

        public void DeleteRow(int row)
        {
            //Delete a row

            //To delete a row, drop-down the entire board over the deleted row and clear the first row
            for (var r = row; r >= 1; r += -1)
            {
                for (var col = 0; col <= Columns - 1; col++)
                {
                    Cells[r + "," + col].Fixed = Cells[r - 1 + "," + col].Fixed;
                    Cells[r + "," + col].Color = Cells[r - 1 + "," + col].Color;
                }
            }
            //Clear the first row
            for (var col = 0; col <= Columns - 1; col++)
            {
                Cells["0," + col].Fixed = false;
            }
        }

        public CellPoint BlockToBoard(CellPoint p) => new CellPoint(p.Row + FallingBlock.Y, p.Column + FallingBlock.X);

        public CellPoint BoardToBlock(CellPoint p) => new CellPoint(p.Row - FallingBlock.Y, p.Column - FallingBlock.X);

        public Color GetCellColor(CellPoint p)
        {
            //Returns the color in which a cell must be painted. Defaults to Transparent, which means that the cell is empty.
            var output = Color.Transparent;

            if (Cells[p.Row + "," + p.Column].Fixed)
            {
                //If the cell if fixed in the board, return its fixed color
                output = Cells[p.Row + "," + p.Column].Color;
            }
            else
            {
                //If the cell is inside the 4x4 matrix of the falling block...
                if (FallingBlock != null && CellIsInsideBlock(p.Row, p.Column))
                {
                    //Translate to block coordinates
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
    }
}