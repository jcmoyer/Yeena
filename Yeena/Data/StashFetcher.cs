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

    class StashFetcherServerErrorEventArgs : EventArgs {
        private readonly string _message;

        public string Message {
            get { return _message; }
        }

        public StashFetcherServerErrorEventArgs(string message) {
            _message = message;
        }
    }

    class StashFetcherServerErrorException : Exception {
        public StashFetcherServerErrorException(string message) : base(message) {
        }
    }

    class StashFetcher {
        private const int RetryCount = 10;
        private const int InitialThrottle = 3000;

        private readonly PoESiteClient _client;

        private Task _currentFetchTask;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler Begin;
        public event EventHandler End;
        public event EventHandler Canceled;
        public event EventHandler NoStashError;
        public event EventHandler<StashTabReceivedEventArgs> StashTabReceived;
        public event EventHandler<StashReceivedEventArgs> StashReceived;
        public event EventHandler<StashFetcherServerErrorEventArgs> ServerError;

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
            } catch (StashFetcherServerErrorException ex) {
                OnServerError(new StashFetcherServerErrorEventArgs(ex.Message));
            }

            OnEnd(new EventArgs());
        }

        private async Task GetLeagueStashAsync(string league, CancellationToken cancellationToken) {
            OnBegin(new EventArgs());
            
            PoEStashTab tab0;
            try {
                tab0 = await GetStashTabThrottledAsync(league, 0, InitialThrottle, RetryCount, cancellationToken);
            } catch (JsonSerializationException) {
                OnNoStashError(new EventArgs());
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            OnStashTabReceived(new StashTabReceivedEventArgs(tab0));

            var tabs = new List<PoEStashTab> { tab0 };
            for (int i = 1; i < tab0.TabCount; i++) {
                cancellationToken.ThrowIfCancellationRequested();

                var tabI = await GetStashTabThrottledAsync(league, i, InitialThrottle, RetryCount, cancellationToken);
                tabs.Add(tabI);

                cancellationToken.ThrowIfCancellationRequested();
                OnStashTabReceived(new StashTabReceivedEventArgs(tabI));
            }

            cancellationToken.ThrowIfCancellationRequested();
            OnStashReceived(new StashReceivedEventArgs(new PoEStash(tabs), league));
        }

        private async Task<PoEStashTab> GetStashTabThrottledAsync(string league, int page, int throttle, int numRetries, CancellationToken cancellationToken) {
            PoEStashTab tab;
            do {
                tab = await _client.GetStashTabAsync(league, page, cancellationToken);
                // Wait for a bit if there was an error, then try again.
                if (tab.Error != null) {
                    // Let clients know what is going on.
                    string message = String.Format("{0} Retrying in {1} seconds.", tab.Error.Message, Math.Round(throttle / 1000.0f, 1));
                    OnServerError(new StashFetcherServerErrorEventArgs(message));

                    await Task.Delay(throttle, cancellationToken);
                    throttle = (int)(throttle * 1.5);
                } else {
                    return tab;
                }
            } while (numRetries-- > 0);
            throw new StashFetcherServerErrorException("Cancelling due to numerous errors: " + tab.Error.Message);
        }

        protected virtual void OnBegin(EventArgs e) {
            var handler = Begin;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnEnd(EventArgs e) {
            var handler = End;
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

        protected virtual void OnServerError(StashFetcherServerErrorEventArgs e) {
            var handler = ServerError;
            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
