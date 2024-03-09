using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.StartScreen;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{

    public class CustomDataObject
    {
        private string _imageLocation;
        public string Title { get; set; }
        public string ImageLocation
        {
            get
            {
                //Task.Delay(975).Wait();
                return _imageLocation;
            }
            set { _imageLocation = value; }
        }
        public string Views { get; set; }
        public string Likes { get; set; }
        public string Description { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public int phase = 0;

        public CustomDataObject(string Location) 
        {
            ImageLocation = Location;
            InitImage();
        }

        private void InitImage()
        {
            Title = "123";
            Views = "456";
            Likes = "789";
            using FileStream fs = new(ImageLocation, FileMode.Open, FileAccess.Read);
            System.Drawing.Image image = System.Drawing.Image.FromStream(fs);
            Height = image.Height;
            Width = image.Width;

            Debug.Print(Width.ToString());
        }
    }

    public sealed partial class HomePage : Page
    {
        ObservableCollection<CustomDataObject> TempList { get; set; }

        private ImageArrangement MyImageArrangement {  get; set; }

        public HomePage()
        {
            this.InitializeComponent();
            
            CustomDataObject[] temp1 = {
                new("H:\\123.png"),
                new("H:\\456.jpg"),
                new("H:\\789.jpg"),
                new("H:\\9.jpg")
            };


            ObservableCollection<CustomDataObject> Items = new();

            Random rd = new Random(114);

            for (int i = 0; i < 500; i++)
            {
                int a = rd.Next(0, 4);
                Items.Add(temp1[a]);
            }

            MyImageArrangement = new(Items);
            MyImageArrangement.SetImgSize(new Size[]{new(500, 100), new(500, 175), new(500, 200) }, 
                new Size(500, 250), 
                new double[] { 400, 980, 1200}, 
                8, 8);
            this.Loaded += LoadedImage;
            

        }

        private void LoadedImage(object sender, RoutedEventArgs e)
        {

            repeater2.ItemsSource = MyImageArrangement.ImgList;
            //SwappableLayoutsItemsView.Height = this.Height;
            //SwappableLayoutsItemsView.PointerWheelChanged += MyPointerWheelChanged;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Debug.Print("Page_Size " + this.ActualWidth.ToString());
            //SwappableLayoutsItemsView.Height = this.ActualHeight;
            ScrollViewer.Height = this.ActualHeight;
        }




        private async void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image _image = sender as Image;
            var _opacity = _image.Opacity;
            //Debug.Print(_image.Opacity.ToString());
            var myStoryboard1 = _image.Resources["myStoryboard1"] as Storyboard;
            var myStoryboard2 = _image.Resources["myStoryboard2"] as Storyboard;
            if (Math.Abs((e.PreviousSize.Width / e.NewSize.Width) - 1) > 0.2)
            {
                if (_opacity == 1)
                {
                    myStoryboard2.Begin();
                    await Task.Delay(200);

                    myStoryboard1.Begin();
                    await Task.Delay(400);
                    _image.Opacity = 1;
                }

            }
            
            //else
            //{
            //    if (_opacity == 0)
            //    {
            //        myStoryboard1.Begin();
            //    }
            //}
            
            


            //if (i == null)
            //{
            //    Debug.Print("ggg");
            //}
            //else
            //{
            //    Debug.Print("winwinwin");
            //}

            //Debug.Print("_image: " + _image.Name);
            //myStoryboard.SetValue(Storyboard.TargetNameProperty, sender);
            //myStoryboard2.Begin();

        }
    }
}
