using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
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

        private void OnFileDeletedEvent(object sender, EventArgs e)
        {
            Debug.Print("OnDeleteFolderEvent");
            string _folderName = sender as string;
            PictureClass _tempImg = (e as FileChangeEvent).File;
            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {

                    SelectionToImgList[_selectedItem.Key].Remove(_tempImg);

                    if (_selectedItem.Key == NowPageName)
                    {
                        MyImageArrangement.SortImg(0);
                        MyImageArrangement.UpdateImgRect();

                        bool isQueued = Window.DispatcherQueue.TryEnqueue(
                        () =>
                        {
                            Debug.Print(Window.DispatcherQueue.HasThreadAccess + " DispatcherQueue");
                            MyImageArrangement.ImgListForRepeater.Remove(_tempImg);
                        });

                            Debug.Print(isQueued + " OnDeleteFolderEvent");

                    }

                }

            }

        }

        private void OnFolderExistEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFolderExistEvent");

            string _folderName = sender as string;
            
            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {


                    foreach (var _item in LocalFolders[_folderName].ImageList)
                        SelectionToImgList[_selectedItem.Key].Add(_item);

                    Debug.Print("Add " + LocalFolders[_folderName].ImageList.Count);

                    if (_selectedItem.Key == NowPageName)
                    {
                        MyImageArrangement.SortImg(0);
                        MyImageArrangement.UpdateImgRect();
                        lock (MyImageArrangement.ImgListForRepeater)
                        {
                            MyImageArrangement.ImgListChangedEvent();
                        }

                    }
                    

                }
            }

            Debug.Print(Window.NaPage.DispatcherQueue.HasThreadAccess + " OnFolderExistEvent");

        }

        private void OnFolderNotFoundEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFolderNotFoundEvent");

            string _folderName = sender as string;

            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {

                    foreach (var _item in LocalFolders[_folderName].ImageList)
                        SelectionToImgList[_selectedItem.Key].Remove(_item);

                    Debug.Print(_selectedItem.Key + " And " + NowPageName);

                    if (_selectedItem.Key == NowPageName)
                    {
                        MyImageArrangement.SortImg(0);
                        MyImageArrangement.UpdateImgRect();
                        MyImageArrangement.ImgListChangedEvent();
                    }
                    
                }
            }

        }

        public async void InitFolder()
        {
            await InitConfig();

            foreach (var _foldPath in MyConfig.FolderPathConfig)
            {
                LocalFolder _localFolder = new(_foldPath.Value, _foldPath.Key);
                LocalFolders.Add(_foldPath.Key, _localFolder);
                _localFolder.FileDeleted += OnFileDeletedEvent;
                _localFolder.FolderNotFound += OnFolderNotFoundEvent;
                _localFolder.FolderExist += OnFolderExistEvent;
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
