"""
Advent of Code 2023, Day 5
If You Give A Seed A Fertilizer
https://adventofcode.com/2023/day/5
"""

from collections.abc import Iterable, Iterator, Sequence
from os import path
from typing import NamedTuple, Optional

INPUT_FILE = "input.txt"
TEST_FILE = "test.txt"


# CONSIDER: It seems that, if a seed lands in a given range, the destination
# location is the seed number plus the difference between the start of the
# source range and the start of the destination range.
#
# For example, with the first test example, water 81 maps to light 74. The
# corresponding mapping has a source range with start 18, a destination range
# with start 25. The difference between the source start and destination start
# is equal to 18 - 25 = -7. Therefore, the destination location is 81 - 7 = 74.
#
# Verify this with the other examples to see if this is a general rule.

# CONSIDER: While we're using an interval tree style approach to mapping the
# categories, the actual mappings could also be represented using a graph
# instead, where each range is a node and the edges represent the mappings
# to ranges in other categories.

# CONSIDER: Is it possible to determine the lowest location number by working
# backwards from the location number to a seed number? Alternatively, could
# we calculate this using some kind of formula based on the mappings?

# CONSIDER: When working with a smaller set of seeds, the algorithm works
# very quickly. However, when working with much larger sets of seeds, it
# can't keep up, despite using what seems to be a fairly efficient approach.
# Maybe the problem lies in how we're using the seed ranges for the second
# example? Maybe we don't have to evaluate all the possible seeds or,
# alternatively, we could use optimization techniques to speed up the process.


def chunk(seq: Sequence[int], size: int) -> Iterable[Sequence[int]]:
    """Split the given sequence into chunks of the given size."""

    return (seq[pos : pos + size] for pos in range(0, len(seq), size))


Mapping = tuple[range, int]


class CategoryNode:
    """Describes a node in a our custom category mappings binary search tree."""

    destination_start: int
    interval: range
    key: int
    left: Optional["CategoryNode"]
    right: Optional["CategoryNode"]

    def __init__(self, interval: range, destination_start: int) -> None:
        """Create a new node with the given interval and destination start."""

        self.destination_start = destination_start
        self.interval = interval
        self.left = None
        self.right = None

        self.key = interval.start

    def add_child(self, node: "CategoryNode") -> None:
        """Add the given node as a child of this node."""

        if node.key < self.key:
            if self.left is None:
                self.left = node
            else:
                self.left.add_child(node)
        else:
            if self.right is None:
                self.right = node
            else:
                self.right.add_child(node)

    def map(self, number: int) -> int:
        """Map the given number from the source set to the destination set."""

        if number in self.interval:
            return self.destination_start + number - self.interval.start

        if number < self.key and self.left is not None:
            return self.left.map(number)

        if number > self.key and self.right is not None:
            return self.right.map(number)

        return number


class CategoryTree:
    """Maps a set of source numbers to a set of destination numbers."""

    root: Optional[CategoryNode] = None

    def add_mapping(self, mapping: Mapping) -> None:
        """Add the given mapping to the category map."""

        interval, destination_start = mapping
        node = CategoryNode(interval, destination_start)

        if self.root is None:
            self.root = node
            return

        self.root.add_child(node)

    def map(self, number: int) -> int:
        """Map the given number from the source set to the destination set."""

        if not self.root:
            return number

        return self.root.map(number)


class Almanac:
    """Describes the mappings between the various categories of gardening components."""

    def __init__(
        self,
        seed_to_soil: CategoryTree,
        soil_to_fertilizer: CategoryTree,
        fertilizer_to_water: CategoryTree,
        water_to_light: CategoryTree,
        light_to_temperature: CategoryTree,
        temperature_to_humidity: CategoryTree,
        humidity_to_location: CategoryTree,
    ) -> None:
        """Create a new almanac with the given mappings."""

        self.seed_to_soil = seed_to_soil
        self.soil_to_fertilizer = soil_to_fertilizer
        self.fertilizer_to_water = fertilizer_to_water
        self.water_to_light = water_to_light
        self.light_to_temperature = light_to_temperature
        self.temperature_to_humidity = temperature_to_humidity
        self.humidity_to_location = humidity_to_location

        self.seed_to_location = {}

    def find_location_for_seed(self, seed: int) -> int:
        """Determine the location number that corresponds to the given seed number."""

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


class GardeningResources(NamedTuple):
    """Represents the resources needed to plant a garden."""

    seeds: list[int]
    almanac: Almanac


def read_gardening_resources(file_path: str) -> GardeningResources:
    """Read the almanac data from the input file."""

    with open(file_path, encoding="utf-8") as file:
        lines = iter(file.readlines())

    seeds = parse_seeds(next(lines))
    _ = next(lines)
    _ = next(lines)
    seed_to_soil = parse_category(lines)
    soil_to_fertilizer = parse_category(lines)
    fertilizer_to_water = parse_category(lines)
    water_to_light = parse_category(lines)
    light_to_temperature = parse_category(lines)
    temperature_to_humidity = parse_category(lines)
    humidity_to_location = parse_category(lines)

    almanac = Almanac(
        seed_to_soil,
        soil_to_fertilizer,
        fertilizer_to_water,
        water_to_light,
        light_to_temperature,
        temperature_to_humidity,
        humidity_to_location,
    )

    return GardeningResources(seeds, almanac)


def parse_seeds(line: str) -> list[int]:
    """Parse seed numbers from a line of text."""

    segments = iter(line.split())
    _ = next(segments)

    return [int(segment) for segment in segments]


def parse_mapping(line: str) -> Mapping:
    """Parse a mapping from a line of text."""

    destination_start, source_start, size = (int(segment) for segment in line.split())
    source_range = range(source_start, source_start + size)

    return (source_range, destination_start)


def parse_category(lines: Iterator[str]) -> CategoryTree:
    """Parse the next category from lines of text."""

    category_map = CategoryTree()

    for line in lines:
        if line == "\n":
            break

        if line[0].isalpha():
            continue

        category_map.add_mapping(parse_mapping(line))

    return category_map


def find_lowest_location_number_for_individual_seeds(
    seeds: list[int],
    almanac: Almanac,
) -> int:
    """Find the lowest location number for the given seeds."""

    return min(almanac.find_location_for_seed(seed) for seed in seeds)


def find_lowest_location_number_for_seed_ranges(
    seeds: list[int],
    almanac: Almanac,
) -> int:
    """Find the lowest location number for the seeds in the given ranges."""

    pairs = chunk(seeds, 2)
    seed_ranges = [range(start, start + size) for start, size in pairs]

    return min(
        almanac.find_location_for_seed(seed)
        for seed_range in seed_ranges
        for seed in seed_range
    )


def main() -> None:
    """Read almanac data from an input file and process it."""

    input_file = INPUT_FILE
    file_path = path.join(path.dirname(__file__), input_file)

    seeds, almanac = read_gardening_resources(file_path)

    print(find_lowest_location_number_for_individual_seeds(seeds, almanac))
    print(find_lowest_location_number_for_seed_ranges(seeds, almanac))


if __name__ == "__main__":
    main()
