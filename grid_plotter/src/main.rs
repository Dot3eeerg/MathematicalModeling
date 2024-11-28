use eframe::egui::{self, Color32, DragValue, Event, RichText, Vec2};
use egui_plot::{Legend, Line, PlotPoints, Polygon};
use std::fs::File;
use std::io::{self, BufRead};

fn main() -> eframe::Result {
    let mesh_points = read_mesh_from_file("grid/points").expect("Failed to read mesh points");
    let elements =
        read_elements_from_file("grid/finite_elements").expect("Failed to read elements");
    let dirichlet =
        read_dirichlet_from_file("grid/dirichlet").expect("Failed to read Dirichlet data");
    let neumann = read_neumann_from_file("grid/neumann").expect("Failed to read Neumann data");
    let solution = read_solution_from_file("grid/solution").expect("Failed to read solution");

    let options = eframe::NativeOptions::default();

    let plotter = GridPlotter::new(mesh_points, elements, dirichlet, neumann, solution);
    plotter.triangulate();
    eframe::run_native(
        "Grid Plotter",
        options,
        Box::new(|_cc| Ok(Box::new(plotter))),
    )
}

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

fn read_dirichlet_from_file(filename: &str) -> io::Result<Vec<usize>> {
    let file = File::open(filename)?;
    let reader = io::BufReader::new(file);
    Ok(reader
        .lines()
        .map(|line| line.unwrap().trim().parse().unwrap())
        .collect())
}

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

fn read_solution_from_file(filename: &str) -> io::Result<Vec<f64>> {
    let file = File::open(filename)?;
    let reader = io::BufReader::new(file);
    Ok(reader
        .lines()
        .map(|line| line.unwrap().trim().parse().unwrap())
        .collect())
}

struct Point {
    x: f64,
    y: f64,
}

struct GridPlotter {
    lock_x: bool,
    lock_y: bool,
    ctrl_to_zoom: bool,
    shift_to_horizontal: bool,
    show_triangles: bool,
    show_numbers: bool,
    show_points: bool,
    zoom_speed: f32,
    scroll_speed: f32,
    show_grid: bool,
    show_heatmap: bool,
    show_contours: bool,
    points: Vec<(f64, f64)>,
    elements: Vec<Vec<usize>>,
    triangles_vector: Vec<Vec<usize>>,
    points_vector: Vec<usize>,
    dirichlet: Vec<usize>,
    neumann: Vec<Vec<usize>>,
    solution: Vec<f64>,
    isolines_count: u16,
}

impl Default for GridPlotter {
    fn default() -> Self {
        Self {
            lock_x: false,
            lock_y: false,
            ctrl_to_zoom: false,
            shift_to_horizontal: false,
            show_triangles: false,
            zoom_speed: 1.0,
            scroll_speed: 1.0,
            show_grid: true,
            show_heatmap: true,
            show_contours: true,
            show_numbers: false,
            show_points: false,
            points: Vec::new(),
            elements: Vec::new(),
            triangles_vector: Vec::new(),
            points_vector: Vec::new(),
            dirichlet: Vec::new(),
            neumann: Vec::new(),
            solution: Vec::new(),
            isolines_count: 10,
        }
    }
}

impl GridPlotter {
    pub fn new(
        points: Vec<(f64, f64)>,
        elements: Vec<Vec<usize>>,
        dirichlet: Vec<usize>,
        neumann: Vec<Vec<usize>>,
        solution: Vec<f64>,
    ) -> Self {
        let instance = Self {
            lock_x: false,
            lock_y: false,
            ctrl_to_zoom: false,
            shift_to_horizontal: false,
            show_triangles: false,
            zoom_speed: 1.0,
            scroll_speed: 1.0,
            show_grid: true,
            show_heatmap: true,
            show_contours: true,
            show_numbers: false,
            show_points: false,
            points,
            elements,
            triangles_vector: Vec::new(),
            points_vector: Vec::with_capacity(3),
            dirichlet,
            neumann,
            solution,
            isolines_count: 10,
        };

        let triangles = instance.triangulate();

        Self {
            triangles_vector: triangles,
            ..instance
        }
    }

    fn triangulate(&self) -> Vec<Vec<usize>> {
        let mut triangles: Vec<Vec<usize>> = Vec::new();

        for element in &self.elements {
            if element.len() == 10 {
                // Create 8 triangles (2 for each rectangle)
                // First rectangle (top left)
                triangles.push(vec![element[0], element[1], element[9]]);
                triangles.push(vec![element[0], element[9], element[7]]);

                // Second rectangle (top right)
                triangles.push(vec![element[1], element[2], element[9]]);
                triangles.push(vec![element[2], element[3], element[9]]);

                // Third rectangle (bottom right)
                triangles.push(vec![element[9], element[3], element[4]]);
                triangles.push(vec![element[9], element[4], element[5]]);

                // Fourth rectangle (bottom left)
                triangles.push(vec![element[7], element[9], element[6]]);
                triangles.push(vec![element[9], element[5], element[6]]);
            }
        }

        triangles
    }

    fn get_element_state(&mut self, i: usize, binary_map: Vec<usize>) -> usize {
        self.points_vector.clear();

        self.points_vector = self.triangles_vector[i].clone();

        let bin: Vec<usize> = self.triangles_vector[i]
            .iter()
            .map(|&node| binary_map[node])
            .collect();

        let state = bin[2] * 4 + bin[1] * 2 + bin[0] * 1;

        if state == 0 || state == 7 {
            return state;
        }

        self.rearrange_bin(bin);

        state
    }

    fn rearrange_bin(&mut self, bin: Vec<usize>) {
        let ones_count = bin.iter().filter(|&&x| x == 1).count();

        if ones_count == 1 && bin.len() == 3 {
            if let Some(one_pos) = bin.iter().position(|&x| x == 1) {
                if one_pos != 0 {
                    self.points_vector.swap(0, one_pos);
                }
            }
        } else if ones_count == 2 && bin.len() == 3 {
            if let Some(zero_pos) = bin.iter().position(|&x| x == 0) {
                if zero_pos != 0 {
                    self.points_vector.swap(0, zero_pos);
                }
            }
        }
    }
}

impl eframe::App for GridPlotter {
    fn update(&mut self, ctx: &egui::Context, _: &mut eframe::Frame) {
        egui::SidePanel::left("options").show(ctx, |ui| {
            ui.checkbox(&mut self.lock_x, "Lock x axis").on_hover_text("Check to keep the X axis fixed, i.e., pan and zoom will only affect the Y axis");
            ui.checkbox(&mut self.lock_y, "Lock y axis").on_hover_text("Check to keep the Y axis fixed, i.e., pan and zoom will only affect the X axis");
            ui.checkbox(&mut self.ctrl_to_zoom, "Ctrl to zoom").on_hover_text("If unchecked, the behavior of the Ctrl key is inverted compared to the default controls\ni.e., scrolling the mouse without pressing any keys zooms the plot");
            ui.checkbox(&mut self.shift_to_horizontal, "Shift for horizontal scroll").on_hover_text("If unchecked, the behavior of the shift key is inverted compared to the default controls\ni.e., hold to scroll vertically, release to scroll horizontally");
            ui.checkbox(&mut self.show_grid,"Show grid").on_hover_text("Check to show grid on plot");
            ui.checkbox(&mut self.show_heatmap, "Show heatmap").on_hover_text("Check to show solution heatmap");
            ui.checkbox(&mut self.show_contours, "Show contours").on_hover_text("Check to show solution contour lines");
            ui.checkbox(&mut self.show_triangles, "Show triangulate grid").on_hover_text("Check to show triangulate grid");
            ui.checkbox(&mut self.show_points, "Show points on grid").on_hover_text("Check to show points");
            ui.checkbox(&mut self.show_numbers, "Show points numbers on grid").on_hover_text("Check to show point numbers");
            ui.horizontal(|ui| {
                ui.label("Isolines amount");
                integer_edit_field(ui, &mut self.isolines_count);
            });
            ui.horizontal(|ui| {
                ui.add(
                    DragValue::new(&mut self.zoom_speed)
                        .range(0.1..=2.0)
                        .speed(0.1),
                );
                ui.label("Zoom speed").on_hover_text("How fast to zoom in and out with the mouse wheel");
            });
            ui.horizontal(|ui| {
                ui.add(
                    DragValue::new(&mut self.scroll_speed)
                        .range(0.1..=100.0)
                        .speed(0.1),
                );
                ui.label("Scroll speed").on_hover_text("How fast to pan with the mouse wheel");
            });
        });
        egui::CentralPanel::default().show(ctx, |ui| {
            let (scroll, pointer_down, modifiers) = ui.input(|i| {
                let scroll = i.events.iter().find_map(|e| match e {
                    Event::MouseWheel {
                        unit: _,
                        delta,
                        modifiers: _,
                    } => Some(*delta),
                    _ => None,
                });
                (scroll, i.pointer.primary_down(), i.modifiers)
            });

            egui_plot::Plot::new("Grid plotter")
                .allow_zoom(false)
                .allow_drag(false)
                .allow_scroll(false)
                .legend(Legend::default())
                .show_grid(self.show_grid)
                .show(ui, |plot_ui| {
                    if let Some(mut scroll) = scroll {
                        if modifiers.ctrl == self.ctrl_to_zoom {
                            scroll = Vec2::splat(scroll.x + scroll.y);
                            let mut zoom_factor = Vec2::from([
                                (scroll.x * self.zoom_speed / 10.0).exp(),
                                (scroll.y * self.zoom_speed / 10.0).exp(),
                            ]);
                            if self.lock_x {
                                zoom_factor.x = 1.0;
                            }
                            if self.lock_y {
                                zoom_factor.y = 1.0;
                            }
                            plot_ui.zoom_bounds_around_hovered(zoom_factor);
                        } else {
                            if modifiers.shift == self.shift_to_horizontal {
                                scroll = Vec2::new(scroll.y, scroll.x);
                            }
                            if self.lock_x {
                                scroll.x = 0.0;
                            }
                            if self.lock_y {
                                scroll.y = 0.0;
                            }
                            let delta_pos = self.scroll_speed * scroll;
                            plot_ui.translate_bounds(delta_pos);
                        }
                    }
                    if plot_ui.response().hovered() && pointer_down {
                        let mut pointer_translate = -plot_ui.pointer_coordinate_drag_delta();
                        if self.lock_x {
                            pointer_translate.x = 0.0;
                        }
                        if self.lock_y {
                            pointer_translate.y = 0.0;
                        }
                        plot_ui.translate_bounds(pointer_translate);
                    }

                    let maximum = *self
                        .solution
                        .iter()
                        .max_by(|a, b| a.partial_cmp(b).unwrap())
                        .unwrap();
                    let minimum = *self
                        .solution
                        .iter()
                        .min_by(|a, b| a.partial_cmp(b).unwrap())
                        .unwrap();

                    if self.show_heatmap {
                        for element in &self.triangles_vector {
                            let vertices: Vec<[f64; 2]> = element
                                .iter()
                                .take(3)
                                .map(|&i| [self.points[i].0, self.points[i].1])
                                .collect();

                            let avg_value = element
                                .iter()
                                .take(3)
                                .map(|&i| self.solution[i])
                                .sum::<f64>()
                                / 3.0;
                            let color = interpolate_heat_color(avg_value, maximum, minimum);

                            if self.show_triangles {
                                plot_ui.polygon(
                                    Polygon::new(vertices)
                                        .fill_color(color)
                                        .stroke(egui::Stroke::new(1.0, egui::Color32::DARK_GRAY)),
                                );
                            } else {
                                plot_ui.polygon(
                                    Polygon::new(vertices)
                                        .fill_color(color)
                                        .stroke(egui::Stroke::new(1.0, color)),
                                );
                            }
                        }
                    } else {
                        for element in &self.elements {
                            if element.len() == 10 {
                                let vertices: Vec<[f64; 2]> = element
                                    .iter()
                                    .take(8)
                                    .map(|&i| [self.points[i].0, self.points[i].1])
                                    .collect();

                                let color = match element[8] {
                                    0 => Color32::LIGHT_BLUE,
                                    1 => Color32::GREEN,
                                    2 => Color32::GRAY,
                                    3 => Color32::KHAKI,
                                    4 => Color32::DARK_RED,
                                    5 => Color32::YELLOW,
                                    _ => Color32::BLACK,
                                };

                                plot_ui.polygon(
                                    Polygon::new(vertices)
                                        .fill_color(color)
                                        .stroke(egui::Stroke::new(1.0, egui::Color32::DARK_GRAY)),
                                );
                            }
                        }
                    }

                    if self.show_contours {
                        let mut binary_map: Vec<usize> = Vec::with_capacity(self.solution.len());
                        let mut points_to_render: Vec<[f64; 2]> = Vec::new();

                        let _step = (maximum - minimum) / (self.isolines_count as f64);

                        for i in 0..self.isolines_count + 1 {
                            let limit = minimum + (i as f64) * _step;

                            make_binary_map(limit, &mut binary_map, self);

                            for i in 0..self.triangles_vector.len() - 1 {
                                let state = self.get_element_state(i, binary_map.clone());

                                if state == 0 || state == 7 {
                                    continue;
                                }

                                let p1 = Point {
                                    x: self.points[self.points_vector[0]].0,
                                    y: self.points[self.points_vector[0]].1,
                                };
                                let p2 = Point {
                                    x: self.points[self.points_vector[1]].0,
                                    y: self.points[self.points_vector[1]].1,
                                };
                                let p3 = Point {
                                    x: self.points[self.points_vector[2]].0,
                                    y: self.points[self.points_vector[2]].1,
                                };

                                points_to_render.push(interpolate_value(
                                    &p1,
                                    &p2,
                                    self.solution[self.points_vector[0]],
                                    self.solution[self.points_vector[1]],
                                    limit,
                                ));

                                points_to_render.push(interpolate_value(
                                    &p1,
                                    &p3,
                                    self.solution[self.points_vector[0]],
                                    self.solution[self.points_vector[2]],
                                    limit,
                                ));
                            }
                            let plot_points: PlotPoints = points_to_render
                                .iter()
                                .map(|&[x, y]| [x, y])
                                .collect::<Vec<[f64; 2]>>()
                                .into();
                            plot_ui.line(Line::new(plot_points).color(Color32::DARK_GREEN));

                            points_to_render.clear();
                        }
                    }

                    if self.show_points {
                        let grid_points: PlotPoints = self
                            .points
                            .iter()
                            .map(|&(x, y)| [x, y])
                            .collect::<Vec<[f64; 2]>>()
                            .into();
                        plot_ui.points(
                            egui_plot::Points::new(grid_points)
                                .radius(5.0)
                                .color(Color32::BLACK)
                                .name("Mesh Points"),
                        );

                        if self.show_numbers {
                            for (i, &(x, y)) in self.points.iter().enumerate() {
                                plot_ui.text(
                                    egui_plot::Text::new(
                                        [x + 0.15, y].into(),
                                        RichText::new(format!("{}", i)).size(15.0),
                                    )
                                    .color(Color32::DARK_BLUE),
                                );
                            }
                        }
                    }

                    let dirichlet_points: Vec<_> =
                        self.dirichlet.iter().map(|&i| self.points[i]).collect();
                    let dirichlet_plot_points: PlotPoints = dirichlet_points
                        .into_iter()
                        .map(|(x, y)| [x, y])
                        .collect::<Vec<[f64; 2]>>()
                        .into();
                    plot_ui.points(
                        egui_plot::Points::new(dirichlet_plot_points)
                            .name("Dirichlet")
                            .shape(egui_plot::MarkerShape::Circle)
                            .radius(5.0)
                            .color(egui::Color32::ORANGE),
                    );

                    for neumann_edge in &self.neumann {
                        if neumann_edge.len() == 2 {
                            let neumann_plot_points: Vec<_> = neumann_edge
                                .iter()
                                .map(|&i| [self.points[i].0, self.points[i].1])
                                .collect();
                            plot_ui.line(
                                egui_plot::Line::new(neumann_plot_points)
                                    .name("Neumann Edges")
                                    .color(Color32::RED)
                                    .width(2.0),
                            );
                        }
                    }
                });
        });
    }
}

fn integer_edit_field(ui: &mut egui::Ui, value: &mut u16) -> egui::Response {
    let mut tmp_value = format!("{}", value);
    let res = ui.text_edit_singleline(&mut tmp_value);
    if let Ok(result) = tmp_value.parse() {
        *value = result;
    }
    res
}

fn interpolate_heat_color(value: f64, max: f64, min: f64) -> Color32 {
    let normalized = (value - min) / max - min;
    let r = (normalized * 255.0) as u8;
    let b = ((1.0 - normalized) * 255.0) as u8;
    Color32::from_rgb(r, 0, b)
}

fn interpolate_value(p1: &Point, p2: &Point, v1: f64, v2: f64, v: f64) -> [f64; 2] {
    let t = (v - v1) / (v2 - v1);
    let x = p1.x + t * (p2.x - p1.x);
    let y = p1.y + t * (p2.y - p1.y);
    let p: [f64; 2] = [x, y];
    p
}

fn make_binary_map(limit: f64, array: &mut Vec<usize>, grid_plotter: &GridPlotter) {
    array.clear();

    for i in 0..array.capacity() {
        if grid_plotter.solution[i] < limit {
            array.push(0);
        } else {
            array.push(1);
        }
    }
}
