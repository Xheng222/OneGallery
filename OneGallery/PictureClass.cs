using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage.FileProperties;

namespace OneGallery
{
    public class PictureClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int Height { get; set; }

        public int Width { get; set; }

        public DateTimeOffset CreatDate { get; set; }

        public DateTimeOffset LastEditDate { get; set; }

        public DateTimeOffset ShootDate { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public string _name
        {
            get => Name;
            set
            {
                Name = value;
                OnPropertyChanged();
            }
        }

        public string ImageLocation { get; set; }


        [JsonIgnore]
        public string _imageLocation
        {
            get => ImageLocation;
            set
            {
                ImageLocation = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public int Index { get; set; }

        //public string SourceName { get; set; }


        [JsonIgnore]
        public bool IsSelected { get; set; } = false;

        [JsonIgnore]
        public bool _isSelected
        {
            get => IsSelected;
            set
            {
                IsSelected = value;
                OnPropertyChanged();
            }
        }


        [JsonIgnore]

        public int CheckBoxOpacity { set; get; } = 0;

        [JsonIgnore]
        public int _checkBoxOpacity
        {
            get => CheckBoxOpacity;
            set
            {
                CheckBoxOpacity = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public double RectangleOpacity { set; get; } = 0;


        [JsonIgnore]
        public double _rectangleOpacity
        {
            get => RectangleOpacity;
            set
            {
                RectangleOpacity = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public int BorderOpacity { set; get; } = 0;

        [JsonIgnore]
        public int _borderOpacity
        {
            get => BorderOpacity;
            set
            {
                BorderOpacity = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public static Stretch NowStretch { get; set; } = Stretch.UniformToFill;

        [JsonIgnore]
        public Stretch _nowStretch
        {
            get => NowStretch;
            set
            {
                NowStretch = value;
            }
        }

        public  void OnPropertyChanged_Stretch()
        {
            OnPropertyChanged(nameof(_nowStretch));
        }

        public PictureClass(string _path, string _name, uint _width, uint _height, DateTimeOffset creatDate, DateTimeOffset lastEditDate, DateTimeOffset shootDate)
        {
            ImageLocation = _path;
            Name = _name;
            Height = (int)_height;
            Width = (int)_width;
            IsSelected = false;
            CreatDate = creatDate;
            LastEditDate = lastEditDate;
            ShootDate = shootDate;
        }

        public PictureClass() { }
    }
}
