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
    public class PoEStashTabInfo {
        [JsonProperty("n")] private readonly string _name;
        [JsonProperty("i")] private readonly int _index;
        // colour { r, g, b }
        // src

        public string Name {
            get { return _name; }
        }

        public int Index {
            get { return _index; }
        }
    }
}
