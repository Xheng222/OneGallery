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

namespace OneGallery
{
    internal class NavigateHelper
    {
        private static readonly Dictionary<string, PageStackContent> DictPageContent
                    = new();

        public static void OnNavigatedTo(Frame frame, string pageName, Action<object> backPageCallBack = null)
        {

            object pageParameter;
            if (DictPageContent.ContainsKey(pageName))
            {
                var temp = DictPageContent[pageName];
                frame.Content = temp.PageContent;
                pageParameter = temp.PageParameter;

                backPageCallBack?.Invoke(pageParameter);
            }

            
        }

        public static void OnNavigatingFrom(string pageName, object pageContent, ICloneable pageParameter)
        {

            if (DictPageContent.ContainsKey(pageName))
            {
                DictPageContent.Remove(pageName);

            }
            DictPageContent.Add(pageName, new(pageContent, pageParameter?.Clone()));

        }


    }


    class PageStackContent
    {
        public PageStackContent()
        {
        }

        public PageStackContent(object pageContent, object pageParameter)
        {
            this.PageContent = pageContent;
            this.PageParameter = pageParameter;
        }

        public object PageContent { get; set; }
        public object PageParameter { get; set; }

    }

    class PageParameters :ICloneable
    {
        public int SortedIndex { get; set; }

        public double Width { get; set; }

        public double Offset { get; set; }

        public PageParameters(int sortedIndex)
        {
            SortedIndex = sortedIndex;
            Offset = 0;
            Width = 1;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }


    }
}
