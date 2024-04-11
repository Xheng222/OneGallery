using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.Storage;

namespace OneGallery
{
    public class LocalFolderManager
    {
        private Dictionary<string, LocalFolder> LocalFolders = new();

        private Dictionary<string, List<PictureClass>> GallerySelectionToImgList = new();

        //public Dictionary<string, List<PictureClass>> FolderSelectionToImgList = new();

        private bool FolderInitSuccess = false;

        private MainWindow Window { get; set; }

        private Category NowCategory { get; set; }

        private string NowPageName = string.Empty;

        public PathConfig MyPathConfig {  get; set; }

        public SettingsConfig MySettingsConfig { get; set; }

        public ImageArrangement MyImageArrangement { get; set; }


       
        public LocalFolderManager() 
        {
            MyImageArrangement = new();

            MyImageArrangement.SetImgSize(
                new Size[] { new(500, 125), new(500, 200), new(500, 400) },
                new double[] { 400, 900, 1400 },
                12, 12
            );

            //Window = (Application.Current as App).Main;
        }

        public void SaveConfig(int _lastWidth, int _lastHeight)
        {
            MyPathConfig.StorePathConfig();
            MySettingsConfig.StoreSettingsConfig(_lastWidth, _lastHeight);
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
        }

        public async void InitFolder()
        {
            while (MyPathConfig is null)
            {
                await Task.Delay(500);
            }

            foreach (var _selectItemName in MyPathConfig.GalleryToFolderListConfig.Keys)
                GallerySelectionToImgList.Add(_selectItemName, new());

            foreach (var _foldPath in MyPathConfig.FolderPathConfig)
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

            FolderInitSuccess = true;
        }

        public async Task InitPageFolder(Category _category)
        {
            while (!FolderInitSuccess)
                await Task.Delay(100);

            //NowPageName = _pageName;
            NowCategory = _category;
            if (NowCategory.IsGallery)
                MyImageArrangement.ImgList = GallerySelectionToImgList[NowCategory.Name];
            else if (NowCategory.IsFolder)
            {
                MyImageArrangement.ImgList = LocalFolders[MyPathConfig.FolderToFolderListConfig[NowCategory.Name]].ImageList;
            }

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

            if (NowCategory != null)
            {

                if ((NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)) ||
                    (NowCategory.IsFolder && MyPathConfig.FolderToFolderListConfig[NowCategory.Name] == _folderName))
                {
                    MyImageArrangement.SortImg(0);
                    MyImageArrangement.UpdateImgRect();

                        bool isQueued = Window.DispatcherQueue.TryEnqueue(
                        () =>
                        {
                            //int _index = MyImageArrangement.ImgList.IndexOf(_tempImg);
                            //MyImageArrangement.ImgListForRepeater.Insert(_index, _tempImg);
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

                if (NowCategory != null)
                {
                    if ((NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)) ||
                        (NowCategory.IsFolder && MyPathConfig.FolderToFolderListConfig[NowCategory.Name] == _folderName))
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
            if (NowCategory != null)
            {
                if ((NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)) ||
                    (NowCategory.IsFolder && MyPathConfig.FolderToFolderListConfig[NowCategory.Name] == _folderName))
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

        /*
         * FolderFound
         */

        private void OnFolderExistEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFolderExistEvent");

            string _folderName = sender as string;
            
            foreach (var _selectedItem in MyPathConfig.GalleryToFolderListConfig)
            {
                lock (GallerySelectionToImgList[_selectedItem.Key])
                {
                    foreach (var _item in LocalFolders[_folderName].ImageList)
                        GallerySelectionToImgList[_selectedItem.Key].Add(_item);
                }
            }
            if (NowCategory != null)
            {

                if ((NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)) ||
                    (NowCategory.IsFolder && MyPathConfig.FolderToFolderListConfig[NowCategory.Name] == _folderName))
                {
                    MyImageArrangement.SortImg(0);
                    MyImageArrangement.UpdateImgRect();

                    bool isQueued = Window.DispatcherQueue.TryEnqueue(() =>
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

                    //if (NowCategory == Window.NowCategory)
                    //{                        
                    //    MyImageArrangement.SortImg(0);  
                    //    MyImageArrangement.UpdateImgRect();

                    //    foreach (var _item in LocalFolders[_folderName].ImageList)
                    //    {
                    //        bool isQueued = Window.DispatcherQueue.TryEnqueue(
                    //        () =>
                    //        {
                    //            MyImageArrangement.ImgListForRepeater.Remove(_item);
                    //        });
                    //    }
                    //}
                }
            }

            if (NowCategory != null)
            {
                if ((NowCategory.IsGallery && MyPathConfig.GalleryToFolderListConfig[NowCategory.Name].Contains(_folderName)) || 
                    (NowCategory.IsFolder && MyPathConfig.FolderToFolderListConfig[NowCategory.Name] == _folderName))
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
