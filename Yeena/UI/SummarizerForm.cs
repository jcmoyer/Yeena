﻿// Copyright 2013 J.C. Moyer
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

using System.Windows.Forms;
using Yeena.Data;

namespace Yeena.UI {
    partial class SummarizerForm : Form {
        private readonly IStashSummarizer _summarizer;

        public SummarizerForm(IStashSummarizer summarizer) {
            _summarizer = summarizer;

            InitializeComponent();

            pgProps.SelectedObject = _summarizer;
        }

        private void btnOk_Click(object sender, System.EventArgs e) {
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, System.EventArgs e) {
            DialogResult = DialogResult.Cancel;
        }
    }
}
