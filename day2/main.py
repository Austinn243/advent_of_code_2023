"""
Advent of Code 2023, Day 2
Cube Conundrum
https://adventofcode.com/2023/day/2
"""

from collections import Counter
from math import prod
from os import path
from re import compile as re_compile
from typing import Iterable, NamedTuple

INPUT_FILE = "input.txt"

GAME_REGEX = re_compile(r"Game (\d+): (.*)")
COLOR_REGEX = re_compile(r"(\d+) (red|green|blue)")

Round = dict[str, int]

class Game(NamedTuple):
    id: int
    rounds: list[Round]


TARGET_ROUND: Round = Counter(
    {
        "red": 12,
        "green": 13,
        "blue": 14,
    }
)


def extract_counts(line: str) -> Iterable[tuple[int, str]]:
    """Extract the counts of each color from the given line of input."""

    for count, color in COLOR_REGEX.findall(line):
        yield int(count), color


def sum_powers_of_minimum_counts(file_path: str) -> int:
    """Sum the powers of the minimum counts of each color required for each game."""

    power_sum = 0

    with open(file_path, encoding="utf-8") as file:
        for line in file:
            required_counts = {color: 0 for color in TARGET_ROUND}
            for count, color in extract_counts(line):
                required_counts[color] = max(required_counts[color], count)

            power = prod(required_counts.values())
            power_sum += power

    return power_sum



def read_games(file_path: str) -> list[Game]:
    """Read game information from a file."""

    with open(file_path, encoding="utf-8") as file:
        return [parse_game(line.strip()) for line in file]


def parse_game(line: str) -> Game:
    """Parse a game from a line of text."""

    game_pattern_match = GAME_REGEX.match(line)
    if not game_pattern_match:
        raise ValueError(f"Invalid game line: {line}")
    
    game_id = int(game_pattern_match.group(1))
    rounds = parse_rounds(game_pattern_match.group(2))

    return Game(game_id, rounds)


def parse_rounds(segment: str) -> list[dict[str, int]]:
    """Parse the rounds of a game from a segment of text."""

    rounds = []

    for round_data in segment.split("; "):
        color_counts = {}
        colors_data = round_data.split(", ")

        for color_data in colors_data:
            count, color = color_data.split(" ")
            color_counts[color] = int(count)

        rounds.append(color_counts)

    return rounds


def get_max_played_color_counts(game: Game) -> dict[str, int]:
    """Get the maximum counts of each color played in a game."""

    max_color_counts = {color: 0 for color in TARGET_ROUND}

    for round in game.rounds:
        for color, count in round.items():
            max_color_counts[color] = max(max_color_counts[color], count)

    return max_color_counts


def can_play_round(game: Game, target_round: Round) -> bool:
    """Determine if a round could be played given information about a game."""
    
    max_color_counts = get_max_played_color_counts(game)

    return all(max_color_counts[color] <= count for color, count in target_round.items())


def sum_ids_of_games_that_can_play_round(games: list[Game], target_round: Round) -> int:
    """Sum the ids of all games that can play a given round."""

    return sum(game.id for game in games if can_play_round(game, target_round))


def main() -> None:
    """Read numbers of colored cubes from a file and process them."""

    file_path = path.join(path.dirname(__file__), INPUT_FILE)

    games = read_games(file_path)

    game_id_sum = sum_ids_of_games_that_can_play_round(games, TARGET_ROUND)
    print(f"The sum of the IDs of games that can play the target round is {game_id_sum}")

    print(sum_powers_of_minimum_counts(file_path))


if __name__ == "__main__":
    main()
