# Yeena

Yeena is a stash evaluation tool for Path of Exile named in the vein of the trusty old ATMA V tool for Diablo 2. It allows you to quickly find vendor recipes by connecting to the Path of Exile website and retreiving your stash.

Yeena is safe to use - your account password is never stored locally, and your credentials are transmitted to the Path of Exile site through https. Browse the source code and see for yourself.

A word of warning: this program was quickly thrown together for the purpose of finding whetstone and armorer's scrap ingredients. For this reason, there are a lot of messy parts that will eventually be cleaned up.

## Precompiled Binaries

You can download a precompiled version of Yeena [here](http://jcmoyer.github.com/Yeena/). 

## Compilation

### Windows (Visual Studio 2012)

First, clone the project. In the same directory as Yeena.sln, create a folder named `Libraries`. External dependencies will go in this directory. Go [here](http://json.codeplex.com/) and download Json.NET and extract it somewhere, then navigate to the `Bin/Net40` directory and copy `Newtonsoft.Json.dll` to the newly created `Libraries` directory. Do the same thing with HtmlAgilityPack from [here](http://htmlagilitypack.codeplex.com/). Once this is done you can open up the Yeena solution file in Visual Studio 2012 and compile it.

### Windows (Mono)

If you are using a version of Windows that is not supported by Microsoft's .NET Framework 4.5, this is the preferred method. This has been tested with Mono 3.0.3. 

After cloning the project, create a folder named `Libraries`. Download [Json.NET](http://json.codeplex.com/) and [HtmlAgilityPack](http://htmlagilitypack.codeplex.com/). Place the .NET 4.x binaries in this directory. From the command line, run `xbuild` in the same directory as Yeena.sln.

### Linux (Mono)

This has been tested on Arch Linux with Mono 3.0.6. Please follow the same instructions for compiling with Mono for Windows.

## License

    Copyright 2013 J.C. Moyer
    
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
    
        http://www.apache.org/licenses/LICENSE-2.0
    
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
