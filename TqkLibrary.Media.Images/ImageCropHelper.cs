using System.Collections.Generic;
using System.Drawing;
using System;
namespace TqkLibrary.Media.Images
{
    public class ImageCropHelper
    {
        public ImageCropHelper()
        {
            dict_crops = new Dictionary<string, Rectangle>();
        }
        public ImageCropHelper(Dictionary<string, Rectangle> dict_crops)
        {
            this.dict_crops = dict_crops ?? throw new ArgumentNullException(nameof(dict_crops));
        }
        public ImageCropHelper(IDictionary<string,Rectangle> dict): this(new Dictionary<string, Rectangle>(dict))
        {

        }



        readonly Dictionary<string, Rectangle> dict_crops;
        public void Add(string name,Rectangle value) => dict_crops.Add(name, value);
        public Rectangle? GetCrop(string name)
        {
            if (dict_crops.ContainsKey(name)) return dict_crops[name];
            else return null;
        }
    }
}
