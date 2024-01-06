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

namespace CNS_V01
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        private string conexion= @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\SAID\Documents\CNS V01\DataBase01.accdb";
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Establecer la conexión
                using (SqlConnection connection = new SqlConnection(conexion))
                {
                    connection.Open();

                    // Crear el comando SQL para insertar en la tabla de productos
                    string query = "INSERT INTO Productos (CódigoBarras, Nombre, Descripción, PrecioCompra, PVP, StockMáximo, StockMínimo) " +
                                   "VALUES (@CodigoBarras, @Nombre, @Descripcion, @PrecioCompra, @PVP, @StockMaximo, @StockMinimo)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Asignar los parámetros desde los TextBox
                        command.Parameters.AddWithValue("@CodigoBarras", txtCodigoBarras.Text);
                        command.Parameters.AddWithValue("@Nombre", txtNombreProducto.Text);
                        command.Parameters.AddWithValue("@Descripcion", txtDescripcion.Text);
                        command.Parameters.AddWithValue("@PrecioCompra", decimal.Parse(txtPrecioCompra.Text));
                        command.Parameters.AddWithValue("@PVP", decimal.Parse(txtPVP.Text));
                        command.Parameters.AddWithValue("@StockMaximo", int.Parse(txtStockMaximo.Text));
                        command.Parameters.AddWithValue("@StockMinimo", int.Parse(txtStockMinimo.Text));

                        // Ejecutar el comando
                        command.ExecuteNonQuery();
                    }
                }

                // Limpiar los TextBox después de la inserción
                LimpiarTextBox();

                // Actualizar el DataGridView con los nuevos datos
                CargarDatosEnDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CargarDatosEnDataGridView()
        {
            try
            {
                // Establecer la conexión
                using (SqlConnection connection = new SqlConnection(conexion))
                {
                    connection.Open();

                    // Crear el comando SQL para seleccionar todos los productos
                    string query = "SELECT * FROM Productos";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        // Crear un DataTable para almacenar los resultados
                        DataTable dataTable = new DataTable();

                        // Llenar el DataTable con los datos del adaptador
                        adapter.Fill(dataTable);

                        // Asignar el DataTable al DataGridView
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos en el DataGridView: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // Cargar datos en el DataGridView al cargar el formulario
            CargarDatosEnDataGridView();
        }
    }
}
