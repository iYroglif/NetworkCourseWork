using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        static System.Windows.Controls.ListBox lb;
        static Chat ths;
        static bool checkExit = false;
        static Dictionary<byte, string> ul;

        public Chat()
        {
            InitializeComponent();
            lb = listBox;
            ths = this;
            ul = new Dictionary<byte, string>();
        }

        public static void List(Dictionary<byte, string> userLists)
        {
            ul = userLists;
        }

        public static void inMessage(string message, byte userAdress)
        {
            string username = "";
            foreach (byte b in ul.Keys)
            {
                if (b == userAdress)
                {
                    username = ul[b];
                }
            }
            Thread.Sleep(200);
            lb.Dispatcher.Invoke(() => { lb.Items.Add(DateTime.Now.ToString("HH:mm") +" "+ username + ": " + message); });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string massege = textBox.Text;
            DataLinkLayer.SendMessage(0x7F, massege);
            textBox.Clear();
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
            if (e.Key == Key.Enter)
            {
                string massege = textBox.Text;
                DataLinkLayer.SendMessage(0x7F, massege);
                textBox.Clear();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (checkExit)
            {
                Application.Current.MainWindow.Show();
                checkExit = false;
            }
            else
            {
                if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Подтверждение закрытия", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                    DataLinkLayer.CloseConnection();
                    Application.Current.MainWindow.Show();
                }
            }
        }

        public static void exit()
        {
            if (MessageBox.Show("Другой пользователь вышел из программы, разрыв соединения", "Разрыв соединения", MessageBoxButton.OK) == MessageBoxResult.OK)
            {
                ths.Dispatcher.Invoke(() => { checkExit = true; ths.Close(); });
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
