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

        public event EventHandler MyEvent;

        private void OnMyEvent()
        {
            MyEvent.Invoke(null, null);
        }

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
            double _newTop = 0;
            //Debug.Print("333");
            if (LayoutImgArrangement == null)
                return new Size(0, 0);

            if (context.ItemCount > 0 && context.ItemCount <= LayoutImgArrangement.ImgList.Count)
            {
                var state = context.LayoutState as ActivityFeedLayoutState;
                state.LayoutRects.Clear();
                state.FirstRealizedIndex = -1;
                int _firstItemIndex;
                int i = 0;
                if (!UpdateImgRect(availableSize.Width))
                    return new Size(availableSize.Width, _newTop);

                foreach (var _item in LayoutImgArrangement.RowFirstIndex)
                {
                    double zoomImg = (availableSize.Width - LayoutImgArrangement.RowImgCount[i] * _colSpacing) / (LayoutImgArrangement.NowWidth - (LayoutImgArrangement.RowImgCount[i] - 1) * _colSpacing);

                    if (_newTop >= context.VisibleRect.Top - 300 && _newTop <= context.VisibleRect.Bottom + 300)
                    {

                        if (state.FirstRealizedIndex == -1)
                        {
                            state.FirstRealizedIndex = _item;

                            _firstItemIndex = _item;

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
            Debug.Print("1111 " + args.Action);
            if (args.Action == NotifyCollectionChangedAction.Move)
            {
                var _temp = args.OldItems;

                Debug.Print("" + args.OldStartingIndex);
                Debug.Print("" + args.NewStartingIndex);

                var container = (ItemContainer)context.GetOrCreateElementAt(args.OldStartingIndex);
                context.RecycleElement(container);

                container = (ItemContainer)context.GetOrCreateElementAt(args.NewStartingIndex);
                context.RecycleElement(container);

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
