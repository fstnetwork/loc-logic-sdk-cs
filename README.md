# C# Runtime Experiment

## Getting Started
Step 1. run Saffron Runtime simulation (provide gRPC agent calls)
```bash
dotnet run --project example/RuntimeServer
```

Step 2. build your Logic code
```bash
dotnet publish Logic \
    /p:NativeLib=Shared \
    --runtime linux-arm64 \
    --no-self-contained
```

Step 3. Execute C# Runtime with your Logic
```bash
export LD_LIBRARY_PATH='/workspaces/LOC/Logic/bin/Debug/net7.0/linux-arm64/publish'
# export LD_LIBRARY_PATH='/workspaces/LOC/Logic/bin/release/net7.0/linux-arm64/publish'
dotnet run --project Runtime -- --runtime-address http://localhost:5224
```

## Pack and Publish SDK
Step 1. package SDK as a .nupkg file
```bash
dotnet pack SDK
```

Step 2.
publish to https://www.nuget.org/
```bash
dotnet nuget push ./SDK/bin/Debug/LOC.Logic.SDK.0.0.1.nupkg \
    --api-key qz2jga8pl3dvn2akksyquwcs9ygggg4exypy3bhxy6w6x6 \
    --source https://api.nuget.org/v3/index.json \
    --skip-duplicate
```

or publish to the test server https://int.nugettest.org/
```bash
dotnet nuget push ./SDK/bin/Debug/LOC.Logic.SDK.0.0.1.nupkg \
    --api-key oy2owt5lhzhy7k2txbwapz4ovzyn6r4enohleifrbvyq64 \
    --source https://int.nugettest.org/api/v2/package \
    --skip-duplicate
```

or publish to self-host NuGet server (e.g. BaGet)
```bash
kubectl port-forward svc/baget 5000:80 --address 192.168.96.80
dotnet nuget push ./SDK/bin/Debug/LOC.Logic.SDK.0.0.1.nupkg \
    --api-key ERTdMbF49MS6jaaxvXqDntly \
    --source http://192.168.96.80:5000/v3/index.json \
    --skip-duplicate
```

## Download from difference NuGet source

add new NuGet source
```bash
dotnet nuget add source https://int.nugettest.org/api/v2/ --name nugettest.org
```

add package from specify source
```bash
dotnet add package SDK --version 0.0.1 --source "nugettest.org"
```

download all decencies from specify source
```bash
dotnet restore --source "nugettest.org
```