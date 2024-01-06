using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace CNS_V01
{
    public partial class Form1 : Form
    {
        private OleDbConnection conexion;
        private OleDbDataAdapter adapter;
        private DataTable dataTable;
        public Form1()
        {
            InitializeComponent();

            // Configurar la cadena de conexión de Access
            string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\SAID\\Documents\\CNS V01\\DataBase01.accdb;Persist Security Info=False;";
            conexion = new OleDbConnection(connectionString);

            // Configurar el adaptador y el DataTable
            adapter = new OleDbDataAdapter("SELECT * FROM Productos", conexion);
            dataTable = new DataTable();

            // Configurar el DataGridView
            dataGridView1.AutoGenerateColumns = true;

        }
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigoBarras.Text) ||
        string.IsNullOrWhiteSpace(txtNombreProducto.Text) ||
        string.IsNullOrWhiteSpace(txtPrecioCompra.Text) ||
        string.IsNullOrWhiteSpace(txtPVP.Text) ||
        string.IsNullOrWhiteSpace(txtStockMaximo.Text) ||
        string.IsNullOrWhiteSpace(txtStockMinimo.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios, excepto Descripción.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Salir del método si hay campos vacíos
            }

            // Validar que los campos numéricos tengan el formato correcto
            double precioCompra, pvp;
            int stockMaximo, stockMinimo;

            if (!double.TryParse(txtPrecioCompra.Text, out precioCompra) ||
                !double.TryParse(txtPVP.Text, out pvp) ||
                !int.TryParse(txtStockMaximo.Text, out stockMaximo) ||
                !int.TryParse(txtStockMinimo.Text, out stockMinimo))
            {
                MessageBox.Show("Los campos numéricos deben tener un formato válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Salir del método si hay formato incorrecto en campos numéricos
            }
            precioCompra = double.Parse(txtPrecioCompra.Text);
            pvp=double.Parse(txtPVP.Text);
            stockMinimo = int.Parse(txtStockMinimo.Text);
            stockMaximo = int.Parse(txtStockMaximo.Text);
            if (precioCompra <= 0 || pvp <= 0 || stockMaximo <= 0 || stockMinimo <= 0)
            {
                MessageBox.Show("Los precios y stocks no pueden ser negativos o 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Salir del método si hay valores negativos
            }
            try
            {
                // Abrir la conexión
                conexion.Open();

                if (ProductoExiste(txtCodigoBarras.Text))
                {
                    // Si existe, aumentar el stock
                    AumentarStock(txtCodigoBarras.Text, stockMaximo);
                    HabilitarTodosLosCampos();
                }
                else
                {
                    // Crear un nuevo comando de inserción
                    OleDbCommand insertCommand = new OleDbCommand("INSERT INTO Productos ([Código Barras], Nombre, Descripción, [Precio Compra], PVP, [Stock Máximo], [Stock Mínimo]) " +
                                                                  "VALUES (?, ?, ?, ?, ?, ?, ?)", conexion);

                    // Asignar los parámetros desde los TextBox
                    insertCommand.Parameters.AddWithValue("[Código Barras]", txtCodigoBarras.Text);
                    insertCommand.Parameters.AddWithValue("Nombre", txtNombreProducto.Text);
                    insertCommand.Parameters.AddWithValue("Descripción", txtDescripcion.Text);
                    insertCommand.Parameters.AddWithValue("[Precio Compra]", double.Parse(txtPrecioCompra.Text));
                    insertCommand.Parameters.AddWithValue("PVP", double.Parse(txtPVP.Text));
                    insertCommand.Parameters.AddWithValue("[Stock Máximo]", int.Parse(txtStockMaximo.Text));
                    insertCommand.Parameters.AddWithValue("[Stock Mínimo]", int.Parse(txtStockMinimo.Text));

                    // Ejecutar el comando de inserción
                    insertCommand.ExecuteNonQuery();
                }

                // Actualizar el DataTable y el DataGridView
                dataTable.Clear();
                adapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Cerrar la conexión
                LimpiarTextBox();
                conexion.Close();
            }
        }
        private void HabilitarTodosLosCampos()
        {
            // Habilitar todos los TextBox
            foreach (Control control in Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Enabled = true;
                }
            }
        }
        private void LimpiarTextBox()
        {
            // Limpiar todos los TextBox
            foreach (Control control in Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Clear();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarDatosEnDataGridView();
        }
        private void CargarDatosEnDataGridView()
        {
            try
            {
                // Abrir la conexión
                conexion.Open();

                // Crear el comando SQL para seleccionar todos los productos
                string query = "SELECT * FROM Productos";

                // Configurar el adaptador con la nueva consulta
                adapter.SelectCommand = new OleDbCommand(query, conexion);

                // Limpiar el DataTable existente
                dataTable.Clear();

                // Llenar el DataTable con los datos del adaptador
                adapter.Fill(dataTable);

                // Asignar el DataTable al DataGridView
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos en el DataGridView: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Cerrar la conexión
                conexion.Close();
            }
        }

        private void txtCodigoBarras_Leave(object sender, EventArgs e)
        {
            // Cuando el TextBox de Código de Barras pierde el foco (cuando el usuario termina de ingresar el código)
            BuscarProductoPorCodigoBarras(txtCodigoBarras.Text);
        }
        private void BuscarProductoPorCodigoBarras(string codigoBarras)
        {
            try
            {
                // Abrir la conexión
                conexion.Open();

                // Crear el comando SQL para buscar el producto por código de barras
                string query = "SELECT * FROM Productos WHERE [Código Barras] = ?";

                using (OleDbCommand command = new OleDbCommand(query, conexion))
                {
                    command.Parameters.AddWithValue("CódigoBarras", codigoBarras);

                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        // Si se encuentra el producto, cargar la información en los TextBox
                        if (reader.Read())
                        {
                            txtNombreProducto.Text = reader["Nombre"].ToString();
                            txtDescripcion.Text = reader["Descripción"].ToString();
                            txtPrecioCompra.Text = reader["Precio Compra"].ToString();
                            txtPVP.Text = reader["PVP"].ToString();
                            txtStockMaximo.Text = reader["Stock Máximo"].ToString();
                            txtStockMinimo.Text = reader["Stock Mínimo"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar producto: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Cerrar la conexión
                conexion.Close();
                // Bloquear los TextBox que no deben ser editados
                txtNombreProducto.Enabled = false;
                txtDescripcion.Enabled = false;
                txtPrecioCompra.Enabled = false;
                txtPVP.Enabled = false;
                txtStockMinimo.Enabled = false;

                // Habilitar solo el TextBox de Stock Máximo
                txtStockMaximo.Enabled = true;
            }
        }
        private bool ProductoExiste(string codigoBarras)
        {
            // Verificar si el producto ya existe por código de barras
            string query = "SELECT COUNT(*) FROM Productos WHERE [Código Barras] = ?";
            using (OleDbCommand command = new OleDbCommand(query, conexion))
            {
                command.Parameters.AddWithValue("CódigoBarras", codigoBarras);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        private void AumentarStock(string codigoBarras, int cantidad)
        {
            // Aumentar el stock del producto existente
            string updateQuery = "UPDATE Productos SET [Stock Máximo] = [Stock Máximo] + ? WHERE [Código Barras] = ?";
            using (OleDbCommand updateCommand = new OleDbCommand(updateQuery, conexion))
            {
                updateCommand.Parameters.AddWithValue("Cantidad", cantidad);
                updateCommand.Parameters.AddWithValue("CódigoBarras", codigoBarras);
                updateCommand.ExecuteNonQuery();
            }
        }
    }
}
