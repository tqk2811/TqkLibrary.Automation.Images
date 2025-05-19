using System;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageTimeoutException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public WaitImageTimeoutException(string message) : base(message)
        {
        }
    }
}
