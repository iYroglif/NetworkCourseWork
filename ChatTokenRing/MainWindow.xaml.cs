using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ChatTokenRing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        public Chat chatWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Setup_Load(object sender, EventArgs e)
        {
            // Показываем список COM-портов.
            string[] portNames = 
            foreach (string portName in portNames)
            {
                comboBox.Items.Add(portName);
                comboBox1.Items.Add(portName);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ///DataLinkLayer.OpenConnection(textBoxUserName.Text);
            chatWindow = new Chat();
        }

        private void D_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void buttonConnection_Click(object sender, RoutedEventArgs e)
        {
            //if((outcomePort.Selected))
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string[] portNames = NetworkService.GetSharedService().GetPortsNames();
            //foreach (string portName in portNames)
            //{
            //    outcomePortBox.Items.Add(portName);
            //}
        }

        private void comboBox1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            //string[] portNames = NetworkService.GetSharedService().GetPortsNames();
            //foreach (string portName in portNames)
            //{
            //    incomePortBox.Items.Add(portName);
            //}
        }

        private void comboBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }
    }
}
