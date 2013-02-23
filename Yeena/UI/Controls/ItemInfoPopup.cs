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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Yeena.PathOfExile;

namespace Yeena.UI.Controls {
    partial class ItemInfoPopup : Form {
        private PoEItem _item;
        public PoEItem Item {
            get { return _item; }
            set { SetItem(value); }
        }

        private readonly Control[] _displayOrderedControls;
        private readonly PoEItemTable _itemTable;

        public ItemInfoPopup(PoEItemTable itemTable) {
            InitializeComponent();

            _itemTable = itemTable;
            _displayOrderedControls = new Control[] {
                lblItemName1,
                lblItemName2,
                lblProps
            };
        }

        private int LayoutVertically(IEnumerable<Control> controls) {
            int nextY = 0;
            foreach (var control in controls) {
                if (!control.Visible) continue;
                control.Top = nextY;
                nextY = control.Bottom;
            }
            return nextY;
        }

        private void CenterHorizontally(IEnumerable<Control> controls) {
            foreach (var control in controls) {
                control.Left = (ClientSize.Width / 2) - (control.ClientSize.Width / 2);
            }
        }

        private string GetItemPropertyString(PoEItem item) {
            StringBuilder builder = new StringBuilder();
            foreach (var prop in item.Properties) {
                builder.AppendLine(prop.DisplayText);
            }
            if (item.AdditionalProperties != null) {
                builder.AppendLine();
                foreach (var prop in item.AdditionalProperties) {
                    builder.AppendLine(prop.DisplayText);
                }
            }
            if (item.ImplicitMods != null) {
                builder.AppendLine();
                foreach (var mod in item.ImplicitMods) {
                    builder.AppendLine(mod);
                }
            }
            if (item.ExplicitMods != null) {
                builder.AppendLine();
                foreach (var mod in item.ExplicitMods) {
                    builder.AppendLine(mod);
                }
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
            } else if (_itemTable.IsMagic(_item)) {
                lblItemName1.ForeColor = Color.RoyalBlue;
                lblItemName1.Text = item.TypeLine;
                lblItemName2.Visible = false;
            } else {
                lblItemName1.ForeColor = Color.White;
                lblItemName1.Text = item.TypeLine;
                lblItemName2.Visible = false;
            }
            if (item.Properties != null && item.Properties.Count > 0) {
                lblProps.Text = GetItemPropertyString(item);
                lblProps.Visible = true;
            } else {
                lblProps.Visible = false;
            }

            int largestWidth = Controls.Cast<Control>().Max(c => c.Width);

            // Resize to fit contents
            ClientSize = new Size(largestWidth, LayoutVertically(_displayOrderedControls));

            // Center all of the items
            CenterHorizontally(_displayOrderedControls);
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }
    }
}
