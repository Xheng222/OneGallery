using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private MainWindow Window;

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

        private void OnDeleteFolderEvent(object sender, EventArgs e)
        {
            Debug.Print("received event");
            //Console.WriteLine($"Deleted: {e.FullPath}");

        }

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

                    Debug.Print("Add " + LocalFolders[_folderName].ImageList.Count);

                    if (_selectedItem.Key == NowPageName)
                    {
                        //foreach (var _item in LocalFolders[_folderName].ImageList)
                        //    MyImageArrangement.ImgList.Add(_item);

                        //MyImageArrangement.SortImg(0);
                        MyImageArrangement.UpdateImgRect();

                        Window.NaPage.InvalidateMeasure();
                        //Window.NaPage.InvalidateArrange();
                    }
                        //else
                        //{
                        //    SelectionToImgList[_selectedItem.Key].Union(LocalFolders[_folderName].ImageList);  
                        //}
                    }




                    Debug.Print(_selectedItem.Key + " And " + NowPageName);


                }
            }

        }

        private void OnFolderNotFoundEvent(object sender, EventArgs e)
        {
            Debug.Print("OnFolderNotFoundEvent");

            string _folderName = sender as string;

            foreach (var _selectedItem in MyConfig.GalleryToFolderListConfig)
            {
                if (_selectedItem.Value.Contains(_folderName))
                {
                    lock(SelectionToImgList[_selectedItem.Key])
                    {
                        foreach (var _item in LocalFolders[_folderName].ImageList)
                            SelectionToImgList[_selectedItem.Key].Remove(_item);

                        Debug.Print(_selectedItem.Key + " And " + NowPageName);

                        if (_selectedItem.Key == NowPageName)
                        {                            
                            //foreach (var _item in LocalFolders[_folderName].ImageList)
                            //    MyImageArrangement.ImgList.Remove(_item);

                            //MyImageArrangement.SortImg(0);
                            MyImageArrangement.UpdateImgRect();

                        }
                    }


                    //else
                    //{
                    //    SelectionToImgList[_selectedItem.Key].Except(LocalFolders[_folderName].ImageList);
                    //}
                    


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
                _localFolder.FolderEvent += OnDeleteFolderEvent;
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
            Debug.Print(_configString);
            MyConfig = JsonSerializer.Deserialize<Config>(_configString);
            MyConfig.ConfigFile = _configFile;

            foreach (var _selectItemName in MyConfig.GalleryToFolderListConfig.Keys)
            {
                SelectionToImgList.Add(_selectItemName, new());
            }
        }

        public async Task InitPageFolder(string _pageName)
        {
            while (!FolderInitSuccess)
                await Task.Delay(2000);

            NowPageName = _pageName;

            MyImageArrangement.ImgList = SelectionToImgList[_pageName];

            MyImageArrangement.SortImg(0);

            MyImageArrangement.UpdateImgRect();

            Debug.Print(NowPageName);
            Debug.Print(MyImageArrangement.ImgList.Count + "");

            return;

            if (SelectionToImgList.ContainsKey(_pageName))
            {
                foreach (var _folderName in MyConfig.GalleryToFolderListConfig[_pageName])
                {
                    if (LocalFolders[_folderName].IsFolderFound == false)
                    {
                        await LocalFolders[_folderName].FindFolder();

                        if (LocalFolders[_folderName].IsFolderFound)
                            SelectionToImgList[_pageName].Union(LocalFolders[_folderName].ImageList);
                    }
                }

                MyImageArrangement.ImgList = SelectionToImgList[_pageName];
            }

            else
            {
                SortableObservableCollection<PictureClass> _tempList = new();

                foreach (var _folderName in MyConfig.GalleryToFolderListConfig[_pageName])
                {
                    await LocalFolders[_folderName].FindFolder();

                    if (LocalFolders[_folderName].IsFolderFound)
                        _tempList.Union(LocalFolders[_folderName].ImageList);                                         
                }

                SelectionToImgList.Add(_pageName, _tempList);

                MyImageArrangement.ImgList = _tempList;
            }

            MyImageArrangement.SortImg(0);

            MyImageArrangement.SetImgSize(
                new Size[] { new(500, 125), new(500, 150), new(500, 250) },
                new Size(500, 250),
                new double[] { 400, 900, 1400 },
                12, 12
            );

        }

        public async void DeleteImg(PictureClass _img)
        {
            MyImageArrangement.ImgList.Remove(_img);

            MyImageArrangement.UpdateImgRect();

            for (int _index = _img.Index; _index < MyImageArrangement.ImgList.Count; _index++)
                MyImageArrangement.ImgList[_index].Index = _index;

            LocalFolders[_img.SourceName].ImageList.Remove(_img);

            Debug.Print("Delete " + _img.ImageLocation);

            var _tempImg = await StorageFile.GetFileFromPathAsync(_img.ImageLocation);
            //await _tempImg.DeleteAsync();
        }

    }
}
