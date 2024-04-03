using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace OneGallery
{
    internal class MyNonVirtualizingLayout : NonVirtualizingLayout
    {
        private double _rowSpacing = 12;

        private double _colSpacing = 12;

        public ImageArrangement LayoutImgArrangement { get; set; }

        public List<Rect> LayoutRects = new();


        protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            double _newTop = 0;
            int i = 0;

            if (LayoutImgArrangement == null)
                return new Size(availableSize.Width, 0);

            if (!UpdateImgRect(availableSize.Width))
                return new Size(availableSize.Width, _newTop);

            LayoutRects.Clear();

            foreach (var _item in LayoutImgArrangement.RowFirstIndex)
            {
                double zoomImg = (availableSize.Width - LayoutImgArrangement.RowImgCount[i] * _colSpacing) / (LayoutImgArrangement.NowWidth - (LayoutImgArrangement.RowImgCount[i] - 1) * _colSpacing);

                int RowImgCount = LayoutImgArrangement.RowImgCount[i];
                double _newX = _colSpacing;

                for (var j = 0; j < RowImgCount; j++)
                {
                    var _index = _item + j;
                    //if (_index >= context.ItemCount)
                    //    break;
                    //var container = context.GetOrCreateElementAt(_index);
                    var _rect = LayoutImgArrangement.ImageRect[_index];
                    Rect _size = new()
                    {
                        Width = _rect.Width * zoomImg,
                        Height = _rect.Height * zoomImg,
                        X = _newX,
                        Y = _newTop
                    };

                    context.Children[_index].Measure(new Size(_size.Width, _size.Height));
                    LayoutRects.Add(_size);
                    _newX += _size.Width + _colSpacing;
                }
                
                i++;
                _newTop += zoomImg * LayoutImgArrangement.ImageRect[_item].Height + _rowSpacing;
            }

            return new Size(availableSize.Width, _newTop);
        }

       
        protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            if (context.Children.Count == 0)
                return finalSize;

           

            int currentIndex = 0;

            foreach (var arrangeRect in LayoutRects)
            {
                var container = context.Children[currentIndex++];
                container.Arrange(arrangeRect);
                //currentIndex++;
            }

            return finalSize;
        }

        private bool UpdateImgRect(double _width)
        {

            if (LayoutImgArrangement is not null)
            {
                LayoutImgArrangement.SetImageRect(_width);
                return true;
            }

            return false;
        }
    }
}
