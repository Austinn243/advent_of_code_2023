"""
Advent of Code 2023, Day 8

Evaluate paths through a haunted wasteland.
"""

from dataclasses import dataclass
from os import path
from re import compile as re_compile
from typing import Optional

TEST_FILE_1 = "test1.txt"
TEST_FILE_2 = "test2.txt"
TEST_FILE_3 = "test3.txt"
INPUT_FILE = "input.txt"

JUNCTION_REGEX = re_compile(r"^(\w{3}) = \((\w{3}), (\w{3})\)$")

START_ID = "AAA"
EXIT_ID = "ZZZ"


@dataclass
class Junction:
    """
    Represents a junction in a path with two possible directions.
    """

    def __init__(
        self,
        junction_id: str,
        left: Optional["Junction"] = None,
        right: Optional["Junction"] = None,
    ):
        self.id = junction_id
        self.left = left
        self.right = right

    def __repr__(self) -> str:
        return f"Junction({self.id}, L: {self.left.id}, R: {self.right.id})"

    def __hash__(self) -> int:
        return hash(self.id)


def read_input(filename: str) -> tuple[str, dict[str, Junction]]:
    """
    Read data from the input file, returning the directions and a dictionary of
    junction ids to junctions.
    """

    def get_junction(junction_id: str) -> Junction:
        """
        Get a junction by its id, creating it if it does not exist.
        """

        if junction_id not in junctions:
            junctions[junction_id] = Junction(junction_id)

        return junctions[junction_id]

    file_path = path.join(path.dirname(__file__), filename)
    with open(file_path, "r", encoding="utf-8") as file:
        lines = iter(file.readlines())

    junctions = {}
    directions = next(lines).strip()
    _ = next(lines)

    for line in lines:
        junction_id, left_id, right_id = read_junction_ids(line)
        current_junction = get_junction(junction_id)
        left_junction = get_junction(left_id)
        right_junction = get_junction(right_id)

        current_junction.left = left_junction
        current_junction.right = right_junction

    return directions, junctions


def read_junction_ids(line: str) -> tuple[str, str, str]:
    """
    Read the junction ids from a line of input.
    """

    match = JUNCTION_REGEX.match(line)

    if match is None:
        raise ValueError(f"Invalid junction line: {line}")

    return match.group(1), match.group(2), match.group(3)


def count_steps_to_exit(
    directions: str, junctions: dict[str, Junction], start_id: str, destination_id: str
) -> int:
    """
    Count the number of steps it takes to traverse the path from the start to
    the exit.
    """

    step_count = 0
    step_index = 0
    current_junction = junctions[start_id]

    while current_junction.id != destination_id:
        direction = directions[step_index]
        if direction == "L":
            current_junction = current_junction.left
        else:
            current_junction = current_junction.right

        step_index = (step_index + 1) % len(directions)
        step_count += 1

    return step_count


def count_steps_for_all_paths_ending_in_a_to_z(
    directions: str,
    junctions: dict[str, Junction],
) -> int:
    """
    Count the number of steps required to simultaneously traverse all junctions
    ending in A until each path reaches a junction that ends in Z.
    """

    current_junctions = [
        junction for junction in junctions.values() if junction.id.endswith("A")
    ]

    step_count = 0
    step_index = 0

    while any(junction.id[2] != "Z" for junction in current_junctions):
        current_junctions = (
            [junction.left for junction in current_junctions]
            if directions[step_index] == "L"
            else [junction.right for junction in current_junctions]
        )

        step_index = (step_index + 1) % len(directions)
        step_count += 1

    return step_count


def test_count_steps_to_exit(filename: str) -> None:
    """
    Test count_steps_to_exit().
    """

    directions, junctions = read_input(filename)
    print(count_steps_to_exit(directions, junctions, START_ID, EXIT_ID))


def test_count_steps_for_all_paths_ending_in_a_to_z(filename: str) -> None:
    """
    Test count_steps_for_all_paths_ending_in_a_to_z().
    """

    directions, junctions = read_input(filename)
    print(count_steps_for_all_paths_ending_in_a_to_z(directions, junctions))


def run(filename: str) -> None:
    """
    Run the program.
    """

    directions, junctions = read_input(filename)
    print(count_steps_to_exit(directions, junctions, START_ID, EXIT_ID))
    print(count_steps_for_all_paths_ending_in_a_to_z(directions, junctions))


def main():
    """
    Read path information from the input path and evaluate the data.
    """

    # test_count_steps_to_exit(TEST_FILE_1)
    # test_count_steps_to_exit(TEST_FILE_2)
    # test_count_steps_for_all_paths_ending_in_a_to_z(TEST_FILE_3)
    run(INPUT_FILE)


if __name__ == "__main__":
    main()
