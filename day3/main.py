"""
Advent of Code 2023, Day 3
Gear Ratios
https://adventofcode.com/2023/day/3
"""

from math import prod
from os import path
from typing import NamedTuple

INPUT_FILE = "input.txt"
TEST_FILE = "test.txt"

GEAR_INDICATOR = "*"
PART_INDICATORS = {
    "/",
    "@",
    "*",
    "+",
    "#",
    "$",
    "&",
    "=",
    "-",
    "%",
}


class Position(NamedTuple):
    """Represents a position in the schema."""

    row: int
    col: int


class PartInformation:
    """Information about a part."""

    def __init__(self, part_type: str, part_position: Position) -> None:
        """Create a new part information object."""

        self.position = part_position
        self.type = part_type
        self.numbers = []

    def add_number(self, number: int) -> None:
        """Add the given number to the part."""

        self.numbers.append(number)

    def is_gear(self) -> bool:
        """Determine if the part is a gear."""

        return self.type == GEAR_INDICATOR and len(self.numbers) == 2

    def gear_ratio(self) -> int:
        """Calculate the gear ratio for the part."""

        if not self.is_gear():
            return 0

        return prod(self.numbers)


SchemaInformation = dict[Position, PartInformation]


def read_schema(file_path: str) -> list[str]:
    """Read the schema from the given input file."""

    with open(file_path, encoding="utf-8") as file:
        return file.readlines()


def within_bounds(schema: list[str], position: Position) -> bool:
    """Determine if the given position is within the bounds of the given file."""

    row, col = position

    return 0 <= row < len(schema) and 0 <= col < len(schema[row])


def get_neighbor_positions(
    schema: list[str],
    position: Position,
) -> list[Position]:
    """Get the neighbor positions of the given position."""

    row, col = position

    possible_neighbors = [
        Position(row - 1, col - 1),
        Position(row - 1, col),
        Position(row - 1, col + 1),
        Position(row, col - 1),
        Position(row, col + 1),
        Position(row + 1, col - 1),
        Position(row + 1, col),
        Position(row + 1, col + 1),
    ]

    return [
        neighbor for neighbor in possible_neighbors if within_bounds(schema, neighbor)
    ]


def extract_schema_info(schema: list[str]) -> SchemaInformation:
    """Extract the part information from the given schema."""

    current_digits = []
    current_part_info = None

    schema_info: SchemaInformation = {}

    for row, line in enumerate(schema):
        for col, char in enumerate(line):
            if not char.isdigit():
                if current_part_info is not None:
                    if current_part_info.position not in schema_info:
                        schema_info[current_part_info.position] = current_part_info

                    schema_info[current_part_info.position].add_number(
                        int("".join(current_digits)),
                    )

                    current_part_info = None

                current_digits.clear()
                continue

            current_digits.append(char)
            if current_part_info is not None:
                continue

            position = Position(row, col)
            for neighbor_position in get_neighbor_positions(schema, position):
                n_row, n_col = neighbor_position
                neighbor_value = schema[n_row][n_col]
                if neighbor_value in PART_INDICATORS:
                    current_part_info = PartInformation(
                        neighbor_value,
                        neighbor_position,
                    )
                    break

    return schema_info


def main() -> None:
    """Identify the part numbers in the given input file and calculate their sum."""

    input_file = INPUT_FILE
    file_path = path.join(path.dirname(__file__), input_file)

    schema = read_schema(file_path)
    schema_info = extract_schema_info(schema)

    part_number_sum = sum(sum(part_info.numbers) for part_info in schema_info.values())
    print(part_number_sum)

    gear_ratio_sum = sum(part_info.gear_ratio() for part_info in schema_info.values())
    print(gear_ratio_sum)


if __name__ == "__main__":
    main()
