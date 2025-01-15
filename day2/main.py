"""
Advent of Code 2023, Day 2
Cube Conundrum
https://adventofcode.com/2023/day/2
"""

from collections import Counter
from math import prod
from os import path
from re import compile as re_compile
from typing import Iterable

INPUT_FILE = "input.txt"

GAME_PATTERN = re_compile(r"^Game (\d+)")
SET_PATTERN = re_compile(r"(\d+) (red|green|blue)")

TARGET_COUNTS = Counter(
    {
        "red": 12,
        "green": 13,
        "blue": 14,
    }
)


def extract_game_id(line: str) -> int:
    """Extract the id of the game from the given line of input."""

    return int(GAME_PATTERN.match(line).group(1))


def extract_counts(line: str) -> Iterable[tuple[int, str]]:
    """Extract the counts of each color from the given line of input."""

    for count, color in SET_PATTERN.findall(line):
        yield int(count), color


def can_play_game(line: str) -> tuple[int, list[Counter]]:
    """Determine if the game specified by the given line of input can be played."""

    for count, color in extract_counts(line):
        if count > TARGET_COUNTS[color]:
            return False

    return True


def sum_of_playable_games(file_path: str) -> int:
    """Determine the sum of the ids of all games which can be played."""

    game_ids = []

    with open(file_path, encoding="utf-8") as file:
        for line in file:
            game_id = extract_game_id(line)

            if can_play_game(line):
                game_ids.append(game_id)

    return sum(game_ids)


def sum_powers_of_minimum_counts(file_path: str) -> int:
    """Sum the powers of the minimum counts of each color required for each game."""

    power_sum = 0

    with open(file_path, encoding="utf-8") as file:
        for line in file:
            required_counts = {color: 0 for color in TARGET_COUNTS}
            for count, color in extract_counts(line):
                required_counts[color] = max(required_counts[color], count)

            power = prod(required_counts.values())
            power_sum += power

    return power_sum


def main() -> None:
    """Read numbers of colored cubes from a file and process them."""

    file_path = path.join(path.dirname(__file__), INPUT_FILE)

    print(sum_of_playable_games(file_path))
    print(sum_powers_of_minimum_counts(file_path))


if __name__ == "__main__":
    main()
