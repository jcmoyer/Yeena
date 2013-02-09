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
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Yeena {
    // In an attempt to reduce the load on GGG's servers we'll cache images locally.
    class ImageCache {
        private const string CacheFileName = "ImageCache.dat";

        private static ConcurrentDictionary<Uri, Image> _images = new ConcurrentDictionary<Uri, Image>();

        private static readonly string CacheFile = Storage.ResolvePath(CacheFileName);

        static ImageCache() {
            if (File.Exists(CacheFile)) {
                Load();
            }
        }

        private static async Task<Image> DownloadImage(HttpClient client, Uri uri) {
            return Image.FromStream(await client.GetStreamAsync(uri));
        }

        public static async Task<Image> GetAsync(HttpClient client, Uri uri) {
            Image img;
            if (_images.TryGetValue(uri, out img)) {
                return img;
            }

            Image i = await DownloadImage(client, uri);
            return _images.GetOrAdd(uri, _ => i);
        }

        public static void Save() {
            BinaryFormatter f = new BinaryFormatter();
            using (var fs = File.OpenWrite(CacheFile))
                f.Serialize(fs, _images);
        }
        public static void Load() {
            BinaryFormatter f = new BinaryFormatter();
            using (var fs = File.OpenRead(CacheFile))
                _images = (ConcurrentDictionary<Uri, Image>)f.Deserialize(fs);
        }

    }
}
