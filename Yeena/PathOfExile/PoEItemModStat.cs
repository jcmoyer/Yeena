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

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    [JsonObject]
    class PoEItemModStat {
        [JsonProperty("effect")]
        private readonly string _effect;

        [JsonProperty("min")]
        private readonly int _min;
        [JsonProperty("max")]
        private readonly int _max;

        public string Effect { get { return _effect; } }
        public int Min { get { return _min; } }
        public int Max { get { return _max; } }

        [JsonConstructor]
        private PoEItemModStat() {
        }

        public PoEItemModStat(string effect, int min, int max) {
            _effect = effect;
            _min = min;
            _max = max;
        }

        public override string ToString() {
            if (Min == Max) {
                return String.Format("{0}: {1}", Effect, Min);
            } else {
                return String.Format("{0}: {1} to {2}", Effect, Min, Max);
            }
        }

        public static IEnumerable<PoEItemModStat> Parse(string statText, string magText) {
            var splitChars = new[] {'\r', '\n'};

            string[] statLines = statText.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            string[] magLines = magText.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < statLines.Length; i++) {
                string statLine = statLines[i];
                // In case of more stat lines than mag lines, just use the previously available
                // mag line.
                string magLine = magLines[Math.Min(i, magLines.Length - 1)];

                int min, max;
                if (magLine.Contains("to")) {
                    min = Int32.Parse(magLine.Substring(0, magLine.IndexOf(' ')));
                    max = Int32.Parse(magLine.Substring(magLine.LastIndexOf(' ') + 1));
                } else {
                    min = max = Int32.Parse(magLine);
                }

                yield return new PoEItemModStat(statLine, min, max);
            }
        }
    }
}