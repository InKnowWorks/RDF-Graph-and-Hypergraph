# Developer Setup Guide: Trinity Graph Engine with LIKQ

This guide provides comprehensive instructions for setting up a development environment for Trinity Graph Engine (TGE) with LIKQ fanout search capabilities. The guide assumes you're using Visual Studio Professional 2022, VSCode, C# 13, and .NET 9.

## System Requirements

Ensure your development machine meets these minimum requirements:

- Windows 10/11 or Windows Server 2019/2022
- 16 GB RAM (32 GB recommended for larger graph data)
- 100 GB free disk space for development and data
- Multi-core processor (8+ cores recommended)
- SSD storage (for improved performance with large graphs)

## Installation of Required Software

### 1. Install Visual Studio 2022 Professional

1. Download Visual Studio 2022 Professional from the official website
2. During installation, select the following workloads:
   - .NET desktop development
   - ASP.NET and web development
   - Data storage and processing
   - .NET Multi-platform App UI development
3. Under Individual components, ensure these are selected:
   - .NET 9.0 SDK
   - .NET Framework 4.8 SDK
   - C# and Visual Basic
   - Git for Windows
   - GitHub Extension for Visual Studio

### 2. Install Visual Studio Code (Optional for Script Editing)

1. Download VSCode from code.visualstudio.com
2. Install these recommended extensions:
   - C# Dev Kit
   - .NET Runtime Install Tool
   - XML Tools
   - Trinity Graph Engine VSCode Extension (if available)

### 3. Install .NET 9 SDK

1. Download the .NET 9 SDK from dotnet.microsoft.com

2. Follow the installation instructions

3. Verify installation by opening a command prompt and running:

   ```
   dotnet --version
   ```

### 4. Install Trinity Graph Engine

Trinity Graph Engine can be installed via NuGet packages or from source:

#### Method 1: NuGet Installation (Recommended)

Create a new project first (see next section), then add these NuGet packages:

- GraphEngine.Core
- GraphEngine.LIKQ
- GraphEngine.Client
- GraphEngine.Storage

#### Method 2: Source Installation (For Advanced Customization)

1. Clone the repository:

   bash

   ```bash
   git clone https://github.com/Microsoft/GraphEngine.git
   ```

2. Follow the build instructions in the repository README

## Setting Up a Trinity Graph Engine Project

### 1. Create a New Project

1. Open Visual Studio 2022
2. Choose "Create a new project"
3. Select "Console App" with C# language
4. Name your project (e.g., "TGEAdvancedDemo") and choose a location
5. Select ".NET 9.0" as the target framework
6. Enable nullable reference types and uncheck "Do not use top-level statements"

### 2. Add NuGet Packages

1. Right-click on the project in Solution Explorer and select "Manage NuGet Packages"

2. Add the following packages:

   ```
   GraphEngine.Core (latest version)
   GraphEngine.LIKQ (latest version)
   GraphEngine.Client (latest version)
   GraphEngine.Storage (latest version)
   ```

### 3. Create Project Structure

Create the following folders in your project:

```
/Models        # For TSL data models
/Services      # For service implementations
/Operations    # For graph operations
/Utils         # For utility classes
/Config        # For configuration files
```

### 4. Configure Trinity Settings

1. Create a `trinity.xml` file in your project root with the following content:

xml

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Trinity ConfigVersion="2.0" xmlns="https://www.graphengine.io/schema/config">
    <Cluster RunningMode="Server">
        <Server Endpoint="localhost:7001" 
                AssemblyPath="[AppPath]">
            <Network HttpPort="-1" 
                     ClientMaxConn="2" 
                     ClientSendRetry="5" 
                     ClientReconnectRetry="5" 
                     Handshake="TRUE"/>
            <Logging LogDirectory="[AppPath]/Logs/"
                     LogLevel="Verbose" 
                     LogToFile="TRUE" 
                     EchoOnConsole="TRUE"/>
            <Storage TrunkCount="256"
                     ReadOnly="FALSE" 
                     StorageCapacity="Max4G"
                     StorageRoot="[AppPath]/Storage/" 
                     DefragInterval="600"/>
            <LIKQ Timeout="60000" />
        </Server>        
    </Cluster>
</Trinity>
```

1. Ensure the file is copied to the output directory by setting its properties:
   - Right-click the file
   - Properties
   - Set "Copy to Output Directory" to "Copy if newer"

## Configuring Trinity Specification Language (TSL)

TSL is Trinity's data modeling language for defining graph structures.

### 1. Create TSL Schema Files

1. Create a new folder called "TSL" in your project
2. Create a file for each graph structure (e.g., `DirectedGraph.tsl`, `RedBlackTree.tsl`, etc.)

Example content for `DirectedGraph.tsl`:

```
// DirectedGraph.tsl
struct Vertex
{
    long id;
    string data;
    
    [GraphEdge]
    List<long> outEdges;
    
    [GraphEdge]
    List<long> inEdges;
}

struct Edge
{
    long id;
    long source;
    long target;
    string data;
}

struct DirectedGraph
{
    int vertexCount;
    int edgeCount;
}

cell DirectedGraphModel
{
    DirectedGraph graph;
    
    [Index]
    List<Vertex> vertices;
    List<Edge> edges;
}
```

### 2. Install TSL Compiler

The TSL compiler generates C# code from TSL schemas.

1. Install the TSL compiler as a global .NET tool:

   ```
   dotnet tool install --global GraphEngine.TSL.CompilerClient
   ```

2. Add a build event in Visual Studio to compile TSL files:

   - Right-click project → Properties

   - Build → Events

   - Pre-build event command line:

     ```
     tslc -d "$(ProjectDir)TSL" -o "$(ProjectDir)Generated" -n "$(RootNamespace).Generated"
     ```

3. Add Generated folder to .gitignore (it contains auto-generated code)

### 3. Configure TSL Integration

1. Create a `tsl.json` file in the TSL folder:

json

```json
{
  "Namespace": "TGEAdvancedDemo.Generated",
  "OutputDirectory": "../Generated",
  "ReferencedModules": [],
  "EnableExtensionLibrary": true
}
```

1. Add a C# file called `TSLModuleRegister.cs` to register generated TSL modules:

csharp

```csharp
using Trinity;
using Trinity.TSL;

namespace TGEAdvancedDemo
{
    public static class TSLModuleRegister
    {
        public static void RegisterTSLModules()
        {
            TSLCompiler.CompileFromFile("TSL/DirectedGraph.tsl");
            // Add more TSL files as needed
            
            // Register generated modules
            TrinityConfig.LoadTSLPlugins();
        }
    }
}
```

## Setting Up LIKQ Components

### 1. Create LIKQ Configuration

1. Create a `LIKQConfig.cs` file in the Config folder:

csharp

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Trinity;
using FanoutSearch;
using FanoutSearch.LIKQ;

namespace TGEAdvancedDemo.Config
{
    public static class LIKQConfig
    {
        public static void Configure()
        {
            // Register index service for custom query logic
            FanoutSearchModule.RegisterIndexService(CustomIndexService);
            
            // Configure LIKQ dialect
            LambdaDSL.SetDialect("GraphNode", "StartFrom", "VisitNode", "FollowEdge", "Action");
            
            // Set query timeout (in milliseconds)
            FanoutSearchModule.SetQueryTimeout(60000);
            
            // Register custom types for traversal
            RegisterCustomTypes();
            
            Console.WriteLine("LIKQ configured successfully!");
        }
        
        private static IEnumerable<long> CustomIndexService(object matchObject, string typeString)
        {
            // Implement custom index query logic for node lookup
            // This is a simple example that returns empty results
            return Enumerable.Empty<long>();
        }
        
        private static void RegisterCustomTypes()
        {
            // Register BFO entity and relation types
            TraverseActionSecurityChecker.RegisterQueryWhitelistType(typeof(BFOEntityType));
            TraverseActionSecurityChecker.RegisterQueryWhitelistType(typeof(BFORelationType));
            
            // Add more custom types as needed
        }
    }
}
```

### 2. Create BFO Type Definitions

1. Create a file `BFOTypes.cs` in the Models folder:

csharp

```csharp
namespace TGEAdvancedDemo.Models
{
    // BFO entity and relation types
    public enum BFOEntityType 
    { 
        IndependentContinuant, 
        SpecificallyDependent, 
        GenericallyDependent,
        SpatialRegion, 
        Occurrent, 
        TemporalRegion 
    }
    
    public enum BFORelationType 
    { 
        PartOf, 
        HasParticipant, 
        ParticipatesIn, 
        HasQuality,
        Realizes, 
        LocatedIn, 
        TemporallyRelated 
    }
}
```

## Project Configuration for Performance

### 1. Memory Optimization

Add the following to your `.csproj` file to optimize memory usage:

xml

```xml
<PropertyGroup>
  <ServerGarbageCollection>true</ServerGarbageCollection>
  <ConcurrentGarbageCollection>true</ConcurrentGarbageCollection>
  <GCLargeObjectHeapCompactionMode>Default</GCLargeObjectHeapCompactionMode>
  <GCHighMemoryPercent>90</GCHighMemoryPercent>
</PropertyGroup>
```

### 2. Performance Settings

Create an `App.config` file with the following content:

xml

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <runtime>
    <gcServer enabled="true"/>
    <gcConcurrent enabled="true"/>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Trinity.Core" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-3.0.0.0" newVersion="3.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

### 3. Advanced Build Settings

1. Configure project for 64-bit builds:
   - Project Properties → Build → Platform target: x64
2. Enable optimizations:
   - Project Properties → Build → Optimize code: Checked
   - Project Properties → Build → Advanced → Enable Tailcalls: true

## IDE Configuration

### 1. Visual Studio Configuration

1. Install the Trinity Graph Engine Extension (if available):
   - Extensions → Manage Extensions → Search for "Trinity Graph Engine"
2. Configure code style settings:
   - Tools → Options → Text Editor → C# → Code Style
   - Set "Prefer braces" to "When multiline"
   - Set "Prefer expression body" to "Never"
3. Enable advanced debugging:
   - Tools → Options → Debugging
   - Enable "Enable Just My Code" and "Enable source server support"

### 2. VSCode Configuration (Optional)

Create a `.vscode` folder with the following files:

**launch.json**:

json

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "TGE Debug",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net9.0/TGEAdvancedDemo.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "console": "internalConsole",
      "stopAtEntry": false
    }
  ]
}
```

**tasks.json**:

json

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/TGEAdvancedDemo.csproj",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

**settings.json**:

json

```json
{
  "csharp.format.enable": true,
  "editor.formatOnSave": true,
  "editor.rulers": [100],
  "files.exclude": {
    "**/Generated": true,
    "**/bin": true,
    "**/obj": true
  }
}
```

## Debugging and Development Tips

### 1. Trinity-Specific Debugging

1. Enable Trinity logging in your `Program.cs`:

csharp

```csharp
// Configure Trinity logging
TrinityConfig.LoggingLevel = LogLevel.Verbose;
TrinityConfig.LogToConsole = true;
```

1. Visualize graphs during debugging:

   - Create a DebugVisualizer helper class:

   csharp

   ```csharp
   public static class DebugVisualizer
   {
       public static void DumpGraph(long graphId)
       {
           using var graph = Global.LocalStorage.UseDirectedGraphModel(graphId);
           Console.WriteLine($"Graph ID: {graphId}");
           Console.WriteLine($"Vertices: {graph.graph.vertexCount}");
           Console.WriteLine($"Edges: {graph.graph.edgeCount}");
           
           Console.WriteLine("\nVertices:");
           foreach (var vertex in graph.vertices)
           {
               Console.WriteLine($"  ID: {vertex.id}, Data: {vertex.data}");
               Console.WriteLine($"    OutEdges: {string.Join(", ", vertex.outEdges)}");
               Console.WriteLine($"    InEdges: {string.Join(", ", vertex.inEdges)}");
           }
           
           Console.WriteLine("\nEdges:");
           foreach (var edge in graph.edges)
           {
               Console.WriteLine($"  ID: {edge.id}, Source: {edge.source}, Target: {edge.target}, Data: {edge.data}");
           }
       }
   }
   ```

### 2. LIKQ Query Debugging

1. Enable LIKQ query logging:

csharp

```csharp
// Enable LIKQ query logging
FanoutSearchModule.EnableQueryLogging = true;
```

1. Create a LIKQ query debugger helper:

csharp

```csharp
public static class LIKQDebugger
{
    public static void TraceQuery(FanoutSearchDescriptor query)
    {
        Console.WriteLine("LIKQ Query Structure:");
        Console.WriteLine($"  Origin: {(query.m_origin != null ? string.Join(", ", query.m_origin) : query.m_origin_query)}");
        Console.WriteLine($"  Traverse Actions: {query.m_traverseActions.Count}");
        Console.WriteLine($"  Edge Types: {query.m_edgeTypes.Count}");
        Console.WriteLine($"  Select Fields: {query.m_selectFields.Count}");
        
        // Execute query and capture results for inspection
        var results = query.ToList();
        Console.WriteLine($"  Results: {results.Count} paths found");
        
        foreach (var result in results.Take(5)) // Show first 5 results
        {
            Console.WriteLine("  Path:");
            foreach (var node in result)
            {
                Console.WriteLine($"    Node ID: {node.id}");
                foreach (var kv in node)
                {
                    Console.WriteLine($"      {kv.Key}: {kv.Value}");
                }
            }
        }
    }
}
```

### 3. Performance Profiling

1. Add .NET counters for monitoring:

csharp

```csharp
// Add to Program.cs
using System.Diagnostics;

static void EnablePerformanceCounters()
{
    DiagnosticListener.AllListeners.Subscribe(new DiagnosticObserver());
}

class DiagnosticObserver : IObserver<DiagnosticListener>
{
    public void OnCompleted() { }
    public void OnError(Exception error) { }
    
    public void OnNext(DiagnosticListener listener)
    {
        if (listener.Name == "GraphEngine")
        {
            listener.Subscribe(new GraphEngineMetricsObserver());
        }
    }
}

class GraphEngineMetricsObserver : IObserver<KeyValuePair<string, object>>
{
    public void OnCompleted() { }
    public void OnError(Exception error) { }
    
    public void OnNext(KeyValuePair<string, object> value)
    {
        Console.WriteLine($"[Metric] {value.Key}: {value.Value}");
    }
}
```

## Testing Trinity Applications

### 1. Create Unit Tests

1. Add a new Test Project to your solution:

   - Right-click Solution → Add → New Project
   - Select "xUnit Test Project" or "NUnit Test Project"
   - Name it "TGEAdvancedDemo.Tests"

2. Add NuGet packages to the test project:

   ```
   GraphEngine.Core
   GraphEngine.LIKQ
   GraphEngine.Client
   GraphEngine.Storage
   Microsoft.NET.Test.Sdk
   xunit (or NUnit)
   xunit.runner.visualstudio (or NUnit.ConsoleRunner)
   ```

3. Create a base test class for TGE:

csharp

```csharp
using System;
using System.IO;
using Trinity;
using Xunit;

namespace TGEAdvancedDemo.Tests
{
    public abstract class TrinityTestBase : IDisposable
    {
        protected TrinityServer Server { get; private set; }
        
        protected TrinityTestBase()
        {
            // Use in-memory storage for tests
            TrinityConfig.StorageRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TrinityConfig.StorageCapacity = StorageCapacity.Max1G;
            
            // Start a server for the test
            Server = new TrinityServer();
            Server.RegisterCommunicationModule<FanoutSearchModule>();
            Server.Start();
            
            // Setup test data
            SetupTestData();
        }
        
        protected abstract void SetupTestData();
        
        public void Dispose()
        {
            Server?.Stop();
            Global.CloudStorage?.Dispose();
            Directory.Delete(TrinityConfig.StorageRoot, true);
        }
    }
}
```

1. Create specific test classes:

csharp

```csharp
using System;
using System.Linq;
using FanoutSearch.LIKQ;
using Xunit;

namespace TGEAdvancedDemo.Tests
{
    public class DirectedGraphTests : TrinityTestBase
    {
        private long _graphId;
        
        protected override void SetupTestData()
        {
            // Create a test directed graph
            _graphId = CreateTestGraph();
        }
        
        private long CreateTestGraph()
        {
            // Create a small test graph
            var graph = new DirectedGraphModel
            {
                graph = new DirectedGraph
                {
                    vertexCount = 0,
                    edgeCount = 0
                },
                vertices = new List<Vertex>(),
                edges = new List<Edge>()
            };
            
            // Add vertices and edges
            // ...
            
            return Global.LocalStorage.SaveDirectedGraphModel(graph);
        }
        
        [Fact]
        public void TestFindPath()
        {
            // Test finding a path in the graph
            var paths = KnowledgeGraph
                .StartFrom(_graphId)
                .VisitNode(graph => graph.continue_if(graph.type("DirectedGraphModel")))
                .FollowEdge("vertices")
                .VisitNode(vertex => {
                    // Find vertex with ID 1
                    long id = long.Parse(vertex.get("id"));
                    return vertex.continue_if(id == 1);
                })
                .FollowEdge("outEdges")
                .VisitNode(edge => Action.Continue)
                .FollowEdge("target")
                .VisitNode(target => Action.Return)
                .ToList();
            
            Assert.NotEmpty(paths);
            // Add more assertions
        }
    }
}
```

### 2. Integration Tests

1. Create an integration test class with end-to-end scenarios:

csharp

```csharp
public class IntegrationTests : TrinityTestBase
{
    protected override void SetupTestData()
    {
        // Create all necessary data structures
        CreateDirectedGraph();
        CreateRedBlackTree();
        CreateSkipList();
        CreateHypergraph();
        CreateBFOOntologyModel();
    }
    
    [Fact]
    public void TestEndToEndScenario()
    {
        // Test a complete workflow
        // ...
    }
}
```

### 3. Performance Tests

1. Create a performance test class:

csharp

```csharp
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

[Collection("Sequential")]
public class PerformanceTests : TrinityTestBase
{
    private const int IterationCount = 100;
    
    protected override void SetupTestData()
    {
        // Create large test graph
        CreateLargeGraph(10000, 50000);
    }
    
    [Fact]
    public void MeasureLIKQQueryPerformance()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        for (int i = 0; i < IterationCount; i++)
        {
            var paths = KnowledgeGraph
                .StartFrom(_graphId)
                // Query details
                .ToList();
        }
        
        stopwatch.Stop();
        
        Console.WriteLine($"Average query time: {stopwatch.ElapsedMilliseconds / (double)IterationCount} ms");
        Assert.True(stopwatch.ElapsedMilliseconds / (double)IterationCount < 100); // Assert average time < 100ms
    }
}
```

## Deployment Considerations

### 1. Creating a Production Build

1. Configure Release build settings:
   - Project Properties → Build → Configuration: Release
   - Project Properties → Build → Optimize code: Checked
   - Project Properties → Package → Generate NuGet package: Checked (if creating a library)
2. Create a deployment script `deploy.ps1`:

powershell

```powershell
# Build the solution
dotnet build -c Release

# Create output directory
$deployDir = ".\Deploy"
if (!(Test-Path $deployDir)) {
    New-Item -ItemType Directory -Path $deployDir
}

# Copy binaries
Copy-Item .\bin\Release\net9.0\* $deployDir -Recurse

# Copy configuration
Copy-Item .\trinity.xml $deployDir

# Create data directories
New-Item -ItemType Directory -Path "$deployDir\Logs" -Force
New-Item -ItemType Directory -Path "$deployDir\Storage" -Force

Write-Host "Deployment package created in $deployDir"
```

### 2. Running as a Service

1. Create a Windows Service wrapper:

csharp

```csharp
// Create a file TrinityService.cs
using System.ServiceProcess;
using Trinity;

namespace TGEAdvancedDemo
{
    public class TrinityService : ServiceBase
    {
        private TrinityServer server;
        
        public TrinityService()
        {
            this.ServiceName = "TrinityGraphEngine";
            this.CanStop = true;
            this.CanPauseAndContinue = false;
        }
        
        protected override void OnStart(string[] args)
        {
            TrinityConfig.LoadConfig("trinity.xml");
            
            server = new TrinityServer();
            server.RegisterCommunicationModule<FanoutSearchModule>();
            
            // Configure LIKQ
            Config.LIKQConfig.Configure();
            
            server.Start();
        }
        
        protected override void OnStop()
        {
            server?.Stop();
        }
        
        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Console.WriteLine("Trinity Graph Engine running. Press Enter to stop.");
            Console.ReadLine();
            OnStop();
        }
    }
}
```

1. Update Program.cs to support both console and service mode:

csharp

```csharp
using System;
using System.ServiceProcess;

namespace TGEAdvancedDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new TrinityService();
            
            if (args.Length > 0 && args[0].ToLower() == "-service")
            {
                // Run as Windows Service
                ServiceBase.Run(service);
            }
            else
            {
                // Run as console application
                Console.WriteLine("Trinity Graph Engine Advanced Demo");
                Console.WriteLine("==================================================");
                
                service.RunAsConsole(args);
            }
        }
    }
}
```

1. Install as a Windows Service:

```
sc create TrinityGraphEngine binPath= "C:\Path\To\TGEAdvancedDemo.exe -service" start= auto
sc description TrinityGraphEngine "Trinity Graph Engine Service with LIKQ"
sc start TrinityGraphEngine
```

### 3. Docker Deployment

1. Create a Dockerfile:

dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TGEAdvancedDemo.csproj", "./"]
RUN dotnet restore "TGEAdvancedDemo.csproj"
COPY . .
RUN dotnet build "TGEAdvancedDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TGEAdvancedDemo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME /app/Storage
VOLUME /app/Logs
EXPOSE 7001
ENTRYPOINT ["dotnet", "TGEAdvancedDemo.dll"]
```

1. Create a docker-compose.yml file:

yaml

```yaml
version: '3.8'
services:
  tge:
    build: .
    ports:
      - "7001:7001"
    volumes:
      - ./Storage:/app/Storage
      - ./Logs:/app/Logs
    environment:
      - DOTNET_RUNNING_IN_CONTAINER=true
      - DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
```

1. Build and run with Docker:

bash

```bash
docker-compose build
docker-compose up -d
```

------

This setup guide provides a comprehensive framework for developing Trinity Graph Engine applications with LIKQ. By following these steps, you'll have a well-structured environment for creating, testing, and deploying advanced graph data structure applications.

For additional support or advanced configurations, refer to the official Trinity Graph Engine documentation or reach out to the community forums.