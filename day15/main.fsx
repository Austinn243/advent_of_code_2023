/// Advent of Code Day 15
/// Lens Library
/// https://adventofcode.com/2023/day/15


/// Hash a string using the algorithm described in the problem.
let hash (str: string) : int =
    let reducer =
        fun (acc: int) (c: char) -> acc |> (+) (int c) |> (*) 17 |> fun x -> x % 256

    Seq.fold reducer 0 str


/// Determine the box number for a given lens.
let boxNumber (label: string) : int = hash label

type Step = string

/// A lens with a label and a focal length.
module Lens =
    type T = { label: string; focalLength: int }

    let focusingPower (lens: T) (boxNumber: int) (slotNumber: int) : int =
        (boxNumber + 1)
        * (slotNumber + 1)
        * lens.focalLength

/// A box containing a collection of lenses.
module Box =
    type T = Lens.T list

    /// Calculate the total focusing power of the lenses in the box.
    let focusingPower (box: T) (boxNumber: int) : int =
        box
        |> List.mapi (fun i lens -> Lens.focusingPower lens boxNumber i)
        |> List.sum

/// A collection of numbered boxes, each containing a collection of lenses.
module Boxes =
    type T = Map<int, Box.T>

    /// Create a map of box numbers to empty boxes.
    let create () : T =
        [ 0 .. 255 ]
        |> List.map (fun i -> (i, []))
        |> Map.ofList

    /// Calculate the total focusing power of the lenses in the boxes.
    let focusingPower (boxes: T) : int =
        boxes
        |> Map.fold (fun acc boxNumber box -> acc + Box.focusingPower box boxNumber) 0

/// Operations that can be performed on the boxes.
module Operation =
    open System.Text.RegularExpressions

    let private StepRegex = Regex("^([a-z]+)([-=])(\d*)$")

    type T =
        | Remove of string
        | Insert of Lens.T

    /// Convert a character to a digit.
    let private toDigit (c: char) : int = int c - int '0'

    /// Interpret a step as an operation.
    let interpret (step: string) : T =
        let matches = StepRegex.Match(step)
        let label = matches.Groups.[1].Value
        let operation = matches.Groups.[2].Value

        match operation with
        | "-" -> Remove label
        | "=" ->
            Insert
                { label = label
                  focalLength = toDigit matches.Groups.[3].Value.[0] }
        | _ -> failwith "Invalid operation"

    /// Determine if the given lens has the given label.
    let private hasLabel (label: string) (lens: Lens.T) : bool = lens.label = label

    /// Remove the lens with the given label from the boxes if it exists.
    let private remove (boxes: Boxes.T) (label: string) : Boxes.T =
        let boxNumber = boxNumber label
        let box = boxes.[boxNumber]

        match List.tryFindIndex (hasLabel label) box with
        | None -> boxes
        | Some index ->
            let newBox = List.removeAt index box
            boxes |> Map.add boxNumber newBox

    /// Insert the given lens into the boxes at the start of the box with the given label.
    /// If a lens with the same label already exists, it is replaced in-place.
    let private insert (boxes: Boxes.T) (lens: Lens.T) : Boxes.T =
        let boxNumber = boxNumber lens.label
        let box = boxes.[boxNumber]

        let newBox =
            match List.tryFindIndex (hasLabel lens.label) box with
            | None -> box @ [ lens ]
            | Some index ->
                box
                |> List.insertAt index lens
                |> List.removeAt (index + 1)

        boxes |> Map.add boxNumber newBox

    /// Execute the given operation on the given boxes, returning the updated boxes.
    let execute (boxes: Boxes.T) (operation: T) : Boxes.T =
        match operation with
        | Remove label -> remove boxes label
        | Insert lens -> insert boxes lens

    /// Execute the given operations on the given boxes, returning the updated boxes.
    let executeAll (operations: T array) (boxes: Boxes.T) : Boxes.T = Array.fold execute boxes operations


module Main =
    open System.IO

    let INPUT_FILE = "input.txt"

    let INPUT_PATH =
        Path.Combine(__SOURCE_DIRECTORY__, INPUT_FILE)

    /// Read the input file as a list of steps.
    let readInput (path: string) : Step array =
        path
        |> File.ReadAllText
        |> fun s -> s.Split ','
        |> Array.map (fun s -> s.Trim())


    /// Hash each step and sum the results, printing the total sum to the console.
    let evaluateStepHashSum (steps: Step array) : unit =
        steps
        |> Array.map hash
        |> Array.sum
        |> printfn "Result: %A"

    let main () =
        let steps = readInput INPUT_PATH
        evaluateStepHashSum steps

        let operations = steps |> Array.map Operation.interpret

        let boxes =
            Boxes.create () |> Operation.executeAll operations

        let totalFocusingPower = Boxes.focusingPower boxes
        printfn "Total focusing power: %A" totalFocusingPower

    main ()
