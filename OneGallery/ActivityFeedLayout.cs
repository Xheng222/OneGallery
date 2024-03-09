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

        // We'll cache copies of the dependency properties to avoid calling GetValue during layout since that
        // can be quite expensive due to the number of times we'd end up calling these.
        private double _pageWidth = 0;
        
        private double _extentHeight = 0;

        private double _lastWidth = 0;

        private int _firstItemIndex = 0;

        private double _rowSpacing;

        private double _colSpacing;

        private Size _minItemSize = Size.Empty;

        private Size _maxItemSize = Size.Empty;

        public ImageArrangement _imageArrangement
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the whitespace gutter to include between rows
        /// </summary>
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

        /// <summary>
        /// Gets or sets the size of the whitespace gutter to include between items on the same row
        /// </summary>
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
                // Store any state we might need since (in theory) the layout could be in use by multiple
                // elements simultaneously
                // In reality for the Xbox Activity Feed there's probably only a single instance.
                context.LayoutState = new ActivityFeedLayoutState();
            }
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.UninitializeForContextCore(context);

            // clear any state
            context.LayoutState = null;
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            
            _pageWidth = availableSize.Width;
                
            _imageArrangement.SetImageRect(availableSize.Width);

            //var _rowIndex = _imageArrangement.FindFirstAndLastImageIndex(context.RealizationRect.Y, context.RealizationRect.Bottom);
            //Debug.Print("_rowIndex[0] " + _rowIndex[0] +
            //    "\n_rowIndex[1] " + _rowIndex[1]);

            var state = context.LayoutState as ActivityFeedLayoutState;
            state.LayoutRects.Clear();
            state.FirstRealizedIndex = -1;

            _extentHeight = 0;
            var changeFirstIndex = ChangeFirstIndex(availableSize.Width);
            
            if (context.ItemCount > 0)
            {
                int i = 0;
                double _lastextentHeight = 0;
                foreach (var _item in _imageArrangement.RowFirstIndex)
                {        
                    double zoomImg = (availableSize.Width - _imageArrangement.RowImgCount[i] * (_colSpacing - 1) - 16) / (_imageArrangement.NowWidth  - _imageArrangement.RowImgCount[i] * (_colSpacing - 1));
                    double LastZoomImg = (_lastWidth - _imageArrangement.RowImgCount[i] * (_colSpacing - 1) - 16) / (_imageArrangement.NowWidth - _imageArrangement.RowImgCount[i] * (_colSpacing - 1));
                    if (true)
                    {
                        if (_lastextentHeight >= context.RealizationRect.Top && _lastextentHeight <= context.RealizationRect.Bottom)
                        {
                            
                            if (state.FirstRealizedIndex == -1)
                            {
                                state.FirstRealizedIndex = _item;
                                
                                _firstItemIndex = _item;

                                //Debug.Print("RecommendedAnchorIndex " + context.RecommendedAnchorIndex +
                                //    "\navailableSize.Width " + availableSize.Width +
                                //    "\nRealizationRect.Top " + context.RealizationRect.Width +
                                //    "\nFirstRealizedIndex " + _item +
                                //    "\n_extentHeight" + _extentHeight);

                                //Debug.Print(context.LayoutOrigin.ToString());
                            }

                            int RowImgCount = _imageArrangement.RowImgCount[i];
                            double _newX = 0;
                        
                            for ( var j = 0; j < RowImgCount; j++)
                            {
                                var _index = _item + j;
                                var container = context.GetOrCreateElementAt(_index);
                                var _rect = _imageArrangement.ImageRect[_index];
                                Rect _size = new()
                                {
                                    Width = _rect.Width * zoomImg,
                                    Height = _rect.Height * zoomImg,
                                    X = _newX,
                                    Y = _extentHeight
                                };

                                container.Measure(new Size(_size.Width, _size.Height));
                                state.LayoutRects.Add(_size);
                                _newX += _size.Width + _colSpacing;

                                
                            }

                            //Debug.Print(_newX + "");
                        }
                    }
                    else
                    {
                        if (_item >= _firstItemIndex && _extentHeight <= context.RealizationRect.Bottom + 3000)
                        {
                            if (state.FirstRealizedIndex == -1)
                            {
                                state.FirstRealizedIndex = _item;

                                if (_lastWidth != availableSize.Width)
                                {
                                    var _rect = _imageArrangement.ImageRect[_item];
                                    var originY = _rect.Top;

                                    //context.LayoutOrigin = new Point(0, _rect.Top - _extentHeight);
                                    _lastWidth = availableSize.Width;
                                }

                                //Debug.Print("RecommendedAnchorIndex " + context.RecommendedAnchorIndex +
                                //    "\navailableSize.Width " + availableSize.Width +
                                //    "\nRealizationRect.Top " + context.RealizationRect.Top +
                                //    "\nFirstRealizedIndex " + _item +
                                //    "\n_extentHeight" + _extentHeight);

                                //Debug.Print("Donnt Change");

                                //Debug.Print(context.LayoutOrigin.ToString());
                            }

                            int RowImgCount = _imageArrangement.RowImgCount[i];
                            double _newX = 0;

                            for (var j = 0; j < RowImgCount; j++)
                            {
                                var _index = _item + j;
                                var container = context.GetOrCreateElementAt(_index);
                                var _rect = _imageArrangement.ImageRect[_index];
                                Rect _size = new()
                                {
                                    Width = _rect.Width * zoomImg,
                                    Height = _rect.Height * zoomImg,
                                    X = _newX,
                                    Y = _extentHeight
                                };

                                container.Measure(new Size(_size.Width, _size.Height));
                                state.LayoutRects.Add(_size);
                                _newX += _size.Width + _colSpacing;

                            }
                        }
                    }

                    
                    
                            
                    i++;    
                    _extentHeight += zoomImg * _imageArrangement.ImageRect[_item].Height + _rowSpacing;
                    _lastextentHeight += LastZoomImg * _imageArrangement.ImageRect[_item].Height + _rowSpacing;
                }
            
            }   
            
            _lastWidth = availableSize.Width;
            //context.LayoutOrigin = new Point(0, -500);
            return new Size(availableSize.Width, _extentHeight);
         
            var firstRowIndex = Math.Max(
                (int)(context.RealizationRect.Y / (this.MinItemSize.Height + this.RowSpacing)) ,
                0);
            var lastRowIndex = Math.Min(
                (int)(context.RealizationRect.Bottom / (this.MinItemSize.Height + this.RowSpacing)) + 1,
                (int)(context.ItemCount / 3));

            // Determine which items will appear on those rows and what the rect will be for each item


            // Save the index of the first realized item.  We'll use it as a starting point during arrange.
            // state.FirstRealizedIndex = firstRowIndex * 3;

            // ideal item width that will expand/shrink to fill available space
            double desiredItemWidth = Math.Max(this.MinItemSize.Width, (availableSize.Width - this.ColumnSpacing * 3) / 4);

            // Foreach item between the first and last index,
            //     Call GetElementOrCreateElementAt which causes an element to either be realized or retrieved
            //       from a recycle pool
            //     Measure the element using an appropriate size
            //
            // Any element that was previously realized which we don't retrieve in this pass (via a call to
            // GetElementOrCreateAt) will be automatically cleared and set aside for later re-use.
            // Note: While this work fine, it does mean that more elements than are required may be
            // created because it isn't until after our MeasureOverride completes that the unused elements
            // will be recycled and available to use.  We could avoid this by choosing to track the first/last
            // index from the previous layout pass.  The diff between the previous range and current range
            // would represent the elements that we can pre-emptively make available for re-use by calling
            // context.RecycleElement(element).
            //for (int rowIndex = firstRowIndex; rowIndex < lastRowIndex; rowIndex++)
            //{
            //    int firstItemIndex = rowIndex * 3;
            //    var boundsForCurrentRow = CalculateLayoutBoundsForRow(rowIndex, desiredItemWidth);

            //    for (int columnIndex = 0; columnIndex < 3; columnIndex++)
            //    {
            //        var index = firstItemIndex + columnIndex;
            //        var container = context.GetOrCreateElementAt(index);

            //        container.Measure(
            //            new Size(boundsForCurrentRow[columnIndex].Width, boundsForCurrentRow[columnIndex].Height));

            //        // state.LayoutRects.Add(boundsForCurrentRow[columnIndex]);
            //    }
            //}

           // Calculate and return the size of all the content (realized or not) by figuring out
           // what the bottom/right position of the last item would be.
           var extentHeight = ((int)(context.ItemCount / 3) - 1 ) * (this.MinItemSize.Height + this.RowSpacing) + this.MinItemSize.Height;
           var ex = Math.Max(0, extentHeight);
            // Report this as the desired size for the layout
           return new Size(desiredItemWidth * 4 + this.ColumnSpacing * 2, ex);
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

        private bool ChangeFirstIndex(double _width)
        {
            if (_width == _lastWidth)
            {
                return true;
            }
            else
            {
                _lastWidth = _width;
                return false;
            }

            
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
