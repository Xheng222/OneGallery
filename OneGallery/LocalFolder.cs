using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
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
        public bool IsFolderFound = false;

        private bool Stop = false;

        private Task CheckFolderTask { get; set; }

        public string FolderPath { get; set; }

        public string FolderName { get; set; }

        public StorageFolder Folder { get; set; }

        public FileSystemWatcher Watcher { get; set; }

        public List<PictureClass> ImageList { get; set; }



        /*
         * FolderExistEvent
         */

        public event EventHandler FolderExist;

        private void FolderExistEvent()
        {
            FolderExist.Invoke(FolderName, null);
        }

        /*
         * FolderNotFoundEvent
         */

        public event EventHandler FolderNotFound;

        private void FolderNotFoundEvent()
        {
            FolderNotFound.Invoke(FolderName, null);
        }

        /*
         * Watcher
         */

        private void DeleteWatcher()
        {
            Watcher?.Dispose();
            Watcher = null;
        }

        public void SetWatcher()
        {
            Watcher = new FileSystemWatcher(FolderPath)
            {
                EnableRaisingEvents = true,

                NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size,

                IncludeSubdirectories = true
            };

            Watcher.Deleted += OnDeleted;
            Watcher.Created += OnCreated;
            Watcher.Renamed += OnRenamed;
            Watcher.Error += OnError;

        }

        /*
         * FileCreated
         */

        public event EventHandler FileCreated;

        private void FileCreatedEvent(FileChangeEvent e)
        {
            FileCreated.Invoke(FolderName, e);
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            Debug.Print("Created " + e.Name);
            if (IsImageChanged(e.Name))
            {
                StorageFile _image = await StorageFile.GetFileFromPathAsync(e.FullPath);
                var imageProps = await _image.Properties.GetImagePropertiesAsync();
                var basicProperties = await _image.GetBasicPropertiesAsync();

                var _newImage = new PictureClass(
                    _image.Path,
                    _image.Name,
                    imageProps.Width,
                    imageProps.Height,
                    FolderName,
                    _image.DateCreated,
                    basicProperties.DateModified,
                    (empty == imageProps.DateTaken) ? basicProperties.DateModified : imageProps.DateTaken
                );

                FileCreatedEvent(new(_newImage));
            }
        }

        /*
         * FileReNamedEvent
         */

        public event EventHandler FileRenamed;

        private void FileRenamedEvent(RenamedEventArgs e)
        {
            FileRenamed.Invoke(FolderName, e);

        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            FileRenamedEvent(e);
        }

        /*
         * FileDeletedEvent
         */

        public event EventHandler FileDeleted;

        private void FileDeletedEvent(FileChangeEvent e)
        {
            FileDeleted.Invoke(FolderName, e);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Debug.Print("Delete " + e.Name);

            if(IsImageChanged(e.Name))
            {
                var _tempImage = ImageList.Find(x => x.ImageLocation == e.FullPath);

                if (_tempImage == null)
                {
                    Debug.Print("NULL FIND");
                }
                else
                {
                    FileDeletedEvent(new(_tempImage));
                }
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Watcher.Dispose();
            Watcher = null;
        }

        public async Task FirstInitFolder()
        {
            await GetFolder(true);
            CheckFolderTask = BackGroundFindFolder();
        }

        private async Task BackGroundFindFolder()
        {
            while (!Stop)
            {
                await Task.Delay(4000);
                await GetFolder(false);
            }
        }

        private async Task GetFolder(bool _isFirst)
        {
            try
            {
                Folder = await StorageFolder.GetFolderFromPathAsync(FolderPath);

                if (IsFolderFound == false)
                {
                    await SearchImages(_isFirst);
                    IsFolderFound = true;
                    FolderExistEvent();
                }

                if (Watcher is null)
                    SetWatcher();
            }
            catch (FileNotFoundException)
            {
                if (IsFolderFound)
                {
                    DeleteWatcher();
                    IsFolderFound = false;
                    FolderNotFoundEvent();
                }
            }
        }
        public LocalFolder(string _path, string _name) 
        {
            FolderPath = _path;
            FolderName = _name;
            ImageList = new();

        }

        static readonly DateTimeOffset empty = new(1601, 1, 1, 8, 0, 0,
                     new TimeSpan(8, 0, 0));
        public async Task SearchImages(bool _isfirst)
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
                _imgQuery.FileTypeFilter.Add(".ico");
                _imgQuery.FileTypeFilter.Add(".tiff");

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
                                FolderName,
                                _image.DateCreated,
                                basicProperties.DateModified,
                                (empty == imageProps.DateTaken)? basicProperties.DateModified : imageProps.DateTaken
                            ));
                            i++;
                        }
                        
                    }
                    _index += _step;
                    _images = await _queryResult.GetFilesAsync(_index, _step);
                }


                if (_isfirst)
                    break;
                else
                {
                    if (_count == ImageList.Count)
                        break;
                    else
                        _count = ImageList.Count;
                }
            }
        }

        public static bool IsImageChanged(string _fileName)
        {
            var _suffix = _fileName.Split('.').Last();
            if (_suffix == "png" || _suffix == "jpg" || _suffix == "gif" || _suffix == "bmp")
                return true;

            return false;
        }

        public async void Close()
        {
            Stop = true;
            await CheckFolderTask;
            Watcher.Dispose();
            ImageList = null;
        }
    }

    internal class FileChangeEvent: EventArgs
    {
        public PictureClass File { get; set; }

        public FileChangeEvent(PictureClass file)
        {
            File = file;
        }
    }


}
