namespace Shared

open System

type Todo =
    { Id : Guid
      Description : string }

type CodeSubmission =
    { Id : Guid
      TeamName : string
      FileName : string
      SourceCode : string
      // TODO : Add language indicator?
    }

type Result =
    ((string * int * int * int * int) array) * (((string * string)*(int * int)) array)

module CodeSubmission =
    let create teamName fileName source =
        { Id = Guid.NewGuid()
          TeamName = teamName
          FileName = fileName
          SourceCode = source }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos : unit -> Async<Todo list>
      getResults : unit -> Async<Result option>
      addTodo : Todo -> Async<Todo>
      addSubmission : CodeSubmission -> Async<bool>
      runCompetition : unit -> Async<Result> }