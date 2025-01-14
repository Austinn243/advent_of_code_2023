"""
Advent of Code 2023 Day 7

Evaluate hands in Camel Cards.
"""

from collections import Counter
from enum import IntEnum
from os import path


INPUT_FILE = "input.txt"
INPUT_PATH = path.join(path.dirname(__file__), INPUT_FILE)


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
    """
    Represents a type of hand in Camel Cards.
    """

    HIGH_CARD = 0
    ONE_PAIR = 1
    TWO_PAIR = 2
    THREE_OF_A_KIND = 3
    FULL_HOUSE = 4
    FOUR_OF_A_KIND = 5
    FIVE_OF_A_KIND = 6


class Hand:
    """
    Represents a hand in Camel Cards.
    """

    def __init__(self, cards: list[str]) -> None:
        self.cards = cards
        self.type = None

    def __repr__(self) -> str:
        return f"Hand({self.cards})"

    def __gt__(self, other: "Hand") -> bool:
        if self.hand_type() != other.hand_type():
            return self.hand_type() > other.hand_type()

        for card, other_card in zip(self.cards, other.cards):
            if RANKS[card] != RANKS[other_card]:
                return RANKS[card] > RANKS[other_card]

        return False

    def hand_type(self) -> HandType:
        """
        Determine the type of this hand.
        """

        if self.type is not None:
            return self.type

        card_counts = Counter(self.cards)

        if len(card_counts) == 1:
            self.type = HandType.FIVE_OF_A_KIND

        elif len(card_counts) == 2:
            if 4 in card_counts.values():
                self.type = HandType.FOUR_OF_A_KIND
            else:
                self.type = HandType.FULL_HOUSE

        elif len(card_counts) == 3:
            if 3 in card_counts.values():
                self.type = HandType.THREE_OF_A_KIND
            else:
                self.type = HandType.TWO_PAIR

        elif len(card_counts) == 4:
            self.type = HandType.ONE_PAIR

        else:
            self.type = HandType.HIGH_CARD

        return self.type


def read_data() -> list[tuple[Hand, int]]:
    """
    Read the hands and bids from the input file.
    """

    data = []

    with open(INPUT_PATH, "r", encoding="utf-8") as file:
        for line in file:
            cards, bid = line.split()
            data.append((Hand(cards), int(bid)))

    return data


def total_winnings(hands_and_bids: list[tuple[str, int]]) -> int:
    """
    Calculate the total winnings from the given hands and bids.
    """

    sorted_by_rank = sorted(hands_and_bids, key=lambda x: x[0], reverse=True)

    return sum(bid * rank for rank, (_, bid) in enumerate(sorted_by_rank, start=1))


def main() -> None:
    """
    Read Camel Card hands and their bids from the input file and evaluate them.
    """

    hands_and_bids = read_data()
    for hand, _ in hands_and_bids:
        print(f"{hand} is a {hand.hand_type().name}")

    print(total_winnings(hands_and_bids))


if __name__ == "__main__":
    main()


# INCORRECT: 249952973
# TOO HIGH: 251048314
