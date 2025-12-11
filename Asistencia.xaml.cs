using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace WpfApp1
{
    public partial class Asistencia : Window
    {
        private string connectionString = "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.izfxvtasfylqalgnkwia;Password=Mandachitos34;SSL Mode=Require;Trust Server Certificate=true";
        private string rolUsuario;

        public Asistencia(string rol)
        {
            InitializeComponent();
            rolUsuario = rol;
            CargarUsuarios();
            dpFecha.SelectedDate = DateTime.Today;
            CargarHistorial();
        }

        private void CargarUsuarios()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand("SELECT id_empleado, nombre FROM empleado", conn);
                var reader = cmd.ExecuteReader();
                var usuarios = new List<Tuple<int, string>>();
                while (reader.Read())
                {
                    usuarios.Add(Tuple.Create(reader.GetInt32(0), reader.GetString(1)));
                }
                cbUsuarios.ItemsSource = usuarios;
                cbUsuarios.DisplayMemberPath = "Item2";
                cbUsuarios.SelectedValuePath = "Item1";
            }
        }

        private void BtnEntrada_Click(object sender, RoutedEventArgs e)
        {
            if (cbUsuarios.SelectedValue == null || dpFecha.SelectedDate == null)
            {
                MessageBox.Show("Seleccione usuario y fecha.");
                return;
            }
            int idEmpleado = (int)cbUsuarios.SelectedValue;
            DateTime fecha = dpFecha.SelectedDate.Value.Date;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                // Validar doble marcación
                var cmdCheck = new NpgsqlCommand("SELECT hora_entrada FROM asistencia WHERE id_empleado=@id AND fecha=@fecha", conn);
                cmdCheck.Parameters.AddWithValue("id", idEmpleado);
                cmdCheck.Parameters.AddWithValue("fecha", fecha);
                var existe = cmdCheck.ExecuteScalar();
                if (existe != null && existe != DBNull.Value)
                {
                    MessageBox.Show("Ya se registró la entrada para este usuario y fecha.");
                    return;
                }
                // Insertar registro
                var cmd = new NpgsqlCommand("INSERT INTO asistencia (id_empleado, fecha, hora_entrada) VALUES (@id, @fecha, @hora)", conn);
                cmd.Parameters.AddWithValue("id", idEmpleado);
                cmd.Parameters.AddWithValue("fecha", fecha);
                cmd.Parameters.AddWithValue("hora", DateTime.Now);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Entrada registrada.");
                CargarHistorial();
            }
        }

        private void BtnSalida_Click(object sender, RoutedEventArgs e)
        {
            if (cbUsuarios.SelectedValue == null || dpFecha.SelectedDate == null)
            {
                MessageBox.Show("Seleccione usuario y fecha.");
                return;
            }
            int idEmpleado = (int)cbUsuarios.SelectedValue;
            DateTime fecha = dpFecha.SelectedDate.Value.Date;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                // Validar marcación
                var cmdCheck = new NpgsqlCommand("SELECT hora_salida FROM asistencia WHERE id_empleado=@id AND fecha=@fecha", conn);
                cmdCheck.Parameters.AddWithValue("id", idEmpleado);
                cmdCheck.Parameters.AddWithValue("fecha", fecha);
                var salida = cmdCheck.ExecuteScalar();
                if (salida != null && salida != DBNull.Value)
                {
                    MessageBox.Show("Ya se registró la salida para este usuario y fecha.");
                    return;
                }
                // Actualizar registro
                var cmd = new NpgsqlCommand("UPDATE asistencia SET hora_salida=@hora WHERE id_empleado=@id AND fecha=@fecha", conn);
                cmd.Parameters.AddWithValue("hora", DateTime.Now);
                cmd.Parameters.AddWithValue("id", idEmpleado);
                cmd.Parameters.AddWithValue("fecha", fecha);
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    MessageBox.Show("Primero debe registrar la entrada.");
                }
                else
                {
                    MessageBox.Show("Salida registrada.");
                    CargarHistorial();
                }
            }
        }

        private void BtnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            CargarHistorial();
        }

        private void BtnMostrarTodo_Click(object sender, RoutedEventArgs e)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand("SELECT e.nombre, a.fecha, a.hora_entrada, a.hora_salida FROM asistencia a JOIN empleado e ON a.id_empleado = e.id_empleado ORDER BY a.fecha DESC", conn);
                var da = new NpgsqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                dgAsistencias.ItemsSource = dt.DefaultView;
            }
        }

        private void CargarHistorial()
        {
            // Permitir filtrar por rango de fechas si hay dos DatePickers: dpFechaInicio y dpFechaFin
            DateTime? fechaInicio = null, fechaFin = null;
            if (this.FindName("dpFechaInicio") is DatePicker dpInicio && dpInicio.SelectedDate != null)
                fechaInicio = dpInicio.SelectedDate.Value.Date;
            if (this.FindName("dpFechaFin") is DatePicker dpFin && dpFin.SelectedDate != null)
                fechaFin = dpFin.SelectedDate.Value.Date;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query;
                NpgsqlCommand cmd;
                if (rolUsuario == "Administrador")
                {
                    if (fechaInicio != null && fechaFin != null)
                    {
                        query = "SELECT e.nombre, a.fecha, a.hora_entrada, a.hora_salida FROM asistencia a JOIN empleado e ON a.id_empleado = e.id_empleado WHERE a.fecha BETWEEN @inicio AND @fin ORDER BY a.fecha DESC";
                        cmd = new NpgsqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("inicio", fechaInicio.Value);
                        cmd.Parameters.AddWithValue("fin", fechaFin.Value);
                    }
                    else if (dpFecha.SelectedDate != null)
                    {
                        query = "SELECT e.nombre, a.fecha, a.hora_entrada, a.hora_salida FROM asistencia a JOIN empleado e ON a.id_empleado = e.id_empleado WHERE a.fecha=@fecha ORDER BY a.fecha DESC";
                        cmd = new NpgsqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("fecha", dpFecha.SelectedDate.Value.Date);
                    }
                    else
                    {
                        query = "SELECT e.nombre, a.fecha, a.hora_entrada, a.hora_salida FROM asistencia a JOIN empleado e ON a.id_empleado = e.id_empleado ORDER BY a.fecha DESC";
                        cmd = new NpgsqlCommand(query, conn);
                    }
                }
                else
                {
                    if (cbUsuarios.SelectedValue == null)
                        return;
                    int idEmpleado = (int)cbUsuarios.SelectedValue;
                    if (fechaInicio != null && fechaFin != null)
                    {
                        query = "SELECT fecha, hora_entrada, hora_salida FROM asistencia WHERE id_empleado=@id AND fecha BETWEEN @inicio AND @fin ORDER BY fecha DESC";
                        cmd = new NpgsqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("id", idEmpleado);
                        cmd.Parameters.AddWithValue("inicio", fechaInicio.Value);
                        cmd.Parameters.AddWithValue("fin", fechaFin.Value);
                    }
                    else if (dpFecha.SelectedDate != null)
                    {
                        query = "SELECT fecha, hora_entrada, hora_salida FROM asistencia WHERE id_empleado=@id AND fecha=@fecha ORDER BY fecha DESC";
                        cmd = new NpgsqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("id", idEmpleado);
                        cmd.Parameters.AddWithValue("fecha", dpFecha.SelectedDate.Value.Date);
                    }
                    else
                    {
                        query = "SELECT fecha, hora_entrada, hora_salida FROM asistencia WHERE id_empleado=@id ORDER BY fecha DESC";
                        cmd = new NpgsqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("id", idEmpleado);
                    }
                }
                var da = new NpgsqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                dgAsistencias.ItemsSource = dt.DefaultView;
            }
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            // Exportar el DataGrid a Excel usando ClosedXML
            var dt = ((DataView)dgAsistencias.ItemsSource)?.ToTable();
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.");
                return;
            }
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = "Asistencias.xlsx"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        workbook.Worksheets.Add(dt, "Asistencias");
                        workbook.SaveAs(saveFileDialog.FileName);
                    }
                    MessageBox.Show("Exportado correctamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar: " + ex.Message);
            }
        }
    }
}
