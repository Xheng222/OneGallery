using System;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        private Category NowCategory { get; set; }

        private MainWindow Window { get; set; }

        private SettingsConfig Settings { get; set; }

        public static int[] Height_Large = { 375, 400, 425, 450, 500 };
        public static int[] Height_Meduim = { 225, 250, 275, 300, 350 };
        public static int[] Height_Small = { 100, 125, 150, 175, 200 };

        public SettingPage()
        {
            this.InitializeComponent();
            Window = (Application.Current as App).Main;
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NowCategory = e.Parameter as Category;
            Window._nowCategory = NowCategory;
            Settings = Window.MySettingsConfig;
            LoadSetting();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ChooseComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            SortComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            UpOrDownComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            ImageSizeComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            ImageZoomComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            HeightSmallComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            HeightMeduimComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            HeightLargeComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            DeleteComboBox.SelectionChanged -= ComboBox_SelectionChanged;
            base.OnNavigatedFrom(e);
            GC.Collect();
        }

        private void LoadSetting()
        {
            ChooseComboBox.SelectedIndex = Settings.ChooseMode;
            SortComboBox.SelectedIndex = Settings.SortMode;
            UpOrDownComboBox.SelectedIndex = Settings.IsAscending ? 0 : 1;
            ImageSizeComboBox.SelectedIndex = Settings.ImageSizeMode;
            ImageZoomComboBox.SelectedIndex = Settings.ImageZoomMode;
            HeightSmallComboBox.SelectedIndex = Settings.ImageHeight_Small;
            HeightMeduimComboBox.SelectedIndex = Settings.ImageHeight_Medium;
            HeightLargeComboBox.SelectedIndex = Settings.ImageHeight_Large;
            DeleteComboBox.SelectedIndex = Settings.DeleteToTrashcan ? 0 : 1;

            ChooseComboBox.SelectionChanged += ComboBox_SelectionChanged;
            SortComboBox.SelectionChanged += ComboBox_SelectionChanged;
            UpOrDownComboBox.SelectionChanged += ComboBox_SelectionChanged;
            ImageSizeComboBox.SelectionChanged += ComboBox_SelectionChanged;
            ImageZoomComboBox.SelectionChanged += ComboBox_SelectionChanged;
            HeightSmallComboBox.SelectionChanged += ComboBox_SelectionChanged;
            HeightMeduimComboBox.SelectionChanged += ComboBox_SelectionChanged;
            HeightLargeComboBox.SelectionChanged += ComboBox_SelectionChanged;
            DeleteComboBox.SelectionChanged += ComboBox_SelectionChanged;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AcceptButton.Opacity = 1;
            CancleButton.Opacity = 1;
        }

        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSetting();
            AcceptButton.Opacity = 0;
            CancleButton.Opacity = 0;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSetting();
            AcceptButton.Opacity = 0;
            CancleButton.Opacity = 0;
        }

        private void SaveSetting()
        {
            Settings.ChooseMode = ChooseComboBox.SelectedIndex;
            Settings.SortMode = SortComboBox.SelectedIndex;

            Settings.ImageSizeMode = ImageSizeComboBox.SelectedIndex;
            Settings.ImageZoomMode = ImageZoomComboBox.SelectedIndex;
            Settings.ImageHeight_Small = HeightSmallComboBox.SelectedIndex;
            Settings.ImageHeight_Medium = HeightMeduimComboBox.SelectedIndex;
            Settings.ImageHeight_Large = HeightLargeComboBox.SelectedIndex;

            Settings.IsAscending = UpOrDownComboBox.SelectedIndex == 0;
            Settings.DeleteToTrashcan = DeleteComboBox.SelectedIndex == 0;

            Window.InitConfigs();
            Window.SaveConfigs();
        }



    }
}
