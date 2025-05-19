using System.Drawing;

namespace TqkLibrary.Media.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class OpenCvFindResult
    {
        /// <summary>
        /// 
        /// </summary>
        public Point Point { get; internal set; }

        /// <summary>
        /// 
        /// </summary>        
        public double Percent { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Point}-{Percent}";
        }
    }
}
