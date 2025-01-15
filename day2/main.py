"""
Advent of Code 2023, Day 2
Cube Conundrum
https://adventofcode.com/2023/day/2
"""

from math import prod
from os import path
from re import compile as re_compile
from typing import NamedTuple

INPUT_FILE = "input.txt"

GAME_REGEX = re_compile(r"Game (\d+): (.*)")

Round = dict[str, int]


class Game(NamedTuple):
    """Represents a game that can be played with colored cubes."""

    id: int
    rounds: list[Round]


TARGET_ROUND: Round = {
    "red": 12,
    "green": 13,
    "blue": 14,
}


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


def get_required_color_counts(game: Game) -> dict[str, int]:
    """Get the required counts of each color to play all rounds of a game."""

    required_color_counts = {color: 0 for color in TARGET_ROUND}

    for game_round in game.rounds:
        for color, count in game_round.items():
            required_color_counts[color] = max(required_color_counts[color], count)

    return required_color_counts


def can_play_round(game: Game, target_round: Round) -> bool:
    """Determine if a round could be played given information about a game."""

    required_color_counts = get_required_color_counts(game)

    return all(
        required_color_counts[color] <= target_count
        for color, target_count in target_round.items()
    )


def sum_ids_of_games_that_can_play_round(games: list[Game], target_round: Round) -> int:
    """Sum the ids of all games that can play a given round."""

    return sum(game.id for game in games if can_play_round(game, target_round))


def get_power(game: Game) -> int:
    """Get the power of a game."""

    required_counts = get_required_color_counts(game)

    return prod(required_counts.values())


def sum_powers_of_games(games: list[Game]) -> int:
    """Sum the powers of the required counts of each color for all games."""

    return sum(get_power(game) for game in games)


def main() -> None:
    """Read numbers of colored cubes from a file and process them."""

    file_path = path.join(path.dirname(__file__), INPUT_FILE)

    games = read_games(file_path)

    game_id_sum = sum_ids_of_games_that_can_play_round(games, TARGET_ROUND)
    print(
        f"The sum of the IDs of games that can play the target round is {game_id_sum}",
    )

    power_sum = sum_powers_of_games(games)
    print(f"The sum of the powers of all games is {power_sum}")


if __name__ == "__main__":
    main()
