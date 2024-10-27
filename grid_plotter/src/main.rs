use eframe::egui::{self, Color32, DragValue, Event, Vec2};
use egui_plot::{Legend, PlotPoints, Polygon};
use std::fs::File;
use std::io::{self, BufRead};

fn main() -> eframe::Result {
    let mesh_points = read_mesh_from_file("grid/points").expect("Failed to read mesh points");
    let elements =
        read_elements_from_file("grid/finite_elements").expect("Failed to read elements");
    let dirichlet =
        read_dirichlet_from_file("grid/dirichlet").expect("Failed to read Dirichlet data");
    let neumann = read_neumann_from_file("grid/neumann").expect("Failed to read Neumann data");

    let options = eframe::NativeOptions::default();
    eframe::run_native(
        "Grid Plotter",
        options,
        Box::new(|_cc| {
            Ok(Box::new(GridPlotter::new(
                mesh_points,
                elements,
                dirichlet,
                neumann,
            )))
        }),
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

struct GridPlotter {
    lock_x: bool,
    lock_y: bool,
    ctrl_to_zoom: bool,
    shift_to_horizontal: bool,
    zoom_speed: f32,
    scroll_speed: f32,
    show_grid: bool,
    points: Vec<(f64, f64)>,
    elements: Vec<Vec<usize>>,
    dirichlet: Vec<usize>,
    neumann: Vec<Vec<usize>>,
}

impl Default for GridPlotter {
    fn default() -> Self {
        Self {
            lock_x: false,
            lock_y: false,
            ctrl_to_zoom: false,
            shift_to_horizontal: false,
            zoom_speed: 1.0,
            scroll_speed: 1.0,
            show_grid: true,
            points: Vec::new(),
            elements: Vec::new(),
            dirichlet: Vec::new(),
            neumann: Vec::new(),
        }
    }
}

impl GridPlotter {
    pub fn new(
        points: Vec<(f64, f64)>,
        elements: Vec<Vec<usize>>,
        dirichlet: Vec<usize>,
        neumann: Vec<Vec<usize>>,
    ) -> Self {
        Self {
            lock_x: false,
            lock_y: false,
            ctrl_to_zoom: false,
            shift_to_horizontal: false,
            zoom_speed: 1.0,
            scroll_speed: 1.0,
            show_grid: true,
            points,
            elements,
            dirichlet,
            neumann,
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

                    for element in &self.elements {
                        if element.len() == 5 {
                            let vertices: Vec<[f64; 2]> = element
                                .iter()
                                .take(4)
                                .map(|&i| [self.points[i].0, self.points[i].1])
                                .collect();

                            let color = match element[4] {
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
