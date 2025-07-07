using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
namespace TqkLibrary.Automation.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageCropHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public ImageCropHelper()
        {
            dict_crops = new Dictionary<string, Rectangle>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict_crops"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ImageCropHelper(Dictionary<string, Rectangle> dict_crops)
        {
            this.dict_crops = dict_crops ?? throw new ArgumentNullException(nameof(dict_crops));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        public ImageCropHelper(IDictionary<string, Rectangle> dict) : this(new Dictionary<string, Rectangle>(dict))
        {

        }

        readonly string? _workingDir;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workingDir"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public ImageCropHelper(string workingDir) : this()
        {
            if (!Directory.Exists(workingDir)) throw new DirectoryNotFoundException(workingDir);
            _workingDir = workingDir;
        }



        readonly Dictionary<string, Rectangle> dict_crops;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, Rectangle value) => dict_crops.Add(name, value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Rectangle? GetCrop(string name)
        {
            if (!string.IsNullOrWhiteSpace(_workingDir))
            {
                string path = Path.Combine(_workingDir, $"{name}.json");
                if (File.Exists(path))
                {
                    string text = File.ReadAllText(path);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return JsonConvert.DeserializeObject<Rectangle>(text);
                    }
                }
            }
            if (dict_crops.ContainsKey(name)) return dict_crops[name];
            else return null;
        }
    }
}
