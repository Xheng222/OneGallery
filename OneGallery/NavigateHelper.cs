using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace OneGallery
{
    internal class NavigateHelper
    {
        private static readonly Dictionary<string, PageStackContent> DictPageForGalleryPage = new();

        private static readonly Dictionary<string, PageStackContent> DictPageForFolderPage = new();

        private static readonly PageStackContent ContentForHomePage = new();

        public static void GetContent(Category _nowPage)
        {
            ImageListPage _tempPage = null;
            Frame _frame = MainWindow.Window.NaPage;

            if (_nowPage.IsHomePage)
            {
                _tempPage = (ImageListPage)ContentForHomePage.PageContent;
            }
            else if (_nowPage.IsGallery)
            {
                if (DictPageForGalleryPage.ContainsKey(_nowPage._name))
                    _tempPage = (ImageListPage)DictPageForGalleryPage[_nowPage._name].PageContent;
            }
            else if (_nowPage.IsFolder)
            {
                if (DictPageForFolderPage.ContainsKey(_nowPage._name))
                    _tempPage = (ImageListPage)DictPageForFolderPage[_nowPage._name].PageContent;
            }

            if (_tempPage is not null)
            {
                MainWindow.Window.DispatcherQueue.TryEnqueue(() =>
                {
                    _tempPage.InitPage();
                    _frame.Content = _tempPage;
                });

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
                if (DictPageForGalleryPage.ContainsKey(_nowPage._name))
                    pageParameter = DictPageForGalleryPage[_nowPage._name].PageParameter;
            }
            else if (_nowPage.IsFolder)
            {
                if (DictPageForFolderPage.ContainsKey(_nowPage._name))
                    pageParameter = DictPageForFolderPage[_nowPage._name].PageParameter;
            }

            if (backPageCallBack != null)
                backPageCallBack?.Invoke(pageParameter);
        }

        public static void StoreContent(Category _nowPage, PageParameters pageContent)
        {
            Frame frame = MainWindow.Window.NaPage;

            if (_nowPage.IsHomePage)
            {
                ContentForHomePage.PageParameter = pageContent.Clone();
                ContentForHomePage.PageContent = frame.Content;
            }
            else if (_nowPage.IsGallery)
            {
                if (DictPageForGalleryPage.ContainsKey(_nowPage._name))
                {
                    DictPageForGalleryPage[_nowPage._name].PageContent = frame.Content;
                    DictPageForGalleryPage[_nowPage._name].PageParameter = pageContent.Clone();
                }
                else
                {
                    var _temp = new PageStackContent(frame.Content, pageContent?.Clone());
                    DictPageForGalleryPage.Add(_nowPage._name, _temp);
                }
            }
            else if (_nowPage.IsFolder)
            {
                if (DictPageForFolderPage.ContainsKey(_nowPage._name))
                {
                    DictPageForFolderPage[_nowPage._name].PageContent = frame.Content;
                    DictPageForFolderPage[_nowPage._name].PageParameter = pageContent.Clone();
                }
                else
                {
                    var _temp = new PageStackContent(frame.Content, pageContent?.Clone());
                    DictPageForFolderPage.Add(_nowPage._name, _temp);
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
