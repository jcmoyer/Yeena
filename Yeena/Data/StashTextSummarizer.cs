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
using System.Text;
using Yeena.PathOfExile;

namespace Yeena.Data {
    [SummarizerName("Text")]
    class StashTextSummarizer : IStashSummarizer {
        private readonly PoEItemTable _itemTable;

        private int _indentSize;
        public int IndentSize {
            get { return _indentSize; }
            set {
                if (_indentSize < 0) throw new ArgumentException("Indent size must be greater than or equal to zero.");
                _indentSize = value;
            }
        }

        private int _emptyLinesBetweenItems;
        public int EmptyLinesBetweenItems {
            get { return _emptyLinesBetweenItems; }
            set {
                if (_emptyLinesBetweenItems < 0) throw new ArgumentException("Empty line count must be greater than or equal to zero.");
                _emptyLinesBetweenItems = value;
            }
        }

        private int _emptyLinesBetweenTabs;
        public int EmptyLinesBetweenTabs {
            get { return _emptyLinesBetweenTabs; }
            set {
                if (_emptyLinesBetweenTabs < 0) throw new ArgumentException("Empty line count must be greater than or equal to zero.");
                _emptyLinesBetweenTabs = value;
            }
        }

        public bool IncludeSockets { get; set; }
        public bool IncludeProperties { get; set; }
        public bool IncludeImplicitMods { get; set; }
        public bool IncludeExplicitMods { get; set; }
        public bool IncludeDescriptions { get; set; }
        public bool IncludeFlavorText { get; set; }

        public StashTextSummarizer(PoEItemTable itemTable) {
            _itemTable = itemTable;

            EmptyLinesBetweenTabs = 2;
            EmptyLinesBetweenItems = 1;
            IndentSize = 2;

            IncludeSockets = true;
        }

        public void Summarize(string filename, PoEStash stash) {
            using (var fs = File.Open(filename, FileMode.Create))
            using (var sw = new StreamWriter(fs)) {
                foreach (var tab in stash.Tabs) {
                    WriteTab(sw, tab);
                }
            }
        }

        private void WriteTabName(TextWriter writer, PoEStashTab tab) {
            WriteSeparator(writer);
            writer.WriteLine("== Tab {0}", tab.TabInfo.Name);
            WriteSeparator(writer);
        }

        private void WriteSeparator(TextWriter writer) {
            writer.WriteLine(new String('=', 80));
        }

        private void WriteTab(TextWriter writer, PoEStashTab tab) {
            WriteTabName(writer, tab);

            foreach (var item in tab) {
                if (item.FrameType == PoEItemFrameType.Rare || item.FrameType == PoEItemFrameType.Unique) {
                    WriteNamedItem(writer, item);
                } else if (!String.IsNullOrEmpty(item.StackSize) && !IncludeProperties) {
                    WriteStack(writer, item);
                } else {
                    WriteSimple(writer, item);
                }

                if (IncludeFlavorText) {
                    WriteFlavorText(writer, item);
                }
                if (IncludeDescriptions) {
                    WriteDescription(writer, item);
                }
                if (IncludeSockets && item.SocketGroups.Any()) {
                    WriteSocketLine(writer, item);
                }
                if (IncludeProperties) {
                    WriteProperties(writer, item);
                }
                if (IncludeImplicitMods) {
                    WriteMods(writer, item.ImplicitMods);
                }
                if (IncludeExplicitMods) {
                    WriteMods(writer, item.ExplicitMods);
                }
                
                WriteEmptyLines(writer, _emptyLinesBetweenItems);
            }

            WriteEmptyLines(writer, _emptyLinesBetweenTabs);
        }

        private void WriteNamedItem(TextWriter writer, PoEItem item) {
            writer.WriteLine("{0}, {1}", item.Name, item.TypeLine);
        }

        private void WriteStack(TextWriter writer, PoEItem item) {
            writer.WriteLine("{0} ({1})", item.TypeLine, item.StackSize);
        }

        private void WriteSimple(TextWriter writer, PoEItem item) {
            writer.Write(item.TypeLine);
            if (!item.IsIdentified && item.FrameType != PoEItemFrameType.Normal) {
                string itemRarity = Enum.GetName(typeof(PoEItemFrameType), item.FrameType);
                writer.WriteLine(" ({0}, unidentified)", itemRarity);
            } else {
                writer.WriteLine();
            }
        }

        private void WriteProperties(TextWriter writer, PoEItem item) {
            foreach (var prop in item.Properties) {
                WriteProperty(writer, prop);
            }
            foreach (var prop in item.AdditionalProperties) {
                WriteProperty(writer, prop);
            }
        }

        private void WriteProperty(TextWriter writer, PoEItemProperty property) {
            WriteIndent(writer);
            writer.WriteLine(property.DisplayText);
        }

        private void WriteMods(TextWriter writer, IEnumerable<string> mods) {
            foreach (var mod in mods) {
                WriteMod(writer, mod);
            }
        }

        private void WriteMod(TextWriter writer, string mod) {
            WriteIndent(writer);
            writer.WriteLine(mod);
        }

        private void WriteEmptyLines(TextWriter writer, int count) {
            for (int i = 0; i < count; i++) {
                writer.WriteLine();
            }
        }

        private string SanitizeText(string text) {
            return text.Replace('\n', ' ');
        }

        private void WriteFlavorText(TextWriter writer, PoEItem item) {
            if (item.FlavorText.Length > 0) {
                WriteIndent(writer);
                writer.WriteLine(item.FlavorText);
            }
        }

        private void WriteDescription(TextWriter writer, PoEItem item) {
            if (item.Description.Length > 0) {
                WriteIndent(writer);
                writer.WriteLine(SanitizeText(item.Description));
            }
            if (item.SecondaryDescription.Length > 0) {
                WriteIndent(writer);
                writer.WriteLine(SanitizeText(item.SecondaryDescription));
            }
        }

        private void WriteSocketLine(TextWriter writer, PoEItem item) {
            WriteIndent(writer);
            writer.WriteLine("Sockets: {0}", BuildSocketString(item));
        }

        private string BuildSocketString(PoEItem item) {
            var builder = new StringBuilder();
            foreach (var group in item.SocketGroups) {
                builder.Append(String.Join("-", group.Select(s => s.Color.GetShortName())));
                builder.Append(' ');
            }
            return builder.ToString().TrimEnd();
        }

        private void WriteIndent(TextWriter writer) {
            writer.Write(new String(' ', IndentSize));
        }
    }
}
