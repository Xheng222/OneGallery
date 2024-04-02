using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.Devices.Pwm;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace OneGallery
{
    internal class LocalFolder
    {
        public string FolderPath { get; set; }

        public string FolderName { get; set; }

        public StorageFolder Folder { get; set; }

        public FileSystemWatcher Watcher { get; set; }

        public bool IsFolderFound = false;

        public bool IsImageFound = false;

        public SortableObservableCollection<PictureClass> ImageList { get; set; }
        
        public event EventHandler FolderEvent;
        
        private void DeleteFolderEvent()
        {
            FolderEvent.Invoke(null, null);
        }

        public event EventHandler FolderExist;

        private void FolderExistEvent()
        {
            FolderExist.Invoke(FolderName, null);
        }

        public event EventHandler FolderNotFound;

        private void FolderNotFoundEvent()
        {
            FolderNotFound.Invoke(FolderName, null);
        }

        private void DeleteWatcher()
        {
            Watcher?.Dispose();
            Watcher = null;
        }

        private void SetWatcher()
        {
            Watcher = new FileSystemWatcher(FolderPath);

            Watcher.EnableRaisingEvents = true;

            Watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size;

            Watcher.IncludeSubdirectories = true;

            Watcher.Deleted += OnDeleted;
            Watcher.Changed += OnChanged;
            Watcher.Renamed += OnRenamed;
            Watcher.Created += OnCreated;
            Watcher.Error += OnError;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Debug.Print("Created " + e.FullPath);
        }

        private void OnRenamed(object sender, FileSystemEventArgs e)
        {
            Debug.Print("Renamed " + e.FullPath);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Debug.Print("delete " + e.FullPath);
            DeleteFolderEvent();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Debug.Print("changed " + e.FullPath);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Watcher.Dispose();
            Watcher = null;
        }

        private async void BackGroundFindFolder()
        {
            while (true)
            {
                try
                {
                    Folder = await StorageFolder.GetFolderFromPathAsync(FolderPath);

                    if (IsFolderFound == false)
                    {
                        SetWatcher();
                        await SearchImages();
                        IsFolderFound = true;
                        FolderExistEvent();
                    }
                }
                catch (FileNotFoundException)
                {
                    if (IsFolderFound)
                    {
                        //ImageList.Clear();
                        DeleteWatcher();
                        IsFolderFound = false;
                        FolderNotFoundEvent();
                    }

                    
                    //continue;
                }

                await Task.Delay(2000);
            }
            
        }
        public LocalFolder(string _path, string _name) 
        {
            FolderPath = _path;
            FolderName = _name;
            ImageList = new();
            BackGroundFindFolder();
        }

        public async Task FindFolder()
        {
            //try
            //{
            //    Folder = await StorageFolder.GetFolderFromPathAsync(FolderPath);
            //}
            //catch (FileNotFoundException)
            //{
            //    IsFolderFound = false;
            //    Watcher?.Dispose();
            //    Watcher = null;
            //    ImageList.Clear();
            //    BackGroundFindFolder();
            //    return;
            //}

            //if (Watcher == null)
            //    SetWatcher();

            //if (IsImageFound == false)
            //    await SearchImages();
            
            //IsFolderFound = true;
        }

        public async Task SearchImages()
        {
            var _count = 0;

            while (true)
            {
                ImageList.Clear();

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
                            ImageList.Add(new PictureClass(
                                _image.Path,
                                _image.Name,
                                imageProps.Width,
                                imageProps.Height,
                                FolderName
                            ));
                            i++;
                        }

                    }
                    _index += _step;
                    _images = await _queryResult.GetFilesAsync(_index, _step);
                }

                if (ImageList.Count == _count)
                    break;
                else
                    _count = ImageList.Count;
            }
        }
    
    
    }
}
