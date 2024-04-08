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
        private static readonly Dictionary<string, PageStackContent> DictPageContent
                    = new();
        public static void GetContent(MainWindow window, Frame frame, string pageName)
        {

            if (DictPageContent.ContainsKey(pageName))
            {
                var _tempPage = (ImageListPage)DictPageContent[pageName].PageContent;
                _tempPage.MyActivityFeedLayout.LayoutImgArrangement = window.FolderManager.MyImageArrangement;
                window.FolderManager.MyImageArrangement.ImgListForRepeater = _tempPage.ImgList;
                window.FolderManager.MyImageArrangement.ImgListChanged();
                frame.Content = DictPageContent[pageName].PageContent;
            }

        }

        public static void GetParameter(string pageName, Action<object> backPageCallBack = null)
        {

            PageParameters pageParameter = null;
            if (DictPageContent.ContainsKey(pageName))
            {
                pageParameter = DictPageContent[pageName].PageParameter;
            }

            if (backPageCallBack != null)
                backPageCallBack?.Invoke(pageParameter);
        }

        public static void StoreContent(Frame frame, string pageName, PageParameters pageContent)
        {

            if (DictPageContent.ContainsKey(pageName))
            {
                DictPageContent[pageName].PageContent = frame.Content;
                DictPageContent[pageName].PageParameter = pageContent.Clone();
                return;
            }
            var _temp = new PageStackContent(frame.Content, pageContent?.Clone());
            DictPageContent.Add(pageName, _temp);

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
