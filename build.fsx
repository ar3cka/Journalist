#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.Testing.XUnit2

let nugetDir = "out/nuget"

let applicationProjects = !! "src/**/*.fsproj" ++ "src/**/*.csproj"
let testProjects = !! "test/**/*.fsproj" ++ "test/**/*.csproj"

MSBuildDefaults <- { MSBuildDefaults with Verbosity = Some MSBuildVerbosity.Minimal }

Target "Clean" (fun _ ->
    CleanDirs <| !! "src/**/bin/Release"
    CleanDirs <| !! "test/**/bin/Release"
    CleanDirs [ nugetDir ]
)

Target "GenerateAssemblyInfo" (fun _ ->

    CreateCSharpAssemblyInfo "src/SolutionInfo.cs" [
            Attribute.Product "Journalist";
            Attribute.Version "0.0.1";
            Attribute.InformationalVersion "0.0.1";
            Attribute.FileVersion "0.0.1";
            Attribute.Company "Anton Mednonogov" ]
)

Target "BuildApp" (fun _ ->
    MSBuildRelease null "Build" [ "./Journalist.sln" ] |> ignore
)

Target "RunUnitTests" (fun _ ->
    !! ("./test/**/bin/Release/*.UnitTests.dll")
    |> xUnit2 (fun p ->
        { p with ToolPath = "packages/xunit.runner.console/tools/xunit.console.exe" })
)

Target "RunIntegrationTests" (fun _ ->
    !! ("./test/**/bin/Release/*.IntegrationTests.dll")
    |> xUnit2 (fun p ->
        { p with ToolPath = "packages/xunit.runner.console/tools/xunit.console.exe" })
)

Target "CreatePackages" (fun _ ->
    Paket.Pack (fun p ->
        { p with OutputPath = nugetDir; })
)

"Clean"
    ==> "GenerateAssemblyInfo"
    ==> "BuildApp"
    ==> "RunUnitTests"
    ==> "RunIntegrationTests"
    ==> "CreatePackages"

RunTargetOrDefault "CreatePackages"
