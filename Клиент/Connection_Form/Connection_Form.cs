using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
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
        } // Поле для хранения IP-адреса
        string remoteAddress;
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
                        IPHostEntry host = Dns.GetHostEntry(iP);
                        if (host != null)
                        {
                            remoteAddress = data;
                            textBox_Address.Text = "";
                            MessageBox.Show("Сервер найден!");
                            this.Hide();
                        }
                        else
                            MessageBox.Show("Не удалось найти сервер по заданному IP!");
                    }
                    catch
                    {
                        IPHostEntry host = Dns.GetHostEntry(data);
                        if (host != null)
                        {
                            remoteAddress = data;
                            textBox_Address.Text = "";
                            MessageBox.Show("Сервер найден!");
                            this.Hide();
                        }
                        else
                            MessageBox.Show("Не удалось найти сервер по заданному имени хоста!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{data} - этот хост неизвестен!\r\nПричиной проблемы может быть следующее:\r\n  1) Неверное имя хоста или IP-адрес\r\n  2) Отсутствие подключения к Интернету. \r\nОшибка: {ex}");
                textBox_Address.Text = "Добавить сервер не удалось..";
            }
        }

    }
}
