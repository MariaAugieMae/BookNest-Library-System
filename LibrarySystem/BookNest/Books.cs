using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace BookNest
{
    public partial class Books : Form
    {
        string connectionString =
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
            Application.StartupPath + "\\BookNest.accdb";

        public Dashboard2 dashboard;

        public Books()
        {
            InitializeComponent();

            this.Load += Books_Load;
            this.VisibleChanged += Books_VisibleChanged;

            UserSession.OnUserChanged += RefreshUser;
        
            btnDashb.Click += BtnDashb_Click;
            btnLogout.Click += BtnLogout_Click;

            btnSearch.Click += BtnSearch_Click;
            btnApply.Click += BtnApply_Click;
            btnClear.Click += BtnClear_Click;
        }

        private void RefreshUser()
        {
            lblUsername.Text = UserSession.LoggedInUsername;
        }

        // FORM LOAD
        private void Books_Load(object sender, EventArgs e)
        {
            cmbCategory.SelectedIndex = 0;
            cmbAvilability.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;

            lblUsername.Text = UserSession.LoggedInUsername;

            RefreshUser();
            LoadBooks();
        }

        // AUTO REFRESH WHEN BOOKS PANEL/FORM SHOWS
        private void Books_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                LoadBooks();
            }
        }

        // NAVIGATION
        private void BtnDashb_Click(object sender, EventArgs e)
        {
            dashboard?.Show();
            this.Hide();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            UserSession.LoggedInUserID = 0;
            UserSession.LoggedInUsername = "";

            Form1 login = new Form1();
            login.Show();

            this.Hide();
        }

        // BUTTONS
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadBooks();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            LoadBooks();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();

            cmbCategory.SelectedIndex = 0;
            cmbAvilability.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;

            LoadBooks();
        }

        // LOAD BOOKS
        private void LoadBooks()
        {
            flpBooks.Controls.Clear();

            using (OleDbConnection conn =
                new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                        SELECT BookID, Title, Author, Category, Status
                        FROM Books
                        WHERE 1=1";

                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        sql += " AND (Title LIKE ? OR Author LIKE ?)";
                    }

                    if (cmbCategory.Text != "All")
                    {
                        sql += " AND Category = ?";
                    }

                    if (cmbAvilability.Text != "All")
                    {
                        sql += " AND Status = ?";
                    }

                    if (cmbSort.Text == "Title A-Z")
                    {
                        sql += " ORDER BY Title ASC";
                    }
                    else if (cmbSort.Text == "Title Z-A")
                    {
                        sql += " ORDER BY Title DESC";
                    }

                    OleDbCommand cmd = new OleDbCommand(sql, conn);

                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        cmd.Parameters.Add("@p1", OleDbType.VarChar).Value = "%" + txtSearch.Text + "%";
                        cmd.Parameters.Add("@p2", OleDbType.VarChar).Value = "%" + txtSearch.Text + "%";
                    }

                    if (cmbCategory.Text != "All")
                    {
                        cmd.Parameters.Add("@p3", OleDbType.VarChar).Value = cmbCategory.Text;
                    }

                    if (cmbAvilability.Text != "All")
                    {
                        cmd.Parameters.Add("@p4", OleDbType.VarChar).Value = cmbAvilability.Text;
                    }

                    OleDbDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        flpBooks.Controls.Add(
                            CreateBookCard(
                                Convert.ToInt32(reader["BookID"]),
                                reader["Title"].ToString(),
                                reader["Author"].ToString(),
                                reader["Status"].ToString()
                            )
                        );
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading books:\n" + ex.Message);
                }
            }
        }

        // BOOK CARD UI
        private Panel CreateBookCard(int id, string title, string author, string status)
        {
            Panel panel = new Panel
            {
                Size = new Size(250, 120),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(10)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 15)
            };

            Label lblAuthor = new Label
            {
                Text = "By " + author,
                AutoSize = true,
                Location = new Point(15, 45)
            };

            Label lblStatus = new Label
            {
                Text = status,
                AutoSize = true,
                Location = new Point(15, 70),
                ForeColor = status == "Available" ? Color.Green : Color.Red
            };

            Button btnBorrow = new Button
            {
                Text = "Borrow",
                Size = new Size(80, 30),
                Location = new Point(150, 75),
                Enabled = status != "Borrowed"
            };

            btnBorrow.Click += (s, e) => BorrowBook(id);

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblAuthor);
            panel.Controls.Add(lblStatus);
            panel.Controls.Add(btnBorrow);

            return panel;
        }

        // BORROW BOOK
        private void BorrowBook(int bookID)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    OleDbCommand checkCmd =
                        new OleDbCommand("SELECT Status FROM Books WHERE BookID = ?", conn);

                    checkCmd.Parameters.Add("@p1", OleDbType.Integer).Value = bookID;

                    string status = Convert.ToString(checkCmd.ExecuteScalar());

                    if (status == "Borrowed")
                    {
                        MessageBox.Show("This book is already borrowed.");
                        return;
                    }

                    OleDbCommand updateBook =
                        new OleDbCommand("UPDATE Books SET Status=? WHERE BookID=?", conn);

                    updateBook.Parameters.Add("@p1", OleDbType.VarChar).Value = "Borrowed";
                    updateBook.Parameters.Add("@p2", OleDbType.Integer).Value = bookID;

                    updateBook.ExecuteNonQuery();

                    DateTime borrowDate = DateTime.Now;
                    DateTime dueDate = borrowDate.AddDays(7);

                    OleDbCommand insertTrans =
                        new OleDbCommand(@"
                            INSERT INTO BookTransactions
                            (UserID, BookID, BorrowDate, DueDate, Status)
                            VALUES (?, ?, ?, ?, ?)", conn);

                    insertTrans.Parameters.Add("@p1", OleDbType.Integer).Value = UserSession.LoggedInUserID;
                    insertTrans.Parameters.Add("@p2", OleDbType.Integer).Value = bookID;
                    insertTrans.Parameters.Add("@p3", OleDbType.Date).Value = borrowDate;
                    insertTrans.Parameters.Add("@p4", OleDbType.Date).Value = dueDate;
                    insertTrans.Parameters.Add("@p5", OleDbType.VarChar).Value = "Borrowed";

                    insertTrans.ExecuteNonQuery();

                    MessageBox.Show("Book borrowed successfully!");

                    LoadBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Borrow Error:\n" + ex.Message);
                }
            }
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
    }
}