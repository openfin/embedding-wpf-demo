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
        const string version = "beta";
        List<Person> peopleData;
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

            //Initialize the grid view by passing the runtime Options and the ApplicationOptions
            OpenFinEmbeddedView.Initialize(runtimeOptions, new Openfin.Desktop.ApplicationOptions("hyper-grid", "hyper-grid-uuid", "http://cdn.openfin.co/embed-web-wpf/index.html"));

            //Once the grid is ready get the data and populate the list box.
            OpenFinEmbeddedView.OnReady += (sender, e) =>
            {
                //set up the data
                peopleData = PeopleData.Get();
                var peopleInStates = (from person in peopleData
                                      group person by person.BirthState into stateGroup
                                      select new
                                      {
                                          StateName = stateGroup.First().BirthState,
                                          People = stateGroup
                                      }).ToList();
                
                //Any Interactions with the UI must be done in the right thread.
                Openfin.WPF.Utils.InvokeOnUiThreadIfRequired(this, () => peopleInStates.ForEach(state => StatesBox.Items.Add(state.StateName)));

                var t = new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.Sleep(2000);
                    //OpenFinEmbeddedView.OpenfinWindow.showDeveloperTools();
                    sendDataToGrid(peopleData);
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
            sendDataToGrid(data);
        }

        private void sendDataToGrid(List<Person> people)
        {
            //package the data and send it over the inter application bus
            var message = JObject.FromObject(new { data = people });
            OpenFinEmbeddedView.OpenfinRuntime.InterApplicationBus.send("hyper-grid-uuid", "more-data", message);
        }
    }
}
