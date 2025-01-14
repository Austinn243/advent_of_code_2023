/// Advent of Code 2023, Day 19
/// Aplenty
/// https://adventofcode.com/2023/day/19


/// Represents a part to be graded by the workflows.
module Part =
    open System.Text.RegularExpressions

    let PART_REGEX =
        Regex(@"^{x=(\d+),m=(\d+),a=(\d+),s=(\d+)}$")

    type T =
        { Coolness: int
          Musicality: int
          Aerodynamicality: int
          Shininess: int }

    /// Create a part from a string.
    let ofString (str: string) : T =
        let regex_match = PART_REGEX.Match(str)

        if regex_match.Success then
            // NOTE: We skip the first group because it's the entire match.
            // We are only interested in the capture groups as they represent
            // the part's attributes.
            let values =
                regex_match.Groups
                |> Seq.skip 1
                |> Seq.map (fun g -> g.Value)
                |> Seq.map int
                |> Seq.toList

            match values with
            | [ x; m; a; s ] ->
                { Coolness = x
                  Musicality = m
                  Aerodynamicality = a
                  Shininess = s }
            | _ -> failwith "Invalid part string"
        else
            failwith "Invalid part string"

    /// Get the total rating of a part.
    let totalRating (part: T) : int =
        part.Coolness
        + part.Musicality
        + part.Aerodynamicality
        + part.Shininess

    /// Get the rating of a part for a given attribute.
    let getAttribute (attribute: string) (part: T) : int =
        match attribute with
        | "x" -> part.Coolness
        | "m" -> part.Musicality
        | "a" -> part.Aerodynamicality
        | "s" -> part.Shininess
        | _ -> failwith "Invalid attribute"

/// Represents the status of a part after being graded by a workflow.
module PartStatus =
    type T =
        | Accepted
        | Rejected

/// Identifies a workflow.
type WorkflowID = string

// A Step represents a single operation in a workflow.
// A step performs a check on a part and results in one of several outcomes:
// - The part passes the check and the associated workflow is invoked.
// - The part fails the check and the next step in the current workflow is invoked.
// - The part status is resolved and the workflow is terminated.

// A Workflow is a sequence of 1 or more steps and an ID.
// A workflow executes each step in order.

type StepOutcome =
    | Continue
    | Delegate of WorkflowID
    | Resolve of PartStatus.T

module Step =
    open System.Text.RegularExpressions

    let STEP_REGEX = Regex(@"([amsx])(>|<)(\d+):(\w+)")

    type T = Part.T -> bool

    let lessThan (attribute: string) (value: int) (part: Part.T) : bool =
        Part.getAttribute attribute part < value

    let greaterThan (attribute: string) (value: int) (part: Part.T) : bool =
        Part.getAttribute attribute part > value

    let accept (part: Part.T) : StepOutcome = Resolve PartStatus.Accepted
    let reject (part: Part.T) : StepOutcome = Resolve PartStatus.Rejected

    let ofString (str: string) : (T * WorkflowID) =
        let regex_match = STEP_REGEX.Match(str)

        if regex_match.Success then
            let groups =
                regex_match.Groups
                |> Seq.skip 1
                |> Seq.map (fun g -> g.Value)
                |> Seq.toList

            match groups with
            | [ attribute; operator; value; workflowID ] ->
                let condition =
                    match operator with
                    | ">" -> greaterThan attribute (int value)
                    | "<" -> lessThan attribute (int value)
                    | _ -> failwith "Invalid operator"

                (condition, workflowID)
            | _ -> failwith "Matched condition string does not have 4 groups"
        else
            failwith "Condition string does not match regex"

module Workflow =
    open System.Text.RegularExpressions

    let WORKFLOW_REGEX = Regex(@"^(\w+){(.*?),+(\w+)}$")

    type T =
        { ID: WorkflowID
          Steps: (Step.T * WorkflowID) list
          FallbackWorkflowID: WorkflowID }

    // Workflows can be modelled similar to a Monoid.
    // If each workflow produces either a status or another workflow,
    // we can combine them by applying the first workflow and then the second.

    // Since we are constructing a workflow from a text file and because
    // each workflow may reference other workflows that haven't been defined yet,
    // we may need to use the ids of workflows to reference other workflows instead
    // of using a direct reference.

    // We can use a map to store the workflows and then use the ids to reference them.
    // If we want to be able to combine workflows in a functional way, we would ideally
    // want to use a function that takes a workflow and returns a new workflow. This would
    // suggest that we provide the map of workflows as an argument to the function.
    // However, this would mean that we would need to pass the map of workflows to every
    // function that we use to construct a workflow. This is not ideal, so perhaps we
    // can utilize a Reader monad to pass the map of workflows implicitly. As a result,
    // each workflow would then be a function that takes a map of workflows and a part
    // and returns a status or another workflow. The status could be used like a Result
    // type, where we can short-circuit the workflow if we encounter a status or invoke the
    // next workflow if we encounter another workflow.

    // Another approach could be to model each workflow as a pair of a predicate and a workflow.
    // The predicate would be used to determine whether the workflow should be invoked or not.
    // For the final workflow, the predicate would always return true.


    let ofString (str: string) : T =
        let regex_match = WORKFLOW_REGEX.Match(str)

        if regex_match.Success then
            let groups =
                regex_match.Groups
                |> Seq.skip 1
                |> Seq.map (fun g -> g.Value)
                |> Seq.toList

            let workflowID = groups.[0]

            let conditions =
                groups.[1]
                |> fun s -> s.Split(',')
                |> Array.toList
                |> List.map Step.ofString

            let fallbackWorkflowID = groups.[2]

            { ID = workflowID
              Steps = conditions
              FallbackWorkflowID = fallbackWorkflowID }
        else
            failwith "Invalid workflow string"


    let accept (part: Part.T) : PartStatus.T = PartStatus.Accepted
    let reject (part: Part.T) : PartStatus.T = PartStatus.Rejected

module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    let readInput (filename: string) : (Map<WorkflowID, Workflow.T> * Part.T list) =
        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        let lines = File.ReadLines(path)

        let workflows =
            lines
            |> Seq.takeWhile (fun line -> line <> "")
            |> Seq.toList
            |> List.map Workflow.ofString
            |> List.map (fun w -> w.ID, w)
            |> Map.ofList

        let parts =
            lines
            |> Seq.skip (workflows.Count + 1)
            |> Seq.map Part.ofString
            |> Seq.toList

        workflows, parts

    let run (filename: string) : unit =
        let workflows, parts = readInput filename

        printfn "Workflows: %A" workflows
        printfn "Parts: %A" parts

        let totalRating =
            parts |> Seq.map Part.totalRating |> Seq.sum

        printfn "Total rating: %d" totalRating


    let main () =
        // run INPUT_FILE
        run TEST_FILE

    main ()
