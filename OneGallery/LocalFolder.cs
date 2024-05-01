using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.Management.Deployment;
using Windows.ApplicationModel.Background;
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

        private Task CheckFolderTask { get; set; } = Task.CompletedTask;

        public string FolderPath { get; set; }

        public string FolderName { get; set; }

        public string ImageJsonPath { get; set; }

        public StorageFolder Folder { get; set; }

        public FileSystemWatcher Watcher { get; set; }

        public List<PictureClass> ImageList { get; set; }


        private enum SearchMode
        {
            Add,
            Search,
            SearchLoop
        }


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

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (IsImage(e.Name))
            {
                //StorageFile _image = await StorageFile.GetFileFromPathAsync(e.FullPath);
                //var imageProps = await _image.Properties.GetImagePropertiesAsync();
                //var basicProperties = await _image.GetBasicPropertiesAsync();
                try
                {
                    FileInfo _file = new(e.FullPath);
                    using Stream stream = File.OpenRead(e.FullPath);
                    using Image sourceImage = Image.FromStream(stream, false, false);

                    if (sourceImage.Width != 0 && sourceImage.Height != 0)
                    {
                        DateTime _dateTaken;

                        try
                        {
                            PropertyItem propItem = sourceImage.GetPropertyItem(36867);
                            string dateTaken = Encoding.ASCII.GetString(propItem.Value);
                            _dateTaken = DateTime.ParseExact(dateTaken.Trim('\0'), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                            _dateTaken = _file.LastWriteTime;
                        }

                        if (sourceImage.Width != 0 && sourceImage.Height != 0)
                        {
                            var _newImage = new PictureClass(
                                            _file.FullName,
                                            _file.Name,
                                            (uint)sourceImage.Width,
                                            (uint)sourceImage.Height,
                                            _file.CreationTime,
                                            _file.LastWriteTime,
                                            _dateTaken);

                            FileCreatedEvent(new(_newImage));
                        }
                    }
                }
                catch (Exception) { }

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

            if(IsImage(e.Name))
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
            else
            {
                List<PictureClass> list = new();

                foreach (var _image in ImageList)
                {
                    if (_image.ImageLocation.Contains(e.FullPath))
                        list.Add(_image);
                }

                foreach (var _image in list)
                    FileDeletedEvent(new(_image));
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Watcher.Dispose();
            Watcher = null;
        }

        public static bool IsImage(string _fileName)
        {
            var _suffix = _fileName.Split('.').Last();
            if (_suffix == "png" || _suffix == "jpg" || _suffix == "gif" || 
                _suffix == "bmp" || _suffix == "ico" || _suffix == "tiff")
                return true;

            return false;
        }        

        /*
         * 
         */

        public LocalFolder(string _path, string _name)
        {
            FolderPath = _path;
            FolderName = _name;
            ImageJsonPath = ApplicationData.Current.LocalFolder.Path + "\\" + FolderName + ".json";
        }

        public async void RenameFolder(string _newName)
        {
            FolderName = _newName;

            try
            {
                StorageFile _jsonfile = await StorageFile.GetFileFromPathAsync(ImageJsonPath);
                await _jsonfile.RenameAsync(_newName + ".json");
            }
            catch (Exception)
            {

            }

            ImageJsonPath = ApplicationData.Current.LocalFolder.Path + "\\" + FolderName + ".json";
        }
        
        public async Task DeleteFolder()
        {
            Stop = true;
            await CheckFolderTask;
            Watcher?.Dispose();

            try
            {
                StorageFile _jsonfile = await StorageFile.GetFileFromPathAsync(ImageJsonPath);
                await _jsonfile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception)
            {
                return;
            }
        }

        public async Task FirstInitFolder()
        {
            CheckFolderTask = GetFolder(true);
            await CheckFolderTask;

            ImageList ??= new();

            if (!Stop)
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
                if (!Directory.Exists(FolderPath))
                {
                    throw new FileNotFoundException();
                }

                if (IsFolderFound == false)
                {
                    if (_isFirst)
                    {
                        bool _isJsonExist = await GetFolderImageFromJson();
                        if (!_isJsonExist)
                        {
                            ImageList = new();
                            SearchImages(SearchMode.Search);
                        }
                        else
                        {
                            SearchImages(SearchMode.Add);
                        }

                    }
                    else
                    {
                        SearchImages(SearchMode.SearchLoop);
                    }


                    if (!Stop)
                    {
                        IsFolderFound = true;
                        FolderExistEvent();
                    }
                }

                if (Watcher is null && !Stop)
                    SetWatcher();
            }
            catch (Exception)
            {
                if (IsFolderFound)
                {
                    DeleteWatcher();
                    IsFolderFound = false;
                    FolderNotFoundEvent();
                }
            }


        }

        private async Task<bool> GetFolderImageFromJson()
        {
            try
            {
                StorageFile _jsonfile = await StorageFile.GetFileFromPathAsync(ImageJsonPath);
                string _imageJson = await FileIO.ReadTextAsync(_jsonfile);
                ImageList = JsonSerializer.Deserialize<List<PictureClass>>(_imageJson);

                List<PictureClass> _notFound = new();
                foreach (PictureClass _image in ImageList)
                {
                    if (!File.Exists(_image.ImageLocation))
                    {
                        _notFound.Add(_image);
                    }
                }

                foreach (PictureClass _image in _notFound)
                {
                    ImageList.Remove(_image);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        //static readonly DateTimeOffset empty = new(1601, 1, 1, 8, 0, 0,
        //             new TimeSpan(8, 0, 0));
        //private async Task SearchImages(SearchMode _searchMode, bool aa)
        //{
        //    var _count = 0;

        //    while (true)
        //    {
        //        if (_searchMode != SearchMode.Add)
        //            ImageList.Clear();

        //        QueryOptions _imgQuery = new()
        //        {
        //            FolderDepth = FolderDepth.Deep,
        //            ApplicationSearchFilter = "System.Security.EncryptionOwners:[]",
        //            IndexerOption = IndexerOption.UseIndexerWhenAvailable,
        //        };

        //        _imgQuery.FileTypeFilter.Add(".jpg");
        //        _imgQuery.FileTypeFilter.Add(".png");
        //        _imgQuery.FileTypeFilter.Add(".bmp");
        //        _imgQuery.FileTypeFilter.Add(".gif");
        //        _imgQuery.FileTypeFilter.Add(".ico");
        //        _imgQuery.FileTypeFilter.Add(".tiff");

        //        _imgQuery.SetPropertyPrefetch(PropertyPrefetchOptions.BasicProperties | PropertyPrefetchOptions.ImageProperties, null);

        //        uint _index = 0;
        //        const int _step = 500;

        //        StorageFileQueryResult _queryResult = Folder.CreateFileQueryWithOptions(_imgQuery);
        //        IReadOnlyList<StorageFile> _images = await _queryResult.GetFilesAsync(_index, _step);

        //        int i = 0;

        //        while (_images.Count != 0 && !Stop)
        //        {
        //            foreach (StorageFile _image in _images)
        //            {
        //                var imageProps = await _image.Properties.GetImagePropertiesAsync();
        //                var basicProperties = await _image.GetBasicPropertiesAsync();

        //                //if (imageProps.Height != 0 && imageProps.Width != 0)
        //                //{
        //                //    var _temp = new PictureClass(
        //                //        _image.Path,
        //                //        _image.Name,
        //                //        imageProps.Width,
        //                //        imageProps.Height,
        //                //        _image.DateCreated,
        //                //        basicProperties.DateModified,
        //                //        (empty == imageProps.DateTaken) ? basicProperties.DateModified : imageProps.DateTaken);

        //                //    if (_searchMode == SearchMode.Add)
        //                //    {
        //                //        if (ImageList.Find(x=> x.ImageLocation == _temp.ImageLocation) is null)
        //                //            FileCreatedEvent(new(_temp));
        //                //    }
        //                //    else
        //                //    {
        //                //        ImageList.Add(_temp);
        //                //    }

        //                //    i++;

        //                //}
        //            }
        //            _index += _step;
        //            _images = await _queryResult.GetFilesAsync(_index, _step);
        //        }


        //        if (_searchMode == SearchMode.Add || _searchMode == SearchMode.Search)
        //            break;
        //        else
        //        {
        //            if (_count == ImageList.Count)
        //                break;
        //            else
        //                _count = ImageList.Count;
        //        }
        //    }

        //}

        private void SearchImages(SearchMode _searchMode)
        {
            var _count = 0;
            Queue<DirectoryInfo> _foldersQueue = new();

            while (true)
            {
                if (_searchMode != SearchMode.Add)
                    ImageList.Clear();

                _foldersQueue.Enqueue(new DirectoryInfo(FolderPath));

                while (_foldersQueue.Count > 0 && !Stop)
                {
                    DirectoryInfo _folder = _foldersQueue.Dequeue();

                    try
                    {
                        foreach (FileInfo _file in _folder.EnumerateFiles())
                        {
                            if (_file.Attributes == System.IO.FileAttributes.System)
                                continue;

                            if (IsImage(_file.Name))
                            {
                                try
                                {
                                    DateTime _dateTaken;

                                    using Stream stream = File.OpenRead(_file.FullName);
                                    using Image sourceImage = Image.FromStream(stream, false, false);
                                    try
                                    {
                                        PropertyItem propItem = sourceImage.GetPropertyItem(36867);
                                        string dateTaken = Encoding.ASCII.GetString(propItem.Value);
                                        _dateTaken = DateTime.ParseExact(dateTaken.Trim('\0'), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception)
                                    {
                                        _dateTaken = _file.LastWriteTime;
                                    }

                                    if (sourceImage.Width != 0 && sourceImage.Height != 0)
                                    {
                                        if (_searchMode == SearchMode.Add)
                                        {
                                            if (ImageList.Find(x => x.ImageLocation == _file.FullName) is null)
                                            {
                                                var _temp = new PictureClass(
                                                               _file.FullName,
                                                               _file.Name,
                                                               (uint)sourceImage.Width,
                                                               (uint)sourceImage.Height,
                                                               _file.CreationTime,
                                                               _file.LastWriteTime,
                                                               _dateTaken);

                                                FileCreatedEvent(new(_temp));
                                            }
                                        }
                                        else
                                        {
                                            var _temp = new PictureClass(
                                                           _file.FullName,
                                                           _file.Name,
                                                           (uint)sourceImage.Width,
                                                           (uint)sourceImage.Height,
                                                           _file.CreationTime,
                                                           _file.LastWriteTime,
                                                           _dateTaken);

                                            ImageList.Add(_temp);
                                        }


                                    }
                                }
                                catch (Exception) { }
                            }
                        }

                        foreach (DirectoryInfo _subFolder in _folder.EnumerateDirectories())
                        {
                            _foldersQueue.Enqueue(_subFolder);
                        }
                    }
                    catch (Exception) { }




                }

                if (_searchMode != SearchMode.SearchLoop)
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
