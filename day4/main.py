"""
Advent of Code 2023, Day 4
Scratchcards
https://adventofcode.com/2023/day/4
"""

import re
from os import path

INPUT_FILE = "input.txt"

CARD_ITEMS_REGEX = re.compile(r"(\d+)(?!\d*:)|\|")


def extract_numbers(line: str) -> tuple[set[int], set[int]]:
    """Extract the winning numbers and selected numbers from the given line."""

    winning_numbers = set()
    selected_numbers = set()

    target_set = winning_numbers
    items = re.findall(CARD_ITEMS_REGEX, line)

    for item in items:
        if item == "":
            target_set = selected_numbers
        else:
            target_set.add(int(item))

    return winning_numbers, selected_numbers


def count_matches(winning_numbers: set[int], selected_numbers: set[int]) -> int:
    """Count the number of matches between the winning numbers and selected numbers."""

    return len(winning_numbers.intersection(selected_numbers))


def count_matches_per_game(file_path: str) -> dict[int, int]:
    """Count the number of matches for each game."""

    matches_per_game = {}

    with open(file_path, encoding="utf-8") as file:
        for game_id, line in enumerate(file, start=1):
            winning_numbers, selected_numbers = extract_numbers(line)
            match_count = count_matches(winning_numbers, selected_numbers)
            matches_per_game[game_id] = match_count

    return matches_per_game


def calculate_total_points(matches_per_game: dict[int, int]) -> int:
    """Calculate the total points earned from the given cards."""

    total_points = 0

    for match_count in matches_per_game.values():
        points = 2 ** (match_count - 1) if match_count > 0 else 0
        total_points += points

    return total_points


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

    matches_per_game = count_matches_per_game(file_path)
    print(matches_per_game)
    print(calculate_total_points(matches_per_game))
    print(count_cards(matches_per_game))


if __name__ == "__main__":
    main()
