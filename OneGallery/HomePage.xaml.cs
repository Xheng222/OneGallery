using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
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
using Windows.UI;
using Windows.UI.StartScreen;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{

    public sealed partial class HomePage : Page
    {

        private static PageParameters Parameters = new();

        private Category NowCategory;

        readonly MainWindow Window;

        string Path;

        public HomePage()
        {
            this.InitializeComponent();
            Window = (MainWindow)(Application.Current as App).m_window;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Category)
            {
                NowCategory = e.Parameter as Category;

                NavigateHelper.OnNavigatedTo(
                    Window.page,
                    NowCategory.Name,
                    o =>
                    {
                        if (o is PageParameters p)
                        {
                            Parameters = p;
                        }
                        else
                        {
                            Parameters.Clear();
                        }
                    });
            }

            await LoadData();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Parameters.Width = ScrollViewer.ActualWidth;
            Parameters.Offset = ScrollViewer.VerticalOffset;
            Parameters.FirstShow = false;
            NavigateHelper.OnNavigatingFrom(NowCategory.Name, Window.page.Content, Parameters);
            base.OnNavigatingFrom(e);
        }

        private async Task LoadData()
        {
            await Window.InitFolder(NowCategory.Name);

            if (Parameters.FirstShow == true)
            {
                repeater2.Layout = new ActivityFeedLayout(Window.FolderManager.MyImageArrangement);
            }

            repeater2.ItemsSource = Window.FolderManager.MyImageArrangement.ImgList;


        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollViewer.Height = this.ActualHeight;
        }



        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image _image = sender as Image;

        }

        private void ItemContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

            var _temp = sender as ItemContainer;
            float _scaleX = (float)(_temp.ActualWidth / 2);
            float _scaleY = (float)(_temp.ActualHeight / 2); ;

            _temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            _temp.Scale = new Vector3((float)1.02, (float)1.02, 1);

            var _grid = _temp.Child as Grid;
            var _girdItems = _grid.Children;

            Debug.Print("" + repeater2.GetElementIndex(_temp));

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

            _temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            _temp.Scale = new Vector3(1, 1, 1);

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
            _temp.ScaleTransition = new Vector3Transition()
            {
                Components = Vector3TransitionComponents.X | Vector3TransitionComponents.Y,
                Duration = TimeSpan.FromMilliseconds(150)
            };
        }

        private void ItemContainer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            var _temp = sender as ItemContainer;

            var _scaleX = (float)(_temp.ActualWidth / 2);
            var _scaleY = (float)(_temp.ActualHeight / 2);

            _temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            _temp.Scale = new Vector3((float)0.98, (float)0.98, 1);

        }

        private void ItemContainer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

            var _temp = sender as ItemContainer;
            var _ptr = e.GetCurrentPoint(_temp);
            Debug.Print("" + _ptr.Properties.PointerUpdateKind);
            if (_ptr != null)
            {
                var _index = repeater2.GetElementIndex(_temp);
                var _image = Window.FolderManager.MyImageArrangement.ImgList.First(x => x.Index == _index);

                if ((int)_ptr.Properties.PointerUpdateKind == (int)(Windows.UI.Input.PointerUpdateKind.LeftButtonReleased))
                {
                    var _grid = _temp.Child as Grid;
                    var _girdItems = _grid.Children;

                    foreach (var _girdItem in _girdItems)
                    {
                        if (_girdItem is CheckBox)
                        {
                            //SwitchCheckBox(_girdItem as CheckBox);
                            break;
                        }
                    }

                    //var _index = int.Parse(_temp.Name);
                    //var _image = Window.FolderManager.MyImageArrangement.ImgList[_index];

                    Parameters.SortedIndex = _index;


                    var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _temp);
                    anim.Configuration = new DirectConnectedAnimationConfiguration();
                    

                    ItemContainer_PointerExited(sender, e);
                    //Window.page.Navigate(typeof(ImagePage), _image, new SuppressNavigationTransitionInfo());
                    Window.page.Navigate(typeof(ImagePage), _image, new DrillInNavigationTransitionInfo());
                }
                else
                {
                    Debug.Print(repeater2.GetElementIndex(_temp) + "");

                    Window.FolderManager.DeleteImg(_image);
                }

            }

        }

        private void SwitchCheckBox(CheckBox _checkBox)
        {
            if (_checkBox.IsChecked == false)
            {
                _checkBox.IsChecked = true;
            }
            else
            {
                _checkBox.IsChecked = false;
            }
        }
    
        private async Task ConnectAnimate()
        {
            if (Parameters.SortedIndex != -1)
            {
                var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackwardConnectedAnimation");

                if (anim != null)
                {

                    var _item = repeater2.TryGetElement(Parameters.SortedIndex);
                    while (_item == null)
                    {
                        await Task.Delay(100);
                        _item = repeater2.TryGetElement(Parameters.SortedIndex);
                    }

                    anim.TryStart(_item);
              
                }
                Parameters.SortedIndex = -1;
            }

        }

        private async void repeater2_Loaded(object sender, RoutedEventArgs e)
        {
            double _offset = Parameters.Offset * ScrollViewer.ActualWidth / Parameters.Width;
            ScrollViewer.ChangeView(null, _offset, null);
            await ConnectAnimate();
        }
    }
}
