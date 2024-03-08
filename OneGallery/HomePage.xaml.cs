using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
        public string Title { get; set; }
        public string ImageLocation { get; set; }
        public string Views { get; set; }
        public string Likes { get; set; }
        public string Description { get; set; }
    }

    public sealed partial class HomePage : Page
    {

        ObservableCollection<CustomDataObject> tempList;


        public HomePage()
        {
            this.InitializeComponent();
            this.Loaded += LoadedImage;
        }

        private void LoadedImage(object sender, RoutedEventArgs e)
        {
            CustomDataObject temp = new()
            {
                Title = "123",
                ImageLocation = "H:\\123.png",
                Views = "0",
                Likes = "0",
                Description = "011",
            };

            ObservableCollection<CustomDataObject> Items = new()
            {
                temp,
                temp,
                temp,
                temp,
                temp
            };

            tempList = Items;

            SwappableLayoutsItemsView.ItemsSource = tempList;
            SwappableLayoutsItemsView.Height = this.Height;
        }


    }
}
