using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Connection_Form
{
    public partial class Connection_Form : Form
    {
        public string RemoteAddress
        {
            get => remoteAddress;
            set => remoteAddress = value;
        } 
        private string remoteAddress;
        public Connection_Form()
        {
            InitializeComponent();
        }

        private void button_Connection_Click(object sender, EventArgs e)
        {
            string data = textBox_Address.Text;
            try
            {
                if (data == "")
                {
                    MessageBox.Show("Поле ввода осталось незаполненным!");
                }
                else
                {
                    IPAddress iP;
                    try
                    {
                        iP = IPAddress.Parse(data);

                        // Проверка, находится ли IP-адрес в локальной сети
                        Ping ping = new Ping();
                        PingReply reply = ping.Send(iP, 1000);
                        if (reply.Status == IPStatus.Success)
                        {
                            IPHostEntry host = Dns.GetHostEntry(iP);
                            if (host != null)
                            {
                                remoteAddress = data;
                                textBox_Address.Text = "";
                                MessageBox.Show("Данный хост существует!\nИдет проверка на наличие сервера на нем...");
                                this.Hide();
                            }
                            else
                                MessageBox.Show("Не удалось найти сервер по заданному IP!");
                        }
                        else
                        {
                            MessageBox.Show($"Не удалось подключиться к {data} - данный IP-адрес отсутствует в локальной сети.");
                            textBox_Address.Text = "";
                        }
                    }
                    catch
                    {
                        IPAddress[] addresses = Dns.GetHostAddresses(data);
                        IPAddress[] ipv4Addresses = addresses.Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToArray();
                        if (ipv4Addresses.Length > 0)
                        {
                            remoteAddress = ipv4Addresses[0].ToString();
                            textBox_Address.Text = "";
                            MessageBox.Show("Данный хост существует!\nИдет проверка на наличие сервера на нем...");
                            this.Hide();
                        }
                        else
                            MessageBox.Show("Не удалось найти сервер по заданному имени хоста!");
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"{data} - этот хост неизвестен!\r\nПричиной проблемы может быть следующее:\r\n  1) Неверное имя хоста или IP-адрес\r\n  2) Отсутствие подключения к Интернету.");
                textBox_Address.Text = "Добавить сервер не удалось..";
            }
        }
    }
}