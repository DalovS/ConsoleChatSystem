using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private static List<ClientHandler> clients = new List<ClientHandler>();

    static void Main()
    {
        TcpListener server = null;
        try
        {
            // Задаване на IP адрес и порт за сървъра
            int port = 12345;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            // Създаване на TcpListener за слушане на входящи връзки
            server = new TcpListener(ipAddress, port);
            server.Start();

            Console.WriteLine("Сървърът е стартиран и чака за клиенти...");

            while (true)
            {
                // Приемане на връзка от клиент
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Клиент свързан.");

                // Стартиране на нова нишка за обработка на клиента
                ClientHandler clientHandler = new ClientHandler(client);
                clients.Add(clientHandler);
                Thread clientThread = new Thread(clientHandler.Handle);
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Грешка: {ex}");
        }
        finally
        {
            server?.Stop();
        }
    }

    // Клас за обработка на връзката с клиент
    class ClientHandler
    {
        private TcpClient client;
        private NetworkStream stream;
        private string clientName;

        public ClientHandler(TcpClient client)
        {
            this.client = client;
        }

        public void Handle()
        {
            try
            {
                stream = client.GetStream();
                byte[] buffer = new byte[4096]; // Увеличете размера на буфера

                // Вземете името на клиента (може да го изпратите от клиента преди това)
                byte[] nameBuffer = new byte[4096];
                int nameBytesRead = stream.Read(nameBuffer, 0, nameBuffer.Length);
                clientName = Encoding.UTF8.GetString(nameBuffer, 0, nameBytesRead);

               

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Изпратете съобщението на всички клиенти, освен на текущия
                    SendToOthers($"{clientName}: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Грешка при обслужване на клиент: {ex}");
            }
            finally
            {
                // Известете всички клиенти, че този клиент напуска чата
                clients.Remove(this);
                SendToAll($"{clientName} напусна чата.");
                client.Close();
            }
        }

        private void SendToAll(string message)
        {
            foreach (var clientHandler in clients)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                clientHandler.stream.Write(data, 0, data.Length);
                clientHandler.stream.Flush();
            }
        }

        private void SendToOthers(string message)
        {
            foreach (var clientHandler in clients)
            {
                if (clientHandler != this)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    clientHandler.stream.Write(data, 0, data.Length);
                    clientHandler.stream.Flush();
                }
            }
        }
    }
}
