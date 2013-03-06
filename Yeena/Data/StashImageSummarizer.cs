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
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using Yeena.PathOfExile;

namespace Yeena.Data {
    [SummarizerName("Image")]
    class StashImageSummarizer : IStashSummarizer {
        private readonly HttpClient _client;

        private readonly ImageCache _imageCache;

        private SolidBrush _background;
        public Color BackgroundColor {
            get { return _background.Color; }
            set { _background = new SolidBrush(value); }
        }

        private int _cellSize;
        public int CellSize {
            get { return _cellSize; }
            set {
                if (value <= 0) throw new ArgumentException("Value must be greater than zero.");
                _cellSize = value;
            }
        }

        [Browsable(false)]
        public int TabWidth { get { return PoEGame.StashWidth * CellSize; } }

        public StashImageSummarizer(ImageCache imgCache) {
            _imageCache = imgCache;

            _client = new HttpClient();

            BackgroundColor = Color.Black;
            CellSize = 32;
        }

        public void Summarize(string filename, PoEStash stash) {
            int imageWidth = PoEGame.StashWidth * CellSize * stash.Tabs.Count;
            int imageHeight = PoEGame.StashHeight * CellSize;
            var bitmap = new Bitmap(imageWidth, imageHeight);
            
            using (var g = Graphics.FromImage(bitmap)) {
                g.FillRectangle(_background, 0, 0, imageWidth, imageHeight);
                foreach (var tab in stash.Tabs) {
                    RenderTab(g, tab);
                }
            }

            bitmap.Save(filename);
        }

        private void RenderTab(Graphics g, PoEStashTab tab) {
            foreach (var item in tab) {
                RenderItem(g, item, tab.TabInfo.Index * TabWidth);
            }
        }

        private async void RenderItem(Graphics g, PoEItem item, int offsetX) {
            var itemImage = await _imageCache.GetAsync(_client, item.IconUri);
            g.DrawImage(itemImage, offsetX + item.X * CellSize, item.Y * CellSize, item.Width * CellSize, item.Height * CellSize);
        }
    }
}
