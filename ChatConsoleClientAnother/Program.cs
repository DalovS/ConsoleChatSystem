using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    static void Main()
    {
        try
        {
            // Задаване на IP адрес и порт за сървъра
            int port = 12345;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            TcpClient client = new TcpClient();
            // Свързване към сървъра
            client.Connect(ipAddress, port);
            Console.WriteLine("Свързан със сървъра.");
            NetworkStream stream = client.GetStream();
            Console.Write("Въведете вашето име: ");
            string clientName = Console.ReadLine();
            byte[] nameData = Encoding.UTF8.GetBytes(clientName);
            stream.Write(nameData, 0, nameData.Length);
            // Нишка за приемане на съобщения от сървъра и изписване на конзолата
            Thread receiveThread = new Thread(() =>
            {
                byte[] data = new byte[4096];
                int bytesRead;
                while (true)
                {
                    try
                    {
                        bytesRead = stream.Read(data, 0, data.Length);
                        if (bytesRead <= 0)
                        {
                            break;
                        }
                        string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                        Console.WriteLine(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Грешка при приемане на съобщение: {ex}");
                        break;
                    }
                }
            });
            receiveThread.Start();
            // Нишка за изпращане на съобщения
            Thread sendThread = new Thread(() =>
            {
                while (true)
                {
                    // Четене на съобщение от конзолата и изпращане на сървъра                  
                    string message = Console.ReadLine();                 
                    byte[] messageData = Encoding.UTF8.GetBytes(message);
                    stream.Write(messageData, 0, messageData.Length);
                }
            });
            sendThread.Start();
            // Изчакване на завършването на нишките
            receiveThread.Join();
            sendThread.Join();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Грешка: {ex}");
        }
    }
}
