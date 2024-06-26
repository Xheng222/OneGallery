﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddFolderOrGallery : Page
    {
        private Category NowCategory { get; set; }

        private ObservableCollection<Category> Items { get; set; }

        private Category SelectedCategory { get; set; }

        public AddFolderOrGallery()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Category)
            {
                NowCategory = e.Parameter as Category;
                MainWindow.Window._nowCategory = NowCategory;
                MainWindow.Window.FolderManager.NowCategory = null;
                LoadData();
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            GC.Collect();
        }


        private void LoadData()
        {
            Items = NowCategory.Children;
        }


        /*
         * Grid_PointerEntered
         */

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

            var _temp = sender as Grid;
            if (_temp != null)
            {
                var _openAni = _temp.Resources["Open"] as Storyboard;
                _openAni.Begin();
            }
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var _temp = sender as Grid;
            if (_temp != null)
            {
                var _closeAni = _temp.Resources["Close"] as Storyboard;
                _closeAni.Begin();
            }
        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Debug.Print("Grid_PointerEntered");
            var _temp = sender as Grid;
            if (_temp != null)
            {
                var _ptr = e.GetCurrentPoint(_temp);
                if ((int)_ptr.Properties.PointerUpdateKind == (int)Windows.UI.Input.PointerUpdateKind.RightButtonReleased)
                {
                    Debug.Print("Grid_PointerEntered RightButtonReleased");
                    foreach (var _category in Items)
                    {
                        if (_category._name == _temp.Name)
                        {
                            SelectedCategory = _category;
                            break;
                        }
                    }

                    var _flyout = _temp.Resources["Flyout"] as Flyout;
                    FlyoutShowOptions myOption = new()
                    {
                        ShowMode = FlyoutShowMode.Transient
                    };
                    _flyout.ShowAt(_temp, myOption);
                    NowFlyout = _flyout;
                }
            }
        }


        /*
         * Flyout Click
         */

        Flyout NowFlyout { get; set; }
        private void Property_Click_Folder(object sender, RoutedEventArgs e)
        {
            if (NowCategory.IsFolderInfo)
            {
                foreach (var _pathConfig in MainWindow.Window.MyPathConfig.FolderPathConfig)
                {
                    if (_pathConfig.Key == SelectedCategory._name)
                    {
                        ShowDialog(SelectedCategory._name, _pathConfig.Value, AddContentDialog.Mode.ChangeFolderMode);
                        break;
                    }
                }
            }
            else
            {
                foreach (var _galleryConfig in MainWindow.Window.MyPathConfig.GalleryToFolderListConfig)
                {
                    if (_galleryConfig.Key == SelectedCategory._name)
                    {
                        ShowDialog(SelectedCategory._name, _galleryConfig.Value, AddContentDialog.Mode.ChangeGalleryMode);
                        break;
                    }
                }
            }

            if (NowFlyout is not null)
            {
                NowFlyout.Hide();
                NowFlyout = null;
            }
        }

        private void Delete_Click_Folder(object sender, RoutedEventArgs e)
        {

            if (NowCategory.IsFolderInfo)
            {
                ShowDialog(SelectedCategory._name, AddContentDialog.Mode.DeleteFolderMode);
            }
            else
            {
                ShowDialog(SelectedCategory._name, AddContentDialog.Mode.DeleteGalleryMode);
            }


            if (NowFlyout is not null)
            {
                NowFlyout.Hide();
                NowFlyout = null;
            }
        }




        /*
         * ItemGridView
         */

        private void ItemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var _item in e.AddedItems)
            {
                var _temp = _item as Category;
                if (!_temp.IsAddSelection)
                {
                    MainWindow.Window.DispatcherQueue.TryEnqueue(() =>
                    {
                        MainWindow.Window.NaPage.Navigate(typeof(ImageListPage), _item, new EntranceNavigationTransitionInfo());
                    });

                    break;
                }
                else
                {
                    if (_temp.IsGallery)
                        ShowDialog(AddContentDialog.Mode.AddGalleryMode);
                    else if (_temp.IsFolder)
                        ShowDialog(AddContentDialog.Mode.AddFolderMode);

                    break;
                }


            }
            e.AddedItems.Clear();
            ItemGridView.SelectedItem = null;


        }


        private void ItemGridView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            Debug.Print("ItemGridView_DragItemsCompleted");

            Category _temp = null;
            int i;
            int count = Items.Count;

            for (i = 0; i < count; i++)
            {
                if (Items[i].IsAddSelection)
                {
                    _temp = Items[i];
                    break;
                }
            }

            if (i != count - 1)
            {
                Items.Remove(_temp);
                Items.Add(_temp);
            }
        }

        private async void ShowDialog(AddContentDialog.Mode _mode)
        {
            AddContentDialog signInDialog = new(_mode)
            {
                XamlRoot = this.XamlRoot
            };
            await signInDialog.ShowAsync();
        }

        private async void ShowDialog(string _nowName, AddContentDialog.Mode _mode)
        {
            AddContentDialog signInDialog = new(_nowName, _mode)
            {
                XamlRoot = this.XamlRoot
            };
            await signInDialog.ShowAsync();
        }


        private async void ShowDialog(string _nowName, string _nowPath, AddContentDialog.Mode _mode)
        {
            AddContentDialog signInDialog = new(_nowName, _nowPath, _mode)
            {
                XamlRoot = this.XamlRoot
            };
            await signInDialog.ShowAsync();
        }

        private async void ShowDialog(string _nowName, List<string> _folders, AddContentDialog.Mode _mode)
        {
            ObservableCollection<object> _temp = new();
            var _Cfolders = (MainWindow.Window.Categories[5] as Category).Children;

            foreach (var _item in _Cfolders)
            {
                if (_item is Category _folder)
                {
                    if (_folders.Contains(_folder._name))
                    {
                        _temp.Add(_folder);
                    }
                }
            }

            AddContentDialog signInDialog = new(_nowName, _temp, _mode)
            {
                XamlRoot = this.XamlRoot
            };
            await signInDialog.ShowAsync();
        }
    }


    internal class MyAddFolderOrGallerySelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate FolderAddTemplate { get; set; }

        public DataTemplate GalleryTemplate { get; set; }

        public DataTemplate GalleryAddTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            // Return the correct data template based on the item's type.

            if (item is Category _temp)
            {
                if (_temp.IsFolder)
                {
                    if (_temp.IsAddSelection)
                    {
                        return FolderAddTemplate;
                    }
                    else
                    {
                        return FolderTemplate;
                    }
                }
                else if (_temp.IsGallery)
                {
                    if (_temp.IsAddSelection)
                    {

                        return GalleryAddTemplate;
                    }
                    else
                    {
                        return GalleryTemplate;
                    }
                }
                else
                    return null;
            }
            else
                return null;
        }
    }
}
