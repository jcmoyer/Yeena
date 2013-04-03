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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    class PoESiteClient : IDisposable {
        private readonly HttpClientHandler _handler = new HttpClientHandler();
        private readonly HttpClient _client;

        //private readonly PoESiteSession _session = new PoESiteSession();

        public CookieContainer Cookies {
            get { return _handler.CookieContainer; }
            set { _handler.CookieContainer = value; }
        }

        public PoESiteClient() {
            _client = new HttpClient(_handler);

            // 2013-04-13: This appears to be set by the server now.

            // All requests require this cookie to be set to a constant value.
            //Cookie sessionStart = new Cookie("session_start", _session.SessionStart.ToString(), "/", PoESite.Uri.Host);
            //_handler.CookieContainer.Add(sessionStart);
        }

        ~PoESiteClient() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                if (_handler != null) {
                    _handler.Dispose();
                }
                if (_client != null) {
                    _client.Dispose();
                }
            }
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

        public Task<PoEStashTab> GetStashTabAsync(string league, int page = 0, int throttle = 2500) {
            return GetStashTabAsync(league, page, throttle, new CancellationToken());
        }

        /// <exception cref="System.InvalidCastException">Thrown if there's no stash for this league</exception>
        public async Task<PoEStashTab> GetStashTabAsync(string league, int page, int throttle, CancellationToken cancellationToken) {
            // This should probably be moved elsewhere.
            await Task.Delay(throttle);

            var result = await _client.PostAsync(PoESite.GetStashItems, new FormUrlEncodedContent(new Dictionary<string, string> {
                { "league", league },
                { "tabIndex", page.ToString() },
                { "tabs", "1" },
            }), cancellationToken);
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();

            StreamingContext c = new StreamingContext(StreamingContextStates.CrossMachine, page);
            JsonSerializer ser = new JsonSerializer();
            ser.Context = c;

            var stashTab = ser.Deserialize<PoEStashTab>(new JsonTextReader(new StringReader(json)));

            if (stashTab.Error != null) {
                // Retry later.
                int newThrottle = (int)(throttle * 1.2);
                await Task.Delay(throttle);
                return await GetStashTabAsync(league, page, newThrottle, cancellationToken);
            } else {
                return stashTab;
            }
        }

        public async Task<PoEStash> GetStashAsync(string league, int throttle = 2500) {
            PoEStashTab tab = await GetStashTabAsync(league);

            var tabs = new List<PoEStashTab>(tab.TabCount);
            for (int i = 1; i < tab.TabCount; i++) {
                tabs.Add(await GetStashTabAsync(league, i, throttle));
                await Task.Delay(throttle);
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

        public Task<PoEItemCategory> GetPrefixListAsync() {
            return GetItemListAsync(PoESite.ItemDataPrefixes, "Prefixes");
        }

        public Task<PoEItemCategory> GetSuffixListAsync() {
            return GetItemListAsync(PoESite.ItemDataSuffixes, "Suffixes");
        }

        public async Task<PoEItemTable> GetItemTable() {
            return new PoEItemTable(
                await GetWeaponListAsync(),
                await GetArmorListAsync(),
                await GetJewelryListAsync(),
                await GetCurrencyListAsync(),
                await GetPrefixListAsync(),
                await GetSuffixListAsync());
        }

        public async Task<IReadOnlyList<PoELeague>> GetLeagues() {
            var result = await _client.GetAsync(PoESite.Leagues);
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<PoELeague>>(json);
        }
    }
}
