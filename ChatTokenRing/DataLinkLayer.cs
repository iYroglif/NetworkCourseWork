using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace ChatTokenRing
{
    /// <summary>
    /// Класс описывающий кадр
    /// </summary>
    class Frame
    {
        /// <summary>
        /// Тип кадра
        /// </summary>
        public enum Type : byte
        {
            I,      // Информационный кадр
            Link,   // Кадр установки соединения
            Dis,    // Кадр разрыва соединения
            ACK,    // Кадр подтверждения безошибочного приема кадра
            Ret     // Кадр запроса повторения последнего отправленного кадра
        }

        public byte destination;            // Адрес получателя
        public byte departure;              // Адрес отправителя
        public Type type;                   // Тип кадра
        public byte? data_length = null;    // Длина поля данных
        public byte[] data = null;          // Данные

        /// <summary>
        /// Конструктор кадра
        /// </summary>
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

                case Type.Dis:
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

        /// <summary>
        /// Преобразование массива байтов в кадр (true - успешно, false - не удалось)
        /// </summary>
        public bool TryConvertFromBytes(byte[] bytes)
        {
            if (bytes[0] != 0xFF) // Проверка на стартовый байт
            {
                // Ошибка: неверный стартовый байт
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
                        // Ошибка: длина данных больше чем массив байтов
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
                    // Ошибка: неверный стоповый байт
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Преобразование кадра в массив байтов
        /// </summary>
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
        static int timeOut = 2000; // Стандартный тайм-аут
        static byte? userAddress = null; // Адрес пользователя
        static string userNickname; // Никнейм пользователя
        static Queue<Frame> sendingFrames = new Queue<Frame>(); // Буфер ожидающих к отправлению сообщений (ждущих маркер)
        static AutoResetEvent waitACC = new AutoResetEvent(false); // Событие прихода кадра подтверждения успешной доставки сообщения
        static AutoResetEvent recdRet = new AutoResetEvent(false); // Событие прихода кадра на повторную отправку сообщения
        static AutoResetEvent Disc = new AutoResetEvent(false); // Событие прихода кадра разрыва соединения
        static AutoResetEvent Lnk = new AutoResetEvent(false); // Событие прихода Link-кадра

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
            Disc = new AutoResetEvent(false); // Обнуление статических переменных
            Lnk = new AutoResetEvent(false); // Обнуление статических переменных

            Connection.OpenPorts(incomePortName, outcomePortName, isMaster); // Установка физического соединения
            if (isMaster) // Если станция ведущая
            {
                userAddress = 1;
                SendFrame(new Frame((byte)userAddress, Frame.Type.Link, bytes: Encoding.UTF8.GetBytes("[1, " + userNickname + ']'))); // Отправка Link кадра (маркера)
                SendFramesToken();
            }
            else
            {
                // !!! Вывод сообщения о установлении соединения...
                Lnk.WaitOne();
                // !!! Вывод сообщения что соединение установлено
            }
        }

        /// <summary>
        /// Отправка кадра на физический уровень
        /// </summary>
        static public void SendFrameToConnection(byte[] bytes)
        {
            if (!Disc.WaitOne(1)) // Если кадр разрыва соединения не приходил
            {
                Connection.SendBytes(bytes);
            }
            else
            {
                Disc.Set();
            }
        }

        /// <summary>
        /// Отправка кадра
        /// </summary>
        static public void SendFrame(Frame frame)
        {
            if ((frame.type == Frame.Type.ACK) || (frame.type == Frame.Type.Ret) || (frame.type == Frame.Type.Dis)) // Служебные кадры передаются без владения маркера
            {
                SendFrameToConnection((byte[])frame);
            }
            else
            {
                sendingFrames.Enqueue(frame); // Сохранение кадра для отправления когда будет захвачен маркер
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
                if ((tmp.destination != 0x7F) || (tmp.type == Frame.Type.Link)) // Для кадров с подтверждением успешной доставки
                {
                    waitACC.Reset();
                    recdRet.Reset();
                    SendFrameToConnection((byte[])tmp); // Отправка кадра на физический уровень
                    bool flg = false;
                    while (!(AutoResetEvent.WaitAny(new WaitHandle[] { waitACC, recdRet }, timeOut) == 0)) // Ожидание кадра успешной доставки или запроса на повторную отправку в случае ошибки
                    {
                        if (!Disc.WaitOne(1))
                        {
                            if (!flg)
                            {
                                flg = true;
                                Chat.connectionWait(); // Соединение потеряно... Восстановление соединения...
                            }
                            SendFrameToConnection((byte[])tmp); // Повторная отправка кадра
                        }
                        else
                        {
                            Disc.Set();
                            return;
                        }
                    }
                    if (flg)
                    {
                        Chat.connectionRestored(); // Соединение восстановлено
                    }
                }
                else // Для кадров без подтверждения успешной доставки
                {
                    SendFrameToConnection((byte[])tmp); // Отправка кадра на физический уровень
                }
            } while (tmp.type != Frame.Type.Link); // Выполнять пока маркер не отдан
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
            SendFrame(new Frame((byte)userAddress, Frame.Type.Dis)); // Отправка кадра разрыва соединения
            Disc.Set(); // Событие разрыва соединения
            Connection.ClosePorts();
        }

        /// <summary>
        /// Обработка пришедшего кадра
        /// </summary>
        static public void HandleFrame(byte[] bytes)
        {
            Frame frame = new Frame();
            if (!frame.TryConvertFromBytes(bytes)) // Попытка восстановить кадр из массива байтов
            {
                Debug.Assert(false, "Ошибка: нужен запрос на повторную отправку и повторная отправка");
                SendFrame(new Frame((byte)userAddress, Frame.Type.Ret, des:/* ??? в идеале соседний комп с ком порта потому что ошибки могут быть и с адресом отправителя */frame.departure)); // Запрос на повторную отправку
            }
            else
            {
                switch (frame.type)
                {
                    case Frame.Type.I: // Обработка информационного кадра
                        if ((frame.destination == 0x7F) || (frame.destination == (byte)userAddress)) // Если кадр предназначен этой станции
                        {
                            if (frame.destination != 0x7F)
                            {
                                if (frame.departure == (byte)userAddress)
                                {
                                    waitACC.Set(); // Нет смысла посылать кадр об успешной доставке самому себе
                                }
                                else
                                {
                                    SendFrame(new Frame((byte)userAddress, Frame.Type.ACK, des: frame.departure)); // Отправка кадра подтверждения безошибочного приема кадра
                                }
                            }

                            Chat.inMessage(Encoding.UTF8.GetString(frame.data, 0, frame.data.Length), frame.departure, frame.destination); // Передача сообщения на пользовательский уровень

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

                    case Frame.Type.Link: // Обработка маркера
                        Lnk.Set(); // Событие прихода маркера
                        Dictionary<byte, string> users = new Dictionary<byte, string>(); // Словарь пользователей

                        string[] items = Encoding.UTF8.GetString(frame.data, 0, frame.data.Length).Split(new string[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in items)
                        {
                            string[] tmp = item.Trim('[', ']').Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                            users.Add(Convert.ToByte(tmp[0]), tmp[1]);
                        }

                        Chat.List(users, userAddress); // Передача списка пользователей на пользовательский уровень

                        if (userAddress == null)
                        {
                            userAddress = (byte?)(users.Last().Key + 1);
                        }

                        // Если пользователь с таким именем уже есть
                        bool flg;
                        if (!users.ContainsKey((byte)userAddress))
                        {
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
                        SendFrame(new Frame((byte)userAddress, Frame.Type.ACK, des: frame.departure)); // Отправка кадра подтверждения безошибочного приема кадра
                        frame.departure = (byte)userAddress;
                        SendFrame(frame);
                        SendFramesToken(); // Отправка сообщений с захваченным маркером

                        // Ожидание возвращения маркера (проверка целостности соединения)
                        flg = false;
                        while (!Lnk.WaitOne(timeOut))
                        {
                            if (!Disc.WaitOne(1))
                            {
                                if (!flg)
                                {
                                    flg = true;
                                    Chat.connectionWait(); // Соединение потеряно... Восстановление соединения...
                                }
                            }
                            else
                            {
                                Disc.Set();
                                return;
                            }
                        }
                        if (flg)
                        {
                            Chat.connectionRestored(); // Соединение восстановлено
                        }
                        break;

                    case Frame.Type.Dis: // Обработка кадра разрыва соединения
                        SendFrameToConnection((byte[])frame); // Отправка кадра разрыва соединения
                        Disc.Set(); // Событие прихода кадра разрыва соединения

                        Connection.ClosePorts(); // Разрыв соединения на физическом уровне
                        Chat.exit(); // Выход из приложения на пользовательском уровне
                        break;

                    case Frame.Type.ACK: // Обработка кадра подтверждения безошибочного приема кадра
                        if (userAddress != null)
                        {
                            if (frame.destination == (byte)userAddress) // Если кадр предназначался этой станции
                            {
                                waitACC.Set(); // Событие прихода кадра подтверждения безошибочного приема кадра
                            }
                            else // Если кадр предназначался НЕ этой станции
                            {
                                SendFrame(frame);
                            }
                        }
                        else // Если кадр предназначался НЕ этой станции
                        {
                            SendFrame(frame);
                        }
                        break;

                    case Frame.Type.Ret: // Обработка кадра запроса повторения последнего отправленного кадра
                        if (frame.destination == (byte)userAddress) // Если кадр предназначался этой станции
                        {
                            recdRet.Set(); // Событие прихода кадра запроса повторения последнего отправленного кадра
                        }
                        else // Если кадр предназначался НЕ этой станции
                        {
                            SendFrame(frame);
                        }
                        break;
                }
            }
        }
    }
}
