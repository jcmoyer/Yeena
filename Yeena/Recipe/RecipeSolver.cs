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

using System.Collections.Generic;
using System.Threading.Tasks;
using Yeena.PathOfExile;

namespace Yeena.Recipe {
    public abstract class RecipeSolver {
        protected PoEItemTable ItemTable { get; private set; }

        protected RecipeSolver(PoEItemTable itemTable) {
            ItemTable = itemTable;
        }

        public abstract IEnumerable<VendorRecipe> Solve(IEnumerable<PoEItem> items);

        public Task<IEnumerable<VendorRecipe>> SolveAsync(IEnumerable<PoEItem> items) {
            return Task<IEnumerable<VendorRecipe>>.Factory.StartNew(() => Solve(items));
        }
    }
}
