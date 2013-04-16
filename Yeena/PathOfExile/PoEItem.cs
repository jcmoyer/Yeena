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

#pragma warning disable 649

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Yeena.PathOfExile {
    /// <summary>
    /// Represents an item in Path of Exile.
    /// </summary>
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
        [JsonProperty("support")]
        private readonly bool _support;
        [JsonProperty("league")]
        private readonly string _league;
        [JsonProperty("sockets")]
        private readonly List<PoEItemSocket> _sockets;
        [JsonProperty("socketedItems")]
        private readonly List<PoESocketedItem> _socketedItems;
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

        // Accessing properties is fairly expensive so retreival is memoized.
        private readonly Lazy<int> _quality;
        private readonly Lazy<string> _stackSize;

        /// <summary>
        /// Returns the zero-based distance from the left of this item's container in inventory or stash cells.
        /// </summary>
        public int X { get { return _x; } }

        /// <summary>
        /// Returns the zero-based distance from the top of this item's container in inventory or stash cells.
        /// </summary>
        public int Y { get { return _y; } }

        /// <summary>
        /// Returns the width of this item in inventory or stash cells.
        /// </summary>
        public int Width { get { return _width; } }

        /// <summary>
        /// Returns the height of this item in inventory or stash cells.
        /// </summary>
        public int Height { get { return _height; } }

        /// <summary>
        /// Returns whether or not this item has been verified.
        /// </summary>
        public bool IsVerified { get { return _verified; } }

        /// <summary>
        /// Returns whether or not this item is a support gem.
        /// </summary>
        public bool IsSupport { get { return _support; } }

        /// <summary>
        /// Returns the name of the league this item is in.
        /// </summary>
        public string League { get { return _league; } }

        /// <summary>
        /// Returns an identifier.
        /// </summary>
        public string InventoryId { get { return _inventoryId; } }

        /// <summary>
        /// Returns a fully qualified Uri that can be used to access the item's icon.
        /// </summary>
        public Uri IconUri {
            get { return new Uri(PoESite.Uri, _iconUrl); }
        }

        /// <summary>
        /// Returns the name of this item. This is only used for rare or unique items.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Returns the base name of this item. All items support this property.
        /// </summary>
        public string TypeLine { get { return _typeLine; } }

        /// <summary>
        /// Returns the identification status of this item. For Normal rarity items, this will always be false.
        /// </summary>
        public bool IsIdentified { get { return _identified; } }

        /// <summary>
        /// Returns the quality level of this item.
        /// </summary>
        public int Quality { get { return _quality.Value; } }

        /// <summary>
        /// Returns the stack size of this item. This is a string in the format X/Y.
        /// </summary>
        public string StackSize { get { return _stackSize.Value; } }

        /// <summary>
        /// Returns a list of item properties.
        /// </summary>
        /// <see cref="HasProperty"/>
        /// <see cref="GetProperty"/>
        public IReadOnlyList<PoEItemProperty> Properties {
            get { return _properties ?? new List<PoEItemProperty>(); }
        }

        /// <summary>
        /// Returns a list of item properties.
        /// </summary>
        /// <see cref="HasProperty"/>
        /// <see cref="GetProperty"/>
        public IReadOnlyList<PoEItemProperty> AdditionalProperties {
            get { return _additionalProperties ?? new List<PoEItemProperty>(); }
        }

        /// <summary>
        /// Returns a list of sockets in this item.
        /// </summary>
        /// <see cref="PoEItemSocket"/>
        public IReadOnlyList<PoEItemSocket> Sockets {
            get { return _sockets; }
        }

        /// <summary>
        /// Returns a list of items that are socketed into this item.
        /// </summary>
        /// <see cref="PoESocketedItem"/>
        public IReadOnlyList<PoESocketedItem> SocketedItems {
            get { return _socketedItems ?? new List<PoESocketedItem>(); }
        }

        /// <summary>
        /// Returns a sequence of socket groups. Each socket group is a sequence of PoEItemSockets.
        /// </summary>
        /// <see cref="PoEItemSocket"/>
        public IEnumerable<IEnumerable<PoEItemSocket>> SocketGroups {
            get {
                return from socket in _sockets
                       group socket by socket.Group
                       into linked
                       select linked;
            }
        }

        /// <summary>
        /// Returns a list of implicit mods. All items with the same base item type share the same kinds of implicit
        /// mods but with varying magnitudes.
        /// </summary>
        public IReadOnlyList<string> ImplicitMods {
            get { return _implicitMods ?? new List<string>(); }
        }

        /// <summary>
        /// Returns a list of explicit mods. This property is only used for non-Normal rarity items. The mods contained
        /// by this property vary by item.
        /// </summary>
        public IReadOnlyList<string> ExplicitMods {
            get { return _explicitMods ?? new List<string>(); }
        } 

        /// <summary>
        /// Returns the frame type of this item. This is used to determine item rarity.
        /// </summary>
        public PoEItemFrameType FrameType {
            get { return (PoEItemFrameType)_frameType; }
        }

        /// <summary>
        /// Returns the description for this item. This is typically instructions on how to use the item.
        /// </summary>
        public string Description {
            get { return _descriptionText ?? String.Empty; }
        }

        /// <summary>
        /// Returns the secondary description for this item. This is typically a description of what the item does.
        /// </summary>
        public string SecondaryDescription {
            get { return _secondaryDescriptionText ?? String.Empty; }
        }

        /// <summary>
        /// Returns the flavor text for this item. This property is only available for Unique rarity items.
        /// </summary>
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
        protected PoEItem() {
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

        /// <summary>
        /// Searches this item's properties for one with a specific name and returns it if it exists.
        /// </summary>
        /// <param name="name">Name of the property to find.</param>
        /// <returns>The property if it exists, or null if it does not exist.</returns>
        public PoEItemProperty GetProperty(string name) {
            var result = Properties.FirstOrDefault(prop => prop.Name == name) ??
                AdditionalProperties.FirstOrDefault(prop => prop.Name == name);
            return result;
        }

        /// <summary>
        /// Determines whether or not a property exists.
        /// </summary>
        /// <param name="name">Name of the property to find.</param>
        /// <returns>True if the property exists, or false if it does not exist.</returns>
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

        /// <summary>
        /// Returns a string representation of this item.
        /// </summary>
        /// <returns>A string representation of this item.</returns>
        public override string ToString() {
            if (FrameType == PoEItemFrameType.Rare || FrameType == PoEItemFrameType.Unique) return Name;
            return TypeLine;
        }
    }
}