using System;

namespace TqkLibrary.Media.Images
{
    public class WaitImageTimeoutException : Exception
    {
        internal WaitImageTimeoutException(string message) : base(message)
        {
        }
    }
}
