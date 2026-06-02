using System;
using System.Collections.Generic;
using System.Linq;

namespace TqkLibrary.Automation.Images.MvcHelpers
{
    /// <summary>
    /// Marks a method as the handler invoked when one of the given image <see cref="Names"/> is found.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ImageNameAttribute : Attribute
    {
        /// <summary>
        /// The image names this handler is responsible for.
        /// </summary>
        public IReadOnlyList<string> Names { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="names"></param>
        /// <exception cref="ArgumentException"></exception>
        public ImageNameAttribute(params string[] names)
        {
            if (names is null || names.Length == 0)
                throw new ArgumentException($"{nameof(names)} must contain at least one image name");
            if (names.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException($"{nameof(names)} must not contain null or whitespace entries");
            Names = names;
        }
    }
}
