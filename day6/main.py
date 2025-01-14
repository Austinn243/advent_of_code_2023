"""
Advent of Code 2023 Day 6

Determine the product of the number of ways to win each race.
"""

from dataclasses import dataclass
from math import prod
from os import path


INPUT_FILE = "input.txt"
INPUT_PATH = path.join(path.dirname(__file__), INPUT_FILE)


@dataclass
class Race:
    """
    Represents a single race in the competition.
    """

    def __init__(self, time: int, distance: int) -> None:
        self.time = time
        self.distance = distance

    def __repr__(self) -> str:
        return f"Race(time: {self.time}, distance: {self.distance})"

    def count_ways_to_win(self) -> int:
        """
        Count the number of ways to win this race.
        """

        least_amount_of_time = 0
        most_amount_of_time = 0

        for wait_time in range(1, self.time // 2):
            remaining_time = self.time - wait_time
            distance_covered = remaining_time * wait_time

            if distance_covered > self.distance:
                least_amount_of_time = wait_time
                most_amount_of_time = remaining_time

                break

        way_count = most_amount_of_time - least_amount_of_time + 1

        return way_count


def read_raw_race_data() -> tuple[list[str], list[str]]:
    """
    Read the times and distances from the input file.
    """

    with open(INPUT_PATH, "r", encoding="utf-8") as file:
        lines = file.readlines()

        times = lines[0].split()[1:]
        distances = lines[1].split()[1:]

        return (times, distances)


def interpret_data_as_multiple_races(
    times: list[str], distances: list[str]
) -> list[Race]:
    """
    Interpret the input as a set of races.
    """

    return [Race(int(time), int(distance)) for time, distance in zip(times, distances)]


def interpret_data_as_single_race(times: list[str], distances: list[str]) -> Race:
    """
    Interpret the input as a single race.
    """

    time = int("".join(times))
    distance = int("".join(distances))

    return Race(time, distance)


def product_of_ways_to_win(races: list[Race]) -> int:
    """
    Count the number of ways to win each race, then return their product.
    """

    return prod(race.count_ways_to_win() for race in races)


def main():
    """
    Evaluate race data from the input file.
    """

    print(Race(7, 9).count_ways_to_win())
    print(Race(15, 40).count_ways_to_win())
    print(Race(30, 200).count_ways_to_win())

    times, distances = read_raw_race_data()

    races = interpret_data_as_multiple_races(times, distances)
    print(product_of_ways_to_win(races))

    race = interpret_data_as_single_race(times, distances)
    print(race)
    print(race.count_ways_to_win())


if __name__ == "__main__":
    main()
