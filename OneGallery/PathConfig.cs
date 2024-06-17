using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;

namespace OneGallery
{
    public class PathConfig
    {
        public Dictionary<string, string> FolderPathConfig { get; set; }

        public Dictionary<string, List<string>> GalleryToFolderListConfig { get; set; }

        public async void StorePathConfig(Category _galleries, Category _folders)
        {
            Dictionary<string, List<string>> _tempGalleries = new();
            foreach (var _item in _galleries.Children)
            {
                if (_item is Category _gallery)
                {
                    if (!_gallery.IsAddSelection)
                    {
                        _tempGalleries.Add(_gallery._name, GalleryToFolderListConfig[_gallery._name]);
                    }
                }
            }
            GalleryToFolderListConfig = _tempGalleries;

            Dictionary<string, string> _tempFolders = new();
            foreach (var _item in _folders.Children)
            {
                if (_item is Category _folder)
                {
                    if (!_folder.IsAddSelection)
                    {
                        _tempFolders.Add(_folder._name, FolderPathConfig[_folder._name]);
                    }
                }
            }
            FolderPathConfig = _tempFolders;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(this, options);
            var _pathFile = await StorageFile.GetFileFromPathAsync(ApplicationData.Current.LocalCacheFolder.Path + "\\Path.json");
            await FileIO.WriteTextAsync(_pathFile, jsonString);
        }

    }
}
