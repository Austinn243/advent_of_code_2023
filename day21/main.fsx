/// Advent of Code 2023, Day 21
/// Step Counter
/// https://adventofcode.com/2023/day/21

// A key insight is that, for any plot in the garden, that plot is reachable
// in N steps if both N and the distance from the start to that plot are the
// same parity. That is, if N is even, then any plot that is an even distance
// from the start is reachable in N steps and vice versa. This ensures that
// we only need to visit each plot once and that we can stop visiting plots
// once we've reached the target number of steps.

type Range = int * int

/// Represents a position in the garden.
module Position =
    type T = int * int

    /// Get the neighbors of a position.
    let neighbors (position: T) : T list =
        let row, col = position

        [ (row - 1, col)
          (row + 1, col)
          (row, col - 1)
          (row, col + 1) ]

    /// Wrap a value around the given lower and upper bounds.
    let private wrapValue (value: int) (lower: int) (upper: int) : int =
        let modulus = upper - lower + 1

        if value > upper then
            value % modulus
        elif value < lower then
            // 11 plots
            // -1 -> 10
            // -2 -> 9
            // -3 -> 8
            // -4 -> 7
            // -5 -> 6
            // -6 -> 5
            // -7 -> 4
            // -8 -> 3
            // -9 -> 2
            // -10 -> 1
            // -11 -> 0
            // -12 -> 11

            let modulated = value % modulus
            // printfn "Modulated: %d" modulated

            if modulated = 0 then
                upper
            else
                upper + modulated + 1
        else
            value

    /// Wrap a position around the given row and column bounds.
    let wrap (rows: Range) (cols: Range) (position: T) : T =
        let row, col = position
        let lowerRow, upperRow = rows
        let lowerCol, upperCol = cols

        let row = wrapValue row lowerRow upperRow
        let col = wrapValue col lowerCol upperCol

        (row, col)



/// Represents a plot in the garden.
module Plot =
    type T =
        | Start
        | Open
        | Rock

    /// Convert a character to a plot.
    let ofChar (ch: char) : T =
        match ch with
        | '.' -> Open
        | '#' -> Rock
        | 'S' -> Start
        | _ -> failwith "Invalid plot"

    /// Determine if a plot is visitable.
    let visitable (plot: T) : bool =
        match plot with
        | Start -> true
        | Open -> true
        | _ -> false

/// Represents a garden.
module Garden =
    type T = Plot.T list list

    /// Locate the starting position in the garden.
    let locateStart (garden: T) : Position.T =
        let rowCount = garden.Length
        let colCount = garden.[0].Length

        let rec visit (position: Position.T) : Position.T =
            let row, col = position
            let plot = garden.[row].[col]

            match plot with
            | Plot.Start -> position
            | _ when col < colCount - 1 -> visit (row, col + 1)
            | _ when row < rowCount - 1 -> visit (row + 1, 0)
            | _ -> failwith "Start not found"

        visit (0, 0)


/// Represents a garden with finite bounds.
module FiniteGarden =

    /// Determine if a position is within the bounds of a garden.
    let private withinBounds (garden: Garden.T) (position: Position.T) : bool =
        let row, col = position
        let rowCount = garden.Length
        let colCount = garden.[0].Length

        row >= 0
        && row < rowCount
        && col >= 0
        && col < colCount

    let private getNeighbors (garden: Garden.T) (position: Position.T) : Position.T list =
        let neighbors = Position.neighbors position

        let neighbors =
            neighbors
            |> List.filter (withinBounds garden)
            |> List.filter (fun (row, col) -> Plot.visitable garden.[row].[col])

        neighbors

    let locatePositionsAtStepDistance (garden: Garden.T) (steps: int) : Position.T Set =
        let start = Garden.locateStart garden
        let targetRemainder = steps % 2

        let rec visit
            (queue: (Position.T * int) list)
            (visited: Position.T Set)
            (targets: Position.T Set)
            : Position.T Set =
            match queue with
            | [] -> targets
            | (position, _) :: rest when Set.contains position visited -> visit rest visited targets
            | (_, distance) :: rest when distance > steps -> visit rest visited targets
            | (position, distance) :: rest ->
                let newVisited = Set.add position visited

                let targets =
                    if distance % 2 = targetRemainder then
                        Set.add position targets
                    else
                        targets

                let neighbors = getNeighbors garden position

                let newPlots =
                    neighbors
                    |> List.map (fun neighbor -> (neighbor, distance + 1))

                let newQueue = rest @ newPlots

                visit newQueue newVisited targets

        let queue = [ (start, 0) ]
        let visited = Set.empty
        let targets = Set.empty

        visit queue visited targets


/// Represents a garden that repeats infinitely in all directions.
module InfiniteGarden =
    /// Translate a position to a local position within the bounds of the garden.
    let private toLocalPosition (garden: Garden.T) (position: Position.T) : Position.T =
        let rowCount = garden.Length
        let colCount = garden.[0].Length

        let rowRange = 0, rowCount - 1
        let colRange = 0, colCount - 1

        let localPosition = Position.wrap rowRange colRange position

        localPosition

    /// Get the neighbors of a position.
    let private getNeighbors (garden: Garden.T) (position: Position.T) : Position.T list =
        let neighbors = Position.neighbors position

        let neighbors =
            neighbors
            |> List.filter (fun neighbor ->
                let row, col = toLocalPosition garden neighbor
                let plot = garden.[row].[col]

                Plot.visitable plot)

        neighbors

    /// Locate all positions in the garden that are reachable in a given number of steps.
    let locatePositionsAtStepDistance (garden: Garden.T) (steps: int) : int =
        let start = Garden.locateStart garden
        let targetRemainder = steps % 2

        let mutable visited = Set.empty
        let mutable targets = 0
        let mutable queue = Set.singleton (start, 0)

        let rec visit (step: Position.T * int) : (Position.T * int) list =
            let position, distance = step
            let wasVisited = Set.contains position visited
            let withinSteppingDistance = distance <= steps
            let shouldVisit = not wasVisited && withinSteppingDistance

            if shouldVisit then
                visited <- Set.add position visited

                if distance % 2 = targetRemainder then
                    targets <- targets + 1

                let neighbors = getNeighbors garden position

                let nextSteps =
                    neighbors
                    |> List.map (fun neighbor -> (neighbor, distance + 1))

                nextSteps
            else
                []

        while queue <> Set.empty do
            let nextSteps =
                queue
                |> Seq.map (fun step -> async { return visit step })
                |> Async.Parallel
                |> Async.RunSynchronously
                |> List.concat
                |> Set.ofList

            printfn "Next Steps: %A" nextSteps
            queue <- nextSteps

        targets



/// Run the program.
module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    /// Read the input file into a garden.
    let readInput (filename: string) : Garden.T =
        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        path
        |> File.ReadLines
        |> Seq.map (fun line -> line |> Seq.map Plot.ofChar |> Seq.toList)
        |> Seq.toList

    /// Run the test cases.
    let test () : unit =
        let garden = readInput TEST_FILE

        let stepLocationsInFiniteGarden =
            FiniteGarden.locatePositionsAtStepDistance garden 6

        printfn "Step locations: %A" stepLocationsInFiniteGarden
        printfn "Step location count: %d" stepLocationsInFiniteGarden.Count

        let positions =
            [ (0, 64)
              (0, 129)
              (0, -1)
              (0, -66)
              (64, 0)
              (129, 0)
              (-1, 0)
              (-66, 0) ]

        positions
        |> List.map (fun pos -> pos, Position.wrap (0, 63) (0, 63) pos)
        |> List.iter (fun (pos, loc) -> printfn "%A -> %A" pos loc)


        let results =
            [
              //
              6
              10
              50
              //   100
              //   500
              //   1000
              //   5000
              //
              ]
            |> List.map (fun steps -> steps, InfiniteGarden.locatePositionsAtStepDistance garden steps)
            |> List.map (fun (steps, positions) -> steps, positions)

        printfn "Results: %A" results

    /// Run the program.
    let run () : unit =
        let garden = readInput INPUT_FILE

        let stepLocationsInFiniteGarden =
            FiniteGarden.locatePositionsAtStepDistance garden 64

        printfn "Step locations: %A" stepLocationsInFiniteGarden
        printfn "Step location count: %d" stepLocationsInFiniteGarden.Count

        let stepLocationsInInfiniteGarden =
            InfiniteGarden.locatePositionsAtStepDistance garden 26501365

        ()


    let main () =
        test ()

        0

    main () |> ignore
