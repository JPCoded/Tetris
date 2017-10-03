#region

using System;
using System.Collections.Generic;
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
#region "Private Classes"

        private class Board
        {
            public delegate void FullRowsEventHandler(object sender, FullRowsEventArgs e);

            public delegate void GameOverEventHandler(object sender, EventArgs e);

            public delegate void GotNewBlockEventHandler(object sender, NewBlockEventArgs e);
            //Allows to customize the color of the type 7 blocks

            //The next block that will fall after the current that is falling
            private int _nextBlock;
            //Fires every time a new block is created. This is useful for adding difficulties every certain number of blocks.

            public Board(int rows, int columns)
            {
                //Instantiates a new Board class passing its rows and columns number

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

            private int GetRandomNumber(int lowerbound, int upperbound) => Convert.ToInt32(
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

            private bool CanRotate()
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

            private bool CanMoveLeft()
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

            private bool CanMoveRight()
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

            private void CheckFullRows()
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

            private void DeleteRow(int row)
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

            private CellPoint BlockToBoard(CellPoint p) => new CellPoint(p.Row + FallingBlock.Y, p.Column + FallingBlock.X);

            private CellPoint BoardToBlock(CellPoint p) => new CellPoint(p.Row - FallingBlock.Y, p.Column - FallingBlock.X);

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

            private bool CellIsInsideBlock(int row, int column) => row >= FallingBlock.Y && row <= FallingBlock.Y + 3 &&
                                                                   column >= FallingBlock.X && column <= FallingBlock.X + 3;
        }

        #endregion

        #region "Public Classes"

        //Argument for the FullRows event.
        public class FullRowsEventArgs : EventArgs
        {
            public FullRowsEventArgs(int numberOfRows)
            {
                NumberOfRows = numberOfRows;
            }

            //Contains the number of rows filled at once, from 1 to 4. In the classic Tetris game, the more rows you filled at once, the more points are you rewarded with.
            public int NumberOfRows { get; }
        }

        //Argument for the NewBlock event.
        public class NewBlockEventArgs : EventArgs
        {
            public NewBlockEventArgs(int blockType, int nextBlockType)
            {
                BlockType = blockType;
                NextBlockType = nextBlockType;
            }

            //This property notifies about the falling block type
            public int BlockType { get; }

            //This property notifies about the next falling block type
            public int NextBlockType { get; }
        }

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