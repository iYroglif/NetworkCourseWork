using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

namespace ChatTokenRing
{
    class Frame
    {
        /// <summary>
        /// Тип кадра
        /// </summary>
        public enum Type : byte
        {
            I,      // Информационный кадр
            Link,   // Кадр установки соединения
            Uplink, // Кадр разрыва соединения
            ACK,    // Кадр подтверждения безошибочного приема тестового сообщения
            Ret     // Кадр запроса повторения последнего отправленного кадра
        }

        public byte destination;            // Адрес получателя
        public byte departure;              // Адрес отправителя
        public Type type;                   // Тип кадра
        public byte? data_length = null;    // Длина поля данных
        public byte[] data = null;          // Данные

        public Frame(byte dep, Type type, byte? des = null, byte[] bytes = null)
        {
            departure = dep;
            this.type = type;
            switch (this.type)
            {
                case Type.I:
                    if (des == null)
                    {
                        // Ошибка: нет адреса получателя
                    }
                    else
                    {
                        destination = (byte)des;

                        if (bytes == null)
                        {
                            // Ошибка: нет данных
                        }
                        else
                        {
                            if (bytes.Length > 255)
                            {
                                // Ошибка: данные не помещаются в кадр
                            }
                            else
                            {
                                data_length = (byte)bytes.Length;
                                data = bytes;
                            }
                        }
                    }
                    break;

                case Type.Link:
                    if (des == null)
                    {
                        destination = 0x7F; // Адрес получателя широковещательный
                    }
                    else
                    {
                        if (des != 0x7F)
                        {
                            // Ошибка: адрес не широковещательный
                        }
                        else
                        {
                            destination = (byte)des;
                        }
                    }
                    if (bytes == null)
                    {
                        // Ошибка: нет данных
                    }
                    else
                    {
                        if (bytes.Length > 255)
                        {
                            // Ошибка: данные не помещаются в кадр
                        }
                        else
                        {
                            data_length = (byte)bytes.Length;
                            data = bytes;
                        }
                    }
                    break;

                case Type.Uplink:
                    if (des == null)
                    {
                        destination = 0x7F; // Адрес получателя широковещательный
                    }
                    else
                    {
                        if (des != 0x7F)
                        {
                            // Ошибка: адрес не широковещательный
                        }
                        else
                        {
                            destination = (byte)des;
                        }
                    }
                    if (bytes != null)
                    {
                        // Ошибка: есть какие-то данные
                    }
                    break;

                case Type.ACK:
                    if (des == null)
                    {
                        // Ошибка: нет адреса получателя
                    }
                    else
                    {
                        destination = (byte)des;

                        if (bytes != null)
                        {
                            // Ошибка: есть какие-то данные
                        }
                    }
                    break;

                case Type.Ret:
                    if (des == null)
                    {
                        // Ошибка: нет адреса получателя
                    }
                    else
                    {
                        destination = (byte)des;

                        if (bytes != null)
                        {
                            // Ошибка: есть какие-то данные
                        }
                    }
                    break;

                default:
                    // Ошибка: неверный тип кадра
                    break;
            }
        }

        public Frame() { }

        public bool TryConvertFromBytes(byte[] bytes)
        {
            if (bytes[0] != 0xFF) // Проверка на стартовый байт
            {
                // ошибка кадра?
                return false;
            }
            else
            {
                destination = bytes[1];
                departure = bytes[2];

                if (bytes[3] > 4)
                {
                    // Ошибка: недопустимый тип кадра
                    return false;
                }
                type = (Type)bytes[3];

                byte i = 0;
                if ((type == Type.I) || (type == Type.Link))
                {
                    data_length = bytes[4];

                    if (data_length > bytes.Length - 6)
                    {
                        // Длина данных больше чем массив байтов
                        return false;
                    }

                    data = new byte[(byte)data_length];
                    for (i = 0; i < data_length; ++i)
                    {
                        data[i] = bytes[i + 5];
                    }
                    ++i;
                }

                if (bytes[i + 4] != 0xFF) // Проверка на стоповый байт
                {
                    // ошибка кадра?
                    return false;
                }
                return true;
            }
        }

        public static explicit operator byte[](Frame frame)
        {
            byte[] bytes;
            if (frame.data == null)
            {
                bytes = new byte[5];
                bytes[0] = 0xFF; // Стартовый байт
                bytes[1] = frame.destination;
                bytes[2] = frame.departure;
                bytes[3] = (byte)frame.type;
                bytes[4] = 0xFF; // Стоповый байт
            }
            else
            {
                bytes = new byte[5 + (byte)frame.data_length + 1];
                bytes[0] = 0xFF; // Стартовый байт
                bytes[1] = frame.destination;
                bytes[2] = frame.departure;
                bytes[3] = (byte)frame.type;
                bytes[4] = (byte)frame.data_length;
                for (byte i = 0; i < bytes[4]; ++i)
                {
                    bytes[i + 5] = (byte)frame.data[i];
                }
                bytes[bytes[4] + 5] = 0xFF; // Стоповый байт
            }
            return bytes;
        }
    }

    abstract class DataLinkLayer
    {
        static byte? userAddress = null;
        static string userNickname;
        static Frame lastFrame;
        static System.Windows.Controls.ListBox listBox;

        /// <summary>
        /// Установка логического соединения
        /// </summary>
        static public void OpenConnection(string incomePortName, string outcomePortName, bool isMaster, string userName)
        {
            Connection.OpenPorts(incomePortName, outcomePortName, isMaster);
            // !!! Установка физического соединения
            userNickname = userName;// !!! Получение никнейма с пользовательского уровня (my)
            if (isMaster)
            {
                userAddress = 1;
                SendFrame(new Frame((byte)userAddress, Frame.Type.Link, bytes: Encoding.UTF8.GetBytes("[1, " + userNickname + ']')));
            }
        }

        /// <summary>
        /// Отправка кадра
        /// </summary>
        static public void SendFrame(Frame frame)
        {
            // ??? token ring
            lastFrame = frame;
            // !!! Отправка массива байтов на физический уровень (byte[])frame;
            Connection.SendBytes((byte[])frame);
        }

        static public void SendMessage(byte? des, string mes)
        {
            SendFrame(new Frame((byte)userAddress, Frame.Type.I, des, Encoding.UTF8.GetBytes(mes)));
        }

        /// <summary>
        /// Разъединение логического соединения
        /// </summary>
        static public void CloseConnection()
        {
            SendFrame(new Frame((byte)userAddress, Frame.Type.Uplink));
            // !!! Разрыв соединения на физическом уровне и/или выход из приложения на пользовательском
            Connection.ClosePorts();
        }

        static public void SetChat(System.Windows.Controls.ListBox lb)
        {
            listBox = lb;
        }

        /// <summary>
        /// Обработка пришедшего кадра
        /// </summary>
        static public void HandleFrame(byte[] bytes)
        {
            Frame frame = new Frame();
            if (!frame.TryConvertFromBytes(bytes))
            {
                SendFrame(new Frame((byte)userAddress, Frame.Type.Ret, des:/* ??? по идее соседний комп с ком порта потому что ошибки могут быть и с адресом отправителя */frame.departure));
                // Ошибка: нужен запрос на повторную отправку и повторная отправка
            }
            else
            {
                switch (frame.type)
                {
                    case Frame.Type.I:
                        if ((frame.destination == 0x7F) || (frame.destination == (byte)userAddress))
                        {
                            SendFrame(new Frame((byte)userAddress, Frame.Type.ACK, des: frame.departure));

                            //await Task.Run(() => (Application.Current.MainWindow as MainWindow).chatWindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length)));

                            /*Dispatcher.Invoke(async () => {

                                listBox.Items.Add(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length));

                                //(Application.Current.MainWindow as MainWindow).chatWindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length))
                            });*/
                                listBox.Dispatcher.Invoke((MethodInvoker)delegate
                            {
                                // Running on the UI thread
                                listBox.Items.Add(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length));
                            });

                            // !!! Передача сообщения на пользовательский уровень frame.data
                            if ((frame.destination == 0x7F) && (frame.departure != userAddress))
                            {
                                SendFrame(frame);
                            }
                        }
                        else
                        {
                            SendFrame(frame);
                        }
                        break;

                    case Frame.Type.Link:
                        Dictionary<byte, string> users = new Dictionary<byte, string>();

                        string[] items = Encoding.UTF8.GetString(frame.data, 0, frame.data.Length).Split(new string[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in items)
                        {
                            string[] tmp = item.Trim('[', ']').Split(',');
                            users.Add(Convert.ToByte(tmp[0]), tmp[1]);
                        }

                        // !!! Передача списка пользователей на пользовательский уровень (users-словарь пользователей, значение ключей-имена пользователей(string))
                        if (userAddress == null)
                        {
                            userAddress = (byte?)(users.Last().Key + 1);
                        }
                        if (!users.ContainsKey((byte)userAddress))
                        {
                            // ??? Может ли быть случай остутствия никнейма к этому моменту? userNickname = "nick2";// !!! Получение никнейма с пользовательского уровня
                            users.Add((byte)userAddress, userNickname);
                            frame.data = Encoding.UTF8.GetBytes(string.Join(null, users));
                            frame.data_length = (byte?)frame.data.Length;
                        }
                        Thread.Sleep(50); // если будет норм работать без этого то нужно убрать
                        SendFrame(frame);
                        break;

                    case Frame.Type.Uplink:
                        SendFrame(frame);
                        // !!! Разрыв соединения на физическом уровне и/или выход из приложения на пользовательском
                        Connection.ClosePorts();
                        break;

                    case Frame.Type.ACK:
                        if (frame.destination == (byte)userAddress)
                        {
                            // token ring
                        }
                        else
                        {
                            SendFrame(frame);
                        }
                        break;

                    case Frame.Type.Ret:
                        if (frame.destination == (byte)userAddress)
                        {
                            SendFrame(lastFrame); // ??? Просто последний отправленный кадр или последний отправленный кадр данному пользователю? 
                        }
                        else
                        {
                            SendFrame(frame);
                        }
                        break;
                }
            }
        }
    }
}
