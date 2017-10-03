#region

using System;

#endregion



namespace Tetris
{
   
        public sealed class FullRowsEventArgs : EventArgs, IFullRowsEventArgs
        {
            internal FullRowsEventArgs(int numberOfRows)
            {
                NumberOfRows = numberOfRows;
            }

            //Contains the number of rows filled at once, from 1 to 4. In the classic Tetris game, the more rows you filled at once, the more points are you rewarded with.
            public int NumberOfRows { get; }
        }

      
    }
