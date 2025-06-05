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
    public partial class EmployeeForm : Form
    {
        private string connectionString = @"Data Source=PC\SQLEXPRESS;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        private readonly int userId;

        // Объявление компонентов
        private Label labelRequests;
        private DataGridView dataGridViewRequests;
        private Label labelStatus;
        private ComboBox comboBoxStatus;
        private Button buttonChangeStatus;
        private Button buttonCompleteRequest;
        private Label labelMessage;
        public EmployeeForm(int userId)
        {
            InitializeComponentsManually();
            LoadStatusComboBox();
            LoadEmployeeRequests();
        }

        private void InitializeComponentsManually()
        {
            // Инициализация компонентов
            this.labelRequests = new Label();
            this.dataGridViewRequests = new DataGridView();
            this.labelStatus = new Label();
            this.comboBoxStatus = new ComboBox();
            this.buttonChangeStatus = new Button();
            this.buttonCompleteRequest = new Button();
            this.labelMessage = new Label();

            // Настройка формы
            this.Text = "Управление заявками сотрудника";
            this.Size = new Size(800, 500);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // labelRequests
            this.labelRequests.Text = "Все заявки:";
            this.labelRequests.AutoSize = true;
            this.labelRequests.Location = new Point(12, 15);
            this.labelRequests.Size = new Size(100, 13);

            // dataGridViewRequests
            this.dataGridViewRequests.Location = new Point(12, 40);
            this.dataGridViewRequests.Size = new Size(760, 350);
            this.dataGridViewRequests.ReadOnly = true;
            this.dataGridViewRequests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewRequests.MultiSelect = false;

            // labelStatus
            this.labelStatus.Text = "Новый статус:";
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new Point(12, 400);
            this.labelStatus.Size = new Size(80, 13);

            // comboBoxStatus
            this.comboBoxStatus.Location = new Point(100, 397);
            this.comboBoxStatus.Size = new Size(200, 21);
            this.comboBoxStatus.DropDownStyle = ComboBoxStyle.DropDownList;

            // buttonChangeStatus
            this.buttonChangeStatus.Text = "Изменить статус";
            this.buttonChangeStatus.Location = new Point(310, 397);
            this.buttonChangeStatus.Size = new Size(120, 25);
            this.buttonChangeStatus.Click += new EventHandler(buttonChangeStatus_Click);

            // buttonCompleteRequest
            this.buttonCompleteRequest.Text = "Завершить заявку";
            this.buttonCompleteRequest.Location = new Point(440, 397);
            this.buttonCompleteRequest.Size = new Size(120, 25);
            this.buttonCompleteRequest.Click += new EventHandler(buttonCompleteRequest_Click);

            // labelMessage
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new Point(12, 430);
            this.labelMessage.Size = new Size(0, 13);
            this.labelMessage.MaximumSize = new Size(760, 0);
            this.labelMessage.ForeColor = Color.Red;

            // Добавление компонентов
            this.Controls.AddRange(new Control[] {
            this.labelRequests,
            this.dataGridViewRequests,
            this.labelStatus,
            this.comboBoxStatus,
            this.buttonChangeStatus,
            this.buttonCompleteRequest,
            this.labelMessage
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

        private void LoadStatusComboBox()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT StatusID, StatusName FROM RequestStatus";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable statusTable = new DataTable();
                        adapter.Fill(statusTable);
                        comboBoxStatus.DataSource = statusTable;
                        comboBoxStatus.DisplayMember = "StatusName";
                        comboBoxStatus.ValueMember = "StatusID";
                    }
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка загрузки статусов: {ex.Message}";
            }
        }

        private void LoadEmployeeRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT r.RequestID, r.RequestNumber, r.RequestDate, e.[Name] AS Equipment, 
                           ft.TypeName AS FailureType, c.FirstName + ' ' + c.LastName AS Client, 
                           rs.StatusName, r.AssignedEmployeeID, em.FirstName + ' ' + em.LastName AS EmployeeName
                    FROM RepairRequests r
                    LEFT JOIN Equipment e ON r.EquipmentID = e.EquipmentID
                    LEFT JOIN FailureTypes ft ON r.FailureTypeID = ft.FailureTypeID
                    LEFT JOIN Clients c ON r.ClientID = c.ClientID
                    LEFT JOIN RequestStatus rs ON r.StatusID = rs.StatusID
                    LEFT JOIN Employees em ON r.AssignedEmployeeID = em.EmployeeID";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable requestsTable = new DataTable();
                        adapter.Fill(requestsTable);
                        dataGridViewRequests.DataSource = requestsTable;

                        // Отладка: Проверяем количество записей
                        labelMessage.Text = $"Найдено заявок: {requestsTable.Rows.Count}";

                        // Настройка заголовков
                        if (dataGridViewRequests.Columns.Contains("RequestID"))
                            dataGridViewRequests.Columns["RequestID"].HeaderText = "ID заявки";
                        if (dataGridViewRequests.Columns.Contains("RequestNumber"))
                            dataGridViewRequests.Columns["RequestNumber"].HeaderText = "Номер заявки";
                        if (dataGridViewRequests.Columns.Contains("RequestDate"))
                            dataGridViewRequests.Columns["RequestDate"].HeaderText = "Дата";
                        if (dataGridViewRequests.Columns.Contains("Equipment"))
                            dataGridViewRequests.Columns["Equipment"].HeaderText = "Оборудование";
                        if (dataGridViewRequests.Columns.Contains("FailureType"))
                            dataGridViewRequests.Columns["FailureType"].HeaderText = "Тип неисправности";
                        if (dataGridViewRequests.Columns.Contains("Client"))
                            dataGridViewRequests.Columns["Client"].HeaderText = "Клиент";
                        if (dataGridViewRequests.Columns.Contains("StatusName"))
                            dataGridViewRequests.Columns["StatusName"].HeaderText = "Статус";
                        if (dataGridViewRequests.Columns.Contains("AssignedEmployeeID"))
                            dataGridViewRequests.Columns["AssignedEmployeeID"].HeaderText = "ID сотрудника";
                        if (dataGridViewRequests.Columns.Contains("EmployeeName"))
                            dataGridViewRequests.Columns["EmployeeName"].HeaderText = "Сотрудник";
                    }
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка загрузки заявок: {ex.Message}";
            }
        }

        private void buttonChangeStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewRequests.SelectedRows.Count == 0)
                {
                    labelMessage.Text = "Ошибка: Выберите заявку.";
                    return;
                }

                if (comboBoxStatus.SelectedValue == null)
                {
                    labelMessage.Text = "Ошибка: Выберите статус.";
                    return;
                }

                string requestNumber = dataGridViewRequests.SelectedRows[0].Cells["RequestNumber"].Value.ToString();
                int newStatusId = (int)comboBoxStatus.SelectedValue;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE RepairRequests SET StatusID = @StatusID WHERE RequestNumber = @RequestNumber";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StatusID", newStatusId);
                        command.Parameters.AddWithValue("@RequestNumber", requestNumber);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            labelMessage.Text = "Ошибка: Заявка не найдена.";
                            return;
                        }
                    }
                }

                labelMessage.Text = "Статус заявки успешно изменен!";
                LoadEmployeeRequests();
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void buttonCompleteRequest_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewRequests.SelectedRows.Count == 0)
                {
                    labelMessage.Text = "Ошибка: Выберите заявку.";
                    return;
                }

                string requestNumber = dataGridViewRequests.SelectedRows[0].Cells["RequestNumber"].Value.ToString();
                int completedStatusId = GetStatusId("Выполнено");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE RepairRequests SET StatusID = @StatusID WHERE RequestNumber = @RequestNumber";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StatusID", completedStatusId);
                        command.Parameters.AddWithValue("@RequestNumber", requestNumber);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            labelMessage.Text = "Ошибка: Заявка не найдена.";
                            return;
                        }
                    }
                }

                labelMessage.Text = "Заявка успешно завершена!";
                LoadEmployeeRequests();
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка: {ex.Message}";
            }
        }

        private int GetStatusId(string statusName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT StatusID FROM RequestStatus WHERE StatusName = @StatusName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StatusName", statusName);
                        object result = command.ExecuteScalar();
                        if (result == null)
                            throw new Exception($"Статус '{statusName}' не найден.");
                        return (int)result;
                    }
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка получения статуса: {ex.Message}";
                throw;
            }
        }


    }
}
