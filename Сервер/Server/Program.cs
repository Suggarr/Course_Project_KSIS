using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class FileTransferServer
{
    private const int ListenPort = 12345; // Порт, на котором сервер прослушивает подключения
    private static readonly string StoragePath = Path.Combine(Directory.GetCurrentDirectory(), "Файлы");
    private const int Size = 1024;
    private const int length = 10;

    public static void Main()
    {
        StartServer();
        GetFileList();
    }

    public static void StartServer()
    {
        try
        {
            // Создание TCP-сокета
            using (Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // Привязка сокета к локальной конечной точке и начало прослушивания подключений
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, ListenPort);
                listener.Bind(localEndPoint);
                listener.Listen(length);

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    // Принятие подключения
                    using (Socket handler = listener.Accept())
                    {

                        IPEndPoint clientEndPoint = (IPEndPoint)handler.RemoteEndPoint;
                        string clientIPAddress = clientEndPoint.Address.ToString();
                        Console.WriteLine("Подключение установлено.");

                        // Получение данных от клиента
                        byte[] data = new byte[Size];
                        int bytesRead = handler.Receive(data);
                        string request = Encoding.UTF8.GetString(data, 0, bytesRead);

                        // Разбор запроса
                        string[] requestParts = request.Split('|');
                        string action = requestParts[0];
                        string fileName = requestParts.Length > 1 ? requestParts[1] : "";

                        // Выполнение действия в зависимости от запроса
                        string response = "";
                        if (action == "DELETE")
                        {
                            response = DeleteFile(fileName);
                            Console.WriteLine($"Клиент {clientIPAddress} удалили файл {fileName}.\n");
                        }
                        else if (action == "RENAME" && requestParts.Length > 2)
                        {
                            string newFileName = requestParts[2];
                            response = RenameFile(fileName, newFileName);
                            Console.WriteLine($"Клиент {clientIPAddress} переименовал файл {fileName} в {newFileName}.\n");
                        }
                        else if (action == "DOWNLOAD")
                        {
                            response = SendFile(handler, fileName);
                            Console.WriteLine($"Клиент {clientIPAddress} скачал файл {fileName}\n.");
                        }
                        else if (action == "UPLOAD")
                        {
                            response = ReceiveFile(handler, fileName);
                            Console.WriteLine($"Клиент {clientIPAddress} отправил файл {fileName}.\n");
                        }
                        else if (action == "LIST")
                        {
                            response = GetFileList();
                        }
                        else
                        {
                            response = action;
                        }

                        // Отправка ответа клиенту
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        handler.Send(responseBytes);

                        Console.WriteLine("Запрос обработан: " + response);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка сервера: " + e.Message);
        }
    }

    public static string DeleteFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(StoragePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return "OK";
            }
            else
            {
                return "Файл не существует.";
            }
        }
        catch (Exception e)
        {
            return "Ошибка при удалении файла: " + e.Message;
        }
    }

    public static string RenameFile(string oldFileName, string newFileName)
    {
        try
        {
            string oldFilePath = Path.Combine(StoragePath, oldFileName);
            string newFilePath = Path.Combine(StoragePath, newFileName);

            if (File.Exists(oldFilePath))
            {
                File.Move(oldFilePath, newFilePath);
                return "OK";
            }
            else
            {
                return "Файл не существует.";
            }
        }
        catch (Exception e)
        {
            return "Ошибка при переименовании файла: " + e.Message;
        }
    }

    public static string SendFile(Socket handler, string fileName)
    {
        try
        {
            string filePath = Path.Combine(StoragePath, fileName);

            if (File.Exists(filePath))
            {
                // Отправка размера файла
                long fileSize = new FileInfo(filePath).Length;
                byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
                handler.Send(fileSizeBytes);

                // Отправка файла
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = 0;

                        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            handler.Send(buffer, 0, bytesRead, SocketFlags.None);
                        }
                    }
                }

                return "OK";
            }
            else
            {
                return "Файл не существует.";
            }
        }
        catch (Exception e)
        {
            return "Ошибка при отправке файла: " + e.Message;
        }
    }

    public static string ReceiveFile(Socket handler, string fileName)
    {
        try
        {
            string filePath = Path.Combine(StoragePath, fileName);

            // Отправка подтверждения клиенту
            byte[] confirmationBytes = Encoding.UTF8.GetBytes("OK");
            handler.Send(confirmationBytes);

            // Получение размера файла
            byte[] fileSizeBytes = new byte[8];
            int bytesRead = handler.Receive(fileSizeBytes);
            long fileSize = BitConverter.ToInt64(fileSizeBytes, 0);

            // Получение файла по частям
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[1024];
                long bytesReceived = 0;
                int bytesToRead;

                while (bytesReceived < fileSize)
                {
                    bytesToRead = (int)Math.Min(buffer.Length, fileSize - bytesReceived);
                    bytesRead = handler.Receive(buffer, 0, bytesToRead, SocketFlags.None);
                    fileStream.Write(buffer, 0, bytesRead);
                    bytesReceived += bytesRead;
                }
            }

            return "OK";
        }
        catch (Exception e)
        {
            return "Ошибка при получении файла: " + e.Message;
        }
    }

    public static string GetFileList()
    {
        try
        {
            StringBuilder fileList = new StringBuilder();
            DirectoryInfo directoryInfo = new DirectoryInfo(StoragePath);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (FileInfo file in files)
            {
                fileList.AppendLine(file.Name);
            }

            byte[] encodedBytes = Encoding.UTF8.GetBytes(fileList.ToString());
            string encodedFileListString = Encoding.UTF8.GetString(encodedBytes);

            return encodedFileListString;
        }
        catch (Exception e)
        {
            return "Ошибка при получении списка файлов: " + e.Message;
        }
    }
}
