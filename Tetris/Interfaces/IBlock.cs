using System.Collections.Generic;
using System.Drawing;

namespace Tetris
{
    internal interface IBlock
    {
        Color Color { get; set; }
        int X { get; set; }
        int Y { get; set; }
        List<string> CurrentMatrix { get; }
        List<string> NextRotationMatrix { get; }
        int Type { get; }
        void OffsetRotation();
        bool FilledCell(int x, int y);
        void InitializeBlock(params List<string>[] rotations);
    }
}