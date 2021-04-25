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
        static System.Windows.Controls.ListBox lb1;
        static Chat ths;
        static bool checkExit = false;
        static Dictionary<byte, string> ul;
        static Dictionary<string, ListBox> lkl;
        static Stack<ListBox> stuck;
        static byte? usArd;
        static StackPanel panel;

        public Chat()
        {
            lkl = new Dictionary<string, ListBox>();
            lkl.Add("Общий", new ListBox());
            lkl["Общий"].Height = 220;
            InitializeComponent();
            listBox1.Items.Add("Общий");
            listBox1.SelectedItem = listBox1.Items[0];
            panelka.Children.Add(lkl["Общий"]);
            panel = panelka;
            lb1 = listBox1;
            ths = this;
            ul = new Dictionary<byte, string>();
            ListBox z1 = new ListBox();
            ListBox z2 = new ListBox();
            ListBox z3 = new ListBox();
            ListBox z4 = new ListBox();
            ListBox z5 = new ListBox();
            stuck = new Stack<ListBox>();
            stuck.Push(z1);
            stuck.Push(z2);
            stuck.Push(z3);
            stuck.Push(z4);
            stuck.Push(z5);
        }

        public static void List(Dictionary<byte, string> userLists, byte? userAddress)
        {
            usArd = userAddress;
            ul = userLists;
            bool ckeck = false;
            foreach (byte b in ul.Keys)
            {
                ckeck = false;
                foreach (var item in lb1.Items)
                {
                    if ((string)item == ul[b])
                    {
                        ckeck = true;
                        break;
                    }
                }
                if (!ckeck && lb1.Items[0].ToString().Contains("*") == false)
                {
                    lkl.Add(ul[b], stuck.Pop());
                    lb1.Dispatcher.Invoke(() =>
                    {
                        lb1.Items.Add(ul[b]);
                    });
                }
            }
        }

        public static void inMessage(string message, byte userAdress, byte addressKydanado)
        {
            string username = "";
            string username1 = "";
            foreach (byte b in ul.Keys)
            {
                if (b == userAdress)
                {
                    username = ul[b];
                    break;
                }
            }
            if (addressKydanado == 0x7F)
            {
                username1 = "Общий";
            }
            else
            {
                username1 = username;
            }
            lkl[username1].Dispatcher.Invoke(() =>
            {
                if (lkl[username1] != panel.Children[0])
                {
                    foreach (var lbl in lb1.Items)
                    {

                        if ((string)lbl == username1)
                        {
                            //Style styl = new Style();
                            //styl.Setters.Add(new Setter { Property=Control.ForegroundProperty, Value=new SolidColorBrush(Colors.Red)});
                            //lb1.ItemContainerStyle = styl;
                            //FontStyle fontStyle = lb1.FontStyle;
                            //fontStyle
                            lb1.Items.Remove(lbl);
                            lb1.Items.Insert(0, lbl + "*");
                            //Style stol = new Style();
                            //stol.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = new SolidColorBrush(Colors.Black) });
                            //lb1.ItemContainerStyle = stol;
                            break;
                        }
                    }
                }
            });
            //Thread.Sleep(50);
            lkl[username1].Dispatcher.Invoke(() => { lkl[username1].Items.Add(DateTime.Now.ToString("HH:mm") + " " + username + ": " + message); });
            //lb.Dispatcher.Invoke(() => { lb.Items.Add(DateTime.Now.ToString("HH:mm") + " " + username + ": " + message); });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            byte? address = null;
            //string user = lb1.SelectedItem.ToString();//.Trim(new char[] { '*' });
            string massege = textBox.Text;
            foreach (var l in lkl)
            {
                if (l.Value == panelka.Children[0])
                {
                    if (l.Key == "Общий")
                    {
                        address = 0x7F;
                    }
                    else
                    {
                        foreach (var p in ul)
                        {
                            if (l.Key == p.Value)
                            {
                                address = p.Key;
                                break;
                            }
                        }
                    }
                    if (address != 0x7F && address != usArd)
                    {
                        l.Value.Items.Add(DateTime.Now.ToString("HH:mm") + " Вы: " + massege);
                    }
                    break;
                }
            }
            DataLinkLayer.SendMessage(address, massege);
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
                button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
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

        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string user = lb1.SelectedItem.ToString().Trim(new char[] { '*' });
            string user1 = lb1.SelectedItem.ToString();
            panelka.Children.Clear();
            lkl[user].Height = 220;
            panelka.Children.Add(lkl[user]);

            lb1.Items.Remove(user1);
            lb1.Items.Insert(0, user);
        }

        private void listBox1_Initialized(object sender, EventArgs e)
        {

        }

        private void panelka_Initialized(object sender, EventArgs e)
        {

        }
    }
}
