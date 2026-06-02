using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TqkLibrary.Automation.Images.WaitImageHelpers
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
            : this(name, 0, openCvFindResult)
        {
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <param name="openCvFindResult"></param>
        [SetsRequiredMembers()]
        public WaitImageDataResult(string name, int index, OpenCvFindResult openCvFindResult)
        {
            this.Name = name;
            this.Index = index;
            this.FindResult = openCvFindResult;
        }
        /// <summary>
        ///
        /// </summary>
        public required virtual string Name { get; set; }
        /// <summary>
        /// Template index (the j-th template image of <see cref="Name"/>).
        /// </summary>
        public virtual int Index { get; set; }
        /// <summary>
        ///
        /// </summary>
        public required virtual OpenCvFindResult FindResult { get; set; }
    }
}