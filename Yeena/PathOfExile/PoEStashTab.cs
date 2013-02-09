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
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    [JsonObject]
    public class PoEStashTab : IEnumerable<PoEItem> {
        [JsonProperty("numTabs")] private readonly int _numTabs = 0;
        [JsonProperty("items")] private readonly List<PoEItem> _items = new List<PoEItem>();
        [JsonProperty("tabs")] private readonly List<PoEStashTabInfo> _tabInfo = new List<PoEStashTabInfo>();
        [JsonProperty("error")] private readonly PoEStashRequestError _error;
        
        private PoEStashTabInfo _assocInfo;

        public int TabCount {
            get { return _numTabs; }
        }

        public IReadOnlyCollection<PoEItem> Items {
            get { return _items; }
        }

        public PoEStashTabInfo TabInfo {
            get { return _assocInfo; }
        }

        public PoEStashRequestError Error {
            get { return _error; }
        }

        public IEnumerator<PoEItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            // Something went wrong. The caller can handle this.
            if (Error != null) return;
            _assocInfo = _tabInfo[(int)context.Context];
        }
    }
}
