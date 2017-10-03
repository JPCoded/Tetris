namespace Tetris
{
    internal interface ICellPoint
    {
        int Row { get; }
        int Column { get; }
        string ToString();
    }
}