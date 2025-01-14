"""
Advent of Code 2023, Day 1
Trebuchet?!
https://adventofcode.com/2023/day/1
"""

from os import path


INPUT_FILE = "input.txt"
TEST_FILE_1 = "test1.txt"
TEST_FILE_2 = "test2.txt"
PARENT_DIRECTORY = path.dirname(path.abspath(__file__))

DIGITS_TO_VALUES = {
    "one": 1,
    "two": 2,
    "three": 3,
    "four": 4,
    "five": 5,
    "six": 6,
    "seven": 7,
    "eight": 8,
    "nine": 9,
}


def extract_first_digit(line: str) -> int:
    """
    Extract the first digit from a line of input.
    """

    def loop(remaining: str) -> int:
        if remaining[0].isdigit():
            return int(remaining[0])

        for key, value in DIGITS_TO_VALUES.items():
            if remaining.startswith(key):
                return value

        return loop(remaining[1:])

    return loop(line)


def extract_last_digit(line: str) -> int:
    """
    Extract the last digit from a line of input.
    """

    def loop(remaining: str) -> int:
        if remaining[-1].isdigit():
            return int(remaining[-1])

        for key, value in DIGITS_TO_VALUES.items():
            if remaining.endswith(key):
                return value

        return loop(remaining[:-1])

    return loop(line)


def extract_calibration_value(line: str) -> int:
    """
    Retrieve the calibration value from a line of input.
    """

    first_digit = extract_first_digit(line)
    last_digit = extract_last_digit(line)

    return (first_digit * 10) + last_digit


def sum_calibration_values_in_file(filename: str) -> int:
    """
    Sum the calibration values in the input file.
    """

    file_path = path.join(PARENT_DIRECTORY, filename)

    with open(file_path, "r", encoding="utf-8") as file:
        return sum(extract_calibration_value(line) for line in file)


def main() -> None:
    """
    Execute the program.
    """

    inputs = [TEST_FILE_1, TEST_FILE_2, INPUT_FILE]

    for input_file in inputs:
        calibration_value_sum = sum_calibration_values_in_file(input_file)
        print(f"{input_file}: {calibration_value_sum}")


if __name__ == "__main__":
    main()
