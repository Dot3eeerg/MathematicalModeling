use full_palette::GREY;
use plotters::prelude::*;
use plotters::style::ShapeStyle;
use std::fs::File;
use std::io::{self, BufRead};

// Function to read mesh data from a file
fn read_mesh_from_file(filename: &str) -> io::Result<Vec<(f64, f64)>> {
    let file = File::open(filename)?;
    let reader = io::BufReader::new(file);
    Ok(reader
        .lines()
        .map(|line| {
            let coords: Vec<f64> = line
                .unwrap()
                .split_whitespace()
                .map(|num| num.parse().unwrap())
                .collect();
            (coords[0], coords[1])
        })
        .collect())
}

// Function to read elements from a file
fn read_elements_from_file(filename: &str) -> io::Result<Vec<Vec<usize>>> {
    let file = File::open(filename)?;
    let reader = io::BufReader::new(file);
    Ok(reader
        .lines()
        .map(|line| {
            line.unwrap()
                .split_whitespace()
                .map(|num| num.parse().unwrap())
                .collect()
        })
        .collect())
}

// Function to read Dirichlet data from a file
fn read_dirichlet_from_file(filename: &str) -> io::Result<Vec<usize>> {
    let file = File::open(filename)?;
    let reader = io::BufReader::new(file);
    Ok(reader
        .lines()
        .map(|line| line.unwrap().trim().parse().unwrap())
        .collect())
}

// Function to read Neumann data from a file
fn read_neumann_from_file(filename: &str) -> io::Result<Vec<Vec<usize>>> {
    let file = File::open(filename)?;
    let reader = io::BufReader::new(file);
    Ok(reader
        .lines()
        .map(|line| {
            line.unwrap()
                .split_whitespace()
                .map(|num| num.parse().unwrap())
                .collect()
        })
        .collect())
}

// Function to visualize the grid with rectangular elements and connections between points
fn plot_mesh_with_elements(
    points: &[(f64, f64)],
    elements: &[Vec<usize>],
    dirichlet: Option<&Vec<usize>>,
    neumann: Option<&Vec<Vec<usize>>>,
) -> Result<(), Box<dyn std::error::Error>> {
    let root_area = BitMapBackend::new("grid/grid.png", (1920, 1080)).into_drawing_area();
    root_area.fill(&WHITE)?;

    let mut chart = ChartBuilder::on(&root_area)
        .caption("Finite Element Mesh", ("sans-serif", 50))
        .x_label_area_size(35)
        .y_label_area_size(35)
        .build_cartesian_2d(-1.0..14.0, -1.0..14.0)?;

    chart.configure_mesh().draw()?;

    // Plot elements as polygons
    for element in elements {
        if element.len() == 5 {
            let vertices: Vec<(f64, f64)> =
                element.iter().take(4).map(|&index| points[index]).collect();

            let color = match element[4] {
                0 => &CYAN,
                1 => &GREEN,
                2 => &GREY,
                3 => &BLUE,
                _ => &BLACK,
            };

            // Draw the polygon using Polygon
            let polygon = Polygon::new(
                vertices.clone(),
                ShapeStyle {
                    color: color.to_rgba(),
                    filled: true,
                    stroke_width: 0,
                },
            );

            // Draw lines between vertices to separate polygons
            for i in 0..vertices.len() {
                let start = vertices[i];
                let end = vertices[(i + 1) % vertices.len()];
                chart.draw_series(LineSeries::new(vec![start, end], BLACK.stroke_width(2)))?;
            }

            chart.draw_series(std::iter::once(polygon))?;
        }
    }

    // Plot points
    chart.draw_series(
        points
            .iter()
            .map(|&(x, y)| Circle::new((x, y), 3, BLACK.filled())),
    )?;

    // Plot Dirichlet boundary points
    if let Some(dirichlet_points) = dirichlet {
        chart.draw_series(dirichlet_points.iter().map(|&index| {
            let (x, y) = points[index];
            Circle::new((x, y), 5, BLUE.filled())
        }))?;
    }

    // Plot Neumann boundary edges
    if let Some(neumann_edges) = neumann {
        for edge in neumann_edges {
            let (x1, y1) = points[edge[0]];
            let (x2, y2) = points[edge[1]];
            chart.draw_series(LineSeries::new(vec![(x1, y1), (x2, y2)], &RED))?;
        }
    }

    root_area.present()?;
    Ok(())
}

fn main() -> Result<(), Box<dyn std::error::Error>> {
    // Reading the mesh data and elements from files
    let mesh_points = read_mesh_from_file("grid/points")?; // Replace with actual file path
    let elements = read_elements_from_file("grid/finite_elements")?; // Replace with actual file path
    let dirichlet = read_dirichlet_from_file("grid/dirichlet")?;
    let neumann = read_neumann_from_file("grid/neumann")?;

    // Plotting the mesh grid with rectangular elements and connections
    plot_mesh_with_elements(&mesh_points, &elements, Some(&dirichlet), Some(&neumann))?;

    Ok(())
}
