"""
Advent of Code 2023, Day 25
Snowverload
https://adventofcode.com/2023/day/25
"""

from collections import defaultdict
from os import path
from pprint import pprint
from typing import Optional

Graph = dict[str, list[str]]

INPUT_FILE = "input.txt"
TEST_FILE = "test.txt"

PARENT_DIRECTORY = path.dirname(path.abspath(__file__))


def extract_components_from_line(line: str) -> tuple[str, list[str]]:
    """
    Parse a line, returning the source component and a list of
    its connected components.
    """

    segments = line.split()

    # NOTE: The source component in the text data is followed by a comma,
    # designating that the following components are connected to it. We
    # remove the comma from the source component here using slicing to
    # avoid requiring a more sophisticated parsing method, such as regex.
    source = segments[0][:-1]
    connected_components = segments[1:]

    return source, connected_components


def read_graph_from_file(filename: str) -> Graph:
    """
    Read the input file and return the graph of connected components.
    """

    file_path = path.join(PARENT_DIRECTORY, filename)

    graph = defaultdict(list)

    with open(file_path, "r", encoding="utf-8") as file:
        for line in file:
            source_component, connected_components = extract_components_from_line(line)

            for component in connected_components:
                graph[source_component].append(component)
                graph[component].append(source_component)

    return graph


def find_bridges(graph: Graph) -> list[tuple[str, str]]:
    """
    Identify the bridges in the graph using Tarjan's algorithm.
    """

    node_ids = {}
    low_links = {}

    bridges = []

    def visit(node: str, parent: Optional[str] = None) -> None:
        node_ids[node] = len(node_ids)
        low_links[node] = node_ids[node]

        for neighbor in graph[node]:
            if neighbor == parent:
                continue

            if neighbor in node_ids:
                low_links[node] = min(low_links[node], node_ids[neighbor])
                continue

            visit(neighbor, node)

            low_links[node] = min(low_links[node], low_links[neighbor])

            if low_links[neighbor] > node_ids[node]:
                bridges.append((node, neighbor))

    for node in graph:
        if node not in node_ids:
            visit(node)

    return bridges


def product_of_ssc_sizes(graph: Graph) -> int:
    """
    Calculate the product of the sizes of the strongly connected components
    created by disconnecting 3 pairs of components.
    """

    bridges = find_bridges(graph)
    pprint(bridges)


def run(filename: str) -> None:
    """
    Execute the program.
    """

    graph = read_graph_from_file(filename)
    pprint(graph)

    product_of_ssc_sizes(graph)


def main() -> None:
    """
    Execute the program.
    """

    # run(TEST_FILE)

    graph = {
        "a": ["b"],
        "b": ["a", "c", "d"],
        "c": ["b", "d"],
        "d": ["b", "c", "e"],
        "e": ["d", "f"],
        "f": ["e"],
    }

    bridges = find_bridges(graph)
    pprint(bridges)


if __name__ == "__main__":
    main()
