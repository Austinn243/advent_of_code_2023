"""
Advent of Code 2023, Day 3

Identify the part numbers in the given input file and calculate their sum.
"""

from math import prod
from os import path

Position = tuple[int, int]

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


class PartInformation:
    """
    Information about a part.
    """

    def __init__(self, part_type: str, part_position: Position) -> None:
        self.position = part_position
        self.type = part_type
        self.numbers = []

    def add_number(self, number: int) -> None:
        """
        Add the given number to the part.
        """

        self.numbers.append(number)

    def is_gear(self) -> bool:
        """
        Determine if the part is a gear.
        """

        return self.type == GEAR_INDICATOR and len(self.numbers) == 2

    def gear_ratio(self) -> int:
        """
        Calculate the gear ratio for the part.
        """

        if not self.is_gear():
            return 0

        return prod(self.numbers)


SchemaInformation = dict[Position, PartInformation]


def read_schema(filename: str) -> list[str]:
    """
    Read the schema from the given input file.
    """

    file_path = path.join(path.dirname(path.abspath(__file__)), filename)

    with open(file_path, "r", encoding="utf-8") as file:
        return file.readlines()


def within_bounds(schema: list[str], position: tuple[int, int]) -> bool:
    """
    Determine if the given position is within the bounds of the given file.
    """

    row, col = position

    return 0 <= row < len(schema) and 0 <= col < len(schema[row])


def get_neighbor_positions(
    schema: list[str], position: tuple[int, int]
) -> list[tuple[int, int]]:
    """
    Get the neighbor positions of the given position.
    """

    row, col = position

    possible_neighbors = [
        (row - 1, col - 1),
        (row - 1, col),
        (row - 1, col + 1),
        (row, col - 1),
        (row, col + 1),
        (row + 1, col - 1),
        (row + 1, col),
        (row + 1, col + 1),
    ]

    return [
        neighbor for neighbor in possible_neighbors if within_bounds(schema, neighbor)
    ]


def extract_schema_info(schema: list[str]) -> SchemaInformation:
    """
    Extract the part information from the given schema.
    """

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
                        int("".join(current_digits))
                    )

                    current_part_info = None

                current_digits.clear()
                continue

            current_digits.append(char)
            if current_part_info is not None:
                continue

            for neighbor in get_neighbor_positions(schema, (row, col)):
                n_row, n_col = neighbor
                neighbor_value = schema[n_row][n_col]
                if neighbor_value in PART_INDICATORS:
                    current_part_info = PartInformation(neighbor_value, (n_row, n_col))
                    break

    return schema_info


def run(filename: str) -> None:
    """
    Perform operations on the given file.
    """

    schema = read_schema(filename)
    schema_info = extract_schema_info(schema)

    part_number_sum = sum(sum(part_info.numbers) for part_info in schema_info.values())
    print(part_number_sum)

    gear_ratio_sum = sum(part_info.gear_ratio() for part_info in schema_info.values())
    print(gear_ratio_sum)


def main() -> None:
    """
    Identify the part numbers in the given input file and calculate their sum.
    """

    # run(TEST_FILE)
    run(INPUT_FILE)


if __name__ == "__main__":
    main()
