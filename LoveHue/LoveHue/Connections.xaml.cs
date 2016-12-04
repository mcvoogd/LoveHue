using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LoveHue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Connections : Page
    {

        public Connections()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            List<Connection> ListOfConnections = new List<Connection>();
            Connection connection = new Connection("1234", "Home", "axc");
            Lamp lamp1 = new Lamp(1, "Lamp1");
            Lamp lamp2 = new Lamp(2, "Lamp2");
            List<Lamp> listlamps = new List<Lamp>();
            listlamps.Add(lamp1);
            listlamps.Add(lamp2);
            Color c = new Color();
            c = Color.FromArgb(1,30,180,225);
            connection.AddSection(new Section("Woonkamer", listlamps, c));
            ListOfConnections.Add(connection);
            ListOfConnections.Add(new Connection("1234", "School", "axc"));
            ListOfConnections.Add(new Connection("1234", "Work", "axc"));
            GroupOfConnections.ItemsSource = ListOfConnections;
        }

        private void GroupOfConnections_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Select!");
            this.Frame.Navigate(typeof(MainPage), e.AddedItems[0]);
        }

        private void GroupOfConnections_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("Select!");
            this.Frame.Navigate(typeof(MainPage), e.OriginalSource);
        }
    }
}
