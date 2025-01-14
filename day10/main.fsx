/// Advent of Code 2023, Day 10
/// Pipe Maze
/// https://adventofcode.com/2023/day/10

/// Represents a position within the maze.
type Position = (int * int)

/// Represents a direction within the maze.
module Direction =
    type T =
        | North
        | East
        | South
        | West

    /// Converts the given direction to a tuple of row and column offsets.
    let private toTuple (direction: T) : Position =
        match direction with
        | North -> (-1, 0)
        | East -> (0, 1)
        | South -> (1, 0)
        | West -> (0, -1)

    /// Apply a direction to a position, returning the new position.
    let apply (direction: T) (position: Position) : Position =
        let rowOffset, colOffset = toTuple direction
        let row, col = position
        (row + rowOffset, col + colOffset)

    /// Retrieve the opposite direction.
    let opposite (direction: T) : T =
        match direction with
        | North -> South
        | East -> West
        | South -> North
        | West -> East

/// Represents a tile within the maze.
module Tile =
    type T =
        | VerticalPipe
        | HorizontalPipe
        | NorthEastPipe
        | NorthWestPipe
        | SouthWestPipe
        | SouthEastPipe
        | Ground
        override this.ToString() =
            match this with
            | VerticalPipe -> "|"
            | HorizontalPipe -> "-"
            | NorthEastPipe -> "L"
            | NorthWestPipe -> "J"
            | SouthWestPipe -> "7"
            | SouthEastPipe -> "F"
            | Ground -> "."

    /// Convert a char to a tile.
    let ofChar (char: char) : T =
        match char with
        | '|' -> VerticalPipe
        | '-' -> HorizontalPipe
        | 'L' -> NorthEastPipe
        | 'J' -> NorthWestPipe
        | '7' -> SouthWestPipe
        | 'F' -> SouthEastPipe
        | '.' -> Ground
        | _ -> failwith "Invalid tile"

    /// Retrieve the directions in which this tile is connected.
    let connections (tile: T) : Direction.T list =
        match tile with
        | VerticalPipe -> [ Direction.North; Direction.South ]
        | HorizontalPipe -> [ Direction.East; Direction.West ]
        | NorthEastPipe -> [ Direction.North; Direction.East ]
        | NorthWestPipe -> [ Direction.North; Direction.West ]
        | SouthWestPipe -> [ Direction.South; Direction.West ]
        | SouthEastPipe -> [ Direction.South; Direction.East ]
        | Ground -> []


/// Represents a maze.
module Maze =
    type T = Tile.T list list

    /// The character used to indicate the start position.
    let START_CHAR = 'S'

    /// Determine the tile type of the start pipe.
    let private determineStartPipe (chars: char list list) (position: Position) : Tile.T =
        let getTile position =
            let row, col = position

            if row >= 0
               && row < chars.Length
               && col >= 0
               && col < chars.[0].Length then
                Tile.ofChar chars.[row].[col]
            else
                Tile.Ground

        let isConnected direction =
            position
            |> Direction.apply direction
            |> getTile
            |> Tile.connections
            |> List.contains (Direction.opposite direction)

        let connections =
            [ Direction.North
              Direction.East
              Direction.South
              Direction.West ]
            |> List.filter isConnected

        match connections with
        | [ Direction.North; Direction.South ] -> Tile.VerticalPipe
        | [ Direction.East; Direction.West ] -> Tile.HorizontalPipe
        | [ Direction.North; Direction.East ] -> Tile.NorthEastPipe
        | [ Direction.North; Direction.West ] -> Tile.NorthWestPipe
        | [ Direction.South; Direction.West ] -> Tile.SouthWestPipe
        | [ Direction.East; Direction.South ] -> Tile.SouthEastPipe
        | _ -> failwith "Invalid start pipe"

    /// Parse the tile at the given position.
    let private parseCharAtPosition (chars: char list list) (position: Position) : Tile.T =
        let row, col = position
        let ch = chars.[row].[col]

        if ch <> START_CHAR then
            Tile.ofChar ch
        else
            determineStartPipe chars position

    /// Parse the given 2d list of chars into a maze, returning the maze and the start position.
    let ofChars (chars: char list list) : (T * Position) =
        let rowCount = chars.Length
        let colCount = chars.[0].Length

        let startPosition =
            List.allPairs [ 0 .. rowCount - 1 ] [ 0 .. colCount - 1 ]
            |> List.find (fun (row, col) -> chars.[row].[col] = START_CHAR)

        let maze =
            List.init rowCount (fun row -> List.init colCount (fun col -> parseCharAtPosition chars (row, col)))

        (maze, startPosition)

    /// Get the furthest distance from the given start position.
    let getFurthestDistance (maze: T) (startPosition: Position) : int =
        let rec loop (queue: (Position * int) list) (visited: Position Set) : int =
            match queue with
            | [] -> 0
            | (position, distance) :: _ when Set.contains position visited -> distance
            | (position, distance) :: remainingQueue ->
                let updatedVisited = Set.add position visited
                let newDistance = distance + 1

                let row, col = position
                let tile = maze.[row].[col]
                let connections = Tile.connections tile

                let nextPositions =
                    connections
                    |> List.map (fun direction -> (Direction.apply direction position, newDistance))
                    |> List.filter (fun (position, _) -> not (Set.contains position updatedVisited))

                let updatedQueue = remainingQueue @ nextPositions

                loop updatedQueue updatedVisited

        loop [ (startPosition, 0) ] Set.empty

    /// Print the given maze.
    let print (maze: T) : unit =
        maze
        |> List.map (fun row -> row |> List.map (fun tile -> tile.ToString()))
        |> List.iter (fun row -> printfn "%A" row)

/// Main entry point.
module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE_1 = "test1.txt"
    let TEST_FILE_2 = "test2.txt"

    /// Read input from the given file and return it as a 2d list of chars.
    let readInput (filename: string) =
        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        path
        |> File.ReadLines
        |> Seq.map List.ofSeq
        |> Seq.toList

    /// Run the program using input from the given file.
    let run (filename: string) : unit =
        let input = readInput filename
        let maze, startPosition = Maze.ofChars input

        let furthestDistance =
            Maze.getFurthestDistance maze startPosition

        Maze.print maze
        printfn "Furthest distance: %d" furthestDistance


    let main () =
        let inputs =
            [
              //
              //   TEST_FILE_1
              //   TEST_FILE_2
              INPUT_FILE
            //
             ]

        inputs |> List.iter run

    main ()
