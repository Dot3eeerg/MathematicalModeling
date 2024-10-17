import matplotlib.pyplot as plt
from matplotlib.patches import Polygon
import random

# Function to read and parse the mesh data
def read_mesh(file_data):
    points = []
    lines = file_data.strip().split('\n')
    for line in lines:
        x, y = map(float, line.split())
        points.append((x, y))
    return points

# Function to read and parse the mesh data from a file
def read_mesh_from_file(filename):
    points = []
    with open(filename, 'r') as file:
        for line in file:
            x, y = map(float, line.split())
            points.append((x, y))
    return points

# Function to read the elements from a file
def read_elements_from_file(filename):
    elements = []
    with open(filename, 'r') as file:
        for line in file:
            indices = list(map(int, line.split()))
            elements.append(indices)
    return elements

def read_dirichlet_from_file(filename):
    dirichlet = []
    with open(filename, 'r') as file:
        for line in file:
            x = map(float, line)
            dirichlet.append(int(line.strip()))
    return dirichlet

def read_neumann_from_file(filename):
    neumann = []
    with open(filename, 'r') as file:
        for line in file:
            indices = list(map(int, line.split()))
            neumann.append(indices)
    return neumann

# Function to visualize the grid with rectangular elements and connections between points
def plot_mesh_with_elements(points, elements, dirichlet = None, neumann = None):
    # Create the plot and axes
    fig, ax = plt.subplots(figsize=(10, 6))

    # Loop through each element
    for element in elements:
        if len(element) == 5:
            # Create a polygon for the rectangular element using its vertices
            vertices = [points[element[0]],
                        points[element[1]],
                        points[element[2]],
                        points[element[3]]]
            # Random color for each element
            colorElem = ("cyan", "green", "grey", "blue")

            # Create a polygon and add it to the plot
            polygon = Polygon(vertices, edgecolor='black', facecolor=colorElem[element[4]], alpha=0.5)
            ax.add_patch(polygon)

    # Separate x and y coordinates
    x_coords, y_coords = zip(*points)

    # Plot the points
    ax.scatter(x_coords, y_coords, color='black', marker='o', label="Конечно-элементные узлы")

    if dirichlet is not None:
        dirichlet_coords = [points[i] for i in dirichlet]
        dx, dy = zip(*dirichlet_coords)
        ax.scatter(dx, dy, color='blue', marker='o', label="Узлы с первым краевым условием")

    if neumann is not None:
        neumann_label_added = False
        for pair in neumann:
            point1, point2 = points[pair[0]], points[pair[1]]

            ax.scatter([point1[0], point2[0]], [point1[1], point2[1]], color='red', marker='o', label='Грани со вторым краевым условием' if not neumann_label_added else "")

            ax.plot([point1[0], point2[0]], [point1[1], point2[1]], color='red', linewidth=2)
            neumann_label_added = True

    plt.legend()
    # Display the plot
    plt.show()

# Reading the mesh data and elements from files
mesh_points = read_mesh_from_file("points")  # Replace "mesh.txt" with your mesh file path
elements = read_elements_from_file("finite_elements")  # Replace "elements.txt" with your elements file path
dirichlet = read_dirichlet_from_file("dirichlet")
neumann = read_neumann_from_file("neumann")

# Plotting the mesh grid with rectangular elements and connections
#plot_mesh_with_elements(mesh_points, elements, dirichlet, neumann)
# plot_mesh_with_elements(mesh_points, elements, neumann = neumann)
plot_mesh_with_elements(mesh_points, elements, dirichlet, neumann)
