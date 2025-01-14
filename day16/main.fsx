/// Advent of Code 2023, Day 16
/// The Floor Will Be Lava
/// https://adventofcode.com/2023/day/16


/// Represents a position on the contraption.
/// The first element is the row, and the second element is the column.
type Position = (int * int)

/// Represents a direction of travel on the contraption.
module Direction =
    type T =
        | North
        | South
        | East
        | West

    /// Convert a direction to a tuple.
    let toTuple (direction: T) : (int * int) =
        match direction with
        | North -> (-1, 0)
        | South -> (1, 0)
        | East -> (0, 1)
        | West -> (0, -1)

/// Represents a tile on the contraption.
module Tile =
    type T =
        | Empty
        | ForwardMirror
        | BackwardMirror
        | HorizontalSplitter
        | VerticalSplitter

    /// Convert a character to a tile.
    let ofChar (c: char) : T =
        match c with
        | '.' -> Empty
        | '/' -> ForwardMirror
        | '\\' -> BackwardMirror
        | '-' -> HorizontalSplitter
        | '|' -> VerticalSplitter
        | _ -> failwith "Invalid tile character."

    /// Determine the next possible traversal options from the given position and direction
    /// when the light passes through a tile.
    let private passThrough (position: Position) (direction: Direction.T) : (Position * Direction.T) list =
        let (row, col) = position
        let (drow, dcol) = Direction.toTuple direction

        [ (row + drow, col + dcol), direction ]

    /// Determine the next possible traversal options from the given position and direction
    /// when the light reflects off a forward-facing mirror.
    let private reflectForwardMirror (position: Position) (direction: Direction.T) : (Position * Direction.T) list =
        let (row, col) = position

        match direction with
        | Direction.North -> [ (row, col + 1), Direction.East ]
        | Direction.South -> [ (row, col - 1), Direction.West ]
        | Direction.East -> [ (row - 1, col), Direction.North ]
        | Direction.West -> [ (row + 1, col), Direction.South ]

    /// Determine the next possible traversal options from the given position and direction
    /// when the light reflects off a backward-facing mirror.
    let private reflectBackwardMirror (position: Position) (direction: Direction.T) : (Position * Direction.T) list =
        let (row, col) = position

        match direction with
        | Direction.North -> [ (row, col - 1), Direction.West ]
        | Direction.South -> [ (row, col + 1), Direction.East ]
        | Direction.East -> [ (row + 1, col), Direction.South ]
        | Direction.West -> [ (row - 1, col), Direction.North ]

    /// Determine the next possible traversal options from the given position and direction
    /// when the light hits a horizontal splitter.
    let private splitHorizontal (position: Position) (direction: Direction.T) : (Position * Direction.T) list =
        if direction = Direction.North
           || direction = Direction.South then
            let (row, col) = position

            [ (row, col + 1), Direction.East
              (row, col - 1), Direction.West ]
        else
            passThrough position direction

    /// Determine the next possible traversal options from the given position and direction
    /// when the light hits a vertical splitter.
    let private splitVertical (position: Position) (direction: Direction.T) : (Position * Direction.T) list =
        if direction = Direction.East
           || direction = Direction.West then
            let (row, col) = position

            [ (row - 1, col), Direction.North
              (row + 1, col), Direction.South ]
        else
            passThrough position direction

    /// Determine the next possible traversal options from the given position, direction, and tile.
    let nextTraversals (position: Position) (direction: Direction.T) (tile: T) : (Position * Direction.T) list =
        match tile with
        | Empty -> passThrough position direction
        | ForwardMirror -> reflectForwardMirror position direction
        | BackwardMirror -> reflectBackwardMirror position direction
        | VerticalSplitter -> splitVertical position direction
        | HorizontalSplitter -> splitHorizontal position direction

/// Represents a contraption of tiles.
module Contraption =
    type T = Tile.T array2d

    /// Determine whether the given position is within the bounds of the contraption.
    let withinBounds (position: Position) (contraption: T) : bool =
        let rowCount = Array2D.length1 contraption
        let colCount = Array2D.length2 contraption

        let (row, col) = position

        row >= 0
        && row < rowCount
        && col >= 0
        && col < colCount

    /// Determine the next possible traversal options from the given position and direction.
    let nextTraversals (position: Position) (direction: Direction.T) (contraption: T) : (Position * Direction.T) list =
        let (row, col) = position
        let tile = contraption.[row, col]

        let traversals =
            Tile.nextTraversals position direction tile

        traversals
        |> List.filter (fun (position, _) -> withinBounds position contraption)

    /// Determine which tiles will be energized by the given contraption when
    /// the light starts at the given position and direction.
    let countEnergized (contraption: T) (startPosition: Position) (startDirection: Direction.T) : int =
        let rowCount = Array2D.length1 contraption
        let colCount = Array2D.length2 contraption

        let energized =
            Array2D.create rowCount colCount Set.empty

        let mutable traversals = [ (startPosition, startDirection) ]

        while traversals.Length > 0 do
            let traversal = List.head traversals
            traversals <- List.tail traversals

            let (position, direction) = traversal
            let (row, col) = position

            if Set.contains direction energized.[row, col] then
                ()
            else
                energized.[row, col] <- Set.add direction energized.[row, col]

                let nextTraversals =
                    nextTraversals position direction contraption

                traversals <- traversals @ nextTraversals

        let mutable count = 0

        energized
        |> Array2D.iter (fun directions ->
            if not (Set.isEmpty directions) then
                count <- count + 1)

        count

    /// Determine the maximum number of tiles that can be energized by a contraption
    /// for any given starting position and direction.
    let countMaxEnergized (contraption: T) : int =
        let rowCount = Array2D.length1 contraption
        let colCount = Array2D.length2 contraption

        let mutable allStarts = []

        for row in 0 .. rowCount - 1 do
            allStarts <-
                allStarts
                @ [ ((row, 0), Direction.East)
                    ((row, colCount - 1), Direction.West) ]

        for col in 0 .. colCount - 1 do
            allStarts <-
                allStarts
                @ [ ((0, col), Direction.South)
                    ((rowCount - 1, col), Direction.North) ]

        let allEnergized =
            allStarts
            |> List.map (fun (position, direction) -> async { return countEnergized contraption position direction })
            |> Async.Parallel

        allEnergized
        |> Async.RunSynchronously
        |> Array.max

/// The main entry point of the program.
module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    /// Read information about the contraption from the given file.
    let readInput (path: string) : Contraption.T =
        let tiles =
            path
            |> File.ReadLines
            |> Seq.map (fun line -> line.ToCharArray() |> Array.ofSeq)
            |> Seq.toArray

        Array2D.init tiles.Length tiles.[0].Length (fun row col -> Tile.ofChar tiles.[row].[col])

    /// Run the program with the given input file.
    let main (args: string []) : int =
        let filename =
            if args.Length > 0 then
                args.[0]
            else
                INPUT_FILE

        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        let contraption = readInput path
        let startPosition = (0, 0)

        let energized =
            Contraption.countEnergized contraption startPosition Direction.East

        let maxEnergized =
            Contraption.countMaxEnergized contraption

        printfn "Part 1: %d" energized
        printfn "Part 2: %d" maxEnergized

        0

    [<EntryPoint>]
    main [||]
