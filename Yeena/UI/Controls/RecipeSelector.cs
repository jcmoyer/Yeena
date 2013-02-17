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
using Yeena.Recipe;

namespace Yeena.UI.Controls {
    partial class RecipeSelector : UserControl {
        public IReadOnlyList<VendorRecipe> Recipes { get; private set; }
        public IReadOnlyList<PoEItem> ItemSource { get; set; } 

        private List<RecipeSolver> _solvers = new List<RecipeSolver>();
        private Dictionary<string, RecipeSolver> _solverMap = new Dictionary<string, RecipeSolver>();

        public event EventHandler RecipesSolved;

        public RecipeSelector() {
            InitializeComponent();
        }

        public void RegisterRecipeSolver(RecipeSolver solver) {
            var recipeNameAttr = solver.GetType().GetCustomAttributes(false).OfType<VendorRecipeNameAttribute>().FirstOrDefault();
            if (recipeNameAttr != null) {
                comboBox1.Items.Add(recipeNameAttr.Name);
                _solvers.Add(solver);
                _solverMap[recipeNameAttr.Name] = solver;
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e) {
            SolveRecipes();
        }

        public async void SolveRecipes() {
            if (ItemSource == null) return;

            string name = (string)comboBox1.SelectedItem;

            // User hasn't selected a recipe yet
            if (String.IsNullOrEmpty(name)) return;

            RecipeSolver solver;

            if (_solverMap.TryGetValue(name, out solver)) {
                Recipes = (await solver.SolveAsync(ItemSource)).ToList();
                OnRecipesSolved(new EventArgs());
            }
        }

        protected void OnRecipesSolved(EventArgs e) {
            var handler = RecipesSolved;
            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
