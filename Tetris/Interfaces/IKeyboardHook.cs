using System;

namespace Tetris
{
    internal interface IKeyboardHook : IDisposable
    {
        void Dispose(bool disposing);
        new void Dispose();
    }
}