using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LoveHue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Connection _connection;
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _connection = (Connection)e.Parameter;
            if(_connection != null)
                GroupOfSections.ItemsSource = _connection.GetSectionList();

        }

        private void GroupOfSections_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Click");
            this.Frame.Navigate(typeof(Section), e.AddedItems[0]);
        }

        private void GroupOfSections_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("Tap");
            this.Frame.Navigate(typeof(Sections), e.OriginalSource);
        }
    }
}
