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
using Windows.UI.ViewManagement;
using WinRT;
using WinUIEx;
using WinUIEx.Messaging;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OneGallery
{
    public class Category
    {
        public string Name { get; set; }

        public FontIcon Icon = new();

        public ObservableCollection<object> Children { get; set; }

        public string PageType { get; set; }

        public int[] PageSource { get; set; }

        public Category() 
        {
            Icon.Glyph = "\uE80F";
        }
    }

    public partial class MainWindow : WindowEx
    {
        public string appTitleText = "OneGallery";

        public Stack<string> HistoryPages = new();

        Stack<string> PartenPagemName = new();

        public SettingsConfig MySettingsConfig { get; set; }

        // expand ”√
        Dictionary<string, string> ParentDictionary = new Dictionary<string, string>();

        // expand ”√
        Dictionary<string, NavigationViewItem> PageDictionary = new Dictionary<string, NavigationViewItem>();

        // select ”√
        Dictionary<string, Category> NvItemDictionary = new Dictionary<string, Category>();

        // Folder
        public LocalFolderManager FolderManager { get; set; }

        public ObservableCollection<object> Categories = new()
        {
            new NavigationViewItemSeparator(),
            new Category() {
            Name = "HomePage",
            PageType = "ImageListPage",
            PageSource = new int[] {-1},
                Children = new ObservableCollection<object>() {
                    new Category(){
                        PageType = "ImageListPage",
                        Name = "Menu item 2",               
                    }
                }
            },
            new NavigationViewItemSeparator(),
            new Category(){
                Name = "Menu item 6",
                PageType = "ImageListPage",
                Children = new ObservableCollection<object>() {
                    new Category(){
                        Name = "Menu item 7",
                        PageType = "ImageListPage"
                    }
                }
            },
            new NavigationViewItemSeparator(),
            new Category(){
                Name = "Menu item 10",
                PageType = "ImageListPage",
                PageSource = new int[] {1},
            }
        };

        private string SelectPageName {  get; set; }

        public Frame NaPage
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
            ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            appTitleText = "666";
            FolderManager = new();
            this.TrySetAcrylicBackdrop();
            //var gcTimer = new DispatcherTimer();
            //gcTimer.Tick += (sender, e) => { Myfind(); };
            //gcTimer.Interval = TimeSpan.FromSeconds(5);
            //gcTimer.Start();
        }

        public async Task InitFolder(string _pageName)
        {
            await FolderManager.InitPageFolder(_pageName);
        }

        SystemBackdropConfiguration m_configurationSource;
        DesktopAcrylicController m_backdropController;

        private async void TrySetAcrylicBackdrop()
        {
            //this.AppWindow.Resize(new(2084, 1214));
            //Debug.Print(AppWindow.Presenter.Kind + "");
            await FolderManager.InitSettings();
            MySettingsConfig = FolderManager.MySettingsConfig;
            this.SetWindowSize(MySettingsConfig.LastWidth, MySettingsConfig.LastHeight);

            if (DesktopAcrylicController.IsSupported())
            {          
                m_configurationSource = new SystemBackdropConfiguration();
                m_configurationSource.IsInputActive = true;
                m_configurationSource.Theme = SystemBackdropTheme.Default;

                m_backdropController = new DesktopAcrylicController();
                m_backdropController.Kind = DesktopAcrylicKind.Default;

                m_backdropController.TintColor = Color.FromArgb(255, 255, 255, 255);
                m_backdropController.TintOpacity = 0.5f;
                m_backdropController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);

                this.SystemBackdrop = new DesktopAcrylicBackdrop();

            }
            
            this.Closed += Window_Closed;
            
        }

        private void NavView_Navigate(Type navPageType, Category page)
        {
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
                    //if (!string.Equals(CurrentPage.Name, "Settings"))
                    //{
                        HistoryPages.Push(SelectPageName);
                        SelectPageName = "Settings";
                        NavView_Navigate(typeof(SettingPage), null);
                    //}
                }
                else if (args.InvokedItemContainer != null)
                {
                    var PageCategory = Nv.SelectedItem as Category;

                    //if (!string.Equals(PageCategory.Name, SelectPageName))
                    //{
                        HistoryPages.Push(SelectPageName);
                        SelectPageName = PageCategory.Name;
                        Type navPageType = Type.GetType("OneGallery." + PageCategory.PageType);
                        NavView_Navigate(navPageType, PageCategory);
                    //}   
                
                }

            }

        }

        public async void Nv_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            Nv.IsBackEnabled = false;

            if (Nv_page.CanGoBack)
            {
                if (Nv_page.CurrentSourcePageType == typeof(ImagePage))
                {
                    await ImagePage.NowImagePage.ResetAll();
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
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            ExpandParentPage(ParentDictionary[SelectPageName]);
                        });
                    }

                    if (Nv.IsPaneOpen)
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
            Nv_page.CacheSize = 0;

            FolderManager.InitFolder();
            var rootGrid = VisualTreeHelper.GetChild(sender as NavigationView, 0);
            FindNaView(rootGrid);
            UpdateNvItemDir(Categories);
            SelectPageName = "HomePage";
            Nv.SelectedItem = NvItemDictionary["HomePage"];
            NavView_Navigate(typeof(ImageListPage), (Category)Categories[1]);
        }

        private void UpdateNvItemDir(ObservableCollection<object> Items)
        {

            if (Items != null)
            {
                foreach (var _item in Items)
                {
                    //Debug.Print(Item.Name);
                    if (_item is Category)
                    {
                        NvItemDictionary.Add((_item as Category).Name, (Category)_item);
                        UpdateNvItemDir((_item as Category).Children);
                    }

                }

            }
            return;
        }

        private void Nv_PaneClosing(NavigationView sender, object args)
        {
            Nv_page.Width = Nv.ActualWidth - Nv.CompactPaneLength + 8;
        }

        private void Nv_PaneOpening(NavigationView sender, object args)
        {
            Nv_page.Width = Nv.ActualWidth - Nv.OpenPaneLength + 8;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed
            if (m_backdropController != null)
            {
                m_backdropController.Dispose();
                m_backdropController = null;
                m_configurationSource = null;
            }


            FolderManager.SaveConfig((int)Width, (int)Height);
        }

        int kk = 0;

        private void FindItem(DependencyObject Item)
        {
            var TypeName = Item.GetType().Name;
            var ChildNum = VisualTreeHelper.GetChildrenCount(Item);
            if (ChildNum > 0)
            {
                Debug.Print(TypeName);
                if (TypeName.Equals("ItemsRepeater"))
                {
                    kk++;
                }


                else
                {
                    for (var i = 0; i < ChildNum; i++)
                    {
                        FindItem(VisualTreeHelper.GetChild(Item, i));
                    }
                }

            }

            return;
        }

        private void Nv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Nv.IsPaneOpen)
            {

                Nv_page.Width = Nv.ActualWidth - Nv.OpenPaneLength + 8;
            }
            else
            {
                Nv_page.Width = Nv.ActualWidth - Nv.CompactPaneLength + 8;
            }

            Nv_page.Height = Nv.ActualHeight - 40;

            if (Nv.ActualWidth < 700)
            {
                if (Nv.IsPaneOpen)
                    Nv.IsPaneOpen = false;
            }
            else if (!Nv.IsPaneOpen)
                Nv.IsPaneOpen = true;
        }




}

    internal class MyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }

        public DataTemplate SeparatorTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            // Return the correct data template based on the item's type.
            if (item.GetType() == typeof(Category))
            {
                return ItemTemplate;
            } 
            else if (item.GetType() == typeof(NavigationViewItemSeparator))
            {
                return SeparatorTemplate;
            }
            else
            {
                return null;
            }
        }
    }
}
