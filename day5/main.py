"""
Advent of Code 2023, Day 5
"""

from os import path
from typing import Iterator, Iterable, Sequence


INPUT_FILE = "input.txt"
TEST_FILE = "test.txt"


def chunk(seq: Sequence[int], size: int) -> Iterable[Sequence[int]]:
    """
    Split the given sequence into chunks of the given size.
    """

    return (seq[pos : pos + size] for pos in range(0, len(seq), size))


Mapping = tuple[range, int]


class CategoryMap:
    """
    Describes mappings within a single category of gardening components.
    """

    def __init__(self) -> None:
        self.mappings = []

    def add_mapping(self, mapping: Mapping) -> None:
        """
        Add the given mapping to the category map.
        """

        self.mappings.append(mapping)

    def map(self, number: int) -> int:
        """
        Map the given number from the source set to the destination set.
        """

        for mapping in self.mappings:
            source_range, destination_start = mapping

            if number in source_range:
                return destination_start + number - source_range.start

        return number


class Almanac:
    """
    Describes the mappings between the various categories of gardening components.
    """

    def __init__(
        self,
        seeds: list[int],
        seed_to_soil: CategoryMap,
        soil_to_fertilizer: CategoryMap,
        fertilizer_to_water: CategoryMap,
        water_to_light: CategoryMap,
        light_to_temperature: CategoryMap,
        temperature_to_humidity: CategoryMap,
        humidity_to_location: CategoryMap,
    ):
        self.seeds = seeds
        self.seed_to_soil = seed_to_soil
        self.soil_to_fertilizer = soil_to_fertilizer
        self.fertilizer_to_water = fertilizer_to_water
        self.water_to_light = water_to_light
        self.light_to_temperature = light_to_temperature
        self.temperature_to_humidity = temperature_to_humidity
        self.humidity_to_location = humidity_to_location

        self.seed_to_location = {}

    def find_location_for_seed(self, seed: int) -> int:
        """
        Determine the location number that corresponds to the given seed number.
        """

        if seed in self.seed_to_location:
            return self.seed_to_location[seed]

        soil = self.seed_to_soil.map(seed)
        fertilizer = self.soil_to_fertilizer.map(soil)
        water = self.fertilizer_to_water.map(fertilizer)
        light = self.water_to_light.map(water)
        temperature = self.light_to_temperature.map(light)
        humidity = self.temperature_to_humidity.map(temperature)
        location = self.humidity_to_location.map(humidity)

        self.seed_to_location[seed] = location

        return location

    def find_lowest_location_number_for_individual_seeds(self) -> int:
        """
        Determine the lowest location number that corresponds to
        any of the seed numbers in the almanac.
        """

        return min(self.find_location_for_seed(seed) for seed in self.seeds)

    def find_lowest_location_number_for_seed_ranges(self) -> int:
        """
        Determine the lowest location number that corresponds to
        any of the seed numbers within the pairs of seed ranges in the almanac.
        """

        pairs = chunk(self.seeds, 2)
        seed_ranges = [range(pair[0], pair[0] + pair[1]) for pair in pairs]

        return min(
            self.find_location_for_seed(seed)
            for seed_range in seed_ranges
            for seed in seed_range
        )


def extract_seeds(line: str) -> list[int]:
    """
    Retrieve the seeds from the first line of the input file.
    """

    segments = iter(line.split())
    _ = next(segments)

    return [int(segment) for segment in segments]


def extract_mapping(line: str) -> Mapping:
    """
    Retrieve the mapping from the given line of the input file.
    """

    destination_start, source_start, size = (int(segment) for segment in line.split())
    source_range = range(source_start, source_start + size)

    return (source_range, destination_start)


def extract_category_map(lines: Iterator[str]) -> CategoryMap:
    """
    Extract the next category map from the input file.
    """

    category_map = CategoryMap()

    for line in lines:
        if line == "\n":
            break

        if line[0].isalpha():
            continue

        category_map.add_mapping(extract_mapping(line))

    return category_map


def read_almanac(filename: str) -> Almanac:
    """
    Read the almanac data from the input file.
    """

    file_path = path.join(path.dirname(__file__), filename)

    with open(file_path, "r", encoding="utf-8") as file:
        lines = iter(file.readlines())

    seeds = extract_seeds(next(lines))
    _ = next(lines)
    _ = next(lines)
    seed_to_soil = extract_category_map(lines)
    soil_to_fertilizer = extract_category_map(lines)
    fertilizer_to_water = extract_category_map(lines)
    water_to_light = extract_category_map(lines)
    light_to_temperature = extract_category_map(lines)
    temperature_to_humidity = extract_category_map(lines)
    humidity_to_location = extract_category_map(lines)

    return Almanac(
        seeds,
        seed_to_soil,
        soil_to_fertilizer,
        fertilizer_to_water,
        water_to_light,
        light_to_temperature,
        temperature_to_humidity,
        humidity_to_location,
    )


def run(filename: str) -> None:
    """
    Run the program.
    """

    almanac = read_almanac(filename)
    print(almanac)

    print(almanac.find_lowest_location_number_for_individual_seeds())
    print(almanac.find_lowest_location_number_for_seed_ranges())


def main():
    """
    The main function of the program.
    """

    run(TEST_FILE)
    # run(INPUT_FILE)


if __name__ == "__main__":
    main()
