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

        public bool IncludeProperties { get; set; }
        public bool IncludeImplicitMods { get; set; }
        public bool IncludeExplicitMods { get; set; }

        public StashTextSummarizer(PoEItemTable itemTable) {
            _itemTable = itemTable;

            EmptyLinesBetweenItems = 1;
            IndentSize = 2;
        }

        public void Summarize(string filename, PoEStash stash) {
            using (var fs = File.OpenWrite(filename))
            using (var sw = new StreamWriter(fs)) {
                foreach (var tab in stash.Tabs) {
                    WriteTab(sw, tab);
                    sw.WriteLine();
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
                if (item.IsRare) {
                    WriteRare(writer, item);
                } else if (!String.IsNullOrEmpty(item.StackSize) && !IncludeProperties) {
                    WriteStack(writer, item);
                } else {
                    WriteSimple(writer, item);
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

            writer.WriteLine();
        }

        private void WriteRare(TextWriter writer, PoEItem item) {
            writer.WriteLine("{0}, {1}", item.RareName, item.TypeLine);
        }

        private void WriteStack(TextWriter writer, PoEItem item) {
            writer.WriteLine("{0} ({1})", item.TypeLine, item.StackSize);
        }

        private void WriteSimple(TextWriter writer, PoEItem item) {
            writer.WriteLine(item.TypeLine);
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
            writer.Write(new String(' ', IndentSize));
            writer.WriteLine(property.DisplayText);
        }

        private void WriteMods(TextWriter writer, IEnumerable<string> mods) {
            foreach (var mod in mods) {
                WriteMod(writer, mod);
            }
        }

        private void WriteMod(TextWriter writer, string mod) {
            writer.Write(new String(' ', IndentSize));
            writer.WriteLine(mod);
        }

        private void WriteEmptyLines(TextWriter writer, int count) {
            for (int i = 0; i < count; i++) {
                writer.WriteLine();
            }
        }
    }
}
