using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Recording_of_requests_for_equipment_repair
{
    public partial class LoginForm : Form
    {
        private string connectionString = @"Data Source=PC\SQLEXPRESS;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        public LoginForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Авторизация";
            this.Size = new System.Drawing.Size(300, 200);

            Label lblTitle = new Label { Text = "Вход в систему", Font = new Font("Arial", 14, FontStyle.Bold), AutoSize = true };
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.Width) / 3, 20);
            this.Controls.Add(lblTitle);

            Label lblLogin = new Label { Text = "Логин:", AutoSize = true };
            lblLogin.Location = new Point(20, 60);
            this.Controls.Add(lblLogin);

            TextBox txtLogin = new TextBox { Name = "txtLogin", Location = new Point(100, 60), Width = 150 };
            this.Controls.Add(txtLogin);

            Label lblPassword = new Label { Text = "Пароль:", AutoSize = true };
            lblPassword.Location = new Point(20, 90);
            this.Controls.Add(lblPassword);

            TextBox txtPassword = new TextBox { Name = "txtPassword", Location = new Point(100, 90), Width = 150 };
            txtPassword.UseSystemPasswordChar = true;
            this.Controls.Add(txtPassword);

            Button btnRegister = new Button { Text = "Регистрация", Location = new Point(190, 130), Width = 80 };
            btnRegister.Click += (s, e) => RegisterButton_Click();
            this.Controls.Add(btnRegister);

            Label lblError = new Label { Name = "lblError", ForeColor = System.Drawing.Color.Red, AutoSize = true };
            lblError.Location = new Point(20, 160);
            this.Controls.Add(lblError);

            Button btnLogin = new Button { Text = "Войти", Location = new Point(100, 130), Width = 80 };
            btnLogin.Click += (s, e) => LoginButton_Click(s, e, txtLogin, txtPassword, lblError);
            this.Controls.Add(btnLogin);

            // Кастомный крестик для выхода
            Button btnClose = new Button
            {
                Text = "X",
                Location = new Point(this.ClientSize.Width - 30, 5),
                Size = new System.Drawing.Size(25, 25),
                FlatStyle = FlatStyle.Flat,
                ForeColor = System.Drawing.Color.Red,
                BackColor = System.Drawing.Color.Transparent
            };
            btnClose.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnClose);
        }


        private void LoginButton_Click(object sender, EventArgs e, TextBox txtLogin, TextBox txtPassword, Label lblError)
        {
            //MessageBox.Show($"Успешный вход! Роль");
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();
            lblError.Text = "";


            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Заполните все поля.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT UserID, [Role] FROM Users WHERE [Login] = @Login AND [Password] = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(0);
                                string role = reader.GetString(1);

                                MessageBox.Show($"Успешный вход! Роль: {role}, UserID: {userId}");
                                this.Hide();

                                if (role == "Клиент")
                                {
                                    new ClientForm(userId).Show();
                                }
                                else if (role == "Сотрудник")
                                {
                                    new EmployeeForm(userId).Show();
                                }
                                else if (role == "Администратор")
                                {
                                    new AdminForm().Show();
                                }
                                else
                                {
                                    lblError.Text = "Неизвестная роль пользователя.";
                                    this.Show();
                                }
                            }
                            else
                            {
                                lblError.Text = "Неверный логин или пароль.";
                                MessageBox.Show("Ошибка: Неверный логин или пароль");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblError.Text = $"Ошибка подключения: {ex.Message}";
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }

        }


        private void RegisterButton_Click()
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
            this.Hide();
        }


       
    }
}
