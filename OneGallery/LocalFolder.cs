using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace OneGallery
{
    internal class LocalFolder
    {
        private Dictionary<string, SortableObservableCollection<PictureClass>> FolderNameToImgList = new();

        
        public Dictionary<string, SortableObservableCollection<PictureClass>> SelectionToImgList = new();

        private bool FolderInitSuccess = false;

        private Config MyConfig {  get; set; }
        
        public ImageArrangement MyImageArrangement { get; set; }
       
        public LocalFolder()
        {
            MyImageArrangement = new();
        }

        public async Task InitConfig()
        {
            var _configFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Res/settings.json"));
            string _configString = await FileIO.ReadTextAsync(_configFile);
            Debug.Print(_configString);
            MyConfig = JsonSerializer.Deserialize<Config>(_configString);
            MyConfig.ConfigFile = _configFile;
            MyConfig.ToConfigFile();
        }

        public async void InitFolder()
        {
            await InitConfig();

            foreach (var _foldPath in MyConfig.FolderPathConfig)
            {
                StorageFolder Folder;

                try
                {
                    Folder = await StorageFolder.GetFolderFromPathAsync(_foldPath.Value);

                }
                catch (FileNotFoundException)
                {
                    continue;
                }

                await SearchFolderImg(Folder, _foldPath.Key);
            }

            FolderInitSuccess = true;
        }



        public async Task InitPageFolder(string _pageName)
        {
            while (!FolderInitSuccess)
                await Task.Delay(1000);

            if (SelectionToImgList.ContainsKey(_pageName))
            {
                MyImageArrangement.ImgList = SelectionToImgList[_pageName];

                MyImageArrangement.SetImgSize(
                    new Size[] { new(500, 125), new(500, 150), new(500, 250) },
                    new Size(500, 250),
                    new double[] { 400, 900, 1400 },
                    12, 12
                );
            }

            else
            {
                SortableObservableCollection<PictureClass> _tempList = new();

                foreach (var _folderName in MyConfig.SelectionToFolderListConfig[_pageName])
                {
                    _tempList.Union(FolderNameToImgList[_folderName]);
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
        public async Task SearchFolderImg(StorageFolder Folder, string _sourceName)
        {
            SortableObservableCollection<PictureClass> _imgList = new();

            QueryOptions _imgQuery = new()
            {
                FolderDepth = FolderDepth.Deep,
                ApplicationSearchFilter = "System.Security.EncryptionOwners:[]",
                IndexerOption = IndexerOption.UseIndexerWhenAvailable,    
            };

            _imgQuery.FileTypeFilter.Add(".jpg");
            _imgQuery.FileTypeFilter.Add(".png");
            _imgQuery.FileTypeFilter.Add(".bmp");
            _imgQuery.FileTypeFilter.Add(".gif");

            _imgQuery.SetPropertyPrefetch(PropertyPrefetchOptions.BasicProperties | PropertyPrefetchOptions.ImageProperties, null);

            uint _index = 0;
            const int _step = 100;

            StorageFileQueryResult _queryResult = Folder.CreateFileQueryWithOptions(_imgQuery);
            IReadOnlyList<StorageFile> _images = await _queryResult.GetFilesAsync(_index, _step);
            int i = 0;
            while (_images.Count != 0)
            {
                foreach (StorageFile _image in _images)
                {
                    var imageProps = await _image.Properties.GetImagePropertiesAsync();
                    var basicProperties = await _image.GetBasicPropertiesAsync();

                    for (int j = 0; j < 1; j++)
                    {
                        _imgList.Add(new PictureClass(
                            _image.Path,
                            _image.Name,
                            imageProps.Width,
                            imageProps.Height,
                            _sourceName
                        ));
                        i++;
                    }

                }
                _index += _step;
                _images = await _queryResult.GetFilesAsync(_index, _step);
            }

            FolderNameToImgList.Add(_sourceName, _imgList);
        }

        public async void DeleteImg(PictureClass _img)
        {
            MyImageArrangement.ImgList.Remove(_img);

            MyImageArrangement.UpdateImgRect();

            for (int _index = _img.Index; _index < MyImageArrangement.ImgList.Count; _index++)
                MyImageArrangement.ImgList[_index].Index = _index;

            FolderNameToImgList[_img.SourceName].Remove(_img);

            Debug.Print("Delete " + _img.ImageLocation);

            var _tempImg = await StorageFile.GetFileFromPathAsync(_img.ImageLocation);
            //await _tempImg.DeleteAsync();
        }

    }
}
