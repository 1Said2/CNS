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
            string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\SAID\\Documents\\Prueba\\DataBase01.accdb;Persist Security Info=False;";
            conexion = new OleDbConnection(connectionString);

            // Configurar el adaptador y el DataTable
            adapter = new OleDbDataAdapter("SELECT * FROM Productos", conexion);
            dataTable = new DataTable();

            // Configurar el DataGridView
            dataGridView1.AutoGenerateColumns = true;

        }
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Abrir la conexión
                conexion.Open();

                // Crear un nuevo comando de inserción
                OleDbCommand insertCommand = new OleDbCommand("INSERT INTO Productos ([Código Barras], Nombre, Descripción, [Precio Compra], PVP, [Stock Máximo], [Stock Mínimo]) " +
                                                              "VALUES (?, ?, ?, ?, ?, ?, ?)", conexion);

                // Asignar los parámetros desde los TextBox
                insertCommand.Parameters.AddWithValue("[Código Barras]", txtCodigoBarras.Text);
                insertCommand.Parameters.AddWithValue("Nombre", txtNombreProducto.Text);
                insertCommand.Parameters.AddWithValue("Descripción", txtDescripcion.Text);
                insertCommand.Parameters.AddWithValue("[Precio Compra]", decimal.Parse(txtPrecioCompra.Text));
                insertCommand.Parameters.AddWithValue("PVP", decimal.Parse(txtPVP.Text));
                insertCommand.Parameters.AddWithValue("[Stock Máximo]", int.Parse(txtStockMaximo.Text));
                insertCommand.Parameters.AddWithValue("[Stock Mínimo]", int.Parse(txtStockMinimo.Text));

                // Ejecutar el comando de inserción
                insertCommand.ExecuteNonQuery();

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
                conexion.Close();
            }
            LimpiarTextBox();
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
    }
}
