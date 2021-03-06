# xmp-sharp

xmp-sharp is a wrapper for Adobe's [XMP Toolkit SDK](http://www.adobe.com/devnet/xmp.html) and will allow you to add, modify and/or delete XMP using any .NET 2.0 Application (ie. C#, VB.NET, etc.).
Currently, the antique version 4.1.1 of Adobe's SDK is wrapped.

## Building xmp-sharp
The following instructions have been tested with VS 2012. Building xmp-sharp is a four-step process:
* Compile Adobe's XMP Toolkit SDK  
Open `XMP-SDK\build\vsnet\XMPToolkit.sln` and compile the desired platform (e.g. Release x64).
* Copy the generated .lib files (e.g. `XMPCoreStaticRelease64.lib` and `XMPFilesStaticRelease64.lib` from `XMP-SDK\public\libraries\windows_x64\release\`) to `SE.Halligang\XmpToolkit\lib\`.
* Compile the managed XMP Toolkit wrapper  
Open `SE.Halligang\XmpToolkit\XmpToolkit.sln` and compile the same platform as before.
* Compile the C# XMP Toolkit  
Open `SE.Halligang\CsXmpToolkit\CsXmpToolkit.sln` and compile the same platform as before.

## Using xmp-sharp
If you want to use xmp-sharp in your application you must add a reference to `CsXmpToolkit.dll` and copy the correct version of `XmpToolkit.dll` (e.g. from `SE.Halligang\XmpToolkit\x64\Release\`) to the path (e.g. your application's working directory).

## License
Copyright 2007-2013 Martin Sanneblad  
Copyright 2016 Hannes Hofmann

xmp-sharp is licensed under the 3-clause BSD license.
It is based on code from C# XMP Toolkit made available under the 3-clause BSD license.
It includes Adobe's XMP Toolkit SDK made available under the 3-clause BSD license.

xmp-sharp is a fork of [C# XMP Toolkit](https://sourceforge.net/projects/csxmptk/), because
* The original project has not been updated since 2013-04-24.
* It is impossible to trace and contact the author of the original project.

## Alternatives
* [xmp-core-dotnet](https://github.com/drewnoakes/xmp-core-dotnet): Port of Adobe's SDK version 5.1.2 to managed C#. It lacks the schema classes and the XMPFiles project.
