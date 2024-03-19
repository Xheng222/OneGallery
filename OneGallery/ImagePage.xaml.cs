using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
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

        float Zoom = 1f;

        double ScrollBackWidth;

        double ScrollBackHeight;

        ConnectedAnimation imageAnimation;

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
            image.Height = ActualHeight - 100;
            image.Width = ActualWidth - 100;
            scrollViewer.CenterPoint = new((float)ActualWidth, (float)ActualHeight, 0);


        }



        Pointer pointer;
        PointerPoint scrollMousePoint;
        double hOff = 1;
        double vOff = 1;

        private void scrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (scrollViewer.PointerCaptures != null && scrollViewer.PointerCaptures.Count > 0)
            {
                var deltaX = hOff + (scrollMousePoint.Position.X - e.GetCurrentPoint(scrollViewer).Position.X);
                var deltaY = vOff + (scrollMousePoint.Position.Y - e.GetCurrentPoint(scrollViewer).Position.Y);

                if (deltaX < ScrollBackWidth)
                {
                    deltaX = 2500 / (0.2 * (ScrollBackWidth - deltaX) + 50) + ScrollBackWidth - 50;
                    //deltaX = 50 * ScrollBackWidth / (ScrollBackWidth + 50 - deltaX);
                }
                else if (deltaX > scrollViewer.ScrollableWidth - ScrollBackWidth)
                {
                    deltaX = scrollViewer.ScrollableWidth - ScrollBackWidth + 50 - 2500 / (0.2 * (deltaX - scrollViewer.ScrollableWidth + ScrollBackWidth) + 50);
                }

                if (deltaY < ScrollBackHeight)
                {
                    deltaY = 2500 / (0.2 * (ScrollBackHeight - deltaY) + 50) + ScrollBackHeight - 50;
                }
                else if (deltaY > scrollViewer.ScrollableHeight - ScrollBackHeight)
                {
                    deltaY = scrollViewer.ScrollableHeight - ScrollBackHeight + 50 - 2500 / (0.2 * (deltaY - scrollViewer.ScrollableHeight + ScrollBackHeight) + 50);
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

            if (scrollViewer.VerticalOffset < ScrollBackHeight)
                scrollViewer.ChangeView(null, ScrollBackHeight, null);
            else if (scrollViewer.VerticalOffset > scrollViewer.ScrollableHeight - ScrollBackHeight)
                scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight - ScrollBackHeight, null);


            if (scrollViewer.HorizontalOffset < ScrollBackWidth)
                scrollViewer.ChangeView(ScrollBackWidth, null, null);
            else if (scrollViewer.HorizontalOffset > scrollViewer.ScrollableWidth - ScrollBackWidth)
                scrollViewer.ChangeView(scrollViewer.ScrollableWidth - ScrollBackWidth, null, null);
            
            scrollViewer.ReleasePointerCaptures();
        }

        private void scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            //double _centerScrollBackWidth = ((Zoom - 1) * ActualWidth + 100 * Zoom) * 0.5;
            ScrollBackWidth = ((Zoom - 1) * ActualWidth + 100 * Zoom) * 0.5;
            ScrollBackHeight = ((Zoom - 1) * ActualHeight + 100 * Zoom) * 0.5;
            Debug.Print("" + ScrollBackWidth);
            scrollViewer.ChangeView(ScrollBackWidth, ScrollBackHeight, Zoom, true);
        }

        private async void image_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollBackWidth = Math.Min((ActualWidth + 100 - image.ActualWidth) * Zoom / 2 - 50, ScrollBackWidth);
            ScrollBackHeight = Math.Min((ActualHeight - image.ActualHeight) * Zoom / 2 - 50, ScrollBackHeight);

            await Task.Delay(1);
            imageAnimation?.TryStart(image);


        }

    }






}
