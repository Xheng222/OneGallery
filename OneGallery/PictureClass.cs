using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml.Media;

namespace OneGallery
{
    public class PictureClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int Height { get; set; }

        public int Width { get; set; }

        public DateTime CreatDate { get; set; }

        public DateTime LastEditDate { get; set; }

        public DateTime ShootDate { get; set; }

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

        public void OnPropertyChanged_Stretch()
        {
            OnPropertyChanged(nameof(_nowStretch));
        }

        public PictureClass(string _path, string _name, int _width, int _height,
                            DateTime creatDate, DateTime lastEditDate, DateTime shootDate)
        {
            ImageLocation = _path;
            Name = _name;
            Height = _height;
            Width = _width;
            IsSelected = false;
            CreatDate = creatDate;
            LastEditDate = lastEditDate;
            ShootDate = shootDate;
        }

        public PictureClass() { }
    }
}
