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
using System.Linq;

namespace Yeena.PathOfExile {
    public class PoEItemTable {
        private readonly PoEItemCategory _weapons;
        private readonly PoEItemCategory _armor;
        private readonly PoEItemCategory _jewelry;
        private readonly PoEItemCategory _currency;

        public PoEItemTable(PoEItemCategory weapons, PoEItemCategory armor, PoEItemCategory jewelry,
                            PoEItemCategory currency) {
            _weapons = weapons;
            _armor = armor;
            _jewelry = jewelry;
            _currency = currency;
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
            throw new NotImplementedException();
        }
    }
}
