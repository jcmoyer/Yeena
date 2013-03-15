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
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using Yeena.PathOfExile;

namespace Yeena.Data {
    [SummarizerName("Image")]
    class StashImageSummarizer : IStashSummarizer, IDisposable {
        private readonly HttpClient _client;

        private readonly ImageCache _imageCache;

        private SolidBrush _background;
        public Color BackgroundColor {
            get { return _background.Color; }
            set { _background = new SolidBrush(value); }
        }

        private readonly Pen _gridPen = new Pen(Color.Black);
        public Color GridColor {
            get { return _gridPen.Color; }
            set { _gridPen.Color = value; }
        }
        public float GridWidth {
            get { return _gridPen.Width; }
            set { _gridPen.Width = value; }
        }

        private int _cellSize;
        public int CellSize {
            get { return _cellSize; }
            set {
                if (value <= 0) throw new ArgumentException("Value must be greater than zero.");
                _cellSize = value;
            }
        }

        private int TabWidth { get { return PoEGame.StashWidth * CellSize; } }
        public bool IncludeGrid { get; set; }

        public StashImageSummarizer(ImageCache imgCache) {
            _imageCache = imgCache;

            _client = new HttpClient();

            BackgroundColor = Color.DimGray;
            GridColor = Color.Black;
            IncludeGrid = true;
            CellSize = 32;
        }

        ~StashImageSummarizer() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                if (_client != null) {
                    _client.Dispose();
                }
                if (_background != null) {
                    _background.Dispose();
                }
            }
        }

        private ImageFormat PickImageFormat(string filename) {
            string ext = Path.GetExtension(filename);
            switch (ext.ToLowerInvariant()) {
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Gif;
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".bmp":
                    return ImageFormat.Bmp;
                default:
                    return ImageFormat.Png;
            }
        }

        public void Summarize(string filename, PoEStash stash) {
            int imageWidth = PoEGame.StashWidth * CellSize * stash.Tabs.Count;
            int imageHeight = PoEGame.StashHeight * CellSize;
            var imageFormat = PickImageFormat(filename);
            
            using (var bitmap = new Bitmap(imageWidth, imageHeight)) {
                using (var g = Graphics.FromImage(bitmap))
                    Render(g, stash, imageWidth, imageHeight);
                bitmap.Save(filename, imageFormat);
            }
        }

        private void Render(Graphics g, PoEStash stash, int imageWidth, int imageHeight) {
            g.FillRectangle(_background, 0, 0, imageWidth, imageHeight);
            if (IncludeGrid) {
                RenderGrid(g, stash.Tabs.Count);
            }
            foreach (var tab in stash.Tabs) {
                RenderTab(g, tab);
            }
        }

        private void RenderGrid(Graphics g, int tabCount) {
            for (int x = 0; x <= PoEGame.StashWidth * tabCount; x++) {
                g.DrawLine(_gridPen, x * CellSize, 0, x * CellSize, PoEGame.StashHeight * CellSize);
            }
            for (int y = 0; y <= PoEGame.StashHeight; y++) {
                g.DrawLine(_gridPen, 0, y * CellSize, PoEGame.StashWidth * tabCount * CellSize, y * CellSize);
            }
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
