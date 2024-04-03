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
        //private static readonly Dictionary<string, PageParameters> DictPageContent
        //            = new();        
        
        private static readonly Dictionary<string, PageStackContent> DictPageContent
                    = new();

        //public static void OnNavigatedTo(string pageName, Action<object> backPageCallBack = null)
        //{

        //    object pageParameter = null;
        //    if (DictPageContent.ContainsKey(pageName))
        //    {
        //        pageParameter = DictPageContent[pageName];
        //        //frame.Content = temp.PageContent;
        //        //pageParameter = temp; 
        //    }

        //    backPageCallBack?.Invoke(pageParameter);
        //}

        //public static void OnNavigatingFrom(string pageName, PageParameters pageContent)
        //{

        //    if (DictPageContent.ContainsKey(pageName))
        //    {
        //        //DictPageContent.Remove(pageName);
        //        DictPageContent[pageName] = pageContent.Clone();
        //        return;
        //    }
        //    DictPageContent.Add(pageName, pageContent.Clone());

        //}

        public static void OnNavigatedTo(MainWindow window, Frame frame, string pageName, Action<object> backPageCallBack = null)
        {

            PageParameters pageParameter = null;
            if (DictPageContent.ContainsKey(pageName))
            {
                var _temp = DictPageContent[pageName];
                var _tempPage = (ImageListPage)_temp.PageContent;
                _tempPage.MyActivityFeedLayout.LayoutImgArrangement = window.FolderManager.MyImageArrangement;
                window.FolderManager.MyImageArrangement.ImgListForRepeater = _tempPage.ImgList;
                window.FolderManager.MyImageArrangement.ImgListChangedEvent();
                frame.Content = _temp.PageContent;
                pageParameter = _temp.PageParameter;
            }

            if (backPageCallBack != null) 
                backPageCallBack?.Invoke(pageParameter);
        }

        public static void OnNavigatingFrom(Frame frame, string pageName, PageParameters pageContent)
        {

            if (DictPageContent.ContainsKey(pageName))
            {
                //DictPageContent.Remove(pageName);

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
        public int SortedIndex { get; set; }

        public double Width { get; set; }

        public double Offset { get; set; }

        public bool FirstShow { get; set; }

        public ActivityFeedLayout ActivityFeedLayout { get; set; }

        public PageParameters()
        {
            Clear();
        }

        public void Clear()
        {
            SortedIndex = -1;
            Offset = 0;
            Width = 1;
            FirstShow = true;
            ActivityFeedLayout = null;
        }

        public PageParameters Clone()
        {
            return (PageParameters)MemberwiseClone();
        }


    }
}
