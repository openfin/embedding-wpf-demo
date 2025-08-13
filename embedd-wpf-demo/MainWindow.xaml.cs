using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using Openfin.Desktop;
using Openfin.Desktop.Messaging;
using Fin = Openfin.Desktop;

    
namespace embedd_wpf_demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        //const string RuntimeVersion = "38.126.83.86";
        //const string RuntimeVersion = "40.130.100.7";
        const string RuntimeVersion = "40.130.105.1";
        const string AppUuid = "hyper-grid-uuid";
        const string AppName = "hyper-grid";
        const string ChannelName = "user-data";
        const string DataChangeTopic = "data-updated";
        const string SelectionChangeTopic = "selection-changed";

        List<Person> peopleData;

        Fin.Messaging.ChannelClient channelClient;

        public MainWindow()
        {
            InitializeComponent();
            //Runtime options is how we set up the OpenFin Runtime environment
            peopleData = PeopleData.Get();

            var runtimeOptions = new Fin.RuntimeOptions
            {
                Version = RuntimeVersion,
                EnableRemoteDevTools = true,
                RuntimeConnectTimeout = 50000,
                Arguments = "--inspect-brk",
            };

            var fin = Fin.Runtime.GetRuntimeInstance(runtimeOptions);

            fin.Error += (sender, e) =>
            {
                Console.Write(e);
            };

            fin.Connect(() => 
            {
                Debug.WriteLine("Connected!");
            });

            //Initialize the grid view by passing the runtime Options and the ApplicationOptions
            var fileUri = new Uri("http://www.google.co.uk");
            var options = new Fin.ApplicationOptions(AppName, AppUuid, fileUri.ToString());
            options.ContentNavigationRules = ContentNavigation.CreateBlacklist();           
            OpenFinEmbeddedView.Initialize(runtimeOptions, options);

            //Once the grid is ready get the data and populate the list box.
            OpenFinEmbeddedView.Ready += (sender, e) =>
            {
                Debug.WriteLine("Ready!");
            };
        }

        private void ChannelProvider_ClientConnected(object sender, ChannelConnectedEventArgs e)
        {
            channelClient = e.Client;

            // There is currently a bug that requires this task to occur on a different
            // thread than the connected event. Fixed in later versions.
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                channelClient?.DispatchAsync(DataChangeTopic, peopleData);
            });
        }
    }
}
