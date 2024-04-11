using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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


    public partial class MainWindow : WindowEx, INotifyPropertyChanged
    {
        public string appTitleText = "OneGallery";

        //public Stack<string> HistoryPages = new();

        //Stack<string> PartenPagemName = new();

        public SettingsConfig MySettingsConfig { get; set; }

        public PathConfig MyPathConfig { get; set; }

        // expand 用
        //Dictionary<string, string> ParentDictionary = new Dictionary<string, string>();

        // expand 用
        Dictionary<string, NavigationViewItem> PageDictionary = new Dictionary<string, NavigationViewItem>();

        // select 用
        //Dictionary<string, Category> NvItemDictionary = new Dictionary<string, Category>();

        // Folder
        public LocalFolderManager FolderManager { get; set; }

        public ObservableCollection<object> Categories = new();

        public Category SettingCategory = new()
        {
            Name = "Settings",
        };
        public Category NowCategory {  get; set; }

        public Category _nowCategory
        {
            get => NowCategory;
            set
            {
                NowCategory = value;
                OnPropertyChanged();
            }
        }

        private string SelectPageName {  get; set; }

        public Frame NaPage
        {
            get { return Nv_page; }
            set { Nv_page = value; }
        } 

        public NavigationView NaView
        {
            get { return Nv; }
        }

        public Storyboard TitleBorderUp
        {
            get { return BorderUpForImagePage; }
        }

        public Storyboard TitleBorderDown
        {
            get { return BorderDownForImagePage; }
        }

        public Grid NaGrid
        {
            get { return Nv_grid; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            FolderManager = new();
            //_nowCategory = (Category)Categories[1];
            InitWindow();
        }

        public async Task InitFolder()
        {
            await FolderManager.InitPageFolder(NowCategory);
        }

        SystemBackdropConfiguration m_configurationSource;
        DesktopAcrylicController m_backdropController;

        private async void InitWindow()
        {
            await FolderManager.InitConfigs();
            //MySettingsConfig = FolderManager.MySettingsConfig;

            this.SetWindowSize(MySettingsConfig.LastWidth, MySettingsConfig.LastHeight);
            InitCategories();

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
            FindItem();
            this.Closed += Window_Closed;
            
        }

        private void InitCategories()
        {
            Categories.Add(new NavigationViewItemSeparator());
            Category _temp = new Category()
            {
                _name = "所有照片",
                PageType = "ImageListPage",
                IsGallery = true,
            };
            _temp.SetFontIcon("\uE80F");
            
            Categories.Add(_temp);
            Categories.Add(new NavigationViewItemSeparator());

            _temp = new Category()
            {
                _name = "画廊",
                PageType = "ImageListPage",
                IsGallery = true,
            };
            _temp.SetFontIcon("\uE8B9");
            AddCategories(_temp, MyPathConfig.GalleryToFolderListConfig.Keys.ToList(), false);
            Categories.Add(_temp);
            Categories.Add(new NavigationViewItemSeparator());
            
            _temp = new Category()
            {
                _name = "文件夹",
                PageType = "ImageListPage",
                IsGallery = true,
            };
            _temp.SetFontIcon("\uEC50");
            AddCategories(_temp, MyPathConfig.FolderToFolderListConfig.Keys.ToList(), true);
            Categories.Add(_temp);
        }

        private void AddCategories(Category _parent, List<string> _children, bool _isFolder)
        {
            foreach (var _child in _children)
            {
                Category _temp = new Category()
                {
                    _name = _child,
                    PageType = "ImageListPage",
                };

                if (_isFolder)
                {
                    _temp.IsFolder = true;
                    _temp.SetFontIcon("\uE8B7");
                }
                    
                else
                {
                    _temp.IsGallery = true;
                    _temp.SetFontIcon("\uE8EC");
                }
                    
                
                _parent.Children.Add(_temp);
            }
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


        /*
         * NavView_Navigate
         */

        private void NavView_Navigate(Type navPageType, Category page)
        {
            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null)
            {
                Nv_page.Navigate(navPageType, page, new EntranceNavigationTransitionInfo());
            }
        }

        private void NavView_Navigate(Type navPageType)
        {
            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null)
            {
                Nv_page.Navigate(navPageType, SettingCategory, new EntranceNavigationTransitionInfo());
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
                        //HistoryPages.Push(SelectPageName);
                        SelectPageName = "Settings";
                        NavView_Navigate(typeof(SettingPage));
                    }
                }
                else if (args.InvokedItemContainer != null)
                {
                    var PageCategory = Nv.SelectedItem as Category;

                    if (PageCategory != NowCategory)
                    {
                        //HistoryPages.Push(SelectPageName);
                        SelectPageName = PageCategory.Name;
                        Type navPageType = Type.GetType("OneGallery." + PageCategory.PageType);
                        NavView_Navigate(navPageType, PageCategory);
                    }

                }

            }

        }

        private Category LastCategory {  get; set; }
        public async void Nv_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            Nv.IsBackEnabled = false;

            if (Nv_page.CanGoBack)
            {
                if (Nv_page.CurrentSourcePageType == typeof(ImagePage))
                {
                    await ImagePage.NowImagePage.ResetAll();
                }
                if (Nv_page.BackStack.Count > 0)
                {
                    Debug.Print((Nv_page.BackStack.First().Parameter as Category).Name + " First");
                    Debug.Print((Nv_page.BackStack.Last().Parameter as Category).Name + " Last");
                }
                LastCategory = Nv_page.BackStack.Last().Parameter as Category;
                Nv_page.GoBack();
            }
        }

        private async void Nv_Page_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                Debug.Print(_nowCategory.Name);

                if (LastCategory == SettingCategory)
                    Nv.SelectedItem = PageDictionary["Settings"];
                else
                {
                    if (LastCategory.IsFolder)
                    {

                    }
                    else
                    {

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            ExpandParentPage();
                        });

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            if (Nv.IsPaneOpen)
                            {
                                Nv.SelectedItem = LastCategory;
                            }
                            else
                            {
                                SelectPage();
                            }
                        });
                        //if (Nv.IsPaneOpen)
                        //{
                        //    Nv.SelectedItem = LastCategory;
                        //}
                        //else
                        //{
                        //    SelectPage();
                        //}
                    }
                }
            }


            if (Nv_page.CanGoBack)
            {
                await Task.Delay(250);
                Nv.IsBackEnabled = true;
            }
        }

        private async void SelectPage()
        {
            if (LastCategory.IsFolder)
            {
                Nv.SelectedItem = Categories[5];
                await Task.Delay(50);
            }
            else
            {
                if (LastCategory.IsGallery)
                {
                    Nv.SelectedItem = Categories[3];
                    await Task.Delay(50);
                }
            }

            Nv.SelectedItem = LastCategory;
            //string ParentPageName = PageName;
            //while (ParentDictionary[ParentPageName] != null)
            //{
            //    ParentPageName = ParentDictionary[ParentPageName];
            //}

            //Nv.SelectedItem = NvItemDictionary[ParentPageName];
            //await Task.Delay(50);
            //Nv.SelectedItem = NvItemDictionary[PageName];
        }
        private void ExpandParentPage()
        {
            //if (ParentDictionary[PageName] != null)
            //{
            //    ExpandParentPage(ParentDictionary[PageName]);
            //}

            if (_nowCategory.IsFolder)
            {
                //Nv.SelectedItem = Categories[5];
                Nv.Expand(PageDictionary["文件夹"]);
            }
            else
            {
                if (_nowCategory.IsGallery)
                {
                    Nv.Expand(PageDictionary["画廊"]);
                }
            }

            return;
        }

        /* 
         * NavigationView
         */

        private void Nv_Loaded(object sender, RoutedEventArgs e)
        {
            Nv_page.CacheSize = 0;

            FolderManager.InitFolder();
            var rootGrid = VisualTreeHelper.GetChild(sender as NavigationView, 0);
            FindNaView(rootGrid);
            //UpdateNvItemDir(Categories);

            Nv.SelectedItem = Categories[1];
            NavView_Navigate(typeof(ImageListPage), (Category)Categories[1]);
        }

        private void Nv_PaneClosing(NavigationView sender, object args)
        {
            Nv_page.Width = Nv.ActualWidth - Nv.CompactPaneLength + 8;
        }

        private void Nv_PaneOpening(NavigationView sender, object args)
        {
            Nv_page.Width = Nv.ActualWidth - Nv.OpenPaneLength + 8;
        }

        private void Nv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Nv.IsPaneOpen)
                Nv_grid.Width = Nv.ActualWidth - Nv.OpenPaneLength + 8;
            else
                Nv_grid.Width = Nv.ActualWidth - Nv.CompactPaneLength + 8;

            Nv_grid.Height = Nv.ActualHeight - 40;

            if (Nv.ActualWidth < 700)
                if (Nv.IsPaneOpen)
                    Nv.IsPaneOpen = false;
            else if (!Nv.IsPaneOpen)
                Nv.IsPaneOpen = true;
        }


        private void FindNaView(DependencyObject Item)
        {
            var ChildNum = VisualTreeHelper.GetChildrenCount(Item);
            if (ChildNum > 0)
            {
                if (Item.GetType() == typeof(NavigationViewItem))
                {
                    var NaView = (NavigationViewItem)Item;
                    var PageName = NaView.Tag.ToString();
                    Debug.Print("FIND " +  PageName);   
                    if (!PageDictionary.ContainsKey(PageName))
                    {
                        PageDictionary.Add(PageName, NaView);
                    }

                    return;

                    //if (!ParentDictionary.ContainsKey(PageName))
                    //{
                    //    if (PartenPagemName.Count != 0)
                    //    {
                    //        ParentDictionary.Add(PageName, PartenPagemName.Peek());
                    //    }
                    //    else
                    //    {
                    //        ParentDictionary.Add(PageName, null);
                    //    }
                        
                    //}

                    //PartenPagemName.Push(PageName);

                    //for (var i = 0; i < ChildNum; i++)
                    //    FindNaView(VisualTreeHelper.GetChild(Item, i));

                    //PartenPagemName.Pop();
                }

                else
                {
                    for (var i = 0; i < ChildNum; i++)
                        FindNaView(VisualTreeHelper.GetChild(Item, i));
                }
            }
            return;
        }
        //private void UpdateNvItemDir(ObservableCollection<object> Items)
        //{

        //    if (Items != null)
        //    {
        //        foreach (var _item in Items)
        //        {
        //            if (_item is Category)
        //            {
        //                NvItemDictionary.Add((_item as Category).Name, (Category)_item);
        //                UpdateNvItemDir((_item as Category).Children);
        //            }

        //        }

        //    }
        //    return;
        //}





        double RowHeight = 120;

        double _rowHeight
        {
            get => RowHeight;
            set
            {
                RowHeight = value;
                OnPropertyChanged();
            }
        }



        private async void FindItem()
        {
            //await Task.Delay(5000);
            //while (true)
            //{
            //    Category category = (Category)Categories[1];
            //    if (category.Icon.Glyph == "\uE701")
            //    {
            //        category.SetFontIcon("\uE80F");
            //        //category.Name = "555";
            //        _nowCategory = (Category)Categories[1];
            //    }
            //    else
            //    {
            //        category.SetFontIcon("\uE701");
            //        //category.Name = "HomePage";
            //        _nowCategory = (Category)Categories[3];
            //    }
            //    await Task.Delay(1500);
            //    Debug.Print("111");
            //}



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
