// Copyright 2013 J.C. Moyer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Yeena.PathOfExile;
using Yeena.Properties;

namespace Yeena.UI {
    partial class LoginForm : Form {
        private const string PasswordNotNeededString = "[Not needed]";

        private bool _infoDirty;

        public PoESiteClient Client { get; private set; }

        public LoginForm() {
            InitializeComponent();

            LoadSettings();

            if (Settings.Default.RememberMe) {
                txtPassword.UseSystemPasswordChar = false;
                txtPassword.Text = PasswordNotNeededString;
                _infoDirty = false;
            }
        }

        void LoadSettings() {
            txtEmail.Text = Settings.Default.Email;
            chkRememberMe.Checked = Settings.Default.RememberMe;
        }

        void SaveSettings() {
            Settings.Default.Email = txtEmail.Text;
            Settings.Default.RememberMe = chkRememberMe.Checked;
            Settings.Default.Save();

            string cookiesFile = Storage.ResolvePath("cookies.dat");

            if (!Settings.Default.RememberMe) {
                // Delete the cookies file if there is one since it's no longer relevant.
                if (File.Exists(cookiesFile)) File.Delete(cookiesFile);
            }
        }

        void ApplyCookies() {
            string cookiesFile = Storage.ResolvePath("cookies.dat");
            BinaryFormatter bf = new BinaryFormatter();
            using (Stream s = File.OpenRead(cookiesFile)) {
                Client.Cookies = (CookieContainer)bf.Deserialize(s);
            }
        }

        void EnableChildren(bool state) {
            foreach (var child in Controls.Cast<Control>()) {
                child.Enabled = state;
            }
            // Should always be enabled.
            tblLayout.Enabled = true;
            lblStatus.Enabled = true;
        }

        private async void btnLogin_Click(object sender, EventArgs e) {
            EnableChildren(false);

            string response;
            lblStatus.ForeColor = Color.Black;
            lblStatus.Text = "Authenticating; please wait...";

            Client = new PoESiteClient();

            // We require the user to not have made any changes to the login info if we're to fetch old cookies from cache.
            if (Settings.Default.RememberMe && !_infoDirty) {
                ApplyCookies();
                response = string.Empty; //await Client.LoginAsync();
            } else {
                response = await Client.LoginAsync(new PoESiteCredentials(txtEmail.Text, txtPassword.Text));
            }

            if (response.Contains("Invalid Login")) {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "Invalid Login.";
            } else {
                SaveSettings();
                DialogResult = DialogResult.OK;
            }

            EnableChildren(true);
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
        }

        private void txtEmail_TextChanged(object sender, EventArgs e) {
            _infoDirty = true;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e) {
            _infoDirty = true;
        }

        private void txtPassword_Enter(object sender, EventArgs e) {
            if (txtPassword.Text == PasswordNotNeededString) {
                txtPassword.UseSystemPasswordChar = true;
                txtPassword.Text = String.Empty;
            }
        }
    }
}
