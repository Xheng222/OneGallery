using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace OneGallery
{
    internal class LocalFolderManager
    {
        private Dictionary<string, LocalFolder> LocalFolders = new();

        public Dictionary<string, SortableObservableCollection<PictureClass>> SelectionToImgList = new();

        private bool FolderInitSuccess = false;

        private MainWindow Window { get; set; }

        private string NowPageName = string.Empty;

        private Config MyConfig {  get; set; }
        
        public ImageArrangement MyImageArrangement { get; set; }
       
        public LocalFolderManager()
        {
            MyImageArrangement = new();

            MyImageArrangement.SetImgSize(
                new Size[] { new(500, 125), new(500, 150), new(500, 250) },
                new Size(500, 250),
                new double[] { 400, 900, 1400 },
                12, 12
            );      
        }

        public void SaveConfig()
        {
            MyConfig.ToConfigFile();
        }

        /*
         * FileRenamed
         */

        private async void OnFileReNamedEvent(object sender, EventArgs _e)
        {
            Debug.Print("OnFileRenamedEvent");
            string _folderName = sender as string;
            RenamedEventArgs e = _e as RenamedEventArgs;

            if (LocalFolders[_folderName].IsImageChanged(e.Name))
            {
                var _tempImage = LocalFolders[_folderName].ImageList.Find<PictureClass>(x => x.ImageLocation, e.OldFullPath);
                
                if (_tempImage != null)
                {
                    var _newName = e.Name.Split('\\').Last();

                    bool isQueued = Window.DispatcherQueue.TryEnqueue(
                    () =>
                    {
                        _tempImage._Name = _newName;
                        _tempImage._ImageLocation = e.FullPath;
                    });

                    Debug.Print("Renamed " + _newName);
                }
                //await Task.Delay(1000);
                if (MyConfig.GalleryToFolderListConfig.ContainsKey(NowPageName))
                {
                    if (MyConfig.GalleryToFolderListConfig[NowPageName].Contains(_folderName))
                    {


                        bool isQueued = Window.DispatcherQueue.TryEnqueue(
                        () =>
                        {
                            //MyImageArrangement.ImgListForRepeater.Sort(x => x.Name);

                            //MyImageArrangement.ImgListForRepeater.Move(0, 7);
                            MyImageArrangement.SortImg(0);
                            MyImageArrangement.UpdateImgRect();
                            MyImageArrangement.ImgListForRepeater.Move(0, 1);
                            //MyImageArrangement.ImgListForRepeater.Move(1, 5);
                            //(Window.NaPage.Content as ImageListPage).ImageListChanged();


                        });
                        
                    }
                }
            }
            else
            {
                try
                {
                    var Folder = await StorageFolder.GetFolderFromPathAsync(e.FullPath);

                    bool isQueued = Window.DispatcherQueue.TryEnqueue(
                    () =>
                    {
                        foreach (var item in LocalFolders[_folderName].ImageList)
                        {
                            if (item._ImageLocation.Contains(e.OldFullPath))
                                item._ImageLocation = item._ImageLocation.Replace(e.OldFullPath, e.FullPath);
                        }
                    });
                }
                catch (Exception)
                {
                    return;
                }
            }





            //foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            //{
            //    if (_selectedItem.Key == NowPageName)
            //    {
            //        if (_selectedItem.Value.Contains(_folderName))
            //        {
            //            lock(MyImageArrangement.ImgList)
            //            {
            //                MyImageArrangement.SortImg(0);
            //                MyImageArrangement.UpdateImgRect();

            //                bool isQueued = Window.DispatcherQueue.TryEnqueue(
            //                () =>
            //                {
            //                    //MyImageArrangement.ImgListChanged();
            //                    Window.NaPage.InvalidateMeasure();
            //                });
            //            }
            //        }

            //    }
            //}
        }


        /*
         * FileDeleted
         */

        private void OnFileDeletedEvent(object sender, EventArgs e)
        {
            Debug.Print("OnDeleteFolderEvent");
            string _folderName = sender as string;
            PictureClass _tempImg = (e as FileChangeEvent).File;
            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock(SelectionToImgList[_selectedItem.Key])
                    {
                        SelectionToImgList[_selectedItem.Key].Remove(_tempImg);

                        if (_selectedItem.Key == NowPageName)
                        {
                                MyImageArrangement.SortImg(0);
                                MyImageArrangement.UpdateImgRect();

                            bool isQueued = Window.DispatcherQueue.TryEnqueue(
                            () =>
                            {
                                MyImageArrangement.ImgListForRepeater.Remove(_tempImg);
                            });
                        }
                    }
                }
            }
        }

        /*
         * FolderFound
         */

        private void OnFolderExistEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFolderExistEvent");

            string _folderName = sender as string;
            
            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock (SelectionToImgList[_selectedItem.Key])
                    {
                        foreach (var _item in LocalFolders[_folderName].ImageList)
                            SelectionToImgList[_selectedItem.Key].Add(_item);

                        //Debug.Print("Add " + LocalFolders[_folderName].ImageList.Count);

                        if (_selectedItem.Key == NowPageName)
                        {
                            MyImageArrangement.SortImg(0);
                            MyImageArrangement.UpdateImgRect();
                            bool isQueued = Window.DispatcherQueue.TryEnqueue(
                            () =>
                            {
                                MyImageArrangement.ImgListChanged();

                            });
                        }
                    }       
                }
            }
        }

        /*
         * FolderNotFound
         */

        private void OnFolderNotFoundEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFolderNotFoundEvent");

            string _folderName = sender as string;

            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock (SelectionToImgList[_selectedItem.Key])
                    {
                        foreach (var _item in LocalFolders[_folderName].ImageList)
                            SelectionToImgList[_selectedItem.Key].Remove(_item);

                        if (_selectedItem.Key == NowPageName)
                        {
                            MyImageArrangement.SortImg(0);
                            MyImageArrangement.UpdateImgRect();
                            bool isQueued = Window.DispatcherQueue.TryEnqueue(
                            () =>
                            {
                                MyImageArrangement.ImgListChanged();
                            });
                        }
                    }

                }
            }

            LocalFolders[_folderName].ImageList.Clear();
        }

        public async void InitFolder()
        {
            await InitConfig();

            foreach (var _foldPath in MyConfig.FolderPathConfig)
            {
                LocalFolder _localFolder = new(_foldPath.Value, _foldPath.Key);
                LocalFolders.Add(_foldPath.Key, _localFolder);
                _localFolder.FileDeleted += OnFileDeletedEvent;
                _localFolder.FileRenamed += OnFileReNamedEvent;

                _localFolder.FolderNotFound += OnFolderNotFoundEvent;
                _localFolder.FolderExist += OnFolderExistEvent;
                await Task.Delay(1500);
            }
            Window = (MainWindow)(Application.Current as App).m_window;

            FolderInitSuccess = true;
        }

        public async Task InitConfig()
        {
            var _configFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Res/settings.json"));
            string _configString = await FileIO.ReadTextAsync(_configFile);

            MyConfig = JsonSerializer.Deserialize<Config>(_configString);
            MyConfig.ConfigFile = _configFile;

            foreach (var _selectItemName in MyConfig.GalleryToFolderListConfig.Keys)
                SelectionToImgList.Add(_selectItemName, new());
        }

        public async Task InitPageFolder(string _pageName)
        {
            while (!FolderInitSuccess)
                await Task.Delay(100);

            NowPageName = _pageName;

            MyImageArrangement.ImgList = SelectionToImgList[_pageName];

            MyImageArrangement.SortImg(0);

            MyImageArrangement.UpdateImgRect();

            return;
        }

        public async void DeleteImg(PictureClass _img)
        {
            //MyImageArrangement.ImgList.Remove(_img);

            //MyImageArrangement.UpdateImgRect();

            //MyImageArrangement.ImgListForRepeater.Remove(_img);

            //for (int _index = _img.Index; _index < MyImageArrangement.ImgList.Count; _index++)
            //    MyImageArrangement.ImgList[_index].Index = _index;

            //LocalFolders[_img.SourceName].ImageList.Remove(_img);

            //Debug.Print("Delete " + _img.ImageLocation);

            var _tempImg = await StorageFile.GetFileFromPathAsync(_img.ImageLocation);

            //await _tempImg.DeleteAsync();

        }



    }
}
