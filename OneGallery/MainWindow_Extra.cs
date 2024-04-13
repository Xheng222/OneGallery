using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using WinUIEx;

namespace OneGallery
{
    public partial class MainWindow : WindowEx, INotifyPropertyChanged
    {

        private void InitConfigs()
        {
            NowSelectMode = SelectMode.Single;
            NowImageSizeMode = ImageSizeMode.Medium;

            FolderManager.MyImageArrangement.SetImgSize(
                250,
                Width,
                12, 12
            );

            NowSortMode = SortMode.ShootDate;
            IsAscending = false;




            SwitchImageSizeMode();
            SwitchSortMode();
            SwitchAscending();
            SwitchSelectMode();
        }

        /*
         * Settings
         */

        public enum SelectMode
        {
            Single,
            Multiple,
            None,
        }

        public static SelectMode NowSelectMode { set; get; }

        private async void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("SelectAll_Click");


            if (NowCategory.IsGallery || NowCategory.IsFolder)
            {
                NowSelectMode = SelectMode.Multiple;
                _selectedCount = FolderManager.MyImageArrangement.ImgList.Count;

                foreach (var _image in FolderManager.MyImageArrangement.ImgList)
                {
                    if (!_image.IsSelected)
                    {
                        _image._checkBoxOpacity = 1;
                        _image._borderOpacity = 1;
                        _image._rectangleOpacity = 0.8;
                        _image._isSelected = true;
                    }
                }

                SwitchSelectMode();
            }

            await Task.Delay(150);

            if (MoreFlyout.IsOpen)
                MoreFlyout.Hide();
        }

        private async void UnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("UnSelectAll_Click");

            if (NowCategory.IsGallery || NowCategory.IsFolder)
            {
                ClearAllSelect();
            }

            await Task.Delay(150);

            if (MoreFlyout.IsOpen)
                MoreFlyout.Hide();
        }

        public enum ImageSizeMode
        {
            Big,
            Medium,
            Small,
        }

        public static ImageSizeMode NowImageSizeMode { set; get; }

        private async void ImageSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("ImageSizeSmall_Click");

            if (NowImageSizeMode != ImageSizeMode.Small)
            {
                NowImageSizeMode = ImageSizeMode.Small;
                SwitchImageSizeMode();
                FolderManager.MyImageArrangement.SetImgSize(125, Width);
                ((ImageListPage)Nv_page.Content).MyActivityFeedLayout.MyInvalidateMeasure();
                
                await Task.Delay(400);
                if (ImageSizeFlyout.IsOpen)
                    ImageSizeFlyout.Hide();

            }
        }

        private async void ImageSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("ImageSizeMedium_Click");

            if (NowImageSizeMode != ImageSizeMode.Medium)
            {
                NowImageSizeMode = ImageSizeMode.Medium;
                SwitchImageSizeMode();
                FolderManager.MyImageArrangement.SetImgSize(250, Width);
                ((ImageListPage)Nv_page.Content).MyActivityFeedLayout.MyInvalidateMeasure();

                await Task.Delay(400);
                if (ImageSizeFlyout.IsOpen)
                    ImageSizeFlyout.Hide();
            }
        }

        private async void ImageSizeBig_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("ImageSizeBig_Click");

            if (NowImageSizeMode != ImageSizeMode.Big)
            {
                NowImageSizeMode = ImageSizeMode.Big;
                FolderManager.MyImageArrangement.SetImgSize(400, Width);
                SwitchImageSizeMode();

                ((ImageListPage)Nv_page.Content).MyActivityFeedLayout.MyInvalidateMeasure();

                await Task.Delay(400);
                if (ImageSizeFlyout.IsOpen)
                    ImageSizeFlyout.Hide();
            }

        }

        private void SwitchImageSizeMode()
        {
            switch (NowImageSizeMode)
            {
                case ImageSizeMode.Big:
                    {
                        SmallSizeEllipse.Opacity = 0;
                        MediumSizeEllipse.Opacity = 0;
                        BigSizeEllipse.Opacity = 1;
                        break;
                    }
                case ImageSizeMode.Medium:
                    {
                        SmallSizeEllipse.Opacity = 0;
                        MediumSizeEllipse.Opacity = 1;
                        BigSizeEllipse.Opacity = 0;
                        break;
                    }

                case ImageSizeMode.Small:
                    {
                        SmallSizeEllipse.Opacity = 1;
                        MediumSizeEllipse.Opacity = 0;
                        BigSizeEllipse.Opacity = 0;
                        break;
                    }
            }
        }

        public enum SortMode
        {
            ShootDate,
            CreateDate,
            LastEditDate,
            Name,
        }

        public static bool IsAscending;

        public static SortMode NowSortMode { set; get; }


        private async void ShootDate_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("ShootDate_Click");
            if (NowSortMode != SortMode.ShootDate)
            {
                NowSortMode = SortMode.ShootDate;
                SwitchSortMode();
                FolderManager.MyImageArrangement.SortImg();
                FolderManager.MyImageArrangement.UpdateImgRect();
                FolderManager.MyImageArrangement.ImgListChanged();

                await Task.Delay(400);
                if (SortFlyout.IsOpen)
                    SortFlyout.Hide();
            }
        }

        private async void CreateDate_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("CreateDate_Click");
            if (NowSortMode != SortMode.CreateDate)
            {
                NowSortMode = SortMode.CreateDate;
                SwitchSortMode();
                FolderManager.MyImageArrangement.SortImg();
                FolderManager.MyImageArrangement.UpdateImgRect();
                FolderManager.MyImageArrangement.ImgListChanged();

                await Task.Delay(400);
                if (SortFlyout.IsOpen)
                    SortFlyout.Hide();
            }
        }

        private async void LastEditDate_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("LastEditDate_Click");
            if (NowSortMode != SortMode.LastEditDate)
            {
                NowSortMode = SortMode.LastEditDate;
                SwitchSortMode();
                FolderManager.MyImageArrangement.SortImg();
                FolderManager.MyImageArrangement.UpdateImgRect();
                FolderManager.MyImageArrangement.ImgListChanged();

                await Task.Delay(400);
                if (SortFlyout.IsOpen)
                    SortFlyout.Hide();
            }
        }

        private async void Name_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Name_Click");
            if (NowSortMode != SortMode.Name)
            {
                NowSortMode = SortMode.Name;
                SwitchSortMode();
                FolderManager.MyImageArrangement.SortImg();
                FolderManager.MyImageArrangement.UpdateImgRect();
                FolderManager.MyImageArrangement.ImgListChanged();

                await Task.Delay(400);
                if (SortFlyout.IsOpen)
                    SortFlyout.Hide();
            }
        }

        private void SwitchSortMode()
        {
            switch (NowSortMode)
            {
                case SortMode.ShootDate:
                    {
                        ShootDateEllipse.Opacity = 1;
                        CreateDateEllipse.Opacity = 0;
                        LastEditDateEllipse.Opacity = 0;
                        NameEllipse.Opacity = 0;
                        break;
                    }
                case SortMode.CreateDate:
                    {
                        ShootDateEllipse.Opacity = 0;
                        CreateDateEllipse.Opacity = 1;
                        LastEditDateEllipse.Opacity = 0;
                        NameEllipse.Opacity = 0;
                        break;
                    }
                case SortMode.LastEditDate: 
                    {
                        ShootDateEllipse.Opacity = 0;
                        CreateDateEllipse.Opacity = 0;
                        LastEditDateEllipse.Opacity = 1;
                        NameEllipse.Opacity = 0;
                        break;
                    }
                case SortMode.Name:
                    {
                        ShootDateEllipse.Opacity = 0;
                        CreateDateEllipse.Opacity = 0;
                        LastEditDateEllipse.Opacity = 0;
                        NameEllipse.Opacity = 1;
                        break;
                    }
            }
        }

        private async void Ascending_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Ascending_Click");
            if (!IsAscending)
            {
                IsAscending = true;
                SwitchAscending();
                FolderManager.MyImageArrangement.SortImg();
                FolderManager.MyImageArrangement.UpdateImgRect();
                FolderManager.MyImageArrangement.ImgListChanged();

                await Task.Delay(400);
                if (SortFlyout.IsOpen)
                    SortFlyout.Hide();
            }
        }

        private async void Descending_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Ascending_Click");
            if (IsAscending)
            {
                IsAscending = false;
                SwitchAscending();
                FolderManager.MyImageArrangement.SortImg();
                FolderManager.MyImageArrangement.UpdateImgRect();
                FolderManager.MyImageArrangement.ImgListChanged();

                await Task.Delay(400);
                if (SortFlyout.IsOpen)
                    SortFlyout.Hide();
            }
        }

        private void SwitchAscending()
        {
            if (IsAscending)
            {
                AscendingEllipse.Opacity = 1;
                DescendingEllipse.Opacity = 0;
            }
            else
            {
                AscendingEllipse.Opacity = 0;
                DescendingEllipse.Opacity = 1;
            }
        }


        private async void Single_Click(object sender, RoutedEventArgs e)
        {
            if (NowSelectMode != SelectMode.Single)
            {
                if (NowSelectMode == SelectMode.Multiple)
                {
                    ClearAllSelect();
                }

                NowSelectMode = SelectMode.Single;
                SwitchSelectMode();

                await Task.Delay(400);
                if (SelectFlyout.IsOpen)
                    SelectFlyout.Hide();
            }
        }

        private async void Multiple_Click(object sender, RoutedEventArgs e)
        {
            if (NowSelectMode != SelectMode.Multiple)
            {
                NowSelectMode = SelectMode.Multiple;
                ImageListPage.SelectedImage = null;
                SwitchSelectMode();

                await Task.Delay(400);
                if (SelectFlyout.IsOpen)
                    SelectFlyout.Hide();
            }
        }

        private async void UnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (NowSelectMode != SelectMode.None)
            {
                ClearAllSelect();
                ImageListPage.SelectedImage = null;
                NowSelectMode = SelectMode.None;
                SwitchSelectMode();

                await Task.Delay(400);
                if (SelectFlyout.IsOpen)
                    SelectFlyout.Hide();
            }
        }

        public void ClearAllSelect()
        {
            _selectedCount = 0;
            ImageListPage.SelectedImage = null;
            foreach (var _image in FolderManager.MyImageArrangement.ImgList)
            {
                if (_image.IsSelected)
                {
                    _image._checkBoxOpacity = 0;
                    _image._borderOpacity = 0;
                    _image._rectangleOpacity = 0;
                    UnSelect(_image);
                }
            }
        }

        private async void UnSelect(PictureClass _image)
        {
            await Task.Delay(200);
            _image._isSelected = false;
        }

        private void SwitchSelectMode()
        {
            switch (NowSelectMode)
            {
                case SelectMode.Single:
                    {
                        SingleEllipse.Opacity = 1;
                        MultipleEllipse.Opacity = 0;
                        UnSelectEllipse.Opacity = 0;
                        break;
                    }
                case SelectMode.Multiple:
                    {
                        SingleEllipse.Opacity = 0;
                        MultipleEllipse.Opacity = 1;
                        UnSelectEllipse.Opacity = 0;
                        break;
                    }
                case SelectMode.None:
                    {
                        SingleEllipse.Opacity = 0;
                        MultipleEllipse.Opacity = 0;
                        UnSelectEllipse.Opacity = 1;
                        break;
                    }
            }
        }

        public static List<StorageFile> _selectImages = new();

        private async Task GetImageFile(string _path)
        {
            _selectImages.Add(await StorageFile.GetFileFromPathAsync(_path));
        }

        private async Task<DataPackage> CopyOrCutImages()
        {
            List<Task> _copyTasks = new();

            foreach (var _image in FolderManager.MyImageArrangement.ImgList)
            {
                if (_image.IsSelected)
                {
                    _copyTasks.Add(GetImageFile(_image.ImageLocation));
                }
            }

            var _images = new DataPackage();
            await Task.WhenAll(_copyTasks);

            _images.SetStorageItems(_selectImages);

            return _images;
        }

        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            Task _copyStart = CopyStarted();
            var _images = await CopyOrCutImages();
            _images.RequestedOperation = DataPackageOperation.Copy;
            Clipboard.SetContent(_images);
            _selectImages.Clear();
            await _copyStart;
            CopySuccessed();
        }

        private async Task CopyStarted()
        {
            Copy.IsEnabled = false;
            await Task.Delay(300);
        }

        private async void CopySuccessed()
        {
            CopySuccess.Opacity = 1;
            await Task.Delay(1250);
            CopySuccess.Opacity = 0;
            await Task.Delay(300);
            Copy.IsEnabled = true;
        }

        private async void Cut_Click(object sender, RoutedEventArgs e)
        {
            Task _cutStart = CutStarted();
            var _images = await CopyOrCutImages();
            _images.RequestedOperation = DataPackageOperation.Move;
            Clipboard.SetContent(_images);
            _selectImages.Clear();
            await _cutStart;
            CutSuccessed();
        }



        private async Task CutStarted()
        {
            Cut.IsEnabled = false;
            await Task.Delay(300);
        }

        private async void CutSuccessed()
        {
            CutSuccess.Opacity = 1;
            await Task.Delay(1250);
            CutSuccess.Opacity = 0;
            await Task.Delay(300);
            Cut.IsEnabled = true;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            AddContentDialog _deleteDialog = new(AddContentDialog.Mode.DeleteImageMode)
            {
                XamlRoot = Grid.XamlRoot
            };
            await _deleteDialog.ShowAsync();

            if (_deleteDialog.Result == AddContentDialog.ContentDialogResult.ConfirmDelete)
            {
                foreach (var _image in FolderManager.MyImageArrangement.ImgList)
                {
                    if (_image.IsSelected)
                    {
                        FolderManager.DeleteImg(_image);
                    }
                }

                //_selectedCount = 0;
                //ImageListPage.SelectedImage = null;
            }
        }

        private int SelectedCount = 0;

        public int _selectedCount
        {
            get => SelectedCount;
            set
            {
                SelectedCountChanged(value);                
            }
        } 

        private void SelectedCountChanged(int _newCount)
        {
            if (SelectedCount != _newCount)
            {
                string _temp = _newCount.ToString();
                CancleGrid.Width = 70 + _temp.Length * 8;
                SelectedCount = _newCount;
                OnPropertyChanged(nameof(_selectedCount));
            }

            if (SelectedCount == 0 && Cancle.IsEnabled)
            {
                Cancle.IsEnabled = false;
                Delete.IsEnabled = false;
                Cut.IsEnabled = false;
                Copy.IsEnabled = false;
            }
            else if (SelectedCount > 0 && !Cancle.IsEnabled)
            {
                Cancle.IsEnabled = true;
                Delete.IsEnabled = true;
                Cut.IsEnabled = true;
                Copy.IsEnabled = true;
            }
        }

        public void Cancle_Click(object sender, RoutedEventArgs e)
        {
            ClearAllSelect();
        }

        private int ImageCount = 0;

        public int _imageCount 
        {
            get => ImageCount;
            set
            {
                ImageCountChanged(value);

            }
        }

        private void ImageCountChanged(int _newCount)
        {
            if (ImageCount != _newCount)
            {
                ImageCount = _newCount;
                string _temp = _newCount.ToString();
                CountText.Margin = new(72 + 8 * _temp.Length, 45, 0, 0);

                OnPropertyChanged(nameof(_imageCount));
            }

        }



    }
}
