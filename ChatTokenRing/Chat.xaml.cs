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
        static System.Windows.Controls.ListBox listBoxListOfUserToDisplay;
        static Chat ths;
        static bool checkExit = false;
        static Dictionary<byte, string> dictionaryWithListOfUser;
        static Dictionary<string, ListBox> dictionaryWithListBox;
        static Stack<ListBox> stackOfListBox;
        static byte? thisUserAddress;
        static StackPanel staticVariableStackPanel;
        static Ellipse connectionStatus;
        static TextBox messageInputer;
        static Button buttonToSend;

        public Chat()
        {
            dictionaryWithListBox = new Dictionary<string, ListBox>();
            dictionaryWithListBox.Add("Общий", new ListBox());
            dictionaryWithListBox["Общий"].Height = 220;
            InitializeComponent();
            listBox1.Items.Add("Общий");
            listBox1.SelectedItem = listBox1.Items[0];
            StackPanel.Children.Add(dictionaryWithListBox["Общий"]);
            staticVariableStackPanel = StackPanel;
            listBoxListOfUserToDisplay = listBox1;
            messageInputer = textBox;
            buttonToSend = button;
            ths = this;
            connectionStatus = ellipse;
            dictionaryWithListOfUser = new Dictionary<byte, string>();
            stackOfListBox = new Stack<ListBox>();
            for (int i = 0; i < 20; i++)
            {
                stackOfListBox.Push(new ListBox());
            }
        }

        public static void List(Dictionary<byte, string> userLists, byte? userAddress)
        {
            thisUserAddress = userAddress;
            dictionaryWithListOfUser = userLists;
            bool ckeck = false;
            foreach (byte b in dictionaryWithListOfUser.Keys)
            {
                ckeck = false;
                foreach (var item in listBoxListOfUserToDisplay.Items)
                {
                    if (item.ToString().TrimEnd(new char[] { '*' }) == dictionaryWithListOfUser[b])
                    {
                        ckeck = true;
                        break;
                    }
                }
                if (!ckeck && !listBoxListOfUserToDisplay.Items[0].ToString().Contains("*"))
                {
                    dictionaryWithListBox.Add(dictionaryWithListOfUser[b], stackOfListBox.Pop());
                    listBoxListOfUserToDisplay.Dispatcher.Invoke(() =>
                    {
                        listBoxListOfUserToDisplay.Items.Add(dictionaryWithListOfUser[b]);
                    });
                    break;
                }
            }
        }

        public static void inMessage(string message, byte userAdress, byte addressKydanado)
        {
            string username = "";
            string username1 = "";
            foreach (byte b in dictionaryWithListOfUser.Keys)
            {
                if (b == userAdress)
                {
                    username = dictionaryWithListOfUser[b];
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
            dictionaryWithListBox[username1].Dispatcher.Invoke(() =>
            {
                if (dictionaryWithListBox[username1] != staticVariableStackPanel.Children[0])
                {
                    foreach (var lbl in listBoxListOfUserToDisplay.Items)
                    {

                        if (lbl.ToString() == username1)
                        {
                            listBoxListOfUserToDisplay.Items.Remove(lbl);
                            listBoxListOfUserToDisplay.Items.Insert(0, lbl + "*");
                            break;
                        }
                    }
                }
            });
            dictionaryWithListBox[username1].Dispatcher.Invoke(() => { dictionaryWithListBox[username1].Items.Add(DateTime.Now.ToString("HH:mm") + " " + username + ": " + message); });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            byte? UserAddressToSendToDataLinkLayer = null;
            string messageToSendToDataLinkLayer = textBox.Text;
            foreach (var variableToIteratedictionaryWithListBox in dictionaryWithListBox)
            {
                if (variableToIteratedictionaryWithListBox.Value == StackPanel.Children[0])
                {
                    if (variableToIteratedictionaryWithListBox.Key == "Общий")
                    {
                        UserAddressToSendToDataLinkLayer = 0x7F;
                    }
                    else
                    {
                        foreach (var variableToIteratedictionaryWithListOfUser in dictionaryWithListOfUser)
                        {
                            if (variableToIteratedictionaryWithListBox.Key == variableToIteratedictionaryWithListOfUser.Value)
                            {
                                UserAddressToSendToDataLinkLayer = variableToIteratedictionaryWithListOfUser.Key;
                                break;
                            }
                        }
                    }
                    if (UserAddressToSendToDataLinkLayer != 0x7F && UserAddressToSendToDataLinkLayer != thisUserAddress)
                    {
                        variableToIteratedictionaryWithListBox.Value.Items.Add(DateTime.Now.ToString("HH:mm") + " Вы: " + messageToSendToDataLinkLayer);
                    }
                    break;
                }
            }
            DataLinkLayer.SendMessage(UserAddressToSendToDataLinkLayer, messageToSendToDataLinkLayer);
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
            connectionStatus.Dispatcher.Invoke(() => { connectionStatus.Fill = Brushes.Red; });
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
            string userWithSpecialCharacter = listBoxListOfUserToDisplay.SelectedItem.ToString().Trim(new char[] { '*' });
            string userWithoutSpecialCharacter = listBoxListOfUserToDisplay.SelectedItem.ToString();
            StackPanel.Children.Clear();
            dictionaryWithListBox[userWithSpecialCharacter].Height = 220;
            StackPanel.Children.Add(dictionaryWithListBox[userWithSpecialCharacter]);
            listBoxListOfUserToDisplay.Items.Remove(userWithoutSpecialCharacter);
            listBoxListOfUserToDisplay.Items.Insert(0, userWithSpecialCharacter);
            listBoxListOfUserToDisplay.SelectedItem = listBoxListOfUserToDisplay.Items[0];
        }

        private void listBox1_Initialized(object sender, EventArgs e)
        {

        }

        private void StackPanel_Initialized(object sender, EventArgs e)
        {

        }

        private void textBox_Initialized(object sender, EventArgs e)
        {
            textBox.MaxLength = 127;
        }

        public static void connectionWait()
        {
            connectionStatus.Dispatcher.Invoke(() => { connectionStatus.Fill = Brushes.Red; });
            messageInputer.Dispatcher.Invoke(() =>
            {
                messageInputer.IsReadOnly = true;
                messageInputer.Text = "Разрыв соединения, ожидайте восстановления соединения";
                buttonToSend.IsEnabled = false;
            });
            //textBox1.Text = "Разрыв соединения, ожидайте восстановления соединения";
        }

        public static void connectionRestored()
        {
            connectionStatus.Dispatcher.Invoke(() => { connectionStatus.Fill = Brushes.Green; });
            messageInputer.Dispatcher.Invoke(() =>
            {
                messageInputer.IsReadOnly = false;
                messageInputer.Text = "Введите сообщение";
                buttonToSend.IsEnabled = true;
            });
            //textBox1.Text = "Разрыв соединения, ожидайте восстановления соединения";
            //textBox1.IsReadOnly = false;
        }
    }
}
