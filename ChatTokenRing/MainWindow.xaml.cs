﻿using System;
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
            this.ResizeMode = System.Windows.ResizeMode.CanMinimize;
        }

        private void buttonConnection_Click(object sender, RoutedEventArgs e)
        {
            if ((comboBox.SelectedItem != null) && (comboBox1.SelectedItem != null) && (textBoxUserName.Text != "") && (textBoxUserName.Text != "Введите имя пользователя"))
            {
                if (comboBox.SelectedItem.ToString() == comboBox1.SelectedItem.ToString())
                {
                    MessageBox.Show("Выберите различные COM-порты и повторите попытку", "Ошибка соединения", MessageBoxButton.OK);
                    return;
                }
                else
                {
                    string outcomePort = comboBox.SelectedItem.ToString();
                    string incomePort = comboBox1.SelectedItem.ToString();
                    chatWindow = new Chat();
                    if (D.IsChecked == true)
                    {
                        chatWindow.Title = "Чат (Вы вошли как: " + textBoxUserName.Text + ") Ведущая станция";
                    }
                    else
                    {
                        chatWindow.Title = "Чат (Вы вошли как: " + textBoxUserName.Text + ")";
                    }
                    chatWindow.ResizeMode = System.Windows.ResizeMode.CanMinimize;
                    DataLinkLayer.OpenConnection(incomePort, outcomePort, (bool)D.IsChecked, textBoxUserName.Text.Trim(new char[] { '*' }));
                    Application.Current.MainWindow.Hide();
                    chatWindow.Show();
                }
            }
            else
            {
                if ((comboBox.SelectedItem == null) || (comboBox1.SelectedItem == null))
                {
                    MessageBox.Show("Выберите COM-порты и повторите попытку", "Ошибка соединения", MessageBoxButton.OK);
                    return;
                }
                else
                {
                    MessageBox.Show("Введите имя пользователя и повторите попытку", "Ошибка соединения", MessageBoxButton.OK);
                    return;
                }
            }
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

        private void textBoxUserName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxUserName.Text == "")
            {
                textBoxUserName.Text = "Введите имя пользователя";
            }
        }

        private void textBoxUserName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxUserName.Text == "Введите имя пользователя")
            {
                textBoxUserName.Text = "";
            }
        }
    }
}
