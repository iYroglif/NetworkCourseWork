using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO.Ports;

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
            /* old
            /// Установка логического соединения.
            Link,
            /// Получение списка пользователей.
            Ask,
            /// Отправка сообщений пользователя.
            I,
            /// Отправка запроса на переотправку сообщения.
            Ret,
            /// Разъединение соединения.
            Uplink
            */
        }

        public byte destination;            // Адрес получателя
        public byte departure;              // Адрес отправителя
        public Type type;                   // Тип кадра
        public byte? data_length = null;    // Длина поля данных
        public byte?[] data = null;         // Данные

        public Frame(byte dep, Type type, byte? des = null, byte?[] bytes = null)
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

                            if (bytes != null)
                            {
                                // Ошибка: есть какие-то данные
                            }
                        }
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
                // Добавить проверку на допустимые типы
                type = (Type)bytes[3];

                byte i = 0;
                if ((type == Type.I) || (type == Type.Link))
                {
                    // Добавить проверку того что длина данных не больше чем весь массив байтов
                    data_length = bytes[4];
                    data = new byte?[(byte)data_length];
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
        /* old
        public int authorID;
        public string message;

        public Frame()
        {
            // ..
        }

        public Frame(List<byte> data)
        {
            this.data = data;

            byte typeByte = data[0];
            switch (typeByte)
            {
                case (byte)Type.Link:
                    this.type = Type.Link;
                    break;

                case (byte)Type.Ask:
                    this.type = Type.Ask;
                    break;

                case (byte)Type.I:
                    this.type = Type.I;
                    this.authorID = (int)data[1];

                    byte[] byteArray = data.ToArray();

                    int messageLength = data.Count - 2;
                    byte[] messageData = new byte[messageLength];

                    Array.Copy(byteArray, 2, messageData, 0, messageLength);

                    this.message = System.Text.Encoding.UTF8.GetString(messageData, 0, messageData.Length);

                    break;

                case (byte)Type.Ret:
                    this.type = Type.Ret;
                    break;

                case (byte)Type.Uplink:
                    this.type = Type.Uplink;
                    break;

                default:
                    this.type = Type.Link;
                    break;
            }

        }
        */

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
                bytes[4] = 0xFF; // Стоповый байт
            }
            return bytes;
        }
    }

    class DataLinkLayer
    {
        /// <summary>
        /// Установка логического соединения
        /// </summary>
        public void OpenConnection(string incomePortName, string outcomePortName, bool isMaster)
        {
            // !!! Установка физического соединения

            SendFrame(new Frame(/* ??? свой адрес */0, Frame.Type.Link, bytes: /* ??? данные со своим адресом и ником */ new byte?[1] { 0 }));
        }

        /// <summary>
        /// Отправка кадра
        /// </summary>
        public void SendFrame(Frame frame)
        {
            // !!! Отправка массива байтов на физический уровень (byte[])frame;
        }

        //

        /// <summary>
        /// Разъединение логического соединения
        /// </summary>
        public void CloseConnection()
        {
            SendFrame(new Frame(/* ??? свой адрес */0, Frame.Type.Uplink));
            HandleFrame(new byte[1] { 0 });
        }

        /// <summary>
        /// Обработка пришедшего кадра
        /// </summary>
        public void HandleFrame(byte[] bytes)
        {
            Frame frame = new Frame();
            if (!frame.TryConvertFromBytes(bytes))
            {
                // Ошибка: нужен запрос на повторную отправку и повторная отправка
            }
            else
            {

            }

            switch (frame.type)
            {
                case Frame.Type.Link:

                    this.notificationLabel.Invoke((MethodInvoker)delegate
                    {

                        // Running on the UI thread
                        this.notificationLabel.Text = "Соединение установлено";
                        this.connectButton.Text = "Войти";
                    });

                    // Если станция не ведущая, то отправляем дальше
                    if (currentConnection.isMaster == false)
                    {
                        this.SendFrame(frame);
                    }

                    break;

                case Frame.Type.Ask:

                    // Если станция не ведущая, то отправляем дальше
                    if (currentConnection.isMaster == false)
                    {
                        this.SendFrame(frame);
                    }

                    break;

                case Frame.Type.Data:

                    this.chatBox.Invoke((MethodInvoker)delegate
                    {

                        // Running on the UI thread
                        this.chatBox.Items.Add(string.Format("{0} ({1}) {2}", DateTime.Now.ToString("hh:mm"), frame.authorID, frame.message));
                    });

                    // Если станция не ялвяется отправителем, то отправляем дальше
                    if (currentSession.username != frame.authorID)
                    {
                        this.SendFrame(frame);
                    }

                    break;

                case Frame.Type.Error:

                    // Если станция не ведущая, то отправляем дальше
                    if (currentConnection.isMaster == false)
                    {
                        this.SendFrame(frame);
                    }

                    break;

                case Frame.Type.Downlink:

                    // Если станция не ведущая, то отправляем дальше
                    if (currentConnection.isMaster == false)
                    {
                        this.SendFrame(frame);
                    }

                    System.Windows.Forms.Application.Exit();

                    break;
            }
        }

        // -----------------------------------------------------------------------
        public Label notificationLabel;
        public Button connectButton;
        public ListBox chatBox;

        private NetworkService()
        {
            // ..
        }

        private static readonly NetworkService _sharedService = new NetworkService();

        public static NetworkService GetSharedService()
        {
            return _sharedService;
        }

        /// Текущее соединение
        public Connection currentConnection;

        /// Текущая сессия
        public Session currentSession;




        /// <summary>
        /// Обработка пришедшего сообщения
        /// </summary>
        public void HandleMessage(List<byte> message)
        {
            Frame frame = new Frame(message);
            this.HandleFrame(frame);
        }





        public void SendMessage(string message)
        {
            byte[] byteStr = System.Text.Encoding.UTF8.GetBytes(message);

            List<byte> data = new List<byte>();

            data.Add((byte)Frame.Type.Data);
            data.Add((byte)currentSession.username);

            foreach (byte b in byteStr)
            {
                data.Add(b);
            }

            Frame frame = new Frame();
            frame.data = data;

            this.SendFrame(frame);
        }

        /// <summary>
        /// Список доступных портов
        /// </summary>
        public string[] GetPortsNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Создание сессии
        /// </summary>
        public void CreateSession(int username)
        {
            this.currentSession = new Session(username);

            // формирование кадра c username..
            // отправка кадра c username..
        }

        /// <summary>
        /// Закрытие сессии
        /// </summary>
        public void CloseSession()
        {
            // ..
        }
    }
}
