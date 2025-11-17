using System;
using System.Data;
using System.Windows;
using Npgsql;
using System.Collections.Generic;

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

        private void CargarHistorial()
        {
            if (rolUsuario == "Administrador")
            {
                if (dpFecha.SelectedDate == null)
                    return;
                DateTime fecha = dpFecha.SelectedDate.Value.Date;
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("SELECT e.nombre, a.fecha, a.hora_entrada, a.hora_salida FROM asistencia a JOIN empleado e ON a.id_empleado = e.id_empleado WHERE a.fecha=@fecha", conn);
                    cmd.Parameters.AddWithValue("fecha", fecha);
                    var da = new NpgsqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgAsistencias.ItemsSource = dt.DefaultView;
                }
            }
            else
            {
                if (cbUsuarios.SelectedValue == null || dpFecha.SelectedDate == null)
                    return;
                int idEmpleado = (int)cbUsuarios.SelectedValue;
                DateTime fecha = dpFecha.SelectedDate.Value.Date;
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("SELECT fecha, hora_entrada, hora_salida FROM asistencia WHERE id_empleado=@id AND fecha=@fecha", conn);
                    cmd.Parameters.AddWithValue("id", idEmpleado);
                    cmd.Parameters.AddWithValue("fecha", fecha);
                    var da = new NpgsqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgAsistencias.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            // Aquí puedes usar ClosedXML o EPPlus para exportar el DataTable a Excel.
            MessageBox.Show("Función de exportar a Excel pendiente de implementar.");
        }
    }
}
