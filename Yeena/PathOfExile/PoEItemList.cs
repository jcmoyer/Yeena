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
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    /// <summary>
    /// An item list is a list of items with a name that classifies their type,
    /// i.e. Claw has Nailed Fist, Sharktooth Claw, etc...
    /// </summary>
    [JsonObject]
    public class PoEItemList : IEnumerable<string> {
        [JsonProperty("name")]
        private readonly string _name;
        [JsonProperty("items")]
        private readonly List<string> _names; 

        [JsonConstructor]
        private PoEItemList() {
        }

        public PoEItemList(string name, IEnumerable<string> itemNames) {
            _name = name;
            _names = new List<string>(itemNames);
        }

        public IReadOnlyCollection<string> Names {
            get { return _names; }
        }

        public IEnumerator<string> GetEnumerator() {
            return _names.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
