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

namespace DnsManager.Windows.Modals
{
    /// <summary>
    /// Interaction logic for AddNewDnsModalWindow.xaml
    /// </summary>
    public partial class AddNewDnsModalWindow : Window
    {
        public DnsItem Item { get; protected set; }

        public AddNewDnsModalWindow(DnsItem item = null)
        {
            InitializeComponent();

            // Setup event handlers
            this.BtnAdd.Click += BtnAdd_Click;
            this.BtnCancel.Click += BtnCancel_Click;
            this.ComboBoxProtocol.SelectionChanged += ComboBoxProtocol_SelectionChanged;
            this.MouseDown += AddNewDnsModalWindow_MouseLeftButtonDown;

            // Load protocol configurations into ComboBox
            LoadProtocolOptions();

            // If editing existing item, populate fields
            if (item != null)
            {
                PopulateFromItem(item);
            }

            // Initially hide encrypted protocol options
            UpdateProtocolVisibility();
        }

        private void LoadProtocolOptions()
        {
            ComboBoxProtocol.Items.Clear();
            var protocols = DnsProtocolHandler.GetProtocolConfigs();

            foreach (var protocol in protocols.Values)
            {
                ComboBoxProtocol.Items.Add(protocol.Description);
            }
        }

        private void PopulateFromItem(DnsItem item)
        {
            TextBoxName.Text = item.Name;
            TextBoxDns.Text = item.DnsAddress;
            TextBoxDnsAlt.Text = item.DnsAddressAlt;
            TextBoxDescription.Text = item.Description;
            TextBoxPriority.Text = item.Priority.ToString();
            TextBoxHostName.Text = item.HostName;
            TextBoxNotes.Text = item.Notes;

            if (item.CustomPort.HasValue && item.CustomPort.Value > 0)
                TextBoxCustomPort.Text = item.CustomPort.Value.ToString();

            if (item.Protocol == DnsProtocolType.HTTPS)
                TextBoxHttpsPath.Text = item.HttpsPath;

            // Set protocol selection
            ComboBoxProtocol.SelectedIndex = (int)item.Protocol;
        }

        private void ComboBoxProtocol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProtocolVisibility();
        }

        private void UpdateProtocolVisibility()
        {
            int selectedIndex = ComboBoxProtocol.SelectedIndex;
            var protocol = (DnsProtocolType)selectedIndex;
            var config = DnsProtocolHandler.GetProtocolConfig(protocol);

            // Show/hide encrypted protocol options based on selection
            bool isEncrypted = protocol != DnsProtocolType.Standard;
            EncryptedProtocolOptions.Visibility = isEncrypted ? Visibility.Visible : Visibility.Collapsed;
            TextBlockHttpsPath.Visibility = protocol == DnsProtocolType.HTTPS ? Visibility.Visible : Visibility.Collapsed;
            TextBoxHttpsPath.Visibility = protocol == DnsProtocolType.HTTPS ? Visibility.Visible : Visibility.Collapsed;

            // Update validation requirements
            UpdateValidationMessages();
        }

        private void UpdateValidationMessages()
        {
            int selectedIndex = ComboBoxProtocol.SelectedIndex;
            var protocol = (DnsProtocolType)selectedIndex;
            var config = DnsProtocolHandler.GetProtocolConfig(protocol);

            TextBlockValidation.Text = config?.ValidationRules ?? "";
        }

        private void AddNewDnsModalWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(TextBoxName.Text))
            {
                MessageBox.Show("Please enter a server name", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxDns.Text))
            {
                MessageBox.Show("Please enter a DNS address", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get selected protocol
            int selectedIndex = ComboBoxProtocol.SelectedIndex;
            var protocol = (DnsProtocolType)selectedIndex;

            // Additional validation for encrypted protocols
            if (protocol != DnsProtocolType.Standard && string.IsNullOrWhiteSpace(TextBoxHostName.Text))
            {
                MessageBox.Show($"Hostname is required for {protocol} protocol", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create the item
            Item = new DnsItem()
            {
                Name = TextBoxName.Text,
                DnsAddress = TextBoxDns.Text,
                DnsAddressAlt = TextBoxDnsAlt.Text,
                Description = TextBoxDescription.Text,
                Priority = int.TryParse(TextBoxPriority.Text, out int priority) ? priority : 999,
                Protocol = protocol,
                HostName = TextBoxHostName.Text,
                HttpsPath = TextBoxHttpsPath.Text,
                Notes = TextBoxNotes.Text
            };

            // Parse custom port if provided
            if (!string.IsNullOrWhiteSpace(TextBoxCustomPort.Text))
            {
                if (int.TryParse(TextBoxCustomPort.Text, out int port) && port > 0 && port <= 65535)
                {
                    Item.CustomPort = port;
                }
                else
                {
                    MessageBox.Show("Invalid port number. Must be between 1 and 65535", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Validate the complete item
            var (isValid, errors) = DnsProtocolHandler.ValidateDnsItem(Item);
            if (!isValid)
            {
                MessageBox.Show($"Validation failed:\n\n{string.Join("\n", errors)}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}
