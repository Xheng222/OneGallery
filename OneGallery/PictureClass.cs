using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.FileProperties;

namespace OneGallery
{
    public class PictureClass : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public string ImageLocation { get; set; }

        public int Index { get; set; }

        public string SourceName { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public PictureClass(string _path, string _name, uint _width, uint _height, string _sourceName)
        {
            ImageLocation = _path;
            Name = _name;
            Height = (int)_height;
            Width = (int)_width;
            SourceName = _sourceName;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string _ImageLocation
        {
            get => ImageLocation;
            set
            {
                ImageLocation = value;
                OnPropertyChanged();
            }
        }

        public string _Name
        {
            get => Name;
            set
            {
                Name = value;
                OnPropertyChanged();
            }
        }

    }
}
