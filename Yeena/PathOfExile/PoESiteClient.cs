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
    /// <summary>
    /// Communicates with the Path of Exile website via HTTP, using HTTPS where possible.
    /// </summary>
    class PoESiteClient : IDisposable {
        private readonly HttpClientHandler _handler = new HttpClientHandler();
        private readonly HttpClient _client;

        /// <summary>
        /// Allows manipulation of the cookies stored by the underlying HttpClientHandler.
        /// </summary>
        /// <see cref="CookieContainer"/>
        public CookieContainer Cookies {
            get { return _handler.CookieContainer; }
            set { _handler.CookieContainer = value; }
        }

        public PoESiteClient() {
            _client = new HttpClient(_handler);
        }

        ~PoESiteClient() {
            Dispose(false);
        }

        /// <summary>
        /// Releases the unmanaged resources and disposes of the managed resources used by the Yeena.PathOfExile.PoESiteClient.
        /// </summary>
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

        /// <summary>
        /// Asynchronously logs in to the Path of Exile website.
        /// </summary>
        /// <param name="creds">Credential object that holds the username and password.</param>
        /// <returns>A task that returns the HTML of the page returned by the Path of Exile web server.</returns>
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

        /// <summary>
        /// Asynchronously fetches a single stash tab. This operation requires you to have logged in successfully. You cannot cancel this task.
        /// </summary>
        /// <param name="league">League to fetch the stash from.</param>
        /// <param name="page">Page number to fetch.</param>
        /// <returns>A task that returns a stash tab.</returns>
        /// <see cref="LoginAsync"/>
        /// <see cref="PoEStashTab"/>
        public Task<PoEStashTab> GetStashTabAsync(string league, int page = 0) {
            return GetStashTabAsync(league, page, new CancellationToken());
        }

        /// <summary>
        /// Asynchronously fetches a single stash tab. This operation requires you to have logged in successfully.
        /// </summary>
        /// <param name="league">League to fetch the stash from.</param>
        /// <param name="page">Page number to fetch.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task.</param>
        /// <returns>A task that returns a stash tab.</returns>
        /// <see cref="LoginAsync"/>
        /// <see cref="PoEStashTab"/>
        public async Task<PoEStashTab> GetStashTabAsync(string league, int page, CancellationToken cancellationToken) {
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
            return stashTab;
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

        private async Task<PoEItemModCategory> GetModListAsync(Uri uri, string categoryName) {
            var result = await _client.GetAsync(uri);
            result.EnsureSuccessStatusCode();

            var doc = new HtmlDocument();
            doc.Load(await result.Content.ReadAsStreamAsync());

            var categories = doc.DocumentNode.SelectNodes("//div[contains(@class, 'layoutBoxFull')]");

            var modLists = new List<PoEItemModList>();
            foreach (var category in categories) {
                var titleNode = category.SelectSingleNode(".//h1[contains(@class, 'layoutBoxTitle')]");
                var itemNames = category.SelectNodes(".//td[contains(@class, 'name')]");

                var modList = new List<PoEItemMod>();
                foreach (var nameEl in itemNames) {
                    var modName = nameEl.InnerText;

                    var levelEl = nameEl.NextSibling.NextSibling;
                    var level = Int32.Parse(levelEl.InnerText);

                    var effectEl = levelEl.NextSibling.NextSibling;
                    var effect = String.Join(Environment.NewLine, effectEl.ChildNodes.Select(a => a.InnerText));

                    var magnitudeEl = effectEl.NextSibling.NextSibling;
                    var magnitude = String.Join(Environment.NewLine, magnitudeEl.ChildNodes.Select(a => a.InnerText));

                    modList.Add(new PoEItemMod(modName, level, PoEItemModStat.Parse(effect, magnitude)));
                }
                modLists.Add(new PoEItemModList(titleNode.InnerText, modList));
            }

            return new PoEItemModCategory(categoryName, modLists);
        }

        /// <summary>
        /// Asynchronously fetches a category of weapon items from the Path of Exile website.
        /// </summary>
        /// <returns>A task that returns a category of weapon items.</returns>
        /// <see cref="PoEItemCategory"/>
        public Task<PoEItemCategory> GetWeaponListAsync() {
            return GetItemListAsync(PoESite.ItemDataWeapon, "Weapon");
        }

        /// <summary>
        /// Asynchronously fetches a category of armor items from the Path of Exile website.
        /// </summary>
        /// <returns>A task that returns a category of armor items.</returns>
        /// <see cref="PoEItemCategory"/>
        public Task<PoEItemCategory> GetArmorListAsync() {
            return GetItemListAsync(PoESite.ItemDataArmor, "Armor");
        }

        /// <summary>
        /// Asynchronously fetches a category of jewelry items from the Path of Exile website.
        /// </summary>
        /// <returns>A task that returns a category of jewelry items.</returns>
        /// <see cref="PoEItemCategory"/>
        public Task<PoEItemCategory> GetJewelryListAsync() {
            return GetItemListAsync(PoESite.ItemDataJewelry, "Jewelry");
        }

        /// <summary>
        /// Asynchronously fetches a category of currency items from the Path of Exile website.
        /// </summary>
        /// <returns>A task that returns a category of currency items.</returns>
        /// <see cref="PoEItemCategory"/>
        public Task<PoEItemCategory> GetCurrencyListAsync() {
            return GetItemListAsync(PoESite.ItemDataCurrency, "Currency");
        }

        /// <summary>
        /// Asynchronously fetches a category of prefix mods from the Path of Exile website.
        /// </summary>
        /// <returns>A task that returns a category of item prefixes.</returns>
        /// <see cref="PoEItemModCategory"/>
        public Task<PoEItemModCategory> GetPrefixListAsync() {
            return GetModListAsync(PoESite.ItemDataPrefixes, "Prefixes");
        }

        /// <summary>
        /// Asynchronously fetches a category of suffix mods from the Path of Exile website.
        /// </summary>
        /// <returns>A task that returns a category of item suffixes.</returns>
        /// <see cref="PoEItemModCategory"/>
        public Task<PoEItemModCategory> GetSuffixListAsync() {
            return GetModListAsync(PoESite.ItemDataSuffixes, "Suffixes");
        }

        /// <summary>
        /// Asynchronously fetreives a PoEItemTable constructed from data on the Path of Exile website.
        /// </summary>
        /// <returns>A task that returns an item table that contains information about most items in the game.</returns>
        /// <see cref="PoEItemTable"/>
        public async Task<PoEItemTable> GetItemTable() {
            return new PoEItemTable(
                await GetWeaponListAsync(),
                await GetArmorListAsync(),
                await GetJewelryListAsync(),
                await GetCurrencyListAsync(),
                await GetPrefixListAsync(),
                await GetSuffixListAsync());
        }

        /// <summary>
        /// Asynchronously fetches the league list.
        /// </summary>
        /// <returns>A task that returns the list of leagues.</returns>
        /// <see cref="PoELeague"/>
        public async Task<IReadOnlyList<PoELeague>> GetLeagues() {
            var result = await _client.GetAsync(PoESite.Leagues);
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<PoELeague>>(json);
        }
    }
}
