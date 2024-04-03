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

    internal sealed partial class ImageListPage : Page
    {

        private static PageParameters Parameters = new();

        private Category NowCategory {  get; set; }

        private MainWindow Window {  get; set; } 

        public SortableObservableCollection<PictureClass> ImgList {  get; set; }

        public ActivityFeedLayout MyActivityFeedLayout = new();

        public ImageListPage()
        {
            this.InitializeComponent();
            Window = (MainWindow)(Application.Current as App).m_window;
            NavigationCacheMode = NavigationCacheMode.Disabled;
            ImgList = new();
        }

        ~ImageListPage()
        {
            Debug.Print("~" + NowCategory.Name + " " + ImgList.Count);

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
 
            if (e.Parameter is Category)
            {
                NowCategory = e.Parameter as Category;

                await LoadData();
            }


        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //Parameters.Width = ScrollViewer.ActualWidth;
            //Parameters.Offset = ScrollViewer.VerticalOffset;
            Parameters.FirstShow = false;
            //Parameters.ActivityFeedLayout = (ActivityFeedLayout)repeater2.Layout;
            NavigateHelper.OnNavigatingFrom(Window.NaPage, NowCategory.Name, Parameters);

            //Window.FolderManager.MyImageArrangement.ImgListForRepeater.Clear();
            //Window.FolderManager.MyImageArrangement.ImgListForRepeater = null;
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            GC.Collect();
        }


        private async Task LoadData()
        {
            await Window.InitFolder(NowCategory.Name);

            NavigateHelper.OnNavigatedTo(
                Window,
                Window.NaPage,
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
                }
            );

            //Window.FolderManager.MyImageArrangement.ImgListSwitch();
            //repeater2.ItemsSource = Window.FolderManager.MyImageArrangement.ImgList;




            //repeater2.ItemsSource = ImgListForRepeater;
            Debug.Print(ImgList.Count + " first");

            if (Parameters.FirstShow == true)
            {
                MyActivityFeedLayout.LayoutImgArrangement = Window.FolderManager.MyImageArrangement;

                Window.FolderManager.MyImageArrangement.ImgListForRepeater = ImgList;

                Debug.Print(ImgList.Count + " first");

                Needload = true;
            }
            else
            {
                //Debug.Print(((SortableObservableCollection<PictureClass>)repeater2.ItemsSource).Count + "");
            }  
            
            Window.FolderManager.MyImageArrangement.ImgListChangedEvent();
        }

        bool Needload = false;

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ScrollViewer.Height = this.ActualHeight;
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

            var index = repeater2.GetElementIndex(_temp);
            Debug.Print("" + index);
            Debug.Print("" + Window.FolderManager.MyImageArrangement.ImgList[index].ImageLocation);

            foreach (var _girdItem in _girdItems)
            {
                if (_girdItem is CheckBox)
                {
                    if ((_girdItem as CheckBox).IsChecked == false)
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

            var _index = repeater2.GetElementIndex(_temp);
            //Debug.Print("Loaded " + _index);
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
                var _image = Window.FolderManager.MyImageArrangement.ImgListForRepeater.First(x => x.Index == _index);

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
                    Window.NaPage.Navigate(typeof(ImagePage), _image, new DrillInNavigationTransitionInfo());
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
            if (Needload)
            {
                //ActivityFeedLayout.LayoutImgArrangement = Window.FolderManager.MyImageArrangement;

                //Window.FolderManager.MyImageArrangement.ImgListForRepeater = ImgList;

                //Window.FolderManager.MyImageArrangement.ImgListSwitch();


                Debug.Print(((SortableObservableCollection<PictureClass>)repeater2.ItemsSource).Count + " count");
            }


            double _offset = Parameters.Offset * ScrollViewer.ActualWidth / Parameters.Width;
            Debug.Print(ScrollViewer.ChangeView(null, _offset, null) + "");

            await ConnectAnimate();
        }


        private void ItemContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            //var _temp = (ItemContainer)sender;
            //var _index = repeater2.GetElementIndex(_temp);
            //Debug.Print("Unloaded " + _index);
        }

    }
}