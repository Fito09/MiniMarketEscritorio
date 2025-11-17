using System;
using System.Data;
using System.Windows;
using Npgsql;

namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para Inventario.xaml (antes Colas.xaml)
    /// </summary>
    public partial class Colas : Window
    {
        private string connectionString = "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.izfxvtasfylqalgnkwia;Password=Mandachitos34;SSL Mode=Require;Trust Server Certificate=true";
        private int? productoSeleccionadoId = null;

        public Colas()
        {
            InitializeComponent();
            CargarInventario();
        }

        private void CargarInventario()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            id_producto,
                            codigo,
                            nombre AS producto,
                            categoria,
                            precio,
                            stock,
                            fecha_vencimiento
                        FROM producto
                        ORDER BY nombre ASC
                    ";
                    var da = new NpgsqlDataAdapter(query, conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No hay productos registrados.", "Productos", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    dgInventario.ItemsSource = dt.DefaultView;
                }
            }
            catch (NpgsqlException npgEx)
            {
                MessageBox.Show("Error de conexión con la base de datos: " + npgEx.Message, "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los productos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgInventario_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgInventario.SelectedItem is System.Data.DataRowView row)
            {
                productoSeleccionadoId = Convert.ToInt32(row["id_producto"]);
                txtCodigo.Text = row["codigo"].ToString();
                txtNombre.Text = row["producto"].ToString();
                txtCategoria.Text = row["categoria"].ToString();
                txtPrecio.Text = row["precio"].ToString();
                txtStock.Text = row["stock"].ToString();
                dpFechaVencimiento.SelectedDate = row["fecha_vencimiento"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["fecha_vencimiento"]);
            }
        }

        private void BtnGuardarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (productoSeleccionadoId == null)
            {
                MessageBox.Show("Seleccione un producto para editar.");
                return;
            }
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtPrecio.Text) || string.IsNullOrWhiteSpace(txtStock.Text))
            {
                MessageBox.Show("Complete todos los campos obligatorios.");
                return;
            }
            decimal precio;
            int stock;
            if (!decimal.TryParse(txtPrecio.Text, out precio) || !int.TryParse(txtStock.Text, out stock))
            {
                MessageBox.Show("Precio y stock deben ser valores numéricos válidos.");
                return;
            }
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand("UPDATE producto SET codigo=@codigo, nombre=@nombre, categoria=@categoria, precio=@precio, stock=@stock, fecha_vencimiento=@fecha WHERE id_producto=@id", conn);
                cmd.Parameters.AddWithValue("codigo", txtCodigo.Text.Trim());
                cmd.Parameters.AddWithValue("nombre", txtNombre.Text.Trim());
                cmd.Parameters.AddWithValue("categoria", txtCategoria.Text.Trim());
                cmd.Parameters.AddWithValue("precio", precio);
                cmd.Parameters.AddWithValue("stock", stock);
                cmd.Parameters.AddWithValue("fecha", dpFechaVencimiento.SelectedDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("id", productoSeleccionadoId.Value);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Producto actualizado.");
                CargarInventario();
            }
        }
    }
}
