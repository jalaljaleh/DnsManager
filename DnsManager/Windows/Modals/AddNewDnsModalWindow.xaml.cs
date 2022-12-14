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
            if (item != null)
            {
                TextBoxName.Text = item.Name;
                TextBoxDns.Text = item.DnsAddress;
                TextBoxDnsAlt.Text = item.DnsAddressAlt;
            }
            this.BtnAdd.Click += BtnAdd_Click;
            this.BtnCancel.Click += BtnCancel_Click;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Item = new DnsItem()
            {
                Name = TextBoxName.Text,
                DnsAddress = TextBoxDns.Text,
                DnsAddressAlt = TextBoxDnsAlt.Text
            };
            this.DialogResult = true;
            this.Close();
        }
    }
}
