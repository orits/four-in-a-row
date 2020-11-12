using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WcfFourRowServiceLibrary;

namespace FourRowGameHost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// that class is represent the physical server the of type "WcfFourRowServiceLibrary" server.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        ServiceHost _host;

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _host = new ServiceHost(typeof(FourRowService));

                _host.Description.Behaviors.Add(
                    new ServiceMetadataBehavior { HttpGetEnabled = true });

                _host.Open(); // run the "WcfFourRowServiceLibrary" server.
                ServiceLabel.Content = "The Service: FourRowGameService is running!";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
