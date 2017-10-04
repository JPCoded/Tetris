#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Timer = System.Timers.Timer;

#endregion


namespace Tetris
{
    public sealed class TetrisClass : PictureBox
    {
        public delegate void FullRowsEventHandler(object sender, FullRowsEventArgs e);

        public delegate void GameOverEventHandler(object sender, EventArgs e);

        public delegate void NewBlockEventHandler(object sender, NewBlockEventArgs e);

        public delegate void StartingEventHandler(object sender, EventArgs e);

        public enum BackgroundStyles
        {
            SolidColor,


            Gradient,

            Picture
        }

        private readonly Timer _timer;

        private BackgroundStyles _backgroundStyle = BackgroundStyles.SolidColor;

        private Board _board;

        private int _cellSize = 25;

        private int _columns = 10;

        private Color _gradientColor1 = Color.SteelBlue;

        private Color _gradientColor2 = Color.Black;

        private LinearGradientMode _gradientDirection = LinearGradientMode.Vertical;

        private bool _pause;


        private int _rows = 20;

        private bool _running;

        private KeyboardHook withEventsField__hook;


        public TetrisClass()
        {
            DoubleBuffered = true;
            _timer = new Timer(1000) {SynchronizingObject = this};
           
            _timer.Elapsed += TimerElapsed;
        }

        private KeyboardHook Hook
        {
            get => withEventsField__hook;
            set
            {
                if (withEventsField__hook != null)
                {
                    KeyboardHook.KeyDown -= _hook_KeyDown;
                }
                withEventsField__hook = value;
                if (withEventsField__hook != null)
                {
                    KeyboardHook.KeyDown += _hook_KeyDown;
                }
            }
        }


        public Color RandomBlockColor { get; set; }

        public Color UncompleteRowColor { get; set; }

        public Keys LeftKey { get; set; }

        public Keys RightKey { get; set; }

        public Keys RotateKey { get; set; }

        public Keys DropKey { get; set; }

        public Color Block1Color { get; set; }

        public Color Block2Color { get; set; }

        public Color Block3Color { get; set; }

        public Color Block4Color { get; set; }

        public Color Block5Color { get; set; }

        public Color Block6Color { get; set; }

        public Color Block7Color { get; set; }

        public int TimerInterval
        {
            get => (int) _timer.Interval;
            set => _timer.Interval = value;
        }

        public Color GradientColor1
        {
            get => _gradientColor1;
            set
            {
                _gradientColor1 = value;
                if (BackgroundStyle == BackgroundStyles.Gradient)
                {
                    Invalidate();
                }
            }
        }

        public Color GradientColor2
        {
            get => _gradientColor2;
            set
            {
                _gradientColor2 = value;
                if (BackgroundStyle == BackgroundStyles.Gradient)
                {
                    Invalidate();
                }
            }
        }

        public LinearGradientMode GradientDirection
        {
            get => _gradientDirection;
            set
            {
                _gradientDirection = value;
                if (BackgroundStyle == BackgroundStyles.Gradient)
                {
                    Invalidate();
                }
            }
        }

        public BackgroundStyles BackgroundStyle
        {
            get => _backgroundStyle;
            set
            {
                _backgroundStyle = value;
                Invalidate();
            }
        }

        public int Rows
        {
            get => _rows;
            set
            {
                _rows = value;
                Invalidate();
            }
        }

        public int Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                Invalidate();
            }
        }

        public int CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Width = Columns * CellSize - (Columns - 1);
            Height = Rows * CellSize - (Rows - 1);

            switch (BackgroundStyle)
            {
                case BackgroundStyles.SolidColor:
                    e.Graphics.Clear(BackColor);

                    break;
                case BackgroundStyles.Gradient:
                    using (var b = new LinearGradientBrush(DisplayRectangle, GradientColor1, GradientColor2,
                        GradientDirection))
                    {
                        e.Graphics.FillRectangle(b, DisplayRectangle);
                    }


                    break;
                case BackgroundStyles.Picture:
                    if (BackgroundImage != null)
                    {
                        e.Graphics.DrawImage(BackgroundImage, 0, 0, Width, Height);
                    }
                    else
                    {
                        e.Graphics.Clear(BackColor);
                    }
                    break;
            }

            if (_board != null)
            {
                for (var row = 0; row <= Rows - 1; row++)
                {
                    for (var column = 0; column <= Columns - 1; column++)
                    {
                        var c = _board.GetCellColor(new CellPoint(row, column));
                        if (c != Color.Transparent)
                        {
                            using (var b = new SolidBrush(c))
                            {
                                e.Graphics.FillRectangle(b,
                                    new Rectangle(column * (CellSize - 1), row * (CellSize - 1), CellSize - 1,
                                        CellSize - 1));
                            }
                            e.Graphics.DrawRectangle(Pens.Black,
                                new Rectangle(column * (CellSize - 1), row * (CellSize - 1), CellSize - 1,
                                    CellSize - 1));
                        }
                    }
                }
            }
        }


        public event FullRowsEventHandler FullRows;

        public event GameOverEventHandler GameOver;

        public event StartingEventHandler Starting;

        public event NewBlockEventHandler NewBlock;

        public void StartGame()
        {
            if (!IsRunning() && !IsPaused())
            {
                VBMath.Randomize();
                _board = new Board(Rows, Columns);
                var with1 = _board;
                with1.Block1Color = Block1Color;
                with1.Block2Color = Block2Color;
                with1.Block3Color = Block3Color;
                with1.Block4Color = Block4Color;
                with1.Block5Color = Block5Color;
                with1.Block6Color = Block6Color;
                with1.Block7Color = Block7Color;
                _board.FullRows += CatchFullRows;
                _board.GameOver += CatchGameOver;
                _board.GotNewBlock += CatchNewBlock;
                _running = true;
                Hook = new KeyboardHook();
                Starting?.Invoke(this, new EventArgs());
                _timer.Start();
            }
        }

        public void StopGame()
        {
            if (IsRunning())
            {
                _timer.Stop();
                _running = false;
                _pause = false;
                Hook.Dispose();
                Hook = null;
            }
        }

        public int FreeRowsFromTop()
        {
            dynamic freeRows = 0;
            if (_board != null)
            {
                for (var row = 0; row <= Rows - 1; row++)
                {
                    dynamic freeRow = true;
                    for (var column = 0; column <= Columns - 1; column++)
                    {
                        if (_board.Cells[row + "," + column].Fixed)
                        {
                            freeRow = false;
                            break;
                        }
                    }
                    if (freeRow)
                    {
                        freeRows += 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return freeRows;
        }

        public void AddRandomBlock()
        {
            int whichColumn;
            do
            {
                whichColumn = GetRandomNumber(0, Columns - 1);
            } while (_board.Cells["0," + whichColumn].Fixed);


            for (var row = Rows - 1; row >= 0; row += -1)
            {
                if (!_board.Cells[row + "," + whichColumn].Fixed)
                {
                    _board.Cells[row + "," + whichColumn].Fixed = true;
                    _board.Cells[row + "," + whichColumn].Color = RandomBlockColor;
                    Invalidate();
                    break;
                }
            }
        }

        public void AddUncompleteRow()
        {
            dynamic forceGameOver = ThereIsSomethingInFirstRow();
            for (var row = 0; row <= Rows - 2; row++)
            {
                for (var column = 0; column <= Columns - 1; column++)
                {
                    _board.Cells[row + "," + column].Fixed = _board.Cells[row + 1 + "," + column].Fixed;
                    _board.Cells[row + "," + column].Color = _board.Cells[row + 1 + "," + column].Color;
                }
            }

            var emptyColumn = GetRandomNumber(0, Columns - 1);

            for (var column = 0; column <= Columns - 1; column++)
            {
                if (column.Equals(emptyColumn))
                {
                    _board.Cells[Rows - 1 + "," + column].Fixed = false;
                }
                else
                {
                    _board.Cells[Rows - 1 + "," + column].Fixed = true;
                    _board.Cells[Rows - 1 + "," + column].Color = UncompleteRowColor;
                }
            }

            Invalidate();
        }

        public bool IsRunning() => _running;

        public bool IsPaused() => _pause;

        public void Pause()
        {
            if (IsRunning() && !IsPaused())
            {
                _pause = true;
            }
        }

        public void Resume()
        {
            if (IsPaused())
            {
                _pause = false;
            }
        }


        private void TimerElapsed(object sender, EventArgs e)
        {
            if (!IsPaused())
            {
                RedrawAndCheckBlock();

                if (_board.FallingBlock == null)
                {
                    _board.NewBlock();
                    RedrawAndCheckBlock();
                }
                else
                {
                    _board.FallingBlock.Y += 1;
                }
            }
        }

        private void RedrawAndCheckBlock()
        {
            Invalidate();
            _board.CheckBlock();
        }

        private void _hook_KeyDown(Keys key)
        {
            if (!IsPaused())
            {
                if (key == LeftKey)
                {
                    if (_board.MoveLeft())
                    {
                        RedrawAndCheckBlock();
                    }
                }
                else if (key == RightKey)
                {
                    if (_board.MoveRight())
                    {
                        RedrawAndCheckBlock();
                    }
                }
                else if (key == DropKey)
                {
                    TimerElapsed(null, null);
                }
                else if (key == RotateKey)
                {
                    if (_board.Rotate())
                    {
                        RedrawAndCheckBlock();
                    }
                }
            }
        }

        private void CatchFullRows(object sender, FullRowsEventArgs e)
        {
            FullRows?.Invoke(this, e);
            Invalidate();
        }

        private void CatchGameOver(object sender, EventArgs e)
        {
            StopGame();

            GameOver?.Invoke(this, e);
        }

        private void CatchNewBlock(object sender, NewBlockEventArgs e) => NewBlock?.Invoke(sender, e);

        private bool ThereIsSomethingInFirstRow()
        {
            dynamic output = false;
            for (var column = 0; column <= Columns - 1; column++)
            {
                if (_board.Cells["0," + column].Fixed)
                {
                    output = true;
                    break;
                }
            }
            return output;
        }

        private static int GetRandomNumber(int lowerbound, int upperbound) => Convert.ToInt32(
                                                                                  Math.Floor((upperbound - lowerbound +
                                                                                              1) *
                                                                                             VBMath.Rnd())) +
                                                                              lowerbound;
    }
}