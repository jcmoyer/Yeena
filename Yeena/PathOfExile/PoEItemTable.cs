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
using System.Linq;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    [JsonObject]
    class PoEItemTable {
        [JsonProperty("weapons")]
        private readonly PoEItemCategory _weapons;
        [JsonProperty("armor")]
        private readonly PoEItemCategory _armor;
        [JsonProperty("jewelry")]
        private readonly PoEItemCategory _jewelry;
        [JsonProperty("currency")]
        private readonly PoEItemCategory _currency;
        [JsonProperty("prefixes")]
        private readonly PoEItemCategory _prefixes;
        [JsonProperty("suffixes")]
        private readonly PoEItemCategory _suffixes;

        public PoEItemTable(PoEItemCategory weapons, PoEItemCategory armor, PoEItemCategory jewelry,
                            PoEItemCategory currency, PoEItemCategory prefixes, PoEItemCategory suffixes) {
            _weapons = weapons;
            _armor = armor;
            _jewelry = jewelry;
            _currency = currency;
            _prefixes = prefixes;
            _suffixes = suffixes;
        }

        public bool IsEquippable(PoEItem item) {
            return IsWeapon(item) || IsArmor(item) || IsJewelry(item) || IsFlask(item);
        }

        public bool IsWeapon(PoEItem item) {
            return _weapons.Any(il => il.Any(item.TypeLine.Contains));
        }

        public bool IsArmor(PoEItem item) {
            return _armor.Any(il => il.Any(item.TypeLine.Contains));
        }

        public bool IsJewelry(PoEItem item) {
            return _jewelry.Any(il => il.Any(item.TypeLine.Contains));
        }

        public bool IsCurrency(PoEItem item) {
            return _currency.Any(il => il.Any(item.TypeLine.Contains));
        }

        public bool IsFlask(PoEItem item) {
            return item.TypeLine.Contains("Flask");
        }

        public bool IsGem(PoEItem item) {
            return item.HasProperty("Experience");
        }

        public bool IsMagic(PoEItem item) {
            string baseName = GetBaseItemName(item);
            // In the case that the typeline is the same as the base name we know there is no
            // prefix or suffix. This solves an edge case where some prefixes overlap with item
            // names (i.e. without this check, Sapphire Rings are treated as magic because
            // Sapphire is also a prefix)
            if (item.TypeLine == baseName) return false;
            return _prefixes.Any(p => p.Any(item.TypeLine.StartsWith)) || _suffixes.Any(s => s.Any(item.TypeLine.EndsWith));
        }

        public string GetBaseItemName(PoEItem item) {
            if (IsFlask(item)) {
                // If it's a flask the best option seems to be subtracting the prefix/suffix
                string itemName = item.TypeLine;
                string maybePrefix = _prefixes.SelectMany(list => list).FirstOrDefault(item.TypeLine.StartsWith);
                string maybeSuffix = _prefixes.SelectMany(list => list).FirstOrDefault(item.TypeLine.StartsWith);
                if (maybePrefix != null) {
                    itemName = itemName.Remove(0, maybePrefix.Length);
                }
                if (maybeSuffix != null) {
                    itemName = itemName.Remove(itemName.Length - maybeSuffix.Length - 1, maybeSuffix.Length);
                }
                return itemName;
            }

            string maybeName = _weapons
                .Concat(_armor)
                .Concat(_jewelry)
                .SelectMany(list => list)
                .FirstOrDefault(item.TypeLine.Contains);
            return maybeName ?? item.TypeLine;
        }
    }
}
