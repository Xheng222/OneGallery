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
    class Config
    {
        public Dictionary<string, string> FolderPathConfig { get; set; }

        public Dictionary<string, List<string>> SelectionToFolderListConfig { get; set; }

        [JsonIgnore]
        public StorageFile ConfigFile { get; set; }

        public async void ToConfigFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            //FolderPathConfig.Add("Folder4", "H:\\1234\\新建文件夹\\新建文件夹\\新建文件夹");
            var jsonString = JsonSerializer.Serialize(this, options);
            
            Debug.Print(jsonString + "");
            //var buffer = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(
            //        jsonString, Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
            //var _configFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Res/settings.json"));

            await FileIO.WriteTextAsync(ConfigFile, jsonString);

            //await FileIO.WriteBufferAsync(_configFile, buffer);

            //var stream = await _configFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

            Debug.Print("Finish");
        }
    }
}
