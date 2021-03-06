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

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    /// <summary>
    /// A category of items is a read-only list of ItemLists, mapping to a level 1
    /// item group, i.e. Weapon is a category of Claw, Bow, Dagger, etc...
    /// </summary>
    [JsonObject]
    class PoEItemCategory : IReadOnlyList<PoEItemList> {
        [JsonProperty("name")]
        private readonly string _name;
        [JsonProperty("items")]
        private readonly List<PoEItemList> _items; 

        public PoEItemCategory(string name, IEnumerable<PoEItemList> items) {
            _name = name;
            _items = new List<PoEItemList>(items);
        }

        public IEnumerator<PoEItemList> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count {
            get { return _items.Count; }
        }

        public PoEItemList this[int index] {
            get { return _items[index]; }
        }

        public override string ToString() {
            return _name;
        }
    }
}