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
    public partial class RegisterForm : Form
    {
        private string connectionString = @"Data Source=PC\SQLEXPRESS;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        public RegisterForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Регистрация";
            this.Size = new System.Drawing.Size(370, 500);

            Label lblTitle = new Label { Text = "Регистрация нового пользователя", Font = new Font("Arial", 14, FontStyle.Bold), AutoSize = true };
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.Width) / 40, 30);
            this.Controls.Add(lblTitle);

            Label lblLogin = new Label { Text = "Логин:", AutoSize = true };
            lblLogin.Location = new Point(95, 60);
            this.Controls.Add(lblLogin);

            TextBox txtLogin = new TextBox { Name = "txtLogin", Location = new Point(140, 60), Width = 200 };
            this.Controls.Add(txtLogin);

            Label lblPassword = new Label { Text = "Пароль:", AutoSize = true };
            lblPassword.Location = new Point(90, 90);
            this.Controls.Add(lblPassword);

            TextBox txtPassword = new TextBox { Name = "txtPassword", Location = new Point(140, 90), Width = 200 };
            txtPassword.UseSystemPasswordChar = true;
            this.Controls.Add(txtPassword);

            Label lblConfirmPassword = new Label { Text = "Подтвердите пароль:", AutoSize = true };
            lblConfirmPassword.Location = new Point(20, 120);
            this.Controls.Add(lblConfirmPassword);

            TextBox txtConfirmPassword = new TextBox { Name = "txtConfirmPassword", Location = new Point(140, 120), Width = 200 };
            txtConfirmPassword.UseSystemPasswordChar = true;
            this.Controls.Add(txtConfirmPassword);

            Label lblFirstName = new Label { Text = "Имя:", AutoSize = true };
            lblFirstName.Location = new Point(95, 150);
            this.Controls.Add(lblFirstName);

            TextBox txtFirstName = new TextBox { Name = "txtFirstName", Location = new Point(140, 150), Width = 200 };
            this.Controls.Add(txtFirstName);

            Label lblLastName = new Label { Text = "Фамилия:", AutoSize = true };
            lblLastName.Location = new Point(80, 180);
            this.Controls.Add(lblLastName);

            TextBox txtLastName = new TextBox { Name = "txtLastName", Location = new Point(140, 180), Width = 200 };
            this.Controls.Add(txtLastName);

            Label lblEmail = new Label { Text = "Email:", AutoSize = true };
            lblEmail.Location = new Point(90, 210);
            this.Controls.Add(lblEmail);

            TextBox txtEmail = new TextBox { Name = "txtEmail", Location = new Point(140, 210), Width = 200 };
            this.Controls.Add(txtEmail);

            Label lblPhone = new Label { Text = "Телефон:", AutoSize = true };
            lblPhone.Location = new Point(80, 240);
            this.Controls.Add(lblPhone);

            TextBox txtPhone = new TextBox { Name = "txtPhone", Location = new Point(140, 240), Width = 200 };
            this.Controls.Add(txtPhone);

            Label lblRole = new Label { Text = "Роль:", AutoSize = true };
            lblRole.Location = new Point(95, 270);
            this.Controls.Add(lblRole);

            ComboBox cmbRole = new ComboBox { Name = "cmbRole", Location = new Point(140, 270), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new string[] { "Клиент", "Сотрудник", "Администратор" });
            cmbRole.SelectedIndex = 0;
            this.Controls.Add(cmbRole);

            Button btnBack = new Button { Text = "Войти", Location = new Point(210, 310), Width = 80 };
            btnBack.Click += (s, e) => BackButton_Click();
            this.Controls.Add(btnBack);

            Label lblError = new Label { Name = "lblError", ForeColor = System.Drawing.Color.Red, AutoSize = true };
            lblError.Location = new Point(20, 350);
            lblError.MaximumSize = new Size(330, 0);
            this.Controls.Add(lblError);

            Button btnRegister = new Button { Text = "Зарегистрироваться", Location = new Point(50, 310), Width = 150 };
            btnRegister.Click += (s, e) => RegisterButton_Click(s, e, txtLogin, txtPassword, txtConfirmPassword, txtFirstName, txtLastName, txtEmail, txtPhone, cmbRole, lblError);
            this.Controls.Add(btnRegister);

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

        private void RegisterButton_Click(object sender, EventArgs e, TextBox txtLogin, TextBox txtPassword, TextBox txtConfirmPassword, TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail, TextBox txtPhone, ComboBox cmbRole, Label lblError)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string role = cmbRole.SelectedItem.ToString();
            lblError.Text = "";

            // Проверка заполненности всех полей
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) ||
                string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
            {
                lblError.Text = "Заполните все поля.";
                return;
            }

            // Проверка длины пароля
            if (password.Length < 6)
            {
                lblError.Text = "Пароль должен содержать минимум 6 символов.";
                return;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                lblError.Text = "Пароли не совпадают.";
                return;
            }

            // Проверка уникальности логина
            if (LoginExists(login))
            {
                lblError.Text = "Такой аккаунт уже существует.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Вставка в Users
                    string userQuery = @"
                    INSERT INTO Users ([Login], [Password], [Role])
                    OUTPUT INSERTED.UserID
                    VALUES (@Login, @Password, @Role)";
                    int userId;
                    using (SqlCommand userCommand = new SqlCommand(userQuery, conn))
                    {
                        userCommand.Parameters.AddWithValue("@Login", login);
                        userCommand.Parameters.AddWithValue("@Password", password);
                        userCommand.Parameters.AddWithValue("@Role", role);
                        try
                        {
                            userId = (int)userCommand.ExecuteScalar();
                        }
                        catch (SqlException ex) when (ex.Number == 2627) // Нарушение уникальности Login
                        {
                            lblError.Text = "Такой аккаунт уже существует.";
                            return;
                        }
                    }

                    // Вставка в Clients, если роль - Клиент
                    if (role == "Клиент")
                    {
                        string clientQuery = @"
                        INSERT INTO Clients (UserID, FirstName, LastName, Email, Phone)
                        VALUES (@UserID, @FirstName, @LastName, @Email, @Phone)";
                        using (SqlCommand clientCommand = new SqlCommand(clientQuery, conn))
                        {
                            clientCommand.Parameters.AddWithValue("@UserID", userId);
                            clientCommand.Parameters.AddWithValue("@FirstName", firstName);
                            clientCommand.Parameters.AddWithValue("@LastName", lastName);
                            clientCommand.Parameters.AddWithValue("@Email", email);
                            clientCommand.Parameters.AddWithValue("@Phone", phone);
                            try
                            {
                                clientCommand.ExecuteNonQuery();
                            }
                            catch (SqlException ex) when (ex.Number == 2627) // Нарушение уникальности Email
                            {
                                // Удаляем пользователя из Users
                                string deleteUserQuery = "DELETE FROM Users WHERE UserID = @UserID";
                                using (SqlCommand deleteCommand = new SqlCommand(deleteUserQuery, conn))
                                {
                                    deleteCommand.Parameters.AddWithValue("@UserID", userId);
                                    deleteCommand.ExecuteNonQuery();
                                }
                                lblError.Text = "Такой Email уже используется.";
                                return;
                            }
                        }
                    }

                    MessageBox.Show("Регистрация успешна!");
                    this.Hide();
                    new LoginForm().Show();
                }
                catch (Exception ex)
                {
                    lblError.Text = "Ошибка при регистрации: " + ex.Message;
                }
            }
        }

        private void BackButton_Click()
        {
            this.Hide();
            new LoginForm().Show();
        }

        private bool LoginExists(string login)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE [Login] = @Login";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Login", login);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
