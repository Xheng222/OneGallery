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
using Windows.UI.StartScreen;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{

    public sealed partial class HomePage : Page
    {
        private LocalFolder HomePageLocalFolder {  get; set; }

        private static PageParameters Parameters = new PageParameters(-1);

        private ImageArrangement HomePageImageArrangement {  get; set; }

        private Category NowCategory;

        readonly MainWindow Window;

        string Path;

        public HomePage()
        {
            this.InitializeComponent();
            Window = (MainWindow)(Application.Current as App).m_window;
            HomePageLocalFolder = Window.FolderManager;
            HomePageImageArrangement = HomePageLocalFolder.MyImageArrangement;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
          
            base.OnNavigatedTo(e);
            Debug.Print("OnNavigatedTo");
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
                    });
            }


            await LoadData();
            //await Window.InitFolder();
            repeater2.ItemsSource = HomePageLocalFolder.ImgList;




        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Parameters.Width = ScrollViewer.ActualWidth;
            Parameters.Offset = ScrollViewer.VerticalOffset;
            NavigateHelper.OnNavigatingFrom(NowCategory.Name, Window.page.Content, Parameters);
            base.OnNavigatingFrom(e);
        }

        private async Task LoadData()
        {
            await Window.InitFolder();
            repeater2.ItemsSource = HomePageLocalFolder.ImgList;

        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //SwappableLayoutsItemsView.Height = this.ActualHeight;
            ScrollViewer.Height = this.ActualHeight;
        }




        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image _image = sender as Image;

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
            
        }

        private void Image_GotFocus(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;

            //Debug.Print("Image_GotFocus " + image.Name);
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

                    var _index = int.Parse(_temp.Name);
                    var _image = HomePageLocalFolder.ImgList[_index];
                    Parameters.SortedIndex = _index;
                    var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _temp);
                    anim.Configuration = new DirectConnectedAnimationConfiguration();

                    Window.page.Navigate(typeof(ImagePage), _image, new DrillInNavigationTransitionInfo());
                    ItemContainer_PointerExited(sender, e);
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

        private async void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            double _offset = Parameters.Offset * ScrollViewer.ActualWidth / Parameters.Width;
            ScrollViewer.ChangeView(null, _offset, null);
            await ConnectAnimate();
        }
    
        private async Task ConnectAnimate()
        {
            if (Parameters.SortedIndex != -1)
            {
                var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackwardConnectedAnimation");

                if (anim != null)
                {
                    //while (repeater2.TryGetElement(1) is null)
                    //{
                    //    await Task.Delay(50);
                    //}
                    var _item = repeater2.TryGetElement(Parameters.SortedIndex);
                    while (_item == null)
                    {
                        await Task.Delay(100);
                        _item = repeater2.TryGetElement(Parameters.SortedIndex);
                    }

                        anim.TryStart(_item);

                    //var a = anim.TryStart(repeater2.GetOrCreateElement(Parameters.SortedIndex));
                    //Debug.Print(a + "");
                    Parameters.SortedIndex = -1;
                }

            }
        }
        
    
    }
}
