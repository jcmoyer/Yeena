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

#pragma warning disable 649

using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    /// <summary>
    /// Represents a league in Path of Exile.
    /// </summary>
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
        
        /// <summary>
        /// Returns the name of the league.
        /// </summary>
        public string Name {
            get { return _id; }
        }

        /// <summary>
        /// Returns a description of the league.
        /// </summary>
        public string Description {
            get { return _description; }
        }

        /// <summary>
        /// Returns a URL where more information can be obtained about the league.
        /// </summary>
        public string Url {
            get { return _url; }
        }

        /// <summary>
        /// Returns whether or not this league is an event.
        /// </summary>
        public bool IsEvent {
            get { return _event; }
        }

        /// <summary>
        /// Returns a string representation of this league.
        /// </summary>
        /// <returns>The name of this league.</returns>
        public override string ToString() {
            return Name;
        }
    }
}
