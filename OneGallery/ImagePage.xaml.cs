using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.Composition;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class ImagePage : Page
    {
        public PictureClass ChooseImage { get; set; }

        public static ImagePage NowImagePage { get; set; }

        public float ScrollViewerRotation 
        {
            get { return scrollViewer.Rotation; }
            set { scrollViewer.Rotation = value; }
        }

        float Zoom = 1f;

        double ScrollBackWidth;

        double ScrollBackHeight;

        ConnectedAnimation imageAnimation;

        bool isLoaded = false;

        int AnimateState = 0;

        bool isPointInToolBar = false;

        readonly MainWindow window;

        readonly string SourcePageName;

        public ImagePage()
        {
            this.InitializeComponent();
            window = (MainWindow)(Application.Current as App).m_window;
            SourcePageName = (window.NaView.SelectedItem as Category).Name;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);
            // Store the item to be used in binding to UI
            ChooseImage = e.Parameter as PictureClass;
            imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            // Connected animation + coordinated animation

            imageAnimation.Completed += ImageAnimationCompleted;
            ToolsBarIn.Completed += ToolBarInCompelete;

            NowImagePage = this;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (window.NaView.SelectedItem is Category)
            {

                if (SourcePageName == (window.NaView.SelectedItem as Category).Name)
                {
                    var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackwardConnectedAnimation", image);
                    anim.Configuration = new DirectConnectedAnimationConfiguration();

                }

            }

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                window.page.BackStack.RemoveAt(window.page.BackStack.Count - 1);
                window.HistoryPages.Pop();

            }

            base.OnNavigatedFrom(e);

            NowImagePage = null;
        }

        /*
         * Utils
         */

        private void ImageAnimationCompleted(ConnectedAnimation send, object e)
        {
            PageGrid.PointerMoved += Grid_PointerMoved;
            PageGrid.PointerPressed += Grid_PointerPressed;
            PageGrid.PointerReleased += Grid_PointerReleased;
            PageGrid.PointerWheelChanged += Grid_PointerWheelChanged;

            var tempTimeSpan = TimeSpan.FromMilliseconds(200);

            LeftEllipse.ScaleTransition = new() { Duration = tempTimeSpan };
            LeftEllipseBorder.ScaleTransition = new() { Duration = tempTimeSpan };
            LeftFontIcon.ScaleTransition = new() { Duration = tempTimeSpan };

            LeftFontIcon.TranslationTransition = new Vector3Transition()
            {
                Components = Vector3TransitionComponents.X,
                Duration = tempTimeSpan
            };

        }

        private void CalculateImage()
        {
            var _extra = (image.ActualWidth * Zoom - ActualWidth) / 2;
            
            if (_extra > -100)
            {
                ScrollBackWidth = (ImageBorder.Width - scrollViewer.Width) / 2 - _extra - 100;
            }
            
            _extra = (image.ActualHeight * Zoom - ActualHeight) / 2;
            
            if (_extra > -100)
            {
                ScrollBackHeight = (ImageBorder.Height - scrollViewer.Height) / 2 - _extra - 100;
            }

            ScrollBackHeight -= (Zoom - 1) * 80 + 50;
            ScrollBackWidth -= (Zoom - 1) * 80 + 50;

        }

        private void CalculateScrollViewer()
        {
            ScrollBackWidth = (ImageBorder.Width - scrollViewer.Width) / 2;
            ScrollBackHeight = (ImageBorder.Height - scrollViewer.Height) / 2;

        }

        private async void CorrectOffset(double _horizontalOffset, double _verticalOffset)
        {
            await Task.Delay(1);
            if (_horizontalOffset < ScrollBackWidth)
                _horizontalOffset = ScrollBackWidth;
            else if (_horizontalOffset > scrollViewer.ScrollableWidth - ScrollBackWidth)
                _horizontalOffset = scrollViewer.ScrollableWidth - ScrollBackWidth;

            if (_verticalOffset < ScrollBackHeight)
                _verticalOffset = ScrollBackHeight;
            else if (_verticalOffset > scrollViewer.ScrollableHeight - ScrollBackHeight - ((Zoom - 1) * 160))
                _verticalOffset = scrollViewer.ScrollableHeight - ScrollBackHeight - ((Zoom - 1) * 160);

            scrollViewer.ChangeView(_horizontalOffset, _verticalOffset, null);
        }

        private void ShowToolsBar()
        {
            CheckAnimate();

            ToolsBarIn.Begin();

            LeftEllipseIn.Begin();
            LeftFontIconIn.Begin();

            CenterEllipseIn.Begin();
            CenterFontIconIn.Begin();

            RightEllipseIn.Begin();
            RightFontIconIn.Begin();

            DeleteEllipseIn.Begin();
            DeleteFontIconIn.Begin();
        }

        private async void ToolBarInCompelete(object sender, object e)
        {
            await Task.Delay(1500); 
            while (isPointInToolBar)
            {
                await Task.Delay(500);
            }
            
            ToolsBarOut.Begin();

            LeftEllipseOut.Begin();
            LeftFontIconOut.Begin();

            CenterEllipseOut.Begin();
            CenterFontIconOut.Begin();

            RightEllipseOut.Begin();
            RightFontIconOut.Begin();

            DeleteEllipseOut.Begin();
            DeleteFontIconOut.Begin();

            AnimateState = 1;

        }

        private void CheckAnimate()
        {
            ToolsBarOut.Pause();

            LeftEllipseOut.Pause();
            LeftFontIconOut.Pause();

            CenterEllipseOut.Pause();
            CenterFontIconOut.Pause();

            RightEllipseOut.Pause();
            RightFontIconOut.Pause();

            DeleteEllipseOut.Pause();
            DeleteFontIconOut.Pause();
        }

        private void SwitchToolbar()
        {

        }

        private void ChangeSrollView(float _newZoom, float _oldZoom)
        {
            Zoom = _newZoom;
            var _horizontalOffset = scrollViewer.HorizontalOffset  + image.ActualWidth * (Zoom - _oldZoom) / 2;
            var _verticalOffset = scrollViewer.VerticalOffset  + image.ActualHeight * (Zoom - _oldZoom) / 2 - 50;

            CalculateScrollViewer();
            CalculateImage();
            CorrectOffset(_horizontalOffset, _verticalOffset);
        }


        /*
         * Page
         */


        /*
         * Grid
         */
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var _tempLength = ActualHeight + ActualWidth;

            PageGrid.Height = _tempLength;
            PageGrid.Width = _tempLength;

            scrollViewer.Height = _tempLength;
            scrollViewer.Width = _tempLength;

            ImageBorder.Height = _tempLength * Zoom * 2;
            ImageBorder.Width = _tempLength * Zoom * 2;

            var _tempMarginHeight = _tempLength * Zoom - ActualHeight / 2 + 125;
            var _tempMarginWidth = _tempLength * Zoom - ActualWidth / 2 + 125;

            image.Height = ActualHeight - 250;
            image.Width = ActualWidth - 250;
            image.Margin = new(_tempMarginWidth, _tempMarginHeight - 75, _tempMarginWidth, _tempMarginHeight + 75);
            
            _tempMarginWidth = Math.Min(250, Math.Max(50, ActualWidth * 0.25));
            ToolsBar.Width = _tempMarginWidth * 2;


            _tempMarginWidth = _tempLength * 0.5 - _tempMarginWidth;
            _tempMarginHeight = ActualWidth * 0.5 + 50;

            ToolsBar.Margin = new(0, 0, 0, _tempMarginHeight);


            LeftEllipse.Margin = new(_tempMarginWidth, 0, 0, _tempMarginHeight);
            LeftFontIcon.Margin = new(_tempMarginWidth, 0, 0, _tempMarginHeight);
            LeftEllipseBorder.Margin = new(_tempMarginWidth, 0, 0, _tempMarginHeight);
            LeftFontIconForRotate.Margin = new(_tempMarginWidth, 0, 0, _tempMarginHeight);

            CenterEllipse.Margin = new(0, 0, 0, _tempMarginHeight);
            CenterFontIcon.Margin = new(0, 0, 0, _tempMarginHeight);

            RightEllipse.Margin = new(0, 0, _tempMarginWidth, _tempMarginHeight);
            RightFontIcon.Margin = new(0, 0, _tempMarginWidth, _tempMarginHeight);

            DeleteEllipse.Margin = new(0, 0, _tempMarginWidth - 100, _tempMarginHeight);
            DeleteFontIcon.Margin = new(0, 0, _tempMarginWidth - 100, _tempMarginHeight);

            if (isLoaded)
            {
                CalculateScrollViewer();
                CalculateImage();
                scrollViewer.CenterPoint = new((float)_tempLength / 2, (float)_tempLength / 2, 0);
                CorrectOffset(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
            }

        }

        Pointer pointer;
        PointerPoint scrollMousePoint;
        double hOff = 1;
        double vOff = 1;

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (scrollViewer.PointerCaptures != null && scrollViewer.PointerCaptures.Count > 0)
            {
                //e.Handled = true;

                var deltaX = hOff + (scrollMousePoint.Position.X - e.GetCurrentPoint(scrollViewer).Position.X);
                var deltaY = vOff + (scrollMousePoint.Position.Y - e.GetCurrentPoint(scrollViewer).Position.Y);

                if (deltaX < ScrollBackWidth)
                {
                    deltaX = 2500 / (0.2 * (ScrollBackWidth - deltaX) + 50) + ScrollBackWidth - 50;

                }
                else if (deltaX > scrollViewer.ScrollableWidth - ScrollBackWidth)
                {
                    deltaX = scrollViewer.ScrollableWidth - ScrollBackWidth + 50 - 2500 / (0.2 * (deltaX - scrollViewer.ScrollableWidth + ScrollBackWidth) + 50);
                }

                if (deltaY < ScrollBackHeight)
                {
                    deltaY = 2500 / (0.2 * (ScrollBackHeight - deltaY) + 50) + ScrollBackHeight - 50;
                }
                else if (deltaY > scrollViewer.ScrollableHeight - ScrollBackHeight - (Zoom - 1) * 160)
                {
                    deltaY = scrollViewer.ScrollableHeight - ScrollBackHeight - (Zoom - 1) * 160 + 50 - 2500 / (0.2 * (deltaY - scrollViewer.ScrollableHeight + ScrollBackHeight + (Zoom - 1) * 160) + 50);
                }

                //if (deltaY < ScrollBackHeight)
                //{
                //    deltaY = 2500 / (0.2 * (ScrollBackHeight - deltaY) + 50) + ScrollBackHeight - 50;
                //}
                //else if (deltaY > scrollViewer.ScrollableHeight - ScrollBackHeight)
                //{
                //    deltaY = scrollViewer.ScrollableHeight - ScrollBackHeight + 50  - 2500 / (0.2 * (deltaY - scrollViewer.ScrollableHeight + ScrollBackHeight) + 50);
                //}

                scrollViewer.ChangeView(deltaX, deltaY, null);
            }
            else
            {
                if (AnimateState == 0 && isPointInToolBar == false)
                {
                    AnimateState = 2;
                    ShowToolsBar();
                }
                else if (AnimateState == 1)
                {
                    AnimateState = 0;
                }
            }


        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(scrollViewer).Properties.IsLeftButtonPressed)
            {
                pointer = e.Pointer;
                scrollMousePoint = e.GetCurrentPoint(scrollViewer);
                hOff = scrollViewer.HorizontalOffset;
                vOff = scrollViewer.VerticalOffset;
                scrollViewer.CapturePointer(pointer);
            }

        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (scrollViewer.PointerCaptures != null && scrollViewer.PointerCaptures.Count > 0)
            {
                CorrectOffset(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
                scrollViewer.ReleasePointerCaptures();
            }

        }

        int WheelState = 0;

        private async void Grid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (WheelState == 0)
            {
                WheelState = 1;
                var _tempPointer = e.GetCurrentPoint(scrollViewer);
                var _mouseWheelDelta = _tempPointer.Properties.MouseWheelDelta;
                float _newZoom;

                if (_mouseWheelDelta > 0)
                {
                    var _tempWidth = ImageBorder.Width + 600;
                    _newZoom = (float)(_tempWidth / ImageBorder.Width * Zoom);
                    if (_newZoom <= 5)
                    {
                        ImageBorderWidthUp.Begin();
                        ImageBorderHeightUp.Begin();           
                        
                        await Task.Delay(500);
                        //ImageBorder.Height = _tempHeight;
                        //ImageBorder.Width = _tempWidth;
                        ChangeSrollView(_newZoom, Zoom);
                        //CalculateScrollViewer();
                        //CalculateImage();
                        //CorrectOffset(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
                    }
                }
                else
                {
                    var _tempWidth = ImageBorder.Width - 600;
                    _newZoom = (float)(_tempWidth / ImageBorder.Width * Zoom);
                    if (_newZoom >= 1)
                    {
                        ImageBorderWidthDown.Begin();
                        ImageBorderHeightDown.Begin();

                        await Task.Delay(500);
                        //ImageBorder.Height = _tempHeight;
                        //ImageBorder.Width = _tempWidth;
                        //CalculateScrollViewer();
                        //CalculateImage();
                        //CorrectOffset(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
                        ChangeSrollView(_newZoom, Zoom);

                    }
                    else
                    {
                        if (_tempWidth == ActualHeight + ActualWidth)
                        {
                            ImageBorderWidthDown.Begin();
                            ImageBorderHeightDown.Begin();

                            await Task.Delay(500);
                            //ImageBorder.Height = _tempHeight;
                            //ImageBorder.Width = _tempWidth;
                            //CalculateScrollViewer();
                            //CalculateImage();
                            //CorrectOffset(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
                            ChangeSrollView(1, Zoom);
                        }
                    }


                        
                }

                WheelState = 0;
            }
        }

        /*
         * ScrollViewer
         */

        private void scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            CalculateScrollViewer();
            scrollViewer.ChangeView(ScrollBackWidth, ScrollBackHeight, Zoom);
            scrollViewer.CenterPoint = new((float)scrollViewer.Width / 2, (float)scrollViewer.Height / 2, 0);
            scrollViewer.RegisterAnchorCandidate(image);
            scrollViewer.RotationTransition = new()
            {
                Duration = TimeSpan.FromMilliseconds(200)
            };

        }

        /*
         * Image
         */

        private async void image_Loaded(object sender, RoutedEventArgs e)
        {
            CalculateImage();
            isLoaded = true;
            await Task.Delay(1);
            imageAnimation?.TryStart(image);
        }


        /*
         * ToolBar
         * 
         * DeleteFontIcon
         */

        private void DeleteFontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPointInToolBar = true;

        }

        private void DeleteFontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPointInToolBar = false;
            //AnimateState = 0;
        }

        private void DeleteFontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void DeleteFontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        /*
         * CenterFontIcon
         */

        private void CenterFontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPointInToolBar = true;

        }

        private void CenterFontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPointInToolBar = false;
            //AnimateState = 0;
        }

        private void CenterFontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void CenterFontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        /*
         * RightFontIcon
         */

        PointerPoint RotateMousePoint;

        private void RightFontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPointInToolBar = true;
        }

        private void RightFontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPointInToolBar = false;
            //AnimateState = 0;
        }

        private void RightFontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void RightFontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            scrollViewer.Rotation += 90;
        }

        /*
         * LeftFontIcon
         */

        int LeftState = 0;
        float StartRotate;

        private void LeftFontIcon_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            if (LeftFontIcon.PointerCaptures != null && LeftFontIcon.PointerCaptures.Count > 0)
            {
                var deltaX = e.GetCurrentPoint(LeftFontIcon).Position.X - RotateMousePoint.Position.X;
                deltaX = Math.Min(ToolsBar.Width - 50, Math.Max(deltaX, 0));
                //Debug.Print(" " + e.GetCurrentPoint(LeftFontIcon).Position.X);
                LeftEllipseBorder.Width = deltaX + 50;
                if (LeftState == 0)
                {
                    LeftState = 1;
                    scrollViewer.RotationTransition = null;
                    StartRotate = scrollViewer.Rotation;

                    LeftFontIconForRotate.Opacity = 1;
                    LeftFontIcon.Opacity = 0;

                    CenterEllipseOut.Begin();
                    CenterFontIconOut.Begin();

                    RightEllipseOut.Begin();
                    RightFontIconOut.Begin();

                }
                scrollViewer.Rotation = (float)(StartRotate + deltaX * 360 / (ToolsBar.Width - 50));
                LeftFontIconForRotate.Rotation = -(float)(deltaX * 360 / (ToolsBar.Width - 50));
            }
        }

        private void LeftFontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPointInToolBar = true;
            LeftEllipseBorderIn.Begin();
            LeftEllipse.Scale = new Vector3((float)1.06, (float)1.06, 1);
            LeftEllipseBorder.Scale = new Vector3((float)1.06, (float)1.06, 1);
            LeftFontIcon.Scale = new Vector3((float)1.06, (float)1.06, 1);

        }

        private void LeftFontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;        
            
            LeftEllipse.Scale = new Vector3(1, 1, 1);
            LeftEllipseBorder.Scale = new Vector3(1, 1, 1);
            LeftFontIcon.Scale = new Vector3(1, 1, 1);
            
            if (LeftFontIcon.PointerCaptures == null || LeftFontIcon.PointerCaptures.Count == 0)
            {
                isPointInToolBar = false;
                LeftEllipseBorderOut.Begin();
            }
                

                
        }

        private void LeftFontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            LeftFontIcon.CapturePointer(e.Pointer);
            RotateMousePoint = e.GetCurrentPoint(LeftFontIcon);
            LeftEllipse.Scale = new Vector3((float)0.94, (float)0.94, 1);
            LeftEllipseBorder.Scale = new Vector3((float)0.94, (float)0.94, 1);
            LeftFontIcon.Scale = new Vector3((float)0.94, (float)0.94, 1);
        }

        private void LeftFontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;   
            LeftFontIcon.ReleasePointerCaptures();
            LeftEllipseBorderReset.Begin();

            if (LeftState == 1)
            {
                scrollViewer.RotationTransition = new()
                {
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                
                LeftFontIconForRotate.Rotation = 0;
                LeftFontIcon.Opacity = 1;
                LeftFontIconForRotate.Opacity = 0;

                CenterEllipseIn.Begin();
                CenterFontIconIn.Begin();

                RightEllipseIn.Begin();
                RightFontIconIn.Begin();

                LeftState = 0;
            }
            
            if (LeftFontIcon.Scale.X == 1)
            {
                //isPointInToolBar = false;
                LeftEllipseBorderOut.Begin();


                return;
            }
            
            
            LeftEllipse.Scale = new Vector3((float)1.06, (float)1.06, 1);
            LeftEllipseBorder.Scale = new Vector3((float)1.06, (float)1.06, 1);
            LeftFontIcon.Scale = new Vector3((float)1.06, (float)1.06, 1);

            scrollViewer.Rotation += 90;

            CorrectOffset(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);

        }

        /*
         * ToolsBar
         */

        private void ToolsBar_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //e.Handled = true;
            isPointInToolBar = true;
        }

        private void ToolsBar_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //e.Handled = true;
            isPointInToolBar = false;
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate)
            {
                return;
            }
            if (scrollViewer.ZoomFactor != Zoom)
            {
                var _oldZoom = Zoom;
                Zoom = scrollViewer.ZoomFactor;
                var _horizontalOffset = scrollViewer.HorizontalOffset * Zoom / _oldZoom + (Zoom - _oldZoom) * ActualWidth / _oldZoom / 2;
                var _verticalOffset = scrollViewer.VerticalOffset * Zoom / _oldZoom + (Zoom - _oldZoom) * ActualHeight / _oldZoom / 2;
                CalculateScrollViewer();
                CalculateImage();
                //CorrectOffset(_horizontalOffset, _verticalOffset);

            }



        }

        private void scrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            var _final = e.FinalView;

        }
    }






}
