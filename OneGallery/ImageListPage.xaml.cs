using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{

    public partial class ImageListPage : Page
    {
        private static PageParameters Parameters = new();

        public static PictureClass SelectedImage {set; get; }

        public static Task InitPageTask { set; get; }

        private Category NowCategory {  get; set; }

        public ObservableCollection<PictureClass> ImgList {  get; set; }

        public ActivityFeedLayout MyActivityFeedLayout { get; set; }


        public ImageListPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }


        public void Close()
        {
            MyActivityFeedLayout = null;
            ImgList.CollectionChanged -= OnCollectionChanged;
            ImgList = null;
            this.UnloadObject(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
 
            if (e.Parameter is Category)
            {
                NowCategory = e.Parameter as Category;
                LoadData();
                MainWindow.Window._nowCategory = NowCategory;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ImgList != null && MyActivityFeedLayout != null)
            {
                Parameters.Width = ScrollViewer.ActualWidth;
                Parameters.Offset = ScrollViewer.VerticalOffset;
                Parameters.FirstShow = false;
                NavigateHelper.StoreContent(NowCategory, Parameters);
            }

            tokenSource.Cancel();
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            GC.Collect();
        }

        private void LoadData()
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

            tokenSource = new CancellationTokenSource();

            InitPageTask = InitImageList();
        }

        CancellationTokenSource tokenSource;

        private async Task InitImageList()
        {
            if (Parameters.FirstShow == true)
            {
                ImgList = new();
                MyActivityFeedLayout = new();
                await MainWindow.Window.InitFolder(NowCategory);

                if (!tokenSource.IsCancellationRequested)
                {
                    MyActivityFeedLayout.LayoutImgArrangement = MainWindow.Window.FolderManager.MyImageArrangement;
                    MainWindow.Window.FolderManager.MyImageArrangement.ImgListForRepeater = ImgList;
                    MainWindow.Window.FolderManager.MyImageArrangement.ImgListChanged();
                }

                PocessingGrid.Opacity = 0;
                await Task.Delay(300);

                if (ImgList.Count == 0)
                {
                    EmptyGrid.Opacity = 1;
                    ScrollViewer.Opacity = 0;
                }
                else
                {
                    EmptyGrid.Opacity = 0;
                    ScrollViewer.Opacity = 1;
                }

                await Task.Delay(200);
                PocessingGrid.Visibility = Visibility.Collapsed;

                ImgList.CollectionChanged += OnCollectionChanged;
            }

            else
            {
                await MainWindow.Window.InitFolder(NowCategory);

                if (!tokenSource.IsCancellationRequested)
                {
                    NavigateHelper.GetContent(
                        NowCategory
                    );
                }
            }
        }


        /*
         * Grid
         */

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var _grid = sender as Grid;

            _grid.PointerEntered += ItemGrid_PointerEntered;
            _grid.PointerExited += ItemGrid_PointerExited;
            _grid.PointerPressed += ItemGrid_PointerPressed;
            _grid.PointerReleased += ItemGrid_PointerReleased;
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            var _grid = sender as Grid;

            _grid.PointerEntered -= ItemGrid_PointerEntered;
            _grid.PointerExited -= ItemGrid_PointerExited;
            _grid.PointerPressed -= ItemGrid_PointerPressed;
            _grid.PointerReleased -= ItemGrid_PointerReleased;
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
                    MainWindow.Window.NaPage.Navigate(typeof(ImagePage), _image, new DrillInNavigationTransitionInfo());
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
                            MainWindow.Window._selectedCount--;
                            SelectedImage = null;
                        }
                            
                        else
                        {
                            _image._isSelected = true;
                            
                            if (MainWindow.NowSelectMode == MainWindow.SelectMode.Single)
                                UnSelectLastImage(_image);
                            else
                                MainWindow.Window._selectedCount++;
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

        private static void UnSelectLastImage(PictureClass _image)
        {
            if (SelectedImage == null)
            {
                SelectedImage = _image;
                MainWindow.Window._selectedCount++;
            }
            else
            {
                var _last = SelectedImage;

                _last._checkBoxOpacity = 0;
                _last._rectangleOpacity = 0;
                _last._borderOpacity = 0;
                SelectedImage = _image;
                _last._isSelected = false;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MainWindow.Window.NowCategory == NowCategory)
            {
                MainWindow.Window._imageCount = ImgList.Count;
                if (ImgList.Count == 0)
                {
                    EmptyGrid.Opacity = 1;
                    ScrollViewer.Opacity = 0;
                }
                else
                {
                    EmptyGrid.Opacity = 0;
                    ScrollViewer.Opacity = 1;
                }

            }
        }
    }
}
