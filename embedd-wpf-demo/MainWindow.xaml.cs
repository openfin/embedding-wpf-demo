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
            var runtimeOptions = new Openfin.Desktop.RuntimeOptions
            {
                Version = version,
                EnableRemoteDevTools = true,
                RemoteDevToolsPort = 9090
            };
            OpenFinControl.Initialize(runtimeOptions, new Openfin.Desktop.ApplicationOptions("hyper-grid-uuid", "hyper-grid", "http://cdn.openfin.co/embed-web-wpf/"));

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
                
                invokeInUIThread(() => peopleInStates.ForEach(state => StatesBox.Items.Add(state.StateName)));

            };
        }

        private void States_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var data = (from person in peopleData
                       where StatesBox.SelectedItems.Contains(person.BirthState)
                        select person).ToList();

            sendDataToGrid(data);
        }

        private void sendDataToGrid(List<Person> people)
        {
            var message = JObject.FromObject(new
            {
                data = people
            });

            OpenFinControl.OpenfinRuntime.InterApplicationBus.send("hyper-grid", "more-data", message);
        }

        private void invokeInUIThread(Action action)
        {
            Dispatcher.Invoke(action);
        }
    }
}
