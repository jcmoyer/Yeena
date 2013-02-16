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
    class PoEItemSocket {
        [JsonProperty("group")] private readonly int _group;
        [JsonProperty("attr")] private readonly string _attr;

        public int Group {
            get { return _group; }
        }

        public PoESocketColor Color {
            get {
                switch (_attr) {
                    case "S": return PoESocketColor.Red;
                    case "D": return PoESocketColor.Green;
                    case "I": return PoESocketColor.Blue;
                    default: return PoESocketColor.Unknown;
                }
            }
        }
    }
}
