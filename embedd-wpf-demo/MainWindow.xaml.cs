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
using Newtonsoft.Json.Linq;

namespace embedd_wpf_demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string version = "alpha";
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

            //Initialize the chart view by passing the runtime Options and the ApplicationOptions
            OpenFinControl.Initialize(runtimeOptions, new Openfin.Desktop.ApplicationOptions("hyper-grid", "hyper-grid-uuid", "http://cdn.openfin.co/embed-web-wpf/"));

            //We want to re-use the chart application and create a new window for it, lets wait until its ready.
            OpenFinControl.OnReady += (sender, e) =>
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
                invokeInUIThread(() => peopleInStates.ForEach(state => StatesBox.Items.Add(state.StateName)));

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
            OpenFinControl.OpenfinRuntime.InterApplicationBus.send("hyper-grid-uuid", "more-data", message);
        }

        //Any Interactions with the UI must be done in the right thread.
        private void invokeInUIThread(Action action)
        {
            Dispatcher.Invoke(action);
        }
    }
}
