using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.VisualBasic;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.StartScreen;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{

    public sealed partial class HomePage : Page
    {
        private LocalFolder HomePageLocalFolder {  get; set; }

        private ImageArrangement HomePageImageArrangement {  get; set; }

        string Path;

        public HomePage()
        {
            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                Path = e.Parameter.ToString();
            }

            base.OnNavigatedTo(e);

            HomePageLocalFolder = new(Path);
            HomePageImageArrangement = HomePageLocalFolder.MyImageArrangement;
            this.Loaded += Init;



        }

        private async void Init(object sender, RoutedEventArgs e)
        {
            await HomePageLocalFolder.Init();
            
            repeater2.ItemsSource = HomePageLocalFolder.ImgList;

            

            var window = (MainWindow)(Application.Current as App).m_window;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Debug.Print("Page_Size " + this.ActualWidth.ToString());
            //SwappableLayoutsItemsView.Height = this.ActualHeight;
            ScrollViewer.Height = this.ActualHeight;
        }




        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image _image = sender as Image;
            var _opacity = _image.Opacity;


        }

        private void ActivityFeedLayout_MyEvent(object sender, EventArgs e)
        {
            //OpacityOut.Begin();
            //await Task.Delay(300);
            //OpacityIn.Begin();
            //repeater2.Opacity = 1;
        }

        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var image = sender as Image;
            
            Debug.Print("Image_PointerEntered " + image.Name);
        }

        private void Image_GotFocus(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;

            //Debug.Print("Image_GotFocus " + image.Name);
        }

        private void ItemContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var _temp = sender as ItemContainer;
            float _scaleX = (float)(_temp.ActualWidth * 1.02 / 2);
            float _scaleY = (float)(_temp.ActualHeight * 1.02 / 2); ;

            _temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            _temp.Scale = new Vector3((float)1.02, (float)1.02, 1);

            var _grid = _temp.Child as Grid;
            var _girdItems = _grid.Children;

            foreach (var _girdItem in _girdItems)
            {
                if (_girdItem is CheckBox)
                {
                    if((_girdItem as CheckBox).IsChecked == false)
                    {
                        var res = _temp.Resources;
                        var _borderIn = (Storyboard)res["BorderIn"];
                        var _checkBoxIn = (Storyboard)res["CheckBoxIn"];
                        var _rectangleIn = (Storyboard)res["RectangleIn"];

                        _borderIn.Begin();
                        _checkBoxIn.Begin();
                        _rectangleIn.Begin();

                        break;
                    }
                    
                }

            }

        }

        private void ItemContainer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var _temp = sender as ItemContainer;
            float _scaleX = (float)(_temp.ActualWidth / 2);
            float _scaleY = (float)(_temp.ActualHeight / 2);

            //_temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            //_temp.Scale = new Vector3(1, 1, 1);

            var _grid = _temp.Child as Grid;
            var _girdItems = _grid.Children;
            foreach (var _girdItem in _girdItems)
            {
                if (_girdItem is CheckBox)
                {
                    if ((_girdItem as CheckBox).IsChecked == false)
                    {
                        var res = _temp.Resources;
                        var _borderOut = (Storyboard)res["BorderOut"];
                        var _checkBoxOut = (Storyboard)res["CheckBoxOut"];
                        var _rectangleOut = (Storyboard)res["RectangleOut"];

                        _borderOut.Begin();
                        _checkBoxOut.Begin();
                        _rectangleOut.Begin();

                        break;
                    }

                }

            }


        }

        private void ItemContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var _temp = sender as ItemContainer;
            var ScaleTransition = new Vector3Transition()
            {
                Components = Vector3TransitionComponents.X | Vector3TransitionComponents.Y,
                Duration = new TimeSpan(1500000)
            };
            _temp.ScaleTransition = ScaleTransition;
        }

        private void ItemContainer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var _temp = sender as ItemContainer;
            var _res = _temp.Resources;

            var _scaleX = (float)(_temp.ActualWidth * 0.98 / 2);
            var _scaleY = (float)(_temp.ActualHeight * 0.98 / 2);

            _temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            _temp.Scale = new Vector3((float)0.98, (float)0.98, 1);

        }

        private void ItemContainer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var _temp = sender as ItemContainer;
            var _grid = _temp.Child as Grid;
            var _girdItems = _grid.Children;

            foreach (var _girdItem in _girdItems )
            {
                if ( _girdItem is CheckBox ) 
                {
                    SwitchCheckBox(_girdItem as CheckBox);
                    break;
                }
 
            }

            Debug.Print("Now_Index" + _temp.Name);
            int _index = int.Parse(_temp.Name);
            var _image = HomePageLocalFolder.ImgList[_index];
            var _zoom = Math.Min(1, Math.Min(this.ActualHeight / _image.Height, this.ActualWidth / _image.Width));
            var _newHeight = _zoom * _image.Height;
            var _newWidth = _zoom * _image.Width;
            var _offset = _temp.ActualOffset;
            var _tempX = (_offset.X - (this.ActualWidth - _newWidth) / 2);
            var _tempY = (_offset.Y - (this.ActualHeight - _newHeight) / 2 - ScrollViewer.VerticalOffset);
            var _centerPointX = (float)(_tempX * _temp.ActualWidth / (_newWidth - _temp.ActualWidth));
            var _centerPointY = (float)(_tempY * _temp.ActualHeight / (_newHeight - _temp.ActualHeight));
            float _scaleX = (float)(_newWidth / _temp.ActualWidth);
            float _scaleY = (float)(_newHeight / _temp.ActualHeight);

            Debug.Print("X " + _centerPointX + "\nY " + _centerPointY);
            //var _scaleX = (float)(_temp.ActualWidth * 1.5 / 2);
            //var _scaleY = (float)(_temp.ActualHeight * 3 / 2);

            _temp.CenterPoint = new Vector3(_centerPointX, _centerPointY, 0);
            _temp.Scale = new Vector3(_scaleX, _scaleY, 1);

            for(int i = 0; i < 244; i++)
            {
                var temp = (ItemContainer)repeater2.TryGetElement(i);
                if ( temp != null && i != _index)
                {
                    temp.Opacity = 0;
                }
                
            }

            Debug.Print(_temp.ActualHeight.ToString());


        }

        private void SwitchCheckBox(CheckBox _checkBox)
        {
            if (_checkBox.IsChecked == true)
            {
                _checkBox.IsChecked = false;
            }
            else
            {
                _checkBox.IsChecked = true;
            }
        }
    }
}
