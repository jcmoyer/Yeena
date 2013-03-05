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
        private readonly HttpClient _client;

        private readonly int _tabWidth;

        private readonly ImageCache _imageCache;

        public StashImageSummarizer(ImageCache imgCache) {
            _imageCache = imgCache;

            _client = new HttpClient();
            
            // TODO: Make 32 parameterizable here and below
            _tabWidth = PoEGame.StashWidth * 32;
        }

        public void Summarize(string filename, PoEStash stash) {
            int imageWidth = PoEGame.StashWidth * 32 * stash.Tabs.Count;
            int imageHeight = PoEGame.StashHeight * 32;
            var bitmap = new Bitmap(imageWidth, imageHeight);
            
            using (var g = Graphics.FromImage(bitmap)) {
                g.FillRectangle(Brushes.Black, 0, 0, imageWidth, imageHeight);
                foreach (var tab in stash.Tabs) {
                    RenderTab(g, tab);
                }
            }

            bitmap.Save(filename);
        }

        private void RenderTab(Graphics g, PoEStashTab tab) {
            foreach (var item in tab) {
                RenderItem(g, item, tab.TabInfo.Index * _tabWidth);
            }
        }

        private async void RenderItem(Graphics g, PoEItem item, int offsetX) {
            var itemImage = await _imageCache.GetAsync(_client, new Uri(PoESite.Uri, item.IconUrl));
            g.DrawImage(itemImage, offsetX + item.X * 32, item.Y * 32, item.Width * 32, item.Height * 32);
        }
    }
}
