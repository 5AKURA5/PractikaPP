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
    public partial class AdminForm : Form
    {
        private string connectionString = @"Data Source=PC\SQLEXPRESS;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        public AdminForm()
        {
            InitializeComponent();
            InitializeUI();
            LoadRequests();
        }

        private void InitializeUI()
        {
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new System.Drawing.Size(800, 600);

            Label lblTitle = new Label { Text = "Панель администратора", Font = new Font("Arial", 14, FontStyle.Bold), AutoSize = true };
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.Width) / 2, 20);
            this.Controls.Add(lblTitle);

            DataGridView dgvRequests = new DataGridView
            {
                Name = "dgvRequests",
                Location = new Point(20, 60),
                Size = new System.Drawing.Size(this.ClientSize.Width - 40, 400),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            this.Controls.Add(dgvRequests);

            Button btnDeleteRequest = new Button { Text = "Удалить заявку", Location = new Point(20, 470), Width = 120 };
            btnDeleteRequest.Click += (s, e) => DeleteRequest(dgvRequests);
            this.Controls.Add(btnDeleteRequest);

            Button btnAssignEmployee = new Button { Text = "Назначить сотрудника", Location = new Point(150, 470), Width = 150 };
            btnAssignEmployee.Click += (s, e) => AssignEmployee(dgvRequests);
            this.Controls.Add(btnAssignEmployee);

            Label lblStatus = new Label { Name = "lblStatus", AutoSize = true, Location = new Point(20, 510) };
            this.Controls.Add(lblStatus);

            Button btnViewDetails = new Button { Text = "Просмотреть детали", Location = new Point(440, 470), Width = 140 };
            btnViewDetails.Click += (s, e) => ViewDetails(dgvRequests);
            this.Controls.Add(btnViewDetails);
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

        private void LoadRequests()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
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
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    DataGridView dgvRequests = this.Controls.Find("dgvRequests", true)[0] as DataGridView;
                    dgvRequests.DataSource = dt;
                }
                catch (Exception ex)
                {
                    Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                    lblStatus.Text = "Ошибка загрузки: " + ex.Message;
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void DeleteRequest(DataGridView dgvRequests)
        {
            if (dgvRequests.SelectedRows.Count > 0)
            {
                int requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM RepairRequests WHERE RequestID = @RequestID";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@RequestID", requestId);
                        cmd.ExecuteNonQuery();
                        LoadRequests(); // Автоматическое обновление таблицы
                        Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                        lblStatus.Text = "Заявка удалена.";
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                    }
                    catch (Exception ex)
                    {
                        Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                        lblStatus.Text = "Ошибка удаления: " + ex.Message;
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            else
            {
                Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                lblStatus.Text = "Выберите заявку для удаления.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void AssignEmployee(DataGridView dgvRequests)
        {
            if (dgvRequests.SelectedRows.Count > 0)
            {
                int requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
                using (Form assignForm = new Form { Text = "Назначить сотрудника", Size = new System.Drawing.Size(300, 200), FormBorderStyle = FormBorderStyle.None, ControlBox = false })
                {
                    Label lblEmployee = new Label { Text = "Выберите сотрудника:", AutoSize = true, Location = new Point(20, 20) };
                    assignForm.Controls.Add(lblEmployee);

                    ComboBox cmbEmployees = new ComboBox { Location = new Point(20, 50), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "SELECT EmployeeID, FirstName + ' ' + LastName AS FullName FROM Employees";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        cmbEmployees.DataSource = dt;
                        cmbEmployees.DisplayMember = "FullName";
                        cmbEmployees.ValueMember = "EmployeeID";
                    }
                    assignForm.Controls.Add(cmbEmployees);

                    Button btnAssign = new Button { Text = "Назначить", Location = new Point(100, 100), Width = 80 };
                    btnAssign.Click += (s, e) =>
                    {
                        int employeeId = Convert.ToInt32(cmbEmployees.SelectedValue);
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            try
                            {
                                conn.Open();
                                string query = "UPDATE RepairRequests SET AssignedEmployeeID = @EmployeeID WHERE RequestID = @RequestID";
                                SqlCommand cmd = new SqlCommand(query, conn);
                                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                                cmd.Parameters.AddWithValue("@RequestID", requestId);
                                cmd.ExecuteNonQuery();
                                LoadRequests(); // Автоматическое обновление таблицы
                                Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                                lblStatus.Text = "Сотрудник назначен.";
                                lblStatus.ForeColor = System.Drawing.Color.Green;
                                assignForm.Close();
                            }
                            catch (Exception ex)
                            {
                                Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                                lblStatus.Text = "Ошибка назначения: " + ex.Message;
                                lblStatus.ForeColor = System.Drawing.Color.Red;
                            }
                        }
                    };
                    assignForm.Controls.Add(btnAssign);

                    Button btnCancel = new Button { Text = "Отмена", Location = new Point(190, 100), Width = 80 };
                    btnCancel.Click += (s, e) => assignForm.Close();
                    assignForm.Controls.Add(btnCancel);

                    assignForm.ShowDialog();
                }
            }
            else
            {
                Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                lblStatus.Text = "Выберите заявку для назначения.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void ViewDetails(DataGridView dgvRequests)
        {
            if (dgvRequests.SelectedRows.Count > 0)
            {
                int requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"
                            SELECT r.RequestNumber, r.RequestDate, r.ProblemDescription, 
                                   e.[Name] AS Equipment, e.SerialNumber, e.[Type], e.[Location],
                                   ft.TypeName AS FailureType, 
                                   c.FirstName + ' ' + c.LastName AS Client, c.Email, c.Phone,
                                   rs.StatusName, 
                                   em.FirstName + ' ' + em.LastName AS EmployeeName
                            FROM RepairRequests r
                            LEFT JOIN Equipment e ON r.EquipmentID = e.EquipmentID
                            LEFT JOIN FailureTypes ft ON r.FailureTypeID = ft.FailureTypeID
                            LEFT JOIN Clients c ON r.ClientID = c.ClientID
                            LEFT JOIN RequestStatus rs ON r.StatusID = rs.StatusID
                            LEFT JOIN Employees em ON r.AssignedEmployeeID = em.EmployeeID
                            WHERE r.RequestID = @RequestID";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@RequestID", requestId);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            using (Form detailsForm = new Form { Text = "Детали заявки", Size = new System.Drawing.Size(400, 500), FormBorderStyle = FormBorderStyle.None, ControlBox = false })
                            {
                                int yPosition = 20;
                                AddDetailLabel(detailsForm, "Номер заявки:", reader["RequestNumber"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Дата заявки:", reader["RequestDate"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Описание проблемы:", reader["ProblemDescription"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Оборудование:", reader["Equipment"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Серийный номер:", reader["SerialNumber"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Тип оборудования:", reader["Type"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Местоположение:", reader["Location"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Тип неисправности:", reader["FailureType"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Клиент:", reader["Client"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Email клиента:", reader["Email"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Телефон клиента:", reader["Phone"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Статус:", reader["StatusName"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Назначенный сотрудник:", reader["EmployeeName"].ToString(), ref yPosition);

                                Button btnCloseDetails = new Button { Text = "Закрыть", Location = new Point(150, yPosition + 20), Width = 100 };
                                btnCloseDetails.Click += (s, e) => detailsForm.Close();
                                detailsForm.Controls.Add(btnCloseDetails);

                                detailsForm.ShowDialog();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                        lblStatus.Text = "Ошибка просмотра деталей: " + ex.Message;
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            else
            {
                Label lblStatus = this.Controls.Find("lblStatus", true)[0] as Label;
                lblStatus.Text = "Выберите заявку для просмотра деталей.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }


        private void AddDetailLabel(Form form, string labelText, string value, ref int yPosition)
        {
            Label lbl = new Label { Text = $"{labelText} {value}", AutoSize = true, Location = new Point(20, yPosition), MaximumSize = new System.Drawing.Size(360, 0) };
            form.Controls.Add(lbl);
            yPosition += lbl.Height + 5;
        }
    }
}
