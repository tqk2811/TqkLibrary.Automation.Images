using System;
using System.Collections.Generic;

namespace TqkLibrary.Automation.Images.MvcHelpers
{
    /// <summary>
    /// Reason an MVC run finished.
    /// </summary>
    public enum MvcRunReason
    {
        /// <summary>
        /// A handler returned no next image names, so the flow completed normally.
        /// </summary>
        Completed,
        /// <summary>
        /// A step timed out without finding any of the requested images.
        /// </summary>
        TimedOut,
    }

    /// <summary>
    /// Result of an <see cref="ImageMvcHelper{TColor, TDepth}"/> run.
    /// </summary>
    public class MvcRunResult
    {
        /// <summary>
        /// Why the run finished.
        /// </summary>
        public MvcRunReason Reason { get; }

        /// <summary>
        /// Find history of the run, oldest first.
        /// </summary>
        public IReadOnlyList<FindHistory> Histories { get; }

        internal MvcRunResult(MvcRunReason reason, IReadOnlyList<FindHistory> histories)
        {
            this.Reason = reason;
            this.Histories = histories ?? throw new ArgumentNullException(nameof(histories));
        }
    }
}
