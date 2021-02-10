module Index

open Elmish
open Fable.Remoting.Client
open Shared
open System

type Model =
    { Todos: Todo list
      TeamName: string
      FileName: string
      SourceCode: string
      Results: Result option }

type Msg =
    | GotTodos of Todo list
    | SetTeamName of string
    | SetFileName of string
    | SetSourceInput of string
    | SubmitCode
    | CodeSubmitted of bool
    | RunCompetition
    | GetTodos
    | DisplayResult of Result
    | GetResult
    | GotResult of Result option

let isValid (description: string) =
    String.IsNullOrWhiteSpace description |> not

let todosApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init(): Model * Cmd<Msg> =
    let model =
        { Todos = []
          FileName = ""
          TeamName = ""
          SourceCode = ""
          Results = None }
    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotTodos todos ->
        { model with Todos = todos }, Cmd.none
    | SetFileName value ->
        { model with FileName = value }, Cmd.none
    | SetTeamName value ->
        { model with TeamName = value }, Cmd.none
    | SetSourceInput value ->
        { model with SourceCode = value }, Cmd.none
    | SubmitCode ->
        let submission = CodeSubmission.create model.TeamName model.FileName model.SourceCode
        let cmd = Cmd.OfAsync.perform todosApi.addSubmission submission CodeSubmitted
        model, cmd
    | CodeSubmitted result ->
        { model with FileName = ""; SourceCode = "" }, Cmd.none
    | RunCompetition ->
        let cmd = Cmd.OfAsync.perform todosApi.runCompetition () DisplayResult
        model, cmd
    | GetResult ->
        let cmd = Cmd.OfAsync.perform todosApi.getResults () GotResult
        model, cmd
    | GotResult r ->
        printfn "%A" r
        { model with Results = r }, Cmd.none
    | DisplayResult result ->
        { model with Results = Some result }, Cmd.none
    | GetTodos ->
        let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
        model, cmd

open Fable.React
open Fable.React.Props
open Fulma

let navBrand =
    Navbar.Brand.div [ ] [
        Navbar.Item.a [
            Navbar.Item.Props [ Href "https://www.turing.ac.uk" ]
            Navbar.Item.IsActive true
        ] [
            img [
                Src "/favicon.png"
                Alt "Logo"
            ]
        ]
    ]

let submissionBox (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ] [
        // Content.content [ ] [
        //     Heading.h4 [] [str "How to submit code"]
        //     Content.Ol.ol [ ] [
        //         li [ ] [ str "Enter your team name" ]
        //         li [ ] [ str "Enter file name for your python script (include your team name)" ]
        //         li [ ] [ str "Paste in your source code" ]
        //         li [ ] [ str "Please don't crash my server 😬"]
        //     ]
        // ]
        Field.div [ Field.IsGrouped ] [
            Control.p [ Control.IsExpanded ] [
                Input.text [
                  Input.Value model.FileName
                  Input.Placeholder "Source file name (including extension)"
                  Input.OnChange (fun x -> SetFileName x.Value |> dispatch) ]
                br []
                str "Please try to use unique file names, for example including a team name in the file name."
                br []
                br []
                Textarea.textarea [
                    Textarea.Value model.SourceCode
                    Textarea.Placeholder "Paste source code here"
                    Textarea.OnChange (fun x -> SetSourceInput x.Value |> dispatch)
                ] []
                Button.a [
                    Button.Color IsPrimary
                    Button.Disabled ((isValid model.FileName && isValid model.SourceCode) |> not)
                    Button.OnClick (fun _ -> dispatch SubmitCode)
                ] [
                    str "Submit code"
                ]
            ]
        ]
    ]

let getName (filename: string) =
    let dotIdx = filename.LastIndexOf '.'
    filename.[filename.LastIndexOf '/' + 1  .. (if dotIdx < 0 then filename.Length else dotIdx) - 1]

let resultBox (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ] [
        Content.content [ ] [
            match model.Results with
            | None ->
                Heading.h4 [] [str "No results so far"]
            | Some (resultStats, resultLookup) ->
                let strategies = resultStats |> Array.map (fun (s, _, _, _, _) -> s)

                div [ Style [ OverflowX OverflowOptions.Auto ] ] [
                    Table.table [
                        Table.IsBordered
                        Table.IsNarrow
                        Table.IsStriped  ] [
                      thead [ ]
                        [ tr [ ]
                             [  yield th [] []
                                yield th [] [ str "Total Score" ]
                                //for (strategy, score, w, d, l) in resultStats do yield th [] [ str (getName strategy) ]
                                yield th [] [ str "Wins"]
                                yield th [] [ str "Draws" ]
                                yield th [] [ str "Losses" ] ]
                        ]
                      tbody [  ]
                        [ for (strategy, score, w, d, l) in resultStats ->
                             tr [] [
                              yield td [ ] [ str (getName strategy) ]
                              yield td [] [ str (string score) ]
                            //   for s2 in strategies do
                            //       let x1, x2 = resultLookup |> Array.find (fun ((sA, sB), x) -> sA = strategy && sB = s2) |> snd
                            //       yield td [] [ str (sprintf "%d:%d" x1 x2) ]
                              yield td [] [ str (string w) ]
                              yield td [] [ str (string d) ]
                              yield td [] [ str (string l) ] ]
                        ]
                    ]
                ]

                div [ Style [ OverflowX OverflowOptions.Auto ] ] [
                    br []
                    h4 [ ] [ str "Pairwise scores"]
                    Table.table [
                        Table.IsBordered
                        Table.IsNarrow
                        Table.IsStriped  ] [
                      thead [ ]
                        [ tr [ ]
                             [  yield th [] []
                                for (strategy, score, w, d, l) in resultStats do yield th [] [ str (getName strategy) ]
                             ]
                        ]
                      tbody [  ]
                        [ for (strategy, score, w, d, l) in resultStats ->
                             tr [] [
                              yield td [ ] [ str (getName strategy) ]
                              for s2 in strategies do
                                  let x1, x2 = resultLookup |> Array.find (fun ((sA, sB), x) -> sA = strategy && sB = s2) |> snd
                                  yield td [] [ str (sprintf "%d:%d" x1 x2) ]
                             ]
                        ]
                    ]
                ]
              ]
        ]


let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsPrimary
        Hero.IsFullHeight
        Hero.Props [
            Style [
                Background """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
                BackgroundSize "cover"
            ]
        ]
    ] [
        Hero.head [ ] [
            Navbar.navbar [ ] [
                Container.container [ ] [ navBrand ]
            ]
        ]

        Hero.body [ ] [
            Container.container [ ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is10)
                    Column.Offset (Screen.All, Column.Is1)
                ] [
                    Heading.h1 [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "Iterated Prisoner's Dilemma" ]
                    Heading.h2 [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "REG Competition" ]

                    submissionBox model dispatch

                    Heading.h2 [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "Results" ]

                    resultBox model dispatch

                    Control.p [ ] [
                        Button.a [
                            Button.Color IsPrimary
                            Button.OnClick (fun _ -> dispatch GetResult)
                        ] [
                            str "Refresh results"
                        ]
                    ]
                ]
            ]
        ]
    ]
