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
using System.Threading.Tasks;

namespace Yeena.Data {
    abstract class PersistentCache<T> where T : class {
        protected string Name { get; private set; }
        public T Value { get; protected set; }

        protected PersistentCache(string name) {
            Name = name;
        }

        public T Load(Func<T> factory) {
            OnLoad();
            return Value ?? (Value = factory());
        }

        public async Task<T> LoadAsync(Func<Task<T>> factory) {
            OnLoad();
            return Value ?? (Value = await factory());
        }

        public void Save() {
            OnSave();
        }

        protected abstract void OnLoad();
        protected abstract void OnSave();
    }
}
