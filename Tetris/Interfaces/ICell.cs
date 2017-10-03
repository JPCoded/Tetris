using System.Drawing;

namespace Tetris
{
    internal interface ICell
    {
        int Row { get; }
        int Column { get; }
        bool Fixed { get; set; }
        Color Color { get; set; }
    }
}