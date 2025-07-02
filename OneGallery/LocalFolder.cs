using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Windows.Storage;


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

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (IsImage(e.Name))
            {
                PictureClass _newImage = await GetImageAsync(new(e.FullPath));
                if (_newImage != null && !Stop)
                {
                    FileCreatedEvent(new(_newImage));
                }

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
            if (!Stop)
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
            if (Stop)
                return;

            if (IsImage(e.Name))
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

                lock (ImageList)
                {
                    foreach (var _image in ImageList)
                    {
                        if (_image.ImageLocation.Contains(e.FullPath))
                            list.Add(_image);
                    }
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
                _suffix == "bmp" || _suffix == "tiff" || _suffix == "webp")
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

        public void Close()
        {
            Stop = true;
            Watcher?.Dispose();
        }
        public async Task DeleteFolder()
        {
            Close();

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
            await GetFolder(true);
            ImageList ??= new();
            BackGroundFindImage();
            await Task.Delay(500);
        }

        private async void BackGroundFindImage()
        {
            await CheckFolderTask;
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
                            await SearchImages(SearchMode.Search);

                        }
                        else
                        {
                            CheckFolderTask = SearchImages(SearchMode.Add);
                        }
                    }
                    else
                    {
                        await SearchImages(SearchMode.SearchLoop);
                    }


                    if (!Stop)
                    {
                        IsFolderFound = true;
                        FolderExistEvent();
                    }
                }

                if (Watcher is null)
                    SetWatcher();
            }
            catch (Exception)
            {
                if (IsFolderFound && !Stop)
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

        private async Task SearchImages(SearchMode _searchMode)
        {
            SearchEvent(false);

            var _count = 0;

            if (_searchMode == SearchMode.Add)
            {
                await Task.Delay(500);
                _count = ImageList.Count;
            }

            Queue<DirectoryInfo> _foldersQueue = new();

            while (true)
            {
                if (_searchMode != SearchMode.Add)
                    ImageList.Clear();

                _foldersQueue.Enqueue(new DirectoryInfo(FolderPath));

                while (_foldersQueue.Count > 0)
                {
                    DirectoryInfo _folder = _foldersQueue.Dequeue();

                    foreach (FileInfo _file in _folder.EnumerateFiles())
                    {
                        if (_file.Attributes == System.IO.FileAttributes.System)
                            continue;

                        if (Stop)
                            return;

                        if (IsImage(_file.Name))
                        {
                            PictureClass _image = null;

                            if (_searchMode == SearchMode.Add)
                            {
                                if (_count != 0)
                                {
                                    if (ImageList.Find(x => x.ImageLocation == _file.FullName) is not null)
                                    {
                                        _count--;
                                        continue;
                                    }
                                }

                                _image = await GetImageAsync(_file);
                                if (_image is not null)
                                {
                                    FileCreatedEvent(new(_image));
                                }
                            }
                            else
                            {
                                _image = await GetImageAsync(_file);
                                if (_image is not null)
                                {
                                    ImageList.Add(_image);
                                }
                            }
                        }
                    }

                    foreach (DirectoryInfo _subFolder in _folder.EnumerateDirectories())
                    {
                        _foldersQueue.Enqueue(_subFolder);
                    }
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

            SearchEvent(true);
        }

        private static async Task<PictureClass> GetImageAsync(FileInfo _file)
        {
            PictureClass _image = null;
            int _flag = 0;
            while (_flag < 10)
            {
                try
                {
                    //using Stream stream = File.OpenRead(_file.FullName);
                    //ImageInfo imageInfo = Image.Identify(stream);
                    ImageInfo imageInfo = Image.Identify(_file.FullName);

                    if (imageInfo.Width != 0 && imageInfo.Height != 0)
                    {
                        var _exif = imageInfo.Metadata.ExifProfile;
                        DateTime _dateTaken = _file.LastWriteTime;

                        if (_exif is not null)
                        {
                            try
                            {
                                if (_exif.TryGetValue(ExifTag.DateTimeOriginal, out IExifValue<string> _time))
                                {
                                    _dateTaken = DateTime.ParseExact((_time.GetValue() as string).Trim('\0'), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                                else if (_exif.TryGetValue(ExifTag.DateTimeDigitized, out _time))
                                {
                                    _dateTaken = DateTime.ParseExact((_time.GetValue() as string).Trim('\0'), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                                else if (_exif.TryGetValue(ExifTag.DateTime, out _time))
                                {
                                    _dateTaken = DateTime.ParseExact((_time.GetValue() as string).Trim('\0'), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                            }
                            catch (Exception) { }
                        }


                        _image = new PictureClass(
                                        _file.FullName,
                                        _file.Name,
                                        imageInfo.Width,
                                        imageInfo.Height,
                                        _file.CreationTime,
                                        _file.LastWriteTime,
                                        _dateTaken);
                    }

                    break;
                }
                catch (IOException e)
                {
                    Debug.Print("IOException " + e);
                    await Task.Delay(1000);
                }
                catch (UnauthorizedAccessException e)
                {
                    Debug.Print("UnauthorizedAccessException " + e);
                    await Task.Delay(1000);
                }
                catch (Exception)
                {
                    break;
                }

                _flag++;
            }

            return _image;
        }


        public event EventHandler SearchTask;

        private void SearchEvent(bool _isEnd)
        {
            SearchTask.Invoke(FolderName, new SearchChangeEvent(_isEnd));
        }

    }

    internal class FileChangeEvent : EventArgs
    {
        public PictureClass File { get; set; }

        public FileChangeEvent(PictureClass file)
        {
            File = file;
        }
    }

    internal class SearchChangeEvent : EventArgs
    {
        public bool IsEnd { get; set; }

        public SearchChangeEvent(bool isEnd)
        {
            IsEnd = isEnd;
        }
    }

}
