/// Advent of Code 2023, Day 13
/// Point of Incidence
/// https://adventofcode.com/2023/day/13

/// Represents a line of reflection for a given pattern.
module LineOfReflection =
    type T =
        | Vertical of int
        | Horizontal of int

    let summaryValue (lineOfReflection: T) : int =
        match lineOfReflection with
        | Vertical x -> x + 1
        | Horizontal y -> (y + 1) * 100

/// Represents a pattern of ash and rocks in a valley of mirrors.
module Pattern =
    type T = char list list

    /// Determine if splitting the pattern horizontally after the given index
    /// results in two halves that are mirror images of each other.
    let isHorizontalLineOfRelection (pattern: T) (index: int) : bool =
        let rowCount = pattern.Length
        let reflectionSize = min (index + 1) (rowCount - index - 1)

        let topHalf =
            pattern.[(index - reflectionSize + 1)..(index)]

        let bottomHalf =
            pattern.[(index + 1)..(index + reflectionSize)]

        // printfn "topHalf: %A" topHalf
        // printfn "bottomHalf: %A" bottomHalf
        // printfn ""

        let reversedBottomHalf = bottomHalf |> List.rev

        topHalf = reversedBottomHalf

    /// Determine if splitting the pattern vertically after the given index
    /// results in two halves that are mirror images of each other.
    let isVerticalLineOfRelection (pattern: T) (index: int) : bool =
        let columnCount = pattern.[0].Length

        let reflectionSize =
            min (index + 1) (columnCount - index - 1)

        let leftHalf =
            pattern
            |> List.map (fun row -> row.[index - reflectionSize + 1..index])

        let rightHalf =
            pattern
            |> List.map (fun row -> row.[index + 1..index + reflectionSize])

        let reversedRightHalf =
            rightHalf |> List.map (fun row -> row |> List.rev)

        // printfn "leftHalf: %A" leftHalf
        // printfn "rightHalf: %A" rightHalf
        // printfn ""

        leftHalf = reversedRightHalf

    /// Find the line of reflection for the given pattern if one exists.
    let findLineOfReflection (pattern: T) : LineOfReflection.T option =
        let evaluate predicate resultType index =
            async {
                if predicate pattern index then
                    return Some(resultType index)
                else
                    return None
            }

        let horizontalCandidates = [ 0 .. (pattern.Length - 2) ]
        let verticalCandidates = [ 0 .. (pattern.[0].Length - 2) ]

        let horizontalChecks =
            horizontalCandidates
            |> List.map (evaluate isHorizontalLineOfRelection LineOfReflection.Horizontal)

        let verticalChecks =
            verticalCandidates
            |> List.map (evaluate isVerticalLineOfRelection LineOfReflection.Vertical)

        let allChecks = horizontalChecks @ verticalChecks

        let result =
            allChecks
            |> Async.Choice
            |> Async.RunSynchronously

        result

    let private rockPositions (pattern: T) : (int * int) list =
        let rowCount = pattern.Length
        let columnCount = pattern.[0].Length

        let allPositions =
            List.allPairs [ 0 .. (rowCount - 1) ] [ 0 .. (columnCount - 1) ]

        let rockPositions =
            allPositions
            |> List.filter (fun (row, col) -> pattern.[row].[col] = '#')

        rockPositions

    let cleanSmudge (pattern: T) (position: (int * int)) : T =
        let (targetRow, targetCol) = position

        pattern
        |> List.mapi (fun row rowValues ->
            rowValues
            |> List.mapi (fun col value ->
                if row = targetRow && col = targetCol then
                    '.'
                else
                    value))

    let findNewLineOfReflection (pattern: T) : LineOfReflection.T =
        let evaluate position =
            async {
                let cleanPattern = cleanSmudge pattern position

                return findLineOfReflection cleanPattern
            }

        let rockPositions = rockPositions pattern

        printfn "rockPositions: %A" rockPositions

        rockPositions
        |> List.map evaluate
        |> Async.Choice
        |> Async.RunSynchronously
        |> Option.get

module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    /// Read the input file and return a list of patterns.
    let readInput (filename: string) : Pattern.T list =
        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        let rec loop (lines: string list) (patterns) (currentPattern: char list list) =
            match lines with
            | [] -> patterns @ [ currentPattern ]
            | line :: rest when line = "" -> loop rest (patterns @ [ currentPattern ]) []
            | line :: rest -> loop rest patterns (currentPattern @ [ line |> Seq.toList ])

        let lines = File.ReadAllLines path |> Array.toList

        loop lines [] []

    /// Run the program with the given input file.
    let run (filename: string) : unit =
        let patterns = readInput filename

        let linesOfReflection =
            patterns
            |> List.map Pattern.findLineOfReflection
            |> List.map Option.get

        let originalSum =
            linesOfReflection
            |> List.map LineOfReflection.summaryValue
            |> List.sum

        printfn "Result: %d" originalSum

        let newLinesOfReflection =
            patterns
            |> List.map Pattern.findNewLineOfReflection

        let newSum =
            newLinesOfReflection
            |> List.map LineOfReflection.summaryValue
            |> List.sum

        printfn "Result: %d" newSum


    let main () =
        [
          //
          //   INPUT_FILE
          TEST_FILE
        //
         ]
        |> List.iter run

    main ()
