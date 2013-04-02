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
                lblUnidentified,
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

        private string CreateDisplayString(IEnumerable<IEnumerable<string>> sections) {
            StringBuilder builder = new StringBuilder();
            foreach (var section in sections) {
                var sectionList = section as List<string> ?? section.ToList();
                foreach (var line in sectionList) {
                    builder.AppendLine(line);
                }
                // Insert an empty line if there was content in this section to separate this
                // section from other ones.
                if (sectionList.Count > 0) {
                    builder.AppendLine();
                }
            }
            return builder.ToString().TrimEnd();
        }

        private string GetItemPropertyString(PoEItem item) {
            var sections = new List<IEnumerable<string>> {
                item.Properties.Select(prop => prop.DisplayText),
                item.AdditionalProperties.Select(prop => prop.DisplayText),
                item.ImplicitMods,
                item.ExplicitMods,
            };
            return CreateDisplayString(sections);
        }

        private void SetItem(PoEItem item) {
            _item = item;

            if (_item.FrameType == PoEItemFrameType.Unique) {
                lblItemName1.ForeColor = Color.SaddleBrown;
                lblItemName2.ForeColor = Color.SaddleBrown;
                if (_item.IsIdentified) {
                    lblItemName1.Text = item.RareName;
                    lblItemName2.Text = item.TypeLine;
                    lblItemName2.Visible = true;
                } else {
                    lblItemName1.Text = item.TypeLine;
                    lblItemName2.Visible = false;
                }
            } else if (_item.FrameType == PoEItemFrameType.Rare) {
                lblItemName1.ForeColor = Color.Gold;
                lblItemName2.ForeColor = Color.Gold;
                if (_item.IsIdentified) {
                    lblItemName1.Text = item.RareName;
                    lblItemName2.Text = item.TypeLine;
                    lblItemName2.Visible = true;
                } else {
                    lblItemName1.Text = item.TypeLine;
                    lblItemName2.Visible = false;
                }
            } else if (_item.FrameType == PoEItemFrameType.Magic) {
                lblItemName1.ForeColor = Color.RoyalBlue;
                lblItemName1.Text = item.TypeLine;
                lblItemName2.Visible = false;
            } else {
                lblItemName1.ForeColor = Color.White;
                lblItemName1.Text = item.TypeLine;
                lblItemName2.Visible = false;
            }
            string propText = GetItemPropertyString(item);
            if (!String.IsNullOrWhiteSpace(propText)) {
                lblProps.Text = propText;
                lblProps.Visible = true;
            } else {
                lblProps.Visible = false;
            }
            lblUnidentified.Visible = !item.IsIdentified && item.FrameType != PoEItemFrameType.Normal;

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
