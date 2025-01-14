/// Advent of Code 2023, Day 17
/// Clumsy Crucible
/// https://adventofcode.com/2023/day/17

module Position =
    type T = int * int

module Direction =
    type T =
        | North
        | East
        | South
        | West

    let toTuple (direction: T) : (int * int) =
        match direction with
        | North -> (-1, 0)
        | East -> (0, 1)
        | South -> (1, 0)
        | West -> (0, -1)

    let turnLeft (direction: T) : T =
        match direction with
        | North -> West
        | East -> North
        | South -> East
        | West -> South

    let turnRight (direction: T) : T =
        match direction with
        | North -> East
        | East -> South
        | South -> West
        | West -> North

module HeatCostMap =
    type T = int list list

    let MAX_CONSECUTIVE_STRAIGHT_MOVES = 3

    let private createMemo (heatCostMap: T) : int option array2d =
        let rowCount = heatCostMap.Length
        let columnCount = heatCostMap.[0].Length

        let destinationRow = rowCount - 1
        let destinationColumn = columnCount - 1

        let memo = Array2D.create rowCount columnCount None
        memo.[destinationRow, destinationColumn] <- Some heatCostMap.[destinationRow].[destinationColumn]

        memo

    type private Step = Position.T * Direction.T * int

    let private withinBounds (heatCostMap: T) (position: Position.T) : bool =
        let row, col = position

        row >= 0
        && row < heatCostMap.Length
        && col >= 0
        && col < heatCostMap.[0].Length

    let private getNextSteps (heatCostMap: T) (visited: Position.T Set) (step: Step) : Step list =
        let position, direction, consecutiveStraightMoves = step
        let row, col = position

        let mutable nextSteps =
            [ (Direction.turnLeft direction, 1)
              (Direction.turnRight direction, 1) ]

        if consecutiveStraightMoves < MAX_CONSECUTIVE_STRAIGHT_MOVES then
            nextSteps <-
                (direction, consecutiveStraightMoves + 1)
                :: nextSteps

        let nextSteps =
            nextSteps
            |> List.map (fun (direction, consecutiveStraightMoves) ->
                let rowDelta, colDelta = Direction.toTuple direction

                let nextRow = row + rowDelta
                let nextCol = col + colDelta
                let nextPosition = (nextRow, nextCol)

                (nextPosition, direction, consecutiveStraightMoves))
            |> List.filter (fun (position, _, _) ->
                withinBounds heatCostMap position
                && not (Set.contains position visited))

        nextSteps

    let minimumHeatLoss (heatCostMap: T) : int =
        let memo = createMemo heatCostMap

        let rec visit (visited: Position.T Set) (currentStep: Step) : int =
            let position, _, _ = currentStep
            let row, col = position

            match memo.[row, col] with
            | Some minimumStepCost -> minimumStepCost
            | None ->
                let visited = Set.add position visited
                let heatCost = heatCostMap.[row].[col]

                let nextSteps =
                    getNextSteps heatCostMap visited currentStep

                if nextSteps = [] then
                    memo.[row, col] <- Some heatCost
                    heatCost
                else
                    let stepCosts = nextSteps |> List.map (visit visited)

                    let minimumStepCost = stepCosts |> List.min |> (+) heatCost

                    memo.[row, col] <- Some minimumStepCost

                    minimumStepCost

        let visited = Set.empty

        let startingSteps =
            [ ((0, 1), Direction.East, 1)
              ((1, 0), Direction.South, 1) ]

        let minimumHeatLoss =
            startingSteps
            |> List.map (visit visited)
            |> List.min

        minimumHeatLoss



module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    let readInput (filename: string) : HeatCostMap.T =
        let parseLine (line: string) : int list =
            line
            |> Seq.map (fun ch -> int ch - int '0')
            |> List.ofSeq

        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        path
        |> File.ReadLines
        |> Seq.map parseLine
        |> List.ofSeq

    let run (filename: string) : unit =
        let heatCostMap = readInput filename
        printfn "%A" heatCostMap

        let minimumHeatLoss = HeatCostMap.minimumHeatLoss heatCostMap
        printfn "The minimum heat loss is %d." minimumHeatLoss

    let main () =
        [
          //
          ("Using the test input.", TEST_FILE)
        //   ("Using the real input.", INPUT_FILE)
        //
         ]
        |> List.iter (fun (description, filename) ->
            printfn "%s" description
            run filename
            printfn "")

    main ()
