using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Connection_Form;

namespace Server
{
    public partial class Form1 : Form
    {
        private int remotePort = 12345; // Порт удаленного узла
        private Socket socket;
        private string newAddress;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form splashScreen = new Splash_Screen.Splash_Screen();
            splashScreen.ShowDialog();
        }

        private void ConnectToServer()
        {
            try
            {
                // Создание TCP-сокета
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Установка соединения с удаленным узлом
                socket.Connect(newAddress, remotePort);
                string input = "LIST";

                // Отправка введенного текста на сервер
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                socket.Send(inputBytes);

                // Получение ответа от сервера
                byte[] responseBytes = new byte[1024];
                int bytesRead = socket.Receive(responseBytes);
                string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRead);
                Console.WriteLine("Ответ от сервера: " + response);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к серверу: " + ex.Message);
            }
        }

        private void RefreshFileList()
        {
            try
            {
                // Отправка команды получения списка файлов и папок
                string fileListCommand = "LIST";
                byte[] commandBytes = Encoding.ASCII.GetBytes(fileListCommand);
                socket.Send(commandBytes);

                // Получение списка файлов и папок от удаленного узла
                byte[] buffer = new byte[1024];
                int bytesRead = socket.Receive(buffer);
                string fileList = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Очистка ListBox
                listBoxFiles.Items.Clear();

                // Добавление файлов и папок в ListBox
                string[] filesAndFolders = fileList.Split('|');
                foreach (string fileOrFolder in filesAndFolders)
                {
                    listBoxFiles.Items.Add(fileOrFolder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении списка файлов и папок: " + ex.Message);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            string selectedFile = listBoxFiles.SelectedItem?.ToString();
            if (selectedFile != null)
            {
                DeleteFile(selectedFile);
            }
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            string selectedFile = listBoxFiles.SelectedItem?.ToString();
            if (selectedFile != null)
            {
                string newFileName = textBox1.Text;
                if (!string.IsNullOrEmpty(newFileName))
                {
                    RenameFile(selectedFile, newFileName);
                }
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            string selectedFile = listBoxFiles.SelectedItem?.ToString();
            if (selectedFile != null)
            {
                DownloadFile(selectedFile);
            }
        }

        private void buttonUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                UploadFile(openFileDialog.FileName);
            }
        }

        private void DeleteFile(string fileName)
        {
            try
            {
                // Создание TCP-сокета
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    // Установка соединения с удаленным узлом
                    socket.Connect(newAddress, remotePort);

                    // Отправка команды удаления файла
                    string deleteCommand = $"DELETE|{fileName}";
                    byte[] commandBytes = System.Text.Encoding.ASCII.GetBytes(deleteCommand);
                    socket.Send(commandBytes);

                    // Ожидание подтверждения от удаленного узла
                    byte[] confirmationBuffer = new byte[2];
                    socket.Receive(confirmationBuffer);
                    string confirmation = System.Text.Encoding.ASCII.GetString(confirmationBuffer);

                    if (confirmation == "OK")
                    {
                        MessageBox.Show("Файл успешно удален на сервере");
                        RefreshFileList();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка подтверждения от удаленного узла");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении файла: " + ex.Message);
            }
        }

        private void RenameFile(string oldFileName, string newFileName)
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(newAddress, remotePort);

                    string renameCommand = $"RENAME|{oldFileName}|{newFileName}";
                    byte[] commandBytes = System.Text.Encoding.ASCII.GetBytes(renameCommand);
                    socket.Send(commandBytes);

                    byte[] confirmationBuffer = new byte[2];
                    socket.Receive(confirmationBuffer);
                    string confirmation = System.Text.Encoding.ASCII.GetString(confirmationBuffer);

                    if (confirmation == "OK")
                    {
                        MessageBox.Show("Файл успешно переименован на сервере");
                        RefreshFileList();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка подтверждения от удаленного узла");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при переименовании файла: " + ex.Message);
            }
        }

        private void DownloadFile(string fileName)
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(newAddress, remotePort);

                    string downloadCommand = $"DOWNLOAD|{fileName}";
                    byte[] commandBytes = System.Text.Encoding.ASCII.GetBytes(downloadCommand);
                    socket.Send(commandBytes);

                    byte[] buffer = new byte[1024];
                    int bytesRead = socket.Receive(buffer);

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = fileName;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                        }

                        MessageBox.Show("Файл успешно загружен");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке файла: " + ex.Message);
            }
        }

        private void UploadFile(string filePath)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(newAddress, remotePort);

                string uploadCommand = $"UPLOAD|{Path.GetFileName(filePath)}";
                byte[] commandBytes = System.Text.Encoding.ASCII.GetBytes(uploadCommand);
                socket.Send(commandBytes);

                byte[] buffer = new byte[1024];
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        socket.Send(buffer, bytesRead, SocketFlags.None);
                    }
                }

                MessageBox.Show("Файл успешно отправлен на сервер");
                RefreshFileList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке файла: " + ex.Message);
            }
        }
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshFileList();
        }

        private void подключитьсяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var connection_Form = new Connection_Form.Connection_Form();
            connection_Form.ShowDialog();
            newAddress = connection_Form.RemoteAddress;
            label3.Text = "\r\nIP-Адрес: " + newAddress + "\r\nПорт:" + remotePort;
            ConnectToServer();
        }
    }
}