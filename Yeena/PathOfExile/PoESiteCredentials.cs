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

namespace Yeena.PathOfExile {
    /// <summary>
    /// Represents login information for the Path of Exile website.
    /// </summary>
    class PoESiteCredentials {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public PoESiteCredentials(string username, string password) {
            Username = username;
            Password = password;
        }
    }
}
