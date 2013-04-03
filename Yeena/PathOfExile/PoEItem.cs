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
    class PoEItem {
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
        [JsonProperty("sockets")]
        private readonly List<PoEItemSocket> _sockets;
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

        [JsonProperty("flavourText")]
        private readonly List<string> _flavorText;

        public int X { get { return _x; } }
        public int Y { get { return _y; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        /// <summary>
        /// Returns a fully qualified Uri that can be used to access the item's icon.
        /// </summary>
        public Uri IconUri {
            get { return new Uri(PoESite.Uri, _iconUrl); }
        }

        public string Name { get { return _name; } }
        public string TypeLine { get { return _typeLine; } }

        public bool IsIdentified {
            get { return _identified; }
        }

        // Accessing properties is fairly expensive so retreival is memoized.
        private readonly Lazy<int> _quality; 
        public int Quality {
            get { return _quality.Value; }
        }

        private readonly Lazy<string> _stackSize;
        public string StackSize {
            get { return _stackSize.Value; }
        }

        public IReadOnlyList<PoEItemProperty> Properties {
            get { return _properties ?? new List<PoEItemProperty>(); }
        }

        public IReadOnlyList<PoEItemProperty> AdditionalProperties {
            get { return _additionalProperties ?? new List<PoEItemProperty>(); }
        }

        public IReadOnlyList<PoEItemSocket> Sockets {
            get { return _sockets; }
        } 

        public override string ToString() {
            if (FrameType == PoEItemFrameType.Rare || FrameType == PoEItemFrameType.Unique) return Name;
            return TypeLine;
        }

        public IEnumerable<IEnumerable<PoEItemSocket>> SocketGroups {
            get {
                return from socket in _sockets
                       group socket by socket.Group
                       into linked
                       select linked;
            }
        }

        public IReadOnlyCollection<string> ImplicitMods {
            get { return _implicitMods ?? new List<string>(); }
        }

        public IReadOnlyCollection<string> ExplicitMods {
            get { return _explicitMods ?? new List<string>(); }
        } 

        public PoEItemFrameType FrameType {
            get { return (PoEItemFrameType)_frameType; }
        }

        public string Description {
            get { return _descriptionText ?? String.Empty; }
        }

        public string SecondaryDescription {
            get { return _secondaryDescriptionText ?? String.Empty; }
        }

        public string FlavorText {
            get {
                if (_flavorText != null) {
                    return String.Join(String.Empty, _flavorText);
                } else {
                    return String.Empty;
                }
            }
        }

        [JsonConstructor]
        private PoEItem() {
            Func<object, int> qualityConverter = value => {
                // Quality strings are in the format +XX%.
                // We need to chop off the first and last characters
                // and turn the enclosed number into an integer.
                string s = value.ToString();
                return Int32.Parse(s.Substring(1, s.Length - 2));
            };

            _quality = new Lazy<int>(() => TryGetProperty("Quality", qualityConverter, 0));
            _stackSize = new Lazy<string>(() => TryGetProperty("Stack Size", Convert.ToString, String.Empty));
        }

        public PoEItemProperty GetProperty(string name) {
            var result = Properties.FirstOrDefault(prop => prop.Name == name) ??
                AdditionalProperties.FirstOrDefault(prop => prop.Name == name);
            return result;
        }

        public bool HasProperty(string name) {
            bool result = false;
            if (_properties != null) {
                result |= Properties.Any(prop => prop.Name == name);
            }
            if (_additionalProperties != null) {
                result |= AdditionalProperties.Any(prop => prop.Name == name);
            }
            return result;
        }

        private T TryGetProperty<T>(string name, Func<object, T> converter, T defaultValue) {
            if (HasProperty(name)) {
                var prop = GetProperty(name);
                return converter(prop.Values[0][0]);
            } else {
                return defaultValue;
            }
        }
    }
}