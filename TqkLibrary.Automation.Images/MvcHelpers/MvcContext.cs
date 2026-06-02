using System;
using System.Collections.Generic;
using System.Threading;

namespace TqkLibrary.Automation.Images.MvcHelpers
{
    /// <summary>
    /// Context passed to an <see cref="ImageNameAttribute"/> handler when its image is found.
    /// </summary>
    public class MvcContext
    {
        /// <summary>
        /// The image name that was found and routed to this handler.
        /// </summary>
        public required string ImageName { get; init; }

        /// <summary>
        /// Template index (the j-th template image of <see cref="ImageName"/>) that matched.
        /// </summary>
        public required int Index { get; init; }

        /// <summary>
        /// The find result of the matched image.
        /// </summary>
        public required OpenCvFindResult Result { get; init; }

        /// <summary>
        /// Find history of the current run, oldest first. Does not include the current find yet.
        /// </summary>
        public required IReadOnlyList<FindHistory> Histories { get; init; }

        /// <summary>
        /// The service provider used to resolve controllers, if any.
        /// </summary>
        public IServiceProvider? Services { get; init; }

        /// <summary>
        /// Cancellation token of the run.
        /// </summary>
        public CancellationToken CancellationToken { get; init; }
    }
}
