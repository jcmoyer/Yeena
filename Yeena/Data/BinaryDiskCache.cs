﻿// Copyright 2013 J.C. Moyer
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
using System.Runtime.Serialization.Formatters.Binary;

namespace Yeena.Data {
    class BinaryDiskCache<T> : PersistentCache<T> where T : class {
        protected string FileName { get { return Storage.ResolvePath(Name + ".bin"); } }

        private readonly BinaryFormatter _formatter;
        
        public BinaryDiskCache(string name)
            : base(name) {
            _formatter = new BinaryFormatter();
        }

        protected override void OnLoad() {
            if (File.Exists(FileName)) {
                using (var sr = File.OpenRead(FileName))
                    Value = (T)_formatter.Deserialize(sr);
            }
        }

        protected override void OnSave() {
            using (var sw = File.OpenWrite(FileName))
                _formatter.Serialize(sw, Value);
        }

        public static implicit operator T(BinaryDiskCache<T> instance) {
            return instance.Value;
        }
    }
}