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
        static object locker = new object();

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
            //this.incomePort.ReadBufferSize = 4 * 1024; // TODO: Надо пересчитать размер буфера.
            incomePort.DataReceived += new SerialDataReceivedEventHandler(RecieveBytes);

            outcomePort.Parity = Parity.Even;
            outcomePort.Handshake = Handshake.RequestToSend;
            outcomePort.BaudRate = 9600;

            // Открываем порты.
            if (!incomePort.IsOpen)
            {
                incomePort.Open();
            }
            if (!outcomePort.IsOpen)
            {
                outcomePort.Open();
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
            lock (locker)
            {
                outcomePort.Write(outputVect, 0, outputVect.Length);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Получение байтов
        /// </summary>
        public static void RecieveBytes(object sender, SerialDataReceivedEventArgs e)
        {
            lock (locker)
            {
                Thread.Sleep(10);
                int bytes = incomePort.BytesToRead;
                byte[] inputVect = new byte[bytes];

                // Записываем в массив данные от ком порта.
                incomePort.Read(inputVect, 0, bytes);
                DataLinkLayer.HandleFrame(inputVect);
            }
        }
    }
}