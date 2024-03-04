using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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

        public MainWindow()
        {
            this.InitializeComponent();
            


            this.ExtendsContentIntoTitleBar = true;

            this.TrySetAcrylicBackdrop();
            this.InitializeSampleTreeView();
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

        private void InitializeSampleTreeView()
        {
            TreeViewNode workFolder = new TreeViewNode() { Content = "Work Documents" };
            workFolder.IsExpanded = true;

            workFolder.Children.Add(new TreeViewNode() { Content = "XYZ Functional Spec" });
            workFolder.Children.Add(new TreeViewNode() { Content = "Feature Schedule" });
            workFolder.Children.Add(new TreeViewNode() { Content = "Overall Project Plan" });
            workFolder.Children.Add(new TreeViewNode() { Content = "Feature Resources Allocation" });

            TreeViewNode remodelFolder = new TreeViewNode() { Content = "Home Remodel" };
            remodelFolder.IsExpanded = true;

            remodelFolder.Children.Add(new TreeViewNode() { Content = "Contractor Contact Info" });
            remodelFolder.Children.Add(new TreeViewNode() { Content = "Paint Color Scheme" });
            remodelFolder.Children.Add(new TreeViewNode() { Content = "Flooring woodgrain type" });
            remodelFolder.Children.Add(new TreeViewNode() { Content = "Kitchen cabinet style" });

            personalFolder = new TreeViewNode() { Content = "Personal Documents" };
            personalFolder.IsExpanded = true;
            personalFolder.Children.Add(remodelFolder);

            sampleTreeView.RootNodes.Add(workFolder);
            sampleTreeView.RootNodes.Add(personalFolder);
            sampleTreeView.IsHoldingEnabled = false;
        }

        private void sampleTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            return;
        }
    }
}
