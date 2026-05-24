using System;
using System.Data.OleDb;
using System.Windows.Forms;

namespace BookNest
{
    public partial class Form1 : Form
    {
        string connectionString =
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
            Application.StartupPath + "\\BookNest.accdb";

        public Form1()
        {
            InitializeComponent();

            // default view
            panelLogin.BringToFront();
            this.AcceptButton = btnLogin;  


        }

        // LOGIN
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();

            if (username == "" || password == "")
            {
                MessageBox.Show("Enter username and password.");
                return;
            }

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM Users WHERE [Username]=? AND [UserPassword]=?";

                OleDbCommand cmd = new OleDbCommand(query, conn);
                cmd.Parameters.AddWithValue("@p1", username);
                cmd.Parameters.AddWithValue("@p2", password);

                OleDbDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // SAVE SESSION
                    UserSession.LoggedInUserID = Convert.ToInt32(reader["UserID"]);
                    UserSession.LoggedInUsername = reader["Username"].ToString();

                    // OPEN DASHBOARD
                    Dashboard2 dash = new Dashboard2();
                    dash.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.");
                }
            }
        }

        // CLEAR LOGIN
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUser.Clear();
            txtPass.Clear();
        }

        // OPEN REGISTER PANEL
        private void btnGoRegister_Click(object sender, EventArgs e)
        {
            panelRegister.BringToFront();
            this.AcceptButton = btnRegister; 

        }


        // REGISTER ACCOUNT
        private void btnRegister_Click(object sender, EventArgs e)
        {
            string user = txtRegUser.Text.Trim();
            string pass = txtRegPass.Text.Trim();
            string confirm = txtRegConfirm.Text.Trim();

            if (user == "" || pass == "")
            {
                MessageBox.Show("Fill all fields.");
                return;
            }

            if (pass != confirm)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();

                // check if username exists
                OleDbCommand check = new OleDbCommand(
     "SELECT COUNT(*) FROM Users WHERE [Username]=?", conn);
                check.Parameters.AddWithValue("@p1", user);

                int exists = Convert.ToInt32(check.ExecuteScalar());

                if (exists > 0)
                {
                    MessageBox.Show("Username already exists.");
                    return;
                }

                // insert new user
                OleDbCommand insert = new OleDbCommand(
    "INSERT INTO Users (Username, UserPassword) VALUES (?, ?)", conn);

                insert.Parameters.AddWithValue("@p1", user);
                insert.Parameters.AddWithValue("@p2", pass);

                insert.ExecuteNonQuery();
            }

            MessageBox.Show("Account created successfully!");

            // go back to login
            panelLogin.BringToFront();
        }

        // BACK TO LOGIN
        private void btnForm1_Click(object sender, EventArgs e)
        {
            panelLogin.BringToFront();
        }
    }
}