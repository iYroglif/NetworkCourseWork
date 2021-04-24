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
using System.Diagnostics;

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
                        Debug.Assert(false, "Ошибка: нет адреса получателя");
                    }
                    else
                    {
                        destination = (byte)des;

                        if (bytes == null)
                        {
                            Debug.Assert(false, "Ошибка: нет данных");
                        }
                        else
                        {
                            if (bytes.Length > 255)
                            {
                                Debug.Assert(false, "Ошибка: данные не помещаются в кадр");
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
                            Debug.Assert(false, "Ошибка: адрес не широковещательный");
                        }
                        else
                        {
                            destination = (byte)des;
                        }
                    }
                    if (bytes == null)
                    {
                        Debug.Assert(false, "Ошибка: нет данных");
                    }
                    else
                    {
                        if (bytes.Length > 255)
                        {
                            Debug.Assert(false, "Ошибка: данные не помещаются в кадр");
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
                            Debug.Assert(false, "Ошибка: адрес не широковещательный");
                        }
                        else
                        {
                            destination = (byte)des;
                        }
                    }
                    if (bytes != null)
                    {
                        Debug.Assert(false, "Ошибка: есть какие-то данные");
                    }
                    break;

                case Type.ACK:
                    if (des == null)
                    {
                        Debug.Assert(false, "Ошибка: нет адреса получателя");
                    }
                    else
                    {
                        destination = (byte)des;

                        if (bytes != null)
                        {
                            Debug.Assert(false, "Ошибка: есть какие-то данные");
                        }
                    }
                    break;

                case Type.Ret:
                    if (des == null)
                    {
                        Debug.Assert(false, "Ошибка: нет адреса получателя");
                    }
                    else
                    {
                        destination = (byte)des;

                        if (bytes != null)
                        {
                            Debug.Assert(false, "Ошибка: есть какие-то данные");
                        }
                    }
                    break;

                default:
                    Debug.Assert(false, "Ошибка: неверный тип кадра");
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
        static byte? userAddress = null; // Адрес пользователя
        static string userNickname; // Никнейм пользователя
        static Queue<Frame> sendingFrames = new Queue<Frame>(); // Буфер ожидающих к отправлению сообщений (ждущих маркер)
        static AutoResetEvent waitACC = new AutoResetEvent(false); // Событие прихода кадра подтверждения успешной доставки сообщения
        static AutoResetEvent recdRet = new AutoResetEvent(false); // Событие прихода кадра на повторную отправку сообщения
        static AutoResetEvent unDisc = new AutoResetEvent(false); // Событие прихода кадра разрыва соединения

        /// <summary>
        /// Установка логического соединения
        /// </summary>
        static public void OpenConnection(string incomePortName, string outcomePortName, bool isMaster, string userName)
        {
            userAddress = null; // Обнуление статических переменных
            userNickname = userName; // Получение никнейма с пользовательского уровня
            sendingFrames = new Queue<Frame>(); // Обнуление статических переменных
            waitACC = new AutoResetEvent(false); // Обнуление статических переменных
            recdRet = new AutoResetEvent(false); // Обнуление статических переменных
            unDisc = new AutoResetEvent(false);

            Connection.OpenPorts(incomePortName, outcomePortName, isMaster); // Установка физического соединения
            if (isMaster) // Если станция ведущая
            {
                userAddress = 1;
                SendFrameToConnection((byte[])new Frame((byte)userAddress, Frame.Type.Link, bytes: Encoding.UTF8.GetBytes("[1, " + userNickname + ']'))); // Отправка Link кадра (маркера)
            }
        }

        /// <summary>
        /// Отправка кадра на физический уровень
        /// </summary>
        static public void SendFrameToConnection(byte[] bytes)
        {
            if (!unDisc.WaitOne(1))
            {
                Connection.SendBytes(bytes);
            }
        }

        /// <summary>
        /// Отправка кадра
        /// </summary>
        static public void SendFrame(Frame frame)
        {
            if ((frame.type == Frame.Type.ACK) || (frame.type == Frame.Type.Ret) || (frame.type == Frame.Type.Uplink))
            {
                SendFrameToConnection((byte[])frame);
            }
            else
            {
                sendingFrames.Enqueue(frame);
            }
        }

        /// <summary>
        /// Отправка кадров при наличии маркера
        /// </summary>
        static public void SendFramesToken()
        {
            Frame tmp;
            do
            {
                tmp = sendingFrames.Dequeue();
                if ((tmp.destination != 0x7F) || (tmp.type == Frame.Type.Link))
                {
                    waitACC.Reset();
                    recdRet.Reset();
                    SendFrameToConnection((byte[])tmp);
                    try
                    {
                        while ((!(AutoResetEvent.WaitAny(new WaitHandle[] { waitACC, recdRet }, 2000) == 0)) && !unDisc.WaitOne(1))
                        {
                            SendFrameToConnection((byte[])tmp);
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
                else
                {
                    SendFrameToConnection((byte[])tmp);
                }
            } while (tmp.type != Frame.Type.Link);
        }

        /// <summary>
        /// Отправка сообщения с пользовательского уровня
        /// </summary>
        static public void SendMessage(byte? des, string mes)
        {
            SendFrame(new Frame((byte)userAddress, Frame.Type.I, des, Encoding.UTF8.GetBytes(mes)));
        }

        /// <summary>
        /// Разъединение логического соединения
        /// </summary>
        static public void CloseConnection()
        {
            SendFrame(new Frame((byte)userAddress, Frame.Type.Uplink)); // Отправка кадра разрыва соединения
            unDisc.Set();
            Connection.ClosePorts();
        }

        /// <summary>
        /// Обработка пришедшего кадра
        /// </summary>
        static public void HandleFrame(byte[] bytes)
        {
            Frame frame = new Frame();
            if (!frame.TryConvertFromBytes(bytes))
            {
                Debug.Assert(false, "Ошибка: нужен запрос на повторную отправку и повторная отправка");
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
                            if (frame.destination != 0x7F)
                            {
                                if (frame.departure == (byte)userAddress)
                                {
                                    waitACC.Set(); // Нет смысла посылать кадр об успешной доставке самому себе
                                }
                                else
                                {
                                    SendFrame(new Frame((byte)userAddress, Frame.Type.ACK, des: frame.departure));
                                }
                            }

                            //await Task.Run(() => (Application.Current.MainWindow as MainWindow).chatWindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length)));

                            /*Dispatcher.Invoke(async () => {

                                listBox.Items.Add(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length));

                                //(Application.Current.MainWindow as MainWindow).chatWindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length))
                            });*/

                            //Thread.Sleep(200); // !!! работающий костыль (макс, нужно поправить)
                            //listBox.Dispatcher.Invoke(() => { listBox.Items.Add(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length)); });
                            //System.Windows.Forms.Control.Invoke((MethodInvoker)delegate { (System.Windows.Application.Current.MainWindow as MainWindow).chatWindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length), frame.departure); });


                            //(System.Windows.Application.Current.MainWindow as MainWindow).Dispatcher.Invoke(() => { (System.Windows.Application.Current.MainWindow as MainWindow).chatWindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length), frame.departure); });
                            //Dispatcher.Invoke((MethodInvoker)delegate { chwindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length), frame.departure); });
                            Chat.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length), frame.departure, frame.destination);
                            //mv.chatWindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length), frame.departure);
                            //object v = System.Windows.Forms.Control.Invoke( (MethodInvoker)delegate { });
                            //(System.Windows.Application.Current.MainWindow as MainWindow).chatWindow.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length), frame.departure);
                            //Chat.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length), frame.departure);

                            // !!! Передача сообщения на пользовательский уровень frame.data
                            if ((frame.destination == 0x7F) && (frame.departure != userAddress))
                            {
                                SendFrame(frame);
                            }
                        }
                        else
                        {
                            SendFrameToConnection((byte[])frame);
                        }
                        break;

                    case Frame.Type.Link:
                        Dictionary<byte, string> users = new Dictionary<byte, string>();

                        string[] items = Encoding.UTF8.GetString(frame.data, 0, frame.data.Length).Split(new string[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in items)
                        {
                            string[] tmp = item.Trim('[', ']').Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                            users.Add(Convert.ToByte(tmp[0]), tmp[1]);
                        }
                        Chat.List(users, userAddress);
                        // !!! Передача списка пользователей на пользовательский уровень (users-словарь пользователей, значение ключей-имена пользователей(string))
                        if (userAddress == null)
                        {
                            userAddress = (byte?)(users.Last().Key + 1);
                        }

                        if (!users.ContainsKey((byte)userAddress))
                        {
                            // ??? Может ли быть случай остутствия никнейма к этому моменту? userNickname = "nick2";// !!! Получение никнейма с пользовательского уровня
                            bool flg;
                            do
                            {
                                flg = false;
                                foreach (var us in users)
                                {
                                    if (us.Value == userNickname)
                                    {
                                        userNickname += " (1)";
                                        flg = true;
                                        break;
                                    }
                                }
                            } while (flg);

                            users.Add((byte)userAddress, userNickname);
                            frame.data = Encoding.UTF8.GetBytes(string.Join(null, users));
                            frame.data_length = (byte?)frame.data.Length;
                        }
                        SendFrame(new Frame((byte)userAddress, Frame.Type.ACK, des: frame.departure));
                        frame.departure = (byte)userAddress;
                        SendFrame(frame);
                        SendFramesToken();
                        break;

                    case Frame.Type.Uplink:
                        SendFrameToConnection((byte[])frame);
                        // !!! Разрыв соединения на физическом уровне и/или выход из приложения на пользовательском
                        unDisc.Set();
                        Connection.ClosePorts();
                        Chat.exit();
                        break;

                    case Frame.Type.ACK:


                        if (userAddress != null)
                        {
                            if (frame.destination == (byte)userAddress)
                            {
                                waitACC.Set();
                            }
                            else
                            {
                                SendFrame(frame);
                            }
                        }
                        else
                        {
                            SendFrame(frame);
                        }
                        break;

                    case Frame.Type.Ret:
                        if (frame.destination == (byte)userAddress)
                        {
                            recdRet.Set();
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
