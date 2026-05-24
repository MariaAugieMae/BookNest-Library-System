using System;

namespace BookNest
{
    public static class UserSession
    {
        public static int LoggedInUserID;

        private static string loggedInUsername = "";

        public static string LoggedInUsername
        {
            get { return loggedInUsername; }
            set
            {
                loggedInUsername = value;

                // notify all subscribed forms
                OnUserChanged?.Invoke();
            }
        }

        // event so forms can refresh UI automatically
        public static event Action OnUserChanged;
    }
}