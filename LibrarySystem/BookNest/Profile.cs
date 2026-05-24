using System;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace BookNest
{
    public partial class Profile : Form
    {
        public Dashboard2 dashboard;

        private string originalUsername = "";
        private string originalPassword = "";
        private string originalUserID = "";

        string connectionString =
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
            Application.StartupPath + "\\BookNest.accdb";

        public Profile()
        {
            InitializeComponent();

            // PROFILE IMAGE SETTINGS
            picProfile.Image = Properties.Resources.NoProfile;
            picProfile.SizeMode = PictureBoxSizeMode.Zoom;
            picProfile.Cursor = Cursors.Hand;

            // EVENTS
            this.Load += Profile_Load;

            btnSave.Click += btnSave_Click;
            btnDiscard.Click += btnDiscard_Click;

            btnDash.Click += BtnDash_Click;

            // CURRENT PASSWORD VISIBLE
            txtPass.UseSystemPasswordChar = false;
        }

        // LOAD PROFILE
        private void Profile_Load(object sender, EventArgs e)
        {
            using (OleDbConnection conn =
                new OleDbConnection(connectionString))
            {
                conn.Open();

                string query =
                    @"SELECT UserID,
                             Username,
                             UserPassword
                      FROM Users
                      WHERE UserID = ?";

                OleDbCommand cmd =
                    new OleDbCommand(query, conn);

                cmd.Parameters.AddWithValue(
                    "@p1",
                    UserSession.LoggedInUserID);

                OleDbDataReader reader =
                    cmd.ExecuteReader();

                if (reader.Read())
                {
                    // STORE ORIGINAL VALUES
                    originalUserID =
                        reader["UserID"].ToString();

                    originalUsername =
                        reader["Username"].ToString();

                    originalPassword =
                        reader["UserPassword"].ToString();

                    // LABELS
                    lblUser.Text = originalUsername;

                    lblUserID.Text =
                        "User ID: " + originalUserID;

                    // TEXTBOXES
                    txtUser.Text = originalUsername;

                    txtUserID.Text = originalUserID;

                    txtPass.Text = originalPassword;

                    txtNew.Clear();

                    txtConfirm.Clear();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string newUsername =
                txtUser.Text.Trim();

            string currentPassword =
                txtPass.Text.Trim();

            string newPassword =
                txtNew.Text.Trim();

            string confirmPassword =
                txtConfirm.Text.Trim();

            // EMPTY USERNAME
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                MessageBox.Show(
                    "Username cannot be empty.");

                return;
            }

            // CHECK CURRENT PASSWORD
            if (currentPassword != originalPassword)
            {
                MessageBox.Show(
                    "Current password is incorrect.");

                return;
            }

            // IF USER WANTS TO CHANGE PASSWORD
            if (newPassword != "" ||
                confirmPassword != "")
            {
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show(
                        "New passwords do not match.");

                    return;
                }

                originalPassword = newPassword;
            }

            // UPDATE DATABASE
            using (OleDbConnection conn =
                new OleDbConnection(connectionString))
            {
                conn.Open();

                string query =
                    @"UPDATE Users
                      SET Username = ?,
                          UserPassword = ?
                      WHERE UserID = ?";

                OleDbCommand cmd =
                    new OleDbCommand(query, conn);

                cmd.Parameters.AddWithValue(
                    "@p1",
                    newUsername);

                cmd.Parameters.AddWithValue(
                    "@p2",
                    originalPassword);

                cmd.Parameters.AddWithValue(
                    "@p3",
                    originalUserID);

                cmd.ExecuteNonQuery();
            }

            // UPDATE SESSION
            UserSession.LoggedInUsername =
                newUsername;

            // UPDATE ORIGINAL VALUES
            originalUsername = newUsername;

            // REFRESH LABELS
            lblUser.Text = originalUsername;

            lblUserID.Text =
                "User ID: " + originalUserID;

            // REFRESH TEXTBOXES
            txtUser.Text = originalUsername;

            txtUserID.Text = originalUserID;

            txtPass.Text = originalPassword;

            txtNew.Clear();

            txtConfirm.Clear();

            MessageBox.Show(
                "Profile updated successfully.");
        }

        private void btnDiscard_Click(object sender, EventArgs e)
        {
            txtUser.Text = originalUsername;

            txtUserID.Text = originalUserID;

            txtPass.Text = originalPassword;

            txtNew.Clear();

            txtConfirm.Clear();

            MessageBox.Show(
                "Changes discarded.");
        }

        // PROFILE PICTURE
        private void picProfile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd =
                new OpenFileDialog();

            ofd.Filter =
                "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() ==
                DialogResult.OK)
            {
                picProfile.Image =
                    Image.FromFile(ofd.FileName);
            }
        }



        // DASHBOARD BUTTON
        private void BtnDash_Click(object sender, EventArgs e)
        {
            dashboard.Show();

            this.Hide();
        }

        // LOGOUT BUTTON
        private void btnLogout_Click(object sender, EventArgs e)
        
        {
            UserSession.LoggedInUserID = 0;
            UserSession.LoggedInUsername = "";

            Form1 login = new Form1();
            login.Show();

            this.Hide();
        }
    }
}