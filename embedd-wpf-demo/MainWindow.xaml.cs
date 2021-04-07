using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using Openfin.Desktop.Messaging;
using Fin = Openfin.Desktop;
    
namespace embedd_wpf_demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string RuntimeVersion = "19.89.57.15";
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
                RuntimeConnectTimeout = 20000
            };

            var fin = Fin.Runtime.GetRuntimeInstance(runtimeOptions);

            fin.Error += (sender, e) =>
            {
                Console.Write(e);
            };

            fin.Connect(() => 
            {
                // Initialize the communication channel after the runtime has connected
                // but before launching any applications or EmbeddedViews

                var channelProvider = fin.InterApplicationBus.Channel.CreateProvider(ChannelName);
                channelProvider.RegisterTopic<string,bool>(SelectionChangeTopic, OnSelectionChanged);
                channelProvider.ClientConnected += ChannelProvider_ClientConnected;
                channelProvider.OpenAsync().Wait();
            });

            //Initialize the grid view by passing the runtime Options and the ApplicationOptions
            var fileUri = new Uri(System.IO.Path.GetFullPath(@"..\..\web-content\index.html")).ToString();
            OpenFinEmbeddedView.Initialize(runtimeOptions, new Fin.ApplicationOptions(AppName,  AppUuid, fileUri));

            //Once the grid is ready get the data and populate the list box.
            OpenFinEmbeddedView.Ready += (sender, e) =>
            {
                //set up the data
                var peopleInStates = (from person in peopleData
                                      group person by person.BirthState into stateGroup
                                      select new
                                      {
                                          StateName = stateGroup.First().BirthState,
                                          People = stateGroup
                                      })
                                      .OrderBy(p => p.StateName)
                                      .ToList();

                //Any Interactions with the UI must be done in the right thread.
                Dispatcher.Invoke(new Action(() =>
                {
                    peopleInStates.ForEach(state => StatesBox.Items.Add(state.StateName));
                }), null);
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

        private bool OnSelectionChanged(string selection)
        {
            Dispatcher.Invoke(new Action(() => { SelectedValue.Text = selection; }));
            return true;
        }

        private void States_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //On State selection we will send the data to the grid.
            var data = (from person in peopleData
                        where StatesBox.SelectedItems.Contains(person.BirthState)
                        select person).ToList();

            channelClient?.DispatchAsync(DataChangeTopic, data);
        }
    }
}
