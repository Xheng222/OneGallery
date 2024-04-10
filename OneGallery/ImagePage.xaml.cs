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

        ConnectedAnimation imageAnimation { get; set; }

        bool isLoaded = false;

        int AnimateState = 0;

        bool isPointInToolBar = false;

        private MainWindow Window { get; set; }

        readonly string SourcePageName;

        public ImagePage()
        {
            this.InitializeComponent();
            Window = (MainWindow)(Application.Current as App).Main;
            SourcePageName = (Window.NaView.SelectedItem as Category).Name;
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }



        ~ImagePage()
        {
            Debug.Print("~" + GetType().Name);
          
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);
            // Store the item to be used in binding to UI
            ChooseImage = e.Parameter as PictureClass;
            imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");

            imageAnimation.Completed += ImageAnimationCompleted;
            ToolsBarIn.Completed += ToolBarInCompelete;

            NowImagePage = this;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (Window.NaView.SelectedItem is Category)
            {

                if (SourcePageName == (Window.NaView.SelectedItem as Category).Name)
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
                Window.NaPage.BackStack.RemoveAt(Window.NaPage.BackStack.Count - 1);
                Window.HistoryPages.Pop();
            }

            base.OnNavigatedFrom(e);

            NowImagePage = null;

            PageGrid.PointerMoved -= Grid_PointerMoved;
            PageGrid.PointerPressed -= Grid_PointerPressed;
            PageGrid.PointerReleased -= Grid_PointerReleased;
            PageGrid.PointerWheelChanged -= Grid_PointerWheelChanged;

            imageAnimation.Completed -= ImageAnimationCompleted;
            ToolsBarIn.Completed -= ToolBarInCompelete;





            GC.Collect();
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

            RightEllipse.ScaleTransition = new() { Duration = tempTimeSpan };
            RightEllipseBorder.ScaleTransition = new() { Duration = tempTimeSpan };
            RightFontIcon.ScaleTransition = new() { Duration = tempTimeSpan };

            CenterEllipse.ScaleTransition = new() { Duration = tempTimeSpan };
            CenterEllipseBorder.ScaleTransition = new() { Duration = tempTimeSpan };
            CenterFontIcon.ScaleTransition = new() { Duration = tempTimeSpan };

            ResetEllipse.ScaleTransition = new() { Duration = tempTimeSpan };
            ResetEllipseBorder.ScaleTransition = new() { Duration = tempTimeSpan };
            ResetFontIcon.ScaleTransition = new() { Duration = tempTimeSpan };

            DeleteEllipse.ScaleTransition = new() { Duration = tempTimeSpan };
            DeleteEllipseBorder.ScaleTransition = new() { Duration = tempTimeSpan };
            DeleteFontIcon.ScaleTransition = new() { Duration = tempTimeSpan };
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

            if (ViewChanged)
            {
                ResetEllipseIn.Begin();
                ResetFontIconIn.Begin();
            }
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

            if (ViewChanged)
            {
                ResetEllipseOut.Begin();
                ResetFontIconOut.Begin();
            }

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

            if (ViewChanged)
            {
                ResetEllipseOut.Pause();
                ResetFontIconOut.Pause();
            }
        }

        private void SwitchResetBtn(int state)
        {
            if (state == 1)
            {
                if (!ViewChanged)
                {
                    ViewChanged = true;
                    
                    ResetFontIcon.PointerEntered += ResetFontIcon_PointerEntered;
                    ResetFontIcon.PointerExited += ResetFontIcon_PointerExited;
                    ResetFontIcon.PointerPressed += ResetFontIcon_PointerPressed;
                    ResetFontIcon.PointerReleased += ResetFontIcon_PointerReleased;

                    if (ToolsBar.Opacity != 0)
                    {
                        ResetEllipseIn.Begin();
                        ResetFontIconIn.Begin();
                    }

                }
                return;
            }
            
            if (state == 0)
            {
                ViewChanged = false;
                ResetFontIcon.PointerEntered -= ResetFontIcon_PointerEntered;
                ResetFontIcon.PointerExited -= ResetFontIcon_PointerExited;
                ResetFontIcon.PointerPressed -= ResetFontIcon_PointerPressed;
                ResetFontIcon.PointerReleased -= ResetFontIcon_PointerReleased;

                ResetEllipseOut.Begin();
                ResetFontIconOut.Begin();
                ResetEllipseBorderOut.Begin();
                isPointInToolBar = false;
                

            }
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

        private void MoveBorder(PointerRoutedEventArgs e, int choose)
        {            
            switch(choose)
            {
                case 1:
                    {
                        var deltaX = e.GetCurrentPoint(LeftFontIcon).Position.X - RotateMousePoint.Position.X;
                        deltaX = Math.Min(ToolsBar.Width - 50, Math.Max(deltaX, 0));

                        if (RotationState == 0)
                        {
                            RotationState = 1;
                            scrollViewer.RotationTransition = null;
                            StartRotate = scrollViewer.Rotation;

                            CenterEllipseOut.Begin();
                            CenterFontIconOut.Begin();

                            RightEllipseOut.Begin();
                            RightFontIconOut.Begin();

                            LeftFontIconForRotateIn.Begin();
                            LeftFontIconOut.Begin();
                        }

                        scrollViewer.Rotation = (float)(StartRotate - deltaX * 360 / (ToolsBar.Width - 50));
                        LeftEllipseBorder.Width = deltaX + 50;
                        break;

                    }

                case -1:
                    {
                        var deltaX = RotateMousePoint.Position.X - e.GetCurrentPoint(RightFontIcon).Position.X;
                        deltaX = Math.Min(ToolsBar.Width - 50, Math.Max(deltaX, 0));

                        if (RotationState == 0)
                        {
                            RotationState = -1;
                            scrollViewer.RotationTransition = null;
                            StartRotate = scrollViewer.Rotation;

                            CenterEllipseOut.Begin();
                            CenterFontIconOut.Begin();

                            LeftEllipseOut.Begin();
                            LeftFontIconOut.Begin();

                            RightFontIconForRotateIn.Begin();
                            RightFontIconOut.Begin();
                        }

                        scrollViewer.Rotation = (float)(StartRotate + deltaX * 360 / (ToolsBar.Width - 50));
                        RightEllipseBorder.Width = deltaX + 50;
                        break;
                    }

                default: return;
            }
            
        }

        private void ResetBorder()
        {
            scrollViewer.RotationTransition = new()
            {
                Duration = TimeSpan.FromMilliseconds(200)
            };

            CenterEllipseIn.Begin();
            CenterFontIconIn.Begin();

            switch(RotationState)
            {
                case 1:
                    {
                        LeftFontIconForRotateOut.Begin();
                        LeftFontIconIn.Begin();
                        RightEllipseIn.Begin();
                        RightFontIconIn.Begin();
                        break;
                    }

                case -1:
                    {
                        RightFontIconForRotateOut.Begin();
                        RightFontIconIn.Begin();
                        LeftEllipseIn.Begin();
                        LeftFontIconIn.Begin();
                        break;
                    }
                default: return;
            }

            RotationState = 0;
        }

        private void EnterToolBar(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(PageGrid).Position;
            var _tempY = point.Y - ActualWidth / 2;

            if (_tempY > ActualHeight - 100 && _tempY < ActualHeight - 50)
            {
                var _tempX = point.X;
                if (_tempX > LeftEllipse.Margin.Left && _tempX < LeftEllipse.Margin.Left + 50)
                {
                    LeftFontIcon_PointerEntered(null, e);
                    return;
                }

                if (_tempX > LeftEllipse.Margin.Left + ToolsBar.Width - 50 && _tempX < LeftEllipse.Margin.Left + ToolsBar.Width)
                {
                    RightFontIcon_PointerEntered(null, e);
                    return;
                }

                if (ViewChanged)
                {
                    if (_tempX > ResetEllipse.Margin.Left && _tempX < ResetEllipse.Margin.Left + 50)
                    {
                        ResetFontIcon_PointerEntered(null, e);
                        return;
                    }
                }


                if (_tempX > DeleteEllipse.Margin.Right - 50 && _tempX < DeleteEllipse.Margin.Left)
                {
                    DeleteFontIcon_PointerEntered(null, e);
                    return;
                }

                if (_tempX > LeftEllipse.Margin.Left + ToolsBar.Width / 2 - 25 && _tempX < LeftEllipse.Margin.Left + ToolsBar.Width / 2 + 25)
                {
                    CenterFontIcon_PointerEntered(null, e);
                    return;
                }
            }

        }

        bool ViewChanged = false;

        public async Task ResetAll()
        {
            if (ViewChanged)
            {
                var _temp = (int)scrollViewer.Rotation;
                if (_temp < 0)
                    _temp -= 180;
                else
                    _temp += 180;
                _temp = (_temp / 360) * 360;

                if (scrollViewer.Rotation != _temp)
                {
                    scrollViewer.Rotation = _temp;
                    await Task.Delay(250);
                }


                if (Zoom != 1)
                {
                    ImageBorder.Width /= Zoom;
                    ImageBorder.Height /= Zoom;
                    Zoom = 1;
                    CalculateScrollViewer();
                    CorrectOffset(ScrollBackWidth, ScrollBackHeight);
                    CalculateImage();
                    SwitchResetBtn(0);
                    await Task.Delay(400);
                    return;
                }

                CalculateScrollViewer();
                CorrectOffset(ScrollBackWidth, ScrollBackHeight);
                CalculateImage();
                SwitchResetBtn(0);

            }

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

            RightEllipse.Margin = new(0, 0, _tempMarginWidth, _tempMarginHeight);
            RightFontIcon.Margin = new(0, 0, _tempMarginWidth, _tempMarginHeight);
            RightEllipseBorder.Margin = new(0, 0, _tempMarginWidth, _tempMarginHeight);
            RightFontIconForRotate.Margin = new(0, 0, _tempMarginWidth, _tempMarginHeight);

            CenterEllipse.Margin = new(0, 0, 0, _tempMarginHeight);
            CenterFontIcon.Margin = new(0, 0, 0, _tempMarginHeight);
            CenterEllipseBorder.Margin = new(0, 0, 0, _tempMarginHeight);

            DeleteEllipse.Margin = new(0, 0, _tempMarginWidth - 100, _tempMarginHeight);
            DeleteFontIcon.Margin = new(0, 0, _tempMarginWidth - 100, _tempMarginHeight);
            DeleteEllipseBorder.Margin = new(0, 0, _tempMarginWidth - 100, _tempMarginHeight);

            ResetEllipse.Margin = new(_tempMarginWidth - 100, 0, 0, _tempMarginHeight);
            ResetFontIcon.Margin = new(_tempMarginWidth - 100, 0, 0, _tempMarginHeight);
            ResetEllipseBorder.Margin = new(_tempMarginWidth - 100, 0, 0, _tempMarginHeight);


            if (isLoaded)
            {
                CalculateScrollViewer();
                CalculateImage();
                scrollViewer.CenterPoint = new((float)_tempLength / 2, (float)_tempLength / 2, 0);
                CorrectOffset(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
            }

        }

        Pointer Pointer { get; set; }
        PointerPoint ScrollMousePoint { get; set; }

        double hOff = 1;
        double vOff = 1;

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (scrollViewer.PointerCaptures != null && scrollViewer.PointerCaptures.Count > 0)
            {
                //e.Handled = true;

                var deltaX = hOff + (ScrollMousePoint.Position.X - e.GetCurrentPoint(scrollViewer).Position.X);
                var deltaY = vOff + (ScrollMousePoint.Position.Y - e.GetCurrentPoint(scrollViewer).Position.Y);

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

                if (scrollViewer.ChangeView(deltaX, deltaY, null))
                    SwitchResetBtn(1);
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
                Pointer = e.Pointer;
                ScrollMousePoint = e.GetCurrentPoint(scrollViewer);
                hOff = scrollViewer.HorizontalOffset;
                vOff = scrollViewer.VerticalOffset;
                scrollViewer.CapturePointer(Pointer);
            }

        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (scrollViewer.PointerCaptures != null && scrollViewer.PointerCaptures.Count > 0)
            {
                CorrectOffset(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
                scrollViewer.ReleasePointerCaptures();
            }

            EnterToolBar(e);
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
                        ChangeSrollView(_newZoom, Zoom);
                        SwitchResetBtn(1);
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
                        ChangeSrollView(_newZoom, Zoom);
                        SwitchResetBtn(1);
                    }
                    else
                    {
                        if (_tempWidth == ActualHeight + ActualWidth)
                        {
                            ImageBorderWidthDown.Begin();
                            ImageBorderHeightDown.Begin();

                            await Task.Delay(500);
                            ChangeSrollView(1, Zoom);
                            SwitchResetBtn(1);
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

            if (ToolsBar.Opacity == 0)
                ShowToolsBar();

            CenterEllipseBorderIn.Begin();
            CenterEllipse.Scale = new Vector3((float)1.06, (float)1.06, 1);
            CenterEllipseBorder.Scale = new Vector3((float)1.06, (float)1.06, 1);
            CenterFontIcon.Scale = new Vector3((float)1.06, (float)1.06, 1);
        }

        private void CenterFontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            CenterEllipse.Scale = new Vector3(1, 1, 1);
            CenterEllipseBorder.Scale = new Vector3(1, 1, 1);
            CenterFontIcon.Scale = new Vector3(1, 1, 1);

            if (CenterFontIcon.PointerCaptures == null || CenterFontIcon.PointerCaptures.Count == 0)
            {
                isPointInToolBar = false;
                CenterEllipseBorderOut.Begin();
            }
        }

        private void CenterFontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            CenterFontIcon.CapturePointer(e.Pointer);
            RotateMousePoint = e.GetCurrentPoint(CenterFontIcon);
            CenterEllipse.Scale = new Vector3((float)0.94, (float)0.94, 1);
            CenterEllipseBorder.Scale = new Vector3((float)0.94, (float)0.94, 1);
            CenterFontIcon.Scale = new Vector3((float)0.94, (float)0.94, 1);
        }

        private void CenterFontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            CenterFontIcon.ReleasePointerCaptures();

            if (CenterFontIcon.Scale.X == (float)0.94)
            {
                Window.Nv_BackRequested(null, null);
                return;
            }

            EnterToolBar(e);

            if (RightFontIcon.Scale.X == 1)
            {
                isPointInToolBar = false;
                CenterEllipseBorderOut.Begin();
            }
        }

        /*
         * RightFontIcon
         */

        int RotationState = 0;
        float StartRotate;
        PointerPoint RotateMousePoint;

        private void RightFontIcon_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            if (RightFontIcon.PointerCaptures != null && RightFontIcon.PointerCaptures.Count > 0)
            {      
                MoveBorder(e, -1);
                SwitchResetBtn(1);
            }

        }

        private void RightFontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPointInToolBar = true;

            if (ToolsBar.Opacity == 0)
                ShowToolsBar();
            RightEllipseBorderIn.Begin();

            RightEllipse.Scale = new Vector3((float)1.06, (float)1.06, 1);
            RightEllipseBorder.Scale = new Vector3((float)1.06, (float)1.06, 1);
            RightFontIcon.Scale = new Vector3((float)1.06, (float)1.06, 1);
        }

        private void RightFontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            e.Handled = true;

            RightEllipse.Scale = new Vector3(1, 1, 1);
            RightEllipseBorder.Scale = new Vector3(1, 1, 1);
            RightFontIcon.Scale = new Vector3(1, 1, 1);

            if (RightFontIcon.PointerCaptures == null || RightFontIcon.PointerCaptures.Count == 0)
            {
                isPointInToolBar = false;
                RightEllipseBorderOut.Begin();
            }
        }

        private void RightFontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            RightFontIcon.CapturePointer(e.Pointer);
            RotateMousePoint = e.GetCurrentPoint(RightFontIcon);
            RightEllipse.Scale = new Vector3((float)0.94, (float)0.94, 1);
            RightEllipseBorder.Scale = new Vector3((float)0.94, (float)0.94, 1);
            RightFontIcon.Scale = new Vector3((float)0.94, (float)0.94, 1);
        }

        private void RightFontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            RightFontIcon.ReleasePointerCaptures();
            RightEllipseBorderReset.Begin();
            EnterToolBar(e);

            if (RotationState == -1)
            {
                ResetBorder();
            }

            if (RightFontIcon.Scale.X == 1)
            {
                isPointInToolBar = false;
                RightEllipseBorderOut.Begin();
                return;
            }

            scrollViewer.Rotation += 90;
            SwitchResetBtn(1);
        }

        /*
         * LeftFontIcon
         */

        private void LeftFontIcon_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            
            if (LeftFontIcon.PointerCaptures != null && LeftFontIcon.PointerCaptures.Count > 0)
            {
                MoveBorder(e, 1);
                SwitchResetBtn(1);
            }

        }

        private void LeftFontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

            e.Handled = true;
            isPointInToolBar = true;

            if (ToolsBar.Opacity == 0)
                ShowToolsBar();
            
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
            EnterToolBar(e);

            if (RotationState == 1)
            {
                ResetBorder();
            }
            
            if (LeftFontIcon.Scale.X == 1)
            {
                LeftEllipseBorderOut.Begin();
                isPointInToolBar = false;
                return;
            }
            
            scrollViewer.Rotation -= 90;
            SwitchResetBtn(1);
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

        /*
         * Reset
         */

        private void ResetFontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Debug.Print("Entered");
            e.Handled = true;
            isPointInToolBar = true;

            if (ToolsBar.Opacity == 0)
                ShowToolsBar();

            ResetEllipseBorderIn.Begin();
            ResetEllipse.Scale = new Vector3((float)1.06, (float)1.06, 1);
            ResetEllipseBorder.Scale = new Vector3((float)1.06, (float)1.06, 1);
            ResetFontIcon.Scale = new Vector3((float)1.06, (float)1.06, 1);
        }

        private void ResetFontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            Debug.Print("ResetFontIcon_PointerExited");
            ResetEllipse.Scale = new Vector3(1, 1, 1);
            ResetEllipseBorder.Scale = new Vector3(1, 1, 1);
            ResetFontIcon.Scale = new Vector3(1, 1, 1);

            if (ResetFontIcon.PointerCaptures == null || ResetFontIcon.PointerCaptures.Count == 0)
            {
                isPointInToolBar = false;
                ResetEllipseBorderOut.Begin();
            }
        }

        private void ResetFontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            Debug.Print("ResetFontIcon_PointerPressed");
            ResetFontIcon.CapturePointer(e.Pointer);
            RotateMousePoint = e.GetCurrentPoint(ResetFontIcon);
            ResetEllipse.Scale = new Vector3((float)0.94, (float)0.94, 1);
            ResetEllipseBorder.Scale = new Vector3((float)0.94, (float)0.94, 1);
            ResetFontIcon.Scale = new Vector3((float)0.94, (float)0.94, 1);
        }

        private void ResetFontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
           
            e.Handled = true;
            Debug.Print("ResetFontIcon_PointerReleased");
            ResetFontIcon.ReleasePointerCaptures();

            if (ResetFontIcon.Scale.X == (float)0.94)
            {
                _ = ResetAll();
                return;
            }

            EnterToolBar(e);

            if (ResetFontIcon.Scale.X == 1)
            {
                isPointInToolBar = false;
                ResetEllipseBorderOut.Begin();
            }
        }

    }






}
