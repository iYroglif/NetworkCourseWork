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
            if(message=="Соединение разорвано")
            {
                MessageBox.Show("Соединение разорвано");
                Application.Current.MainWindow.Show();
            }
            listBox.Items.Add(message);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string massege = textBox.Text;
            DataLinkLayer.SendMessage(0x7F, DateTime.Now.ToString("HH:mm:ss") +": "+massege);
            textBox.Clear();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DataLinkLayer
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "")
            {
                textBox.Text = "Введите сообщение";
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
            {
                string massege = textBox.Text;
                DataLinkLayer.SendMessage(0x7F, DateTime.Now.ToString("HH:mm:ss") + ": " + massege);
                textBox.Clear();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Подтверждение закрытия", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
                string logout = "Соединение разорвано";
                DataLinkLayer.SendMessage(0x7F, logout);
                //DataLinkLayer.CloseConnection();
                Application.Current.MainWindow.Show();
            }
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "Введите сообщение")
            {
                textBox.Text = "";
            }
        }
    }
}
