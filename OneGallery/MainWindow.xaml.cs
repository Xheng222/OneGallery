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
        readonly string appTitleText = "OneGallery";

        private Stack<string> HistoryPage = new();

        Stack<string> PartenPagemName = new();

        Dictionary<string, string> ParentDictionary = new Dictionary<string, string>();

        Dictionary<string, NavigationViewItem> PageDictionary = new Dictionary<string, NavigationViewItem>();

        Dictionary<string, Category> NvItemDictionary = new Dictionary<string, Category>();

        public MainWindow()
        {
            this.InitializeComponent();

            this.ExtendsContentIntoTitleBar = true;

            this.TrySetAcrylicBackdrop();

            this.SetTitleBar(AppTitleBar);


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
                Nv_page.Navigate(navPageType, null);

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
                        HistoryPage.Push(CurrentPage.Name);
                    }
                }
                else if (args.InvokedItemContainer != null)
                {
                    string ClickedPageName = args.InvokedItemContainer.Tag.ToString();


                    if (!string.Equals(CurrentPage.Name, ClickedPageName))
                    {
                            Debug.Print("history " + CurrentPage.Name);
                            HistoryPage.Push(CurrentPage.Name);

                    }   
                
                }

            }

        }

        private void NavView_SelectionChanged(NavigationView sender,
                                      NavigationViewSelectionChangedEventArgs args)
        {
            string Page = args.SelectedItemContainer.Tag.ToString();
            //Debug.Print("Select " + Page);

            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate(typeof(Settings));
            }
            else if (args.SelectedItemContainer != null)
            {
                Category s = (Category)Nv.SelectedItem; 
                if (s != null)
                {
                    Debug.Print("Select " + s.Name);
                }
                string pageName = "OneGallery." + Page;
                    Type navPageType = Type.GetType(pageName);
                    NavView_Navigate(navPageType);
                
            }
        }

        private void Nv_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            Nv.IsBackEnabled = false;
            var History = HistoryPage.Pop();

            //if (!PageDictionary.ContainsKey(History))
            //{
            //    var rootGrid = VisualTreeHelper.GetChild(Nv, 0);
            //    FindNaView(rootGrid);
            //}

            var rootGrid = VisualTreeHelper.GetChild(Nv, 0);
            FindNaView(rootGrid);
            foreach (var item in HistoryPage)
            {
                Debug.Print("BackSt " + item);
            }

            if (ParentDictionary[History] is not null)
            {
                ExpandParentPage(ParentDictionary[History]);
            }

            if (History.Equals("Settings"))
            {
                Nv.SelectedItem = PageDictionary[History];
            }
            else
            {
                Nv.SelectedItem = NvItemDictionary[History];
            }

            //if (ParentDictionary[History] is not null)
            //{
            //    CollapseParentPage(ParentDictionary[History]);
            //}

            //Nv.UpdateLayout();
        }

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

        private void Nv_Page_Navigated(object sender, NavigationEventArgs e)
        {
            if (HistoryPage.Count > 1)
            {
                Nv.IsBackEnabled = true;
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

            var rootGrid = VisualTreeHelper.GetChild(Grid, 0);
            FindNaView(rootGrid);

            UpdateNvItemDir(Categories);

            Nv.SelectedItem = NvItemDictionary["HomePage"];
            HistoryPage.Push("HomePage");
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
    }
}
