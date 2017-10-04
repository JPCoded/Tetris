#region

using System.Collections.Generic;
using System.Drawing;

#endregion


namespace Tetris
{
    internal sealed class Block : IBlock
    {
        private readonly Dictionary<int, List<string>> _rotations = new Dictionary<int, List<string>>();

        private int _currentRotation = 1;

        private int _rotationsNumber;

        public Block(int blockType)
        {
            Type = blockType;
            switch (Type)
            {
                case 1:
                    InitializeBlock(
                        new List<string> {"0100", "0100", "0100", "0100"},
                        new List<string> {"0000", "1111", "0000", "0000"});
                    break;
                case 2:
                    InitializeBlock(
                        new List<string> {"0110", "0110", "0000", "0000"});
                    break;
                case 3:
                    InitializeBlock(
                        new List<string> {"0100", "1110", "0000", "0000"},
                        new List<string> {"0100", "0110", "0100", "0000"},
                        new List<string> {"1110", "0100", "0000", "0000"},
                        new List<string> {"0100", "1100", "0100", "0000"});
                    break;
                case 4:
                    InitializeBlock(
                        new List<string> {"0010", "0110", "0100", "0000"},
                        new List<string> {"0110", "0011", "0000", "0000"});
                    break;
                case 5:
                    InitializeBlock(
                        new List<string> {"0100", "0110", "0010", "0000"},
                        new List<string> {"0011", "0110", "0000", "0000"});
                    break;
                case 6:
                    InitializeBlock(
                        new List<string> {"0100", "0100", "0110", "0000"},
                        new List<string> {"0111", "0100", "0000", "0000"},
                        new List<string> {"0110", "0010", "0010", "0000"},
                        new List<string> {"0001", "0111", "0000", "0000"});
                    break;
                case 7:
                    InitializeBlock(
                        new List<string> {"0010", "0010", "0110", "0000"},
                        new List<string> {"0100", "0111", "0000", "0000"},
                        new List<string> {"0110", "0100", "0100", "0000"},
                        new List<string> {"0111", "0001", "0000", "0000"});
                    break;
            }
        }

        public Color Color { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public List<string> CurrentMatrix => _rotations[_currentRotation];

        public List<string> NextRotationMatrix
        {
            get
            {
                var nextRotation = _currentRotation + 1;
                if (nextRotation > _rotationsNumber)
                {
                    nextRotation = 1;
                }

                return _rotations[nextRotation];
            }
        }

        public int Type { get; }

        public void OffsetRotation()
        {
            _currentRotation += 1;
            if (_currentRotation > _rotationsNumber)
            {
                _currentRotation = 1;
            }
        }

        public bool FilledCell(int x, int y) => CurrentMatrix[y].Substring(x, 1).Equals("1");

        public void InitializeBlock(params List<string>[] rotations)
        {
            _rotationsNumber = rotations.Length;

            for (var k = 0; k <= rotations.Length - 1; k++)
            {
                _rotations.Add(k + 1, rotations[k]);
            }
        }
    }
}