using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{

    public sealed partial class MainWindow : Window
    {
        TreeViewNode personalFolder;

        readonly String appTitleText = "OneGallery";
        Stack view_history = new();

        public MainWindow()
        {
            this.InitializeComponent();



            this.ExtendsContentIntoTitleBar = true;

            this.TrySetAcrylicBackdrop();

            this.SetTitleBar(AppTitleBar);

            Nv.SelectedItem = Nv.MenuItems[0];
            NavView_Navigate(typeof(HomePage), new EntranceNavigationTransitionInfo());
            // this.InitializeSampleTreeView();


        }
        bool TrySetAcrylicBackdrop()
        {
            if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
            {
                this.SystemBackdrop = null;
                this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();

                return true;
            }

            return false; // Acrylic is not supported on this system
        }

        private void NavView_Navigate(
            Type navPageType,
            NavigationTransitionInfo transitionInfo)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = Nv_page.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null && !Type.Equals(preNavPageType, navPageType))
            {
                Nv_page.Navigate(navPageType, null, transitionInfo);
            }
        }

        private bool Nv_ChangePage(String page)
        {
            Debug.Print(page + "\n");
            if (page == "HomePage" || page == "SettingPage" || page == "SamplePage2")
            {
                string pageName = "OneGallery." + page;
                Type pageType = Type.GetType(pageName);
                Nv_page.Navigate(pageType);
                view_history.Push(page);
                return true;
            }
            
            return false;
        }

        private void Nv_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            /* NOTE: for this function to work, every NavigationView must follow the same naming convention: nvSample# (i.e. nvSample3),
            and every corresponding content frame must follow the same naming convention: contentFrame# (i.e. contentFrame3) */

            // Get the sample number
            string ChoosePage = sender.Name;
            Debug.Print(ChoosePage + "\n");

            if (args.IsSettingsSelected)
            {
                Nv_ChangePage("SettingPage");
                //Nv_page.Navigate(typeof(SettingPage));
            }
            else
            {
                var selectedItem = (NavigationViewItem)args.SelectedItem;
                string page = ((string)selectedItem.Tag);
                Nv_ChangePage(page);

                //    var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
                //    string selectedItemTag = ((string)selectedItem.Tag);
                //    sender.Header = "Sample Page " + selectedItemTag.Substring(selectedItemTag.Length - 1);
                //    string pageName = "WinUIGallery.SamplePages." + selectedItemTag;
                //    Type pageType = Type.GetType(pageName);
                //    contentFrame8.Navigate(pageType);
            }
        }

        private void Nv_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            //if (view_history.Count <= 1)
            //{
            //    return;
            //}

            //string last_page = (string)view_history.Pop();

            //Nv_ChangePage(last_page);
            //Frame rootFrame = Current.Content as Frame;

            if (Nv_page.CanGoBack)
            {
                Nv_page.GoBack();

            }


        }





    }
}
