using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.VisualBasic;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.StartScreen;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{

    public sealed partial class HomePage : Page
    {
        private LocalFolder HomePageLocalFolder {  get; set; }

        private ImageArrangement HomePageImageArrangement {  get; set; }

        public HomePage()
        {
            this.InitializeComponent();       

            string path = "H:\\Sync_images";
            HomePageLocalFolder = new(path);
            HomePageImageArrangement = HomePageLocalFolder.MyImageArrangement;
            this.Loaded += Init;
        }

        private async void Init(object sender, RoutedEventArgs e)
        {
            await HomePageLocalFolder.Init();
            
            repeater2.ItemsSource = HomePageLocalFolder.ImgList;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Debug.Print("Page_Size " + this.ActualWidth.ToString());
            //SwappableLayoutsItemsView.Height = this.ActualHeight;
            ScrollViewer.Height = this.ActualHeight;
        }




        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image _image = sender as Image;
            var _opacity = _image.Opacity;


        }

        private async void ActivityFeedLayout_MyEvent(object sender, EventArgs e)
        {
            //OpacityOut.Begin();
            //await Task.Delay(300);
            //OpacityIn.Begin();
            //repeater2.Opacity = 1;
        }
    }
}
