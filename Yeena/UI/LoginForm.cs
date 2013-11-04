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
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Yeena.Data;
using Yeena.PathOfExile;

namespace Yeena.UI {
    partial class LoginForm : Form {
        private const string PasswordNotNeededString = "[Not needed]";

        private bool _infoDirty;

        public PoESiteClient Client { get; private set; }

        private readonly BinaryDiskCache<CookieContainer> _cookies = new BinaryDiskCache<CookieContainer>("Cookies");
        private readonly ApplicationSettings _settings;

        public LoginForm(ApplicationSettings settings) {
            _settings = settings;

            InitializeComponent();

            LoadSettings();

            if (_settings.RememberMe) {
                txtPassword.UseSystemPasswordChar = false;
                txtPassword.Text = PasswordNotNeededString;
                _infoDirty = false;
            }
        }

        void LoadSettings() {
            txtEmail.Text = _settings.Email;
            chkRememberMe.Checked = _settings.RememberMe;
        }

        void SaveSettings() {
            _settings.Email = txtEmail.Text;
            _settings.RememberMe = chkRememberMe.Checked;

            _cookies.Save();
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
            if (_settings.RememberMe && !_infoDirty) {
                // Try to load the cookies from disk
                Client.Cookies = _cookies.Load(() => Client.Cookies);
                response = string.Empty;
            } else {
                // Make the store reference the client's cookies
                _cookies.Value = Client.Cookies;
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
