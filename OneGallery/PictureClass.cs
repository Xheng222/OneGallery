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

        public DateTimeOffset CreatDate { get; set; }
        public DateTimeOffset LastEditDate { get; set; }
        public DateTimeOffset ShootDate { get; set; }

        public PictureClass(string _path, string _name, uint _width, uint _height, string _sourceName, DateTimeOffset creatDate, DateTimeOffset lastEditDate, DateTimeOffset shootDate)
        {
            ImageLocation = _path;
            Name = _name;
            Height = (int)_height;
            Width = (int)_width;
            SourceName = _sourceName;
            IsSelected = false;
            CreatDate = creatDate;
            LastEditDate = lastEditDate;
            ShootDate = shootDate;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string _imageLocation
        {
            get => ImageLocation;
            set
            {
                ImageLocation = value;
                OnPropertyChanged();
            }
        }

        public string _name
        {
            get => Name;
            set
            {
                Name = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected { get; set; }
        public bool _isSelected
        {
            get => IsSelected;
            set
            {
                IsSelected = value;
                OnPropertyChanged();
            }
        }

        public int CheckBoxOpacity = 0;

        public int _checkBoxOpacity
        {
            get => CheckBoxOpacity;
            set
            {
                CheckBoxOpacity = value;
                OnPropertyChanged();
            }
        }

        public double RectangleOpacity = 0;

        public double _rectangleOpacity
        {
            get => RectangleOpacity;
            set
            {
                RectangleOpacity = value;
                OnPropertyChanged();
            }
        }

        public int BorderOpacity = 0;

        public int _borderOpacity
        {
            get => BorderOpacity;
            set
            {
                BorderOpacity = value;
                OnPropertyChanged();
            }
        }
    }
}
