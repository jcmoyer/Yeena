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
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    [JsonObject]
    class PoEItemModCategory : IEnumerable<PoEItemModList> {
        [JsonProperty("name")]
        private readonly string _name;
        [JsonProperty("modLists")]
        private readonly List<PoEItemModList> _modLists;

        public string Name { get { return _name; } }
        public IReadOnlyList<PoEItemModList> ModLists { get { return _modLists; } } 

        [JsonConstructor]
        private PoEItemModCategory() {
        }

        public PoEItemModCategory(string name, IEnumerable<PoEItemModList> mods) {
            _name = name;
            _modLists = mods.ToList();
        }

        public IEnumerator<PoEItemModList> GetEnumerator() {
            return _modLists.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            return Name;
        }
    }
}