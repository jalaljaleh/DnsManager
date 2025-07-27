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


        public DnsItem CurrentDNS { get => App.DnsService.Connected; }
        public bool IsAnyDnsConnected { get => CurrentDNS is null ? false : true; }
        public DnsItem SelectedDNS { get => (this.ComboBoxItems.SelectedItem as DnsItem); }

        void InitializeComboBox()
        {
            this.ComboBoxItems.Items.Clear();

            foreach (var item in App.DnsService.DnsItems.OrderBy(a => a.Priority))
                ComboBoxItems.Items.Add(item);


            if (IsAnyDnsConnected)
                ComboBoxItems.SelectedIndex = ComboBoxItems.Items.IndexOf(CurrentDNS);
            else
                ComboBoxItems.SelectedIndex = 0;
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

            var selected = SelectedDNS;
            if (selected == null) return;

            LabelDescription.Text =
                $"\t ----    Information    ----\n\n" +
                $"DNS Name:                               \n" +
                $" {selected.Name}\n\n" +
                $"DNS Address:                            \n" +
                $" {selected.DnsAddress}  &  {selected.DnsAddressAlt}\n\n" +
                $"DNS Description:                        \n" +
                $"{selected.Description}";

            var connected = CurrentDNS;

            bool isConnected = connected != null && connected == selected;

            if (isConnected)
            {
                BtnConnect.Content = "Disconnect";
                BtnConnect.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDB171"));

                LabelStatus.Content = $"🔒 Connected to {selected.Name}";
                LabelStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0B5D00"));
            }
            else
            {
                BtnConnect.Content = "Connect";
                BtnConnect.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF71D8FD"));

                LabelStatus.Content = $"🔓 Not Connected";
                LabelStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB5600"));
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
            if (SelectedDNS == null) return;

            var window = new Windows.Modals.AddNewDnsModalWindow(SelectedDNS);
            var result = window.ShowDialog();
            if (result.Value)
            {
                App.DnsService.DnsItems.Remove(SelectedDNS);
                App.DnsService.DnsItems.Add(window.Item);
                App.DnsService.SaveDnsItems();
                InitializeComboBox();
            }
        }
        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDNS == null) return;

            App.DnsService.DnsItems.Remove(SelectedDNS);
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


            if (this.SelectedDNS == null) return;

            NetworkManager.SetDNS(SelectedDNS.DnsAddress, SelectedDNS.DnsAddressAlt);

            App.DnsService.ChangeConnection(SelectedDNS);
            App.DnsService.SaveDnsItems();

            InitializeComboBox();

            //  MessageBox.Show($"DNS {item.Name} Connected Successfuly !");

        }





    }
}
