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
using Yeena.PathOfExile;
using Yeena.Utilities;

namespace Yeena.Recipe {
    class ConfigurableQualitySolver : QualitySolver {
        private readonly Func<PoEItem, bool> _pred;

        public ConfigurableQualitySolver(PoEItemTable itemTable, Func<PoEItem, bool> pred) : base(itemTable) {
            _pred = pred;
        }

        public override IEnumerable<VendorRecipe> Solve(IEnumerable<PoEItem> items) {
            // Only operate on items that satisfy the configured predicate and that have
            // a quality greater than zero
            var qualityItems = from item in items
                               where _pred(item) && item.Quality > 0
                               select item;

            // Force enumeration into a list since we need to traverse it multiple times
            var qualityItemList = qualityItems.ToList();

            // Find all items that satisfy a quality recipe alone
            var q20 = from item in qualityItemList
                      where item.Quality == 20 &&
                      (!item.IsIdentified || (item.IsIdentified && !item.IsRare && !ItemTable.IsMagic(item)))
                      select item;
            var q20List = q20.ToList();
            var q20Recipes = from item in q20List select new VendorRecipe(new[] { item });

            // Find the recipes of all remaining items
            var remaining = qualityItemList.Except(q20List);
            var remainingRecipes = from subset in remaining.ToList().SubsetsWithSum(40, item => item.Quality)
                                   select new VendorRecipe(subset);

            // Return the two recipe sequences
            return q20Recipes.Concat(remainingRecipes);
        }
    }
}
