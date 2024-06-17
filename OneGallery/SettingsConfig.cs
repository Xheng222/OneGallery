using System;
using System.Text.Json;
using Windows.Storage;

namespace OneGallery
{
    public class SettingsConfig
    {
        public int LastWidth { get; set; }

        public int LastHeight { get; set; }

        public int ChooseMode { get; set; }

        public int SortMode { get; set; }

        public bool IsAscending { get; set; }

        public int ImageSizeMode { get; set; }

        public int ImageZoomMode { get; set; }

        public int ImageHeight_Small { get; set; }

        public int ImageHeight_Medium { get; set; }

        public int ImageHeight_Large { get; set; }

        public bool DeleteToTrashcan { get; set; }

        public bool GalleryExpand { get; set; }

        public bool FolderExpand { get; set; }


        public async void StoreSettingsConfig(int _lastWidth, int _lastHeight)
        {
            LastWidth = _lastWidth;
            LastHeight = _lastHeight;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(this, options);
            var _pathFile = await StorageFile.GetFileFromPathAsync(ApplicationData.Current.LocalCacheFolder.Path + "\\Settings.json");
            await FileIO.WriteTextAsync(_pathFile, jsonString);
        }

    }
}
