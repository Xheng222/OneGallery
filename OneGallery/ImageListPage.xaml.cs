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

    public partial class ImageListPage : Page, INotifyPropertyChanged
    {
        private static PageParameters Parameters = new();

        public static PictureClass SelectedImage {set; get; }

        public static Task InitPageTask { set; get; }

        private Category NowCategory {  get; set; }

        private MainWindow Window {  get; set; }

        public ObservableCollection<PictureClass> ImgList {  get; set; }

        public ActivityFeedLayout MyActivityFeedLayout { get; set; }


        public ImageListPage()
        {
            this.InitializeComponent();
            Window = (Application.Current as App).Main;
            NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Height = Window.Height - 176;
        }

        public void Close()
        {
            MyActivityFeedLayout = null;
            ImgList = null;
            this.UnloadObject(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
 
            if (e.Parameter is Category)
            {
                NowCategory = e.Parameter as Category;
                InitPageTask = LoadData();
                Window._nowCategory = NowCategory;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Parameters.Width = ScrollViewer.ActualWidth;
            Parameters.Offset = ScrollViewer.VerticalOffset;
            Parameters.FirstShow = false;
            NavigateHelper.StoreContent(Window.NaPage, NowCategory, Parameters);
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            GC.Collect();
        }

        private async Task LoadData()
        {
            NavigateHelper.GetParameter(
                NowCategory,
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

            if (Parameters.FirstShow == true)
            {
                ImgList = new();
                MyActivityFeedLayout = new();

                await Window.InitFolder(NowCategory);
                MyActivityFeedLayout.LayoutImgArrangement = Window.FolderManager.MyImageArrangement;
                Window.FolderManager.MyImageArrangement.ImgListForRepeater = ImgList;
                Window.FolderManager.MyImageArrangement.ImgListChanged();

                PocessingGrid.Opacity = 0;
                await Task.Delay(300);
                ScrollViewer.Opacity = 1;
                await Task.Delay(200);

                PocessingGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                await Window.InitFolder(NowCategory);
                NavigateHelper.GetContent(
                    Window,
                    Window.NaPage,
                    NowCategory
                );
            }
        }

        /*
         * ImageRepeater_Loaded
         */
        private void ImageRepeater_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectAnimate();
        }
        private async void ConnectAnimate()
        {
            if (Parameters.Image is not null)
            {
                var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackwardConnectedAnimation");
                var _index = ImgList.IndexOf(Parameters.Image);

                if (anim != null)
                {
                    UIElement _item;
                    if (_index != -1)
                    {
                        if (ScrollViewer.ActualWidth != Parameters.Width)
                        {
                            await Task.Delay(50);
                            double _offset = Parameters.Offset * ScrollViewer.ActualWidth / Parameters.Width;
                            Debug.Print(ScrollViewer.ChangeView(null, _offset, null) + "");
                        }

                        _item = ImageRepeater.TryGetElement(_index);

                        while (_item == null)
                        {
                            await Task.Delay(50);
                            _item = ImageRepeater.TryGetElement(_index);
                        }
                    }
                    else
                    {
                        _item = grid;
                    }

                    anim.TryStart(_item);
                }

                Parameters.Image = null;
            }
        }



        /*
         * ItemContainer_Loaded
         */
        private void ItemContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var _tempGrid = sender as Grid;

            foreach (var _girdItem in _tempGrid.Children)
            {
                if (_girdItem is CheckBox _checkBox)
                {
                    _checkBox.OpacityTransition = new ScalarTransition()
                    {
                        Duration = TimeSpan.FromMilliseconds(200)
                    };
                    continue;
                }

                if (_girdItem is Rectangle _rectangle)
                {
                    _rectangle.OpacityTransition = new ScalarTransition()
                    {
                        Duration = TimeSpan.FromMilliseconds(200)
                    };
                    continue;
                }
            }

        }

        /*
         * Pointer
         */
        private void ItemGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var _temp = GetItemContainer(sender as Grid);
            float _scaleX = (float)(_temp.ActualWidth / 2);
            float _scaleY = (float)(_temp.ActualHeight / 2); ;

            _temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            _temp.Scale = new Vector3((float)1.02, (float)1.02, 1);

            int _index = ImageRepeater.GetElementIndex((UIElement)sender);
            if (_index != -1)
            {
                if (MainWindow.NowSelectMode != MainWindow.SelectMode.None)
                {
                    ImgList[_index]._checkBoxOpacity = 1;
                    ImgList[_index]._rectangleOpacity = 0.8;
                }

                ImgList[_index]._borderOpacity = 1;
            }

            e.Handled = true;
        }

        private void ItemGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var _temp = GetItemContainer(sender as Grid);

            float _scaleX = (float)(_temp.ActualWidth / 2);
            float _scaleY = (float)(_temp.ActualHeight / 2);

            _temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            _temp.Scale = new Vector3(1, 1, 1);

            int _index = ImageRepeater.GetElementIndex((UIElement)sender);
            if (_index != -1)
            {
                if (!ImgList[_index].IsSelected)
                {
                    ImgList[_index]._checkBoxOpacity = 0;
                    ImgList[_index]._borderOpacity = 0;
                    ImgList[_index]._rectangleOpacity = 0;
                }
            }

            e.Handled = true;
        }

        private void ItemGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var _temp = GetItemContainer(sender as Grid);
            var _scaleX = (float)(_temp.ActualWidth / 2);
            var _scaleY = (float)(_temp.ActualHeight / 2);
            _temp.CenterPoint = new Vector3(_scaleX, _scaleY, 0);
            _temp.Scale = new Vector3((float)0.98, (float)0.98, 1);
            e.Handled = true;
        }

        private void ItemGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var _temp = GetItemContainer(sender as Grid);
            var _ptr = e.GetCurrentPoint(_temp);

            if (_ptr != null)
            {
                var _index = ImageRepeater.GetElementIndex(sender as Grid);
                var _image = ImgList[_index];

                if ((int)_ptr.Properties.PointerUpdateKind == (int)Windows.UI.Input.PointerUpdateKind.RightButtonReleased)
                {
                    Parameters.Image = _image;

                    var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", (Grid)sender);
                    anim.Configuration = new DirectConnectedAnimationConfiguration();
                    Window.NaPage.Navigate(typeof(ImagePage), _image, new DrillInNavigationTransitionInfo());
                    if (!_image.IsSelected)
                    {
                        _image._checkBoxOpacity = 0;
                        _image._borderOpacity = 0;
                        _image._rectangleOpacity = 0;
                    }
                }
                else if ((int)_ptr.Properties.PointerUpdateKind == (int)Windows.UI.Input.PointerUpdateKind.LeftButtonReleased)
                {
                    if (MainWindow.NowSelectMode != MainWindow.SelectMode.None)
                    {
                        if (_image.IsSelected)
                        {
                            _image._isSelected = false;
                            Window._selectedCount--;
                            SelectedImage = null;
                        }
                            
                        else
                        {
                            _image._isSelected = true;
                            
                            if (MainWindow.NowSelectMode == MainWindow.SelectMode.Single)
                                UnSelectLastImage(_image);
                            else
                                Window._selectedCount++;
                        }


                    }

                    ItemGrid_PointerEntered(sender, e);
                }
            }
            e.Handled = true;
        }

        private static Grid GetItemContainer(Grid _grid)
        {
            Grid _temp = null;
            foreach (var _ui in _grid.Children)
            {
                if (_ui is Grid)
                {
                    _temp = _ui as Grid;
                    break;
                }
            }

            return _temp;
        }

        private async void UnSelectLastImage(PictureClass _image)
        {
            if (SelectedImage == null)
            {
                SelectedImage = _image;
                Window._selectedCount++;
            }
            else
            {
                SelectedImage._checkBoxOpacity = 0; 
                SelectedImage._rectangleOpacity = 0;
                SelectedImage._borderOpacity = 0;
                await Task.Delay(200);
                SelectedImage._isSelected = false;
                SelectedImage = _image;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ImageListChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImgList)));
        }


    }
}
