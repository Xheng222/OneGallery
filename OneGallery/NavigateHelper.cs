using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;

namespace OneGallery
{
    internal class NavigateHelper
    {    
        private static readonly Dictionary<string, PageStackContent> DictPageForGalleryPage = new();

        private static readonly Dictionary<string, PageStackContent> DictPageForFolderPage = new();

        private static readonly PageStackContent ContentForHomePage = new();

        public static void GetContent(MainWindow _window, Frame _frame, Category _nowPage)
        {
            ImageListPage _tempPage = null;

            if (_nowPage.IsHomePage)
            {
                _tempPage = (ImageListPage)ContentForHomePage.PageContent;
            }
            else if (_nowPage.IsGallery)
            {
                if (DictPageForGalleryPage.ContainsKey(_nowPage.Name))
                    _tempPage = (ImageListPage)DictPageForGalleryPage[_nowPage.Name].PageContent;
            }
            else if (_nowPage.IsFolder)
            {
                if (DictPageForFolderPage.ContainsKey(_nowPage.Name))
                    _tempPage = (ImageListPage)DictPageForFolderPage[_nowPage.Name].PageContent;
            }

            if (_tempPage is not null)
            {
                _tempPage.MyActivityFeedLayout.LayoutImgArrangement = _window.FolderManager.MyImageArrangement;
                _window.FolderManager.MyImageArrangement.ImgListForRepeater = _tempPage.ImgList;
                _window.FolderManager.MyImageArrangement.ImgListChanged();
                _frame.Content = _tempPage;
            }
        }

        public static void GetParameter(Category _nowPage, Action<object> backPageCallBack = null)
        {
            PageParameters pageParameter = null;

            if (_nowPage.IsHomePage)
            {
                pageParameter = ContentForHomePage.PageParameter;
            }
            else if (_nowPage.IsGallery)
            {
                if (DictPageForGalleryPage.ContainsKey(_nowPage.Name))
                    pageParameter = DictPageForGalleryPage[_nowPage.Name].PageParameter;
            }
            else if (_nowPage.IsFolder)
            {
                if (DictPageForFolderPage.ContainsKey(_nowPage.Name))
                    pageParameter = DictPageForFolderPage[_nowPage.Name].PageParameter;
            }

            if (backPageCallBack != null)
                backPageCallBack?.Invoke(pageParameter);
        }

        public static void StoreContent(Frame frame, Category _nowPage, PageParameters pageContent)
        {

            if (_nowPage.IsHomePage)
            {
                ContentForHomePage.PageParameter = pageContent.Clone();
                ContentForHomePage.PageContent = frame.Content;
            }
            else if (_nowPage.IsGallery)
            {
                if (DictPageForGalleryPage.ContainsKey(_nowPage.Name))
                {
                    DictPageForGalleryPage[_nowPage.Name].PageContent = frame.Content;
                    DictPageForGalleryPage[_nowPage.Name].PageParameter = pageContent.Clone();
                }
                else
                {
                    var _temp = new PageStackContent(frame.Content, pageContent?.Clone());
                    DictPageForGalleryPage.Add(_nowPage.Name, _temp);
                }
            }
            else if (_nowPage.IsFolder)
            {
                if (DictPageForFolderPage.ContainsKey(_nowPage.Name))
                {
                    DictPageForFolderPage[_nowPage.Name].PageContent = frame.Content;
                    DictPageForFolderPage[_nowPage.Name].PageParameter = pageContent.Clone();
                }
                else
                {
                    var _temp = new PageStackContent(frame.Content, pageContent?.Clone());
                    DictPageForFolderPage.Add(_nowPage.Name, _temp);
                }
            }
        }

        public static void ChangeFolderDict(string _folderName, string _newName)
        {
            if (DictPageForFolderPage.ContainsKey(_folderName))
            {
                var _temp = DictPageForFolderPage[_folderName];
                DictPageForFolderPage.Remove(_folderName);
                DictPageForFolderPage.Add(_newName, _temp);
            }
        }

        public static void ChangeGalleryDict(string _galleryName, string _newName)
        {
            if (DictPageForGalleryPage.ContainsKey(_galleryName))
            {
                var _temp = DictPageForGalleryPage[_galleryName];
                DictPageForGalleryPage.Remove(_galleryName);
                DictPageForGalleryPage.Add(_newName, _temp);
            }
        }

        public static void RemoveFolder(string _folderName)
        {
            if (DictPageForFolderPage.ContainsKey(_folderName))
            {
                var _tempPage = (ImageListPage)DictPageForFolderPage[_folderName].PageContent;
                _tempPage.Close();
                DictPageForFolderPage.Remove(_folderName);
            }
        }

        public static void RemoveGallery(string _galleryName)
        {
            if (DictPageForGalleryPage.ContainsKey(_galleryName))
            {
                var _tempPage = (ImageListPage)DictPageForGalleryPage[_galleryName].PageContent;
                _tempPage.Close();
                DictPageForGalleryPage.Remove(_galleryName);
            }
        }
    }


    class PageStackContent
    {
        public PageStackContent()
        {
        }

        public PageStackContent(object pageContent, PageParameters pageParameter)
        {
            this.PageContent = pageContent;
            this.PageParameter = pageParameter;
        }

        public object PageContent { get; set; }
        public PageParameters PageParameter { get; set; }

    }

    class PageParameters 
    {
        public PictureClass Image { get; set; }

        public double Width { get; set; }

        public double Offset { get; set; }

        public bool FirstShow { get; set; }

        public PageParameters()
        {
            Clear();
        }

        public void Clear()
        {
            Offset = 0;
            Width = 1;
            FirstShow = true;
            Image = null;
        }

        public PageParameters Clone()
        {
            return (PageParameters)MemberwiseClone();
        }


    }
}
