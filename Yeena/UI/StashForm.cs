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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yeena.PathOfExile;
using Yeena.Recipe;
using Yeena.UI.Controls;

namespace Yeena.UI {
    public partial class StashForm : Form {
        private readonly PoESiteClient _client;

        private CancellationTokenSource _fetchCts;
        private Task _fetchTask;

        // Map of league to stash
        private ConcurrentDictionary<string, PoEStash> _leagueStashes = new ConcurrentDictionary<string, PoEStash>();
        private PoEStash _activeStash = null;

        private PoEItemTable _itemTable;

        public StashForm(PoESiteClient client) {
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
            recipeSelector1.ItemSource = _activeStash.Items;
        }

        private async void StartFetchStashPagesAsync(string league, bool refresh = false) {
            if (_leagueStashes.ContainsKey(league) && !refresh) {
                ShowStash(_leagueStashes[league]);
                return;
            }

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
        }

        // Asynchronously fetches all the stash pages for a given league
        private async Task FetchStashPagesAsync(string league, CancellationToken cancellationToken) {
            var stashTabs = new List<PoEStashTab>();

            // We need at least 1 tab to figure out how many tabs there are total
            var stash1 = await _client.GetStashTabAsync(league, 0, cancellationToken);
            var uiTab1 = CreateStashTabPage(stash1.TabInfo[0].Name, stash1);
            tabControl1.TabPages.Add(uiTab1);
            stashTabs.Add(stash1);

            for (int i = 1; i < stash1.TabCount; i++) {
                cancellationToken.ThrowIfCancellationRequested();

                var stashI = await _client.GetStashTabAsync(league, i, cancellationToken);

                var uiTabI = CreateStashTabPage(stashI.TabInfo[i].Name, stashI);
                tabControl1.TabPages.Add(uiTabI);
                stashTabs.Add(stashI);
            }

            var stash = _leagueStashes.GetOrAdd(league, _ => new PoEStash(stashTabs));
            _activeStash = stash;
            recipeSelector1.ItemSource = _activeStash.Items;
        }

        // Creates and returns a TabPage with a StashGrid control
        private TabPage CreateStashTabPage(string name, PoEStashTab tab) {
            TabPage tp = new TabPage(name);
            StashGrid grid = new StashGrid();
            grid.Dock = DockStyle.Fill;
            grid.SetImages(tab.Items);
            grid.Tag = tab;
            tp.Controls.Add(grid);
            tp.Tag = grid;
            return tp;
        }

        private void cboLeague_SelectedIndexChanged(object sender, EventArgs e) {
            StartFetchStashPagesAsync(cboLeague.Text);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e) {
            StartFetchStashPagesAsync(cboLeague.Text, true);
        }

        private async void StashForm_Load(object sender, EventArgs e) {
            _itemTable = await _client.GetItemTable();
            recipeSelector1.RegisterRecipeSolver(new WhetstoneSolver(_itemTable));
            recipeSelector1.RegisterRecipeSolver(new ScrapSolver(_itemTable));
            recipeSelector1.RegisterRecipeSolver(new BaubleSolver(_itemTable));
            recipeSelector1.RegisterRecipeSolver(new DuplicateRareSolver(_itemTable));

            StartFetchStashPagesAsync(cboLeague.Text);
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
            if (recipe != null) {
                foreach (var tcTab in tabControl1.TabPages.OfType<TabPage>()) {
                    tcTab.BackColor = default(Color);
                    StashGrid sg = (StashGrid)tcTab.Tag;
                    sg.ClearMarkings();
                }

                foreach (var item in recipe.Items) {
                    var responsibleTab = _activeStash.GetContainingTab(item);
                    foreach (var tcTab in tabControl1.TabPages.OfType<TabPage>()) {
                        StashGrid sg = (StashGrid)tcTab.Tag;
                        PoEStashTab stashTab = (PoEStashTab)sg.Tag;
                        if (stashTab == responsibleTab) {
                            var marking = new StashGridMarking(item);
                            sg.AddMarking(new StashGridMarking(item));

                            var solidBrush = marking.Brush as SolidBrush;
                            if (solidBrush != null) {
                                tcTab.BackColor = solidBrush.Color;
                            }
                        }
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
                        else if (_itemTable.IsEquippable(item)) sw.WriteLine(item.TypeLine);
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
            var g = Graphics.FromImage(b);
            for (int i = 0; i < _activeStash.Tabs.Count; i++) {
                var sg2 = tabControl1.TabPages[i].Controls.OfType<StashGrid>().First();
                sg2.DrawToBitmap(b, new Rectangle(i * sg.ClientSize.Width, 0, sg.ClientSize.Width, sg.ClientSize.Height));
            }
            b.Save(dialog.FileName, ImageFormat.Png);
        }

        private void StashForm_FormClosing(object sender, FormClosingEventArgs e) {
            string cookiesFile = Storage.ResolvePath("cookies.dat");
            BinaryFormatter bf = new BinaryFormatter();
            using (Stream s = File.OpenWrite(cookiesFile)) {
                bf.Serialize(s, _client.Cookies);
            }
        }
    }
}
