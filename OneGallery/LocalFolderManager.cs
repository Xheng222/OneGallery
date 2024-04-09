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

        public Dictionary<string, List<PictureClass>> SelectionToImgList = new();

        private bool FolderInitSuccess = false;

        private MainWindow Window { get; set; }

        private string NowPageName = string.Empty;

        private Config MyConfig {  get; set; }
        
        public ImageArrangement MyImageArrangement { get; set; }
       
        public LocalFolderManager()
        {
            MyImageArrangement = new();

            MyImageArrangement.SetImgSize(
                new Size[] { new(500, 125), new(500, 200), new(500, 400) },
                new double[] { 400, 900, 1400 },
                12, 12
            );      
        }

        public void SaveConfig()
        {
            MyConfig.ToConfigFile();
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
                _localFolder.FileCreated += OnFileCreatedEvent;

                _localFolder.FolderNotFound += OnFolderNotFoundEvent;
                _localFolder.FolderExist += OnFolderExistEvent;
                await Task.Delay(500);
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

        /*
         * FileCreated
         */
        private void OnFileCreatedEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFileCreatedEvent");
            string _folderName = sender as string;
            PictureClass _tempImg = (e as FileChangeEvent).File;
            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock (SelectionToImgList[_selectedItem.Key])
                    {
                        SelectionToImgList[_selectedItem.Key].Add(_tempImg);
                    }

                    if (_selectedItem.Key == NowPageName)
                    {
                        MyImageArrangement.SortImg(0);
                        MyImageArrangement.UpdateImgRect();

                        int _index = SelectionToImgList[_selectedItem.Key].IndexOf(_tempImg);

                        bool isQueued = Window.DispatcherQueue.TryEnqueue(
                        () =>
                        {
                            MyImageArrangement.ImgListForRepeater.Insert(_index, _tempImg);
                        });
                    }
                    
                }
            }
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
                var _tempImage = LocalFolders[_folderName].ImageList.Find(x => x.ImageLocation == e.OldFullPath);
                
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

                if (MyConfig.GalleryToFolderListConfig.ContainsKey(NowPageName))
                {
                    if (MyConfig.GalleryToFolderListConfig[NowPageName].Contains(_folderName))
                    {
                        MyImageArrangement.SortImg(0);
                        MyImageArrangement.UpdateImgRect();

                        bool isQueued = Window.DispatcherQueue.TryEnqueue(() =>
                        {
                            var _tempImgListForRepeater = MyImageArrangement.ImgListForRepeater;
                            var _tempImgList = MyImageArrangement.ImgList;
                            foreach (var item in _tempImgList)
                            {
                                Window.DispatcherQueue.TryEnqueue(() =>
                                {
                                    _tempImgListForRepeater.Move(_tempImgListForRepeater.IndexOf(item), _tempImgList.IndexOf(item));
                                });

                            }
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
        }


        /*
         * FileDeleted
         */

        private void OnFileDeletedEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFileDeletedEvent");
            string _folderName = sender as string;
            PictureClass _tempImg = (e as FileChangeEvent).File;
            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock(SelectionToImgList[_selectedItem.Key])
                    {
                        SelectionToImgList[_selectedItem.Key].Remove(_tempImg);
                    }

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

        /*
         * FolderFound
         */

        private void OnFolderExistEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFolderExistEvent");

            string _folderName = sender as string;
            
            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                lock (SelectionToImgList[_selectedItem.Key])
                {
                    foreach (var _item in LocalFolders[_folderName].ImageList)
                        SelectionToImgList[_selectedItem.Key].Add(_item);
                }

                if (_selectedItem.Value.Contains(_folderName))
                {
                    if (_selectedItem.Key == NowPageName)
                    {     
                        MyImageArrangement.SortImg(0);
                        MyImageArrangement.UpdateImgRect();
                        
                        bool isQueued = Window.DispatcherQueue.TryEnqueue( () =>
                        {
                            var _tempImgList = MyImageArrangement.ImgList;
                            var _tempImgListForRepeater = MyImageArrangement.ImgListForRepeater;

                            int i;
                            for (i = 0; i < _tempImgList.Count; i++)
                            {
                                if (i < _tempImgListForRepeater.Count)
                                {
                                    if (_tempImgList[i].ImageLocation == _tempImgListForRepeater[i].ImageLocation)
                                        continue;                   
                                    else
                                        _tempImgListForRepeater.Insert(i, _tempImgList[i]);
                                }
                                else
                                    _tempImgListForRepeater.Add(_tempImgList[i]);
                            }
                        });
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
                    }

                    if (_selectedItem.Key == NowPageName)
                    {                        
                        MyImageArrangement.SortImg(0);
                        MyImageArrangement.UpdateImgRect();

                        foreach (var _item in LocalFolders[_folderName].ImageList)
                        {
                            bool isQueued = Window.DispatcherQueue.TryEnqueue(
                            () =>
                            {
                                MyImageArrangement.ImgListForRepeater.Remove(_item);
                            });
                        }

                    }
                    

                }
            }

            LocalFolders[_folderName].ImageList.Clear();
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
