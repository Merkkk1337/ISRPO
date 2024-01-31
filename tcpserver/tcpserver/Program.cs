using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    static void Main()
    {
        TcpListener server = null;
        try
        {
            // Указываем IP-адрес и порт, на котором сервер будет прослушивать входящие соединения
            int port = 8080;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            // Создаем TcpListener
            server = new TcpListener(ipAddress, port);

            // Запускаем прослушивание входящих соединений
            server.Start();

            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                // Принимаем клиентский запрос
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Клиент подключен!");

                // Обрабатываем запрос
                HandleClient(client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка: " + e.Message);
        }
        finally
        {
            server.Stop();
        }
    }

    static void HandleClient(TcpClient client)
    {
        // Получаем NetworkStream для чтения и записи данных
        NetworkStream stream = client.GetStream();

        // Отправляем HTTP-ответ
        string response = "HTTP/1.1 200 OK\n" +
                          "Date: Wed, 11 Feb 2009 11:20:59 GMT\n" +
                          "Server: Apache\n" +
                          "Last-Modified: Wed, 11 Feb 2021 11:20:59 GMT\n" +
                          "Content-Type: text/html; charset=utf-8\n" +
                          "Content-Length: 1234\n\n" +
                          "<!DOCTYPE html>\n<html>\n<body>\n<h1>My First Heading</h1>\n<p>My first paragraph.</p>\n</body>\n</html>";

        byte[] data = Encoding.UTF8.GetBytes(response);

        // Отправляем ответ клиенту
        stream.Write(data, 0, data.Length);

        // Закрываем соединение
        client.Close();
    }
}
