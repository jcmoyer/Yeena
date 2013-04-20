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

using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    [JsonObject]
    class PoEStashTabInfo {
        [JsonProperty("n")] private readonly string _name;
        [JsonProperty("i")] private readonly int _index;
        [JsonProperty("colour")] private readonly PoEStashTabColor _color;
        [JsonProperty("src")] private readonly string _src;

        public string Name {
            get { return _name; }
        }

        public int Index {
            get { return _index; }
        }

        public Color Color {
            get { return _color; }
        }

        /// <summary>
        /// Returns a fully qualified Uri that can be used to access the associated tab's
        /// display bitmap. This is the same image displayed on the site's stash browser.
        /// </summary>
        public Uri ImageUri {
            get { return new Uri(PoESite.UriHttps, _src); }
        }

        public override string ToString() {
            return _name;
        }
    }
}
