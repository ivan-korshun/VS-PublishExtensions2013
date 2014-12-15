using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PHZH.PublishExtensions.Details;
using PHZH.PublishExtensions.Settings;

namespace PHZH.PublishExtensions.UI
{
    /// <summary>
    /// Interaction logic for MappingWindow.xaml
    /// </summary>
    public partial class ProjectSettingsWindow : Window
    {
        private ProjectSettings settings = null;

        public ProjectSettingsWindow(string projectName, ProjectSettings settings)
        {
            InitializeComponent();

            // set variables
            this.settings = settings;

            // fill controls inital values
            lblProject.Text = projectName;
            txbPublishFolder.Text = settings.GetPublishLocation();
            chkIndividualFolder.IsChecked = settings.PublishTarget.IsUserSpecific;
            txbIgnoreFilter.Text = settings.IgnoreFilter;
            chkMappingEnabled.IsChecked = settings.MappingEnabled;

            // initialize controls
            txbPublishFolder.Focus();
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            string location = txbPublishFolder.Text;
            
            // validate input
            if (location.IsNullOrWhiteSpace())
            {
                MessageBox.Show(
                    "The publish location is required.",
                    "Invalid Settings",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            // check access
            if (!Publisher.CheckAccess(location))
                return;
            
            // write settings
            settings.UpdatePublishTarget(chkIndividualFolder.IsChecked.Value, location);
            settings.IgnoreFilter = txbIgnoreFilter.Text;
            settings.MappingEnabled = chkMappingEnabled.IsChecked.Value;

            // close dialog
            this.DialogResult = true;
        }

        private void OnPublishFolderBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select the folder to publish the files to.",
                ShowNewFolderButton = true,
                RootFolder = Environment.SpecialFolder.Desktop,
                SelectedPath = txbPublishFolder.Text.OrDefault(string.Empty)
            };

            using (dialog)
            {
                if (dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    txbPublishFolder.Text = dialog.SelectedPath;
            }
        }

        private void OnResetIgnoreFilter_Click(object sender, RoutedEventArgs e)
        {
            txbIgnoreFilter.Text = ProjectSettings.DEFAULT_IGNORE_FILTER;
        }
    }
}
