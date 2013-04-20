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
using System.Linq;
using System.Windows.Forms;
using Yeena.PathOfExile;

namespace Yeena.UI {
    partial class TabFilterForm : Form {
        class TaggedStashTab {
            public PoEStashTab Tab { get; private set; }
            
            public TaggedStashTab(PoEStashTab tab) {
                Tab = tab;
            }

            public override string ToString() {
                return Tab.TabInfo.Name;
            }
        }

        public TabFilterForm(IEnumerable<PoEStashTab> tabs) {
            InitializeComponent();

            foreach (var tab in tabs) {
                lstTabs.Items.Add(new TaggedStashTab(tab));
            }
        }

        public IEnumerable<PoEStashTab> FilteredTabs {
            get {
                return lstTabs.CheckedItems.Cast<TaggedStashTab>().Select(tag => tag.Tab);
            }
        }

        public void SetCheckedTabs(IEnumerable<PoEStashTab> tabs) {
            lstTabs.ClearSelected();
            foreach (var tab in tabs) {
                lstTabs.SetItemChecked(tab.TabInfo.Index, true);
            }
        }

        private void btnOk_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
        } 
    }
}
