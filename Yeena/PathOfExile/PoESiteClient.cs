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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    public class PoESiteClient {
        private readonly HttpClientHandler _handler = new HttpClientHandler();
        private readonly HttpClient _client;

        private readonly PoESiteSession _session = new PoESiteSession();

        public CookieContainer Cookies {
            get { return _handler.CookieContainer; }
            set { _handler.CookieContainer = value; }
        }

        public PoESiteClient() {
            _client = new HttpClient(_handler);
            
            // All requests require this cookie to be set to a constant value.
            Cookie sessionStart = new Cookie("session_start", _session.SessionStart.ToString(), "/", PoESite.Uri.Host);
            _handler.CookieContainer.Add(sessionStart);
        }

        public async Task<string> LoginAsync(PoESiteCredentials creds) {
            var result = await _client.PostAsync(PoESite.Login, new FormUrlEncodedContent(new Dictionary<string, string> {
                { "login", "Login" },
                { "login_email", creds.Username },
                { "login_password", creds.Password },
                { "remember_me", "1" },
            }));
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync();
        }

        public Task<PoEStashTab> GetStashTabAsync(string league, int page = 0) {
            return GetStashTabAsync(league, page, new CancellationToken());
        }

        public async Task<PoEStashTab> GetStashTabAsync(string league, int page, CancellationToken cancellationToken) {
            var result = await _client.PostAsync(PoESite.GetStashItems, new FormUrlEncodedContent(new Dictionary<string, string> {
                { "league", league },
                { "tabIndex", page.ToString() },
                { "tabs", "1" },
            }), cancellationToken);
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PoEStashTab>(json);
        }

        public async Task<PoEStash> GetStashAsync(string league) {
            PoEStashTab tab = await GetStashTabAsync(league);

            var tabs = new List<PoEStashTab>(tab.TabCount);
            for (int i = 1; i < tab.TabCount; i++) {
                tabs.Add(await GetStashTabAsync(league, i));
            }

            return new PoEStash(tabs);
        }

        private async Task<PoEItemCategory> GetItemListAsync(Uri uri, string categoryName) {
            var result = await _client.GetAsync(uri);
            result.EnsureSuccessStatusCode();

            var doc = new HtmlDocument();
            doc.Load(await result.Content.ReadAsStreamAsync());

            var categories = doc.DocumentNode.SelectNodes("//div[contains(@class, 'layoutBoxFull')]");
            var itemLists = new List<PoEItemList>();

            foreach (var category in categories) {
                var titleNode = category.SelectSingleNode(".//h1[contains(@class, 'layoutBoxTitle')]");
                var itemNames = category.SelectNodes(".//td[contains(@class, 'name')]");
                itemLists.Add(new PoEItemList(titleNode.InnerText, itemNames.Select(el => el.InnerText)));
            }

            return new PoEItemCategory(categoryName, itemLists);
        }

        public Task<PoEItemCategory> GetWeaponListAsync() {
            return GetItemListAsync(PoESite.ItemDataWeapon, "Weapon");
        }

        public Task<PoEItemCategory> GetArmorListAsync() {
            return GetItemListAsync(PoESite.ItemDataArmor, "Armor");
        }

        public Task<PoEItemCategory> GetJewelryListAsync() {
            return GetItemListAsync(PoESite.ItemDataJewelry, "Jewelry");
        }

        public Task<PoEItemCategory> GetCurrencyListAsync() {
            return GetItemListAsync(PoESite.ItemDataCurrency, "Currency");
        }

        public async Task<PoEItemTable> GetItemTable() {
            return new PoEItemTable(
                await GetWeaponListAsync(),
                await GetArmorListAsync(),
                await GetJewelryListAsync(),
                await GetCurrencyListAsync());
        }
    }
}
