using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Media.Images
{
    public class ImageTemplateHelper : IDisposable
    {
        readonly string _workingDir;
        readonly string _extension;
        public ImageTemplateHelper(string workingDir, string extension = "png")
        {
            if (!Directory.Exists(workingDir))
                throw new DirectoryNotFoundException(workingDir);
            if(string.IsNullOrWhiteSpace(extension))
                throw new ArgumentNullException(nameof(extension));

            _extension = extension;
            _workingDir = workingDir;
        }
        ~ImageTemplateHelper()
        {
            Clean();
        }


        public Bitmap GetImage(string name, int index)
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
                    if (!dictionary.ContainsKey(fileName)) dictionary[fileName] = (Bitmap)Bitmap.FromFile($"{_workingDir}\\{fileName}.{_extension}");
                }
            }
            Bitmap bitmap = dictionary[fileName];
            lock (dictionary) return new Bitmap(bitmap);
        }

        private bool ImageExit(string fileName) => File.Exists($"{_workingDir}\\{fileName}.{_extension}");


        public void Dispose()
        {
            Clean();
            GC.SuppressFinalize(this);
        }

        void Clean()
        {
            foreach(var item in dictionary)
            {
                using(var bm = item.Value)
                {

                }
            }
        }
    }
}
