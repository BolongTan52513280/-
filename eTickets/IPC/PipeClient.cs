using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Linq;
using Amazoom.Models;
using System.Collections.Generic;

namespace Amazoom.IPC
{
    class PipeClient
    {

       async static public void Client(string OrderInfo)
        {
        connection:
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 1302);
                string messageToSend = OrderInfo;
                int byteCount = Encoding.ASCII.GetByteCount(messageToSend + 1);
                byte[] sendData = Encoding.ASCII.GetBytes(messageToSend);

                NetworkStream stream = client.GetStream();
                stream.Write(sendData, 0, sendData.Length);
                Console.WriteLine("sending data to server...");

                StreamReader sr = new StreamReader(stream);
                string response = sr.ReadLine();
                Console.WriteLine(response);

                stream.Close();
                client.Close();
                //Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to connect...");
                goto connection;
            }
        }
    }
}
