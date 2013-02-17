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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yeena.PathOfExile;

namespace Yeena.Data {
    class StashTabView : IEnumerable<PoEItem> {
        private readonly PoEStashTab _tab;
        private readonly IReadOnlyCollection<PoEItem> _filtered;

        public StashTabView(PoEStashTab tab) {
            _tab = tab;
        }

        public StashTabView(PoEStashTab tab, IEnumerable<PoEItem> filtered) {
            _tab = tab;
            _filtered = filtered.ToList();
        }

        public IEnumerator<PoEItem> GetEnumerator() {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public PoEStashTab Tab {
            get { return _tab; }
        }

        public IEnumerable<PoEItem> Items {
            get {
                if (_filtered == null) {
                    return _tab;
                } else {
                    return _tab.Except(_filtered);
                }
            }
        }
    }
}
