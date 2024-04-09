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
using System.Collections.Specialized;


namespace OneGallery
{
    internal class ActivityFeedLayout : VirtualizingLayout
    {
        #region Layout parameters

        private double _rowSpacing = 12;

        private double _colSpacing = 12;

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

        public static readonly DependencyProperty MinItemSizeProperty =
            DependencyProperty.Register(
                "MinItemSize",
                typeof(Size),
                typeof(ActivityFeedLayout),
                new PropertyMetadata(Size.Empty, OnPropertyChanged));



        //public static readonly DependencyProperty ImageList =
        //    DependencyProperty.Register(
        //        "ImageList",
        //        typeof(SortableObservableCollection<PictureClass>),
        //        typeof(ActivityFeedLayout),
        //        new PropertyMetadata(Size.Empty, OnPropertyChanged));

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
            //else if (args.Property == ImageList)
            //{
            //    layout._maxItemSize = (Size)args.NewValue;
            //}
            else
            {
                throw new InvalidOperationException("Don't know what you are talking about!");
            }

            layout.InvalidateMeasure();
        }

        public ActivityFeedLayout() { Debug.Print("Create null " + GetType().Name); }

        ~ActivityFeedLayout()
        {
            Debug.Print("~" + GetType().Name);
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
            double _newTop = RowSpacing;

            if (LayoutImgArrangement == null)
                return new Size(availableSize.Width, RowSpacing);

            if (context.ItemCount > 0 && context.ItemCount <= LayoutImgArrangement.ImgList.Count)
            {
                var state = context.LayoutState as ActivityFeedLayoutState;
                int _firstItemIndex = state.FirstRealizedIndex;

                if (!UpdateImgRect(availableSize.Width))
                    return new Size(availableSize.Width, _newTop);

                state.IndexToElementMap.Clear();
                state.LayoutRects.Clear();
                state.FirstRealizedIndex = -1;

                int i = 0;
                foreach (var _item in LayoutImgArrangement.RowFirstIndex)
                {
                    double zoomImg = (availableSize.Width - LayoutImgArrangement.RowImgCount[i] * _colSpacing) / (LayoutImgArrangement.NowWidth - (LayoutImgArrangement.RowImgCount[i] - 1) * _colSpacing);

                    if (_newTop >= context.VisibleRect.Top - 300 && _newTop <= context.VisibleRect.Bottom + 300)
                    {

                        if (state.FirstRealizedIndex == -1)
                        {
                            state.FirstRealizedIndex = _item;
                        }

                        int RowImgCount = LayoutImgArrangement.RowImgCount[i];

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
                            state.IndexToElementMap.Add(_index, (ItemContainer)container);
                            _newX += _size.Width + _colSpacing;

                        }
                    }

                    i++;
                    _newTop += zoomImg * LayoutImgArrangement.ImageRect[_item].Height + _rowSpacing;
                }
            }

            return new Size(availableSize.Width, _newTop);

        }


        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            // walk through the cache of containers and arrange

            if (context.ItemCount == 0)
            {
                return finalSize;
            }

            var state = context.LayoutState as ActivityFeedLayoutState;
            int currentIndex = state.FirstRealizedIndex;

            foreach (var arrangeRect in state.LayoutRects)
            {
                var container = (ItemContainer)context.GetOrCreateElementAt(currentIndex);
                container.Arrange(arrangeRect);
                currentIndex++;
            }

            return finalSize;
        }

        protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
        {
            //Debug.Print("1111 " + args.Action);
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
                //Debug.Print("" + args.NewStartingIndex);
                //Debug.Print("" + args.NewItems);
                //Debug.Print("" + args.OldStartingIndex);
                //Debug.Print("" + args.OldItems);

                var _state = context.LayoutState as ActivityFeedLayoutState;
                var _tempIndex = args.OldStartingIndex;
                if (_state.IndexToElementMap.ContainsKey(_tempIndex))
                {
                    context.RecycleElement(_state.IndexToElementMap[_tempIndex]);
                    _state.IndexToElementMap.Remove(_tempIndex);
                }

                this.InvalidateMeasure();
            }
            else if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                var _indexToElementMap = (context.LayoutState as ActivityFeedLayoutState).IndexToElementMap;
                Debug.Print("" + context.ItemCount);
                
                foreach (var _item in _indexToElementMap.Values)
                {
                    context.RecycleElement(_item);
                }
                _indexToElementMap.Clear();
                this.InvalidateMeasure();
            }
            else if (args.Action == NotifyCollectionChangedAction.Add)
            {
                //Debug.Print("" + args.NewStartingIndex);
                //Debug.Print("" + args.NewItems);
                //Debug.Print("" + args.OldStartingIndex);
                //Debug.Print("" + args.OldItems);
                var _indexToElementMap = (context.LayoutState as ActivityFeedLayoutState).IndexToElementMap;
                int _start = args.NewStartingIndex;

                foreach (var _item in _indexToElementMap)
                {
                    if (_item.Key >= _start)
                    {
                        context.RecycleElement(_item.Value);
                        _indexToElementMap.Remove(_item.Key);
                    }                
                }
                this.InvalidateMeasure();
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

        public Dictionary<int, ItemContainer> IndexToElementMap
        {
            get
            {
                _indexToElementMap ??= new();

                return _indexToElementMap;
            }
        }

        Dictionary<int, ItemContainer> _indexToElementMap;
    }

}
