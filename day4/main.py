"""
Advent of Code 2023, Day 4
Scratchcards
https://adventofcode.com/2023/day/4
"""

import re
from os import path
from typing import NamedTuple

INPUT_FILE = "input.txt"

CARD_ITEMS_REGEX = re.compile(r"(\d+)(?!\d*:)|\|")
CARD_REGEX = re.compile(r"Card\s+(\d+): (.*)")


class Scratchcard(NamedTuple):
    """Represents a scratchcard with winning and selected numbers."""

    id: int
    selected_numbers: set[int]
    winning_numbers: set[int]


def read_scratchcards(file_path: str) -> list[Scratchcard]:
    """Read scratchcard information from a file."""

    with open(file_path, encoding="utf-8") as file:
        return [parse_scratchcard(line.strip()) for line in file]


def parse_scratchcard(line: str) -> Scratchcard:
    """Parse a scratchcard from a line of text."""

    scratchcard_pattern_match = re.match(CARD_REGEX, line)
    if not scratchcard_pattern_match:
        raise ValueError(f"Invalid scratchcard line: {line}")

    scratchcard_id = int(scratchcard_pattern_match.group(1))
    selected_numbers, winning_numbers = parse_numbers(
        scratchcard_pattern_match.group(2),
    )

    return Scratchcard(scratchcard_id, selected_numbers, winning_numbers)


def parse_numbers(segment: str) -> tuple[set[int], set[int]]:
    """Parse the winning and selected numbers from a segment of text."""

    winning_numbers = set()
    selected_numbers = set()

    target_set = winning_numbers
    items = re.findall(CARD_ITEMS_REGEX, segment)

    for item in items:
        if item == "":
            target_set = selected_numbers
        else:
            target_set.add(int(item))

    return winning_numbers, selected_numbers


def count_matches(scratchcard: Scratchcard) -> int:
    """Count the number of matches between the winning numbers and selected numbers."""

    matches = set.intersection(
        scratchcard.selected_numbers,
        scratchcard.winning_numbers,
    )
    return len(matches)


def count_matches_per_card(scratchcards: list[Scratchcard]) -> dict[int, int]:
    """Count the number of matches for each scratchcard."""

    return {card.id: count_matches(card) for card in scratchcards}


def count_points(match_count: int) -> int:
    """Calculate the points earned given the number of matches."""

    return 2 ** (match_count - 1) if match_count > 0 else 0


def calculate_total_points(matches_per_game: dict[int, int]) -> int:
    """Calculate the total points earned given the number of matches per card."""

    return sum(count_points(match_count) for match_count in matches_per_game.values())


def count_cards(matches_per_game: dict[int, int]) -> int:
    """Count the total number of cards given the number of matches per card."""

    card_counts = {card_id: 1 for card_id in matches_per_game}

    for card_id in range(1, len(card_counts)):
        match_count = matches_per_game[card_id]
        for offset in range(1, match_count + 1):
            card_counts[card_id + offset] += card_counts[card_id]

    return sum(card_counts.values())


def main() -> None:
    """Read scratchcard information from a file and process it."""

    file_path = path.join(path.dirname(__file__), INPUT_FILE)

    scratchcards = read_scratchcards(file_path)

    matches_per_card = count_matches_per_card(scratchcards)

    total_points = calculate_total_points(matches_per_card)
    print(f"The total points earned from the cards is {total_points}")

    card_count = count_cards(matches_per_card)
    print(f"The total number of cards given is {card_count}")


if __name__ == "__main__":
    main()
