# AAMotion

**AAMotion** is a C# .NET API for advanced motion controller communication and control, designed for high-performance and flexible integration with industrial motion systems.

## Overview

AAMotion provides a comprehensive interface for connecting to, configuring, and controlling a wide range of motion controllers. The API supports both high-level and low-level operations, enabling users to perform tasks such as initialization, connection, axis control, IO operations, and advanced motion programming.

- **Supported platforms:** Windows 7 or later
- **Supported frameworks:** .NET Framework 4.8
- **Documentation:** Full API documentation is provided in the accompanying `AAMotion_Documentation.chm` file.

## System Requirements

- Microsoft Windows 7 or later
- .NET Framework 4.8
- Visual Studio 2010 or later (other IDEs may be used, but are not officially supported)

## Installation

Install the NuGet package from [nuget.org](https://www.nuget.org/):
```.dotnet add package Agito.AAMotion```

Or via the NuGet Package Manager in Visual Studio.

> **Note:**  
> The package may require additional dependencies such as `YamlDotNet`. These are automatically handled by NuGet.

## Getting Started

1. Create a new C# project targeting .NET Framework 4.8.
2. Add the `Agito.AAMotion` NuGet package.
3. Reference the API documentation in `AAMotion_Documentation.chm` for detailed usage.

## Usage Example

Below is a minimal example demonstrating how to initialize a controller, connect to it, perform basic axis operations, and shut down:
```csharp
using System; 
using System.Diagnostics; 
using AAMotion;

namespace ConsoleApp1 
{ 
    class Program 
    { 
        static void Main(string[] args) 
        { 
            const string ip = "172.1.1.101"; 
            MotionController controller = AAMotionAPI.Initialize(ControllerType.AGM8000);
            try
            {
                Console.WriteLine($"Connecting to {ip}");
                controller.Init();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                bool isConnected = AAMotionAPI.Connect(controller, ip);

                stopwatch.Stop();
                Console.WriteLine($"Connect execution time: {stopwatch.ElapsedMilliseconds} ms");

                if (isConnected)
                {
                    Console.WriteLine("Connect successful");
                    AAMotionAPI.MotorOn(controller, AxisRef.B);

                    // Example: perform additional axis or IO operations here
                }
                else
                {
                    Console.WriteLine("Connect failed");
                }
            }
            finally
            {
                // Ensure proper shutdown
                AAMotionAPI.MotorOff(controller, AxisRef.B);
                Console.WriteLine($"MotorOff status for Axis A: {controller.GetAxis(AxisRef.A).MotorOn}");
                controller.Shutdown();
                Console.WriteLine("AACommServer has been shut down.");
            }
        }
    }
}

```

## API Documentation

Comprehensive API documentation is provided in the `AAMotion_Documentation.chm` file included with the NuGet package.

## Release Notes

See [CHANGELOG.md](CHANGELOG.md) for detailed release notes and version history.

## License

This software is **proprietary**. All rights reserved. Contact Agito for licensing and usage terms.

---

For further information, consult the included documentation or contact Agito support.