using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatTokenRing
{
    abstract class Connection
    {
        static SerialPort incomePort;
        static SerialPort outcomePort;

        static bool isMaster;

        static byte boundByte = 0xFF;

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
            incomePort.Open();
            outcomePort.Open();

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

            return (incomePort.IsOpen && outcomePort.IsOpen);
        }

        /// <summary>
        /// Пересылка байтов
        /// </summary>
        public static void SendBytes(byte[] inputVect)
        {
            
            List<byte> hamm = inputVect.ToList();

            // Делаем так, чтобы внутри кадра не встречалось boundByte.
            List<byte> safeList = new List<byte>(hamm.Count);
            foreach (var b in hamm)
            {
                if ((b & 0x7F) == 0x7F)
                {
                    safeList.Add(0x7F);
                    safeList.Add((byte)(b & 0x80));
                }
                else
                {
                    safeList.Add(b);
                }
            }

            // Добавляем стартовый и конечный байт
            safeList.Insert(0, boundByte);
            safeList.Add(boundByte);

            if (outcomePort.WriteBufferSize - outcomePort.BytesToWrite < safeList.Count)
            {
                // Если сообщение не влезло в порт, то надо что-то с этим делать.
                // То ли очередь придумать, то ли ещё что.
                return;
            }

            byte[] outputVect = safeList.ToArray();
            outcomePort.Write(outputVect, 0, outputVect.Length);
        }

        /// <summary>
        /// Получение байтов
        /// </summary>
        public static void RecieveBytes(object sender, SerialDataReceivedEventArgs e)
        {
            int bytes = incomePort.BytesToRead;

            byte[] comBuffer = new byte[bytes];
            List<byte> listBuffer = new List<byte>();

            bool startByte = false;

            // Записываем в массив данные от ком порта.
            incomePort.Read(comBuffer, 0, bytes);

            /*foreach (byte incomeByte in comBuffer)
            {
                if (incomeByte == boundByte)
                {
                    if (this.bytesBuffer.Count > 0)
                    {
                        NetworkService.GetSharedService().HandleMessage(this.bytesBuffer);
                    }

                    this.bytesBuffer = new List<byte>();
                }
                else
                {
                    this.bytesBuffer.Add(incomeByte);
                }
            }*/

            for (int i = 0; i < comBuffer.Length; i++) {
                if (comBuffer[i] == boundByte) {
                    if (startByte) {
                        byte[] outputArray = listBuffer.ToArray();
                        DataLinkLayer.HandleFrame(outputArray);
                    }
                    else {
                        startByte = true;
                    }
                }

                if (startByte) {
                    if (comBuffer[i] == 0x7F) {
                        listBuffer.Add((byte)(comBuffer[i] | comBuffer[++i]));
                    }
                    else {
                        listBuffer.Add(comBuffer[i]);
                    }
                }
            }
        }
    }
}