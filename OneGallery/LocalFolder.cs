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

        public string FolderPath { get; set; }

        public string FolderName { get; set; }

        public StorageFolder Folder { get; set; }

        public FileSystemWatcher Watcher { get; set; }

        public StorageLibraryChangeTracker FolderTracker { get; set; }

        public StorageLibraryChangeReader FolderChangeReader { get; set; }

        public SortableObservableCollection<PictureClass> ImageList { get; set; }



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
            Watcher.Created += OnCreated;
            Watcher.Renamed += OnRenamed;
            Watcher.Error += OnError;

        }

        /*
         * FileCreated
         */

        public event EventHandler FileCreated;

        private void FileCreatedEvent()
        {
            FileCreated.Invoke(FolderName, null);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {


            //_tempImage.Name = e.Name;
            //_tempImage.ImageLocation = e.FullPath;


            Debug.Print("Created " + e.Name);
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
            //if (IsImageChanged(e.Name))
            //{
            //    var _tempImage = ImageList.Find<PictureClass>(x => x.ImageLocation, e.OldFullPath);
            //    if (_tempImage != null)
            //    {
            //        var _newName = e.Name.Split('\\').Last();

            //        //_tempImage._Name = _newName;
            //        //_tempImage._ImageLocation = e.FullPath;

            //        Debug.Print("Renamed " + _newName);

            //        FileRenamedEvent(e);
            //    }
            //}
            //else
            //{
            //    try
            //    {
            //        Folder = await StorageFolder.GetFolderFromPathAsync(e.FullPath);
            //        foreach (var item in ImageList)
            //        {
            //            if (item._ImageLocation.Contains(e.OldFullPath))
            //                item._ImageLocation = item._ImageLocation.Replace(e.OldFullPath, e.FullPath);
            //        }
            //        FileRenamedEvent();
            //    }
            //    catch (Exception)
            //    {
            //        return;
            //    }
            //}


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
                var _tempImage = ImageList.Find<PictureClass>(x => x.ImageLocation, e.FullPath);

                if (_tempImage == null)
                {
                    Debug.Print("NULL FIND");
                }
                else
                {
                    FileChangeEvent _imageChangeEvent = new(_tempImage);
                    FileDeletedEvent(_imageChangeEvent);
                    ImageList.Remove(_tempImage);
                }
            }
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
                        await SearchImages();
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
                        //ImageList.Clear();
                        DeleteWatcher();
                        IsFolderFound = false;
                        FolderNotFoundEvent();
                    }
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

                        //Debug.Print(_image.Name);
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

        public bool IsImageChanged(string _fileName)
        {
            var _suffix = _fileName.Split('.').Last();
            if (_suffix == "png" || _suffix == "jpg" || _suffix == "gif" || _suffix == "bmp")
                return true;

            return false;
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
