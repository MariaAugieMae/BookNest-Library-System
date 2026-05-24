using System;
using System.Windows.Forms;

namespace BookNest
{
    public partial class   Dashboard2 : Form
    {
        private Books booksForm;
        private Transactions transForm;
        private Profile profileForm;

        public Dashboard2()
        {
            InitializeComponent();

            UserSession.OnUserChanged += RefreshUser;


            // CLICK EVENTS
            panelBooks.Click += OpenBooks;
            panelProfile.Click += OpenProfile;
            panelTransactions.Click += OpenTransactions;
            panelLogout.Click += Logout;
        }

        private void RefreshUser()
        {
            lblUsername.Text = UserSession.LoggedInUsername;
        }

        private void Dashboard2_Load(object sender, EventArgs e)
        {
            RefreshUser();

            lblUsername.Text = UserSession.LoggedInUsername;
            lblWelcome.Text = "Welcome, " + UserSession.LoggedInUsername + "!";

            // INIT FORMS ONLY ONCE
            if (booksForm == null)
            {
                booksForm = new Books();
                transForm = new Transactions();
                profileForm = new Profile();

                // link dashboard
                booksForm.dashboard = this;
                transForm.dashboard = this;
                profileForm.dashboard = this;

                // center all forms
                booksForm.StartPosition = FormStartPosition.CenterScreen;
                transForm.StartPosition = FormStartPosition.CenterScreen;
                profileForm.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void OpenBooks(object sender, EventArgs e)
        {
            booksForm.Show();
            this.Hide();
        }

        private void OpenProfile(object sender, EventArgs e)
        {
            profileForm.Show();
            this.Hide();
        }

        private void OpenTransactions(object sender, EventArgs e)
        {
            transForm.Show();
            this.Hide();
        }

        private void Logout(object sender, EventArgs e)
        {
            UserSession.LoggedInUserID = 0;
            UserSession.LoggedInUsername = "";

            new Form1().Show();
            this.Hide();
        }

        private void panelHeader_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}