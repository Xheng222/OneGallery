using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
using Windows.UI.ApplicationSettings;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{
    public class Category
    {
        public string Name { get; set; }

        public FontIcon Icon = new();

        public ObservableCollection<Category> Children { get; set; }
        public bool IsLeaf { get; set; }

        public Category() 
        {
            Icon.Glyph = "\uE80F";
        }
    }

    public sealed partial class MainWindow : Window
    {
        public string appTitleText = "OneGallery";

        private bool IsPaneOpened = true;

        Stack<string> PartenPagemName = new();

        Dictionary<string, string> ParentDictionary = new Dictionary<string, string>();

        Dictionary<string, NavigationViewItem> PageDictionary = new Dictionary<string, NavigationViewItem>();

        Dictionary<string, Category> NvItemDictionary = new Dictionary<string, Category>();

        public ObservableCollection<Category> Categories = new()
        {
            new Category() {
            Name = "HomePage",
                Children = new ObservableCollection<Category>() {
                    new Category(){
                        Name = "Menu item 2",
                        //CategoryIcon = "Home",
                        Children = new ObservableCollection<Category>() {
                            new Category() {
                                Name  = "Menu item 3",
                                //CategoryIcon = "Home",
                                Children = new ObservableCollection<Category>() {
                                    new Category()
                                    {
                                        Name  = "Menu item 4", 
                                        //CategoryIcon = "Home", 
                                        IsLeaf = true
                                    },
                                    new Category()
                                    {
                                        Name = "SamplePage2"
                                    }
    }
                            }
                        }
                    }
                }
            },
            new Category(){
                Name = "Menu item 6",
                //CategoryIcon = "Home",
                Children = new ObservableCollection<Category>() {
                    new Category(){
                        Name = "Menu item 7",
                        //CategoryIcon = "Home",
                        Children = new ObservableCollection<Category>() {
                            new Category() {
                                Name  = "Menu item 8", 
                                //CategoryIcon = "Home", 
                                IsLeaf = true
                            },
                            new Category() {
                                Name  = "Menu item 9", 
                                //CategoryIcon = "Home", 
                                IsLeaf = true
                            }
                        }
                    }
                }
            },
            new Category(){
                Name = "Menu item 10", 
                //CategoryIcon = "Home", 
                IsLeaf = true }
        };


        public MainWindow()
        {
            this.InitializeComponent();

            this.ExtendsContentIntoTitleBar = true;

            this.TrySetAcrylicBackdrop();

            this.SetTitleBar(AppTitleBar);

            appTitleText = "666";
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

        private void NavView_Navigate(Type navPageType)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = Nv_page.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null && !Equals(preNavPageType, navPageType))
            {
                Nv_page.Navigate(navPageType, "H:\\1234");
                
            }
        }

        private void NavView_ItemInvoked(NavigationView sender,
                                 NavigationViewItemInvokedEventArgs args)
        {
            Type CurrentPage = Nv_page.CurrentSourcePageType;
            if (CurrentPage != null)
            {
                if (args.IsSettingsInvoked == true)
                {
                    if (!string.Equals(CurrentPage.Name, "Settings"))
                    {
                        Debug.Print("history " + CurrentPage.Name);
                        NavView_Navigate(typeof(Settings));
                    }
                }
                else if (args.InvokedItemContainer != null)
                {
                    string ClickedPageName = args.InvokedItemContainer.Tag.ToString();

                    if (!string.Equals(CurrentPage.Name, ClickedPageName))
                    {
                        Debug.Print("history " + CurrentPage.Name);
                        string pageName = "OneGallery." + ClickedPageName;
                        Type navPageType = Type.GetType(pageName);
                        NavView_Navigate(navPageType);
                    }   
                
                }

            }

        }

        private void NavView_SelectionChanged(NavigationView sender,
                                      NavigationViewSelectionChangedEventArgs args)
        {
            //string Page = args.SelectedItemContainer.Tag.ToString();
            ////Debug.Print("Select " + Page);

            //if (args.IsSettingsSelected == true)
            //{
            //    NavView_Navigate(typeof(Settings));
            //}
            //else if (args.SelectedItemContainer != null)
            //{
            //    Category s = (Category)Nv.SelectedItem; 
            //    if (s != null)
            //    {
            //        Debug.Print("Select " + s.Name);
            //    }
            //    string pageName = "OneGallery." + Page;
            //        Type navPageType = Type.GetType(pageName);
            //        NavView_Navigate(navPageType);
                
            //}
        }

        private void Nv_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            Nv.IsBackEnabled = false;

            if (Nv_page.CanGoBack)
            {
                Nv_page.GoBack();
            }

            //if (!PageDictionary.ContainsKey(History))
            //{
            //    var rootGrid = VisualTreeHelper.GetChild(Nv, 0);
            //    FindNaView(rootGrid);
            //}

            //var rootGrid = VisualTreeHelper.GetChild(sender as NavigationView, 0);
            //FindNaView(rootGrid);

            //if (History.Equals("Settings"))
            //{
            //    Debug.Print("Back to Settings");
            //    NavView_Navigate(typeof(Settings));  
            //}
            //else 
            //{
            //    Debug.Print("Back to " + History);

            //    string pageName = "OneGallery." + History;
            //    Type navPageType = Type.GetType(pageName);
            //    NavView_Navigate(navPageType);
            //}

            //if (ParentDictionary[History] is not null)
            //{
            //    ExpandParentPage(ParentDictionary[History]);
            //}

            //if (History.Equals("Settings"))
            //{
            //    Nv.SelectedItem = PageDictionary[History];
            //}
            //else
            //{
            //    Nv.SelectedItem = NvItemDictionary[History];
            //}

        }

        private async void Nv_Page_Navigated(object sender, NavigationEventArgs e)
        {

            string CurrentPageName = Nv_page.CurrentSourcePageType.Name;

            if (!PageDictionary.ContainsKey(CurrentPageName))
            {
                var rootGrid = VisualTreeHelper.GetChild(Nv, 0);
                FindNaView(rootGrid);
            }

            if (ParentDictionary[CurrentPageName] is not null)
            {
                ExpandParentPage(ParentDictionary[CurrentPageName]);
            }

            if (CurrentPageName.Equals("Settings"))
            {
                Nv.SelectedItem = PageDictionary["Settings"];
            }
            else
            {
                if (IsPaneOpened)
                {
                    Nv.SelectedItem = NvItemDictionary[CurrentPageName];
                }
                else
                {
                    SelectPage(CurrentPageName);
                    
                }
                    
            }

            if (Nv_page.CanGoBack)
            {
                await Task.Delay(250);
                Nv.IsBackEnabled = true;
            }

        }

        private async void SelectPage(string PageName)
        {

            var TypeName = Nv.SelectedItem.GetType().Name;
            string SelectName;

            if (TypeName.Equals("Category"))
            {
                SelectName = ((Category)Nv.SelectedItem).Name;
            }
            else
            {
                SelectName = (string)((NavigationViewItem)Nv.SelectedItem).Content;
            }

            if (SelectName.Equals(PageName))
            {
                return;
            }
            else
            {
                string ParentPageName = PageName;
                while (ParentDictionary[ParentPageName] != null)
                {
                    ParentPageName = ParentDictionary[ParentPageName];
                }

                Nv.SelectedItem = NvItemDictionary[ParentPageName];
                await Task.Delay(50);
                Nv.SelectedItem = NvItemDictionary[PageName];

            }

        }

        private void FindNaView(DependencyObject Item)
        {
            var TypeName = Item.GetType().Name;
            var ChildNum = VisualTreeHelper.GetChildrenCount(Item);
            if (ChildNum > 0)
            {
                if (TypeName.Equals("NavigationViewItem"))
                {
                    var NaView = (NavigationViewItem)Item;
                    var PageName = NaView.Tag.ToString();
                    
                    if (!PageDictionary.ContainsKey(PageName))
                    {
                        PageDictionary.Add(PageName, NaView);
                    }
                    //else
                    //{
                    //    PageDictionary.Remove(PageName);
                    //    PageDictionary.Add(PageName, NaView);
                    //}

                    if (!ParentDictionary.ContainsKey(PageName))
                    {
                        if (PartenPagemName.Count != 0)
                        {
                            ParentDictionary.Add(PageName, PartenPagemName.Peek());
                        }
                        else
                        {
                            ParentDictionary.Add(PageName, null);
                        }
                        
                    }

                    PartenPagemName.Push(PageName);

                    for (var i = 0; i < ChildNum; i++)
                    {
                        FindNaView(VisualTreeHelper.GetChild(Item, i));
                    }

                    PartenPagemName.Pop();
                }

                else
                {
                    for (var i = 0; i < ChildNum; i++)
                    {
                        FindNaView(VisualTreeHelper.GetChild(Item, i));
                    }
                }

            }

            return;
        }

        private void ExpandParentPage(string PageName)
        {
            if (ParentDictionary[PageName] != null)
            {
                ExpandParentPage(ParentDictionary[PageName]);
            }

            Nv.Expand(PageDictionary[PageName]);

            return;
        }

        private void CollapseParentPage(string PageName)
        {
            Nv.Collapse(PageDictionary[PageName]);            
            if (ParentDictionary[PageName] != null)
            {
                CollapseParentPage(ParentDictionary[PageName]);
            }
            return;
        }

        private void Nv_Loaded(object sender, RoutedEventArgs e)
        {
            var rootGrid = VisualTreeHelper.GetChild(sender as NavigationView, 0);
            FindNaView(rootGrid);

            UpdateNvItemDir(Categories);

            NavView_Navigate(typeof(HomePage));
            Nv.SelectedItem = NvItemDictionary["HomePage"];
        }

        private void UpdateNvItemDir(ObservableCollection<Category> Items)
        {
            if (Items != null)
            {
                foreach (var Item in Items)
                {
                    //Debug.Print(Item.Name);
                    NvItemDictionary.Add(Item.Name, Item);
                    UpdateNvItemDir(Item.Children);
                }

            }
            return;
        }

        private void Nv_PaneClosed(NavigationView sender, object args)
        {
            IsPaneOpened = false;
        }

        private void Nv_PaneOpened(NavigationView sender, object args)
        {
            IsPaneOpened = true;

        }

    }
}
