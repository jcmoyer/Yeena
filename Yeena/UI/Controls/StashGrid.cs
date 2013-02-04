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
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using Yeena.PathOfExile;

namespace Yeena.UI.Controls {
    partial class StashGrid : UserControl {
        private const float HorizontalCellCount = 12;
        private const float VerticalCellCount = 12;

        private List<PoEItem> _dataSrc = new List<PoEItem>();
        private readonly Dictionary<PoEItem, Image> _images = new Dictionary<PoEItem, Image>();

        private readonly List<StashGridMarking> _markings = new List<StashGridMarking>();

        private float _xCellSize;
        private float _yCellSize;

        private int _drawHeight;
        private int _drawWidth;

        private int _drawOffsetX;
        private int _drawOffsetY;

        public PoEItem HoveredItem { get; private set; }

        private readonly ItemInfoPopup _itemInfoPopup = new ItemInfoPopup();
        private StashGridMarking _mousedMarking;

        public StashGrid() {
            InitializeComponent();
        }

        public async void SetImages(IEnumerable<PoEItem> items) {
            _dataSrc = items.ToList();

            HttpClient client = new HttpClient();
            foreach (var item in _dataSrc) {
                _images[item] = await ImageCache.GetAsync(client, new Uri(PoESite.Uri, item.IconUrl));

                //Image.FromStream(await client.GetStreamAsync(new Uri(PoESite.Uri, item.IconUrl)));
                Invalidate(CalculateItemRect(item));
            }
        }

        private Rectangle CalculateItemRect(PoEItem item) {
            int left = (int)(_drawOffsetX + item.X * _xCellSize);
            int top = (int)(_drawOffsetY + item.Y * _yCellSize);
            int width = (int)(item.Width * _xCellSize);
            int height = (int)(item.Height * _yCellSize);
            return new Rectangle(left, top, width, height);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            // Translate mouse position to grid coordinates
            int cellX = (int)((e.X - _drawOffsetX) / _xCellSize);
            int cellY = (int)((e.Y - _drawOffsetY) / _yCellSize);

            HoveredItem = null;
            foreach (var item in _dataSrc) {
                if (cellX >= item.X && cellX < item.X + item.Width &&
                    cellY >= item.Y && cellY < item.Y + item.Height) {
                    HoveredItem = item;
                    _itemInfoPopup.Item = item;

                    if (_mousedMarking != null) RemoveMarking(_mousedMarking);
                    _mousedMarking = new StashGridMarking(HoveredItem, Brushes.SteelBlue);
                    AddMarking(_mousedMarking);

                    break;
                }
            }

            if (HoveredItem != null) {
                var where = PointToScreen(e.Location);
                where.Offset(16, 16);
                _itemInfoPopup.Location = where;
                if (!_itemInfoPopup.Visible) {
                    _itemInfoPopup.Show(this);
                }
            } else {
                RemoveMarking(_mousedMarking);
                if (_itemInfoPopup.Visible) {
                    _itemInfoPopup.Hide();
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            _itemInfoPopup.Hide();
            if (_mousedMarking != null) RemoveMarking(_mousedMarking);

            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e) {
            //Rectangle drawRect = new Rectangle(_drawOffsetX, _drawOffsetY, _drawWidth, _drawHeight);

            e.Graphics.FillRectangle(Brushes.DimGray, ClientRectangle);

            foreach (var marking in _markings) {
                e.Graphics.FillRectangle(
                    marking.Brush,
                    CalculateItemRect(marking.Item));
            }

            // Draw grids
            for (int x = 0; x < HorizontalCellCount + 1; x++) {
                e.Graphics.DrawLine(
                    Pens.Black,
                    _drawOffsetX + x * _xCellSize,
                    _drawOffsetY,
                    _drawOffsetX + x * _xCellSize,
                    _drawOffsetY + _drawHeight);
                for (int y = 0; y < VerticalCellCount + 1; y++) {
                    e.Graphics.DrawLine(
                        Pens.Black,
                        _drawOffsetX,
                        _drawOffsetY + y * _yCellSize,
                        _drawOffsetX + _drawWidth,
                        _drawOffsetY + y * _yCellSize);
                }
            }

            // Draw items
            foreach (var item in _images) {
                float left = _drawOffsetX + item.Key.X * _xCellSize;
                float top = _drawOffsetY + item.Key.Y * _yCellSize;
                float width = item.Key.Width * _xCellSize;
                float height = item.Key.Height * _yCellSize;
                e.Graphics.DrawImage(item.Value, left, top, width, height);
            }

            base.OnPaint(e);
        }

        public void AddMarking(StashGridMarking marking) {
            _markings.Add(marking);
            Invalidate(CalculateItemRect(marking.Item));
        }

        public bool RemoveMarking(StashGridMarking marking) {
            bool result = _markings.Remove(marking);
            Invalidate(CalculateItemRect(marking.Item));
            return result;
        }

        public void MarkItem(PoEItem item) {
            _markings.Add(new StashGridMarking(item));
            Invalidate(CalculateItemRect(item));
        }

        public void ClearMarkings() {
            _markings.Clear();
            Invalidate();
        }

        public bool ContainsItem(PoEItem item) {
            return _dataSrc.Contains(item);
        }

        private void StashGrid_Resize(object sender, EventArgs e) {
            // We'll scale the stash grid to prevent distortion.
            float smallestDimension = Math.Min(ClientSize.Width, ClientSize.Height);
            float scaleFactorX = ClientSize.Width * (smallestDimension / ClientSize.Width);
            float scaleFactorY = ClientSize.Height * (smallestDimension / ClientSize.Height);

            _drawWidth = (int)scaleFactorX;
            _drawHeight = (int)scaleFactorY;

            _drawOffsetX = (int)(ClientSize.Width / 2.0f - _drawWidth / 2.0f);
            _drawOffsetY = (int)(ClientSize.Height / 2.0f - _drawHeight / 2.0f);

            _xCellSize = _drawWidth / HorizontalCellCount;
            _yCellSize = _drawHeight / VerticalCellCount;
        }
    }
}
