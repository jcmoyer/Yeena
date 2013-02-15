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
using System.ComponentModel;
using Yeena.PathOfExile;

namespace Yeena.Recipe {
    /// <summary>
    /// A Recipe is simply a list of items.
    /// </summary>
    class VendorRecipe : IEnumerable<PoEItem> {
        private readonly List<PoEItem> _items;

        public VendorRecipe(PoEItem item) {
            _items = new List<PoEItem> {
                item
            };
        }

        public VendorRecipe(IEnumerable<PoEItem> items) {
            _items = new List<PoEItem>(items);
        }

        public int ItemCount {
            get { return _items.Count; }
        }

        public string Ingredients {
            get { return string.Join(", ", _items); }
        }

        [Browsable(false)]
        public IReadOnlyList<PoEItem> Items {
            get { return _items; }
        }

        public IEnumerator<PoEItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
