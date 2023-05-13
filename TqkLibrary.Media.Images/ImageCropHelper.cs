using System;
using System.Collections.Generic;
using System.Drawing;
namespace TqkLibrary.Media.Images
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
            if (dict_crops.ContainsKey(name)) return dict_crops[name];
            else return null;
        }
    }
}
