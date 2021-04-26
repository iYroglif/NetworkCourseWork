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
            incomePort.ReceivedBytesThreshold = 2;


            outcomePort.Parity = Parity.Even;
            outcomePort.BaudRate = 9600;
            outcomePort.StopBits = StopBits.Two;

            outcomePort.Handshake = Handshake.RequestToSend;
            outcomePort.WriteBufferSize = 1024;
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
        /// Отправка байтов
        /// <summary>
        public static void SendBytes(byte[] outputVect)
        {
            byte[] codedVect = CyclicCode.Coding(outputVect);
            lock (slocker)
            {
                Thread.Sleep(10);
                if (outcomePort.IsOpen && incomePort.IsOpen && incomePort.DsrHolding)
                {
                    outcomePort.Write(codedVect, 0, codedVect.Length);
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
            if (outcomePort.IsOpen && incomePort.IsOpen && incomePort.DsrHolding)
            {
                int bytes = incomePort.BytesToRead;
                byte[] inputVect = new byte[bytes];
                incomePort.Read(inputVect, 0, bytes);

                FrameIsRead.Set();
                //byte[] encodedVect = CyclicCode.Decoding(inputVect);
                DataLinkLayer.HandleFrame(inputVect);
            }
        }
    }
}


/// <summary>
/// Пересылка байтов
/// </summary>
//    public static void SendBytes(byte[] outputVect)
//    {
//        //lock(slocker)
//        //{
//        if (incomePort.IsOpen && incomePort.DsrHolding)
//        {
//            outcomePort.Write(outputVect, 0, outputVect.Length);
//            //Thread.Sleep(100);
//        }
//        else
//        {
//            //Соединение закрыто
//        }
//        //}
//    }

//    /// <summary>
//    /// Считывание байтов
//    /// </summary>
//    static void RecieveBytes(object sender, SerialDataReceivedEventArgs e)
//    {

//        //lock (glocker)
//        //{
//        //    Thread.Sleep(10);
//        //}

//        int bytes = incomePort.BytesToRead;
//        byte[] inputBytes = new byte[bytes];
//        incomePort.Read(inputBytes, 0, bytes);

//        List<byte> inputVect = new List<byte>();
//        bool StartByteIsMet = false;

//        for (int i = 0; i < bytes; i++)
//        {
//            if (inputBytes[i] == 0xFF)
//            {
//                inputVect.Add(0xFF);
//                if (StartByteIsMet)
//                {
//                    StartByteIsMet = false;
//                    DataLinkLayer.HandleFrame(inputVect.ToArray());
//                    inputVect.Clear();
//                }
//                else
//                {
//                    StartByteIsMet = true;
//                }
//            }
//            else
//            {
//                inputVect.Add(inputBytes[i]);
//            }
//            Console.Write(inputBytes[i] + " ");
//        }
//        Console.WriteLine();
//    }
//}