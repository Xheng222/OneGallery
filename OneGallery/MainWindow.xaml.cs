using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
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

        public string PageType { get; set; }

        public string PageSource { get; set; }

        public Category() 
        {
            Icon.Glyph = "\uE80F";
        }
    }

    internal sealed partial class MainWindow : Window
    {
        public string appTitleText = "OneGallery";

        public Stack<string> HistoryPages = new();

        private bool IsPaneOpened = true;

        Stack<string> PartenPagemName = new();

        // expand ”√
        Dictionary<string, string> ParentDictionary = new Dictionary<string, string>();

        // expand ”√
        Dictionary<string, NavigationViewItem> PageDictionary = new Dictionary<string, NavigationViewItem>();

        // select ”√
        Dictionary<string, Category> NvItemDictionary = new Dictionary<string, Category>();

        // Folder
        public LocalFolder FolderManager { get; set; }

        public ObservableCollection<Category> Categories = new()
        {
            new Category() {
            Name = "HomePage1",
            PageType = "HomePage",
            PageSource = "H:\\1234",
                Children = new ObservableCollection<Category>() {
                    new Category(){
                        PageType = "HomePage",
                        Name = "Menu item 2",               
                    }
                }
            },
            new Category(){
                Name = "Menu item 6",
                PageType = "HomePage",
                Children = new ObservableCollection<Category>() {
                    new Category(){
                        Name = "Menu item 7",
                        PageType = "HomePage"
                    }
                }
            },
            new Category(){
                Name = "Menu item 10",
                PageType = "HomePage",
                PageSource = "H:\\Sync_images\\Phone_picture\\QQ",
                IsLeaf = true }
        };

        public Frame page
        {
            get { return Nv_page; }
            set { Nv_page = value; }
        } 

        public MainWindow()
        {
            this.InitializeComponent();

            this.ExtendsContentIntoTitleBar = true;

            this.TrySetAcrylicBackdrop();

            this.SetTitleBar(AppTitleBar);

            appTitleText = "666";

            FolderManager = new("H:\\1234");
        }

        public async Task InitFolder()
        {
            await FolderManager.Init();
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

        private void NavView_Navigate(Type navPageType, Category page)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = Nv_page.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null)
            {
                Nv_page.Navigate(navPageType, page, new DrillInNavigationTransitionInfo());
                
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
                        HistoryPages.Push("Settings");
                        NavView_Navigate(typeof(Settings), null);
                    }
                }
                else if (args.InvokedItemContainer != null)
                {
                    string ClickedPageName = args.InvokedItemContainer.Tag.ToString();
                    var PageCategory = NvItemDictionary[ClickedPageName];

                    if (!string.Equals(CurrentPage.Name, ClickedPageName))
                    {
                        Debug.Print("history " + PageCategory.Name);
                        HistoryPages.Push(PageCategory.Name);
                        string pageName = "OneGallery." + PageCategory.PageType;
                        Type navPageType = Type.GetType(pageName);
                        NavView_Navigate(navPageType, PageCategory);
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
            //HistoryPages.Pop();
            //if (HistoryPages.Count > 1)
            //{
            //    HistoryPages.Pop();
            //    var NvItemName = HistoryPages.Peek();
            //    if (NvItemName == "Settings")
            //    {
            //        NavView_Navigate(typeof(Settings), null);
            //    }
            //    else
            //    {
            //        var PageCategory = NvItemDictionary[NvItemName];
            //        string pageName = "OneGallery." + PageCategory.PageType;
            //        Type navPageType = Type.GetType(pageName);
            //        NavView_Navigate(navPageType, PageCategory);
            //    }

            //}

            if (Nv_page.CanGoBack)
            {
                Nv_page.GoBack();
            }

            
        }

        private async void Nv_Page_Navigated(object sender, NavigationEventArgs e)
        {

            string CurrentPageName = HistoryPages.Peek();


            if (!CurrentPageName.Equals("ImagePage"))
            {
                if (!PageDictionary.ContainsKey(CurrentPageName))
                {
                    var rootGrid = VisualTreeHelper.GetChild(Nv, 0);
                    FindNaView(rootGrid);
                }

                if (CurrentPageName.Equals("Settings"))
                {
                    Nv.SelectedItem = PageDictionary["Settings"];
                }
                else
                {
                    if (ParentDictionary[CurrentPageName] is not null)
                    {
                        ExpandParentPage(ParentDictionary[CurrentPageName]);
                    }

                    if (IsPaneOpened)
                    {
                        Nv.SelectedItem = NvItemDictionary[CurrentPageName];
                        Debug.Print(NvItemDictionary[CurrentPageName].Name);
                    }
                    else
                    {
                        SelectPage(CurrentPageName);
                    
                    }
                    
                }
            }



            //if (HistoryPages.Count > 1)
            //{
            //    await Task.Delay(250);
            //    Nv.IsBackEnabled = true;
            //}

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
            HistoryPages.Push("HomePage1");
            Nv.SelectedItem = NvItemDictionary["HomePage1"];
            NavView_Navigate(typeof(HomePage), Categories[0]);


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
