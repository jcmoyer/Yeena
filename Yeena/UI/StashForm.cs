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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yeena.Data;
using Yeena.PathOfExile;
using Yeena.Recipe;
using Yeena.UI.Controls;

namespace Yeena.UI {
    partial class StashForm : Form {
        private readonly PoESiteClient _client;

        private CancellationTokenSource _fetchCts;
        private Task _fetchTask;

        // Map of league to stash
        private Dictionary<string, PoEStash> _leagueStashes = new Dictionary<string, PoEStash>();
        private PoEStash _activeStash = null;
        private StashTabCollectionView _recipeTabs;

        //private PoEItemTable _itemTable;
        private JsonDiskCache<PoEItemTable> _itemTable = new JsonDiskCache<PoEItemTable>("ItemTable");
        private ImageCache _imageCache = new ImageCache("Images");
        private readonly ApplicationSettings _settings;

        public StashForm(ApplicationSettings settings, PoESiteClient client) {
            _settings = settings;
            _client = client;

            InitializeComponent();
        }

        private void ShowStash(PoEStash stash) {
            tabControl1.TabPages.Clear();
            int stashNum = 1;
            foreach (var tab in stash.Tabs) {
                var uiTab1 = CreateStashTabPage(stashNum++.ToString(), tab);
                tabControl1.TabPages.Add(uiTab1);
            }
            _activeStash = stash;
            _recipeTabs = new StashTabCollectionView(_activeStash.Tabs);
            recipeSelector1.ItemSource = _recipeTabs.Items.ToList();
        }

        private async void StartFetchStashPagesAsync(string league, bool refresh = false) {
            if (_leagueStashes.ContainsKey(league) && !refresh) {
                ShowStash(_leagueStashes[league]);
                return;
            }

            EnableUnsafeControls(false);

            if (_fetchCts != null) {
                _fetchCts.Cancel(true);
            }
            try {
                if (_fetchTask != null)
                    await _fetchTask;
            } catch (OperationCanceledException) {
            }

            tabControl1.TabPages.Clear();
            _fetchCts = new CancellationTokenSource();
            _fetchTask = FetchStashPagesAsync(cboLeague.Text, _fetchCts.Token);

            await _fetchTask;

            EnableUnsafeControls(true);
        }

        // Asynchronously fetches all the stash pages for a given league
        private async Task FetchStashPagesAsync(string league, CancellationToken cancellationToken) {
            var stashTabs = new List<PoEStashTab>();

            // We need at least 1 tab to figure out how many tabs there are total
            lblStatus.Text = "Fetching first stash page...";
            PoEStashTab stash1;
            try {
                stash1 = await _client.GetStashTabAsync(league, 0, 2500, cancellationToken);
            } catch (InvalidCastException) {
                lblStatus.Text = "There is no stash in this league yet.";
                return;
            }
            var uiTab1 = CreateStashTabPage(stash1.TabInfo.Name, stash1);
            tabControl1.TabPages.Add(uiTab1);
            stashTabs.Add(stash1);

            for (int i = 1; i < stash1.TabCount; i++) {
                cancellationToken.ThrowIfCancellationRequested();

                lblStatus.Text = String.Format("Fetching stash page {0}/{1}...", i + 1, stash1.TabCount);
                var stashI = await _client.GetStashTabAsync(league, i, 2500, cancellationToken);

                var uiTabI = CreateStashTabPage(stashI.TabInfo.Name, stashI);
                tabControl1.TabPages.Add(uiTabI);
                stashTabs.Add(stashI);
            }

            lblStatus.Text = "Finalizing...";
            var stash = new PoEStash(stashTabs);
            _activeStash = stash;
            _recipeTabs = new StashTabCollectionView(_activeStash.Tabs);
            recipeSelector1.ItemSource = _recipeTabs.Items.ToList();

            lblStatus.Text = "Ready";
        }

        // Creates and returns a TabPage with a StashGrid control
        private TabPage CreateStashTabPage(string name, PoEStashTab tab) {
            TabPage tp = new TabPage(name);
            StashGrid grid = new StashGrid(_itemTable, _imageCache);
            grid.Dock = DockStyle.Fill;
            grid.SetImages(tab);
            grid.Tag = tab;
            tp.Controls.Add(grid);
            tp.Tag = grid;
            return tp;
        }

        private void cboLeague_SelectedIndexChanged(object sender, EventArgs e) {
            StartFetchStashPagesAsync(cboLeague.Text);
        }

        private async void refreshToolStripMenuItem_Click(object sender, EventArgs e) {
            var selectedGrid = tabControl1.SelectedTab.Controls.OfType<StashGrid>().FirstOrDefault();
            if (selectedGrid == null) return;

            EnableUnsafeControls(false);

            var tab = await _client.GetStashTabAsync(cboLeague.Text, tabControl1.SelectedIndex);
            selectedGrid.SetImages(tab);

            EnableUnsafeControls(true);
        }

        private async void StashForm_Load(object sender, EventArgs e) {
            lblStatus.Text = "Loading item table...";
            await _itemTable.LoadAsync(_client.GetItemTable);

            lblStatus.Text = "Loading image cache...";
            await _imageCache.LoadAsync(() => Task.Factory.StartNew(() => new ConcurrentDictionary<Uri, Image>()));

            recipeSelector1.RegisterRecipeSolver(new WhetstoneSolver(_itemTable));
            recipeSelector1.RegisterRecipeSolver(new ScrapSolver(_itemTable));
            recipeSelector1.RegisterRecipeSolver(new BaubleSolver(_itemTable));
            recipeSelector1.RegisterRecipeSolver(new PrismSolver(_itemTable));
            recipeSelector1.RegisterRecipeSolver(new DuplicateRareSolver(_itemTable));
            recipeSelector1.RegisterRecipeSolver(new SixSocketSolver(_itemTable));

            var lastLeague = _settings.LastLeagueName;
            if (!String.IsNullOrEmpty(lastLeague)) {
                // This is going to fire StartFetchStashPagesAsync.
                cboLeague.Text = lastLeague;
            } else {
                StartFetchStashPagesAsync(cboLeague.Text);   
            }
        }

        private void recipeSelector1_RecipesSolved(object sender, EventArgs e) {
            dataGridView1.DataSource = recipeSelector1.Recipes.OrderByDescending(r => r.ItemCount).ToList();
        }

        private VendorRecipe SelectedRecipe {
            get {
                if (dataGridView1.SelectedRows.Count > 0) {
                    return (VendorRecipe)dataGridView1.SelectedRows[0].DataBoundItem;
                }
                return null;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e) {
            var recipe = SelectedRecipe;

            foreach (var tcTab in tabControl1.TabPages.OfType<TabPage>()) {
                tcTab.BackColor = default(Color);
                StashGrid sg = (StashGrid)tcTab.Tag;
                sg.ClearMarkings();
            }

            if (recipe == null) {
                return;
            }

            foreach (var item in recipe.Items) {
                var responsibleTab = _activeStash.GetContainingTab(item);
                foreach (var tcTab in tabControl1.TabPages.OfType<TabPage>()) {
                    StashGrid sg = (StashGrid)tcTab.Tag;
                    PoEStashTab stashTab = (PoEStashTab)sg.Tag;
                    
                    if (stashTab != responsibleTab) {
                        continue;
                    }

                    var marking = new StashGridMarking(item);
                    sg.AddMarking(new StashGridMarking(item));

                    var solidBrush = marking.Brush as SolidBrush;
                    if (solidBrush != null) {
                        tcTab.BackColor = solidBrush.Color;
                    }
                }
            }

            tabControl1.Invalidate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e) {
            TabPage page = tabControl1.TabPages[e.Index];
            e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, Font, paddedBounds, page.ForeColor);
        }

        private void summaryToolStripMenuItem_Click(object sender, EventArgs e) {
            FileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dialog.DefaultExt = "txt";
            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            using (var fs = File.OpenWrite(dialog.FileName))
            using (var sw = new StreamWriter(fs)) {
                int tabNumber = 1;
                foreach (var tab in _activeStash.Tabs) {
                    sw.WriteLine(("Tab " + tabNumber++ + " ").PadRight(80, '='));
                    foreach (var item in tab) {
                        if (item.IsRare) sw.WriteLine("{0}, {1}", item.RareName, item.TypeLine);
                        else if (_itemTable.Value.IsEquippable(item)) sw.WriteLine(item.TypeLine);
                        else {
                            string stackSize = item.StackSize;
                            if (!String.IsNullOrEmpty(stackSize)) {
                                sw.WriteLine("{0} ({1})", item.TypeLine, item.StackSize);
                            } else {
                                sw.WriteLine(item.TypeLine);
                            }
                        }
                    }
                    sw.WriteLine();
                }
            }
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e) {
            FileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PNG Images (*.png)|*.png|All Files (*.*)|*.*";
            dialog.DefaultExt = "txt";
            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            var sg = tabControl1.TabPages[0].Controls.OfType<StashGrid>().First();
            var b = new Bitmap(sg.ClientSize.Width * _activeStash.Tabs.Count, sg.ClientSize.Height);
            for (int i = 0; i < _activeStash.Tabs.Count; i++) {
                var sg2 = tabControl1.TabPages[i].Controls.OfType<StashGrid>().First();
                sg2.DrawToBitmap(b, new Rectangle(i * sg.ClientSize.Width, 0, sg.ClientSize.Width, sg.ClientSize.Height));
            }
            b.Save(dialog.FileName, ImageFormat.Png);
        }

        private void StashForm_FormClosing(object sender, FormClosingEventArgs e) {
            _itemTable.Save();
            _imageCache.Save();

            _settings.LastLeagueName = cboLeague.Text;
        }

        private void btnTabs_Click(object sender, EventArgs e) {
            if (_activeStash == null) return;

            var filterForm = new TabFilterForm(_activeStash.Tabs);
            filterForm.SetCheckedTabs(_recipeTabs.Tabs);

            if (filterForm.ShowDialog() == DialogResult.OK) {
                _recipeTabs = _recipeTabs.WithTabs(filterForm.FilteredTabs);
                recipeSelector1.ItemSource = _recipeTabs.Items.ToList();
                recipeSelector1.SolveRecipes();
            }
        }

        private void refreshAllTabsToolStripMenuItem_Click(object sender, EventArgs e) {
            StartFetchStashPagesAsync(cboLeague.Text, true);
        }

        private void removeFromStashToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_activeStash == null) return;
            if (SelectedRecipe == null) return;
            
            _recipeTabs = _recipeTabs.Filter(SelectedRecipe.Items);
            
            foreach (var tab in _recipeTabs) {
                FindStashGridForTab(tab.Tab).SetImages(tab);
            }

            recipeSelector1.ItemSource = _recipeTabs.Items.ToList();
            recipeSelector1.SolveRecipes();
        }

        private StashGrid FindStashGridForTab(PoEStashTab tab) {
            foreach (var page in tabControl1.TabPages.OfType<TabPage>()) {
                var stashGrid = page.Controls.OfType<StashGrid>().FirstOrDefault();
                if (stashGrid != null && stashGrid.StashTab == tab) {
                    return stashGrid;
                }
            }
            return null;
        }

        private void EnableUnsafeControls(bool state) {
            cboLeague.Enabled = state;
            refreshToolStripMenuItem.Enabled = state;
            refreshAllTabsToolStripMenuItem.Enabled = state;
            exportToolStripMenuItem.Enabled = state;
            removeFromStashToolStripMenuItem.Enabled = state;
            dataGridView1.Enabled = state;
            recipeSelector1.Enabled = state;
            btnTabs.Enabled = state;
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                var info = dataGridView1.HitTest(e.X, e.Y);
                dataGridView1.ClearSelection();
                if (info.RowIndex >= 0) {
                    dataGridView1.Rows[info.RowIndex].Selected = true;
                }
            }
        }
    }
}
