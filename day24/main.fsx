/// Advent of Code 2023, Day 24
/// Never Tell Me The Odds
/// https://adventofcode.com/2023/day/24

module ListEx =
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

module Position =
    type T = { X: float; Y: float; Z: float }

module Velocity =
    type T = { X: float; Y: float; Z: float }

module Hailstone =
    open System.Text.RegularExpressions

    let HAILSTONE_REGEX =
        Regex(@"^(\d+), (\d+), (\d+) @ ([\s*|-]\d+), ([\s*|-]\d+), ([\s*|-]\d+)$")

    type T =
        { Position: Position.T
          Velocity: Velocity.T }

    let ofString (str: string) : T =
        let regex_match = HAILSTONE_REGEX.Match(str)

        if regex_match.Success then
            // NOTE: We skip the first group because it's the entire match.
            // We are only interested in the capture groups as they represent
            // the hailstone's position and velocity.
            let values =
                regex_match.Groups
                |> Seq.skip 1
                |> Seq.map (fun g -> g.Value)
                |> Seq.map int
                |> Seq.toList

            match values with
            | [ px; py; pz; vx; vy; vz ] ->
                { Position = { X = px; Y = py; Z = pz }
                  Velocity = { X = vx; Y = vy; Z = vz } }
            | _ -> failwith "Invalid hailstone string"
        else
            failwith "Invalid hailstone string"

    let positionAtTime (time: float) (hailstone: T) : Position.T =
        let position = hailstone.Position
        let velocity = hailstone.Velocity

        { X = position.X + velocity.X * time
          Y = position.Y + velocity.Y * time
          Z = position.Z + velocity.Z * time }

module HailstonePhysics2D =
    open Hailstone

    // FIXME: This implementation does not yield the correct results.

    /// Find the intersection point between two hailstones in 2D space.
    /// Returns None if the hailstones do not intersect.
    let intersect (hailstone1: T) (hailstone2: T) : Position.T option =
        let px1, py1 =
            hailstone1.Position.X, hailstone1.Position.Y

        let px2, py2 =
            hailstone2.Position.X, hailstone2.Position.Y

        let vx1, vy1 =
            hailstone1.Velocity.X, hailstone1.Velocity.Y

        let vx2, vy2 =
            hailstone2.Velocity.X, hailstone2.Velocity.Y

        let dx = px1 - px2
        let dy = py1 - py2
        let dvx = vx1 - vx2
        let dvy = vy1 - vy2

        if dvx = 0 && dvy = 0 then
            None
        else
            let t_x = dx / dvx
            let t_y = dy / dvy

            if t_x = t_y then
                Some(positionAtTime t_x hailstone1)
            else
                None


module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    let readInput (filename: string) : Hailstone.T list =
        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        path
        |> File.ReadLines
        |> Seq.map Hailstone.ofString
        |> Seq.toList

    let run (filename: string) : unit =
        let hailstones = readInput filename

        hailstones |> List.iter (printfn "Hailstone: %A")

        hailstones
        |> ListEx.combinations
        |> List.iter (fun (h1, h2) ->
            match HailstonePhysics2D.intersect h1 h2 with
            | Some position -> printfn "Intersect: %A" position
            | None -> printfn "No intersect")

    let main () =
        [
          //
          //   ("Using Input File:", INPUT_FILE)
          ("Using Test File:", TEST_FILE)
        //
         ]
        |> List.iter (fun (label, filename) ->
            printfn "%s" label
            run filename
            printfn "")

    main ()
