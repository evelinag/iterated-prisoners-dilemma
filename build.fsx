#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"
#r "netstandard"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Farmer
open Farmer.Builders
open System
open System.IO

Target.initEnvironment ()

let sharedPath = Path.getFullName "./src/Shared"
let serverPath = Path.getFullName "./src/Server"
let deployDir = Path.getFullName "./deploy"
let sharedTestsPath = Path.getFullName "./tests/Shared"
let serverTestsPath = Path.getFullName "./tests/Server"
let releaseVersion = "0.0"

let npm args workingDir =
    let npmPath =
        match ProcessUtils.tryFindFileOnPath "npm" with
        | Some path -> path
        | None ->
            "npm was not found in path. Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            |> failwith

    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    Command.RawCommand (npmPath, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let dotnet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

Target.create "Clean" (fun _ -> Shell.cleanDir deployDir)

Target.create "InstallClient" (fun _ -> npm "install" ".")

Target.create "Bundle" (fun _ ->
    dotnet (sprintf "publish -c Release -o \"%s\"" deployDir) serverPath
    npm "run build" "."
)

Target.create "Azure" (fun _ ->
    let web = webApp {
        name "away_day_prisoners_dilemma"
        zip_deploy "deploy"
    }
    let deployment = arm {
        location Location.WestEurope
        add_resource web
    }

    deployment
    |> Deploy.execute "away_day_prisoners_dilemma" Deploy.NoParameters
    |> ignore
)

Target.create "Run" (fun _ ->
    dotnet "build" sharedPath
    [ async { dotnet "watch run" serverPath }
      async { npm "run start" "." } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunTests" (fun _ ->
    dotnet "build" sharedTestsPath
    [ async { dotnet "watch run" serverTestsPath }
      async { npm "run test:live" "." } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)


// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~`

let dockerUser = Environment.environVarOrDefault "DockerUser" ""
let dockerPassword = Environment.environVarOrDefault "DockerPassword" ""
let dockerLoginServer = Environment.environVarOrDefault "DockerLoginServer" ""
let dockerImageName = Environment.environVarOrDefault "DockerImageName" ""

Target.create "CreateDockerImage" (fun _ ->
    if String.IsNullOrEmpty dockerUser then
        failwithf "docker username not given."
    if String.IsNullOrEmpty dockerImageName then
        failwithf "docker image Name not given."
    let result =
        ["build"; "-t"; sprintf "%s/%s" dockerUser dockerImageName ;  "."]
        |> CreateProcess.fromRawCommand "docker"
        |> Proc.run
    if result.ExitCode <> 0 then failwith "Docker build failed"
)

Target.create "PushDockerImage" (fun _ ->
    let result =
        ["login"; "--username"; dockerUser; "--password"; dockerPassword ]
        |> CreateProcess.fromRawCommand "docker"
        |> Proc.run
//            info.WorkingDirectory <- deployDir

    if result.ExitCode <> 0 then failwith "Docker login failed"

    let result =
        [ "push"; sprintf "%s/%s:latest" dockerUser dockerImageName ]
        |> CreateProcess.fromRawCommand "docker"
        |> Proc.run
//            info.WorkingDirectory <- deployDir

    if result.ExitCode <> 0 then failwith "Docker push failed"
)

Target.create "DockerAzure" (fun _ ->
    let web = webApp {
        name "hut23-away-day"
        docker_image "evelina/away-day:latest" ""
        app_insights_off
    }
    let deployment = arm {
        location Location.WestEurope
        add_resource web
    }

    deployment
    |> Deploy.execute "hut23-away-day" Deploy.NoParameters
    |> ignore
)

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~`
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~`

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Bundle"
    ==> "Azure"

"Clean"
    ==> "InstallClient"
    ==> "Run"

"Clean"
    ==> "InstallClient"
    ==> "RunTests"


"Clean"
    ==> "InstallClient"
    ==> "CreateDockerImage"
    ==> "PushDockerImage"
//    ==> "DockerAzure"

Target.runOrDefaultWithArguments "Bundle"
