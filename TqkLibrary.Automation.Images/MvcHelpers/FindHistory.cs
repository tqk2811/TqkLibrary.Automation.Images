using System;

namespace TqkLibrary.Automation.Images.MvcHelpers
{
    /// <summary>
    /// One entry of the find history of an MVC run: an image that was found and routed to a handler.
    /// </summary>
    public class FindHistory
    {
        /// <summary>
        /// The image name that was found.
        /// </summary>
        public string ImageName { get; }

        /// <summary>
        /// Template index (the j-th template image of <see cref="ImageName"/>) that matched.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The find result.
        /// </summary>
        public OpenCvFindResult Result { get; }

        internal FindHistory(string imageName, int index, OpenCvFindResult result)
        {
            this.ImageName = imageName ?? throw new ArgumentNullException(nameof(imageName));
            this.Index = index;
            this.Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{ImageName}{Index} {Result}";
    }
}
