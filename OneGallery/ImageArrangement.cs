using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace OneGallery
{
    internal class ImageArrangement
    {
        public ObservableCollection<CustomDataObject> ImgList { get; set; }

        public List<Rect> ImageRect { get; set; }

        public List<int> RowFirstIndex { get; set; }

        public List<int> RowImgCount { get; set; }

        public double NowWidth = 0;

        private List<Rect>[] ImageRect_Default = new List<Rect>[] {new List<Rect>(), new List<Rect>(), new List<Rect>() };

        private List<int>[] RowFirstIndex_Default = new List<int>[] { new(), new List<int>(), new List<int>() };

        private List<int>[] RowImgCount_Default = new List<int>[] { new List<int>(), new List<int>(), new List<int>() };

        private Size[] _minItemSize = { Size.Empty, Size.Empty, Size.Empty };

        private Size _maxItemSize = Size.Empty;

        private double RowSpacing = 0;

        private double ColSpacing = 0;

        private double[] TotalWidth;

        public ImageArrangement(ObservableCollection<CustomDataObject> objects)
        {
            ImgList = objects;
        }

        public bool CompareImgSize(Size min, Size max, double width, double rowSpacing, double colSpacing)
        {
            //if (_maxItemSize == max && _minItemSize == min && TotalWidth == width && RowSpacing == rowSpacing && ColSpacing == colSpacing)
            //{
            //    return true;
            //}

            //SetImgSize(min, max, width, rowSpacing, colSpacing);
            return false;
        }

        public void SetImageRect(double _width)
        {
            var d1 = Math.Abs(_width - TotalWidth[0]);
            var d2 = Math.Abs(_width - TotalWidth[1]);
            var d3 = Math.Abs(_width - TotalWidth[2]);

            ImageRect = ImageRect_Default[1];
            RowFirstIndex = RowFirstIndex_Default[1];
            RowImgCount = RowImgCount_Default[1];
            NowWidth = TotalWidth[1];

            //if (d1 > d2)
            //{
            //    if (d3 > d2)
            //    {
            //        ImageRect = ImageRect_Default[1];
            //        RowFirstIndex = RowFirstIndex_Default[1];
            //        RowImgCount = RowImgCount_Default[1];
            //        NowWidth = TotalWidth[1];
            //    }
            //    else
            //    {
            //        ImageRect = ImageRect_Default[2];
            //        RowFirstIndex = RowFirstIndex_Default[2];
            //        RowImgCount = RowImgCount_Default[2];
            //        NowWidth = TotalWidth[2];
            //    }
            //}
            //else
            //{
            //    if (d3 > d1)
            //    {
            //        ImageRect = ImageRect_Default[0];
            //        RowFirstIndex = RowFirstIndex_Default[0];
            //        RowImgCount = RowImgCount_Default[0];
            //        NowWidth = TotalWidth[0];
            //    }
            //    else
            //    {
            //        ImageRect = ImageRect_Default[2];
            //        RowFirstIndex = RowFirstIndex_Default[2];
            //        RowImgCount = RowImgCount_Default[2];
            //        NowWidth = TotalWidth[2];
            //    }

            //}

        }

        public int[] FindFirstAndLastImageIndex(double top, double bottom)
        {
            var Index = new int[2];
            int i = 0;
            double height = 0;
            int _index = 0;
            if (top <= 0)
            {
                Index[0] = 0;
            }
            else
            {
                for (i = 0; i < RowFirstIndex.Count; i++)
                {
                    
                    height += ImageRect[_index].Height + RowSpacing;
                    if (top <= height)
                    {
                        Index[0] = _index;
                        i++;
                        break;
                    }
                    _index = RowFirstIndex[i];
                }
            }

            Index[1] = RowFirstIndex[RowFirstIndex.Count - 1];

            for (; i < RowFirstIndex.Count; i++)
            {
                _index = RowFirstIndex[i];
                height += ImageRect[_index].Height + RowSpacing;
                if (bottom <= height)
                {
                    Index[1] = _index;
                    break;
                }
            }

            while (Index[1] < ImageRect.Count-1 && ImageRect[Index[1]].Y == ImageRect[Index[1] + 1].Y)
            {
                Index[1]++;
            }

            return Index;
        }


        public void SetImgSize(Size[] min, Size max, double[] width, double rowSpacing, double colSpacing)
        {
            _minItemSize = min;
            _maxItemSize = max;
            TotalWidth = width;
            RowSpacing = rowSpacing;
            ColSpacing = colSpacing;

            UpdateImgRect();
        }

        private List<double> TryPutImg(int Index, double RemainingWidth, Size ItemSize)
        {
            List<double> list;

            double _acutualWidth = ImgList[Index].Width * (double)(ItemSize.Height / ImgList[Index].Height);

            if (_acutualWidth < ItemSize.Height / 2)
            {
                _acutualWidth = ItemSize.Height / 2;
            }
            else if (_acutualWidth > ItemSize.Height * 3)
            {
                _acutualWidth = ItemSize.Height * 3;
            }

            if (_acutualWidth <= RemainingWidth)
            {
                double _remainingWidth = RemainingWidth - _acutualWidth - ColSpacing;
                if (_remainingWidth > 0 && Index < ImgList.Count - 1)
                {
                    list = TryPutImg(Index+1, _remainingWidth, ItemSize);
                }
                else
                {
                    list = new();
                    
                }
                list.Insert(0, _acutualWidth);
                return list;
            }
            else
            {
                list = new();
                return list;
            }


        }

        private void UpdateImgRect()
        {
            int _imgIndex;
            double _nowY;

            for (int j = 0; j < 3; j++)
            {
                _imgIndex = 0;
                _nowY = 0;

                ImageRect_Default[j].Clear();
                RowFirstIndex_Default[j].Clear();
                RowImgCount_Default[j].Clear();

                while (_imgIndex < ImgList.Count)
                {
                    //Debug.Print("Update " + TotalWidth);

                    List<double> Wigthlist = TryPutImg(_imgIndex, TotalWidth[j], _minItemSize[j]);

                    double _actualWidth = 0; 
                
                    foreach (var i in Wigthlist)
                    {
                        _actualWidth += i;
                    }
                    
                    double zoom = (TotalWidth[j] - ColSpacing * (Wigthlist.Count - 1)) / _actualWidth;

                    if (_imgIndex + Wigthlist.Count == ImgList.Count)
                    {
                        if (zoom > 1.2)
                        {
                            zoom = 1.2;
                        }
                    }

                    double _nowX = 0;

                    for (int i = 0; i < Wigthlist.Count; i++)
                    {
                        Rect _rect = new()
                        {
                            Width = Wigthlist[i] * zoom,
                            Height = _minItemSize[j].Height * zoom,
                            Y = _nowY,
                            X = _nowX
                        };

                        _nowX += Wigthlist[i] * zoom + ColSpacing;
                        ImageRect_Default[j].Add(_rect);

                    }

                    RowFirstIndex_Default[j].Add(_imgIndex);
                    _nowY += _minItemSize[j].Height * zoom + RowSpacing;
                    _imgIndex += Wigthlist.Count;

                }

                int _count = 0;
                for (int i = 1; i < RowFirstIndex_Default[j].Count; i++)
                {
                    _count = RowFirstIndex_Default[j][i] - RowFirstIndex_Default[j][i-1];
                    RowImgCount_Default[j].Add(_count);
                }
                RowImgCount_Default[j].Add(ImgList.Count - RowFirstIndex_Default[j].Last());

            }

            


        }

    }
}
