using System.Collections.Generic;
using System.Drawing;

namespace Tetris
{
    internal interface IBoard
    {
        int Rows { get; }
        int Columns { get; }
        Dictionary<string, Cell> Cells { get; }
        Block FallingBlock { get; set; }
        Color Block1Color { get; set; }
        Color Block2Color { get; set; }
        Color Block3Color { get; set; }
        Color Block4Color { get; set; }
        Color Block5Color { get; set; }
        Color Block6Color { get; set; }
        Color Block7Color { get; set; }
        event Board.FullRowsEventHandler FullRows;
        event Board.GameOverEventHandler GameOver;
        event Board.GotNewBlockEventHandler GotNewBlock;
        bool Rotate();
        bool CanRotate();
        bool MoveLeft();
        bool CanMoveLeft();
        bool CanMoveRight();
        bool MoveRight();
        void NewBlock();
        void CheckBlock();
        void CheckFullRows();
        void DeleteRow(int row);
        CellPoint BlockToBoard(CellPoint p);
        CellPoint BoardToBlock(CellPoint p);
        Color GetCellColor(CellPoint p);
        bool CellIsInsideBlock(int row, int column);
    }
}