using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Openfin.WinForm;

namespace embedd_wpf_demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string version = "alpha";
        public MainWindow()
        {
            InitializeComponent();
            var runtimeOptions = new Openfin.Desktop.RuntimeOptions
            {
                Version = version,
                EnableRemoteDevTools = true,
                RemoteDevToolsPort = 9090
            };
            OpenFinControl.Initialize(runtimeOptions, new Openfin.Desktop.ApplicationOptions("of-chart-tab", "of-chart-tab", "http://cdn.openfin.co/embed-web/chart.html"));
        }
    }
}
