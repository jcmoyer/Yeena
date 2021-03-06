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

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Yeena.PathOfExile;

namespace Yeena.UI.Controls {
    class StashTabControl : TabControl {
        protected override void OnDrawItem(DrawItemEventArgs e) {
            TabPage page = TabPages[e.Index];
            e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, Font, paddedBounds, page.ForeColor);
        }

        public void AddStashTab(PoEStashTab tab, StashGrid stashGrid) {
            var page = new StashTabPage(tab);
            stashGrid.Dock = DockStyle.Fill;
            stashGrid.StashTab = tab;
            stashGrid.Tag = tab;
            page.Controls.Add(stashGrid);
            page.Tag = stashGrid;
            TabPages.Add(page);
        }

        public TabPage GetTabPageForStashTab(PoEStashTab tab) {
            return TabPages
                .OfType<StashTabPage>()
                .First(p => p.StashTab == tab);
        }

        public bool TryGetTabPageForStashTab(PoEStashTab tab, out TabPage page) {
            try {
                page = GetTabPageForStashTab(tab);
                return true;
            } catch (InvalidOperationException) {
                page = null;
                return false;
            }
        }

        public StashGrid GetStashGridForStashTab(PoEStashTab tab) {
            return TabPages
                .OfType<StashTabPage>()
                .Select(p => (StashGrid)p.Tag)
                .First(g => g.StashTab == tab);
        }

        public bool TryGetStashGridForStashTab(PoEStashTab tab, out StashGrid grid) {
            try {
                grid = GetStashGridForStashTab(tab);
                return true;
            } catch (InvalidOperationException) {
                grid = null;
                return false;
            }
        }

        public StashGrid SelectedStashGrid {
            get {
                if (SelectedTab != null) {
                    return (StashGrid)SelectedTab.Tag;
                } else {
                    return null;
                }
            }
        }
    }
}
