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

using System.Linq;
using Newtonsoft.Json;
using Yeena.Utilities;

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
        private readonly PoEItemModCategory _prefixes;
        [JsonProperty("suffixes")]
        private readonly PoEItemModCategory _suffixes;

        public PoEItemTable(PoEItemCategory weapons, PoEItemCategory armor, PoEItemCategory jewelry,
                            PoEItemCategory currency, PoEItemModCategory prefixes, PoEItemModCategory suffixes) {
            _weapons = weapons;
            _armor = armor;
            _jewelry = jewelry;
            _currency = currency;
            _prefixes = prefixes;
            _suffixes = suffixes;
        }

        public bool IsEquippable(PoEItem item) {
            return IsWeapon(item) || IsArmor(item) || IsJewelry(item) || item.IsFlask;
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

        public string GetBaseItemName(PoEItem item) {
            if (item.IsFlask) {
                // If it's a flask the best option seems to be subtracting the prefix/suffix
                string itemName = item.TypeLine;
                var maybePrefix = _prefixes.Flatten().FirstOrDefault(a => item.TypeLine.StartsWith(a.Name));
                var maybeSuffix = _suffixes.Flatten().FirstOrDefault(a => item.TypeLine.EndsWith(a.Name));
                if (maybePrefix != null) {
                    itemName = itemName.Remove(0, maybePrefix.Name.Length);
                }
                if (maybeSuffix != null) {
                    itemName = itemName.Remove(itemName.Length - maybeSuffix.Name.Length - 1);
                }
                return itemName;
            }

            string maybeName = _weapons
                .Concat(_armor)
                .Concat(_jewelry)
                .Flatten()
                .FirstOrDefault(item.TypeLine.Contains);
            return maybeName ?? item.TypeLine;
        }
    }
}
