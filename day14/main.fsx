/// Advent of Code 2023, Day 14
/// Parabolic Reflector Dish
/// https://adventofcode.com/2023/day/14


// One approach to tilt the platform would be to iterate over the columns,
// tracking the indices where the round rocks would end up as well as how
// many round rocks are present between each index and the next. Then,
// we can create our new platform by iterating over the columns again,
// placing the specified number of round rocks at each index and filling
// the rest of the column with empty spaces.

/// Represents a position on the platform.
module Position =
    type T = int * int

module Space =
    type T =
        | Empty
        | RoundRock
        | CubeRock

    let ofChar (ch: char) : T =
        match ch with
        | '.' -> Empty
        | 'O' -> RoundRock
        | '#' -> CubeRock
        | _ -> failwith "Invalid character."

/// The platform of the parabolic reflector dish.
module Platform =
    type T = Space.T list list

    /// Calculate the load that the entity at the given position places on the platform.
    let private load (platform: T) (position: Position.T) : int =
        let row, col = position
        let ch = platform.[row].[col]

        if ch = Space.RoundRock then
            platform
            |> List.length
            |> fun length -> length - row
        else
            0

    let private tilt (row: Space.T list) : Space.T list =
        let addSpaces (count: int) (row: Space.T list) : Space.T list =
            let emptySpaces = List.init count (fun _ -> Space.Empty)
            List.append row emptySpaces

        let rec loop (remaining: Space.T list) (emptySpaceCount: int) (tiltedRow: Space.T list) : Space.T list =
            match remaining with
            | [] -> addSpaces emptySpaceCount tiltedRow
            | head :: rest ->
                match head with
                | Space.Empty -> loop rest (emptySpaceCount + 1) tiltedRow
                | Space.RoundRock -> loop rest emptySpaceCount (tiltedRow @ [ head ])
                | Space.CubeRock -> loop rest 0 ((addSpaces emptySpaceCount tiltedRow) @ [ head ])

        loop row 0 []


    /// Tilt the platform to the north, causing all round rocks to roll north.
    let tiltNorth (platform: T) : T =
        let columnCount = platform |> List.head |> List.length

        let columns =
            [ 0 .. columnCount - 1 ]
            |> List.map (fun col -> platform |> List.map (fun row -> row.[col]))
            |> List.map tilt

        let tiltedPlatform = List.transpose columns
        tiltedPlatform


    /// Calculate the total load on the platform.
    let totalLoad (platform: T) : int =
        platform
        |> List.mapi (fun rowIdx row -> row |> List.mapi (fun colIdx _ -> rowIdx, colIdx))
        |> List.concat
        |> List.sumBy (load platform)


/// The main entry point of the application.
module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    /// Read information about the platform from the given file.
    let readInput (filename: string) : Platform.T =
        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        path
        |> File.ReadLines
        |> Seq.map (Seq.map Space.ofChar)
        |> Seq.map (List.ofSeq)
        |> Seq.toList

    let run (filename: string) =
        let platform = readInput filename
        printfn "%A" platform

        platform
        |> Platform.tiltNorth
        |> Platform.totalLoad
        |> printfn "The total load on the platform is %d."

    let main () =
        [ ("Using the test input.", TEST_FILE)
          ("Using the real input.", INPUT_FILE) ]
        |> List.iter (fun (description, filename) ->
            printfn "%s" description
            run filename
            printfn "")

    main ()
