namespace Tetris
{
    public interface INewBlockEventArgs
    {
        int BlockType { get; }
        int NextBlockType { get; }
    }
}