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

using System.Collections.Generic;
using System.Linq;
using Yeena.PathOfExile;

namespace Yeena.Recipe {
    [VendorRecipeName("Reroll Rare (Five unidentified rare items with same base item name)")]
    class RerollRareSolver : RecipeSolver {
        public RerollRareSolver(PoEItemTable itemTable)
            : base(itemTable) {
        }

        public override IEnumerable<VendorRecipe> Solve(IEnumerable<PoEItem> items) {
            return from item in items
                   where item.FrameType == PoEItemFrameType.Rare && item.IsIdentified == false
                   group item by ItemTable.GetBaseItemName(item)
                       into grouping
                       let g = grouping
                       where g.Count() >= 5
                       select new VendorRecipe(g);
        }
    }
}
