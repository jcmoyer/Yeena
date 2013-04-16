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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    /// <summary>
    /// Repesents an item property.
    /// </summary>
    [JsonObject]
    class PoEItemProperty {
        private static readonly Regex FormatSpecifier = new Regex(@"%(\d+)", RegexOptions.Compiled);

        [JsonProperty("name")] private readonly string _name;
        [JsonProperty("values")] private readonly List<List<object>> _values;
        [JsonProperty("displayMode")] private readonly int _displayMode;

        private readonly Lazy<string> _displayText;

        /// <summary>
        /// Returns the name of this property.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Returns the values associated with this property.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<object>> Values { get { return _values; } }

        /// <summary>
        /// Unknown.
        /// </summary>
        public int DisplayMode { get { return _displayMode; } }

        /// <summary>
        /// Returns display-friendly text for this property.
        /// </summary>
        public string DisplayText {
            get { return _displayText.Value; }
        }

        [JsonConstructor]
        public PoEItemProperty() {
            Func<string> displayTextFactory = () => {
                // If there's no % in it then it's not a formatted string?
                // But we still need to take the first value from the first property list.
                if (!_name.Contains("%")) {
                    if (Values.Count > 0 && Values[0].Count > 0) {
                        return _name + " " + Values[0][0];
                    }
                }

                return FormatSpecifier.Replace(_name, (match) => {
                    int which = Int32.Parse(match.Groups[1].Value);
                    return _values[which][0].ToString();
                });
            };

            _displayText = new Lazy<string>(displayTextFactory);
        }

        /// <summary>
        /// Returns a string representation for this property.
        /// </summary>
        /// <returns>A string representation for this property.</returns>
        public override string ToString() {
            return DisplayText;
        }
    }
}