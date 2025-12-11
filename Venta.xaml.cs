using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Npgsql;

namespace WpfApp1
{
    public class ProductoCarrito
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Total => Cantidad * Precio;
    }

    public partial class Venta : Window
    {
        private string connectionString = "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.izfxvtasfylqalgnkwia;Password=Mandachitos34;SSL Mode=Require;Trust Server Certificate=true";
        private List<ProductoCarrito> carrito = new List<ProductoCarrito>();
        private int idEmpleadoActual;
        private int idClienteActual = 1;
        private string nombreEmpleado;
        private decimal totalActual = 0;
        private bool procesandoQR = false;
        private bool procesandoEfectivo = false;
        private bool procesandoTarjeta = false;
        private bool inicializando = true;
        private bool clienteAnonimo = false;

        public Venta(int idEmpleado = 1, string nombreEmp = "Empleado")
        {
            InitializeComponent();
            idEmpleadoActual = idEmpleado;
            nombreEmpleado = nombreEmp;
            CargarClientes();
            CargarProductos();
            ActualizarCarrito();
            AplicarTemaVenta("Claro");
            inicializando = false;
        }

        public void AplicarTemaVenta(string tema)
        {
            System.Windows.Media.SolidColorBrush backgroundBrush;
            System.Windows.Media.SolidColorBrush foregroundBrush;
            System.Windows.Media.SolidColorBrush headerBackgroundBrush;
            System.Windows.Media.SolidColorBrush borderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
            System.Windows.Media.SolidColorBrush contentBackgroundBrush;
            System.Windows.Media.SolidColorBrush textBoxBackgroundBrush;
            System.Windows.Media.SolidColorBrush textColorBrush;
            System.Windows.Style buttonStyle = null;
            
            switch (tema)
            {
                case "Oscuro":
                    backgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
                    headerBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 45));
                    contentBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40));
                    textBoxBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220));
                    foregroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                    textColorBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40));
                    borderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80));
                    buttonStyle = (System.Windows.Style)Application.Current.Resources["DefaultButtonStyle"];
                    break;
                case "Univalle":
                    backgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80));
                    headerBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100));
                    contentBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 90, 90));
                    textBoxBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                    foregroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    textColorBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40));
                    borderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(120, 120, 120));
                    buttonStyle = (System.Windows.Style)Application.Current.Resources["UnivalleButtonStyle"];
                    break;
                default:
                    backgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    headerBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
                    contentBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    textBoxBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    foregroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                    textColorBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                    borderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220));
                    buttonStyle = (System.Windows.Style)Application.Current.Resources["DefaultButtonStyle"];
                    break;
            }

            this.Background = backgroundBrush;
            this.Foreground = foregroundBrush;

            if (borderHeader1 != null) { borderHeader1.Background = headerBackgroundBrush; borderHeader1.BorderBrush = borderBrush; }
            if (borderHeader2 != null) { borderHeader2.Background = headerBackgroundBrush; borderHeader2.BorderBrush = borderBrush; }
            if (borderProductos != null) { borderProductos.Background = contentBackgroundBrush; borderProductos.BorderBrush = borderBrush; }
            if (borderCarrito != null) { borderCarrito.Background = contentBackgroundBrush; borderCarrito.BorderBrush = borderBrush; }
            if (borderAgregarCarrito != null) { borderAgregarCarrito.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, (byte)headerBackgroundBrush.Color.R, (byte)headerBackgroundBrush.Color.G, (byte)headerBackgroundBrush.Color.B)); borderAgregarCarrito.BorderBrush = borderBrush; }
            if (borderTotales != null) { borderTotales.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, (byte)headerBackgroundBrush.Color.R, (byte)headerBackgroundBrush.Color.G, (byte)headerBackgroundBrush.Color.B)); borderTotales.BorderBrush = borderBrush; }

            foreach (var btn in FindVisualChildren<System.Windows.Controls.Button>(this))
            {
                btn.Style = buttonStyle;
                btn.Foreground = foregroundBrush;
            }
            foreach (var tb in FindVisualChildren<System.Windows.Controls.TextBox>(this))
            {
                tb.Foreground = textColorBrush;
                tb.Background = textBoxBackgroundBrush;
                tb.BorderBrush = borderBrush;
                tb.CaretBrush = textColorBrush;
            }
            foreach (var cb in FindVisualChildren<System.Windows.Controls.ComboBox>(this))
            {
                cb.Foreground = textColorBrush;
                cb.Background = textBoxBackgroundBrush;
                cb.BorderBrush = borderBrush;
            }
            foreach (var dg in FindVisualChildren<System.Windows.Controls.DataGrid>(this))
            {
                dg.Foreground = textColorBrush;
                dg.Background = contentBackgroundBrush;
                dg.BorderBrush = borderBrush;
            }
            foreach (var tbk in FindVisualChildren<System.Windows.Controls.TextBlock>(this))
            {
                tbk.Foreground = foregroundBrush;
            }
            foreach (var label in FindVisualChildren<System.Windows.Controls.Label>(this))
            {
                label.Foreground = foregroundBrush;
            }
            foreach (var radio in FindVisualChildren<System.Windows.Controls.RadioButton>(this))
            {
                radio.Foreground = foregroundBrush;
            }
            foreach (var chk in FindVisualChildren<System.Windows.Controls.CheckBox>(this))
            {
                chk.Foreground = foregroundBrush;
            }
        }

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void CargarClientes()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("SELECT id_cliente, nombre FROM cliente ORDER BY nombre ASC", conn);
                    var reader = cmd.ExecuteReader();
                    var clientes = new List<Tuple<int, string>>();
                    while (reader.Read())
                    {
                        clientes.Add(Tuple.Create(reader.GetInt32(0), reader.GetString(1)));
                    }
                    if (clientes.Count > 0)
                    {
                        cbClienteVenta.ItemsSource = clientes;
                        cbClienteVenta.DisplayMemberPath = "Item2";
                        cbClienteVenta.SelectedValuePath = "Item1";
                        cbClienteVenta.SelectedIndex = 0;
                        idClienteActual = clientes[0].Item1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarProductos(string filtro = "")
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id_producto, codigo, nombre, precio, stock FROM producto WHERE stock > 0";
                    if (!string.IsNullOrWhiteSpace(filtro))
                        query += " AND (nombre ILIKE @filtro OR codigo ILIKE @filtro)";
                    query += " ORDER BY nombre ASC";

                    var da = new NpgsqlDataAdapter(query, conn);
                    if (!string.IsNullOrWhiteSpace(filtro))
                    {
                        da.SelectCommand.Parameters.AddWithValue("filtro", "%" + filtro + "%");
                    }
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgProductosVenta.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBuscarProductoVenta_Click(object sender, RoutedEventArgs e)
        {
            CargarProductos(txtBuscarProductoVenta.Text.Trim());
        }

        private void DgProductosVenta_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void CbClienteVenta_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbClienteVenta.SelectedValue != null)
            {
                idClienteActual = (int)cbClienteVenta.SelectedValue;
                clienteAnonimo = false;
            }
        }

        private void ChkClienteAnonimo_Checked(object sender, RoutedEventArgs e)
        {
            cbClienteVenta.IsEnabled = false;
            cbClienteVenta.SelectedIndex = -1;
            clienteAnonimo = true;
            idClienteActual = 1;
        }

        private void ChkClienteAnonimo_Unchecked(object sender, RoutedEventArgs e)
        {
            cbClienteVenta.IsEnabled = true;
            clienteAnonimo = false;
            if (cbClienteVenta.Items.Count > 0)
            {
                cbClienteVenta.SelectedIndex = 0;
            }
        }

        private void BtnAgregarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductosVenta.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un producto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantidadProducto.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad valida.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rowView = dgProductosVenta.SelectedItem as DataRowView;
            if (rowView == null) return;

            int idProducto = Convert.ToInt32(rowView["id_producto"]);
            string nombre = rowView["nombre"].ToString();
            decimal precio = Convert.ToDecimal(rowView["precio"]);
            int stock = Convert.ToInt32(rowView["stock"]);

            if (cantidad > stock)
            {
                MessageBox.Show("Cantidad no disponible en stock. Stock disponible: " + stock, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var productoEnCarrito = carrito.FirstOrDefault(p => p.IdProducto == idProducto);
            if (productoEnCarrito != null)
            {
                if (productoEnCarrito.Cantidad + cantidad > stock)
                {
                    MessageBox.Show("No hay suficiente stock para esta cantidad. Stock disponible: " + stock, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                productoEnCarrito.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(new ProductoCarrito
                {
                    IdProducto = idProducto,
                    Nombre = nombre,
                    Cantidad = cantidad,
                    Precio = precio
                });
            }

            txtCantidadProducto.Text = "1";
            ActualizarCarrito();
        }

        private void BtnEliminarProductoCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (dgCarrito.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var productoSeleccionado = dgCarrito.SelectedItem as ProductoCarrito;
            if (productoSeleccionado != null)
            {
                carrito.Remove(productoSeleccionado);
                ActualizarCarrito();
            }
        }

        private void ActualizarCarrito()
        {
            dgCarrito.ItemsSource = null;
            dgCarrito.ItemsSource = carrito;

            decimal subtotal = carrito.Sum(p => p.Total);
            decimal descuento = 0;
            totalActual = subtotal - descuento;

            txtSubtotal.Text = subtotal.ToString("$0.00");
            txtDescuento.Text = descuento.ToString("$0.00");
            txtTotal.Text = totalActual.ToString("$0.00");
        }

        private void RbMetodoPago_Checked(object sender, RoutedEventArgs e)
        {
            if (inicializando) return;
            
            try
            {
                if (rbQR.IsChecked == true && !procesandoQR)
                {
                    if (carrito.Count > 0)
                    {
                        // No mostrar QR aquí, solo cambiar la selección
                    }
                    else
                    {
                        MessageBox.Show("Agregue productos al carrito antes de seleccionar QR.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                        rbEfectivo.IsChecked = true;
                    }
                }
                else if (rbEfectivo.IsChecked != true && grdQRModal != null)
                {
                    grdQRModal.Visibility = Visibility.Collapsed;
                    grdEfectivoModal.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar metodo de pago: " + ex.Message);
            }
        }

        private void MostrarModalEfectivo()
        {
            try
            {
                decimal total = carrito.Sum(p => p.Total);
                txtTotalEfectivo.Text = "Total a Pagar: " + total.ToString("$0.00");
                txtCantidadRecibida.Text = "";
                txtCambio.Text = "Cambio: $0.00";
                grdEfectivoModal.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtCantidadRecibida_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                decimal total = carrito.Sum(p => p.Total);
                
                if (decimal.TryParse(txtCantidadRecibida.Text, out decimal recibido))
                {
                    decimal cambio = recibido - total;
                    if (cambio >= 0)
                    {
                        txtCambio.Text = "Cambio: " + cambio.ToString("$0.00");
                        txtCambio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69));
                    }
                    else
                    {
                        txtCambio.Text = "Falta: " + Math.Abs(cambio).ToString("$0.00");
                        txtCambio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69));
                    }
                }
                else
                {
                    txtCambio.Text = "Cambio: $0.00";
                    txtCambio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error calculando cambio: " + ex.Message);
            }
        }

        private void BtnConfirmarPagoEfectivo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal total = carrito.Sum(p => p.Total);
                
                if (!decimal.TryParse(txtCantidadRecibida.Text, out decimal recibido))
                {
                    MessageBox.Show("Ingrese una cantidad valida.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (recibido < total)
                {
                    MessageBox.Show("La cantidad recibida es menor al total. Falta: " + (total - recibido).ToString("$0.00"), "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                grdEfectivoModal.Visibility = Visibility.Collapsed;
                procesandoEfectivo = true;
                
                string metodoPago = "Efectivo";
                if (GuardarVenta(metodoPago, total))
                {
                    MessageBox.Show("Venta procesada exitosamente!", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    grdQRModal.Visibility = Visibility.Collapsed;
                    grdEfectivoModal.Visibility = Visibility.Collapsed;
                    LimpiarVenta();
                }
                
                procesandoEfectivo = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                procesandoEfectivo = false;
            }
        }

        private void BtnCancelarEfectivo_Click(object sender, RoutedEventArgs e)
        {
            grdEfectivoModal.Visibility = Visibility.Collapsed;
        }

        private void MostrarModalTarjeta(string tipoTarjeta)
        {
            try
            {
                decimal total = carrito.Sum(p => p.Total);
                
                if (tipoTarjeta == "Debito")
                {
                    txtTitulTarjeta.Text = "Pago con Tarjeta de Debito";
                }
                else
                {
                    txtTitulTarjeta.Text = "Pago con Tarjeta de Credito";
                }
                
                txtTotalTarjeta.Text = "Total a Pagar: " + total.ToString("$0.00");
                LimpiarCamposTarjeta();
                grdTarjetaModal.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarCamposTarjeta()
        {
            txtNumeroTarjeta.Text = "";
            txtVencimiento.Text = "";
            txtCVV.Text = "";
            txtNombreTitular.Text = "";
        }

        private void BtnConfirmarPagoTarjeta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNumeroTarjeta.Text))
                {
                    MessageBox.Show("Ingrese el numero de tarjeta.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNumeroTarjeta.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtVencimiento.Text))
                {
                    MessageBox.Show("Ingrese la fecha de vencimiento (MM/YY).", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtVencimiento.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCVV.Text))
                {
                    MessageBox.Show("Ingrese el CVV.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCVV.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNombreTitular.Text))
                {
                    MessageBox.Show("Ingrese el nombre del titular.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNombreTitular.Focus();
                    return;
                }

                if (!ValidarNumeroTarjeta(txtNumeroTarjeta.Text))
                {
                    MessageBox.Show("Numero de tarjeta invalido.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNumeroTarjeta.Focus();
                    return;
                }

                if (!ValidarVencimiento(txtVencimiento.Text))
                {
                    MessageBox.Show("Fecha de vencimiento invalida (formato: MM/YY).", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtVencimiento.Focus();
                    return;
                }

                if (!ValidarCVV(txtCVV.Text))
                {
                    MessageBox.Show("CVV invalido (3 o 4 digitos).", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCVV.Focus();
                    return;
                }

                grdTarjetaModal.Visibility = Visibility.Collapsed;
                procesandoTarjeta = true;
                
                decimal total = carrito.Sum(p => p.Total);
                string metodoPago = rbDebito.IsChecked == true ? "Tarjeta de Debito" : "Tarjeta de Credito";
                
                if (GuardarVenta(metodoPago, total))
                {
                    MessageBox.Show("Venta procesada exitosamente!", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    grdTarjetaModal.Visibility = Visibility.Collapsed;
                    LimpiarVenta();
                }
                
                procesandoTarjeta = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                procesandoTarjeta = false;
            }
        }

        private void BtnCancelarTarjeta_Click(object sender, RoutedEventArgs e)
        {
            grdTarjetaModal.Visibility = Visibility.Collapsed;
            LimpiarCamposTarjeta();
        }

        private bool ValidarNumeroTarjeta(string numero)
        {
            string soloNumeros = new string(numero.Where(char.IsDigit).ToArray());
            return soloNumeros.Length >= 13 && soloNumeros.Length <= 19;
        }

        private bool ValidarVencimiento(string vencimiento)
        {
            if (!vencimiento.Contains("/")) return false;
            var partes = vencimiento.Split('/');
            if (partes.Length != 2) return false;
            if (!int.TryParse(partes[0], out int mes) || !int.TryParse(partes[1], out int anio)) return false;
            return mes >= 1 && mes <= 12 && anio >= 0 && anio <= 99;
        }

        private bool ValidarCVV(string cvv)
        {
            return cvv.Length >= 3 && cvv.Length <= 4 && cvv.All(char.IsDigit);
        }

        private void MostrarQR()
        {
            try
            {
                decimal total = carrito.Sum(p => p.Total);
                if (total <= 0)
                {
                    MessageBox.Show("El total debe ser mayor a cero.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    rbEfectivo.IsChecked = true;
                    return;
                }

                txtMonto.Text = "Monto a Pagar: " + total.ToString("$0.00");
                
                var bitmap = GenerarQRSimple(total);
                if (bitmap != null)
                {
                    imgQR.Source = bitmap;
                    grdQRModal.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("No se pudo generar el codigo QR.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    rbEfectivo.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar QR: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                grdQRModal.Visibility = Visibility.Collapsed;
                rbEfectivo.IsChecked = true;
            }
        }

        private BitmapImage GenerarQRSimple(decimal total)
        {
            try
            {
                string qrUrl = "https://api.qrserver.com/v1/create-qr-code/?size=350x350&data=MINIMARKET|" + 
                              total.ToString("F2") + "|" + 
                              DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(qrUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error generando QR: " + ex.Message);
                return null;
            }
        }

        private void BtnConfirmarPagoQR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                procesandoQR = true;
                grdQRModal.Visibility = Visibility.Collapsed;
                
                decimal total = carrito.Sum(p => p.Total);
                string metodoPago = "QR";
                
                if (GuardarVenta(metodoPago, total))
                {
                    MessageBox.Show("Venta procesada exitosamente!", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    grdQRModal.Visibility = Visibility.Collapsed;
                    grdEfectivoModal.Visibility = Visibility.Collapsed;
                    LimpiarVenta();
                }
                
                procesandoQR = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                procesandoQR = false;
            }
        }

        private void BtnCancelarQR_Click(object sender, RoutedEventArgs e)
        {
            grdQRModal.Visibility = Visibility.Collapsed;
            rbEfectivo.IsChecked = true;
        }

        private bool GuardarVenta(string metodoPago, decimal total)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    int idClienteAGuardar = clienteAnonimo ? 1 : idClienteActual;

                    var cmdVenta = new NpgsqlCommand(
                        "INSERT INTO venta (id_cliente, id_empleado, fecha, total, estado) VALUES (@idCliente, @idEmpleado, @fecha, @total, @estado) RETURNING id_venta",
                        conn);
                    cmdVenta.Parameters.AddWithValue("idCliente", idClienteAGuardar);
                    cmdVenta.Parameters.AddWithValue("idEmpleado", idEmpleadoActual);
                    cmdVenta.Parameters.AddWithValue("fecha", DateTime.Now);
                    cmdVenta.Parameters.AddWithValue("total", total);
                    cmdVenta.Parameters.AddWithValue("estado", "completada");

                    int idVenta = Convert.ToInt32(cmdVenta.ExecuteScalar());

                    foreach (var producto in carrito)
                    {
                        var cmdDetalle = new NpgsqlCommand(
                            "INSERT INTO detalle_venta (id_venta, id_producto, cantidad, precio_unitario) VALUES (@idVenta, @idProducto, @cantidad, @precio)",
                            conn);
                        cmdDetalle.Parameters.AddWithValue("idVenta", idVenta);
                        cmdDetalle.Parameters.AddWithValue("idProducto", producto.IdProducto);
                        cmdDetalle.Parameters.AddWithValue("cantidad", producto.Cantidad);
                        cmdDetalle.Parameters.AddWithValue("precio", producto.Precio);
                        cmdDetalle.ExecuteNonQuery();

                        var cmdStock = new NpgsqlCommand(
                            "UPDATE producto SET stock = stock - @cantidad WHERE id_producto = @idProducto",
                            conn);
                        cmdStock.Parameters.AddWithValue("cantidad", producto.Cantidad);
                        cmdStock.Parameters.AddWithValue("idProducto", producto.IdProducto);
                        cmdStock.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al procesar venta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void BtnCancelarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count > 0)
            {
                if (MessageBox.Show("Descartar venta actual?", "Cancelar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    grdQRModal.Visibility = Visibility.Collapsed;
                    grdEfectivoModal.Visibility = Visibility.Collapsed;
                    grdTarjetaModal.Visibility = Visibility.Collapsed;
                    LimpiarVenta();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void BtnProcesarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0)
            {
                MessageBox.Show("El carrito esta vacio.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!clienteAnonimo && cbClienteVenta.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un cliente o marque 'Sin Cliente'.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string metodoPago = "Efectivo";
            if (rbQR.IsChecked == true)
                metodoPago = "QR";
            else if (rbDebito.IsChecked == true)
                metodoPago = "Tarjeta de Debito";
            else if (rbCredito.IsChecked == true)
                metodoPago = "Tarjeta de Credito";

            decimal total = carrito.Sum(p => p.Total);

            // Si ya estamos procesando, no abrir modal
            if (procesandoEfectivo || procesandoQR || procesandoTarjeta)
            {
                return;
            }

            // Mostrar modal segun metodo de pago
            if (rbEfectivo.IsChecked == true)
            {
                MostrarModalEfectivo();
                return;
            }
            else if (rbQR.IsChecked == true)
            {
                MostrarQR();
                return;
            }
            else if (rbDebito.IsChecked == true)
            {
                MostrarModalTarjeta("Debito");
                return;
            }
            else if (rbCredito.IsChecked == true)
            {
                MostrarModalTarjeta("Credito");
                return;
            }
        }

        private void LimpiarVenta()
        {
            carrito.Clear();
            txtCantidadProducto.Text = "1";
            txtBuscarProductoVenta.Text = "";
            rbEfectivo.IsChecked = true;
            grdQRModal.Visibility = Visibility.Collapsed;
            grdEfectivoModal.Visibility = Visibility.Collapsed;
            grdTarjetaModal.Visibility = Visibility.Collapsed;
            chkClienteAnonimo.IsChecked = false;
            cbClienteVenta.IsEnabled = true;
            LimpiarCamposTarjeta();
            if (cbClienteVenta.Items.Count > 0)
            {
                cbClienteVenta.SelectedIndex = 0;
            }
            ActualizarCarrito();
            CargarProductos();
        }

        private void BtnCerrarVenta_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
