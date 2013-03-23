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

using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    [JsonObject]
    class PoELeague {
        [JsonProperty("id")]
        private readonly string _id;
        [JsonProperty("description")]
        private readonly string _description;
        [JsonProperty("url")]
        private readonly string _url;
        [JsonProperty("event")]
        private readonly bool _event;
        
        public string Name {
            get { return _id; }
        }

        public string Description {
            get { return _description; }
        }
    }
}
