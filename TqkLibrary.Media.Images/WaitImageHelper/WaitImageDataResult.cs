using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitImageDataResult
    {
        /// <summary>
        /// 
        /// </summary>
        public WaitImageDataResult()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="openCvFindResult"></param>
        [SetsRequiredMembers()]
        public WaitImageDataResult(string name, OpenCvFindResult openCvFindResult)
        {
            this.Name = name;
            this.FindResult = openCvFindResult;
        }
        /// <summary>
        /// 
        /// </summary>
        public required virtual string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public required virtual OpenCvFindResult FindResult { get; set; }
    }
}