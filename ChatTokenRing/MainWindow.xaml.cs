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
            if ((comboBox.SelectedItem != null)&&(comboBox1.SelectedItem != null))
            {
                string incomePort = comboBox.SelectedItem.ToString();
                string outcomePort = comboBox1.SelectedItem.ToString();

                DataLinkLayer.OpenConnection(incomePort, outcomePort, (bool)D.IsChecked, textBoxUserName.Text);

                chatWindow = new Chat();
                chatWindow.Show();
            }
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

        private void comboBox_Initialized(object sender, EventArgs e)
        {
            string[] portNames = Connection.GetPortsNames();
            foreach (string portName in portNames)
            {
                comboBox.Items.Add(portName);
            }
        }

        private void comboBox1_Initialized(object sender, EventArgs e)
        {
            string[] portNames = Connection.GetPortsNames();
            foreach (string portName in portNames)
            {
                comboBox1.Items.Add(portName);
            }
        }
    }
}
