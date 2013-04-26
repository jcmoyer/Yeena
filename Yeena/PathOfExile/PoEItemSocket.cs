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
    /// Represents a socket in an item.
    /// </summary>
    [JsonObject]
    class PoEItemSocket {
        [JsonProperty("group")] private readonly int _group;
        [JsonProperty("attr")] private readonly string _attr;

        /// <summary>
        /// Returns the group this socket belongs in. Linked sockets will share the same group.
        /// </summary>
        public int Group {
            get { return _group; }
        }

        /// <summary>
        /// Returns the color of this socket.
        /// </summary>
        public PoESocketColor Color {
            get { return PoESocketColorUtilities.Parse(_attr); }
        }

        /// <summary>
        /// Returns a string representation of this socket.
        /// </summary>
        /// <returns>A string representation of this socket.</returns>
        public override string ToString() {
            return Color.ToString();
        }
    }
}
