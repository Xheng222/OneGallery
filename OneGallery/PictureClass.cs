using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace OneGallery
{
    internal class PictureClass
    {
        public string Name { get; set; }

        public string ImageLocation { get; set; }

        public int Index { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public int CanvasZ {  get; set; }

        public PictureClass(string _path, string _name, uint _width, uint _height, int _index)
        {
            ImageLocation = _path;
            Name = _name;
            Height = (int)_height;
            Width = (int)_width;
            CanvasZ = 0;
            Index = _index;
        }

    }
}
