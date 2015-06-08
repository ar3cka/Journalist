#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.Testing.XUnit2

let buildDir = "out/build"
let testDir = "out/test"
let nugetDir = "out/nuget"

let applicationProjects = !! "src/**/*.fsproj" ++ "src/**/*.csproj"
let testProjects = !! "test/**/*.fsproj" ++ "test/**/*.csproj"

Target "Clean" (fun _ ->
    CleanDirs [ buildDir; testDir ]
)

Target "GenerateAssemblyInfo" (fun _ ->

    CreateCSharpAssemblyInfo "src/SolutionInfo.cs" [
            Attribute.Product "Journalist";
            Attribute.Version "0.0.1";
            Attribute.InformationalVersion "0.0.1";
            Attribute.FileVersion "0.0.1" ]
)

Target "BuildApp" (fun _ ->
    MSBuildRelease buildDir "Build" applicationProjects |> ignore
)

Target "BuildTest" (fun _ ->
    MSBuildRelease testDir "Build" testProjects |> ignore
)

Target "RunUnitTests" (fun _ ->
    !! (testDir + "/*.UnitTests.dll")
    |> xUnit2 (fun p ->
        { p with ToolPath = "packages/xunit.runner.console/tools/xunit.console.exe" })
)

Target "RunIntegrationTests" (fun _ ->
    !! (testDir + "/*.IntegrationTests.dll")
    |> xUnit2 (fun p ->
        { p with ToolPath = "packages/xunit.runner.console/tools/xunit.console.exe" })
)

"Clean"
    ==> "GenerateAssemblyInfo"
    ==> "BuildApp"
    ==> "BuildTest"
    ==> "RunUnitTests"
    ==> "RunIntegrationTests"

RunTargetOrDefault "RunIntegrationTests"
