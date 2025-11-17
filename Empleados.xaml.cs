using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using Npgsql;

namespace WpfApp1
{
    public partial class Empleados : Window
    {
        private string connectionString = "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.izfxvtasfylqalgnkwia;Password=Mandachitos34;SSL Mode=Require;Trust Server Certificate=true";
        private int? empleadoSeleccionadoId = null;

        public Empleados()
        {
            InitializeComponent();
            CargarEmpleados();
        }

        private void CargarEmpleados(string filtro = "")
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id_empleado, nombre, direccion, telefono, cargo, fecha_ingreso FROM empleado";
                if (!string.IsNullOrWhiteSpace(filtro))
                    query += " WHERE nombre ILIKE @filtro OR CAST(id_empleado AS TEXT) = @filtroExacto";
                var da = new NpgsqlDataAdapter(query, conn);
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    da.SelectCommand.Parameters.AddWithValue("filtro", "%" + filtro + "%");
                    da.SelectCommand.Parameters.AddWithValue("filtroExacto", filtro);
                }
                var dt = new DataTable();
                da.Fill(dt);
                dgEmpleados.ItemsSource = dt.DefaultView;
            }
        }

        private void BtnBuscarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            CargarEmpleados(txtBuscarEmpleado.Text.Trim());
        }

        private void BtnNuevoEmpleado_Click(object sender, RoutedEventArgs e)
        {
            empleadoSeleccionadoId = null;
            txtNombre.Text = "";
            txtDireccion.Text = "";
            txtTelefono.Text = "";
            txtCargo.Text = "";
            dpFechaIngreso.SelectedDate = null;
        }

        private void BtnGuardarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || dpFechaIngreso.SelectedDate == null)
            {
                MessageBox.Show("Nombre y fecha de ingreso son obligatorios.");
                return;
            }
            if (!string.IsNullOrWhiteSpace(txtTelefono.Text) && !Regex.IsMatch(txtTelefono.Text, @"^[0-9+\- ]+$"))
            {
                MessageBox.Show("El teléfono solo puede contener números, espacios, + y -.");
                return;
            }

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                if (empleadoSeleccionadoId == null)
                {
                    // Crear
                    var cmd = new NpgsqlCommand("INSERT INTO empleado (nombre, direccion, telefono, cargo, fecha_ingreso) VALUES (@nombre, @direccion, @telefono, @cargo, @fecha)", conn);
                    cmd.Parameters.AddWithValue("nombre", txtNombre.Text.Trim());
                    cmd.Parameters.AddWithValue("direccion", txtDireccion.Text.Trim());
                    cmd.Parameters.AddWithValue("telefono", txtTelefono.Text.Trim());
                    cmd.Parameters.AddWithValue("cargo", txtCargo.Text.Trim());
                    cmd.Parameters.AddWithValue("fecha", dpFechaIngreso.SelectedDate.Value);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Empleado creado.");
                }
                else
                {
                    // Actualizar
                    var cmd = new NpgsqlCommand("UPDATE empleado SET nombre=@nombre, direccion=@direccion, telefono=@telefono, cargo=@cargo, fecha_ingreso=@fecha WHERE id_empleado=@id", conn);
                    cmd.Parameters.AddWithValue("nombre", txtNombre.Text.Trim());
                    cmd.Parameters.AddWithValue("direccion", txtDireccion.Text.Trim());
                    cmd.Parameters.AddWithValue("telefono", txtTelefono.Text.Trim());
                    cmd.Parameters.AddWithValue("cargo", txtCargo.Text.Trim());
                    cmd.Parameters.AddWithValue("fecha", dpFechaIngreso.SelectedDate.Value);
                    cmd.Parameters.AddWithValue("id", empleadoSeleccionadoId.Value);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Empleado actualizado.");
                }
                CargarEmpleados();
                BtnNuevoEmpleado_Click(null, null);
            }
        }

        private void BtnEliminarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (empleadoSeleccionadoId == null)
            {
                MessageBox.Show("Seleccione un empleado.");
                return;
            }
            if (MessageBox.Show("¿Está seguro de eliminar este empleado?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("DELETE FROM empleado WHERE id_empleado=@id", conn);
                    cmd.Parameters.AddWithValue("id", empleadoSeleccionadoId.Value);
                    cmd.ExecuteNonQuery();
                }
                CargarEmpleados();
                BtnNuevoEmpleado_Click(null, null);
            }
        }

        private void dgEmpleados_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgEmpleados.SelectedItem is DataRowView row)
            {
                empleadoSeleccionadoId = Convert.ToInt32(row["id_empleado"]);
                txtNombre.Text = row["nombre"].ToString();
                txtDireccion.Text = row["direccion"].ToString();
                txtTelefono.Text = row["telefono"].ToString();
                txtCargo.Text = row["cargo"].ToString();
                dpFechaIngreso.SelectedDate = row["fecha_ingreso"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["fecha_ingreso"]);
            }
        }
    }
}
