using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace UDPchat
{
    public partial class MainForm2 : Form
    {
        private UdpClient client;
        private Thread receivingThread;

        public MainForm2()
        {
            InitializeComponent();
            MainForm2_Load(this, EventArgs.Empty); // Вызываем метод MainForm2_Load при создании экземпляра MainForm2
        }

        private void MainForm2_Load(object sender, EventArgs e)
        {
            // Ваш существующий код из MainForm2_Load
            // Инициализация клиента UDP
            client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.EnableBroadcast = true;

            // Запуск потока для приема сообщений
            receivingThread = new Thread(ReceiveMessages);
            receivingThread.IsBackground = true;
            receivingThread.Start(); // Запускаем поток приема сообщений
        }

        private void UpdateChatDisplay(string message)
        {
            if (listBoxChat.InvokeRequired)
            {
                listBoxChat.Invoke(new Action(() => UpdateChatDisplay(message)));
            }
            else
            {
                listBoxChat.Items.Add(message);
                listBoxChat.Refresh(); // Добавьте вызов Refresh после добавления элемента
            }
        }

        private int FindAvailablePort()
        {
            const int startPort = 11500;
            const int endPort = 12000; // Вы можете настроить диапазон портов в соответствии с вашими потребностями

            for (int port = startPort; port <= endPort; port++)
            {
                try
                {
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    socket.Close();
                    return port;
                }
                catch (SocketException ex)
                {
                    // Проверяем код ошибки, чтобы узнать причину блокировки порта
                    if (ex.SocketErrorCode == SocketError.AccessDenied)
                    {
                        // Порт заблокирован (нет доступа)
                        // продолжаем поиск следующего порта
                        continue;
                    }
                    else
                    {
                        // Возникла другая ошибка, выбрасываем исключение
                        throw;
                    }
                }
            }

            throw new Exception("No available port found in the specified range.");
        }

        // Объявляем булевую переменную для отслеживания состояния потока
        private bool isReceivingThreadRunning = false;

        // В методе приема сообщений меняем значение переменной
        private void ReceiveMessages()
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 11500);

                while (true)
                {
                    byte[] data = client.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);
                    UpdateChatDisplay(message);
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок приема сообщений
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Получаем имя пользователя
            string username = txtUsername.Text;

            // Закрываем текущий сокет, если он уже был открыт
            if (client != null)
            {
                client.Close();
                client.Dispose();
            }

            // Создаем новый экземпляр UdpClient
            client = new UdpClient();

            int availablePort = FindAvailablePort();
            client.Client.Bind(new IPEndPoint(IPAddress.Any, availablePort));

            // Присоединяемся к группе рассылки
            client.JoinMulticastGroup(IPAddress.Parse("239.255.255.250"));

            // Проверяем, выполняется ли поток приема сообщений
            if (!isReceivingThreadRunning)
            {
                // Если поток не выполняется, мы можем попытаться запустить его
                receivingThread = new Thread(ReceiveMessages);
                receivingThread.Start();
            }

            // Блокируем кнопку "Start", чтобы не было возможности повторного подключения
            btnStart.Enabled = false;

            MessageBox.Show("Connected to group chat as: " + username);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // Получаем имя пользователя
            string username = txtUsername.Text;

            // Проверяем, что сообщение не пустое
            if (!string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                // Подготавливаем сообщение для отправки в формате "username: сообщение"
                string message = username + ": " + txtMessage.Text;
                byte[] data = Encoding.UTF8.GetBytes(message);

                // Отправляем сообщение всем участникам группы
                client.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("239.255.255.250"), 11000));
                UpdateChatDisplay(message);

                // Очищаем текстовое поле сообщения
                txtMessage.Clear();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // Завершаем прием сообщений и покидаем группу рассылки
            receivingThread.Abort();
            client.DropMulticastGroup(IPAddress.Parse("239.255.255.250"));

            MessageBox.Show("Disconnected from group chat");

            // Разблокируем кнопку "Start" для возможности повторного подключения
            btnStart.Enabled = true;
        }
    }
}
