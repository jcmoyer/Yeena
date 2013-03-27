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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
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

        private readonly JsonDiskCache<PoEItemTable> _itemTable = new JsonDiskCache<PoEItemTable>("ItemTable");
        private readonly ImageCache _imageCache = new ImageCache("Images");
        private readonly ApplicationSettings _settings;

        public StashForm(ApplicationSettings settings, PoESiteClient client) {
            _settings = settings;
            _client = client;

            InitializeComponent();
        }

        private void ShowStash(PoEStash stash) {
            tabStash.TabPages.Clear();
            foreach (var tab in stash.Tabs) {
                var uiTab1 = CreateStashTabPage(tab.TabInfo.Name, tab);
                tabStash.TabPages.Add(uiTab1);
            }
            _activeStash = stash;
            _recipeTabs = new StashTabCollectionView(_activeStash.Tabs);
            recSelector.ItemSource = _recipeTabs.Items.ToList();
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
            } catch (TaskCanceledException) {
            }

            EnableUnsafeControls(false);

            tabStash.TabPages.Clear();
            _fetchCts = new CancellationTokenSource();
            _fetchTask = FetchStashPagesAsync(cboLeague.Text, _fetchCts.Token);

            try {
                await _fetchTask;
            } catch (TaskCanceledException) {

            }

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
            } catch (JsonSerializationException) {
                lblStatus.Text = "There is no stash in this league yet.";
                return;
            }
            var uiTab1 = CreateStashTabPage(stash1.TabInfo.Name, stash1);
            tabStash.TabPages.Add(uiTab1);
            stashTabs.Add(stash1);

            for (int i = 1; i < stash1.TabCount; i++) {
                cancellationToken.ThrowIfCancellationRequested();

                lblStatus.Text = String.Format("Fetching stash page {0}/{1}...", i + 1, stash1.TabCount);
                var stashI = await _client.GetStashTabAsync(league, i, 2500, cancellationToken);

                var uiTabI = CreateStashTabPage(stashI.TabInfo.Name, stashI);
                tabStash.TabPages.Add(uiTabI);
                stashTabs.Add(stashI);
            }

            lblStatus.Text = "Finalizing...";
            var stash = new PoEStash(stashTabs);
            _activeStash = stash;
            _recipeTabs = new StashTabCollectionView(_activeStash.Tabs);
            recSelector.ItemSource = _recipeTabs.Items.ToList();

            lblStatus.Text = "Ready";
        }

        // Creates and returns a TabPage with a StashGrid control
        private TabPage CreateStashTabPage(string name, PoEStashTab tab) {
            TabPage tp = new TabPage(name);
            StashGrid grid = new StashGrid(_itemTable, _imageCache);
            grid.Dock = DockStyle.Fill;
            grid.StashTab = tab;
            grid.Tag = tab;
            tp.Controls.Add(grid);
            tp.Tag = grid;
            return tp;
        }

        private void cboLeague_SelectedIndexChanged(object sender, EventArgs e) {
            StartFetchStashPagesAsync(cboLeague.Text);
        }

        private async void refreshToolStripMenuItem_Click(object sender, EventArgs e) {
            var selectedGrid = tabStash.SelectedTab.Controls.OfType<StashGrid>().FirstOrDefault();
            if (selectedGrid == null) return;

            EnableUnsafeControls(false);

            var tab = await _client.GetStashTabAsync(cboLeague.Text, tabStash.SelectedIndex);
            selectedGrid.StashTab = tab;

            EnableUnsafeControls(true);
        }

        private async void StashForm_Load(object sender, EventArgs e) {
            lblStatus.Text = "Loading league list...";
            var leagues = await _client.GetLeagues();
            foreach (var league in leagues) {
                cboLeague.Items.Add(league.Name);
            }

            lblStatus.Text = "Loading item table...";
            await _itemTable.LoadAsync(_client.GetItemTable);

            lblStatus.Text = "Loading image cache...";
            await _imageCache.LoadAsync(() => Task.Factory.StartNew(() => new ConcurrentDictionary<Uri, Image>()));

            recSelector.RegisterRecipeSolver(new WhetstoneSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new ScrapSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new BaubleSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new PrismSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new ChiselSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new DuplicateRareSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new SixSocketSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new DivineSixSocketSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new ChromaticSolver(_itemTable));

            var lastLeague = _settings.LastLeagueName;
            if (!String.IsNullOrEmpty(lastLeague)) {
                // This is going to fire StartFetchStashPagesAsync.
                cboLeague.Text = lastLeague;
            } else {
                // Default to the first league.
                cboLeague.SelectedIndex = 0;
            }
        }

        private void recipeSelector1_RecipesSolved(object sender, EventArgs e) {
            dgvRecipes.DataSource = recSelector.Recipes.OrderByDescending(r => r.ItemCount).ToList();
        }

        private VendorRecipe SelectedRecipe {
            get {
                if (dgvRecipes.SelectedRows.Count > 0) {
                    return (VendorRecipe)dgvRecipes.SelectedRows[0].DataBoundItem;
                }
                return null;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e) {
            var recipe = SelectedRecipe;

            foreach (var tcTab in tabStash.TabPages.OfType<TabPage>()) {
                tcTab.BackColor = default(Color);
                StashGrid sg = (StashGrid)tcTab.Tag;
                sg.ClearMarkings();
            }

            if (recipe == null) {
                return;
            }

            foreach (var item in recipe.Items) {
                var responsibleTab = _activeStash.GetContainingTab(item);
                foreach (var tcTab in tabStash.TabPages.OfType<TabPage>()) {
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

            tabStash.Invalidate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e) {
            TabPage page = tabStash.TabPages[e.Index];
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

            DoSummarize(new StashTextSummarizer(_itemTable), dialog.FileName);
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e) {
            FileDialog dialog = new SaveFileDialog();
            dialog.Filter = "BMP Images (*.bmp)|*.bmp|GIF Images (*.gif)|*.gif|JPEG Images (*.jpg)|*.jpg|PNG Images (*.png)|*.png|All Files (*.*)|*.*";
            dialog.DefaultExt = "png";
            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            using (var summarizer = new StashImageSummarizer(_imageCache)) {
                DoSummarize(summarizer, dialog.FileName);
            }
        }

        private void DoSummarize(IStashSummarizer summarizer, string filename) {
            var form = new SummarizerForm(summarizer);
            if (form.ShowDialog() == DialogResult.OK) {
                summarizer.Summarize(filename, _activeStash);
            }
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
                recSelector.ItemSource = _recipeTabs.Items.ToList();
                recSelector.SolveRecipes();
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
                FindStashGridForTab(tab.Tab).StashTab = tab.Tab;
            }

            recSelector.ItemSource = _recipeTabs.Items.ToList();
            recSelector.SolveRecipes();
        }

        private StashGrid FindStashGridForTab(PoEStashTab tab) {
            foreach (var page in tabStash.TabPages.OfType<TabPage>()) {
                var stashGrid = page.Controls.OfType<StashGrid>().FirstOrDefault();
                if (stashGrid != null && stashGrid.StashTab == tab) {
                    return stashGrid;
                }
            }
            return null;
        }

        private void EnableUnsafeControls(bool state) {
            refreshToolStripMenuItem.Enabled = state;
            refreshAllTabsToolStripMenuItem.Enabled = state;
            exportToolStripMenuItem.Enabled = state;
            removeFromStashToolStripMenuItem.Enabled = state;
            dgvRecipes.Enabled = state;
            recSelector.Enabled = state;
            btnTabs.Enabled = state;
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                var info = dgvRecipes.HitTest(e.X, e.Y);
                dgvRecipes.ClearSelection();
                if (info.Type == DataGridViewHitTestType.Cell) {
                    dgvRecipes.Rows[info.RowIndex].Selected = true;
                }
            }
        }
    }
}
