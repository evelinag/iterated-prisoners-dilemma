module Server.Play
open System
open System.IO
open System.Diagnostics

type Decision =
    | Collaborate
    | Betray
    | Unknown
    | Initial

type ProcessMessage =
  | GetDecision of input:Decision * decision:AsyncReplyChannel<Decision>
  | Kill

exception StopProcess

let startProcess id fn wd args =
  let dir = Directory.GetCurrentDirectory()

  printfn "Starting process for '%s': %s %s" id fn args
  let psi =
    ProcessStartInfo(FileName=fn, WorkingDirectory=Path.Combine(Array.append [| dir |] wd),
      Arguments=args, UseShellExecute=false, RedirectStandardOutput=true, RedirectStandardInput=true)
  let ps = System.Diagnostics.Process.Start(psi)

  MailboxProcessor.Start(fun inbox -> async {
    try
      while true do
        let! msg = inbox.Receive()
        match msg with
        | Kill ->
            printfn "Stopping process '%s'" id
            ps.Kill()
            raise StopProcess
        | GetDecision(input, response) ->
//            printfn "Prisoner coming up with decision in process '%s'" id
            let strInput =
                match input with
                | Collaborate -> "C"
                | Betray -> "B"
                | Initial -> "0"
                | Unknown -> "C" // TODO: change this?
            ps.StandardInput.WriteLine(strInput)
            let decision =
              [| let mutable line = ""
                 while (line <- ps.StandardOutput.ReadLine(); line <> "") do
                   yield line |]
            match decision.[0] with
            | "C" | "c" -> response.Reply Collaborate
            | "B" | "b" -> response.Reply Betray
            | _ -> response.Reply Unknown
    with
    | StopProcess -> ()
    | e -> printfn "PROCESS FAILED: %A" e })


let runCompetition nIter pythonSource1 pythonSource2 =
    async {
        let s1 =  startProcess pythonSource1 "python3" [|"."|] pythonSource1
        let s2 =  startProcess pythonSource2 "python3" [|"."|] pythonSource2

        let mutable o1 = Initial
        let mutable o2 = Initial
        let mutable score1 = 0
        let mutable score2 = 0
        for i in 1 .. nIter do
            let! a1 = s1.PostAndAsyncReply(fun repl -> GetDecision(o2, repl))
            let! a2 = s2.PostAndAsyncReply(fun repl -> GetDecision(o1, repl))
            o1 <- a1
            o2 <- a2
            let gain1, gain2 =
              match a1, a2 with
              | Collaborate, Collaborate -> 2, 2
              | Collaborate, Betray -> 0, 3
              | Betray, Collaborate -> 3, 0
              | Betray, Betray -> 1, 1
              | _, _ -> 0,0
            score1 <- score1 + gain1
            score2 <- score2 + gain2

        s1.Post(Kill)
        s2.Post(Kill)

        return score1, score2
    }

let playAll strategies =
  let timeout = 10000
  let iterations = 100
  let results =
      strategies |> Seq.map (fun s1 ->
        strategies |> Seq.map (fun s2 ->
          let result = Async.RunSynchronously(runCompetition iterations s1 s2, timeout)
          s1, s2, result ) |> Array.ofSeq) |> Array.ofSeq
  results

let stats (results: (string * string * (int * int)) array array) =
    results
    |> Array.concat
    |> Array.groupBy (fun (s1, s2, r) -> s1)
    |> Array.map (fun (s1, res) -> s1, res |> Array.map (fun (_, s2, x) -> s2, x))
    |> Array.map (fun (s, res) ->
        let total = res |> Array.sumBy (fun (_, (x, _)) -> x)
        let wins = res |> Array.filter (fun (_, (x1, x2)) -> x1 > x2) |> Array.length
        let losses = res |> Array.filter (fun (_, (x1, x2)) -> x1 < x2) |> Array.length
        let draws = res |> Array.filter (fun (_, (x1, x2)) -> x1 = x2) |> Array.length
        s, total, wins, draws, losses )
    |> Array.sortByDescending (fun (_, x, _, _, _) -> x)

let resultLookup (results: (string * string * (int * int)) array array) =
    results
    |> Array.concat
    |> Array.map (fun (s1, s2, x) -> (s1, s2), x)
