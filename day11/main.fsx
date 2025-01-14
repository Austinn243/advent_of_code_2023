/// Advent of Code 2023, Day 11
/// Cosmic Expansion
/// https://adventofcode.com/2023/day/11

module ListExtensions =
    /// Produce a list of all distinct combinations of items in the list.
    let combinations (items: 'a list) : ('a * 'a) list =
        let rec loop (remaining: 'a list) (combinations: ('a * 'a) list) : ('a * 'a) list =
            match remaining with
            | [] -> combinations
            | [ _ ] -> combinations
            | head :: tail ->
                let combinations =
                    tail
                    |> List.map (fun item -> (head, item))
                    |> List.append combinations

                loop tail combinations

        loop items []

/// Represents a position in the galaxy map.
module Position =
    type T = int * int

    let distance (position1: T) (position2: T) : int =
        let x1, y1 = position1
        let x2, y2 = position2

        abs (x1 - x2) + abs (y1 - y2)

/// Represents a space in the galaxy map.
module Space =
    type T =
        | Empty
        | Galaxy

    /// Parse a character into a space.
    let ofChar (ch: char) : T =
        match ch with
        | '.' -> Empty
        | '#' -> Galaxy
        | _ -> failwith "Invalid space"

/// Represents a map of galaxies in the universe.
module GalaxyMap =
    type T =
        { emptyColumns: int Set option
          emptyRows: int Set option
          expansionFactor: int
          map: Space.T list list }

    let create (map: Space.T list list) (expansionFactor: int) : T =
        { emptyColumns = None
          emptyRows = None
          expansionFactor = expansionFactor
          map = map }

    /// Find the indices of the empty rows in the galaxy map.
    let private findEmptyRows (galaxyMap: T) : int Set =
        galaxyMap.map
        |> List.mapi (fun index row ->
            if not (List.contains Space.Galaxy row) then
                Some index
            else
                None)
        |> List.choose id
        |> Set.ofList

    /// Find the indices of the empty columns in the galaxy map.
    let private findEmptyColumns (galaxyMap: T) : int Set =
        let colummCount =
            galaxyMap.map |> List.head |> List.length

        let columnIndices = [ 0 .. colummCount - 1 ]

        columnIndices
        |> List.filter (fun columnIndex ->
            galaxyMap.map
            |> List.map (fun row -> List.item columnIndex row)
            |> List.contains Space.Galaxy
            |> not)
        |> Set.ofList

    /// Find the positions of all galaxies in the galaxy map.
    let private locateGalaxies (galaxyMap: T) : Position.T list =
        galaxyMap.map
        |> List.mapi (fun rowIndex row ->
            row
            |> List.mapi (fun columnIndex space ->
                match space with
                | Space.Galaxy -> Some(rowIndex, columnIndex)
                | _ -> None)
            |> List.choose id)
        |> List.concat

    /// Calculate the distance between two galaxies in the galaxy map.
    let private distance (galaxy1: Position.T) (galaxy2: Position.T) (galaxyMap: T) : int =
        let x1, y1 = galaxy1
        let x2, y2 = galaxy2

        let horizonalStepCount = abs (x1 - x2)
        let verticalStepCount = abs (y1 - y2)

        let horizontalMovement = if x1 < x2 then 1 else -1
        let verticalMovement = if y1 < y2 then 1 else -1

        let horizonalDistance =
            [ 1 .. horizonalStepCount ]
            |> List.map (fun step -> (x1 + step * horizontalMovement, y1))
            |> List.map (fun (row, _) ->
                if Set.contains row galaxyMap.emptyRows.Value then
                    galaxyMap.expansionFactor
                else
                    1)
            |> List.sum

        let verticalDistance =
            [ 1 .. verticalStepCount ]
            |> List.map (fun step -> (x1, y1 + step * verticalMovement))
            |> List.map (fun (_, column) ->
                if Set.contains column galaxyMap.emptyColumns.Value then
                    galaxyMap.expansionFactor
                else
                    1)
            |> List.sum

        horizonalDistance + verticalDistance

    /// Expand the galaxy map by doubling the size of any empty rows or columns.
    let expand (expansionFactor: int) (galaxyMap: T) : T =
        let emptyRows = findEmptyRows galaxyMap
        let emptyColumns = findEmptyColumns galaxyMap

        { emptyColumns = Some emptyColumns
          emptyRows = Some emptyRows
          expansionFactor = expansionFactor
          map = galaxyMap.map }

    /// Sum the distances between each pair of galaxies in the galaxy map.
    let sumPairDistances (galaxyMap: T) : bigint =
        let galaxyPositions = locateGalaxies galaxyMap

        galaxyPositions
        |> ListExtensions.combinations
        |> List.sumBy (fun (position1, position2) -> distance position1 position2 galaxyMap |> bigint)

module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    let readInput (filename: string) : GalaxyMap.T =
        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        let map =
            path
            |> File.ReadLines
            |> Seq.map (fun line ->
                line.ToCharArray()
                |> Seq.map Space.ofChar
                |> Seq.toList)
            |> Seq.toList

        GalaxyMap.create map 1

    let run (filename: string) : unit =
        let galaxyMap = readInput filename

        [ 1; 2; 10; 100; 1000000 ]
        |> List.map (fun expansionFactor -> GalaxyMap.expand expansionFactor galaxyMap)
        |> List.map (fun galaxyMap -> galaxyMap, GalaxyMap.sumPairDistances galaxyMap)
        |> List.iter (fun (galaxyMap, distanceSum) ->
            printfn
                "When expanding by %d, the sum of the distances between each pair of galaxies is %A"
                galaxyMap.expansionFactor
                distanceSum)

    let main () : unit =
        [ "Using Test File", TEST_FILE
          "Using Input File", INPUT_FILE ]
        |> List.iter (fun (label, filename) ->
            printfn "%s:" label
            run filename
            printfn "")

    main ()
