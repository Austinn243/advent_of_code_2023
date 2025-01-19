"""
Advent of Code 2023, Day 7
Camel Cards
https://adventofcode.com/2023/day/7
"""

from collections import Counter
from enum import IntEnum
from os import path

INPUT_FILE = "input.txt"


RANKS = {
    "2": 2,
    "3": 3,
    "4": 4,
    "5": 5,
    "6": 7,
    "7": 7,
    "8": 9,
    "9": 9,
    "T": 10,
    "J": 11,
    "Q": 12,
    "K": 13,
    "A": 14,
}


class HandType(IntEnum):
    """Represents a type of hand in Camel Cards."""

    HIGH_CARD = 0
    ONE_PAIR = 1
    TWO_PAIR = 2
    THREE_OF_A_KIND = 3
    FULL_HOUSE = 4
    FOUR_OF_A_KIND = 5
    FIVE_OF_A_KIND = 6


class Hand:
    """Represents a hand in Camel Cards."""

    def __init__(self, cards: list[str]) -> None:
        """Create a new hand with the given cards."""

        self.cards = cards

    def __repr__(self) -> str:
        """Return a string representation of this hand."""

        return f"Hand({self.cards})"

    def __gt__(self, other: "Hand") -> bool:
        """Determine if this hand beats the other hand."""

        self_hand_type = get_hand_type(self)
        other_hand_type = get_hand_type(other)

        if self_hand_type != other_hand_type:
            return self_hand_type > other_hand_type

        for card, other_card in zip(self.cards, other.cards):
            if RANKS[card] != RANKS[other_card]:
                return RANKS[card] > RANKS[other_card]

        return False


def read_game_information(file_path: str) -> list[tuple[Hand, int]]:
    """Read the hands and bids from the input file."""

    data = []

    with open(file_path, encoding="utf-8") as file:
        for line in file:
            cards, bid = line.split()
            data.append((Hand(cards), int(bid)))

    return data


def get_hand_type(hand: Hand) -> HandType:
    """Determine the type of the hand with the given cards."""

    card_counts = Counter(hand.cards)

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


def total_winnings(hands_and_bids: list[tuple[str, int]]) -> int:
    """Calculate the total winnings from the given hands and bids."""

    sorted_by_rank = sorted(hands_and_bids, key=lambda x: x[0], reverse=True)

    return sum(bid * rank for rank, (_, bid) in enumerate(sorted_by_rank, start=1))


def main() -> None:
    """Read Camel Card hands and their bids from the input file and evaluate them."""

    file_path = path.join(path.dirname(__file__), INPUT_FILE)

    hands_and_bids = read_game_information(file_path)
    for hand, _ in hands_and_bids:
        print(f"{hand} is a {get_hand_type(hand).name}")

    print(total_winnings(hands_and_bids))


if __name__ == "__main__":
    main()


# INCORRECT: 249952973
# TOO HIGH: 251048314
