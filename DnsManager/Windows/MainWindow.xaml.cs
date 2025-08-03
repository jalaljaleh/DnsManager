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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DnsManager.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.BtnAdd.Click += BtnAddNewDns_Click;
            this.BtnRemove.Click += BtnRemove_Click;
            this.BtnConnect.Click += BtnApply_Click;
            this.BtnEdit.Click += BtnEdit_Click;

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
        }
        void OnDisconnected()
        {
            LabelStatus.Content = $"🔓 Not Connected";
            LabelStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB5600"));
            borderStatus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFEEB3"));
        }
        private void ComboBoxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!App.DnsService.DnsItems.Any())
            {
                BtnAdd.IsEnabled = true;
                BtnEdit.IsEnabled = false;
                BtnConnect.IsEnabled = false;
                BtnRemove.IsEnabled = false;

                LabelStatus.Content = "⚠️ No Server Found, Add Servers!";
                LabelStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB5600"));
                LabelDescription.Text = "No Server Information";

                BtnConnect.Content = "Add DNS";
                return;
            }

            if (CbSelectedDns == null) return;

            LabelDescription.Text =
                $"\n" +
                $"DNS Name:                               \n" +
                $" {CbSelectedDns.Name}\n\n" +
                $"DNS Address:                            \n" +
                $" {CbSelectedDns.DnsAddress}  &  {CbSelectedDns.DnsAddressAlt}\n\n" +
                $"DNS Description:                        \n" +
                $"{CbSelectedDns.Description}";


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
                //   MessageBox.Show($"DNS Disconnected Successfuly !");

            }


            if (this.CbSelectedDns == null) return;

            NetworkManager.SetDNS(CbSelectedDns.DnsAddress, CbSelectedDns.DnsAddressAlt);

            App.DnsService.ChangeConnection(CbSelectedDns);
            App.DnsService.SaveDnsItems();

            InitializeComboBox();

            //  MessageBox.Show($"DNS {item.Name} Connected Successfuly !");

        }





    }
}
