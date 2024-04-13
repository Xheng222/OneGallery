using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Windows.System;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
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
        public SettingsConfig MySettingsConfig { get; set; }

        public PathConfig MyPathConfig { get; set; }

        // expand 用
        Dictionary<string, NavigationViewItem> PageDictionary = new();

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
                TitleFont.Glyph = value.Icon.Glyph;
                OnPropertyChanged();
            }
        }

        public Frame NaPage
        {
            get { return Nv_page; }
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

        Task InitWindowTask {  get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            FolderManager = new();
            InitWindowTask = InitWindow();
        }

        public async Task InitFolder()
        {
            await FolderManager.InitImageListPage(NowCategory);
        }

        SystemBackdropConfiguration m_configurationSource;
        DesktopAcrylicController m_backdropController;

        private async Task InitWindow()
        {
            await FolderManager.InitConfigs();
            this.SetWindowSize(MySettingsConfig.LastWidth, MySettingsConfig.LastHeight);
            Task InitCategory = InitCategories();

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
            InitConfigs();
            await InitCategory;
        }

        private async Task InitCategories()
        {
            Categories.Add(new NavigationViewItemSeparator());
            Category _temp = new Category()
            {
                _name = "所有照片",
                PageType = "ImageListPage",
                IsGallery = true,
                IsHomePage = true,
            };
            _temp.SetFontIcon("\uE80F");
            
            Categories.Add(_temp);
            Categories.Add(new NavigationViewItemSeparator());

            _temp = new Category()
            {
                _name = "画廊",
                PageType = "AddFolderOrGallery",
            };
            _temp.SetFontIcon("\uE8B9");
            //var _tempList = MyPathConfig.GalleryToFolderListConfig.Keys.ToList();
            //_tempList.RemoveAt(0);
            InitAddCategories(_temp, MyPathConfig.GalleryToFolderListConfig.Keys.ToList(), false);
            Categories.Add(_temp);
            Categories.Add(new NavigationViewItemSeparator());
            
            _temp = new Category()
            {
                _name = "文件夹",
                PageType = "AddFolderOrGallery",
                IsFolderInfo = true

            };
            _temp.SetFontIcon("\uEC50");
            InitAddCategories(_temp, MyPathConfig.FolderPathConfig.Keys.ToList(), true);
            Categories.Add(_temp);

            await Task.Delay(100);
        }

        private void InitAddCategories(Category _parent, List<string> _children, bool _isFolder)
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
                        NavView_Navigate(typeof(SettingPage));
                    }
                }
                else if (args.InvokedItemContainer != null)
                {
                    var PageCategory = Nv.SelectedItem as Category;

                    if (PageCategory != NowCategory)
                    {
                        Type navPageType = Type.GetType("OneGallery." + PageCategory.PageType);
                        NavView_Navigate(navPageType, PageCategory);
                    }

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
                //Debug.Print(_nowCategory.Name);
                //ChangeSelect(LastCategory);
                //if (LastCategory == SettingCategory)
                //    Nv.SelectedItem = PageDictionary["Settings"];
                //else
                //{
                //    if (LastCategory.IsHomePage)
                //    {
                //        Nv.SelectedItem = LastCategory;
                //    }
                //    else if (LastCategory.IsFolder)
                //    {
                //        DispatcherQueue.TryEnqueue(() =>
                //        {
                //            Nv.Expand(PageDictionary["文件夹"]);
                //        });

                //        SelectPage();
                //    }
                //    else if (LastCategory.IsGallery)
                //    {
                //        DispatcherQueue.TryEnqueue(() =>
                //        {
                //            Nv.Expand(PageDictionary["画廊"]);
                //        });

                //        SelectPage();
                //    } 
                //    else
                //    {
                //        Nv.SelectedItem = LastCategory;
                //    }
                //}
            }
            //else if (e.NavigationMode == NavigationMode.New && NowCategory is not null)
            //{
            //    Debug.Print(_nowCategory.Name);                    
            //}


            if (Nv_page.CanGoBack)
            {
                await Task.Delay(250);
                Nv.IsBackEnabled = true;
            }
        }

        public void ChangeSelect(Category _category)
        {
            if (_category == SettingCategory)
                Nv.SelectedItem = PageDictionary["Settings"];
            else
            {
                if (Nv.SelectedItem != _category)
                {
                    if (_category.IsHomePage)
                    {
                        Nv.SelectedItem = _category;
                    }
                    else if (_category.IsFolder)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            Nv.Expand(PageDictionary["文件夹"]);
                        });

                        SelectPage(_category);
                    }
                    else if (_category.IsGallery)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            Nv.Expand(PageDictionary["画廊"]);

                        });

                        SelectPage(_category);
                    }
                    else
                    {
                        Nv.SelectedItem = _category;
                    }
                }
            }
        }

        private void SelectPage(Category _category)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                if (Nv.IsPaneOpen)
                {
                    await Task.Delay(50);
                    Nv.SelectedItem = _category;
                }
                else
                {
                    if (_category.IsFolder)
                    {
                        Nv.SelectedItem = Categories[5];
                        await Task.Delay(50);
                    }
                    else
                    {
                        if (_category.IsGallery)
                        {
                            Nv.SelectedItem = Categories[3];
                            await Task.Delay(50);
                        }
                    }
                }

                Nv.SelectedItem = _category;
            });
        }

        /* 
         * NavigationView
         */

        private async void Nv_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Print("Nv_Loaded");
            //Nv_page.CacheSize = 0;

            //FolderManager.InitFolder();
            //var rootGrid = VisualTreeHelper.GetChild(sender as NavigationView, 0);

            //await InitWindowTask;
            //Nv.SelectedItem = Categories[1];
            //NavView_Navigate(typeof(ImageListPage), (Category)Categories[1]);
            //FindNaView(rootGrid);
        }

        private void Nv_PaneClosing(NavigationView sender, object args)
        {
            Nv_grid.Width = Nv.ActualWidth - Nv.CompactPaneLength + 8;
        }

        private void Nv_PaneOpening(NavigationView sender, object args)
        {
            Nv_grid.Width = Nv.ActualWidth - Nv.OpenPaneLength + 8;

            
        }

        /*
         *  Nv_Grid
         */

        private void Nv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Nv.IsPaneOpen)
                Nv_grid.Width = Nv.ActualWidth - Nv.OpenPaneLength + 8;
            else
                Nv_grid.Width = Nv.ActualWidth - Nv.CompactPaneLength + 8;

            Nv_grid.Height = Nv.ActualHeight - 40;

            if (this.Width < 800)
            {
                if (Nv.IsPaneOpen)
                    Nv.IsPaneOpen = false;
            }
            else
                if (!Nv.IsPaneOpen)
                    Nv.IsPaneOpen = true;
        }


        private async void Nv_grid_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Print("Nv_grid_Loaded");
            Nv_page.CacheSize = 0;

            //FolderManager.InitFolder();
            var rootGrid = VisualTreeHelper.GetChild(Nv, 0);

            await InitWindowTask;
            FindNaView(rootGrid);
            Nv.SelectedItem = Categories[1];
            NavView_Navigate(typeof(ImageListPage), (Category)Categories[1]);
        }


        private void FindNaView(DependencyObject Item)
        {
            var ChildNum = VisualTreeHelper.GetChildrenCount(Item);
            if (ChildNum >= 0)
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
                }
                else
                {
                    for (var i = 0; i < ChildNum; i++)
                        FindNaView(VisualTreeHelper.GetChild(Item, i));
                }
            }
            return;
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

        /*
         * Check 
         */

        public bool CheckAddFolderOrGalleryName(string _name)
        {
            if (NowCategory.IsFolderInfo)
            {
                foreach (var _existName in MyPathConfig.FolderPathConfig.Keys)
                {
                    if (_name == _existName)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                foreach (var _existName in MyPathConfig.GalleryToFolderListConfig.Keys)
                {
                    if (_name == _existName)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        

        public bool CheckAddFolderPath(string _folderPath)
        {
            if (NowCategory.IsFolderInfo)
            {
                foreach (var _existPath in MyPathConfig.FolderPathConfig.Values)
                {
                    if (_folderPath == _existPath)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /*
         * Add
         */

        public async void AddFolderOrGallery(string _name, string _folderPath)
        {

            MyPathConfig.FolderPathConfig.Add(_name, _folderPath);
            await FolderManager.InitFolderTask;
            FolderManager.InitFolderTask = FolderManager.AddNewFolder(_name, _folderPath);

            Category _temp = new()
            { 
                Name = _name,
                IsFolder = true,
                PageType = "ImageListPage",
            };
            _temp.SetFontIcon("\uE8B7");

            var FolderCategory = Categories[5] as Category;
            FolderCategory.Children.Insert(FolderCategory.Children.Count - 1, _temp);

            return;
        }

        public bool AddFolderOrGallery(string _name, List<object> _folderName)
        {
            List<string> _tempFolderList = new();
            foreach (var _item in _folderName)
            {
                if (_item is Category _category)
                {
                    _tempFolderList.Add(_category.Name);
                }
            }

            MyPathConfig.GalleryToFolderListConfig.Add(_name, _tempFolderList);
            FolderManager.AddNewGallery(_name);

            Category _temp = new()
            {
                Name = _name,
                IsGallery = true,
                PageType = "ImageListPage",
            };
            _temp.SetFontIcon("\uE8EC");

            var GalleryCategory = Categories[3] as Category;
            GalleryCategory.Children.Insert(GalleryCategory.Children.Count - 1, _temp);

            return true;
        }

        /*
         * Reset
         */

        public bool ResetFolder(string _oldname, string _newName)
        {
            if (_oldname != _newName)
            {
                NavigateHelper.ChangeFolderDict(_oldname, _newName);
                FolderManager.RenameFolder(_oldname, _newName);

                string _folderPath = MyPathConfig.FolderPathConfig[_oldname];
                MyPathConfig.FolderPathConfig.Remove(_oldname);
                MyPathConfig.FolderPathConfig.Add(_newName, _folderPath);

                foreach (var _gallery in MyPathConfig.GalleryToFolderListConfig)
                {
                    if (_gallery.Value.Contains(_oldname))
                    {
                        _gallery.Value.Remove(_oldname);
                        _gallery.Value.Add(_newName);
                    }
                }

                foreach (var _category in (Categories[5] as Category).Children)
                {
                    if (_category is Category _tempCategory)
                    {
                        if (_tempCategory.Name == _oldname)
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                _tempCategory._name = _newName;
                            });

                            break;
                        }
                    }
                }
            }
            return true;
        }

        public bool ResetFolder(string _oldname, string _newName, string _newPath)
        {
            ResetFolder(_oldname, _newName);
            MyPathConfig.FolderPathConfig[_newName] = _newPath;
            FolderManager.ResetFolder(_newName, _newPath);

            return true;
        }


        public bool ResetGallery(string _oldname, string _newName)
        {
            if (_oldname != _newName)
            {
                NavigateHelper.ChangeGalleryDict(_oldname, _newName);

                var _tempFolders = MyPathConfig.GalleryToFolderListConfig[_oldname];
                MyPathConfig.GalleryToFolderListConfig.Remove(_oldname);
                MyPathConfig.GalleryToFolderListConfig.Add(_newName, _tempFolders);

                FolderManager.RenameGallery(_oldname, _newName);

                foreach (var _child in (Categories[3] as Category).Children)
                {
                    if (_child is Category _category)
                    {
                        if (_category.Name == _oldname)
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                _category._name = _newName;
                            });

                            break;
                        }
                    }
                }
            }
            return true;
        }

        public bool ResetGallery(string _oldname, string _newName, List<string> _newFolders)
        {
            ResetGallery(_oldname,_newName);
            MyPathConfig.GalleryToFolderListConfig[_newName] = _newFolders;
            FolderManager.ResetGallery(_newName);
            return true;
        }


        /*
         * Delete
         */

        public bool DeleteFolder(string _folderName)
        {
            NavigateHelper.RemoveFolder(_folderName);
            FolderManager.RemoveFolder(_folderName);

            MyPathConfig.FolderPathConfig.Remove(_folderName);

            foreach (var _gallery in MyPathConfig.GalleryToFolderListConfig)
            {
                if (_gallery.Value.Contains(_folderName))
                {
                    _gallery.Value.Remove(_folderName);
                }
            }

            foreach (var _category in (Categories[5] as Category).Children)
            {
                var _tempCategory = _category as Category;
                if (_tempCategory.Name == _folderName)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        (Categories[5] as Category).Children.Remove(_category);
                    });
                    break;
                }
            }

            foreach (var _temp in Nv_page.BackStack)
            {
                var _c = _temp.Parameter as Category;
                if (_c.IsFolder && _c.Name == _folderName)
                {
                    Nv_page.BackStack.Remove(_temp);
                }
            }

            Category _tempParameter = null;
            List<PageStackEntry> _removeList = new();
            foreach (var _temp in Nv_page.BackStack)
            {

                if (_tempParameter is null)
                {
                    _tempParameter = _temp.Parameter as Category;
                    Debug.Print("first " + _tempParameter.Name);
                }
                else
                {
                    if (_temp.Parameter is Category _c)
                    {
                        if (_c == _tempParameter)
                        {
                            Debug.Print("remove " + _c.Name);
                            _removeList.Add(_temp);
                        }
                        else
                        {
                            _tempParameter = _c;
                            Debug.Print("switch " + _tempParameter.Name);
                        }

                    }
                }
            }

            foreach (var _item in _removeList)
                Nv_page.BackStack.Remove(_item);

            Category _cat = Nv_page.BackStack.Last().Parameter as Category;
            if (_cat == NowCategory)
                Nv_page.BackStack.Remove(Nv_page.BackStack.Last());


            return true;
        }

        public bool DeleteGallery(string _galleryName)
        {
            NavigateHelper.RemoveGallery(_galleryName);
            FolderManager.RemoveGallery(_galleryName);
            MyPathConfig.GalleryToFolderListConfig.Remove(_galleryName);

            foreach (var _category in (Categories[3] as Category).Children)
            {
                if (_category is Category _tempCategory)
                {
                    if (_tempCategory.Name == _galleryName)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            (Categories[3] as Category).Children.Remove(_category);
                        });
                        break;
                    }
                }
            }

            foreach (var _temp in Nv_page.BackStack)
            {
                var _c = _temp.Parameter as Category;
                if (_c.IsGallery && _c.Name == _galleryName)
                {
                    Nv_page.BackStack.Remove(_temp);
                }
            }

            Category _tempParameter = null;
            List<PageStackEntry> _removeList = new();
            foreach (var _temp in Nv_page.BackStack)
            {

                if (_tempParameter is null)
                {
                    _tempParameter = _temp.Parameter as Category;
                    Debug.Print("first " + _tempParameter.Name);
                }
                else
                {
                    if (_temp.Parameter is Category _c) 
                    {
                        if (_c == _tempParameter)
                        {
                            Debug.Print("remove " + _c.Name);
                            _removeList.Add(_temp);
                        }
                        else
                        {
                            _tempParameter = _c;
                            Debug.Print("switch " + _tempParameter.Name);
                        }
                            
                    }
                }
            }
            
            foreach (var _item in _removeList)  
                Nv_page.BackStack.Remove(_item);

            Category _cat = Nv_page.BackStack.Last().Parameter as Category;
            if (_cat == NowCategory)
                Nv_page.BackStack.Remove(Nv_page.BackStack.Last());

            return true;
        }
    }

    internal class MyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }

        public DataTemplate SeparatorTemplate { get; set; }

        public DataTemplate NULLTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            // Return the correct data template based on the item's type.
            if (item.GetType() == typeof(Category))
            {
                if (!(item as Category).IsAddSelection)
                    return ItemTemplate;
                else
                    return NULLTemplate;
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
