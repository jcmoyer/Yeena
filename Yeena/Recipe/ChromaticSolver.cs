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
using System.Linq;
using Yeena.PathOfExile;

namespace Yeena.Recipe {
    [VendorRecipeName("Chromatic Orb")]
    class ChromaticSolver : RecipeSolver {
        public ChromaticSolver(PoEItemTable itemTable) : base(itemTable) {
        }

        public override IEnumerable<VendorRecipe> Solve(IEnumerable<PoEItem> items) {
            return from item in items
                   where item.SocketGroups.Any(HasRGB)
                   select new VendorRecipe(item);
        }

        private static bool HasRGB(IEnumerable<PoEItemSocket> socketGroup) {
            bool red = false, green = false, blue = false;

            foreach (var s in socketGroup) {
                switch (s.Color) {
                    case PoESocketColor.Red:
                        red = true;
                        break;
                    case PoESocketColor.Green:
                        green = true;
                        break;
                    case PoESocketColor.Blue:
                        blue = true;
                        break;
                }
            }

            return red && green && blue;
        }
    }
}
