namespace Connection_Form
{
    partial class Connection_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_Connection = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_Address = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button_Connection
            // 
            this.button_Connection.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button_Connection.Location = new System.Drawing.Point(137, 175);
            this.button_Connection.Name = "button_Connection";
            this.button_Connection.Size = new System.Drawing.Size(256, 58);
            this.button_Connection.TabIndex = 5;
            this.button_Connection.Text = "Войти на сервер";
            this.button_Connection.UseVisualStyleBackColor = true;
            this.button_Connection.Click += new System.EventHandler(this.button_Connection_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(77, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(425, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Введите имя сервера(хоста) или IPv4-адрес";
            // 
            // textBox_Address
            // 
            this.textBox_Address.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox_Address.Location = new System.Drawing.Point(23, 104);
            this.textBox_Address.Name = "textBox_Address";
            this.textBox_Address.Size = new System.Drawing.Size(532, 30);
            this.textBox_Address.TabIndex = 3;
            // 
            // Connection_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 258);
            this.Controls.Add(this.button_Connection);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_Address);
            this.Name = "Connection_Form";
            this.Text = "Подключение к серверу";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Connection;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_Address;
    }
}