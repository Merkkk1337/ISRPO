using System;
using System.Net.Sockets;
using System.Text;

class TcpClientProgram
{
    static void Main()
    {
        try
        {
            // Указываем IP-адрес и порт сервера, к которому будем подключаться
            int port = 8080;
            string ipAddress = "127.0.0.1";

            // Создаем TcpClient и подключаемся к серверу
            TcpClient client = new TcpClient(ipAddress, port);

            // Получаем NetworkStream для чтения данных
            NetworkStream stream = client.GetStream();

            // Читаем ответ от сервера
            byte[] data = new byte[1024];
            int bytesRead = stream.Read(data, 0, data.Length);
            string response = Encoding.UTF8.GetString(data, 0, bytesRead);

            // Выводим ответ на консоль
            Console.WriteLine(response);

            // Закрываем соединение
            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка: " + e.Message);
        }
    }
}
