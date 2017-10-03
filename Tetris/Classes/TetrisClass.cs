#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Timer = System.Timers.Timer;

#endregion



//As this class is, basically, a Tetris drawing surface, I thought that it would be logical to inherit it from the PictureBox class
namespace Tetris
{
    public sealed partial class TetrisClass : PictureBox
    {
      
        public enum BackgroundStyles
        {
            SolidColor,


            Gradient,

            Picture
        }

     

        #region "Constructor"

        public TetrisClass()
        {
            //Set DoubleBuffered to true to avoid screen-flickering.
            DoubleBuffered = true;
            //Instantiate the Timer and set a default interval of 1000 ms.
            _timer = new Timer(1000) {SynchronizingObject = this};
            //Set the SynchronizingObject of the timer (if not, the Elapsed event fires into its own subprocess and it's impossible to update the UI)
            //Add a handler to the Elapsed event
            _timer.Elapsed += TimerElapsed;
        }

        #endregion

        #region "OnPaint"

        protected override void OnPaint(PaintEventArgs e)
        {
            //First, set the control's size
            Width = Columns * CellSize - (Columns - 1);
            Height = Rows * CellSize - (Rows - 1);

            //Draw the background
            switch (BackgroundStyle)
            {
                case BackgroundStyles.SolidColor:
                    //Draws a single color
                    e.Graphics.Clear(BackColor);

                    break;
                case BackgroundStyles.Gradient:
                    //Draws a gradient
                    using (var b = new LinearGradientBrush(DisplayRectangle, GradientColor1, GradientColor2,
                               GradientDirection))
                    {
                        e.Graphics.FillRectangle(b, DisplayRectangle);
                    }


                    break;
                case BackgroundStyles.Picture:
                    //Draws a picture if BackgroundImage is set; if not, draws a single color
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

            //Paint cells
            if (_board != null)
            {
                for (var row = 0; row <= Rows - 1; row++)
                {
                    for (var column = 0; column <= Columns - 1; column++)
                    {
                        //Get the cell color
                        var c = _board.GetCellColor(new CellPoint(row, column));
                        if (c != Color.Transparent)
                        {
                            //Draw the cell background
                            using (var b = new SolidBrush(c))
                            {
                                e.Graphics.FillRectangle(b,
                                    new Rectangle(column * (CellSize - 1), row * (CellSize - 1), CellSize - 1,
                                        CellSize - 1));
                            }
                            //Draw the cell border
                            e.Graphics.DrawRectangle(Pens.Black,
                                new Rectangle(column * (CellSize - 1), row * (CellSize - 1), CellSize - 1, CellSize - 1));
                        }
                    }
                }
            }
        }

#endregion
#region "Public Classes"

        #endregion

        #region "Public Events"

        //Fires when the player completes entire rows
        public event FullRowsEventHandler FullRows;

        public delegate void FullRowsEventHandler(object sender, FullRowsEventArgs e);

        //Fires when a block reaches the top of the board and, therefore, overlaps with existing fixed cells
        public event GameOverEventHandler GameOver;

        public delegate void GameOverEventHandler(object sender, EventArgs e);

        //Fires when the game is about to start
        public event StartingEventHandler Starting;

        public delegate void StartingEventHandler(object sender, EventArgs e);

        //Fires every time a new block is created
        public event NewBlockEventHandler NewBlock;

        public delegate void NewBlockEventHandler(object sender, NewBlockEventArgs e);

        #endregion

        #region "Private Variables"

        //Number of board rows. Internal storage for Rows property.
        private int _rows = 20;

        //Number of board columns. Internal storage for Columns property.
        private int _columns = 10;

        //Cell size (in pixels). Internal storage for CellSize property. [Board width = Columns x CellSize] [Board height = Rows x CellSize]
        private int _cellSize = 25;

        //Background style. Internal storage for BackgroundStyle property.
        private BackgroundStyles _backgroundStyle = BackgroundStyles.SolidColor;

        //First gradient color. Internal storage for GradientColor1 property.
        private Color _gradientColor1 = Color.SteelBlue;

        //Second gradient color. Internal storage for GradientColor2 property.
        private Color _gradientColor2 = Color.Black;

        //Gradient direction. Internal storage for GradientDirection property.
        private LinearGradientMode _gradientDirection = LinearGradientMode.Vertical;

        //Timer used to drop the falling block 1 row automatically.
        private readonly Timer _timer;

        //The board.
        private Board _board;

        //Stores a value indicating whether the game is running or not
        private bool _running;

        //Stores a value indicating whether the game is paused or not
        private bool _pause;

        private KeyboardHook withEventsField__hook;

        private KeyboardHook _hook
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
            //Hook to catch keystrokes
        }

        #endregion

        #region "Public Auto-Implemented Properties"

        public Color RandomBlockColor { get; set; }

        //The color of random-generated blocks (difficulty)
        public Color UncompleteRowColor { get; set; }

        //The color of umcomplete rows (difficulty)
        public Keys LeftKey { get; set; }

        //Allows to customize the Left key for the game (defaults to the left direction key)
        public Keys RightKey { get; set; }

        //Allows to customize the Right key for the game (defaults to the right direction key)
        public Keys RotateKey { get; set; }

        //Allows to customize the Rotate key for the game (defaults to the up direction key)
        public Keys DropKey { get; set; }

        //Allows to customize the Drop key for the game (defaults to the down direction key)
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
        //Allows to customize the color of the type 7 blocks

        #endregion

        #region "Public Properties"

        //Gets or sets the Timer interval (game speed). The shorter interval, the higher speed.
        public int TimerInterval
        {
            get => (int) _timer.Interval;
            set => _timer.Interval = value;
        }

        //Gets or sets the first color of the gradient when BackgroundStyle = Gradient
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

        //Gets or sets the second color of the gradient when BackgroundStyle = Gradient
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

        //Gets or sets the direction of the gradient when BackgroundStyle = Gradient
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

        //Gets or sets the background style
        public BackgroundStyles BackgroundStyle
        {
            get => _backgroundStyle;
            set
            {
                _backgroundStyle = value;
                Invalidate();
            }
        }

        //Gets or sets the number of rows in the game board
        public int Rows
        {
            get => _rows;
            set
            {
                _rows = value;
                Invalidate();
            }
        }

        //Gets or sets the number of columns in the game board
        public int Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                Invalidate();
            }
        }

        //Gets or sets the cell size (in pixels)
        public int CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = value;
                Invalidate();
            }
        }

        #endregion

        #region "Public Methods And Functions"

        public void StartGame()
        {
            //Starts a game
            if (!IsRunning() && !IsPaused())
            {
                //Initialize the ramdom number generator
                VBMath.Randomize();
                //Initialize the game board
                _board = new Board(Rows, Columns);
                var _with1 = _board;
                _with1.Block1Color = Block1Color;
                _with1.Block2Color = Block2Color;
                _with1.Block3Color = Block3Color;
                _with1.Block4Color = Block4Color;
                _with1.Block5Color = Block5Color;
                _with1.Block6Color = Block6Color;
                _with1.Block7Color = Block7Color;
                //Add handlers to catch board events
                _board.FullRows += CatchFullRows;
                _board.GameOver += CatchGameOver;
                _board.GotNewBlock += CatchNewBlock;
                //Set that the game is running
                _running = true;
                //Initialize the keyboard hook
                _hook = new KeyboardHook();
                //Notify user
                Starting?.Invoke(this, new EventArgs());
                //Start the timer
                _timer.Start();
            }
        }

        public void StopGame()
        {
            //Stops a running game
            if (IsRunning())
            {
                //Stop the timer
                _timer.Stop();
                //Not running
                _running = false;
                //Not paused
                _pause = false;
                //Dispose the keyboard hook
                _hook.Dispose();
                _hook = null;
            }
        }

        public int FreeRowsFromTop()
        {
            //In classic Tetris game, the game was not continuous, but the player should be playing among different screens, each one with its one goal. For example, the first
            //screen was an initial empty board and the player must play until he completes 5 full rows. In this game system, when a new screen was achieved, the game rewarded
            //the player not only for the full rows completed, but also for the completely free rows from the top of the board until the first not-completely free row. So you
            //can use this function to get the number of free rows from board's top.
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
                            break; // TODO: might not be correct. Was : Exit For
                        }
                    }
                    if (freeRow)
                    {
                        freeRows += 1;
                    }
                    else
                    {
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }
            return freeRows;
        }

        public void AddRandomBlock()
        {
            //Adds a random block to the board. The block is always positioned over the board's bottom or a existing fixed cell.

            //Choose the column
            var whichColumn = 0;
            do
            {
                whichColumn = GetRandomNumber(0, Columns - 1);
            } while (_board.Cells["0," + whichColumn].Fixed);

            //Check for a fixed cell in the column to position the random block over it.
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
            //Adds an uncomplete row at the bottom of the board, moving up the rest of the cells. This can cause a game over.
            dynamic forceGameOver = ThereIsSomethingInFirstRow();
            //Move all the board up 1 row
            for (var row = 0; row <= Rows - 2; row++)
            {
                for (var column = 0; column <= Columns - 1; column++)
                {
                    _board.Cells[row + "," + column].Fixed = _board.Cells[(row + 1) + "," + column].Fixed;
                    _board.Cells[row + "," + column].Color =
                        _board.Cells[row + 1 + "," + column].Color;
                }
            }
            //Choose the empty column
            var emptyColumn = GetRandomNumber(0, Columns - 1);
            //Draw the uncomplete row
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
            //Redraw
            Invalidate();
        }

        public bool IsRunning() => _running;

        public bool IsPaused() => _pause;

        public void Pause()
        {
            //Pauses a running game
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

        #endregion

        #region "Private Methods And Functions"

        //Timer elapsed event
        private void TimerElapsed(object sender, EventArgs e)
        {
            if (!IsPaused())
            {
                //Redraw the board and check the current falling block
                RedrawAndCheckBlock();

                if (_board.FallingBlock == null)
                {
                    //If there's not a falling block, create new one
                    _board.NewBlock();
                    RedrawAndCheckBlock();
                }
                else
                {
                    //Drop the falling block 1 line
                    _board.FallingBlock.Y += 1;
                }
            }
        }

        //Redraws the board and checks the falling block
        private void RedrawAndCheckBlock()
        {
            Invalidate();
            _board.CheckBlock();
        }

        //Catches keystrokes
        private void _hook_KeyDown(Keys Key)
        {
            if (!IsPaused())
            {
                if (Key == LeftKey)
                {
                    if (_board.MoveLeft())
                    {
                        RedrawAndCheckBlock();
                    }
                }
                else if (Key == RightKey)
                {
                    if (_board.MoveRight())
                    {
                        RedrawAndCheckBlock();
                    }
                }
                else if (Key == DropKey)
                {
                    //If the user presses the Drop key, force a 1-line drop
                    TimerElapsed(null, null);
                }
                else if (Key == RotateKey)
                {
                    if (_board.Rotate())
                    {
                        RedrawAndCheckBlock();
                    }
                }
            }
        }

        //The board notifies that the player has completed full rows. Notify and redraw board.
        private void CatchFullRows(object sender, FullRowsEventArgs e)
        {
            FullRows?.Invoke(this, e);
            Invalidate();
        }

        //The board notifies that the game is over.
        private void CatchGameOver(object sender, EventArgs e)
        {
            //Stop the game
            StopGame();

            //Notify the user
            GameOver?.Invoke(this, e);
        }

        //The board notifies that a new block has been created. Notify.
        private void CatchNewBlock(object sender, NewBlockEventArgs e)
        {
            NewBlock?.Invoke(sender, e);
        }

        //Checks if there is a fixed cell in the first row
        private bool ThereIsSomethingInFirstRow()
        {
            dynamic output = false;
            for (var column = 0; column <= Columns - 1; column++)
            {
                if (_board.Cells["0," + column].Fixed)
                {
                    output = true;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
            return output;
        }

        private int GetRandomNumber(int lowerbound, int upperbound) => Convert.ToInt32(
                                                                           Math.Floor((upperbound - lowerbound + 1) *
                                                                                      VBMath.Rnd())) + lowerbound;

        #endregion
    }
}