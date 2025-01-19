"""
Advent of Code 2023, Day 7
Camel Cards
https://adventofcode.com/2023/day/7
"""

from collections import Counter
from collections.abc import Callable
from enum import IntEnum
from functools import cache, cmp_to_key
from os import path
from typing import NamedTuple

INPUT_FILE = "input.txt"
TEST_FILE = "test.txt"


class Card(NamedTuple):
    """Represents a card in Camel Cards."""

    rank: str


class HandType(IntEnum):
    """Represents a type of hand in Camel Cards."""

    HIGH_CARD = 0
    ONE_PAIR = 1
    TWO_PAIR = 2
    THREE_OF_A_KIND = 3
    FULL_HOUSE = 4
    FOUR_OF_A_KIND = 5
    FIVE_OF_A_KIND = 6


Hand = tuple[Card, Card, Card, Card, Card]


class Play(NamedTuple):
    """Represents a play in Camel Cards."""

    hand: Hand
    bid: int


class Ruleset(NamedTuple):
    """Represents a ruleset for Camel Cards."""

    evaluate_hand: Callable[[Hand], HandType]
    rank_values: dict[str, int]


def get_card_value(card: Card, ruleset: Ruleset) -> int:
    """Return the value of the given card."""

    return ruleset.rank_values[card.rank]


def compare_cards(
    hand1: Hand,
    hand2: Hand,
    ruleset: Ruleset,
) -> int:
    """Compare two hands."""

    hand1_type = ruleset.evaluate_hand(hand1)
    hand2_type = ruleset.evaluate_hand(hand2)

    if hand1_type != hand2_type:
        return hand1_type - hand2_type

    hand1_values = [get_card_value(card, ruleset) for card in hand1]
    hand2_values = [get_card_value(card, ruleset) for card in hand2]
    differences = (v1 - v2 for v1, v2 in zip(hand1_values, hand2_values))

    return next((diff for diff in differences if diff != 0), 0)


@cache
def get_standard_hand_type(hand: Hand) -> HandType:
    """Determine the type of the hand using the standard rules."""

    card_counts = Counter(hand)

    if len(card_counts) == 1:
        return HandType.FIVE_OF_A_KIND

    if len(card_counts) == 2:
        if 4 in card_counts.values():
            return HandType.FOUR_OF_A_KIND
        else:
            return HandType.FULL_HOUSE

    if len(card_counts) == 3:
        if 3 in card_counts.values():
            return HandType.THREE_OF_A_KIND
        else:
            return HandType.TWO_PAIR

    if len(card_counts) == 4:
        return HandType.ONE_PAIR

    return HandType.HIGH_CARD


@cache
def get_wild_hand_type(hand: Hand) -> HandType:
    """Determine the type of the hand using the rules where jacks are jokers."""

    card_counts = Counter(hand)

    # FIXME: This implementation provides the wrong values, likely when it
    # comes down to hands involving different ranks.

    joker_count = card_counts.get(Card("J"), 0)
    max_non_joker_count = max(
        (card_counts[card] for card in card_counts if card != Card("J")),
        default=0,
    )

    largest_same_rank_count = max_non_joker_count + joker_count

    if largest_same_rank_count == 5:
        return HandType.FIVE_OF_A_KIND

    if largest_same_rank_count == 4:
        return HandType.FOUR_OF_A_KIND

    can_make_full_house = largest_same_rank_count >= 3 and len(card_counts) >= 2
    if can_make_full_house:
        return HandType.FULL_HOUSE

    if largest_same_rank_count == 3:
        return HandType.THREE_OF_A_KIND

    if len(card_counts) == 3:
        return HandType.TWO_PAIR

    if largest_same_rank_count == 2:
        return HandType.ONE_PAIR

    return HandType.HIGH_CARD


STANDARD_RULESET = Ruleset(
    get_standard_hand_type,
    {
        "2": 2,
        "3": 3,
        "4": 4,
        "5": 5,
        "6": 6,
        "7": 7,
        "8": 8,
        "9": 9,
        "T": 10,
        "J": 11,
        "Q": 12,
        "K": 13,
        "A": 14,
    },
)

JACKS_ARE_JOKERS_RULESET = Ruleset(
    get_wild_hand_type,
    {
        "J": 1,
        "2": 2,
        "3": 3,
        "4": 4,
        "5": 5,
        "6": 6,
        "7": 7,
        "8": 8,
        "9": 9,
        "T": 10,
        "Q": 12,
        "K": 13,
        "A": 14,
    },
)


def read_plays(file_path: str) -> list[Play]:
    """Read the plays for a game of Camel Cards from the input file."""

    with open(file_path) as file:
        return [parse_play(line) for line in file]


def parse_play(line: str) -> Play:
    """Parse a play from the given line."""

    raw_cards, raw_bid = line.strip().split(" ")

    hand = tuple(Card(raw_card) for raw_card in raw_cards)
    bid = int(raw_bid)

    return Play(hand, bid)


def total_winnings(
    plays: list[Play],
    ruleset: Ruleset,
) -> int:
    """Calculate the total winnings from the given hands and bids."""

    key = cmp_to_key(
        lambda play1, play2: compare_cards(play1.hand, play2.hand, ruleset),
    )

    sorted_by_rank = sorted(plays, key=key)

    return sum(play.bid * rank for rank, play in enumerate(sorted_by_rank, start=1))


def main() -> None:
    """Read Camel Card hands and their bids from the input file and evaluate them."""

    input_file = INPUT_FILE
    file_path = path.join(path.dirname(__file__), input_file)

    plays = read_plays(file_path)

    standard_rule_total_winnings = total_winnings(plays, STANDARD_RULESET)
    print("Using standard rules:")
    print(f"Total winnings are {standard_rule_total_winnings}")
    print()

    total_winnings_with_jokers = total_winnings(plays, JACKS_ARE_JOKERS_RULESET)
    print("Using Jacks Are Jokers rules:")
    print(f"Total winnings are {total_winnings_with_jokers}")


if __name__ == "__main__":
    main()


# 250433274 is too low
# 250637576 is wrong
# 250740442 is wrong
# 251363551 is incorrect
