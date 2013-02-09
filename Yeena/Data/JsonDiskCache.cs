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
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Yeena.Data {
    class JsonDiskCache<T> : PersistentCache<T> where T : class {
        protected string Filename { get { return Storage.ResolvePath(Name + ".json"); } }

        private readonly JsonSerializer _serializer;
        private readonly DefaultContractResolver _resolver;

        public JsonDiskCache(string name) : base(name) {
            _serializer = new JsonSerializer();
            _resolver = new DefaultContractResolver {
                DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField,
            };
            _serializer.ContractResolver = _resolver;
        }

        protected override void OnLoad() {
            if (File.Exists(Filename)) {
                using (var sr = new StreamReader(Filename))
                using (var jr = new JsonTextReader(sr))
                    Value = _serializer.Deserialize<T>(jr);
            }
        }

        protected override void OnSave() {
            using (var sw = new StreamWriter(Filename))
            using (var jw = new JsonTextWriter(sw))
                _serializer.Serialize(jw, Value);
        }

        public static implicit operator T(JsonDiskCache<T> instance) {
            return instance.Value;
        } 
    }
}