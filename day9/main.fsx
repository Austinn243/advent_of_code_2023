/// Advent of Code 2023, Day 9
/// Mirage Maintenance
/// https://adventofcode.com/2023/day/9


module Debug =
    /// Perform an action on a value, returning the value.
    let private tap (f: 'a -> unit) (value: 'a) : 'a =
        f value
        value

    /// Print a label and a value to the console, returning the value.
    let trace (label: string) =
        tap (fun x ->
            printfn "%s" label
            printfn "%A" x)

module History =
    type T = int seq

    /// Parse a string as a history of numbers.
    let fromString (str: string) : T = str.Split(' ') |> Seq.map int

    /// Expand a history with new histories by tracking the difference between
    /// each pair of values. Stops when the difference between all pairs is 0.
    let expand (history: T) : T list =
        let rec loop (history: T) (acc: T list) =
            let nextHistory =
                history
                |> Seq.pairwise
                |> Seq.map (fun (a, b) -> b - a)

            let histories = acc @ [ nextHistory ]
            let isAllZeroes = Seq.forall ((=) 0) nextHistory

            if isAllZeroes then
                histories
            else
                loop nextHistory histories

        loop history [ history ]

    /// Extrapolate the next value in a history by evaluating
    /// the values in the expanded histories.
    let extrapolateNext (histories: T list) : int =
        let oldestFirstHistories = histories |> List.rev |> List.tail

        let rec loop (histories: T list) (extrapolatedValue: int) : int =
            match histories with
            | [] -> extrapolatedValue
            | history :: remaining ->
                let nextValue = extrapolatedValue + Seq.last history
                loop remaining nextValue

        loop oldestFirstHistories 0

    /// Extrapolate the previous value in a history by evaluating
    /// the values in the expanded histories.
    let extrapolatePrevious (histories: T list) : int =
        let oldestFirstHistories = histories |> List.rev |> List.tail

        let rec loop (histories: T list) (extrapolatedValue: int) : int =
            match histories with
            | [] -> extrapolatedValue
            | history :: remaining ->
                let previousValue = Seq.head history - extrapolatedValue
                loop remaining previousValue

        loop oldestFirstHistories 0


module Main =
    open System.IO

    let INPUT_FILE = "input.txt"

    let INPUT_PATH =
        Path.Combine(__SOURCE_DIRECTORY__, INPUT_FILE)

    /// Read the lines of an input file, interpreting each line as a history
    /// of numbers, and return a list of histories.
    let readInput (path: string) : History.T array =
        File.ReadAllLines(path)
        |> Array.map History.fromString

    let main () =
        let histories = INPUT_PATH |> readInput

        let expandedHistories = histories |> Seq.map History.expand

        let extrapolatedNextSum =
            expandedHistories
            |> Seq.map History.extrapolateNext
            |> Seq.sum

        let extrapolatedPreviousSum =
            expandedHistories
            |> Seq.map History.extrapolatePrevious
            |> Seq.sum

        printfn "Extrapolated next sum: %d" extrapolatedNextSum
        printfn "Extrapolated previous sum: %d" extrapolatedPreviousSum

    main ()
