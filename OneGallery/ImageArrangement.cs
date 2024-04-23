using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Foundation;

namespace OneGallery
{
    public class ImageArrangement
    {
        public List<PictureClass> ImgList { get; set; }

        public ObservableCollection<PictureClass> ImgListForRepeater { get; set; }

        public List<Rect> ImageRect { get; set; }

        public List<int> RowFirstIndex { get; set; }

        public List<int> RowImgCount { get; set; }

        private List<Rect> ImageRect_Default = new();

        private List<int> RowFirstIndex_Default = new();

        private List<int> RowImgCount_Default = new();

        private double RowSpacing = 0;

        private double ColSpacing = 0;

        public double NowWidth = 0;

        public double ImageHeight = 0;

        public void ImgListChanged()
        {
            int _count = ImgListForRepeater.Count;

            for (int i = 0;  i < ImgList.Count; i++)
            {
                if (i < _count)
                {
                    if (ImgList[i] != ImgListForRepeater[i])
                        ImgListForRepeater[i] = ImgList[i];
                }
                else
                {
                    ImgListForRepeater.Add(ImgList[i]);
                }
            }

            _count = ImgListForRepeater.Count;
            int _totalCount = ImgList.Count;

            for (int i = _count - 1; i >= _totalCount; i--)
            {
                ImgListForRepeater.RemoveAt(i);
            }
        }

        public ImageArrangement() { }

        public void SetImageRect(double _width)
        {
            double _delta = _width / NowWidth;

            if (_delta > 1.1 || _delta < 0.909)
            {
                NowWidth = _width;
                UpdateImgRect();
            }

            return;
        }

        public void SetImgSize(double _imageHeight, double _width, double rowSpacing, double colSpacing)
        {
            ImageHeight = _imageHeight;
            NowWidth = _width;
            RowSpacing = rowSpacing;
            ColSpacing = colSpacing;

            ImageRect = ImageRect_Default;
            RowFirstIndex = RowFirstIndex_Default;
            RowImgCount = RowImgCount_Default;
        }

        public void SetImgSize(double _imageHeight, double _width)
        {
            ImageHeight = _imageHeight;
            NowWidth = _width;
            UpdateImgRect();
        }

        public void SortImg()
        {
            if (ImgList.Count > 0)
            {
                switch (MainWindow.NowSortMode)
                {
                    case MainWindow.SortMode.CreateDate:
                        {
                            if (MainWindow.IsAscending)
                            {
                                ImgList.Sort((x, y) => 
                                {
                                    int _temp = x.CreatDate.CompareTo(y.CreatDate);
                                    if (_temp != 0)
                                        return _temp;
                                    else
                                    {
                                        _temp = x.LastEditDate.CompareTo(y.LastEditDate);
                                        if (_temp != 0) 
                                            return _temp;
                                        else
                                            return x.Name.CompareTo(y.Name); 
                                    }               
                                });
                            }
                            else
                            {
                                ImgList.Sort((x, y) => 
                                {
                                    int _temp = -x.CreatDate.CompareTo(y.CreatDate);
                                    if (_temp != 0)
                                        return _temp;
                                    else
                                    {
                                        _temp = -x.LastEditDate.CompareTo(y.LastEditDate);
                                        if (_temp != 0)
                                            return _temp;
                                        else
                                            return -x.Name.CompareTo(y.Name);
                                    }
                                });
                            }

                            break;
                        }
                    case MainWindow.SortMode.LastEditDate:
                        {
                            if (MainWindow.IsAscending)
                            {
                                ImgList.Sort((x, y) =>
                                {
                                    int _temp = x.LastEditDate.CompareTo(y.LastEditDate);
                                    if (_temp != 0)
                                        return _temp;
                                    else
                                    {
                                        _temp = x.CreatDate.CompareTo(y.CreatDate);
                                        if (_temp != 0)
                                            return _temp;
                                        else
                                            return x.Name.CompareTo(y.Name);
                                    }
                                });
                            }
                            else
                            {
                                ImgList.Sort((x, y) =>
                                {
                                    int _temp = -x.LastEditDate.CompareTo(y.LastEditDate);
                                    if (_temp != 0)
                                        return _temp;
                                    else
                                    {
                                        _temp = -x.CreatDate.CompareTo(y.CreatDate);
                                        if (_temp != 0)
                                            return _temp;
                                        else
                                            return -x.Name.CompareTo(y.Name);
                                    }
                                });
                            }

                            break;
                        }
                    case MainWindow.SortMode.ShootDate:
                        {
                            if (MainWindow.IsAscending)
                            {
                                ImgList.Sort((x, y) =>
                                {
                                    int _temp = x.ShootDate.CompareTo(y.ShootDate);
                                    if (_temp != 0)
                                        return _temp;
                                    else
                                    {
                                        _temp = x.CreatDate.CompareTo(y.CreatDate);
                                        if (_temp != 0)
                                            return _temp;
                                        else
                                            return x.Name.CompareTo(y.Name);
                                    }
                                });
                            }
                            else
                            {
                                ImgList.Sort((x, y) =>
                                {
                                    int _temp = -x.ShootDate.CompareTo(y.ShootDate);
                                    if (_temp != 0)
                                        return _temp;
                                    else
                                    {
                                        _temp = -x.CreatDate.CompareTo(y.CreatDate);
                                        if (_temp != 0)
                                            return _temp;
                                        else
                                            return -x.Name.CompareTo(y.Name);
                                    }
                                });
                            }

                            break; 
                        }
                    case MainWindow.SortMode.Name: 
                        {
                            if (MainWindow.IsAscending)
                            {
                                ImgList.Sort((x, y) =>
                                {
                                    int _temp = x.Name.CompareTo(y.Name);
                                    if (_temp != 0)
                                        return _temp;
                                    else
                                    {
                                        _temp = x.LastEditDate.CompareTo(y.LastEditDate);
                                        if (_temp != 0)
                                            return _temp;
                                        else
                                            return x.CreatDate.CompareTo(y.CreatDate);
                                    }
                                });
                            }
                            else
                            {
                                ImgList.Sort((x, y) =>
                                {
                                    int _temp = -x.Name.CompareTo(y.Name);
                                    if (_temp != 0)
                                        return _temp;
                                    else
                                    {
                                        _temp = -x.LastEditDate.CompareTo(y.LastEditDate);
                                        if (_temp != 0)
                                            return _temp;
                                        else
                                            return -x.CreatDate.CompareTo(y.CreatDate);
                                    }
                                });
                            }

                            break; 
                        }
                }

                for (int i = 0; i < ImgList.Count; i++)
                {
                    ImgList[i].Index = i;
                }
            }
        }

        private List<double> TryPutImg(int Index, double RemainingWidth)
        {
            if (Index > ImgList.Count - 1)
                return new();

            double _acutualWidth = ImgList[Index].Width * (double)(ImageHeight / ImgList[Index].Height);

            if (_acutualWidth < ImageHeight / 2)
            {
                _acutualWidth = ImageHeight / 2;
            }
            else if (_acutualWidth > ImageHeight * 2.5)
            {
                _acutualWidth = ImageHeight * 2.5;
            }

            if (_acutualWidth > NowWidth)
                _acutualWidth = NowWidth;
            

            List<double> list;

            double _remainingWidth = RemainingWidth - _acutualWidth;
            if (_remainingWidth > ColSpacing)
            {
                list = TryPutImg(Index + 1, _remainingWidth - ColSpacing);
            }
            else
            {
                if (Math.Abs(_remainingWidth) > RemainingWidth)
                {
                    return new();
                }
                else
                {
                    list = new();
                }
            }

            list.Insert(0, _acutualWidth);
            return list;
        }

        public void UpdateImgRect()
        {
            int _imgIndex;
            double _nowY;

            if (ImgList.Count == 0)
                return;

            _imgIndex = 0;
            _nowY = 0;

            ImageRect_Default.Clear();
            RowFirstIndex_Default.Clear();
            RowImgCount_Default.Clear();

            while (_imgIndex < ImgList.Count)
            {
                List<double> Wigthlist = TryPutImg(_imgIndex, NowWidth - ColSpacing);

                double _actualWidth = 0; 
                
                foreach (var i in Wigthlist)
                {
                    _actualWidth += i;
                }
                    
                double zoom = (NowWidth - ColSpacing * Wigthlist.Count) / _actualWidth;

                if (_imgIndex + Wigthlist.Count == ImgList.Count)
                {
                    if (zoom > 1.2)
                    {
                        zoom = 1.2;
                    }
                }

                double _nowX = ColSpacing;

                for (int i = 0; i < Wigthlist.Count; i++)
                {
                    Rect _rect = new()
                    {
                        Width = Wigthlist[i] * zoom,
                        Height = ImageHeight * zoom,
                        Y = _nowY,
                        X = _nowX
                    };

                    _nowX += Wigthlist[i] * zoom + ColSpacing;
                    ImageRect_Default.Add(_rect);
                }

                RowFirstIndex_Default.Add(_imgIndex);
                _nowY += ImageHeight * zoom + RowSpacing;
                _imgIndex += Wigthlist.Count;

            }

            int _count;

            for (int i = 1; i < RowFirstIndex_Default.Count; i++)
            {
                _count = RowFirstIndex_Default[i] - RowFirstIndex_Default[i-1];
                RowImgCount_Default.Add(_count);
            }

            RowImgCount_Default.Add(ImgList.Count - RowFirstIndex_Default.Last());
        }

    }
}
