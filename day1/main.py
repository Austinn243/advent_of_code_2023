"""
Advent of Code 2023, Day 1
Trebuchet?!
https://adventofcode.com/2023/day/1
"""

from os import path


INPUT_FILE = "input.txt"
TEST_FILE_1 = "test1.txt"
TEST_FILE_2 = "test2.txt"

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
    """Extract the first digit from a jumbled calibration value."""

    def loop(remaining: str) -> int:
        if remaining[0].isdigit():
            return int(remaining[0])

        for key, value in DIGITS_TO_VALUES.items():
            if remaining.startswith(key):
                return value

        return loop(remaining[1:])

    return loop(line)


def extract_last_digit(line: str) -> int:
    """Extract the last digit from a jumbled calibration value."""

    def loop(remaining: str) -> int:
        if remaining[-1].isdigit():
            return int(remaining[-1])

        for key, value in DIGITS_TO_VALUES.items():
            if remaining.endswith(key):
                return value

        return loop(remaining[:-1])

    return loop(line)


def extract_calibration_value(line: str) -> int:
    """Retrieve the calibration value from a jumbled calibration value."""

    first_digit = extract_first_digit(line)
    last_digit = extract_last_digit(line)

    return (first_digit * 10) + last_digit


def read_calibration_document(file_path: str) -> list[str]:
    """Read the jumbled calibration values from the input file."""

    with open(file_path, encoding="utf-8") as file:
        return file.readlines()


def sum_calibration_values(calibration_values: list[str]) -> int:
    """Sum the calibration values found in the jumbled calibration values."""

    return sum(extract_calibration_value(line) for line in calibration_values)


def main() -> None:
    """Execute the program."""

    input_file = INPUT_FILE
    file_path = path.join(path.dirname(__file__), input_file)

    jumbled_calibration_values = read_calibration_document(file_path)

    calibration_value_sum = sum_calibration_values(jumbled_calibration_values)
    print(f"The sum of the calibration values is {calibration_value_sum}")


if __name__ == "__main__":
    main()
