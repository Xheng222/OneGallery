using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace OneGallery
{
    internal class LocalFolder
    {

        public SortableObservableCollection<PictureClass> ImgList = new();

        public ImageArrangement MyImageArrangement { get; set; }


        StorageFolder Folder;

        string FolderPath;
        
        public LocalFolder(string _path)
        {
            FolderPath = _path;
            MyImageArrangement = new(ImgList);
        }

        public async Task Init()
        {
            Folder = await StorageFolder.GetFolderFromPathAsync(FolderPath);
            await SearchFolderImg();
            

            MyImageArrangement.SetImgSize(
                new Size[] {new(500, 125), new(500, 150), new(500, 250) },
                new Size(500, 250),
                new double[] { 400, 900, 1400 },
                12, 12
            );
        }
        public async Task SearchFolderImg()
        {

            QueryOptions _imgQuery = new QueryOptions()
            {
                FolderDepth = FolderDepth.Deep,
                // Filter out all files that have WIP enabled
                ApplicationSearchFilter = "System.Security.EncryptionOwners:[]",
                IndexerOption = IndexerOption.UseIndexerWhenAvailable,
                
            };

            _imgQuery.FileTypeFilter.Add(".jpg");
            _imgQuery.FileTypeFilter.Add(".png");
            _imgQuery.FileTypeFilter.Add(".bmp");
            _imgQuery.FileTypeFilter.Add(".gif");

            _imgQuery.SetPropertyPrefetch(PropertyPrefetchOptions.BasicProperties | PropertyPrefetchOptions.ImageProperties, null);

            //SortEntry sortOrder = new()
            //{
            //    AscendingOrder = true,
            //    PropertyName = "System.FileName" // FileName property is used as an example. Any property can be used here.  
            //};
            //_imgQuery.SortOrder.Add(sortOrder);

            uint _index = 0;
            const int _step = 100;

            StorageFileQueryResult _queryResult = Folder.CreateFileQueryWithOptions(_imgQuery);
            IReadOnlyList<StorageFile> _images = await _queryResult.GetFilesAsync(_index, _step);
            int i = 0;
            while (_images.Count != 0)
            {
                foreach (StorageFile _image in _images)
                {
                    // With the OnlyUseIndexerAndOptimizeForIndexedProperties set, this won't  
                    // be async. It will run synchronously. 
                    var imageProps = await _image.Properties.GetImagePropertiesAsync();
                    var basicProperties = await _image.GetBasicPropertiesAsync();

                    for (int j = 0; j < 5; j++)
                    {
                        ImgList.Add(new PictureClass(
                            _image.Path,
                            _image.Name,
                            imageProps.Width,
                            imageProps.Height,
                            i
                        ));
                        i++;
                    }
                    


                }
                _index += _step;
                _images = await _queryResult.GetFilesAsync(_index, _step);
            }


        }


    }
}
