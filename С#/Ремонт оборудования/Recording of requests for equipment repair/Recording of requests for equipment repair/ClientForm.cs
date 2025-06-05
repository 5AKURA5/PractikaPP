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
    public partial class ClientForm : Form
    {
        private readonly int userId;
        // Объявление компонентов
        private Label labelRequestNumber;
        private TextBox textBoxRequestNumber;
        private Label labelEquipment;
        private ComboBox comboBoxEquipment;
        private Label labelFailureType;
        private ComboBox comboBoxFailureType;
        private Label labelProblemDescription;
        private TextBox textBoxProblemDescription;
        private Label labelStatusLabel;
        private ComboBox comboBoxStatus;
        private Button buttonAddRequest;
        private Label labelStatus;
        private DataGridView dataGridViewRequests;
        private string connectionString = @"Data Source=PC\SQLEXPRESS;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        public ClientForm(int userId)
        {
            this.userId = userId;
            InitializeComponentsManually();
            LoadComboBoxes();
            GenerateRequestNumber();
            LoadUserRequests();
        }

        private void InitializeComponentsManually()
        {
            // Инициализация компонентов
            this.labelRequestNumber = new Label();
            this.textBoxRequestNumber = new TextBox();
            this.labelEquipment = new Label();
            this.comboBoxEquipment = new ComboBox();
            this.labelFailureType = new Label();
            this.comboBoxFailureType = new ComboBox();
            this.labelProblemDescription = new Label();
            this.textBoxProblemDescription = new TextBox();
            this.labelStatusLabel = new Label();
            this.comboBoxStatus = new ComboBox();
            this.buttonAddRequest = new Button();
            this.labelStatus = new Label();
            this.dataGridViewRequests = new DataGridView();

            // Настройка формы
            this.Text = "Управление заявками клиента";
            this.Size = new Size(800, 500);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // labelRequestNumber
            this.labelRequestNumber.Text = "Номер заявки:";
            this.labelRequestNumber.AutoSize = true;
            this.labelRequestNumber.Location = new Point(12, 15);
            this.labelRequestNumber.Size = new Size(80, 13);

            // textBoxRequestNumber
            this.textBoxRequestNumber.Location = new Point(120, 12);
            this.textBoxRequestNumber.Size = new Size(200, 20);
            this.textBoxRequestNumber.ReadOnly = false;

            // labelEquipment
            this.labelEquipment.Text = "Оборудование:";
            this.labelEquipment.AutoSize = true;
            this.labelEquipment.Location = new Point(12, 40);
            this.labelEquipment.Size = new Size(80, 13);

            // comboBoxEquipment
            this.comboBoxEquipment.Location = new Point(120, 37);
            this.comboBoxEquipment.Size = new Size(200, 21);
            this.comboBoxEquipment.DropDownStyle = ComboBoxStyle.DropDownList;

            // labelFailureType
            this.labelFailureType.Text = "Тип неисправности:";
            this.labelFailureType.AutoSize = true;
            this.labelFailureType.Location = new Point(12, 65);
            this.labelFailureType.Size = new Size(100, 13);

            // comboBoxFailureType
            this.comboBoxFailureType.Location = new Point(120, 62);
            this.comboBoxFailureType.Size = new Size(200, 21);
            this.comboBoxFailureType.DropDownStyle = ComboBoxStyle.DropDownList;

            // labelProblemDescription
            this.labelProblemDescription.Text = "Описание проблемы:";
            this.labelProblemDescription.AutoSize = true;
            this.labelProblemDescription.Location = new Point(12, 90);
            this.labelProblemDescription.Size = new Size(100, 13);

            // textBoxProblemDescription
            this.textBoxProblemDescription.Location = new Point(120, 87);
            this.textBoxProblemDescription.Size = new Size(200, 50);
            this.textBoxProblemDescription.Multiline = true;

            // labelStatusLabel
            this.labelStatusLabel.Text = "Статус:";
            this.labelStatusLabel.AutoSize = true;
            this.labelStatusLabel.Location = new Point(12, 145);
            this.labelStatusLabel.Size = new Size(50, 13);

            // comboBoxStatus
            this.comboBoxStatus.Location = new Point(120, 142);
            this.comboBoxStatus.Size = new Size(200, 21);
            this.comboBoxStatus.DropDownStyle = ComboBoxStyle.DropDownList;

            // buttonAddRequest
            this.buttonAddRequest.Text = "Добавить заявку";
            this.buttonAddRequest.Location = new Point(120, 170);
            this.buttonAddRequest.Size = new Size(100, 25);
            this.buttonAddRequest.Click += new EventHandler(buttonAddRequest_Click);

            // labelStatus
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new Point(12, 200);
            this.labelStatus.Size = new Size(0, 13);
            this.labelStatus.MaximumSize = new Size(760, 0); // Ограничение ширины

            // dataGridViewRequests
            this.dataGridViewRequests.Location = new Point(12, 225);
            this.dataGridViewRequests.Size = new Size(760, 235);
            this.dataGridViewRequests.ReadOnly = true;
            this.dataGridViewRequests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Добавление компонентов на форму
            this.Controls.AddRange(new Control[] {
            this.labelRequestNumber,
            this.textBoxRequestNumber,
            this.labelEquipment,
            this.comboBoxEquipment,
            this.labelFailureType,
            this.comboBoxFailureType,
            this.labelProblemDescription,
            this.textBoxProblemDescription,
            this.labelStatusLabel,
            this.comboBoxStatus,
            this.buttonAddRequest,
            this.labelStatus,
            this.dataGridViewRequests
        });


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


        private void LoadComboBoxes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Загрузка оборудования
                    string equipmentQuery = "SELECT EquipmentID, [Name] FROM Equipment";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(equipmentQuery, connection))
                    {
                        DataTable equipmentTable = new DataTable();
                        adapter.Fill(equipmentTable);
                        comboBoxEquipment.DataSource = equipmentTable;
                        comboBoxEquipment.DisplayMember = "[Name]";
                        comboBoxEquipment.ValueMember = "EquipmentID";
                    }

                    // Загрузка типов неисправностей
                    string failureQuery = "SELECT FailureTypeID, TypeName FROM FailureTypes";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(failureQuery, connection))
                    {
                        DataTable failureTable = new DataTable();
                        adapter.Fill(failureTable);
                        comboBoxFailureType.DataSource = failureTable;
                        comboBoxFailureType.DisplayMember = "TypeName";
                        comboBoxFailureType.ValueMember = "FailureTypeID";
                    }

                    // Загрузка статусов (по умолчанию "В ожидании")
                    string statusQuery = "SELECT StatusID, StatusName FROM RequestStatus";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(statusQuery, connection))
                    {
                        DataTable statusTable = new DataTable();
                        adapter.Fill(statusTable);
                        comboBoxStatus.DataSource = statusTable;
                        comboBoxStatus.DisplayMember = "StatusName";
                        comboBoxStatus.ValueMember = "StatusID";
                        comboBoxStatus.SelectedValue = GetStatusId("В ожидании");
                    }
                }
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Ошибка загрузки данных: {ex.Message}";
            }
        }


        private void GenerateRequestNumber()
        {
            textBoxRequestNumber.Text = "REQ-" + DateTime.Now.ToString("yMd");
        }

        private int GetStatusId(string statusName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT StatusID FROM RequestStatus WHERE StatusName = @StatusName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StatusName", statusName);
                    return (int)command.ExecuteScalar();
                }
            }
        }

        private int GetClientId()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ClientID FROM Clients WHERE UserID = @UserID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    return (int)command.ExecuteScalar();
                }
            }
        }

        private void LoadUserRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT r.RequestNumber, r.RequestDate, e.[Name] AS EquipmentName, 
                                f.TypeName AS FailureType, r.ProblemDescription, s.StatusName
                                FROM RepairRequests r
                                JOIN Equipment e ON r.EquipmentID = e.EquipmentID
                                JOIN FailureTypes f ON r.FailureTypeID = f.FailureTypeID
                                JOIN RequestStatus s ON r.StatusID = s.StatusID
                                JOIN Clients c ON r.ClientID = c.ClientID
                                WHERE c.UserID = @UserID";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@UserID", userId);
                        DataTable requestsTable = new DataTable();
                        adapter.Fill(requestsTable);
                        dataGridViewRequests.DataSource = requestsTable;

                        // Настройка заголовков столбцов
                        if (dataGridViewRequests.Columns.Contains("RequestNumber"))
                            dataGridViewRequests.Columns["RequestNumber"].HeaderText = "Номер заявки";
                        if (dataGridViewRequests.Columns.Contains("RequestDate"))
                            dataGridViewRequests.Columns["RequestDate"].HeaderText = "Дата";
                        if (dataGridViewRequests.Columns.Contains("EquipmentName"))
                            dataGridViewRequests.Columns["EquipmentName"].HeaderText = "Оборудование";
                        if (dataGridViewRequests.Columns.Contains("FailureType"))
                            dataGridViewRequests.Columns["FailureType"].HeaderText = "Тип неисправности";
                        if (dataGridViewRequests.Columns.Contains("ProblemDescription"))
                            dataGridViewRequests.Columns["ProblemDescription"].HeaderText = "Описание";
                        if (dataGridViewRequests.Columns.Contains("StatusName"))
                            dataGridViewRequests.Columns["StatusName"].HeaderText = "Статус";
                    }
                }
            }
            catch (Exception ex)
            {
                labelStatus.ForeColor = Color.Red;
                labelStatus.Text = $"Ошибка загрузки заявок: {ex.Message}";
            }
        }


        private void buttonAddRequest_Click(object sender, EventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(textBoxProblemDescription.Text))
                {
                    labelStatus.ForeColor = Color.Red;
                    labelStatus.Text = "Ошибка: Описание проблемы не может быть пустым.";
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка существования RequestNumber
                    string checkQuery = "SELECT COUNT(*) FROM RepairRequests WHERE RequestNumber = @RequestNumber";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@RequestNumber", textBoxRequestNumber.Text);
                        int count = (int)checkCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Такая заявка уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            textBoxRequestNumber.Text = "";
                            return;
                        }
                    }
                    string query = @"INSERT INTO RepairRequests (RequestNumber, RequestDate, EquipmentID, FailureTypeID, 
                                ProblemDescription, ClientID, StatusID)
                                VALUES (@RequestNumber, @RequestDate, @EquipmentID, @FailureTypeID, 
                                @ProblemDescription, @ClientID, @StatusID)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RequestNumber", textBoxRequestNumber.Text);
                        command.Parameters.AddWithValue("@RequestDate", DateTime.Now);
                        command.Parameters.AddWithValue("@EquipmentID", comboBoxEquipment.SelectedValue);
                        command.Parameters.AddWithValue("@FailureTypeID", comboBoxFailureType.SelectedValue);
                        command.Parameters.AddWithValue("@ProblemDescription", textBoxProblemDescription.Text);
                        command.Parameters.AddWithValue("@ClientID", GetClientId());
                        command.Parameters.AddWithValue("@StatusID", comboBoxStatus.SelectedValue);
                        command.ExecuteNonQuery();
                    }
                }

                labelStatus.ForeColor = Color.Green;
                labelStatus.Text = "Заявка успешно добавлена!";
                textBoxProblemDescription.Clear();
                GenerateRequestNumber();
                LoadUserRequests(); // Автоматическое обновление DataGridView
                textBoxRequestNumber.Text = "";            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Ошибка: {ex.Message}";
            }
        }


    }
}
