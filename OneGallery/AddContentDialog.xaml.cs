using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{
    public sealed partial class AddContentDialog : ContentDialog
    {
        public enum ContentDialogResult
        {
            ConfirmDelete,
            AddCancel,
            Nothing
        }

        public enum Mode
        {
            AddFolderMode,
            ChangeFolderMode,
            DeleteFolderMode,
            AddGalleryMode,
            ChangeGalleryMode,
            DeleteGalleryMode,
            DeleteImageMode,
        }

        public Mode ShowMode {  get; set; }

        public ContentDialogResult Result { get; private set; }

        public string AddFolderPath { get; set; }

        private string NowName { get; set; }

        private string NowPath { get; set; }

        private ObservableCollection<object> NowFolders { get; set; }

        private ObservableCollection<object> AddFolders { get; set; }

        public AddContentDialog(Mode _mode)
        {
            ShowMode = _mode;
            Init();
        }

        public AddContentDialog(string _nowName, Mode _mode)
        {
            ShowMode = _mode;
            NowName = _nowName;
            Init();
        }

        public AddContentDialog(string _nowName, ObservableCollection<object> _folders, Mode _mode)
        {
            ShowMode= _mode;
            NowName = _nowName;
            AddFolders = _folders;
            Init();
        }

        public AddContentDialog(string _nowName, string _nowPath, Mode _mode)
        {
            ShowMode = _mode;
            NowName = _nowName;
            NowPath = _nowPath;
            Init();
        }

        private void Init()
        {
            this.InitializeComponent();

            switch (ShowMode)
            {
                case Mode.AddFolderMode:
                    {
                        DeleteFolderImage.Visibility = Visibility.Collapsed;
                        DeleteFolderTextBlock.Visibility = Visibility.Collapsed;

                        NowFoldersTextBlock.Visibility = Visibility.Collapsed;
                        NowFoldersViewer.Visibility = Visibility.Collapsed;
                        AddFoldersTextBlock.Visibility = Visibility.Collapsed;
                        AddFoldersViewer.Visibility = Visibility.Collapsed;
                        GalleryWrongInfo.Visibility = Visibility.Collapsed;

                        Title = "添加文件夹";
                        PrimaryButtonText = "添加";
                        CloseButtonText = "取消";
                        NameTextBox.Header = "   文件夹名";
                        this.PrimaryButtonStyle = (Style)this.Resources["StyleButton"];
                        this.CloseButtonStyle = (Style)this.Resources["StyleButton"];
                        break;
                    }

                case Mode.ChangeFolderMode:
                    {
                        DeleteFolderImage.Visibility = Visibility.Collapsed;
                        DeleteFolderTextBlock.Visibility = Visibility.Collapsed;

                        NowFoldersTextBlock.Visibility = Visibility.Collapsed;
                        NowFoldersViewer.Visibility = Visibility.Collapsed;
                        AddFoldersTextBlock.Visibility = Visibility.Collapsed;
                        AddFoldersViewer.Visibility = Visibility.Collapsed;
                        GalleryWrongInfo.Visibility = Visibility.Collapsed;

                        Title = "修改文件夹";
                        PrimaryButtonText = "修改";
                        CloseButtonText = "取消";
                        FolderPath.Text = "选择文件夹: " + NowPath;
                        NameTextBox.Header = "   文件夹名";
                        FolderPath.Height = 24;
                        NameTextBox.Text = NowName;
                        this.PrimaryButtonStyle = (Style)this.Resources["StyleButton"];
                        this.CloseButtonStyle = (Style)this.Resources["StyleButton"];
                        break;
                    }
                
                
                case Mode.DeleteFolderMode:
                    {
                        NameTextBox.Visibility = Visibility.Collapsed;
                        NameWrongInfo.Visibility = Visibility.Collapsed;
                        FolderPicker.Visibility = Visibility.Collapsed;
                        FolderPath.Visibility = Visibility.Collapsed;
                        FolderWrongInfo.Visibility = Visibility.Collapsed;

                        NowFoldersTextBlock.Visibility = Visibility.Collapsed;
                        NowFoldersViewer.Visibility = Visibility.Collapsed;
                        AddFoldersTextBlock.Visibility = Visibility.Collapsed;
                        AddFoldersViewer.Visibility = Visibility.Collapsed;
                        GalleryWrongInfo.Visibility = Visibility.Collapsed;

                        DeleteFolderImage.Source = new SvgImageSource(new Uri("ms-appx:///Images/Folder_delete.svg"));
                        Title = "删除文件夹？";
                        PrimaryButtonText = "删除";
                        CloseButtonText = "取消";
                        DeleteFolderTextBlock.Text = "注意，这并不会删除本地的文件夹与图片";
                        this.PrimaryButtonStyle = (Style)this.Resources["StyleButtonDelete"];
                        this.CloseButtonStyle = (Style)this.Resources["StyleButton"];
                        break;
                    }

                case Mode.AddGalleryMode:
                    {
                        DeleteFolderImage.Visibility = Visibility.Collapsed;
                        DeleteFolderTextBlock.Visibility = Visibility.Collapsed;

                        FolderPicker.Visibility = Visibility.Collapsed;
                        FolderPath.Visibility = Visibility.Collapsed;
                        FolderWrongInfo.Visibility = Visibility.Collapsed;

                        NowFolders = new((MainWindow.Window.Categories[5] as Category).Children);
                        AddFolders = new();
                        if (NowFolders.Count != 0 && NowFolders.Last() != null )
                            if ((NowFolders.Last() as Category).IsAddSelection)
                                NowFolders.RemoveAt(NowFolders.Count - 1);

                        ChangeHeight(0);

                        NowFoldersViewerRepeater.ItemsSource = NowFolders;
                        AddFoldersViewerRepeater.ItemsSource = AddFolders;

                        Title = "创建画廊";
                        NameTextBox.Header = "   画廊名";
                        PrimaryButtonText = "创建";
                        CloseButtonText = "取消";

                        this.PrimaryButtonStyle = (Style)this.Resources["StyleButton"];
                        this.CloseButtonStyle = (Style)this.Resources["StyleButton"];
                        break;
                    }

                case Mode.ChangeGalleryMode:
                    {
                        DeleteFolderImage.Visibility = Visibility.Collapsed;
                        DeleteFolderTextBlock.Visibility = Visibility.Collapsed;

                        FolderPicker.Visibility = Visibility.Collapsed;
                        FolderPath.Visibility = Visibility.Collapsed;
                        FolderWrongInfo.Visibility = Visibility.Collapsed;

                        NowFolders = new((MainWindow.Window.Categories[5] as Category).Children);
                        
                        if (NowFolders.Last() != null)
                            if ((NowFolders.Last() as Category).IsAddSelection)
                                NowFolders.RemoveAt(NowFolders.Count - 1);

                        foreach (var Folder in AddFolders)
                            NowFolders.Remove(Folder);
                        
                        ChangeHeight(0);

                        NowFoldersViewerRepeater.ItemsSource = NowFolders;
                        AddFoldersViewerRepeater.ItemsSource = AddFolders;

                        Title = "修改画廊";
                        NameTextBox.Header = "   画廊名";
                        NameTextBox.Text = NowName;
                        PrimaryButtonText = "修改";
                        CloseButtonText = "取消";

                        this.PrimaryButtonStyle = (Style)this.Resources["StyleButton"];
                        this.CloseButtonStyle = (Style)this.Resources["StyleButton"];
                        break;
                    }

                case Mode.DeleteGalleryMode:
                    {
                        NameTextBox.Visibility = Visibility.Collapsed;
                        NameWrongInfo.Visibility = Visibility.Collapsed;
                        FolderPicker.Visibility = Visibility.Collapsed;
                        FolderPath.Visibility = Visibility.Collapsed;
                        FolderWrongInfo.Visibility = Visibility.Collapsed;

                        NowFoldersTextBlock.Visibility = Visibility.Collapsed;
                        NowFoldersViewer.Visibility = Visibility.Collapsed;
                        AddFoldersTextBlock.Visibility = Visibility.Collapsed;
                        AddFoldersViewer.Visibility = Visibility.Collapsed;
                        GalleryWrongInfo.Visibility = Visibility.Collapsed;

                        //DeleteFolderImage.Source = new BitmapImage(new Uri("/Images/Tag_off.svg"));
                        DeleteFolderImage.Source = new SvgImageSource(new Uri("ms-appx:///Images/Tag_off.svg"));


                        Title = "删除画廊？";
                        PrimaryButtonText = "删除";
                        CloseButtonText = "取消";
                        DeleteFolderTextBlock.Text = "注意，这并不会删除本地的文件夹与图片";
                        this.PrimaryButtonStyle = (Style)this.Resources["StyleButtonDelete"];
                        this.CloseButtonStyle = (Style)this.Resources["StyleButton"];
                        break;
                    }

                case Mode.DeleteImageMode:
                    {
                        NameTextBox.Visibility = Visibility.Collapsed;
                        NameWrongInfo.Visibility = Visibility.Collapsed;
                        FolderPicker.Visibility = Visibility.Collapsed;
                        FolderPath.Visibility = Visibility.Collapsed;
                        FolderWrongInfo.Visibility = Visibility.Collapsed;

                        NowFoldersTextBlock.Visibility = Visibility.Collapsed;
                        NowFoldersViewer.Visibility = Visibility.Collapsed;
                        AddFoldersTextBlock.Visibility = Visibility.Collapsed;
                        AddFoldersViewer.Visibility = Visibility.Collapsed;
                        GalleryWrongInfo.Visibility = Visibility.Collapsed;

                        if (MainWindow.Window._selectedCount == 1)
                            DeleteFolderImage.Source = new SvgImageSource(new Uri("ms-appx:///Images/Image_off.svg"));
                        else
                            DeleteFolderImage.Source = new SvgImageSource(new Uri("ms-appx:///Images/Image_multiple_off.svg"));
                        Title = "删除图片？";
                        PrimaryButtonText = "删除";
                        CloseButtonText = "取消";
                        DeleteFolderTextBlock.Text = "注意，这*会*删除本地的图片";
                        this.PrimaryButtonStyle = (Style)this.Resources["StyleButtonDelete"];
                        this.CloseButtonStyle = (Style)this.Resources["StyleButton"];
                        break;
                    }

                default:
                    break;
            }

            AcrylicBrush myBrush = new()
            {
                TintColor = Color.FromArgb(255, 255, 255, 255),
                FallbackColor = Color.FromArgb(255, 255, 255, 255)
            };
            this.Background = myBrush;
        }

        private void ChangeHeight(int __delta)
        {
            double _windowHeight = (700 - MainWindow.Window.Height > 0)? (700 - MainWindow.Window.Height): -40;
            int _maxHeight = 400 - (int)(_windowHeight / 40 + 1) * 40 - __delta * 40;

            int _nowFolderHeight = (NowFolders.Count + 1) / 2 * 40;
            int _addFolderHeight = (AddFolders.Count + 1) / 2 * 40;

            while (_nowFolderHeight + _addFolderHeight > _maxHeight)
            {
                if (_nowFolderHeight > _addFolderHeight)
                {
                    _nowFolderHeight -= 40;
                }
                else if (_addFolderHeight > _nowFolderHeight)
                {
                    _addFolderHeight -= 40;
                } 
                else
                {
                    _nowFolderHeight = _addFolderHeight -= 40;
                }
            }

            NowFoldersViewer.Height = _nowFolderHeight;
            AddFoldersViewer.Height = _addFolderHeight;
        }


        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            switch (ShowMode)
            {
                case Mode.AddFolderMode:
                    {
                        if (NameTextBox.Text == string.Empty)
                        {
                            NameWrongInfo.Message = "未输入文件夹名";
                            NameWrongInfo.IsOpen = true;
                        }
                        else if (!MainWindow.Window.CheckAddFolderOrGalleryName(NameTextBox.Text))
                        {
                            NameWrongInfo.Message = "文件夹名重复";
                            NameWrongInfo.IsOpen = true;
                        }

                        if (AddFolderPath is null)
                        {
                            FolderWrongInfo.Message = "未选择文件夹";
                            FolderWrongInfo.IsOpen = true;
                        }
                        else if (!MainWindow.Window.CheckAddFolderPath(AddFolderPath))
                        {
                            FolderWrongInfo.Message = "文件夹路径重复";
                            FolderWrongInfo.IsOpen = true;
                        }

                        if (NameWrongInfo.IsOpen || FolderWrongInfo.IsOpen)
                            args.Cancel = true;

                        if (args.Cancel == false)
                        {
                            ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                            MainWindow.Window.AddFolderOrGallery(NameTextBox.Text, AddFolderPath);
                            deferral.Complete();
                        }
                        break;
                    }


                case Mode.ChangeFolderMode:
                    {
                        if (NameTextBox.Text == string.Empty)
                        {
                            NameWrongInfo.Message = "未输入文件夹名";
                            NameWrongInfo.IsOpen = true;
                        }
                        else if (NowName != NameTextBox.Text)
                        {
                            if (!MainWindow.Window.CheckAddFolderOrGalleryName(NameTextBox.Text))
                            {
                                NameWrongInfo.Message = "文件夹名重复";
                                NameWrongInfo.IsOpen = true;
                            }
                        }

                        if (AddFolderPath is not null)
                        {
                            if (AddFolderPath != NowPath)
                            {
                                if (!MainWindow.Window.CheckAddFolderPath(AddFolderPath))
                                {
                                    FolderWrongInfo.Message = "文件夹路径重复";
                                    FolderWrongInfo.IsOpen = true;
                                }
                            }
                        }

                        if (NameWrongInfo.IsOpen || FolderWrongInfo.IsOpen)
                            args.Cancel = true;

                        if (args.Cancel == false)
                        {
                            ContentDialogButtonClickDeferral deferral = args.GetDeferral();

                            if (AddFolderPath is null || AddFolderPath == NowPath)
                            {
                                MainWindow.Window.ResetFolder(NowName, NameTextBox.Text);
                            }
                            else
                            {
                                await MainWindow.Window.ResetFolder(NowName, NameTextBox.Text, AddFolderPath);
                            }

                            deferral.Complete();
                        }

                        break;
                    }
                case Mode.DeleteFolderMode:
                    {
                        ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                        MainWindow.Window.DeleteFolder(NowName);
                        this.Result = ContentDialogResult.ConfirmDelete;
                        deferral.Complete();
                        break;
                    }

                case Mode.AddGalleryMode:
                    {
                        int _delta = 0;

                        if (NameTextBox.Text == string.Empty)
                        {
                            NameWrongInfo.Message = "未输入画廊名";
                            NameWrongInfo.IsOpen = true;
                            _delta++;
                        }
                        else if (!MainWindow.Window.CheckAddFolderOrGalleryName(NameTextBox.Text))
                        {
                            NameWrongInfo.Message = "画廊名重复";
                            NameWrongInfo.IsOpen = true;
                            _delta++;
                        }

                        if (AddFolders.Count == 0)
                        {
                            GalleryWrongInfo.Message = "未选择文件夹";
                            GalleryWrongInfo.IsOpen = true;
                            _delta++;
                        }

                        ChangeHeight(_delta);

                        if (NameWrongInfo.IsOpen || GalleryWrongInfo.IsOpen)
                            args.Cancel = true;

                        if (args.Cancel == false)
                        {
                            ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                            MainWindow.Window.AddFolderOrGallery(NameTextBox.Text, AddFolders.ToList());
                            deferral.Complete();
                        }

                        break;
                    }

                case Mode.ChangeGalleryMode:
                    {
                        int _delta = 0;

                        if (NameTextBox.Text == string.Empty)
                        {
                            NameWrongInfo.Message = "未输入画廊名";
                            NameWrongInfo.IsOpen = true;
                            _delta++;
                        }
                        else if(NowName != NameTextBox.Text)
                        {
                            if (!MainWindow.Window.CheckAddFolderOrGalleryName(NameTextBox.Text))
                            {
                                NameWrongInfo.Message = "画廊名重复";
                                NameWrongInfo.IsOpen = true;
                                _delta++;
                            }
                        }

                        if (AddFolders.Count == 0)
                        {
                            GalleryWrongInfo.Message = "未选择文件夹";
                            GalleryWrongInfo.IsOpen = true;
                            _delta++;
                        }

                        ChangeHeight(_delta);

                        if (NameWrongInfo.IsOpen || GalleryWrongInfo.IsOpen)
                            args.Cancel = true;

                        if (args.Cancel == false)
                        {
                            ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                            var _oldFolders = MainWindow.Window.MyPathConfig.GalleryToFolderListConfig[NowName];
                            var _newFolder = new List<string>();

                            foreach ( var _item in AddFolders)
                            {
                                if (_item is Category _folder)
                                {
                                    _newFolder.Add(_folder._name);
                                }
                            }

                            if (_oldFolders.Count != _newFolder.Count)
                            {
                                MainWindow.Window.ResetGallery(NowName, NameTextBox.Text, _newFolder);
                            }
                            else
                            {
                                bool _flag = true;
                                foreach ( var _item in _newFolder)
                                {
                                    if (!_oldFolders.Contains(_item))
                                    {
                                        _flag = false;
                                        break;
                                    }
                                }

                                if (_flag)
                                {
                                    MainWindow.Window.ResetGallery(NowName, NameTextBox.Text);
                                }
                                else
                                {
                                    MainWindow.Window.ResetGallery(NowName, NameTextBox.Text, _newFolder);
                                }
                            }


                            deferral.Complete();
                        }
                        break;
                    }

                case Mode.DeleteGalleryMode:
                    {
                        ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                        MainWindow.Window.DeleteGallery(NowName);
                        this.Result = ContentDialogResult.ConfirmDelete;
                        deferral.Complete();
                        break;
                    }

                case Mode.DeleteImageMode:
                    {
                        ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                        this.Result = ContentDialogResult.ConfirmDelete;
                        deferral.Complete();
                        break;
                    }


                default:
                    break;
            }

        }


        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Result = ContentDialogResult.AddCancel;
        }

        private void FolderNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Clear the error if the user name field isn't empty.
            if (!string.IsNullOrEmpty(NameTextBox.Text))
            {
                if (NameWrongInfo.IsOpen)
                    NameWrongInfo.IsOpen = false;

                if (ShowMode == Mode.AddGalleryMode || ShowMode == Mode.ChangeGalleryMode)
                    if (GalleryWrongInfo.IsOpen)
                    {
                        GalleryWrongInfo.IsOpen = false;
                        ChangeHeight(0);
                    }       
            }
        }

        /*
         * AddFolder
         */

        private async void FolderPicker_Click(object sender, RoutedEventArgs e)
        {
            FolderWrongInfo.Message = string.Empty;
            FolderWrongInfo.IsOpen = false;

            FolderPicker _folderPicker = new();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(_folderPicker, hWnd);
            _folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            _folderPicker.FileTypeFilter.Add("*");
            _folderPicker.FileTypeFilter.Add(".jpg");
            _folderPicker.FileTypeFilter.Add(".png");
            StorageFolder folder = await _folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                FolderPath.Text = "选择文件夹: " + folder.Path;
                FolderPath.Height = 24;
                AddFolderPath = folder.Path;
            }
            else
            {
                if (ShowMode == Mode.ChangeFolderMode)
                {
                    FolderPath.Height = 0;
                }

                AddFolderPath = null;
            }

        }

        /*
         * AddGallery
         */

        private void FolderAddToGallery_Click(object sender, RoutedEventArgs e)
        {
            var _index = NowFoldersViewerRepeater.GetElementIndex((UIElement)sender);
            var _temp = NowFolders[_index];
            AddFolders.Insert(0, _temp);
            NowFolders.RemoveAt(_index);
            if (NameWrongInfo.IsOpen)
            {
                NameWrongInfo.IsOpen = false;
            }
            if (GalleryWrongInfo.IsOpen)
            {
                GalleryWrongInfo.IsOpen = false;
            }
            ChangeHeight(0);
        }

        private void FolderRemoveFromGallery_Click(object sender, RoutedEventArgs e)
        {
            var _index = AddFoldersViewerRepeater.GetElementIndex((UIElement)sender);
            var _temp = AddFolders[_index];
            NowFolders.Insert(0, _temp);
            AddFolders.RemoveAt(_index);
            if (NameWrongInfo.IsOpen)
            {
                NameWrongInfo.IsOpen = false;
            }
            if (GalleryWrongInfo.IsOpen)
            {
                GalleryWrongInfo.IsOpen = false;
            }
            ChangeHeight(0);
        }
    }
}
