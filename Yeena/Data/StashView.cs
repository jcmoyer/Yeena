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

namespace Yeena.Data {
    /// <summary>
    /// Provides a read-only view of a PoEStash with convenient methods for reading data from it.
    /// </summary>
    class StashView {
        private readonly PoEStash _stash;

        public StashView(PoEStash stash) {
            _stash = stash;
        }

        public StashTabCollectionView Tabs {
            get { return new StashTabCollectionView(_stash); }
        }

        public StashTabCollectionView FilterTabs(IEnumerable<int> indices) {
            var filteredTabs = from tab in _stash
                               from index in indices
                               where tab.TabInfo.Index == index
                               select tab;
            return new StashTabCollectionView(filteredTabs);
        }
    }
}
