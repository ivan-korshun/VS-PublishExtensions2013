using System;
using System.Collections.Generic;
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
using PHZH.PublishExtensions.Settings;

namespace PHZH.PublishExtensions.UI
{
    /// <summary>
    /// Interaction logic for MappingWindow.xaml
    /// </summary>
    public partial class MappingWindow : Window
    {
        private static char[] invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

        private ProjectSettings settings = null;
        private ItemSettings item = null;
        private string itemName = null;
        
        public MappingWindow(ProjectSettings settings, ItemSettings item)
        {
            InitializeComponent();

            // set variables
            this.settings = settings;
            this.item = item;
            this.itemName = item.GetName();

            // fill controls inital values
            lblPath.Text = item.Path;
            txbMapping.Text = item.Mapping.OrDefault(itemName);

            // initialize controls
            txbMapping.SelectAll();
            txbMapping.Focus();

            UpdateMappingPreview();
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            string mapping = txbMapping.Text;
            if (mapping.IndexOfAny(invalidChars) >= 0)
            {
                MessageBox.Show(
                    "The mapping contains invalid characters." + 
                    Environment.NewLine + Environment.NewLine + 
                    "Forbidden characters are: " + invalidChars.ToItemString(" "),
                    "Invalid Mapping",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }
            
            // save mapping
            item.Mapping = GetMapping();

            // close dialog
            this.DialogResult = true;
        }

        private void OnRemoveMapping_Click(object sender, RoutedEventArgs e)
        {
            // save mapping
            item.Mapping = null;

            // close dialog
            this.DialogResult = true;
        }

        private void OnMapping_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMappingPreview();
        }

        private string GetMapping()
        {
            // if mapping is the same as the item name remove mapping
            string mapping = txbMapping.Text;
            if (mapping.EqualsIgnoreCase(itemName))
                mapping = null;

            return mapping;
        }

        private void UpdateMappingPreview()
        {
            if (item == null)
                return;

            string mapping = GetMapping();
            string preview = item.ApplyMapping(mapping);

            if (mapping.IsNullOrWhiteSpace())
            {
                lblPreview.Text = preview;
            }
            else
            {
                int startIndex = preview.LastIndexOf(mapping);
                int length = mapping.Length;

                lblPreview.Inlines.Clear();
                if (startIndex > 0)
                    lblPreview.Inlines.Add(preview.Substring(0, startIndex));

                lblPreview.Inlines.Add(new Bold(new Run(preview.Substring(startIndex, length))));

                if (startIndex + length < preview.Length)
                    lblPreview.Inlines.Add(preview.Substring(startIndex + length));
            }
        }
    }
}
