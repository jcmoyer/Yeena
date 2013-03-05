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
using System.Drawing;
using System.Net.Http;
using Yeena.PathOfExile;

namespace Yeena.Data {
    class StashImageSummarizer : IStashSummarizer {
        private readonly ImageCache _imageCache;

        public StashImageSummarizer(ImageCache imgCache) {
            _imageCache = imgCache;
        }

        public async void Summarize(string filename, PoEStash stash) {
            HttpClient cl = new HttpClient();

            int tabWidth = 12 * 32;
            int imageWidth = 12 * 32 * stash.Tabs.Count;
            int imageHeight = 12 * 32;
            var bitmap = new Bitmap(imageWidth, imageHeight);
            var g = Graphics.FromImage(bitmap);
            int t = 0;

            g.FillRectangle(Brushes.Black, 0, 0, imageWidth, imageHeight);

            foreach (var tab in stash.Tabs) {
                foreach (var item in tab) {
                    var itemImage = await _imageCache.GetAsync(cl, new Uri(PoESite.Uri, item.IconUrl));
                    g.DrawImage(itemImage, tabWidth * t + item.X * 32, item.Y * 32, item.Width * 32, item.Height * 32);
                }

                t++;
            }

            g.Dispose();
            bitmap.Save(filename);
        }


    }
}
