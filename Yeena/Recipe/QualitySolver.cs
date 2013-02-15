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
    abstract class QualitySolver : RecipeSolver {
        protected QualitySolver(PoEItemTable itemTable) : base(itemTable) {
        }

        // Adapted from http://stackoverflow.com/a/10739219
        // TODO: Generalize?
        protected static IEnumerable<List<PoEItem>> GetSubsetsOfQuality(PoEItem[] set, int sum, List<PoEItem> values = null) {
            if (values == null) values = new List<PoEItem>();
            for (int i = 0; i < set.Length; i++) {
                int left = sum - set[i].Quality;

                var vals = new List<PoEItem>(values);
                vals.Add(set[i]);
                
                if (left == 0) {
                    yield return vals;
                } else {
                    PoEItem[] possible = set.Take(i).Where(n => n.Quality <= sum).ToArray();
                    if (possible.Length > 0) {
                        foreach (var s in GetSubsetsOfQuality(possible, left, vals)) {
                            yield return s;
                        }
                    }
                }
            }
        }
    }
}
