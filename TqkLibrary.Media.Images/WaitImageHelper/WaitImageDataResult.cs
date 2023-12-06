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
        public WaitImageDataResult(string name, OpenCvFindResult openCvFindResult)
        {
            this.Name = name;
            this.FindResult = openCvFindResult;
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual OpenCvFindResult FindResult { get; set; }
    }
}