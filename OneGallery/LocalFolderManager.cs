using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.VisualBasic.FileIO;
using Windows.Storage;


namespace OneGallery
{
    public class LocalFolderManager
    {
        private Dictionary<string, LocalFolder> LocalFolders = new();

        private Dictionary<string, List<PictureClass>> GallerySelectionToImgList = new();

        public List<PictureClass> HomPageImgList = new();

        public Category NowCategory { get; set; }

        public PathConfig MyPathConfig { get; set; }

        public SettingsConfig MySettingsConfig { get; set; }

        public ImageArrangement MyImageArrangement { get; set; }

        public Task InitFolderTask { get; set; } = Task.CompletedTask;

        public LocalFolderManager()
        {
            MyImageArrangement = new();
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

            lock (MyPathConfig.GalleryToFolderListConfig)
            {
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
            }

            lock (HomPageImgList)
            {
                HomPageImgList.Add(_tempImg);
            }

            lock (LocalFolders[_folderName].ImageList)
            {
                LocalFolders[_folderName].ImageList.Add(_tempImg);
            }

            if (NowCategory != null)
            {
                if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory._name == _folderName) ||
                    (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory._name].Contains(_folderName)))
                {

                    bool isQueued = MainWindow.Window.DispatcherQueue.TryEnqueue(
                    async () =>
                    {
                        await Task.Delay(200);

                        AddCount--;

                        if (AddCount == 0)
                        {
                            lock (MyImageArrangement.ImgList)
                            {
                                MyImageArrangement.SortImg();
                                MyImageArrangement.UpdateImgRect();
                                MyImageArrangement.ImgListChanged();
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

            if (LocalFolder.IsImage(e.Name))
            {
                var _tempImage = LocalFolders[_folderName].ImageList.Find(x => x.ImageLocation == e.OldFullPath);

                if (_tempImage != null)
                {
                    var _newName = e.Name.Split('\\').Last();

                    bool isQueued = MainWindow.Window.DispatcherQueue.TryEnqueue(
                    () =>
                    {
                        _tempImage._name = _newName;
                        _tempImage._imageLocation = e.FullPath;
                    });

                    Debug.Print("Renamed " + _newName);
                }

                if (NowCategory != null)
                {
                    if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory._name == _folderName) ||
                        (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory._name].Contains(_folderName)))
                    {
                        MyImageArrangement.SortImg();
                        MyImageArrangement.UpdateImgRect();
                        bool isQueued = MainWindow.Window.DispatcherQueue.TryEnqueue(() =>
                        {
                            var _tempImgListForRepeater = MyImageArrangement.ImgListForRepeater;
                            var _tempImgList = MyImageArrangement.ImgList;
                            foreach (var item in _tempImgList)
                            {
                                if (_tempImgListForRepeater.IndexOf(item) != _tempImgList.IndexOf(item))
                                {
                                    MainWindow.Window.DispatcherQueue.TryEnqueue(() =>
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

                    bool isQueued = MainWindow.Window.DispatcherQueue.TryEnqueue(
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

            lock (MyPathConfig.GalleryToFolderListConfig)
            {
                foreach (var _selectedItem in MyPathConfig.GalleryToFolderListConfig)
                {
                    if (_selectedItem.Value.Contains(_folderName))
                    {
                        lock (GallerySelectionToImgList[_selectedItem.Key])
                        {
                            GallerySelectionToImgList[_selectedItem.Key].Remove(_tempImg);
                        }
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
                if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory._name == _folderName) ||
                    (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory._name].Contains(_folderName)))
                {

                    MyImageArrangement.SortImg();
                    MyImageArrangement.UpdateImgRect();

                    bool isQueued = MainWindow.Window.DispatcherQueue.TryEnqueue(
                    () =>
                    {
                        if (_tempImg.IsSelected)
                        {
                            MainWindow.Window._selectedCount--;
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
                    HomPageImgList.Add(_item);
                }
            }

            if (NowCategory != null)
            {

                if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory._name == _folderName) ||
                    (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory._name].Contains(_folderName)))
                {
                    MyImageArrangement.SortImg();
                    MyImageArrangement.UpdateImgRect();

                    bool isQueued = MainWindow.Window.DispatcherQueue.TryEnqueue(() =>
                    {
                        MyImageArrangement.ImgListChanged();
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

            if (NowCategory != null && !NowCategory.IsFolderInfo && !NowCategory.IsGalleryInfo)
            {
                if (NowCategory.IsHomePage || (NowCategory.IsFolder && NowCategory._name == _folderName) ||
                    (NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory._name].Contains(_folderName)))
                {
                    MyImageArrangement.SortImg();
                    MyImageArrangement.UpdateImgRect();
                    MainWindow.Window.DispatcherQueue.TryEnqueue(() =>
                    {
                        lock (MyImageArrangement.ImgList)
                        {
                            foreach (var _item in LocalFolders[_folderName].ImageList)
                            {
                                MyImageArrangement.ImgListForRepeater.Remove(_item);
                                if (_item.IsSelected)
                                {
                                    MainWindow.Window._selectedCount--;
                                    if (_item == ImageListPage.SelectedImage)
                                    {
                                        ImageListPage.SelectedImage = null;
                                    }
                                }

                            }
                        }
                    });
                }
            }

            lock (LocalFolders[_folderName].ImageList)
            {
                LocalFolders[_folderName].ImageList.Clear();
            }
        }

        /*
         * SearchEvent
         */

        private void OnSearchEvent(object sender, EventArgs e)
        {
            string _folderName = sender as string;
            bool _isEnd = (e as SearchChangeEvent).IsEnd;
            MainWindow.Window.DispatcherQueue.TryEnqueue(() =>
            {
                MainWindow.Window.ChangeProcessBar(_folderName, _isEnd);
            });

        }


        public void SaveConfig(int _lastWidth, int _lastHeight, Category _galleries, Category _folders)
        {
            MyPathConfig.StorePathConfig(_galleries, _folders);
            MySettingsConfig.StoreSettingsConfig(_lastWidth, _lastHeight);

            foreach (var _folder in LocalFolders.Values)
                _folder.Close();

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
            var _settingsJsonPath = ApplicationData.Current.LocalCacheFolder.Path + "\\Settings.json";
            StorageFile _settingsFile;

            try
            {
                _settingsFile = await StorageFile.GetFileFromPathAsync(_settingsJsonPath);
            }
            catch (Exception)
            {
                var _originSettingsFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Res/Settings.json"));
                var _folder = ApplicationData.Current.LocalCacheFolder;
                await _originSettingsFile.CopyAsync(_folder);
                _settingsFile = await StorageFile.GetFileFromPathAsync(_settingsJsonPath);
            }

            string _configString = await FileIO.ReadTextAsync(_settingsFile);
            MySettingsConfig = JsonSerializer.Deserialize<SettingsConfig>(_configString);

            var _pathJsonPath = ApplicationData.Current.LocalCacheFolder.Path + "\\Path.json";
            StorageFile _pathFile;

            try
            {
                _pathFile = await StorageFile.GetFileFromPathAsync(_pathJsonPath);
            }
            catch (Exception)
            {
                var _originSettingsFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Res/Path.json"));
                var _folder = ApplicationData.Current.LocalCacheFolder;
                await _originSettingsFile.CopyAsync(_folder);
                _pathFile = await StorageFile.GetFileFromPathAsync(_pathJsonPath);
            }

            _configString = await FileIO.ReadTextAsync(_pathFile);
            MyPathConfig = JsonSerializer.Deserialize<PathConfig>(_configString);

            MainWindow.Window.MySettingsConfig = MySettingsConfig;
            MainWindow.Window.MyPathConfig = MyPathConfig;
        }

        public void InitFolder()
        {
            InitFolderTask = InitFolders();
        }

        private async Task InitFolders()
        {
            foreach (var _selectItemName in MyPathConfig.GalleryToFolderListConfig.Keys)
                GallerySelectionToImgList.Add(_selectItemName, new());

            foreach (var _foldPath in MyPathConfig.FolderPathConfig)
            {
                AddNewFolder(_foldPath.Key, _foldPath.Value);
                await Task.Delay(100);
            }
        }


        private async Task WaitFolderTask(Category _category)
        {
            if (_category.IsHomePage)
            {
                await Task.WhenAll(FolderInitTask.Values);
            }
            else
            {
                if (_category.IsGallery)
                {
                    foreach (var _folderName in MyPathConfig.GalleryToFolderListConfig[_category._name])
                    {
                        await FolderInitTask[_folderName];
                    }
                }
                else if (_category.IsFolder)
                {
                    await FolderInitTask[_category._name];
                }
            }

        }

        public async Task InitImageListPage(Category _category)
        {
            await InitFolderTask;
            await WaitFolderTask(_category);

            if (MainWindow.Window._nowCategory == _category)
            {
                NowCategory = _category;

                if (NowCategory.IsHomePage)
                    MyImageArrangement.ImgList = HomPageImgList;
                else if (NowCategory.IsGallery)
                    MyImageArrangement.ImgList = GallerySelectionToImgList[NowCategory._name];
                else if (NowCategory.IsFolder)
                    MyImageArrangement.ImgList = LocalFolders[NowCategory._name].ImageList;

                MyImageArrangement.SortImg();
                MyImageArrangement.UpdateImgRect();
            }

            return;
        }

        static Dictionary<string, Task> FolderInitTask = new();

        public void AddNewFolder(string _name, string _folderPath)
        {
            Thread thread = new(() =>
            {
                LocalFolder _localFolder = new(_folderPath, _name);
                LocalFolders.Add(_name, _localFolder);
                _localFolder.FileDeleted += OnFileDeletedEvent;
                _localFolder.FileRenamed += OnFileReNamedEvent;
                _localFolder.FileCreated += OnFileCreatedEvent;
                _localFolder.SearchTask += OnSearchEvent;
                _localFolder.FolderNotFound += OnFolderNotFoundEvent;
                _localFolder.FolderExist += OnFolderExistEvent;

                FolderInitTask.Add(_name, _localFolder.FirstInitFolder());
            });
            thread.Start();
        }


        public void AddNewGallery(string _name)
        {
            List<PictureClass> _tempList = new();
            GallerySelectionToImgList.Add(_name, _tempList);

            var _folderNames = MyPathConfig.GalleryToFolderListConfig[_name];

            foreach (var _item in _folderNames)
            {
                lock (LocalFolders[_item].ImageList)
                {
                    var _imageList = LocalFolders[_item].ImageList;
                    foreach (var _image in _imageList)
                    {
                        _tempList.Add(_image);
                    }
                }
            }

            lock (MyPathConfig.GalleryToFolderListConfig)
            {
                var _gallery = (MainWindow.Window.Categories[3] as Category).Children.FirstOrDefault(x => x._name == _name);
                _gallery._searchCount = 0;

                foreach (var _item in _folderNames)
                {
                    var _category = (MainWindow.Window.Categories[5] as Category).Children.FirstOrDefault(x => x._name == _item);
                    _gallery._searchCount += _category._searchCount;
                }
            }
        }

        public void RenameFolder(string _oldname, string _newname)
        {
            var _tempFolder = LocalFolders[_oldname];
            LocalFolders.Remove(_oldname);
            _tempFolder.RenameFolder(_newname);
            LocalFolders.Add(_newname, _tempFolder);

            var _folderTask = FolderInitTask[_oldname];
            FolderInitTask.Remove(_oldname);
            FolderInitTask.Add(_newname, _folderTask);
        }

        public void RenameGallery(string _oldname, string _newname)
        {
            var tempGalleryImageList = GallerySelectionToImgList[_oldname];
            GallerySelectionToImgList.Remove(_oldname);
            GallerySelectionToImgList.Add(_newname, tempGalleryImageList);
        }

        /*
         *  Remove
         */
        public async Task RemoveFolder(string _name)
        {
            var _localFolder = LocalFolders[_name];

            await _localFolder.DeleteFolder();
            _localFolder.FileDeleted -= OnFileDeletedEvent;
            _localFolder.FileRenamed -= OnFileReNamedEvent;
            _localFolder.FileCreated -= OnFileCreatedEvent;
            _localFolder.FolderNotFound -= OnFolderNotFoundEvent;
            _localFolder.FolderExist -= OnFolderExistEvent;
            _localFolder.SearchTask -= OnSearchEvent;
            OnFolderNotFoundEvent(_name, null);
            LocalFolders.Remove(_name);
            FolderInitTask.Remove(_name);

            var _category = (MainWindow.Window.Categories[5] as Category).Children.FirstOrDefault(x => x._name == _name);
            (MainWindow.Window.Categories[1] as Category)._searchCount -= _category._searchCount;

            lock (MyPathConfig.GalleryToFolderListConfig)
            {
                foreach (var _item in MyPathConfig.GalleryToFolderListConfig)
                {
                    if (_item.Value.Contains(_name))
                    {
                        (MainWindow.Window.Categories[3] as Category).Children.FirstOrDefault(x => x._name == _item.Key)._searchCount -= _category._searchCount;
                    }
                }
            }
        }

        public void RemoveGallery(string _name)
        {
            GallerySelectionToImgList.Remove(_name);
        }

        public async Task ResetFolder(string _name, string _path)
        {
            await RemoveFolder(_name);
            AddNewFolder(_name, _path);
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

        public void DeleteImg(PictureClass _img)
        {

            if (MySettingsConfig.DeleteToTrashcan)
            {
                FileSystem.DeleteFile(
                  _img.ImageLocation,
                  UIOption.OnlyErrorDialogs,
                  RecycleOption.SendToRecycleBin);
            }
            else
            {
                FileSystem.DeleteFile(
                  _img.ImageLocation,
                  UIOption.OnlyErrorDialogs,
                  RecycleOption.DeletePermanently);
            }
        }
    }
}
