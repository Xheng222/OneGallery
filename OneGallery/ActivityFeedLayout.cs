using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace OneGallery
{
    class ActivityFeedLayout : VirtualizingLayout
    {
        #region Layout parameters

        public event EventHandler MyEvent;

        private void OnMyEvent()
        {
            MyEvent.Invoke(null, null);
        }

        private double _rowSpacing;

        private double _colSpacing;

        private Size _minItemSize = Size.Empty;

        private Size _maxItemSize = Size.Empty;

        public ImageArrangement LayoutImgArrangement { get; set; }

        public double RowSpacing
        {
            get { return _rowSpacing; }
            set { SetValue(RowSpacingProperty, value); }
        }

        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.Register(
                "RowSpacing",
                typeof(double),
                typeof(ActivityFeedLayout),
                new PropertyMetadata(0, OnPropertyChanged));

        public double ColumnSpacing
        {
            get { return _colSpacing; }
            set { SetValue(ColumnSpacingProperty, value); }
        }

        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.Register(
                "ColumnSpacing",
                typeof(double),
                typeof(ActivityFeedLayout),
                new PropertyMetadata(0, OnPropertyChanged));

        public Size MinItemSize
        {
            get { return _minItemSize; }
            set { SetValue(MinItemSizeProperty, value); }
        }

        public Size MaxItemSize
        {
            get { return _maxItemSize; }
            set { SetValue(MaxItemSizeProperty, value); }
        }

        public static readonly DependencyProperty MinItemSizeProperty =
            DependencyProperty.Register(
                "MinItemSize",
                typeof(Size),
                typeof(ActivityFeedLayout),
                new PropertyMetadata(Size.Empty, OnPropertyChanged));

        public static readonly DependencyProperty MaxItemSizeProperty =
            DependencyProperty.Register(
                "MaxItemSize",
                typeof(Size),
                typeof(ActivityFeedLayout),
                new PropertyMetadata(Size.Empty, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var layout = obj as ActivityFeedLayout;

            if (args.Property == RowSpacingProperty)
            {
                layout._rowSpacing = (double)args.NewValue;
            }
            else if (args.Property == ColumnSpacingProperty)
            {
                layout._colSpacing = (double)args.NewValue;
            }
            else if (args.Property == MinItemSizeProperty)
            {
                layout._minItemSize = (Size)args.NewValue;
            }
            else if (args.Property == MaxItemSizeProperty)
            {
                layout._maxItemSize = (Size)args.NewValue;
            }
            else
            {
                throw new InvalidOperationException("Don't know what you are talking about!");
            }

            layout.InvalidateMeasure();
        }

        #endregion

        #region Setup / teardown

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.InitializeForContextCore(context);
            if (context.LayoutState is not ActivityFeedLayoutState)
            {
                context.LayoutState = new ActivityFeedLayoutState();
            }
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.UninitializeForContextCore(context);

            context.LayoutState = null;
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            double _newTop = 0;

            if (context.ItemCount > 0)
            {     
                var state = context.LayoutState as ActivityFeedLayoutState;                
                state.LayoutRects.Clear();
                state.FirstRealizedIndex = -1;                
                var recommond = context.RecommendedAnchorIndex;  
                int _firstItemIndex;
                int i = 0; 
                
                UpdateImgRect(availableSize.Width);

                if (recommond != -1)
                {
                    var index = Math.Max(LayoutImgArrangement.RowFirstIndex.IndexOf(recommond), 0);

                    foreach (var _item in LayoutImgArrangement.RowFirstIndex)
                    {
                        double zoomImg = (availableSize.Width - LayoutImgArrangement.RowImgCount[i] * _colSpacing) / (LayoutImgArrangement.NowWidth - (LayoutImgArrangement.RowImgCount[i] - 1) * _colSpacing);

                        if (index > i - 5 && index < i + 2)
                        {
                            if (state.FirstRealizedIndex == -1)
                            {
                                state.FirstRealizedIndex = _item;
                                _firstItemIndex = _item;
                            }

                            int RowImgCount = LayoutImgArrangement.RowImgCount[i];
                            double _newX = 0;

                            for (var j = 0; j < RowImgCount; j++)
                            {
                                var _index = _item + j;
                                var container = context.GetOrCreateElementAt(_index);
                                var _rect = LayoutImgArrangement.ImageRect[_index];
                                Rect _size = new()
                                {
                                    Width = _rect.Width * zoomImg,
                                    Height = _rect.Height * zoomImg,
                                    X = _newX,
                                    Y = _newTop
                                };

                                container.Measure(new Size(_size.Width, _size.Height));
                                state.LayoutRects.Add(_size);
                                _newX += _size.Width + _colSpacing;

                            }
                        }

                        i++;
                        _newTop += zoomImg * LayoutImgArrangement.ImageRect[_item].Height + _rowSpacing;

                    }

                }
                else
                {
                    foreach (var _item in LayoutImgArrangement.RowFirstIndex)
                    {
                        double zoomImg = (availableSize.Width - LayoutImgArrangement.RowImgCount[i] * _colSpacing) / (LayoutImgArrangement.NowWidth - (LayoutImgArrangement.RowImgCount[i] - 1) * _colSpacing);

                        if (_newTop >= context.RealizationRect.Top && _newTop <= context.RealizationRect.Bottom)
                        {

                            if (state.FirstRealizedIndex == -1)
                            {
                                state.FirstRealizedIndex = _item;

                                _firstItemIndex = _item;

                            }

                            int RowImgCount = LayoutImgArrangement.RowImgCount[i];
                            double _newX = 0;

                            for (var j = 0; j < RowImgCount; j++)
                            {
                                var _index = _item + j;
                                var container = context.GetOrCreateElementAt(_index);
                                var _rect = LayoutImgArrangement.ImageRect[_index];
                                Rect _size = new()
                                {
                                    Width = _rect.Width * zoomImg,
                                    Height = _rect.Height * zoomImg,
                                    X = _newX,
                                    Y = _newTop
                                };

                                container.Measure(new Size(_size.Width, _size.Height));
                                state.LayoutRects.Add(_size);
                                _newX += _size.Width + _colSpacing;

                            }

                        }

                        i++;
                        _newTop += zoomImg * LayoutImgArrangement.ImageRect[_item].Height + _rowSpacing;
                    }
                }

                //Debug.Print(context.RecommendedAnchorIndex.ToString());
                //if (state.LayoutRects.Count > 0 && state.LayoutRects[0].Top >= context.RealizationRect.Top && state.LayoutRects[0].Bottom <= context.RealizationRect.Bottom)
                //{
                //    var _firstRealizedIndex = state.FirstRealizedIndex;
                //    var _lastRealizedIndex = state.FirstRealizedIndex + state.LayoutRects.Count - 1;
                //    var _oldTop = state.LayoutRects[0].Top;   
                //    state.LayoutRects.Clear();
                //    Debug.Print("aaa");
                //    foreach (var _item in _imageArrangement.RowFirstIndex)
                //    {
                //        double zoomImg = (availableSize.Width - _imageArrangement.RowImgCount[i] * (_colSpacing - 1) - 16) / (_imageArrangement.NowWidth - _imageArrangement.RowImgCount[i] * (_colSpacing - 1));
                //        if (_item >= _firstRealizedIndex && _item <= _lastRealizedIndex + 5)
                //        {

                //            if (_item == _firstRealizedIndex)
                //            {
                //                _extentHeight = _oldTop - _newTop;
                //                _newTop = _oldTop;
                //                context.LayoutOrigin = new Point(0,  _extentHeight);
                //            }

                //            int RowImgCount = _imageArrangement.RowImgCount[i];
                //            double _newX = 0;

                //            for (var j = 0; j < RowImgCount; j++)
                //            {
                //                var _index = _item + j;
                //                var container = context.GetOrCreateElementAt(_index);
                //                var _rect = _imageArrangement.ImageRect[_index];
                //                Rect _size = new()
                //                {
                //                    Width = _rect.Width * zoomImg,
                //                    Height = _rect.Height * zoomImg,
                //                    X = _newX,
                //                    Y = _newTop
                //                };

                //                container.Measure(new Size(_size.Width, _size.Height));
                //                state.LayoutRects.Add(_size);
                //                _newX += _size.Width + _colSpacing;
                //            }

                //        }

                //        _newTop += zoomImg * _imageArrangement.ImageRect[_item].Height + _rowSpacing;
                //        i++;
                //    }
                //}

                //else
                //{
                //    state.LayoutRects.Clear();
                //    state.FirstRealizedIndex = -1;
                //    Debug.Print("bbb");
                //    foreach (var _item in _imageArrangement.RowFirstIndex)
                //    {        
                //        double zoomImg = (availableSize.Width - _imageArrangement.RowImgCount[i] * (_colSpacing - 1) - 16) / (_imageArrangement.NowWidth  - _imageArrangement.RowImgCount[i] * (_colSpacing - 1));

                //        if (_newTop >= context.RealizationRect.Top && _newTop <= context.RealizationRect.Bottom)
                //        {

                //            if (state.FirstRealizedIndex == -1)
                //            {
                //                state.FirstRealizedIndex = _item;

                //                _firstItemIndex = _item;

                //                //Debug.Print("RecommendedAnchorIndex " + context.RecommendedAnchorIndex +
                //                //    "\navailableSize.Width " + availableSize.Width +
                //                //    "\nRealizationRect.Top " + context.RealizationRect.Width +
                //                //    "\nFirstRealizedIndex " + _item +
                //                //    "\n_extentHeight" + _extentHeight);

                //                //Debug.Print(context.LayoutOrigin.ToString());
                //            }

                //            int RowImgCount = _imageArrangement.RowImgCount[i];
                //            double _newX = 0;

                //            for ( var j = 0; j < RowImgCount; j++)
                //            {
                //                var _index = _item + j;
                //                var container = context.GetOrCreateElementAt(_index);
                //                var _rect = _imageArrangement.ImageRect[_index];
                //                Rect _size = new()
                //                {
                //                    Width = _rect.Width * zoomImg,
                //                    Height = _rect.Height * zoomImg,
                //                    X = _newX,
                //                    Y = _newTop
                //                };

                //                container.Measure(new Size(_size.Width, _size.Height));
                //                state.LayoutRects.Add(_size);
                //                _newX += _size.Width + _colSpacing;

                //            }

                //            //Debug.Print(_newX + "");
                //        }

                //        i++;    
                //        _newTop += zoomImg * _imageArrangement.ImageRect[_item].Height + _rowSpacing;
                //    }    
                //}   



            }


            return new Size(availableSize.Width, _newTop);
         
            
        }


        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            // walk through the cache of containers and arrange
            var state = context.LayoutState as ActivityFeedLayoutState;
            var virtualContext = context;
            int currentIndex = state.FirstRealizedIndex;

            foreach (var arrangeRect in state.LayoutRects)
            {
                var container = virtualContext.GetOrCreateElementAt(currentIndex);
                container.Arrange(arrangeRect);
                currentIndex++;
            }

            return finalSize;
        }

        private void UpdateImgRect(double _width)
        {
            var _oldWidth = LayoutImgArrangement.NowWidth;
            LayoutImgArrangement.SetImageRect(_width);
            if (_oldWidth != LayoutImgArrangement.NowWidth)
            {
                OnMyEvent();
            }

            return;
        }




        #endregion

    }



    internal class ActivityFeedLayoutState
    {
        public int FirstRealizedIndex { get; set; }

        /// <summary>
        /// List of layout bounds for items starting with the
        /// FirstRealizedIndex.
        /// </summary>
        public List<Rect> LayoutRects
        {
            get
            {
                _layoutRects ??= new List<Rect>();

                return _layoutRects;
            }
        }

        private List<Rect> _layoutRects;
    }

}
