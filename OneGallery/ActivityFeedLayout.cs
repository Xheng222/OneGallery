﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;


namespace OneGallery
{
    public class ActivityFeedLayout : VirtualizingLayout
    {
        #region Layout parameters

        private double _rowSpacing = 12;

        private double _colSpacing = 12;

        private Size _minItemSize = Size.Empty;

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

        public static readonly DependencyProperty MinItemSizeProperty =
            DependencyProperty.Register(
                "MinItemSize",
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
            else
            {
                throw new InvalidOperationException("Don't know what you are talking about!");
            }

            layout.InvalidateMeasure();
        }

        public ActivityFeedLayout() { Debug.Print("Create null " + GetType().Name); }


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
            double _newTop = RowSpacing;

            if (LayoutImgArrangement == null)
                return new Size(context.VisibleRect.Width, context.VisibleRect.Height);

            if (context.ItemCount > 0)
            {
                var state = context.LayoutState as ActivityFeedLayoutState;


                lock (LayoutImgArrangement.ImgList)
                {
                    if (!UpdateImgRect(availableSize.Width))
                        return new Size(availableSize.Width, _newTop);

                    state.IndexToElementMap.Clear();
                    state.LayoutRects.Clear();
                    state.FirstRealizedIndex = -1;

                    int i = 0;
                    foreach (var _item in LayoutImgArrangement.RowFirstIndex)
                    {
                        double zoomImg = (availableSize.Width - LayoutImgArrangement.RowImgCount[i] * _colSpacing) / (LayoutImgArrangement.NowWidth - (LayoutImgArrangement.RowImgCount[i] - 1) * _colSpacing);

                        if (_newTop >= context.VisibleRect.Top - LayoutImgArrangement.ImageHeight * 2 && _newTop <= context.VisibleRect.Bottom + LayoutImgArrangement.ImageHeight * 2)
                        {
                            int RowImgCount = LayoutImgArrangement.RowImgCount[i];

                            if (state.FirstRealizedIndex == -1)
                            {
                                state.FirstRealizedIndex = _item;
                            }

                            double _newX = _colSpacing;

                            for (var j = 0; j < RowImgCount; j++)
                            {
                                var _index = _item + j;
                                if (_index >= context.ItemCount)
                                    break;

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
                                state.IndexToElementMap.Add(_index, container);
                                _newX += _size.Width + _colSpacing;
                            }
                        }

                        _newTop += zoomImg * LayoutImgArrangement.ImageRect[_item].Height + _rowSpacing;
                        i++;
                    }
                }
            }

            return new Size(availableSize.Width, _newTop);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            if (context.ItemCount == 0)
            {
                return finalSize;
            }

            var state = context.LayoutState as ActivityFeedLayoutState;
            int currentIndex = state.FirstRealizedIndex;

            foreach (var arrangeRect in state.LayoutRects)
            {
                var container = context.GetOrCreateElementAt(currentIndex);
                container.Arrange(arrangeRect);
                currentIndex++;
            }

            return finalSize;
        }

        protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Move)
            {
                var _indexToElementMap = (context.LayoutState as ActivityFeedLayoutState).IndexToElementMap;
                var _tempIndex = args.OldStartingIndex;

                if (_indexToElementMap.ContainsKey(_tempIndex))
                {
                    context.RecycleElement(_indexToElementMap[_tempIndex]);
                    _indexToElementMap.Remove(_tempIndex);
                }

                _tempIndex = args.NewStartingIndex;
                if (_indexToElementMap.ContainsKey(_tempIndex))
                {
                    context.RecycleElement(_indexToElementMap[_tempIndex]);
                    _indexToElementMap.Remove(_tempIndex);
                }

                this.InvalidateMeasure();
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                var _indexToElementMap = (context.LayoutState as ActivityFeedLayoutState).IndexToElementMap;
                int _old = args.OldStartingIndex;

                if (_indexToElementMap.ContainsKey(_old))
                {
                    context.RecycleElement(_indexToElementMap[_old]);
                    _indexToElementMap.Remove(_old);
                }

                if (LayoutImgArrangement.ImgList.Count == LayoutImgArrangement.ImgListForRepeater.Count)
                    this.InvalidateMeasure();
            }
            else if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                var _indexToElementMap = (context.LayoutState as ActivityFeedLayoutState).IndexToElementMap;

                foreach (var item in _indexToElementMap.Values)
                {
                    context.RecycleElement(item);
                }

                _indexToElementMap.Clear();

                this.InvalidateMeasure();
            }
            else if (args.Action == NotifyCollectionChangedAction.Add)
            {
                if (LayoutImgArrangement.ImgList.Count == LayoutImgArrangement.ImgListForRepeater.Count)
                {
                    var _indexToElementMap = (context.LayoutState as ActivityFeedLayoutState).IndexToElementMap;

                    foreach (var _item in _indexToElementMap)
                    {
                        context.RecycleElement(_item.Value);
                        _indexToElementMap.Remove(_item.Key);
                    }
                    this.InvalidateMeasure();
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Replace)
            {
                var _indexToElementMap = (context.LayoutState as ActivityFeedLayoutState).IndexToElementMap;
                int _old = args.OldStartingIndex;

                if (_indexToElementMap.ContainsKey(_old))
                {
                    context.RecycleElement(_indexToElementMap[_old]);
                    _indexToElementMap.Remove(_old);
                }

                base.OnItemsChangedCore(context, source, args);
            }

            else
                base.OnItemsChangedCore(context, source, args);


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

        public void MyInvalidateMeasure()
        {
            this.InvalidateMeasure();
        }
        #endregion

    }


    internal class ActivityFeedLayoutState
    {
        public int FirstRealizedIndex { get; set; }

        public List<Rect> LayoutRects
        {
            get
            {
                _layoutRects ??= new List<Rect>();

                return _layoutRects;
            }
        }

        private List<Rect> _layoutRects;

        public Dictionary<int, UIElement> IndexToElementMap
        {
            get
            {
                _indexToElementMap ??= new();

                return _indexToElementMap;
            }
        }

        Dictionary<int, UIElement> _indexToElementMap;
    }

}
