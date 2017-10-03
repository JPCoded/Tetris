#region

using System.Drawing;

#endregion

namespace Tetris
{
    internal sealed class Cell : ICell
    {
            //The color in which this cell must be painted in the board

            public Cell(int row, int column)
            {
                //Instantiates a new Cell object passing its row and column
                Row = row;
                Column = column;
            }

            //This class represents a single cell (defined by its row and column) inside the game board
            public int Row { get; }

            //Defines the cell row
            public int Column { get; }

            //Defines the cell column
            public bool Fixed { get; set; }

            //When a block drops in the board to its ultimate position (and therefore it cannot be moved anymore), the board cells corresponding to the block filled cells become Fixed
            public Color Color { get; set; }
        }


    
}