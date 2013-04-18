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
using System.Threading.Tasks;
using System.Windows.Forms;
using Yeena.Data;
using Yeena.PathOfExile;
using Yeena.Recipe;
using Yeena.UI.Controls;

namespace Yeena.UI {
    partial class StashForm : Form {
        private readonly PoESiteClient _client;

        // Map of league to stash
        private readonly Dictionary<string, PoEStash> _leagueStashes = new Dictionary<string, PoEStash>();
        private PoEStash _activeStash;
        private StashTabCollectionView _recipeTabs;

        private readonly JsonDiskCache<PoEItemTable> _itemTable = new JsonDiskCache<PoEItemTable>("ItemTable");
        private readonly ImageCache _imageCache = new ImageCache("Images");
        private readonly ApplicationSettings _settings;

        private readonly StashFetcher _stashFetcher;

        public StashForm(ApplicationSettings settings, PoESiteClient client) {
            _settings = settings;
            _client = client;
            _stashFetcher = new StashFetcher(_client);
            _stashFetcher.StashTabReceived += _stashFetcher_StashTabReceived;
            _stashFetcher.StashReceived += _stashFetcher_StashReceived;
            _stashFetcher.Begin += _stashFetcher_Begin;
            _stashFetcher.NoStashError += _stashFetcher_NoStashError;

            InitializeComponent();
        }

        void _stashFetcher_NoStashError(object sender, EventArgs e) {
            lblStatus.Text = "There is no stash in this league yet.";
        }

        void _stashFetcher_Begin(object sender, EventArgs e) {
            lblStatus.Text = "Fetching stash...";
            EnableUnsafeControls(false);
            tabStash.TabPages.Clear();
        }

        void _stashFetcher_StashTabReceived(object sender, StashTabReceivedEventArgs e) {
            tabStash.AddStashTab(e.StashTab, new StashGrid(_itemTable, _imageCache));

            lblStatus.Text = String.Format("Received tab {0}/{1}", e.StashTab.TabInfo.Index + 1, e.StashTab.TabCount);
        }

        void _stashFetcher_StashReceived(object sender, StashReceivedEventArgs e) {
            _activeStash = e.Stash;
            _leagueStashes[e.League] = e.Stash;
            _recipeTabs = new StashTabCollectionView(_activeStash.Tabs);
            recSelector.ItemSource = _recipeTabs.Items.ToList();

            lblStatus.Text = "Ready";
            EnableUnsafeControls(true);
        }

        private void ShowStash(PoEStash stash) {
            tabStash.TabPages.Clear();
            foreach (var tab in stash.Tabs) {
                tabStash.AddStashTab(tab, new StashGrid(_itemTable, _imageCache));
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

            await _stashFetcher.FetchAsync(league);
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

        protected override async void OnLoad(EventArgs e) {
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
            recSelector.RegisterRecipeSolver(new RerollMagicSolver(_itemTable));
            recSelector.RegisterRecipeSolver(new RerollRareSolver(_itemTable));

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

            foreach (var tab in _activeStash.Tabs) {
                tabStash.GetTabPageForStashTab(tab).BackColor = default(Color);
                tabStash.GetStashGridForStashTab(tab).ClearMarkings();
            }

            if (recipe == null) {
                tabStash.Invalidate();
                return;
            }

            foreach (var item in recipe) {
                var responsibleTab = _activeStash.GetContainingTab(item);

                var page = tabStash.GetTabPageForStashTab(responsibleTab);
                var grid = tabStash.GetStashGridForStashTab(responsibleTab);

                var marking = new StashGridMarking(item);
                grid.AddMarking(marking);

                var solidBrush = marking.Brush as SolidBrush;
                if (solidBrush != null) {
                    page.BackColor = solidBrush.Color;
                }
            }

            tabStash.Invalidate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
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

        protected override void OnFormClosing(FormClosingEventArgs e) {
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
            
            foreach (var tabView in _recipeTabs) {
                tabStash.GetStashGridForStashTab(tabView.Tab).TabView = tabView;
            }

            recSelector.ItemSource = _recipeTabs.Items.ToList();
            recSelector.SolveRecipes();
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
