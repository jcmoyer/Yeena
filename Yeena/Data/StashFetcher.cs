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
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yeena.PathOfExile;

namespace Yeena.Data {
    class StashTabReceivedEventArgs : EventArgs {
        private readonly PoEStashTab _stashTab;

        public PoEStashTab StashTab {
            get { return _stashTab; }
        }

        public StashTabReceivedEventArgs(PoEStashTab stashTab) {
            _stashTab = stashTab;
        }
    }

    class StashReceivedEventArgs : EventArgs {
        private readonly PoEStash _stash;
        private readonly string _league;

        public PoEStash Stash {
            get { return _stash; }
        }

        public string League {
            get { return _league; }
        }

        public StashReceivedEventArgs(PoEStash stash, string league) {
            _stash = stash;
            _league = league;
        }
    }

    class StashFetcher {
        private readonly PoESiteClient _client;

        private Task _currentFetchTask;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler Begin;
        public event EventHandler Canceled;
        public event EventHandler NoStashError;
        public event EventHandler<StashTabReceivedEventArgs> StashTabReceived;
        public event EventHandler<StashReceivedEventArgs> StashReceived;

        public StashFetcher(PoESiteClient client) {
            _client = client;
        }

        public async Task FetchAsync(string league) {
            if (_currentFetchTask != null && !_currentFetchTask.IsCompleted) {
                _cancellationTokenSource.Cancel();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _currentFetchTask = GetLeagueStashAsync(league, _cancellationTokenSource.Token);
            try {
                await _currentFetchTask;
            } catch (OperationCanceledException) {
                OnCanceled(new EventArgs());
            }
        }

        private async Task GetLeagueStashAsync(string league, CancellationToken cancellationToken) {
            OnBegin(new EventArgs());
            
            PoEStashTab tab0;
            try {
                tab0 = await _client.GetStashTabAsync(league, 0, 2500, cancellationToken);
            } catch (JsonSerializationException) {
                OnNoStashError(new EventArgs());
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            OnStashTabReceived(new StashTabReceivedEventArgs(tab0));

            var tabs = new List<PoEStashTab> { tab0 };
            for (int i = 1; i < tab0.TabCount; i++) {
                cancellationToken.ThrowIfCancellationRequested();

                var tabI = await _client.GetStashTabAsync(league, i, 2500, cancellationToken);
                tabs.Add(tabI);

                cancellationToken.ThrowIfCancellationRequested();
                OnStashTabReceived(new StashTabReceivedEventArgs(tabI));
            }

            cancellationToken.ThrowIfCancellationRequested();
            OnStashReceived(new StashReceivedEventArgs(new PoEStash(tabs), league));
        }

        protected virtual void OnBegin(EventArgs e) {
            var handler = Begin;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnCanceled(EventArgs e) {
            var handler = Canceled;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnNoStashError(EventArgs e) {
            var handler = NoStashError;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnStashTabReceived(StashTabReceivedEventArgs e) {
            var handler = StashTabReceived;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnStashReceived(StashReceivedEventArgs e) {
            var handler = StashReceived;
            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
