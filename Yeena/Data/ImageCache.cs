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
using System.Collections.Concurrent;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;

namespace Yeena.Data {
    // In an attempt to reduce the load on GGG's servers we'll cache images locally.
    class ImageCache : BinaryDiskCache<ConcurrentDictionary<Uri, Image>> {
        public ImageCache(string name) : base(name) {
        }

        private async Task<Image> DownloadImage(HttpClient client, Uri uri) {
            return Image.FromStream(await client.GetStreamAsync(uri));
        }

        public async Task<Image> GetAsync(HttpClient client, Uri uri) {
            Image img;
            if (Value.TryGetValue(uri, out img)) {
                return img;
            }

            img = await DownloadImage(client, uri);
            return Value.GetOrAdd(uri, _ => img);
        }
    }
}
