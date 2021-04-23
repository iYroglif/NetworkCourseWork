using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatTokenRing
{
    abstract class Connection
    {
        static object slocker = new object();
        static object glocker = new object();

        static SerialPort incomePort;
        static SerialPort outcomePort;

        static bool isMaster;

        public static string[] GetPortsNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Открытие портов
        /// </summary>
        public static bool OpenPorts(string incomePortName, string outcomePortName, bool _isMaster)
        {
            // Создаем объекты портов.
            incomePort = new SerialPort(incomePortName);
            outcomePort = new SerialPort(outcomePortName);

            isMaster = _isMaster;

            // Настраиваем порты.
            incomePort.Parity = Parity.Even;
            incomePort.Handshake = Handshake.RequestToSend;
            incomePort.BaudRate = 9600;
            incomePort.ReadBufferSize = 4096;
            incomePort.DataReceived += new SerialDataReceivedEventHandler(RecieveBytes);

            outcomePort.Parity = Parity.Even;
            outcomePort.Handshake = Handshake.RequestToSend;
            outcomePort.BaudRate = 9600;
            incomePort.WriteBufferSize = 4096;
            outcomePort.DtrEnable = true;

            // Открываем порты.
            if (!incomePort.IsOpen)
            {
                incomePort.Open();
            }
            if (!outcomePort.IsOpen)
            {
                outcomePort.Open();
            }

            if (isMaster)
            {
                while (!incomePort.DsrHolding)
                {
                    Thread.Sleep(100);
                }
            }

            return (incomePort.IsOpen && outcomePort.IsOpen);
        }

        /// <summary>
        /// Закрытие портов
        /// </summary>
        public static bool ClosePorts()
        {
            // Закрываем порты.
            incomePort.Close();
            outcomePort.Close();

            return (!incomePort.IsOpen && !outcomePort.IsOpen);
        }

        /// <summary>
        /// Пересылка байтов
        /// </summary>
        public static void SendBytes(byte[] outputVect)
        {
            lock(slocker)
            {
                Thread.Sleep(50);

                try
                {
                    if (incomePort.IsOpen && incomePort.DsrHolding)
                    {
                        outcomePort.Write(outputVect, 0, outputVect.Length);
                        //Thread.Sleep(100);
                    }
                    else
                    {
                        //Соединение закрыто
                    }
                }
                catch
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        /// <summary>
        /// Ивент на получение байтов
        /// </summary>
        static void RecieveBytes(object sender, SerialDataReceivedEventArgs e)
        {
            Thread myThread = new Thread(new ThreadStart(ReadBytes));
            myThread.Start(); // запускаем поток
            //ReadBytes();
        }

        /// <summary>
        /// Считывание байтов
        /// </summary>
        static void RecieveBytes(object sender, SerialDataReceivedEventArgs e)
        {
            if (incomePort.BytesToRead > 2)
            {
                byte[] inputVect; // тут возможно какой то поток сразу обнулит значение inputVect после выхода из lock -> ошибка
                lock (glocker)
                {
                    Thread.Sleep(10);
                    int bytes = incomePort.BytesToRead;
                    inputVect = new byte[bytes];

                    // Записываем в массив данные от ком порта.
                    try
                    {
                        incomePort.Read(inputVect, 0, bytes);
                    }
                    catch
                    {
                        Thread.CurrentThread.Interrupt();
                    }
                }
                if (inputVect.Length > 2)
                {
                    DataLinkLayer.HandleFrame(inputVect);
                }
            }
        }
    }
}