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
    class StashTabCollectionView : IEnumerable<StashTabView> {
        private readonly IReadOnlyCollection<StashTabView> _tabs;
        private readonly IReadOnlyCollection<PoEItem> _filtered;

        private StashTabCollectionView(IEnumerable<StashTabView> tabs, IEnumerable<PoEItem> filtered) {
            _filtered = filtered.ToList();
            _tabs = tabs.Select(t => new StashTabView(t.Tab, _filtered)).ToList();
        }

        public StashTabCollectionView(IEnumerable<PoEStashTab> tabs) {
            _tabs = tabs.Select(t => new StashTabView(t)).ToList();
        }

        public StashTabCollectionView(IEnumerable<PoEStashTab> tabs, IEnumerable<PoEItem> filtered) {
            _tabs = tabs.Select(t => new StashTabView(t)).ToList();
            if (_filtered != null) {
                _filtered = filtered.ToList();
            }
        }

        public IEnumerator<StashTabView> GetEnumerator() {
            return _tabs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public StashTabCollectionView WithTabs(IEnumerable<PoEStashTab> tabs) {
            return new StashTabCollectionView(tabs, _filtered);
        }

        public StashTabCollectionView Filter(IEnumerable<PoEItem> items) {
            if (_filtered == null) {
                return new StashTabCollectionView(_tabs, items);
            } else {
                return new StashTabCollectionView(_tabs, _filtered.Concat(items));
            }
        }

        public IEnumerable<PoEStashTab> Tabs {
            get { return _tabs.Select(t => t.Tab); }
        } 

        public IEnumerable<PoEItem> Items {
            get {
                var baseQuery = _tabs.SelectMany(t => t);
                if (_filtered == null) {
                    return baseQuery;
                } else {
                    return baseQuery.Except(_filtered);
                }
            }
        }
    }
}