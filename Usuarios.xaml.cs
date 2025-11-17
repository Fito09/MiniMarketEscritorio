using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Npgsql;

namespace WpfApp1
{
    public partial class Usuarios : Window
    {
        private string connectionString = "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.izfxvtasfylqalgnkwia;Password=Mandachitos34;SSL Mode=Require;Trust Server Certificate=true";
        private int? usuarioSeleccionadoId = null;

        public Usuarios()
        {
            InitializeComponent();
            CargarUsuarios();
        }

        private void CargarUsuarios(string filtro = "")
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id_usuario, usuario, rol FROM usuario";
                if (!string.IsNullOrWhiteSpace(filtro))
                    query += " WHERE usuario ILIKE @filtro OR CAST(id_usuario AS TEXT) = @filtroExacto";
                var da = new NpgsqlDataAdapter(query, conn);
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    da.SelectCommand.Parameters.AddWithValue("filtro", "%" + filtro + "%");
                    da.SelectCommand.Parameters.AddWithValue("filtroExacto", filtro);
                }
                var dt = new DataTable();
                da.Fill(dt);
                dgUsuarios.ItemsSource = dt.DefaultView;
            }
        }

        private void BtnBuscarUsuario_Click(object sender, RoutedEventArgs e)
        {
            CargarUsuarios(txtBuscarUsuario.Text.Trim());
        }

        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            usuarioSeleccionadoId = null;
            txtUsuario.Text = "";
            txtContrasena.Password = "";
            cbRol.SelectedIndex = -1;
        }

        private void BtnGuardarUsuario_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtContrasena.Password) || cbRol.SelectedItem == null)
            {
                MessageBox.Show("Todos los campos son obligatorios.");
                return;
            }
            if (!Regex.IsMatch(txtUsuario.Text, @"^[a-zA-Z0-9_.-]+$"))
            {
                MessageBox.Show("El usuario solo puede contener letras, números y . _ -");
                return;
            }

            string rol = ((ComboBoxItem)cbRol.SelectedItem).Content.ToString();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                if (usuarioSeleccionadoId == null)
                {
                    // Crear usuario
                    var cmd = new NpgsqlCommand("INSERT INTO usuario (usuario, contrasena, rol) VALUES (@usuario, @contrasena, @rol) RETURNING id_usuario", conn);
                    cmd.Parameters.AddWithValue("usuario", txtUsuario.Text.Trim());
                    cmd.Parameters.AddWithValue("contrasena", txtContrasena.Password);
                    cmd.Parameters.AddWithValue("rol", rol);
                    try
                    {
                        int nuevoIdUsuario = Convert.ToInt32(cmd.ExecuteScalar());
                        if (rol == "Empleado" || rol == "Administrador")
                        {
                            var cmdEmp = new NpgsqlCommand("INSERT INTO empleado (nombre, id_usuario) VALUES (@nombre, @id_usuario)", conn);
                            cmdEmp.Parameters.AddWithValue("nombre", txtUsuario.Text.Trim());
                            cmdEmp.Parameters.AddWithValue("id_usuario", nuevoIdUsuario);
                            cmdEmp.ExecuteNonQuery();
                        }
                        MessageBox.Show("Usuario creado.");
                    }
                    catch (PostgresException ex) when (ex.SqlState == "23505")
                    {
                        MessageBox.Show("El usuario ya existe.");
                    }
                }
                else
                {
                    // Actualizar
                    var cmd = new NpgsqlCommand("UPDATE usuario SET usuario=@usuario, contrasena=@contrasena, rol=@rol WHERE id_usuario=@id", conn);
                    cmd.Parameters.AddWithValue("usuario", txtUsuario.Text.Trim());
                    cmd.Parameters.AddWithValue("contrasena", txtContrasena.Password);
                    cmd.Parameters.AddWithValue("rol", rol);
                    cmd.Parameters.AddWithValue("id", usuarioSeleccionadoId.Value);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Usuario actualizado.");
                }
                CargarUsuarios();
                BtnNuevoUsuario_Click(null, null);
            }
        }

        private void BtnEliminarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioSeleccionadoId == null)
            {
                MessageBox.Show("Seleccione un usuario.");
                return;
            }
            if (MessageBox.Show("¿Está seguro de eliminar este usuario?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("DELETE FROM usuario WHERE id_usuario=@id", conn);
                    cmd.Parameters.AddWithValue("id", usuarioSeleccionadoId.Value);
                    cmd.ExecuteNonQuery();
                }
                CargarUsuarios();
                BtnNuevoUsuario_Click(null, null);
            }
        }

        private void dgUsuarios_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is DataRowView row)
            {
                usuarioSeleccionadoId = Convert.ToInt32(row["id_usuario"]);
                txtUsuario.Text = row["usuario"].ToString();
                foreach (var item in cbRol.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Content.ToString() == row["rol"].ToString())
                    {
                        cbRol.SelectedItem = cbi;
                        break;
                    }
                }
                txtContrasena.Password = "";
            }
        }
    }
}
