#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing.XUnit2

let buildDir = "out/build"
let testDir = "out/test"

let applicationProjects = !! "src/**/*.fsproj" ++ "src/**/*.csproj"
let testProjects = !! "test/**/*.fsproj" ++ "test/**/*.csproj"

Target "Clean" (fun _ ->
    CleanDirs [ buildDir; testDir ]
)

Target "BuildApp" (fun _ ->
    MSBuildRelease buildDir "Build" applicationProjects |> ignore
)

Target "BuilTest" (fun _ ->
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
    ==> "BuildApp"
    ==> "BuilTest"
    ==> "RunUnitTests"
    ==> "RunIntegrationTests"

RunTargetOrDefault "RunIntegrationTests"
