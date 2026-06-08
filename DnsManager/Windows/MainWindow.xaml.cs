using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DnsManager.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RotateTransform _rotateTransform;
        private DoubleAnimation _rotationAnimation;

        public MainWindow()
        {
            InitializeComponent();
            InitializeAnimation();

            this.BtnAdd.Click += BtnAddNewDns_Click;
            this.BtnRemove.Click += BtnRemove_Click;
            this.BtnConnect.Click += BtnApply_Click;
            this.BtnEdit.Click += BtnEdit_Click;
            this.BtnPing.Click += BtnPing_Click;
            this.BtnTestDns.Click += BtnTestDns_Click;
            this.BtnTestUrl.Click += BtnTestUrl_Click;

            this.ComboBoxItems.SelectionChanged += ComboBoxItems_SelectionChanged;
            this.MouseDown += MainWindow_MouseDown;

            InitializeComboBox();

            this.LabelVersion.Text = $"{App.Version} ❤️ Mohammad Jalal Jaleh";
            void OpenLink()
            {
                Process.Start("https://github.com/jalaljaleh/DnsManager");
                Process.Start("https://github.com/jalaljaleh");
            }
            ImageHeader.MouseLeftButtonDown += (s, e) => OpenLink();
            LabelVersion.MouseLeftButtonDown += (s, e) => OpenLink();
        }

        /// <summary>
        /// Initialize the rotation animation for the header image
        /// </summary>
        private void InitializeAnimation()
        {
            // Create the RotateTransform for the image
            _rotateTransform = new RotateTransform();
            _rotateTransform.CenterX = 84;  // Half of image width (168/2)
            _rotateTransform.CenterY = 74;  // Half of image height (148/2)
            ImageHeader.RenderTransform = _rotateTransform;

            // Create the animation: rotate 360 degrees continuously
            _rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 20,
                AutoReverse = true,
                Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                RepeatBehavior = RepeatBehavior.Forever
            };
        }

        /// <summary>
        /// Start the rotation animation
        /// </summary>
        private void StartAnimation()
        {
            _rotateTransform.BeginAnimation(RotateTransform.AngleProperty, _rotationAnimation);
        }

        /// <summary>
        /// Stop the rotation animation
        /// </summary>
        private void StopAnimation()
        {
            _rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            _rotateTransform.Angle = 0;  // Reset to original position
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public DnsItem CurrentSystemDNS { get => App.DnsService.Connected; }
        public DnsItem CbSelectedDns { get => (this.ComboBoxItems.SelectedItem as DnsItem); }
        public bool IsAnyDnsConnected { get => CurrentSystemDNS is null ? false : true; }

        void InitializeComboBox()
        {
            this.ComboBoxItems.Items.Clear();

            foreach (var item in App.DnsService.DnsItems.OrderBy(a => a.Priority))
                ComboBoxItems.Items.Add(item);

            if (IsAnyDnsConnected)
                ComboBoxItems.SelectedIndex = ComboBoxItems.Items.IndexOf(CurrentSystemDNS);
            else
                ComboBoxItems.SelectedIndex = 0;
        }

        void OnConnected()
        {
            LabelStatus.Content = $"🔒 Connected to {CurrentSystemDNS.Name}";
            LabelStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0B5D00"));
            borderStatus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6FFB3"));
            StartAnimation();  // Start the rotation animation
        }

        void OnDisconnected()
        {
            LabelStatus.Content = $"🔓 Not Connected";
            LabelStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB5600"));
            borderStatus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFEEB3"));
            StopAnimation();  // Stop the rotation animation
        }

        private void ComboBoxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!App.DnsService.DnsItems.Any())
            {
                BtnAdd.IsEnabled = true;
                BtnEdit.IsEnabled = false;
                BtnConnect.IsEnabled = false;
                BtnRemove.IsEnabled = false;
                BtnPing.IsEnabled = false;
                BtnTestDns.IsEnabled = false;
                BtnTestUrl.IsEnabled = false;

                LabelStatus.Content = "⚠️ No Server Found, Add Servers!";
                LabelStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB5600"));
                LabelDescription.Text = "No Server Information";

                BtnConnect.Content = "Add DNS";
                StopAnimation();  // Ensure animation is stopped
                return;
            }

            if (CbSelectedDns == null) return;

            LabelDescription.Text = DnsProtocolHandler.GetProtocolDetails(CbSelectedDns);

            if (CurrentSystemDNS != null)
            {
                OnConnected();
            }
            else
            {
                OnDisconnected();
            }

            if (CurrentSystemDNS != null && CurrentSystemDNS == CbSelectedDns)
            {
                BtnConnect.Content = "Disconnect";
                BtnConnect.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDB171"));
            }
            else
            {
                BtnConnect.Content = "Connect";
                BtnConnect.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF71D8FD"));
            }

            BtnAdd.IsEnabled = true;
            BtnConnect.IsEnabled = true;
            BtnRemove.IsEnabled = true;
            BtnEdit.IsEnabled = true;
            BtnPing.IsEnabled = true;
            BtnTestDns.IsEnabled = true;
            BtnTestUrl.IsEnabled = true;
        }

        private void BtnAddNewDns_Click(object sender, RoutedEventArgs e)
        {
            var window = new Windows.Modals.AddNewDnsModalWindow();
            var result = window.ShowDialog();
            if (result.Value)
            {
                App.DnsService.DnsItems.Add(window.Item);
                App.DnsService.SaveDnsItems();
                InitializeComboBox();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (CbSelectedDns == null) return;

            var window = new Windows.Modals.AddNewDnsModalWindow(CbSelectedDns);
            var result = window.ShowDialog();
            if (result.Value)
            {
                App.DnsService.DnsItems.Remove(CbSelectedDns);
                App.DnsService.DnsItems.Add(window.Item);
                App.DnsService.SaveDnsItems();
                InitializeComboBox();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (CbSelectedDns == null) return;

            App.DnsService.DnsItems.Remove(CbSelectedDns);
            App.DnsService.SaveDnsItems();

            InitializeComboBox();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (BtnConnect.Content == "Disconnect")
            {
                NetworkManager.UnsetDNS();

                if (IsAnyDnsConnected)
                {
                    App.DnsService.Connected.IsConnected = false;
                    App.DnsService.SaveDnsItems();

                    InitializeComboBox();
                    return;
                }
            }

            if (this.CbSelectedDns == null) return;

            NetworkManager.SetDNS(CbSelectedDns.DnsAddress, CbSelectedDns.DnsAddressAlt);

            App.DnsService.ChangeConnection(CbSelectedDns);
            App.DnsService.SaveDnsItems();

            InitializeComboBox();
        }

        /// <summary>
        /// Ping test button click handler
        /// </summary>
        private async void BtnPing_Click(object sender, RoutedEventArgs e)
        {
            if (CbSelectedDns == null) return;

            BtnPing.IsEnabled = false;
            BtnPing.Content = "⏳ Testing...";

            try
            {
                DnsTestingService.DnsTestResult result = null;

                // Test hostname if available and protocol is encrypted, otherwise test IP
                if (CbSelectedDns.Protocol != DnsProtocolType.Standard && !string.IsNullOrWhiteSpace(CbSelectedDns.HostName))
                {
                    result = await App.DnsService.TestingService.PingHostAsync(CbSelectedDns.HostName);
                }
                else
                {
                    result = await App.DnsService.TestingService.PingHostAsync(CbSelectedDns.DnsAddress);
                }

                ShowTestResult(result, "PING TEST");
            }
            finally
            {
                BtnPing.IsEnabled = true;
                BtnPing.Content = "📡 Ping";
            }
        }

        /// <summary>
        /// DNS resolution test button click handler
        /// </summary>
        private async void BtnTestDns_Click(object sender, RoutedEventArgs e)
        {
            if (CbSelectedDns == null) return;

            BtnTestDns.IsEnabled = false;
            BtnTestDns.Content = "⏳ Testing...";

            try
            {
                var result = await App.DnsService.TestingService.ComprehensiveTestAsync(CbSelectedDns);
                ShowTestResult(result, "COMPREHENSIVE DNS TEST");

                // Update DNS item with validation status
                if (result.IsSuccessful)
                {
                    CbSelectedDns.IsValidated = true;
                    CbSelectedDns.LastTestedAt = result.TestedAt;
                    CbSelectedDns.ResponseTimeMs = result.ResponseTimeMs;
                    App.DnsService.SaveDnsItems();
                    ComboBoxItems_SelectionChanged(null, null); // Refresh display
                }
            }
            finally
            {
                BtnTestDns.IsEnabled = true;
                BtnTestDns.Content = "🧪 Test DNS";
            }
        }

        /// <summary>
        /// URL test button click handler
        /// </summary>
        private async void BtnTestUrl_Click(object sender, RoutedEventArgs e)
        {
            if (CbSelectedDns == null) return;

            BtnTestUrl.IsEnabled = false;
            BtnTestUrl.Content = "⏳ Testing...";

            try
            {
                var result = await App.DnsService.TestingService.TestDnsWithUrlAsync("https://www.google.com", 5000);
                ShowTestResult(result, "URL CONNECTIVITY TEST");
            }
            finally
            {
                BtnTestUrl.IsEnabled = true;
                BtnTestUrl.Content = "🌐 Test URL";
            }
        }

        /// <summary>
        /// Display test results in the UI
        /// </summary>
        private void ShowTestResult(DnsTestingService.DnsTestResult result, string testName)
        {
            BorderTestResults.Visibility = Visibility.Visible;
            LabelTestResult.Text = $"{testName} - {result.TestedAt:HH:mm:ss}";

            var displayText = new StringBuilder();
            displayText.AppendLine(result.Message);
            if (result.ResponseTimeMs > 0)
                displayText.AppendLine($"Response Time: {result.ResponseTimeMs}ms");
            if (!string.IsNullOrWhiteSpace(result.ErrorDetails))
                displayText.AppendLine($"Details: {result.ErrorDetails}");

            TextBlockTestOutput.Text = displayText.ToString();

            // Color code the result
            if (result.IsSuccessful)
            {
                BorderTestResults.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC8E6C9"));
                BorderTestResults.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF81C784"));
                LabelTestResult.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0B5D00"));
            }
            else
            {
                BorderTestResults.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCDD2"));
                BorderTestResults.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE57373"));
                LabelTestResult.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC62828"));
            }
        }
    }
}
