using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            SendFrame(new Frame(/* ??? свой адрес */0, Frame.Type.Link, bytes: /* ??? данные со своим адресом и ником */ new byte[1] { 0 }));
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
            // !!! Разрыв соединения на физическом уровне и/или выход из приложения на пользовательском
        }

        /// <summary>
        /// Обработка пришедшего кадра
        /// </summary>
        public void HandleFrame(byte[] bytes)
        {
            Frame frame = new Frame();
            if (!frame.TryConvertFromBytes(bytes))
            {
                SendFrame(new Frame(/* ??? свой адрес */0, Frame.Type.Ret, des:/* ??? по идее соседний комп с ком порта потому что ошибки могут быть и с адресом отправителя */1));
                // Ошибка: нужен запрос на повторную отправку и повторная отправка
            }
            else
            {
                switch (frame.type)
                {
                    case Frame.Type.I:
                        if ((frame.destination == 0x7F) || (frame.destination == /* ??? свой адрес */0))
                        {
                            SendFrame(new Frame(/* ??? свой адрес */0, Frame.Type.ACK, des: frame.departure));
                            // !!! Передача сообщения на пользовательский уровень frame.data
                            if (frame.destination == 0x7F)
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
                        Dictionary<byte, string> users = Encoding.UTF8.GetString(frame.data, 0, frame.data.Length).ToDictionary<byte, string>;
                        // !!! Передача списка пользователей на пользовательский уровень
                        if (!users.ContainsKey(/* ??? свой адрес */0))
                        {
                            users.Add(/* ??? свой адрес */0, /* ??? свой ник */"nick");
                            frame.data = Encoding.UTF8.GetBytes(users.ToString());
                            frame.data_length = (byte?)frame.data.Length;
                        }
                        SendFrame(frame);
                        break;

                    case Frame.Type.Uplink:
                        SendFrame(frame);
                        // !!! Разрыв соединения на физическом уровне и/или выход из приложения на пользовательском
                        break;

                    case Frame.Type.ACK:
                        if (frame.destination == /* ??? свой адрес */0)
                        {
                            // ???
                        }
                        else
                        {
                            SendFrame(frame);
                        }
                        break;

                    case Frame.Type.Ret:
                        if (frame.destination == /* ??? свой адрес */0)
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
