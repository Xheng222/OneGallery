﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;

namespace OneGallery
{
    public class LocalFolderManager
    {
        private Dictionary<string, LocalFolder> LocalFolders = new();

        private Dictionary<string, List<PictureClass>> GallerySelectionToImgList = new();

        public List<PictureClass> HomPageImgList = new();

        private MainWindow Window { get; set; }

        public Category NowCategory { get; set; }

        public PathConfig MyPathConfig {  get; set; }

        public SettingsConfig MySettingsConfig { get; set; }

        public ImageArrangement MyImageArrangement { get; set; }

        public Task InitFolderTask { get; set; }
       
        public LocalFolderManager() 
        {
            MyImageArrangement = new();
        }

        public void SaveConfig(int _lastWidth, int _lastHeight, Category _galleries, Category _folders)
        {
            MyPathConfig.StorePathConfig(_galleries, _folders);
            MySettingsConfig.StoreSettingsConfig(_lastWidth, _lastHeight);

            SaveImageConfigs();
        }

        private void SaveImageConfigs()
        {
            StorageFolder _folder = ApplicationData.Current.LocalFolder;
            foreach (var _localFolder in LocalFolders.Values)
            {
                SaveImageJson(_localFolder, _folder);
            }

        }

        private static async void SaveImageJson(LocalFolder _localFolder, StorageFolder _folder)
        {
            StorageFile _jsonFile = await _folder.CreateFileAsync(_localFolder.FolderName + ".json", CreationCollisionOption.ReplaceExisting);
            
            if (_localFolder.ImageList is not null)
            {
                string jsonString = JsonSerializer.Serialize(_localFolder.ImageList);
                await FileIO.WriteTextAsync(_jsonFile, jsonString);
            }

        }

        public async Task InitConfigs()
        {
            var _configFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Res/Settings.json"));     
            string _configString = await FileIO.ReadTextAsync(_configFile);

            MySettingsConfig = JsonSerializer.Deserialize<SettingsConfig>(_configString);
            MySettingsConfig.ConfigFile = _configFile;

            _configFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Res/Path.json"));
            _configString = await FileIO.ReadTextAsync(_configFile);

            MyPathConfig = JsonSerializer.Deserialize<PathConfig>(_configString);
            MyPathConfig.ConfigFile = _configFile;

            Window = (Application.Current as App).Main;
            Window.MySettingsConfig = MySettingsConfig;
            Window.MyPathConfig = MyPathConfig;

            InitFolderTask = InitFolder();
        }

        public async Task InitFolder()
        {
            foreach (var _selectItemName in MyPathConfig.GalleryToFolderListConfig.Keys)
                GallerySelectionToImgList.Add(_selectItemName, new());

            foreach (var _foldPath in MyPathConfig.FolderPathConfig)
                await AddNewFolder(_foldPath.Key, _foldPath.Value);
        }

        public async Task InitImageListPage(Category _category)
        {
            await InitFolderTask;

            NowCategory = _category;

            if (NowCategory.IsHomePage)
                MyImageArrangement.ImgList = HomPageImgList;
            else if (NowCategory.IsGallery)
                MyImageArrangement.ImgList = GallerySelectionToImgList[NowCategory.Name];
            else if (NowCategory.IsFolder)
                MyImageArrangement.ImgList = LocalFolders[NowCategory.Name].ImageList;
            
            MyImageArrangement.SortImg();
            MyImageArrangement.UpdateImgRect();
            return;
        }

        /*
         * FileCreated
         */

        private int AddCount = 0;
        private void OnFileCreatedEvent(object sender, EventArgs e)
        {
            AddCount++;

            string _folderName = sender as string;
            PictureClass _tempImg = (e as FileChangeEvent).File;

            foreach (var _selectedItem in MyPathConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {        
                    lock (GallerySelectionToImgList[_selectedItem.Key])
                    {
                        GallerySelectionToImgList[_selectedItem.Key].Add(_tempImg);
                    }
                }
            }

            HomPageImgList.Add(_tempImg);
            LocalFolders[_folderName].ImageList.Add(_tempImg);

            if (NowCategory != null)
            {
                if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory.Name == _folderName) ||
                    (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)))
                {

                    bool isQueued = Window.DispatcherQueue.TryEnqueue(
                    () =>
                    {
                        AddCount--;
                        MyImageArrangement.SortImg();
                        MyImageArrangement.UpdateImgRect();

                        if (AddCount == 0)
                        {
                            lock (MyImageArrangement.ImgList)
                            {
                                var _tempImgList = MyImageArrangement.ImgList;
                                var _tempImgListForRepeater = MyImageArrangement.ImgListForRepeater;
                                Window._imageCount = _tempImgList.Count;

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
                            }
                        }

                    });
                }
                else
                    AddCount--;
            }
            else
                AddCount--;
        }

        /*
         * FileRenamed
         */

        private async void OnFileReNamedEvent(object sender, EventArgs _e)
        {
            Debug.Print("OnFileRenamedEvent");
            string _folderName = sender as string;
            RenamedEventArgs e = _e as RenamedEventArgs;

            if (LocalFolder.IsImageChanged(e.Name))
            {
                var _tempImage = LocalFolders[_folderName].ImageList.Find(x => x.ImageLocation == e.OldFullPath);
                
                if (_tempImage != null)
                {
                    var _newName = e.Name.Split('\\').Last();

                    bool isQueued = Window.DispatcherQueue.TryEnqueue(
                    () =>
                    {
                        _tempImage._name = _newName;
                        _tempImage._imageLocation = e.FullPath;
                    });

                    Debug.Print("Renamed " + _newName);
                }

                if (NowCategory != null)
                {
                    if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory.Name == _folderName) ||
                        (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)))
                    {
                        MyImageArrangement.SortImg();
                        MyImageArrangement.UpdateImgRect();
                        bool isQueued = Window.DispatcherQueue.TryEnqueue(() =>
                        {
                            var _tempImgListForRepeater = MyImageArrangement.ImgListForRepeater;
                            var _tempImgList = MyImageArrangement.ImgList;
                            foreach (var item in _tempImgList)
                            {
                                if (_tempImgListForRepeater.IndexOf(item) != _tempImgList.IndexOf(item))
                                {
                                    Window.DispatcherQueue.TryEnqueue(() =>
                                    {
                                        _tempImgListForRepeater.Move(_tempImgListForRepeater.IndexOf(item), _tempImgList.IndexOf(item));
                                    });
                                }
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
                            if (item._imageLocation.Contains(e.OldFullPath))
                                item._imageLocation = item._imageLocation.Replace(e.OldFullPath, e.FullPath);
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
            foreach (var _selectedItem in MyPathConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock(GallerySelectionToImgList[_selectedItem.Key])
                    {
                        GallerySelectionToImgList[_selectedItem.Key].Remove(_tempImg);
                    }                    
                }
            }

            lock (LocalFolders[_folderName].ImageList)
            {
                LocalFolders[_folderName].ImageList.Remove(_tempImg);
            }

            lock (HomPageImgList)
            {
                HomPageImgList.Remove(_tempImg);
            }


            if (NowCategory != null)
            {
                if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory.Name == _folderName) ||
                    (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)))
                {

                    MyImageArrangement.SortImg();
                    MyImageArrangement.UpdateImgRect();

                    bool isQueued = Window.DispatcherQueue.TryEnqueue(
                    () =>
                    {
                        Window._imageCount--;
                        if (_tempImg.IsSelected)
                        {
                            Window._selectedCount--;
                            if (_tempImg == ImageListPage.SelectedImage)
                            {
                                ImageListPage.SelectedImage = null;
                            }
                        }

                        MyImageArrangement.ImgListForRepeater.Remove(_tempImg);
                    });



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
            
            foreach (var _selectedItem in MyPathConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock (GallerySelectionToImgList[_selectedItem.Key])
                    {
                        foreach (var _item in LocalFolders[_folderName].ImageList)
                            GallerySelectionToImgList[_selectedItem.Key].Add(_item);
                    }
                }
            }

            lock (HomPageImgList)
            {
                Stretch _temp = (MySettingsConfig.ImageZoomMode == 0) ? Stretch.UniformToFill : Stretch.Uniform;
                foreach (var _item in LocalFolders[_folderName].ImageList)
                {
                    //_item.NowStretch = _temp;
                    HomPageImgList.Add(_item);
                }          
            }

            if (NowCategory != null)
            {

                if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory.Name == _folderName) ||
                    (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)))
                {
                    MyImageArrangement.SortImg();
                    MyImageArrangement.UpdateImgRect();

                    bool isQueued = Window.DispatcherQueue.TryEnqueue(() =>
                    {
                        var _tempImgList = MyImageArrangement.ImgList;
                        var _tempImgListForRepeater = MyImageArrangement.ImgListForRepeater;
                        Window._imageCount = _tempImgList.Count;

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

        /*
         * FolderNotFound
         */

        private void OnFolderNotFoundEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFolderNotFoundEvent");

            string _folderName = sender as string;

            foreach (var _selectedItem in MyPathConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock (GallerySelectionToImgList[_selectedItem.Key])
                    {
                        foreach (var _item in LocalFolders[_folderName].ImageList)
                            GallerySelectionToImgList[_selectedItem.Key].Remove(_item);
                    }
                }
            }

            lock (HomPageImgList)
            {
                foreach (var _item in LocalFolders[_folderName].ImageList)
                    HomPageImgList.Remove(_item);
            }

            if (NowCategory != null && !NowCategory.IsFolderInfo)
            {
                if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory.Name == _folderName) ||
                    (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)))
                {
                    Window._imageCount = MyImageArrangement.ImgList.Count;
                    MyImageArrangement.SortImg();
                    MyImageArrangement.UpdateImgRect();

                    foreach (var _item in LocalFolders[_folderName].ImageList)
                    {
                        bool isQueued = Window.DispatcherQueue.TryEnqueue(
                        () =>
                        {
                            MyImageArrangement.ImgListForRepeater.Remove(_item);
                            if (_item.IsSelected)
                            {
                                Window._selectedCount--;
                                if (_item == ImageListPage.SelectedImage)
                                {
                                    ImageListPage.SelectedImage = null;
                                }
                            }
                        });
                    }
                }
            }

            LocalFolders[_folderName].ImageList.Clear();
        }

        public async Task AddNewFolder(string _name, string _folderPath) 
        {
            LocalFolder _localFolder = new(_folderPath, _name);
            LocalFolders.Add(_name, _localFolder);
            _localFolder.FileDeleted += OnFileDeletedEvent;
            _localFolder.FileRenamed += OnFileReNamedEvent;
            _localFolder.FileCreated += OnFileCreatedEvent;

            _localFolder.FolderNotFound += OnFolderNotFoundEvent;
            _localFolder.FolderExist += OnFolderExistEvent;
            await _localFolder.FirstInitFolder();
            //await Task.Delay(20000);
        }

        public void AddNewGallery(string _name)
        {
            List<PictureClass> _tempList = new();
            GallerySelectionToImgList.Add(_name, _tempList);

            var _folderNames = MyPathConfig.GalleryToFolderListConfig[_name];
           
            foreach (var _item in _folderNames)
            {
                var _imageList = LocalFolders[_item].ImageList;
                foreach (var _image in _imageList) 
                {
                    _tempList.Add(_image); 
                }
            }

        }

        public void RenameFolder(string _oldname, string _newname)
        {
            var _tempFolder = LocalFolders[_oldname];
            LocalFolders.Remove(_oldname);
            LocalFolders.Add(_newname, _tempFolder);
        }

        public void RenameGallery(string _oldname, string _newname)
        {
            var tempGalleryImageList = GallerySelectionToImgList[_oldname];
            GallerySelectionToImgList.Remove(_oldname);
            GallerySelectionToImgList.Add(_newname, tempGalleryImageList);
        }

        public async void RemoveFolder(string _name)
        {

            var _localFolder = LocalFolders[_name];

            await _localFolder.Close();
            _localFolder.FileDeleted -= OnFileDeletedEvent;
            _localFolder.FileRenamed -= OnFileReNamedEvent;
            _localFolder.FileCreated -= OnFileCreatedEvent;
            _localFolder.FolderNotFound -= OnFolderNotFoundEvent;
            _localFolder.FolderExist -= OnFolderExistEvent;
            OnFolderNotFoundEvent(_name, null);
            LocalFolders.Remove(_name);
        }

        public void RemoveGallery(string _name)
        {
            GallerySelectionToImgList.Remove(_name);
        }

        public void ResetFolder(string _name, string _path)
        {
            RemoveFolder(_name);
            _ = AddNewFolder(_name, _path);
        }

        public void ResetGallery(string _name)
        {
            RemoveGallery(_name);
            AddNewGallery(_name);
        }

        public async void SetStretch()
        {
            await InitFolderTask;

            PictureClass.NowStretch = (MySettingsConfig.ImageZoomMode == 0) ? Stretch.UniformToFill : Stretch.Uniform;

            if (MyImageArrangement.ImgList is not null)
            {
                foreach (var _image in MyImageArrangement.ImgList)
                {
                    _image.OnPropertyChanged_Stretch();
                }
            }

        }

        public async void DeleteImg(PictureClass _img)
        {
            var _tempImg = await StorageFile.GetFileFromPathAsync(_img.ImageLocation);

            if (MySettingsConfig.DeleteToTrashcan)
            {
                await _tempImg.DeleteAsync(StorageDeleteOption.Default);
            }
            else
            {
                await _tempImg.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
    }
}
