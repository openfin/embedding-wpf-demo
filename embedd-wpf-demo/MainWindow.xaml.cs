using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
    
namespace embedd_wpf_demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string version = "stable";
        List<Person> peopleData;
        MessageChannel dataMessageChannel;

        public MainWindow()
        {
            InitializeComponent();
            //Runtime options is how we set up the OpenFin Runtime environment

            var runtimeOptions = new Openfin.Desktop.RuntimeOptions
            {
                Version = version,
                EnableRemoteDevTools = true,
                RemoteDevToolsPort = 9090
            };

            var runtime = Openfin.Desktop.Runtime.GetRuntimeInstance(runtimeOptions);

            runtime.Error += (sender, e) =>
            {
                Console.Write(e);
            };

            runtime.Connect(() => 
            {
                // Initialize the communication channel after the runtime has connected
                // but before launching any applications or EmbeddedViews
                dataMessageChannel = new MessageChannel(runtime.InterApplicationBus, "hyper-grid-uuid", "user-data");
            });

            //Initialize the grid view by passing the runtime Options and the ApplicationOptions
            var fileUri = new Uri(System.IO.Path.GetFullPath(@"..\..\web-content\index.html")).ToString();
            OpenFinEmbeddedView.Initialize(runtimeOptions, new Openfin.Desktop.ApplicationOptions("hyper-grid", "hyper-grid-uuid", fileUri));

            //Once the grid is ready get the data and populate the list box.
            OpenFinEmbeddedView.Ready += (sender, e) =>
            {
                //set up the data
                peopleData = PeopleData.Get();
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
                Openfin.WPF.Utils.InvokeOnUiThreadIfRequired(this, () => peopleInStates.ForEach(state => StatesBox.Items.Add(state.StateName)));

                var t = new System.Threading.Thread(() =>
                {
                    dataMessageChannel.SendData(peopleData);
                });
                t.Start();
            };
        }

        private void States_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //On State selection we will send the data to the grid.
            var data = (from person in peopleData   
                       where StatesBox.SelectedItems.Contains(person.BirthState)
                        select person).ToList();

            dataMessageChannel.SendData(data);
        }
    }
}
