#region

using System;

#endregion

namespace Tetris
{

        public sealed class NewBlockEventArgs : EventArgs, INewBlockEventArgs
        {
            public NewBlockEventArgs(int blockType, int nextBlockType)
            {
                BlockType = blockType;
                NextBlockType = nextBlockType;
            }

          
            public int BlockType { get; }

    
            public int NextBlockType { get; }
        }

       
   
}