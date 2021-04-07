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
using System.Windows.Shapes;

namespace ChatTokenRing
{
    /// <summary>
    /// Логика взаимодействия для Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        public Chat()
        {
            InitializeComponent();
            DataLinkLayer.SetChat(listBox);
        }

        public void inMessage(string message)
        {
            listBox.Items.Add(message);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //258 datalink
            string massege = textBox.Text;
            DataLinkLayer.SendMessage(0x7F, massege);
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DataLinkLayer
        }
    }
}
