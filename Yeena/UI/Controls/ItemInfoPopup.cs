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
using System.Text;
using System.Windows.Forms;
using Yeena.PathOfExile;

namespace Yeena.UI.Controls {
    public partial class ItemInfoPopup : Form {
        private PoEItem _item;
        public PoEItem Item {
            get { return _item; }
            set { SetItem(value); }
        }

        public ItemInfoPopup() {
            InitializeComponent();
        }

        private string GetItemPropertyString(PoEItem item) {
            StringBuilder builder = new StringBuilder();
            foreach (var prop in item.Properties) {
                builder.AppendLine(prop.DisplayText);
            }
            return builder.ToString();
        }

        private void SetItem(PoEItem item) {
            _item = item;

            if (_item.IsRare) {
                lblItemName1.ForeColor = Color.Gold;
                lblItemName2.ForeColor = Color.Gold;
                lblItemName1.Text = item.RareName;
                lblItemName2.Text = item.TypeLine;
                lblItemName2.Visible = true;
            } else {
                lblItemName1.ForeColor = Color.White;
                //lblItemName2.ForeColor = Color.Gold;
                lblItemName1.Text = item.TypeLine;
                lblItemName2.Visible = false;
            }
            if (item.Properties != null && item.Properties.Count > 0) {
                lblProps.Text = GetItemPropertyString(item);
                lblProps.Visible = true;
            }

            int largestWidth = Controls.Cast<Control>().Max(c => c.Width);

            // TODO: This will need to be changed in the future but for now it works
            int farthestBottom = lblItemName1.Height;
            if (!String.IsNullOrEmpty(lblItemName2.Text)) {
                farthestBottom = Controls.Cast<Control>().Max(c => c.Bottom);
            }
            
            // Resize to fit contents
            ClientSize = new Size(largestWidth, farthestBottom);

            // Center the labels
            lblItemName1.Left = (ClientSize.Width / 2) - (lblItemName1.ClientSize.Width / 2);
            lblItemName2.Left = (ClientSize.Width / 2) - (lblItemName2.ClientSize.Width / 2);
            lblProps.Left = (ClientSize.Width / 2) - (lblProps.ClientSize.Width / 2);
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }
    }
}
