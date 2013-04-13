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
    class PoEItemModList : IEnumerable<PoEItemMod> {
        [JsonProperty("name")]
        private readonly string _name;
        [JsonProperty("mods")]
        private readonly List<PoEItemMod> _mods;

        public string Name { get { return _name; } }
        public IReadOnlyList<PoEItemMod> Mods { get { return _mods; } } 

        [JsonConstructor]
        private PoEItemModList() {
        }

        public PoEItemModList(string name, IEnumerable<PoEItemMod> mods) {
            _name = name;
            _mods = mods.ToList();
        }

        public IEnumerator<PoEItemMod> GetEnumerator() {
            return _mods.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            return Name;
        }
    }
}