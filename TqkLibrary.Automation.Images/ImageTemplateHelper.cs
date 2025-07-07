using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TqkLibrary.Automation.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageTemplateHelper : IDisposable
    {
        readonly string _workingDir;
        readonly string _extension;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workingDir"></param>
        /// <param name="extension"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public ImageTemplateHelper(string workingDir, string extension = "png")
        {
            if (!Directory.Exists(workingDir))
                throw new DirectoryNotFoundException(workingDir);
            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentNullException(nameof(extension));

            _extension = extension;
            _workingDir = workingDir;
        }
        /// <summary>
        /// 
        /// </summary>
        ~ImageTemplateHelper()
        {
            Clean();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Clean();
            GC.SuppressFinalize(this);
        }
        void Clean()
        {
            foreach (var item in dictionary)
            {
                using (var bm = item.Value)
                {

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Bitmap? GetImage(string name, int index)
        {
            string fileName = $"{name}{index}";
            if (ImageExit(fileName)) return GetImage(fileName);
            else return null;
        }



        private readonly Dictionary<string, Bitmap> dictionary = new Dictionary<string, Bitmap>();
        private Bitmap GetImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (!dictionary.ContainsKey(fileName))
            {
                lock (dictionary)
                {
                    string path = Path.Combine(_workingDir, $"{fileName}.{_extension}");
                    if (!dictionary.ContainsKey(fileName)) dictionary[fileName] = (Bitmap)Bitmap.FromFile(path);
                }
            }
            Bitmap bitmap = dictionary[fileName];
            lock (dictionary) return new Bitmap(bitmap);
        }

        private bool ImageExit(string fileName)
        {
            string path = Path.Combine(_workingDir, $"{fileName}.{_extension}");
            return File.Exists(path);
        }
    }
}
