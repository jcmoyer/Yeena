﻿// Copyright 2013 J.C. Moyer
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
    /// <summary>
    /// Represents the color of a stash tab.
    /// </summary>
    [JsonObject]
    class PoEStashTabColor {
        [JsonProperty("r")]
        private readonly int _r;
        [JsonProperty("g")]
        private readonly int _g;
        [JsonProperty("b")]
        private readonly int _b;

        private readonly Lazy<Color> _color;

        /// <summary>
        /// Returns the color of a stash tab.
        /// </summary>
        /// <see cref="System.Drawing.Color"/>
        public Color Color {
            get { return _color.Value; }
        }

        [JsonConstructor]
        private PoEStashTabColor() {
            _color = new Lazy<Color>(() => Color.FromArgb(_r, _g, _b));
        }

        public static implicit operator Color(PoEStashTabColor c) {
            return c.Color;
        }
    }
}
