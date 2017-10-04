#region

using System.Drawing;

#endregion

namespace Tetris
{
    internal sealed class Cell : ICell
    {
            public Cell(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public int Row { get; }

            public int Column { get; }

            public bool Fixed { get; set; }

            public Color Color { get; set; }
        }


    
}