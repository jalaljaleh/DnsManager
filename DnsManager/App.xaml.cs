using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DnsManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Version = "1.2v";
        public static string DirectoryPath { get; set; }
        public static DnsService DnsService { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DirectoryPath = System.IO.Directory.GetCurrentDirectory();
            
            DnsService = new DnsService();

            var mainWinodw = new Windows.MainWindow();
            mainWinodw.Show();
        }
    }
}
