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
    /// Represents an item that has been socketed into another item in Path of Exile.
    /// </summary>
    [JsonObject]
    class PoESocketedItem : PoEItem {
        [JsonProperty("socket")]
        private readonly int _socket;
        [JsonProperty("colour")]
        private readonly string _color;

        /// <summary>
        /// Returns the id of the socket this item has been socketed into.
        /// </summary>
        public int Socket { get { return _socket; } }

        /// <summary>
        /// Returns the color of the socket this item has been socketed into.
        /// </summary>
        public PoESocketColor Color { get { return PoESocketColorUtilities.Parse(_color); } }
    }
}
