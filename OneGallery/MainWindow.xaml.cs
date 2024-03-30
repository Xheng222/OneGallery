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
using Microsoft.UI.Composition.SystemBackdrops;
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
using Windows.UI;
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

        public string PageType { get; set; }

        public int[] PageSource { get; set; }

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
            Name = "HomePage",
            PageType = "HomePage",
            PageSource = new int[] {-1},
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
                PageSource = new int[] {1},
            }
        };

        private string SelectPageName {  get; set; }

        public Frame page
        {
            get { return Nv_page; }
            set { Nv_page = value; }
        } 

        public NavigationView NaView
        {
            get { return Nv; }
            set { Nv = value; }
        }

        public MainWindow()
        {
            this.InitializeComponent();

            this.ExtendsContentIntoTitleBar = true;

            this.TrySetAcrylicBackdrop();

            this.SetTitleBar(AppTitleBar);

            appTitleText = "666";

            FolderManager = new();
        }

        //public async Task InitFolder()
        //{
        //    int[] _temp = { -1 };
        //    await FolderManager.Init(_temp);
        //}

        public async Task InitFolder(string _pageName)
        {
            await FolderManager.InitPageFolder(_pageName);
        }

        SystemBackdropConfiguration m_configurationSource;
        DesktopAcrylicController m_backdropController;

        bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
               
                m_configurationSource = new SystemBackdropConfiguration();
                m_configurationSource.IsInputActive = true;
                m_configurationSource.Theme = SystemBackdropTheme.Default;

                m_backdropController = new DesktopAcrylicController();
                m_backdropController.Kind = DesktopAcrylicKind.Default;
                m_backdropController.TintColor = Color.FromArgb(255, 243, 246, 247);
                m_backdropController.TintOpacity = 0.5f;
                m_backdropController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);

                this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();

                this.Closed += Window_Closed;

                return true;
            }

            return false; // Acrylic is not supported on this system
        }

        private void NavView_Navigate(Type navPageType, Category page)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null)
            {
                Nv_page.Navigate(navPageType, page, new EntranceNavigationTransitionInfo());
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
                        HistoryPages.Push(SelectPageName);
                        SelectPageName = "Settings";
                        NavView_Navigate(typeof(Settings), null);
                    }
                }
                else if (args.InvokedItemContainer != null)
                {
                    string ClickedItemTag = args.InvokedItemContainer.Tag.ToString();
                    var PageCategory = Nv.SelectedItem as Category;

                    if (!string.Equals(PageCategory.Name, SelectPageName))
                    {
                        HistoryPages.Push(SelectPageName);
                        SelectPageName = PageCategory.Name;
                        Type navPageType = Type.GetType("OneGallery." + PageCategory.PageType);
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

        public async void Nv_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            Nv.IsBackEnabled = false;

            if (Nv_page.CanGoBack)
            {
                if (Nv_page.CurrentSourcePageType == typeof(ImagePage))
                {
                    await ImagePage.NowImagePage.ResetAll();

                    //if (ImagePage.NowImagePage.ScrollViewerRotation % 360 != 0)
                    //{
                    //    var _temp = (int)(ImagePage.NowImagePage.ScrollViewerRotation);
                    //    if (_temp < 0)
                    //        _temp -= 180;
                    //    else
                    //        _temp += 180;
                    //    _temp = (_temp / 360) * 360; 
                    //    ImagePage.NowImagePage.ScrollViewerRotation = _temp;
                    //await Task.Delay(300);
                    //}


                }
                
                Nv_page.GoBack();
            }

            
        }

        private async void Nv_Page_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                SelectPageName = HistoryPages.Pop();
                if (!PageDictionary.ContainsKey(SelectPageName))
                {
                    var rootGrid = VisualTreeHelper.GetChild(Nv, 0);
                    FindNaView(rootGrid);
                }

                if (SelectPageName.Equals("Settings"))
                {
                    Nv.SelectedItem = PageDictionary["Settings"];
                }
                else
                {
                    if (ParentDictionary[SelectPageName] is not null)
                    {
                        ExpandParentPage(ParentDictionary[SelectPageName]);
                    }

                    if (IsPaneOpened)
                    {
                        Nv.SelectedItem = NvItemDictionary[SelectPageName];
                    }
                    else
                    {
                        SelectPage(SelectPageName);

                    }

                }
            }
            else if (Nv_page.CurrentSourcePageType == typeof(ImagePage))
            {
                HistoryPages.Push(SelectPageName);
            }

            if (Nv_page.CanGoBack)
            {
                await Task.Delay(250);
                Nv.IsBackEnabled = true;
            }

        }

        private async void SelectPage(string PageName)
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
            FolderManager.InitFolder();
            var rootGrid = VisualTreeHelper.GetChild(sender as NavigationView, 0);
            FindNaView(rootGrid);
            UpdateNvItemDir(Categories);
            SelectPageName = "HomePage";
            Nv.SelectedItem = NvItemDictionary["HomePage"];
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

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed
            if (m_backdropController != null)
            {
                m_backdropController.Dispose();
                m_backdropController = null;
            }
            m_configurationSource = null;
        }

    }
}
