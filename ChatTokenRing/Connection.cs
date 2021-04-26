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

        private static AutoResetEvent FrameIsRead = new AutoResetEvent(false);

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
            incomePort.BaudRate = 9600;
            incomePort.StopBits = StopBits.Two;

            incomePort.Handshake = Handshake.RequestToSend;
            incomePort.ReadBufferSize = 1024;
            incomePort.DataReceived += new SerialDataReceivedEventHandler(RecieveBytes);
            incomePort.ReceivedBytesThreshold = 4;
            incomePort.ReadTimeout = 500;
            incomePort.DtrEnable = true;


            outcomePort.Parity = Parity.Even;
            outcomePort.BaudRate = 9600;
            outcomePort.StopBits = StopBits.Two;

            outcomePort.Handshake = Handshake.RequestToSend;
            outcomePort.WriteBufferSize = 1024;
            outcomePort.WriteTimeout = 500;
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

            //if (isMaster)
            //{
                while (!outcomePort.DsrHolding || !incomePort.DsrHolding)
                {
                    Thread.Sleep(100);
                }
            //}

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
        /// Отправка байтов
        /// <summary>
        public static void SendBytes(byte[] outputVect)
        {
            byte[] codedVect = CyclicCode.Coding(outputVect);
            lock (slocker)
            {
                Thread.Sleep(10);
                if (outcomePort.IsOpen && incomePort.IsOpen && outcomePort.DsrHolding)
                {
                    try
                    {
                        outcomePort.Write(codedVect, 0, codedVect.Length);
                    }
                    catch
                    {
                        Console.WriteLine("Connectrion is closed!");
                    }
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
            FrameIsRead.WaitOne();
        }

        /// <summary>
        /// Считывание байтов
        /// </summary>
        static void ReadBytes()
        {
            byte[] inputVect = new byte[0];
            lock (glocker)
            {//!!!!
                if (outcomePort.IsOpen && incomePort.IsOpen)
                {
                    int bytes = incomePort.BytesToRead;
                    inputVect = new byte[bytes];
                    incomePort.Read(inputVect, 0, bytes);
                    foreach (var i in inputVect)
                    {
                        Console.Write(i.ToString() + " ");
                    }
                    Console.WriteLine();
                    //byte[] encodedVect = CyclicCode.Decoding(inputVect);
                }
            }
            FrameIsRead.Set();
            DataLinkLayer.HandleFrame(inputVect);
        }
    }
}
