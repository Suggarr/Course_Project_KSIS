using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Connection_Form;

namespace Client
{
    public partial class ClientForm : Form
    {
        private int remotePort = 12345; // Порт удаленного узла
        private string newAddress;
        private About_Author aboutAuthor;
        private About_Program aboutProgram;

        public ClientForm()
        {
            InitializeComponent();
            listViewFiles.Columns[0].Width = 327;
            listViewFiles.Columns[1].Width = 257;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form splashScreen = new Splash_Screen.Splash_Screen();
            splashScreen.ShowDialog();
            var connection_Form = new Connection_Form.Connection_Form();
            connection_Form.ShowDialog();
            newAddress = connection_Form.RemoteAddress;
            label3.Text = "\r\nХост: " + newAddress + "\r\nПорт:" + remotePort;
            RefreshFileList();
        }

        
        private void RefreshFileList()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    // Подключение к серверу
                    IPAddress serverIP = IPAddress.Parse(newAddress);
                    IPEndPoint serverEndPoint = new IPEndPoint(serverIP, remotePort);
                    socket.Connect(serverEndPoint);

                    // Отправка команды получения списка файлов и папок
                    string fileListCommand = "LIST|";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(fileListCommand);
                    socket.Send(commandBytes);

                    // Получение списка файлов и папок от сервера
                    byte[] buffer = new byte[1024];
                    int bytesRead = socket.Receive(buffer);
                    string fileList = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Очистка ListView
                    listViewFiles.Items.Clear();

                    // Добавление файлов и папок в ListView
                    string[] filesAndFolders = fileList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string fileOrFolder in filesAndFolders)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(fileOrFolder);
                        string fileType = Path.GetExtension(fileOrFolder);
                        ListViewItem item = new ListViewItem(fileName);
                        item.SubItems.Add(fileType);
                        listViewFiles.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении списка файлов: " + ex.Message);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                ListViewItem selectedFile = listViewFiles.SelectedItems[0];
                string selectedName = selectedFile.Text; // Значение первого столбца
                string selectedType = selectedFile.SubItems[1].Text;
                DeleteFile(selectedName, selectedType);
            }
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                string selectedFile = listViewFiles.SelectedItems[0].Text;
                string fileType = listViewFiles.SelectedItems[0].SubItems[1].Text;
                string newFileName = textBox1.Text;
                if (!string.IsNullOrEmpty(newFileName))
                {
                    string newFileNameWithType = newFileName + fileType;
                    RenameFile(selectedFile+fileType, newFileNameWithType);
                }
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                string selectedFile = listViewFiles.SelectedItems[0].Text;
                string fileType = listViewFiles.SelectedItems[0].SubItems[1].Text;
                DownloadFile(selectedFile, fileType);
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

        private void DeleteFile(string fileName, string fileType)
        {
            try
            {
                // Создание TCP-сокета
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    // Установка соединения с удаленным узлом
                    socket.Connect(newAddress, remotePort);

                    // Отправка команды удаления файла
                    string deleteCommand = $"DELETE|{fileName+fileType}";
                    byte[] commandBytes = System.Text.Encoding.UTF8.GetBytes(deleteCommand);
                    socket.Send(commandBytes);

                    // Ожидание подтверждения от удаленного узла
                    byte[] confirmationBuffer = new byte[2];
                    socket.Receive(confirmationBuffer);
                    string confirmation = System.Text.Encoding.UTF8.GetString(confirmationBuffer);

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

        private void RenameFile(string oldFileName, string newFileName)//работает
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(newAddress, remotePort);

                    string renameCommand = $"RENAME|{oldFileName}|{newFileName}";
                    byte[] commandBytes = System.Text.Encoding.UTF8.GetBytes(renameCommand);
                    socket.Send(commandBytes);

                    // Ожидание подтверждения от удаленного узла
                    byte[] confirmationBuffer = new byte[2];
                    socket.Receive(confirmationBuffer);
                    string confirmation = System.Text.Encoding.UTF8.GetString(confirmationBuffer);

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

        private void DownloadFile(string fileName, string fileType)
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(newAddress, remotePort);

                    string downloadCommand = $"DOWNLOAD|{fileName + fileType}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(downloadCommand);
                    socket.Send(commandBytes);

                    byte[] fileSizeBytes = new byte[8];
                    int bytesReceived = socket.Receive(fileSizeBytes);
                    long fileSize = BitConverter.ToInt64(fileSizeBytes, 0);
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = fileName + fileType;
                    saveFileDialog.Filter = $"{fileType} files|*{fileType}|All files|*.*";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                        {
                            using (BinaryWriter writer = new BinaryWriter(fileStream))
                            {
                                byte[] buffer = new byte[1024];
                                int bytesRead;
                                long bytesRemaining = fileSize;

                                while (bytesRemaining > 0 && (bytesRead = socket.Receive(buffer)) > 0)
                                {//Этот блок кода читает данные файла из сокета небольшими порциями и записывает их в FileStream с помощью BinaryWriter. Цикл продолжается, пока не будут получены и записаны все данные файла.
                                    int bytesToWrite = (int)Math.Min(bytesRemaining, bytesRead);
                                    writer.Write(buffer, 0, bytesToWrite);
                                    bytesRemaining -= bytesToWrite;
                                }
                            }
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
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(newAddress, remotePort);

                    // Отправка команды загрузки файла
                    string uploadCommand = $"UPLOAD|{Path.GetFileName(filePath)}";
                    byte[] commandBytes = Encoding.UTF8.GetBytes(uploadCommand);
                    socket.Send(commandBytes);

                    // Получение подтверждения от сервера
                    byte[] confirmationBytes = new byte[2];
                    int bytesRead = socket.Receive(confirmationBytes);
                    string confirmation = Encoding.UTF8.GetString(confirmationBytes, 0, bytesRead);

                    if (confirmation == "OK")
                    {
                        // Отправка размера файла
                        long fileSize = new FileInfo(filePath).Length;
                        byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
                        socket.Send(fileSizeBytes);

                        // Отправка файла по частям
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[1024];
                            bytesRead = 0;
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                socket.Send(buffer, bytesRead, SocketFlags.None);
                            }
                        }
                        RefreshFileList();
                        MessageBox.Show("Файл успешно отправлен на сервер");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось получить подтверждение от сервера");
                    }
                }
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
            label3.Text = "\r\nХост: " + newAddress + "\r\nПорт:" + remotePort;
            RefreshFileList();
        }

        private void выйтиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutProgram = new About_Program();
            aboutProgram.Show();
        }

        private void обАвтореToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutAuthor = new About_Author();
            aboutAuthor.Show();
        }
    }
}