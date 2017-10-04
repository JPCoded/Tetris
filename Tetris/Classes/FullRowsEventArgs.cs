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

            public int NumberOfRows { get; }
        }

      
    }
