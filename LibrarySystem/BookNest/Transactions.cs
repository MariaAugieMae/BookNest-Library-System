using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace BookNest
{
    public partial class Transactions : Form
    {
        string connectionString =
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
            Application.StartupPath + "\\BookNest.accdb";

        public Dashboard2 dashboard;

        public Transactions()
        {
            InitializeComponent();

            // FORM LOAD
            this.Load += Transactions_Load;
            UserSession.OnUserChanged += RefreshUser;


            // BUTTON EVENTS
            btnRefresh.Click += BtnRefresh_Click;
            btnReturn.Click += BtnReturn_Click;
            btnLogout.Click += BtnLogout_Click;

            // btnDash.Click += BtnDash_Click;
        }

        private void RefreshUser()
        {
            lblUsername.Text = UserSession.LoggedInUsername;
        }

        // FORM LOAD
        private void Transactions_Load(
            object sender,
            EventArgs e)
        {
            lblUsername.Text =
                UserSession.LoggedInUsername;

            RefreshUser();
            LoadTransactions();
        }

        // DASHBOARD
        private void BtnDash_Click(
            object sender,
            EventArgs e)
        {
            dashboard?.Show();
            this.Hide();
        }

        // LOGOUT
        private void BtnLogout_Click(
            object sender,
            EventArgs e)
        {
            UserSession.LoggedInUserID = 0;
            UserSession.LoggedInUsername = "";

            Form1 login = new Form1();
            login.Show();

            this.Hide();
        }

        // LOAD TRANSACTIONS
        private void LoadTransactions()
        {
            using (OleDbConnection conn =
                new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT
                            BT.LoanID,
                            BT.BookID,
                            B.Title AS BookTitle,
                            BT.BorrowDate,
                            BT.DueDate,
                            BT.ReturnDate,
                            BT.Status,
                            BT.Penalty
                        FROM BookTransactions AS BT
                        INNER JOIN Books AS B
                        ON BT.BookID = B.BookID
                        WHERE
                            BT.UserID = ?
                            AND BT.Status <> 'Returned'";

                    OleDbDataAdapter adapter =
                        new OleDbDataAdapter(
                            query,
                            conn);

                    adapter.SelectCommand.Parameters.Add(
                        "@p1",
                        OleDbType.Integer
                    ).Value =
                        UserSession.LoggedInUserID;

                    DataTable dt = new DataTable();

                    adapter.Fill(dt);

                    dataGridLoans.DataSource = dt;

                    // DATAGRID SETTINGS
                    dataGridLoans.ReadOnly = true;

                    dataGridLoans.MultiSelect = false;

                    dataGridLoans.SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect;

                    dataGridLoans.AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode.Fill;

                    // HIDE IDs
                    if (dataGridLoans.Columns.Contains("LoanID"))
                    {
                        dataGridLoans.Columns["LoanID"].Visible = false;
                    }

                    if (dataGridLoans.Columns.Contains("BookID"))
                    {
                        dataGridLoans.Columns["BookID"].Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Load Error:\n" + ex.Message
                    );
                }
            }
        }

        // RETURN BOOK
        private void BtnReturn_Click(
            object sender,
            EventArgs e)
        {
            if (dataGridLoans.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select a book first."
                );

                return;
            }

            try
            {
                int loanID =
                    Convert.ToInt32(
                        dataGridLoans
                        .SelectedRows[0]
                        .Cells["LoanID"]
                        .Value);

                int bookID =
                    Convert.ToInt32(
                        dataGridLoans
                        .SelectedRows[0]
                        .Cells["BookID"]
                        .Value);

                DateTime dueDate =
                    Convert.ToDateTime(
                        dataGridLoans
                        .SelectedRows[0]
                        .Cells["DueDate"]
                        .Value);

                // CALCULATE PENALTY
                int daysLate =
                    (DateTime.Now.Date - dueDate.Date).Days;

                int penalty = 0;

                if (daysLate > 0)
                {
                    penalty = daysLate * 5;
                }

                if (penalty > 0)
                {
                    MessageBox.Show(
                        "Late Return Penalty: ₱" +
                        penalty.ToString()
                    );
                }

                using (OleDbConnection conn =
                    new OleDbConnection(connectionString))
                {
                    conn.Open();

                    // UPDATE TRANSACTION
                    OleDbCommand updateTransaction =
                        new OleDbCommand(
                            @"UPDATE BookTransactions
                            SET
                                Status = ?,
                                ReturnDate = ?,
                                Penalty = ?
                            WHERE LoanID = ?",
                            conn);

                    updateTransaction.Parameters.Add(
                        "@p1",
                        OleDbType.VarChar
                    ).Value = "Returned";

                    updateTransaction.Parameters.Add(
                        "@p2",
                        OleDbType.Date
                    ).Value = DateTime.Now;

                    updateTransaction.Parameters.Add(
                        "@p3",
                        OleDbType.Integer
                    ).Value = penalty;

                    updateTransaction.Parameters.Add(
                        "@p4",
                        OleDbType.Integer
                    ).Value = loanID;

                    updateTransaction.ExecuteNonQuery();

                    // UPDATE BOOK STATUS
                    OleDbCommand updateBook =
                        new OleDbCommand(
                            @"UPDATE Books
                            SET Status = 'Available'
                            WHERE BookID = ?",
                            conn);

                    updateBook.Parameters.Add(
                        "@p1",
                        OleDbType.Integer
                    ).Value = bookID;

                    updateBook.ExecuteNonQuery();
                }

                MessageBox.Show(
                    "Book returned successfully!"
                );

                LoadTransactions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Return Error:\n" + ex.Message
                );
            }
        }

        // REFRESH BUTTON
        private void BtnRefresh_Click(
            object sender,
            EventArgs e)
        {
            LoadTransactions();
        }
    }
}