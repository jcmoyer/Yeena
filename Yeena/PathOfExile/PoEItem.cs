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
using System.Linq;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    [JsonObject]
    public class PoEItem {
        [JsonProperty("verified")]
        private readonly bool _verified;
        [JsonProperty("w")]
        private readonly int _width;
        [JsonProperty("h")]
        private readonly int _height;
        [JsonProperty("icon")]
        private readonly string _iconUrl;
        // no idea what this is for
        [JsonProperty("support")]
        private readonly bool _support;
        [JsonProperty("league")]
        private readonly string _league;
        // TODO: sockets
        // TODO: socketedItems
        // seems to only be used for named magics/rares?
        [JsonProperty("name")]
        private readonly string _name;
        [JsonProperty("typeLine")]
        private readonly string _typeLine;

        [JsonProperty("properties")]
        private readonly List<PoEItemProperty> _properties;
        [JsonProperty("additionalProperties")]
        private readonly List<PoEItemProperty> _additionalProperties;

        [JsonProperty("implicitMods")]
        private readonly List<string> _implicitMods;
        [JsonProperty("explicitMods")]
        private readonly List<string> _explicitMods;

        [JsonProperty("secDescrText")]
        private readonly string _secondaryDescriptionText;
        [JsonProperty("descrText")]
        private readonly string _descriptionText;

        [JsonProperty("frameType")]
        private readonly int _frameType;

        [JsonProperty("x")]
        private readonly int _x;
        [JsonProperty("y")]
        private readonly int _y;

        [JsonProperty("inventoryId")]
        private readonly string _inventoryId;

        [JsonProperty("identified")]
        private readonly bool _identified;

        public int X { get { return _x; } }
        public int Y { get { return _y; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public string IconUrl { get { return _iconUrl; } }

        public string RareName { get { return _name; } }
        public string TypeLine { get { return _typeLine; } }

        public bool IsRare {
            get { return !String.IsNullOrEmpty(RareName); }
        }

        public bool IsIdentified {
            get { return _identified; }
        }

        // Quality is embeded within a string within an array...thus retreival is memoized
        private int _cacheQual = -1;
        public int Quality {
            get {
                if (_cacheQual >= 0) return _cacheQual;

                if (_properties == null) return (_cacheQual = 0);
                if (_properties.Count == 0) return (_cacheQual = 0);

                var qualityProp = _properties.FirstOrDefault(p => p.Name == "Quality");
                if (qualityProp == null) return (_cacheQual = 0);
                var qualityStr = qualityProp.Values[0][0] as string;
                if (qualityStr == null) return (_cacheQual = 0);
                return (_cacheQual = Int32.Parse(qualityStr.Substring(1, qualityStr.Length - 2)));
            }
        }

        // Memoized
        private string _stackSizeStr;
        public string StackSize {
            get {
                if (_stackSizeStr != null) return _stackSizeStr;

                if (_properties == null) return (_stackSizeStr = String.Empty);
                if (_properties.Count == 0) return (_stackSizeStr = String.Empty);

                var stackProp = _properties.FirstOrDefault(p => p.Name == "Stack Size");
                if (stackProp == null) return (_stackSizeStr = String.Empty);
                var stackVal = stackProp.Values[0][0] as string;
                if (stackVal == null) return (_stackSizeStr = String.Empty);
                return (_stackSizeStr = stackVal);
            }
        }

        public IReadOnlyList<PoEItemProperty> Properties {
            get { return _properties; }
        }

        public IReadOnlyList<PoEItemProperty> AdditionalProperties {
            get { return _additionalProperties; }
        }

        public override string ToString() {
            if (IsRare) return RareName;
            return TypeLine;
        }
    }
}