
namespace Tetris
{
    internal sealed class CellPoint : ICellPoint
    {
          public CellPoint(int row, int column)
            {
                Row = row;
                Column = column;
            }
        
            public int Row { get; }
        
            public int Column { get; }

            public override string ToString() => Row + "," + Column;
        }

       
    
}