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
        private static readonly Dictionary<string, PageStackContent> DictPageForGalleryPage
                    = new();

        private static readonly Dictionary<string, PageStackContent> DictPageContentForFolderPage
            = new();
        public static void GetContent(MainWindow window, Frame frame, string pageName)
        {

            if (DictPageForGalleryPage.ContainsKey(pageName))
            {
                var _tempPage = (ImageListPage)DictPageForGalleryPage[pageName].PageContent;
                _tempPage.MyActivityFeedLayout.LayoutImgArrangement = window.FolderManager.MyImageArrangement;
                window.FolderManager.MyImageArrangement.ImgListForRepeater = _tempPage.ImgList;
                window.FolderManager.MyImageArrangement.ImgListChanged();
                frame.Content = DictPageForGalleryPage[pageName].PageContent;
            }

        }

        public static void GetParameter(string pageName, Action<object> backPageCallBack = null)
        {

            PageParameters pageParameter = null;
            if (DictPageForGalleryPage.ContainsKey(pageName))
            {
                pageParameter = DictPageForGalleryPage[pageName].PageParameter;
            }

            if (backPageCallBack != null)
                backPageCallBack?.Invoke(pageParameter);
        }

        public static void StoreContent(Frame frame, string pageName, PageParameters pageContent)
        {

            if (DictPageForGalleryPage.ContainsKey(pageName))
            {
                DictPageForGalleryPage[pageName].PageContent = frame.Content;
                DictPageForGalleryPage[pageName].PageParameter = pageContent.Clone();
                return;
            }
            var _temp = new PageStackContent(frame.Content, pageContent?.Clone());
            DictPageForGalleryPage.Add(pageName, _temp);

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
        //public int SortedIndex { get; set; }

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
            //SortedIndex = -1;
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
