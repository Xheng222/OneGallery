using System;
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
    class Config
    {
        public Dictionary<string, string> FolderPathConfig { get; set; }

        public Dictionary<string, List<string>> GalleryToFolderListConfig { get; set; }

        public Dictionary<string, string> FolderToFolderListConfig { get; set; }

        [JsonIgnore]
        public StorageFile ConfigFile { get; set; }

        public async void ToConfigFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(this, options);
        
            Debug.Print(jsonString + "");

            await FileIO.WriteTextAsync(ConfigFile, jsonString);

            Debug.Print("Finish");

   
        }

    }
}
