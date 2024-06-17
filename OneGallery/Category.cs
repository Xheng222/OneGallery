using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace OneGallery
{
    public class Category : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool IsFolder = false;

        public bool IsGallery = false;

        public bool IsHomePage = false;

        public bool IsFolderInfo = false;

        public bool IsGalleryInfo = false;

        public bool IsAddSelection = false;

        private string Name { get; set; }

        public string _name
        {
            get => Name;
            set
            {
                Name = value;
                OnPropertyChanged();
            }
        }

        public FontIcon Icon = new();

        public void SetFontIcon(string _glyph)
        {
            Icon.Glyph = _glyph;
        }

        public ObservableCollection<Category> Children = new();

        public string PageType { get; set; }


        public Category()
        {
            Icon.Glyph = "\uE713";
        }


        private int SearchCount { get; set; } = 0;

        public int _searchCount
        {
            get => SearchCount;
            set
            {
                SearchCount = value;
                if (SearchCount > 0)
                {
                    _processBarOpacity = 1;
                }
                else
                {
                    _processBarOpacity = 0;
                }
            }
        }

        private int ProcessBarOpacity { get; set; } = 0;

        public int _processBarOpacity
        {
            get => ProcessBarOpacity;
            set
            {
                ProcessBarOpacityChange(value);
            }
        }

        private async void ProcessBarOpacityChange(int _value)
        {
            ProcessBarOpacity = _value;
            await Task.Delay(600);
            OnPropertyChanged(nameof(_processBarOpacity));
        }



    }
}
