using System;
using System.Windows;
using Npgsql;

namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private int intentosFallidos = 0;
        private const int MAX_INTENTOS = 3;

        public Login()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text;
            string password = txtPassword.Password;

            // Cadena de conexión Supabase
            string connectionString = "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.izfxvtasfylqalgnkwia;Password=Mandachitos34;SSL Mode=Require;Trust Server Certificate=true";

            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT rol FROM usuario WHERE usuario = @usuario AND contrasena = @contrasena";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("usuario", usuario);
                        cmd.Parameters.AddWithValue("contrasena", password);

                        var rol = cmd.ExecuteScalar();

                        if (rol != null)
                        {
                            // Abrir siempre MainWindow y pasar el rol
                            MainWindow main = new MainWindow(rol.ToString());
                            main.Show();
                            this.Close();
                        }
                        else
                        {
                            intentosFallidos++;
                            MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                            if (intentosFallidos >= MAX_INTENTOS)
                            {
                                MessageBox.Show("Demasiados intentos fallidos. El sistema se cerrará.", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                                Application.Current.Shutdown();
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Error de conexión: " + ex.Message);
                }
            }
        }
    }
}
