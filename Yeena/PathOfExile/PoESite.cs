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

namespace Yeena.PathOfExile {
    public class PoESite {
        // Base uris
        public static readonly Uri Uri = new Uri("http://www.pathofexile.com/");
        public static readonly Uri UriHttps = new Uri("https://www.pathofexile.com/");

        // Functionality
        public static readonly Uri Login = new Uri(UriHttps, "login");

        // Character window
        public static readonly Uri GetStashItems = new Uri(UriHttps, "character-window/get-stash-items");

        // Item data
        public static readonly Uri ItemDataWeapon = new Uri(UriHttps, "item-data/weapon");
        public static readonly Uri ItemDataArmor = new Uri(UriHttps, "item-data/armour");
        public static readonly Uri ItemDataJewelry = new Uri(UriHttps, "item-data/jewelry");
        public static readonly Uri ItemDataCurrency = new Uri(UriHttps, "item-data/currency");
    }
}
