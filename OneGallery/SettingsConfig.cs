using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace OneGallery
{
    public class SettingsConfig
    {
        public int LastWidth { get; set; }

        public int LastHeight { get; set; }



        [JsonIgnore]
        public StorageFile ConfigFile { get; set; }

        public async void StoreSettingsConfig(int _lastWidth, int _lastHeight)
        {
            LastWidth = _lastWidth;
            LastHeight = _lastHeight;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(this, options);

            await FileIO.WriteTextAsync(ConfigFile, jsonString);
        }

    }
}
