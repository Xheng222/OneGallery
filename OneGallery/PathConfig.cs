﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace OneGallery
{
    public class PathConfig
    {
        public Dictionary<string, string> FolderPathConfig { get; set; }

        public Dictionary<string, List<string>> GalleryToFolderListConfig { get; set; }


        [JsonIgnore]
        public StorageFile ConfigFile { get; set; }

        public async void StorePathConfig(Category _galleries, Category _folders)
        {
            Dictionary<string, List<string>> _tempGalleries = new();
            foreach (var _item in _galleries.Children)
            {
                if (_item is Category _gallery)
                {
                    if (!_gallery.IsAddSelection)
                    {
                        _tempGalleries.Add(_gallery.Name, GalleryToFolderListConfig[_gallery.Name]);
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
                        _tempFolders.Add(_folder.Name, FolderPathConfig[_folder.Name]);
                    }
                }
            }
            FolderPathConfig = _tempFolders;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(this, options);
            await FileIO.WriteTextAsync(ConfigFile, jsonString);
        }

    }
}