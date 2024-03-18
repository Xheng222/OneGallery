using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
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

        float Zoom = 2f;

        double ScrollBackWidth;

        double ScrollBackHeight;

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

            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            // Connected animation + coordinated animation
            imageAnimation?.TryStart(scrollViewer);

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
        }


        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.Print("Grid_SizeChanged");
            scrollViewer.Height = ActualHeight;
            scrollViewer.Width = ActualWidth;
            grid.Width = ActualWidth + 100;
            grid.Height = ActualHeight + 100;
            image.Height = ActualHeight - 50;

            scrollViewer.CenterPoint = new((float)ActualWidth, (float)ActualHeight, 0);
            ScrollBackWidth = ((Zoom - 1) * ActualWidth + 100 * Zoom) * 0.5;
            ScrollBackHeight = ((Zoom - 1) * ActualHeight + 100 * Zoom) * 0.5;


            scrollViewer.ChangeView(ScrollBackWidth, ScrollBackHeight, Zoom);


        }



        Pointer pointer;
        PointerPoint scrollMousePoint;
        double hOff = 1;
        double vOff = 1;

        private void scrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (scrollViewer.PointerCaptures != null && scrollViewer.PointerCaptures.Count > 0)
            {
                var deltaX = (hOff + (scrollMousePoint.Position.X - e.GetCurrentPoint(scrollViewer).Position.X)) / Zoom;
                var deltaY = (vOff + (scrollMousePoint.Position.Y - e.GetCurrentPoint(scrollViewer).Position.Y)) / Zoom;
                
                if (deltaX < 50)
                {
                    deltaX = 2500 * Zoom / (100 - deltaX);
                }
                else if (deltaX > scrollViewer.ScrollableWidth / Zoom - 50)
                {
                    deltaX = scrollViewer.ScrollableWidth - 2500 * Zoom / (deltaX  - scrollViewer.ScrollableWidth / Zoom + 100);
                }

                if (deltaY < 50)
                {
                    deltaY = 2500 * Zoom / (100 - deltaY);
                }
                else if (deltaY > scrollViewer.ScrollableHeight / Zoom - 50)
                {
                    deltaY = scrollViewer.ScrollableHeight - 2500 * Zoom / (deltaY - scrollViewer.ScrollableHeight / Zoom + 100);
                }
                
                scrollViewer.ChangeView(deltaX, deltaY, null);
            }

            //Debug.Print("actW" + scrollViewer.ScrollableWidth);
            //Debug.Print("actH" + scrollViewer.ScrollableHeight);

        }

        private void scrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            pointer = e.Pointer;
            scrollMousePoint = e.GetCurrentPoint(scrollViewer);
            hOff = scrollViewer.HorizontalOffset;
            vOff = scrollViewer.VerticalOffset;
            scrollViewer.CapturePointer(pointer);
        }

        private void scrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

            if (scrollViewer.VerticalOffset < 50 * Zoom)
                scrollViewer.ChangeView(null, 50 * Zoom, null);
            else if (scrollViewer.VerticalOffset > scrollViewer.ScrollableHeight - 50 * Zoom)
                scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight - 50 * Zoom, null);


            if (scrollViewer.HorizontalOffset < 50 * Zoom)
                scrollViewer.ChangeView(50 * Zoom, null, null);
            else if (scrollViewer.HorizontalOffset > scrollViewer.ScrollableWidth - 50 * Zoom)
                scrollViewer.ChangeView(scrollViewer.ScrollableWidth - 50 * Zoom, null, null);
            
            scrollViewer.ReleasePointerCaptures();
        }
    }






}
