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

namespace Yeena.PathOfExile {
    class PoEStash : IReadOnlyList<PoEStashTab> {
        private readonly List<PoEStashTab> _tabs;

        public PoEStash(IEnumerable<PoEStashTab> tabs) {
            _tabs = new List<PoEStashTab>(tabs);
        }

        public IReadOnlyList<PoEStashTab> Tabs {
            get { return _tabs; }
        }

        public PoEStashTab GetContainingTab(PoEItem item) {
            foreach (var tab in _tabs) {
                foreach (var tabItem in tab) {
                    if (item == tabItem) return tab;
                }
            }
            return null;
        }

        public IEnumerator<PoEStashTab> GetEnumerator() {
            return Tabs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count {
            get { return Tabs.Count; }
        }

        public PoEStashTab this[int index] {
            get { return Tabs[index]; }
        }
    }
}