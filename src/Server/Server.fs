module Server.Main

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open System
open System.Diagnostics
open System.IO

open Shared

//let strategyDir = "../../strategies/" // change to /strategies in Docker
let strategyDir = "/strategies"

type Storage () =
    let todos = ResizeArray<_>()
    let submissions = ResizeArray<_>()
    let mutable result = None

    member __.GetTodos () =
        List.ofSeq todos

    member __.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok ()
        else Error "Invalid todo"

    member __.AddSubmission (submission: CodeSubmission) =
        submissions.Add submission
        Ok ()

    member __.GetSubmissions () =
        List.ofSeq submissions

    member __.SaveResults r =
        printfn "Saving result"
        result <- Some r
    member __.GetResults () = result

let storage = Storage()

storage.AddTodo(Todo.create "Create new SAFE project") |> ignore
storage.AddTodo(Todo.create "Write your app") |> ignore
storage.AddTodo(Todo.create "Hello from the server") |> ignore




let todosApi =
    { getTodos = fun () -> async { return storage.GetTodos() }
      getResults = fun () -> async {  return storage.GetResults() }
      addTodo =
        fun todo -> async {
            match storage.AddTodo todo with
            | Ok () -> return todo
            | Error e -> return failwith e
        }
      addSubmission =
        fun codeSubmission -> async {
            try
                File.WriteAllText(Path.Combine(strategyDir, codeSubmission.FileName), codeSubmission.SourceCode)
                storage.AddSubmission codeSubmission |> ignore
                return true
            with _ ->
                return false
        }
      runCompetition =
        fun () -> async {

            let strategies = Directory.GetFiles(strategyDir)
            // let strategies =
            //     storage.GetSubmissions

            let results = Server.Play.playAll strategies
            let resultStats: Result =
                Server.Play.stats results,
                Server.Play.resultLookup results
            storage.SaveResults resultStats

            // let strategy1 = System.IO.Path.Combine(strategyDir, "collaborate.py")
            // let strategy2 = System.IO.Path.Combine(strategyDir, "betray.py")
            // let! result = Server.Play.runCompetition 10 strategy1 strategy2
            return resultStats
        }
    }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
