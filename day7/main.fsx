/// Advent of Code 2023, Day 7
/// Camel Cards
/// https://adventofcode.com/2023/day/7

module Debug =
    let tap (value: 'T) (f: 'T -> unit) : 'T =
        f value
        value

    let trace (value: 'T) : 'T =
        printfn "%A" value
        value

/// Represents a rule for the game.
type Rule =
    | JacksAreNormal
    | JacksAreWild

/// Represents a rank of a card.
module Rank =
    type T =
        | Two
        | Three
        | Four
        | Five
        | Six
        | Seven
        | Eight
        | Nine
        | Ten
        | Jack
        | Queen
        | King
        | Ace

    /// Convert a character to a rank.
    let ofChar (c: char) : T =
        match c with
        | '2' -> Two
        | '3' -> Three
        | '4' -> Four
        | '5' -> Five
        | '6' -> Six
        | '7' -> Seven
        | '8' -> Eight
        | '9' -> Nine
        | 'T' -> Ten
        | 'J' -> Jack
        | 'Q' -> Queen
        | 'K' -> King
        | 'A' -> Ace
        | _ -> failwith "Invalid rank character."

    /// Determine the value of a rank according to the given rule.
    let value (rule: Rule) (rank: T) : int =
        match rank with
        | Two -> 2
        | Three -> 3
        | Four -> 4
        | Five -> 5
        | Six -> 6
        | Seven -> 7
        | Eight -> 8
        | Nine -> 9
        | Ten -> 10
        | Queen -> 12
        | King -> 13
        | Ace -> 14
        | Jack ->
            match rule with
            | JacksAreNormal -> 11
            | JacksAreWild -> 1

    /// Compare two ranks according to the given rule.
    let compare (rule: Rule) (rank1: T) (rank2: T) : int =
        let getValue = value rule
        getValue rank1 - getValue rank2

/// Represents the overall strength of a hand.
module HandType =
    type T =
        | HighCard
        | OnePair
        | TwoPair
        | ThreeOfAKind
        | FullHouse
        | FourOfAKind
        | FiveOfAKind

    /// Determine the type of hand represented by the given cards
    /// under the standard rules.
    let private evaluateNormal (cards: Rank.T list) : T =
        let counts =
            cards
            |> List.countBy id
            |> List.map snd
            |> List.sort

        match counts with
        | [ 1; 1; 1; 1; 1 ] -> HighCard
        | [ 1; 1; 1; 2 ] -> OnePair
        | [ 1; 2; 2 ] -> TwoPair
        | [ 1; 1; 3 ] -> ThreeOfAKind
        | [ 2; 3 ] -> FullHouse
        | [ 1; 4 ] -> FourOfAKind
        | [ 5 ] -> FiveOfAKind
        | _ -> failwith "Invalid hand."

    /// Determine the type of hand represented by the given cards
    /// under the jacks are wild rules.
    let private evaluateWild (cards: Rank.T list) : T =
        let mutable jackCount =
            cards
            |> List.filter (fun rank -> rank = Rank.Jack)
            |> List.length

        if jackCount = 0 then
            evaluateNormal cards
        else
            let mostCommonRank =
                cards |> List.countBy id |> List.maxBy snd |> fst

            // let mostCommonRankCount =
            //     cards
            //     |> List.filter (fun rank -> rank = mostCommonRank)
            //     |> List.length

            cards
            |> List.rev
            |> List.map (fun rank ->
                if rank = Rank.Jack && jackCount > 0 then
                    jackCount <- jackCount - 1
                    mostCommonRank
                else
                    rank)
            |> List.rev
            |> evaluateNormal


    /// Determine the type of hand is represented by the given cards according to the given rule.
    let evaluate (rule: Rule) (cards: Rank.T list) : T =
        match rule with
        | JacksAreNormal -> evaluateNormal cards
        | JacksAreWild -> evaluateWild cards

    /// Determine the ranking of a hand type.
    let ranking (handType: T) : int =
        match handType with
        | HighCard -> 1
        | OnePair -> 2
        | TwoPair -> 3
        | ThreeOfAKind -> 4
        | FullHouse -> 5
        | FourOfAKind -> 6
        | FiveOfAKind -> 7

    /// Compare two types of hands.
    let compare (handType1: T) (handType2: T) : int = ranking handType1 - ranking handType2

/// Represents a hand of cards.
module Hand =
    type T = Rank.T list

    /// Create a hand from a string.
    let ofString (s: string) : T = s |> Seq.map Rank.ofChar |> Seq.toList

    /// Compare two hands using the given rule.
    let compare (rule: Rule) (hand1: T) (hand2: T) : int =
        let handType1 = HandType.evaluate rule hand1
        let handType2 = HandType.evaluate rule hand2

        let handTypeComparison = HandType.compare handType1 handType2

        if handTypeComparison <> 0 then
            handTypeComparison
        else
            let pairs = List.zip hand1 hand2

            let mismatch =
                List.tryFind (fun (rank1, rank2) -> rank1 <> rank2) pairs

            match mismatch with
            | Some (rank1, rank2) -> Rank.compare rule rank1 rank2
            | None -> 0

/// Represents a bid on a given hand.
type Bid = { Hand: Hand.T; Bid: int }

/// Run the program.
module Main =
    open System.IO

    let INPUT_FILE = "input.txt"
    let TEST_FILE = "test.txt"

    /// Parse a bid from a line of text.
    let parseBid (line: string) : Bid =
        let parts = line.Split()
        let hand = parts.[0] |> Hand.ofString
        let bid = int parts.[1]

        { Hand = hand; Bid = bid }

    /// Read the input from the given file.
    let readInput (path: string) : Bid seq =
        path |> File.ReadLines |> Seq.map parseBid

    /// Sum the bids according to the given rule.
    let sumBids (rule: Rule) (bids: Bid seq) : int =
        let sortedBids =
            bids
            |> Seq.sortWith (fun bid1 bid2 -> Hand.compare rule bid1.Hand bid2.Hand)

        printfn "%A" sortedBids

        let sum =
            sortedBids
            |> Seq.mapi (fun index bid -> bid.Bid * (index + 1))
            |> Seq.sum

        sum

    /// Run the program with the given input file.
    let main (args: string []) : int =
        let filename =
            if args.Length > 0 then
                args.[0]
            else
                INPUT_FILE

        let path =
            Path.Combine(__SOURCE_DIRECTORY__, filename)

        let bids = readInput path

        let sumNormal = sumBids JacksAreNormal bids
        let sumWild = sumBids JacksAreWild bids

        printfn "Part 1: %d" sumNormal
        printfn "Part 2: %d" sumWild

        0

    [<EntryPoint>]
    main [| INPUT_FILE |]

// main [| TEST_FILE |]

// TOO HIGH: 251006848
