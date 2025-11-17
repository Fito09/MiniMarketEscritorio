using System;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private string rolUsuario;

        public MainWindow(string rol)
        {
            InitializeComponent();
            rolUsuario = rol;
            AplicarPermisos();
            AplicarTema("Claro");
        }

        public MainWindow() : this("Administrador")
        {
            var login = new Login();
            login.Show();
            this.Close();
        }

        private void AplicarPermisos()
        {
            var rol = rolUsuario?.Trim().ToLower();
            btnUsuarios.Visibility = (rol == "administrador" || rol == "admin") ? Visibility.Visible : Visibility.Collapsed;
            btnAsistencia.Visibility = (rol == "administrador" || rol == "admin" || rol == "empleado") ? Visibility.Visible : Visibility.Collapsed;
            btnColas.Visibility = (rol == "administrador" || rol == "admin" || rol == "empleado" || rol == "cliente") ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnAsistencia_Click(object sender, RoutedEventArgs e)
        {
            new Asistencia(rolUsuario).ShowDialog();
        }

        private void btnColas_Click(object sender, RoutedEventArgs e)
        {
            new Colas().ShowDialog();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            new Usuarios().ShowDialog();
        }

        private void cbTema_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var tema = (cbTema.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
            AplicarTema(tema);
        }

        private void AplicarTema(string tema)
        {
            SolidColorBrush backgroundBrush;
            SolidColorBrush foregroundBrush;
            Style buttonStyle = null;
            switch (tema)
            {
                case "Oscuro":
                    backgroundBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                    foregroundBrush = new SolidColorBrush(Colors.White);
                    buttonStyle = (Style)Application.Current.Resources["DefaultButtonStyle"];
                    break;
                case "Univalle":
                    backgroundBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                    foregroundBrush = new SolidColorBrush(Colors.White);
                    buttonStyle = (Style)Application.Current.Resources["UnivalleButtonStyle"];
                    break;
                default:
                    backgroundBrush = new SolidColorBrush(Colors.White);
                    foregroundBrush = new SolidColorBrush(Colors.Black);
                    buttonStyle = (Style)Application.Current.Resources["DefaultButtonStyle"];
                    break;
            }
            Application.Current.Resources["MainBackground"] = backgroundBrush;
            Application.Current.Resources["MainForeground"] = foregroundBrush;
            foreach (Window win in Application.Current.Windows)
            {
                win.Background = backgroundBrush;
                win.Foreground = foregroundBrush;
                foreach (var btn in FindVisualChildren<System.Windows.Controls.Button>(win))
                {
                    btn.Style = buttonStyle;
                    btn.Foreground = foregroundBrush;
                }
                foreach (var label in FindVisualChildren<System.Windows.Controls.Label>(win))
                {
                    label.Foreground = foregroundBrush;
                }
                foreach (var tb in FindVisualChildren<System.Windows.Controls.TextBox>(win))
                {
                    tb.Foreground = foregroundBrush;
                    tb.Background = backgroundBrush;
                }
                foreach (var cb in FindVisualChildren<System.Windows.Controls.ComboBox>(win))
                {
                    cb.Foreground = foregroundBrush;
                    cb.Background = backgroundBrush;
                }
                foreach (var dp in FindVisualChildren<System.Windows.Controls.DatePicker>(win))
                {
                    dp.Foreground = foregroundBrush;
                    dp.Background = backgroundBrush;
                }
                foreach (var dg in FindVisualChildren<System.Windows.Controls.DataGrid>(win))
                {
                    dg.Foreground = foregroundBrush;
                    dg.Background = backgroundBrush;
                    dg.ColumnHeaderStyle = new System.Windows.Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader))
                    {
                        Setters = {
                            new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, foregroundBrush),
                            new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, backgroundBrush)
                        }
                    };
                }
                foreach (var tbk in FindVisualChildren<System.Windows.Controls.TextBlock>(win))
                {
                    tbk.Foreground = foregroundBrush;
                }
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

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
        }
    }
}
