using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            App.DnsService.OnDnsListChanged += DnsService_OnDnsListChanged;
            this.BtnSet.Click += BtnSet_Click;
            this.BtnUnset.Click += BtnUnset_Click;
            this.BtnRemove.Click += BtnRemove_Click;
            this.BtnSaveChanges.Click += BtnSaveChanges_Click;
            this.BtnAddNewDns.Click += BtnAddNewDns_Click;
            this.BtnEdit.Click += BtnEdit_Click;
            this.BtnUndoChanges.Click += BtnUndoChanges_Click;
            this.ComboBoxItems.SelectionChanged += ComboBoxItems_SelectionChanged;
            Initialize();
        }

        private void ComboBoxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (this.ComboBoxItems.SelectedItem as DnsItem);
            if (item == null) return;
            this.LabelSelectedDnsInfo.Text = item.DnsAddress.PadRight(12) + item.DnsAddressAlt;
        }

        void Initialize()
        {
            var list = App.DnsService.LoadDnsItems();
            if (list == null)
            {
                list = new List<DnsItem>()
                {
                    new DnsItem()
                    {
                        Name="Google",
                        DnsAddress="8.8.8.8",
                        DnsAddressAlt="8.8.4.4"
                    },
                    new DnsItem()
                    {
                        Name="Quad9",
                        DnsAddress="9.9.9.9",
                        DnsAddressAlt="149.112.112.112"
                    },
                    new DnsItem()
                    {
                        Name="Cloudflare",
                        DnsAddress="1.1.1.1",
                        DnsAddressAlt="1.0.0.1"
                    }
                };
                App.DnsService.SaveDnsItems(list);
            }
            App.DnsService.DnsItems = list;
            App.DnsService.InvokeEvent();
        }
        private void BtnUndoChanges_Click(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var item = (this.ComboBoxItems.SelectedItem as DnsItem);
            if (item == null) return;

            var window = new Windows.Modals.AddNewDnsModalWindow(item);
            var result = window.ShowDialog();
            if (result.Value)
            {
                App.DnsService.RemoveDns(item);
                App.DnsService.AddDns(window.Item);

                ComboBoxItems.SelectedItem = window.Item;
                return;
            }
        }
        void SetSelectedToTheLastItem()
        {
            ComboBoxItems.SelectedIndex = ComboBoxItems.Items.Count - 1;

        }
        private void BtnAddNewDns_Click(object sender, RoutedEventArgs e)
        {
            var window = new Windows.Modals.AddNewDnsModalWindow();
            var result = window.ShowDialog();
            if (result.Value)
            {
                App.DnsService.AddDns(window.Item);
                SetSelectedToTheLastItem();
                return;
            }
        }

        private void BtnUnset_Click(object sender, RoutedEventArgs e)
        {
            NetworkManager.UnsetDNS();
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            App.DnsService.SaveDnsItems(App.DnsService.DnsItems);
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var item = (this.ComboBoxItems.SelectedItem as DnsItem);
            if (item == null) return;

            App.DnsService.RemoveDns(item);
            SetSelectedToTheLastItem();
        }

        private void BtnSet_Click(object sender, RoutedEventArgs e)
        {
            var item = (this.ComboBoxItems.SelectedItem as DnsItem);
            if (item == null) return;

            NetworkManager.SetDNS(item.DnsAddress, item.DnsAddressAlt);
        }

        private void DnsService_OnDnsListChanged(object sender, DnsService.DnsListChangedEventArgs e)
        {
            this.ComboBoxItems.Items.Clear();
          e.UpdatedList.ForEach(x=>this.ComboBoxItems.Items.Add(x));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://jalaljaleh.github.io/");
        }
    }
}
