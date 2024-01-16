using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace CNS_V01
{
    public partial class Form1 : Form
    {
        private PrintDocument printDocument;
        private OleDbConnection conexion;
        private OleDbDataAdapter adapter;
        private DataTable dataTable;
        private string connectionString;
        public Form1()
        {
            InitializeComponent();

            // Configurar la cadena de conexión de Access
            connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\bdd\\DataBase01.accdb;Persist Security Info=False;";
            conexion = new OleDbConnection(connectionString);

            // Configurar el adaptador y el DataTable
            adapter = new OleDbDataAdapter("SELECT * FROM Productos", conexion);
            dataTable = new DataTable();

            // Configurar el DataGridView
            dataGridView1.AutoGenerateColumns = true;

        }
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Obtener el contenido del RichTextBox para imprimirlo
            string contenido = rtbFactura.Text;

            // Configurar la fuente y los márgenes
            Font fuente = new Font("Arial", 10);
            Margins margenes = new Margins(50, 50, 50, 50);

            // Configurar el área de impresión
            RectangleF areaImpresion = new RectangleF(
                e.MarginBounds.Left,
                e.MarginBounds.Top,
                e.MarginBounds.Width,
                e.MarginBounds.Height
            );

            // Imprimir el contenido en el área definida
            e.Graphics.DrawString(contenido, fuente, Brushes.Black, areaImpresion, new StringFormat());

            // Verificar si hay más páginas para imprimir
            e.HasMorePages = false;
        }
        private bool EsCodigoBarrasEAN13(string codigo)
        {
            // Utilizar una expresión regular para verificar que sea un número de 13 dígitos
            if (System.Text.RegularExpressions.Regex.IsMatch(codigo, @"^\d{13}$"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string codigo = txtCodigoBarras.Text;
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
            if (stockMinimo >= stockMaximo)
            {
                MessageBox.Show("El stock mínimo debe ser inferior al stock máximo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Salir del método si la validación no se cumple
            }
            if (precioCompra <= 0 || pvp <= 0 || stockMaximo <= 0 || stockMinimo <= 0)
            {
                MessageBox.Show("Los precios y stocks no pueden ser negativos o 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Salir del método si hay valores negativos
            }
            if (!EsCodigoBarrasEAN13(codigo))
            {
                MessageBox.Show("El código de barras ingresado no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Salir del método si el código de barras no es válido
            }
            try
            {

                if (ProductoExiste(txtCodigoBarras.Text))
                {
                    // Abrir la conexión
                    conexion.Open();
                    AumentarStock(txtCodigoBarras.Text, stockMaximo);
                    HabilitarTodosLosCampos();
                }
                else
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
            txtNombreProducto.Clear();
            txtDescripcion.Clear();
            txtPrecioCompra.Clear();
            txtPVP.Clear();
            txtStockMinimo.Clear();
            txtStockMaximo.Clear();
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
            if (ProductoExiste(txtCodigoBarras.Text))
            {
                MessageBox.Show("¿Desea aumentar el stock?");
                // Bloquear los TextBox que no deben ser editados
                txtNombreProducto.Enabled = false;
                txtDescripcion.Enabled = false;
                txtPrecioCompra.Enabled = false;
                txtPVP.Enabled = false;
                txtStockMinimo.Enabled = false;

                // Habilitar solo el TextBox de Stock Máximo
                txtStockMaximo.Enabled = true;
                // Si existe, aumentar el stock
                BuscarProductoPorCodigoBarras(txtCodigoBarras.Text);
            }
            else
            {
                // Bloquear los TextBox que no deben ser editados
                txtNombreProducto.Enabled = true;
                txtDescripcion.Enabled = true;
                txtPrecioCompra.Enabled = true;
                txtPVP.Enabled = true;
                txtStockMinimo.Enabled = true;

                // Habilitar solo el TextBox de Stock Máximo
                txtStockMaximo.Enabled = true;
                LimpiarTextBox();
            }
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
            }
        }
        private bool ProductoExiste(string codigoBarras)
        {
            // Verificar si el producto ya existe por código de barras
            string query = "SELECT COUNT(*) FROM Productos WHERE [Código Barras] = ?";
            using (OleDbCommand command = new OleDbCommand(query, conexion))
            {
                conexion.Open();
                command.Parameters.AddWithValue("CódigoBarras", codigoBarras);
                int count = Convert.ToInt32(command.ExecuteScalar());
                conexion.Close();
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

        private void txtCodeProductV_Leave(object sender, EventArgs e)
        {
            string codigoProducto = txtCodeProductV.Text.Trim();

            // Verificar si el código del producto no está vacío
            if (!string.IsNullOrEmpty(codigoProducto))
            {
                // Consultar la base de datos para obtener el nombre del producto
                string query = "SELECT Nombre FROM Productos WHERE [Código Barras] = @Codigo";
                OleDbCommand comando = new OleDbCommand(query, conexion);
                comando.Parameters.AddWithValue("@Codigo", codigoProducto);

                try
                {
                    conexion.Open();
                    object resultado = comando.ExecuteScalar();

                    // Verificar si se encontró un nombre para el código proporcionado
                    if (resultado != null)
                    {
                        txtNameProductV.Text = resultado.ToString();
                    }
                    else
                    {
                        MessageBox.Show("El código no es válido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCodeProductV.Focus();
                        txtCodeProductV.SelectAll();
                        txtNameProductV.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al acceder a la base de datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conexion.Close();
                }
            }
        }

        private void btnAddVenta_Click(object sender, EventArgs e)
        {
            string codigoProducto = txtCodeProductV.Text.Trim();
            string nombreProducto = txtNameProductV.Text;
            decimal precioProducto;
            int cantidadVenta;

            // Verificar si el código del producto no está vacío
            if (string.IsNullOrEmpty(codigoProducto))
            {
                MessageBox.Show("Por favor, ingrese un código de producto válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Verificar si el producto existe en la base de datos
            string query = "SELECT PVP FROM Productos WHERE [Código Barras] = @Codigo";
            using (OleDbConnection conexion = new OleDbConnection(connectionString))
            using (OleDbCommand comando = new OleDbCommand(query, conexion))
            {
                comando.Parameters.AddWithValue("@Codigo", codigoProducto);

                try
                {
                    conexion.Open();
                    object resultado = comando.ExecuteScalar();

                    // Verificar si se encontró un precio para el código proporcionado
                    if (resultado != null)
                    {
                        precioProducto = Convert.ToDecimal(resultado);
                    }
                    else
                    {
                        MessageBox.Show("El producto no se encuentra en la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCodeProductV.Clear();
                        txtNameProductV.Clear();
                        txtCantidadVenta.Clear();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al acceder a la base de datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Verificar si la cantidad es un número válido
            if (!int.TryParse(txtCantidadVenta.Text, out cantidadVenta) || cantidadVenta <= 0)
            {
                MessageBox.Show("Por favor, ingrese una cantidad válida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCantidadVenta.Focus();
                txtCantidadVenta.SelectAll();
                return;
            }
            string stockQuery = "SELECT [Stock Máximo], [Stock Mínimo] FROM Productos WHERE [Código Barras] = @Codigo";
            using (OleDbConnection conexionStock = new OleDbConnection(connectionString))
            using (OleDbCommand comandoStock = new OleDbCommand(stockQuery, conexionStock))
            {
                comandoStock.Parameters.AddWithValue("@Codigo", codigoProducto);

                try
                {
                    conexionStock.Open();
                    using (OleDbDataReader reader = comandoStock.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int stockMaximo = reader.GetInt32(0);
                            int stockMinimo = reader.GetInt32(1);

                            if (stockMaximo - cantidadVenta < 0)
                            {
                                MessageBox.Show("La cantidad ingresada excede el stock mínimo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtCantidadVenta.Focus();
                                txtCantidadVenta.SelectAll();
                                return;
                            }
                            else if (stockMaximo - cantidadVenta <= stockMinimo)
                            {
                                MessageBox.Show("Por favor, actualice el stock del producto.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                // Puedes añadir aquí el código adicional si lo necesitas
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al acceder al stock máximo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Calcular el total
            decimal totalVenta = precioProducto * cantidadVenta;

            // Agregar la fila al DataGridView
            dataGridVentas.Rows.Add(codigoProducto, nombreProducto, precioProducto, cantidadVenta, totalVenta);

            // Limpiar los controles
            txtCodeProductV.Clear();
            txtNameProductV.Clear();
            txtCantidadVenta.Clear();
        }
        private bool VerificarClienteExistente(string cedula)
        {
            bool clienteExistente = false;

            try
            {
                conexion.Open();

                string query = "SELECT COUNT(*) FROM Clientes WHERE Cédula = @Cedula";
                OleDbCommand cmd = new OleDbCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Cedula", cedula);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                // Si el cliente ya existe, establecer la bandera a true
                clienteExistente = count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al verificar cliente existente: " + ex.Message);
            }
            finally
            {
                conexion.Close();
            }

            return clienteExistente;
        }
        private bool ValidarCorreoElectronico(string correo)
        {
            // Expresión regular para validar dirección de correo electrónico
            string patronCorreo = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            return System.Text.RegularExpressions.Regex.IsMatch(correo, patronCorreo);
        }
        private bool ValidarCedula(string cedula)
        {
            // Expresión regular para validar cédula ecuatoriana
            string patronCedula = @"^[0-9]{10}$";

            return System.Text.RegularExpressions.Regex.IsMatch(cedula, patronCedula);
        }

        private bool ValidarNumeroTelefono(string telefono)
        {
            // Expresión regular para validar número de teléfono con código de país opcional
            string patronTelefono = @"^\+?[0-9]{1,4}?[0-9]{6,}$";

            return System.Text.RegularExpressions.Regex.IsMatch(telefono, patronTelefono);
        }



        private void btnFacturar_Click(object sender, EventArgs e)
        {
            string cedulaCliente = txtIDCliente.Text;
            string telefonoCliente = txtTelefono.Text;
            string correoCliente = txtEmail.Text;
            if (dataGridVentas.Rows.Count == 1)
            {
                MessageBox.Show("Debe hacer una compra antes");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtIDCliente.Text) || string.IsNullOrWhiteSpace(txtNombreCliente.Text) || string.IsNullOrWhiteSpace(txtTelefono.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Por favor, complete todos campos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Validar cédula, número de teléfono y correo electrónico
            if (!ValidarCedula(cedulaCliente))
            {
                MessageBox.Show("Formato de cédula no válido");
                return;
            }

            if (!ValidarNumeroTelefono(telefonoCliente))
            {
                MessageBox.Show("Formato de número de teléfono no válido");
                return;
            }

            if (!ValidarCorreoElectronico(correoCliente))
            {
                MessageBox.Show("Formato de correo electrónico no válido");
                return;
            }
            // Verificar si el cliente ya existe en la base de datos
            bool clienteExistente = VerificarClienteExistente(cedulaCliente);

            if (!clienteExistente)
            {
                // Si el cliente no existe, insertar el nuevo cliente
                InsertarCliente(cedulaCliente, txtNombreCliente.Text, txtTelefono.Text, txtEmail.Text);
            }

            // Recorrer las filas del DataGridView
            foreach (DataGridViewRow fila in dataGridVentas.Rows)
            {
                if (fila.Cells["Código"].Value != null)
                {
                    string codigoProducto = fila.Cells["Código"].Value.ToString();
                    int cantidad = Convert.ToInt32(fila.Cells["Cantidad"].Value);

                    // Insertar información de la venta en la tabla Ventas
                    InsertarVenta(cedulaCliente, codigoProducto, cantidad);
                    RestarStockMaximo(codigoProducto, cantidad);
                }
            }

            MessageBox.Show("Venta realizada con éxito");
            CrearFactura(cedulaCliente);
            tabControl1.SelectedIndex = 2;
            dataTable.Clear();
            adapter.Fill(dataTable);
            dataGridView1.DataSource = dataTable;
        }
        private decimal CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (DataGridViewRow fila in dataGridVentas.Rows)
            {
                if (fila.Cells["Código"].Value != null)
                {
                    decimal total = Convert.ToDecimal(fila.Cells["Total"].Value);
                    subtotal += total;
                }
            }

            return subtotal;
        }

        private void CrearFactura(string cedulaCliente)
        {
            try
            {
                conexion.Open();

                // Obtener información del cliente
                string obtenerClienteQuery = "SELECT [Nombre y Apellido], Teléfono, Correo FROM Clientes WHERE Cédula = @CedulaCliente";
                OleDbCommand obtenerClienteCmd = new OleDbCommand(obtenerClienteQuery, conexion);
                obtenerClienteCmd.Parameters.AddWithValue("@CedulaCliente", cedulaCliente);
                OleDbDataReader clienteReader = obtenerClienteCmd.ExecuteReader();

                // Crear factura en el RichTextBox
                rtbFactura.Clear();
                rtbFactura.AppendText("Centro Naturista El Silo\n");
                rtbFactura.AppendText($"Fecha: {DateTime.Now.ToString("dd/MM/yyyy - HH:mm")}\n\n");

                if (clienteReader.Read())
                {
                    string nombreCliente = clienteReader["Nombre y Apellido"].ToString();
                    string telefonoCliente = clienteReader["Teléfono"].ToString();
                    string correoCliente = clienteReader["Correo"].ToString();

                    rtbFactura.AppendText($"CI Cliente: {cedulaCliente}\n");
                    rtbFactura.AppendText($"Cliente: {nombreCliente}\n");
                    rtbFactura.AppendText($"Teléfono: {telefonoCliente}\n");
                    rtbFactura.AppendText($"Correo: {correoCliente}\n\n");
                }

                rtbFactura.AppendText("CANT\t\tNombre\t\t\t\t\tV. UNIT\t\t\t\tV. TOTAL\n");

                foreach (DataGridViewRow fila in dataGridVentas.Rows)
                {
                    if (fila.Cells["Código"].Value != null)
                    {
                        string descripcion = fila.Cells["Nombre"].Value.ToString();
                        int cantidad = Convert.ToInt32(fila.Cells["Cantidad"].Value);
                        decimal precioUnitario = Convert.ToDecimal(fila.Cells["Precio"].Value);
                        decimal total = cantidad * precioUnitario;

                        rtbFactura.AppendText($"{cantidad}\t\t{descripcion}\t\t\t{precioUnitario:C}\t\t\t\t{total:C}\n");
                    }
                }

                decimal subtotal = CalcularSubtotal();
                decimal iva = subtotal * 0.12m;
                decimal total1 = subtotal + iva;
                rtbFactura.AppendText($"\n");
                rtbFactura.AppendText($"\nSubtotal\t\t\t\t\t{subtotal:C}\n");
                rtbFactura.AppendText($"Iva 12%\t\t\t\t\t{iva:C}\n");
                rtbFactura.AppendText($"Total\t\t\t\t\t{total1:C}\n");

                rtbFactura.AppendText("\n*** GRACIAS POR SU COMPRA ***");
                dataGridVentas.Rows.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear la factura: " + ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }
        private void RestarStockMaximo(string codigoProducto, int cantidad)
        {
            try
            {
                conexion.Open();

                // Obtener el stock máximo actual del producto
                string obtenerStockMaximoQuery = "SELECT [Stock Máximo] FROM Productos WHERE [Código Barras] = @CodigoProducto";
                OleDbCommand obtenerStockMaximoCmd = new OleDbCommand(obtenerStockMaximoQuery, conexion);
                obtenerStockMaximoCmd.Parameters.AddWithValue("@CodigoProducto", codigoProducto);
                int stockMaximoActual = Convert.ToInt32(obtenerStockMaximoCmd.ExecuteScalar());

                // Calcular el nuevo stock máximo restando la cantidad vendida
                int nuevoStockMaximo = stockMaximoActual - cantidad;

                // Actualizar el stock máximo en la tabla Productos
                string actualizarStockMaximoQuery = "UPDATE Productos SET [Stock Máximo] = @NuevoStockMaximo WHERE [Código Barras] = @CodigoProducto";
                OleDbCommand actualizarStockMaximoCmd = new OleDbCommand(actualizarStockMaximoQuery, conexion);
                actualizarStockMaximoCmd.Parameters.AddWithValue("@NuevoStockMaximo", nuevoStockMaximo);
                actualizarStockMaximoCmd.Parameters.AddWithValue("@CodigoProducto", codigoProducto);

                actualizarStockMaximoCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al restar el stock máximo: " + ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }
        private void InsertarCliente(string cedula, string nombreApellido, string telefono, string correo)
        {
            try
            {
                conexion.Open();

                string query = "INSERT INTO Clientes (Cédula, [Nombre y Apellido], Teléfono, Correo) VALUES (@Cedula, @NombreApellido, @Telefono, @Correo)";
                OleDbCommand cmd = new OleDbCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Cedula", cedula);
                cmd.Parameters.AddWithValue("@NombreApellido", nombreApellido);
                cmd.Parameters.AddWithValue("@Telefono", telefono);
                cmd.Parameters.AddWithValue("@Correo", correo);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al insertar cliente: " + ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }

        private void InsertarVenta(string cedulaCliente, string codigoProducto, int cantidad)
        {
            try
            {
                conexion.Open();

                string query = "INSERT INTO Ventas ([Cédula Cliente], [Código Barras Producto], Cantidad, [Fecha y Hora]) VALUES (@CedulaCliente, @CodigoProducto, @Cantidad, @FechaHora)";
                OleDbCommand cmd = new OleDbCommand(query, conexion);
                cmd.Parameters.AddWithValue("@CedulaCliente", cedulaCliente);
                cmd.Parameters.AddWithValue("@CodigoProducto", codigoProducto);
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                cmd.Parameters.AddWithValue("@FechaHora", DateTime.Now.ToString());

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al insertar venta: " + ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            // Crear un documento PDF
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Configurar la fuente y la posición inicial
            XFont font = new XFont("Arial", 12);
            XPoint point = new XPoint(10, 10);

            // Obtener el contenido del RichTextBox
            string contenido = rtbFactura.Text;

            // Dividir el contenido en líneas
            string[] lineas = contenido.Split('\n');

            // Escribir cada línea en el documento PDF
            foreach (string linea in lineas)
            {
                gfx.DrawString(linea, font, XBrushes.Black, point);
                point.Y += 12; // Ajustar la posición vertical para la siguiente línea
            }

            // Especificar la ruta del archivo PDF
            string nombreArchivo = $"Factura_{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf";
            string rutaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string rutaPDF = Path.Combine(rutaUsuario, nombreArchivo);


            // Guardar el documento PDF en el archivo
            document.Save(rutaPDF);

            // Abrir el archivo PDF con el visor de PDF predeterminado
            System.Diagnostics.Process.Start(rutaPDF);
        }

        private void txtIDCliente_Leave(object sender, EventArgs e)
        {
            string cedula = txtIDCliente.Text;
            try
            {
                conexion.Open();

                string query = "SELECT [Nombre y Apellido], Teléfono, Correo FROM Clientes WHERE Cédula = @Cedula";
                OleDbCommand cmd = new OleDbCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Cedula", cedula);

                OleDbDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Llenar los TextBox con la información del cliente existente
                    txtNombreCliente.Text = reader["Nombre y Apellido"].ToString();
                    txtTelefono.Text = reader["Teléfono"].ToString();
                    txtEmail.Text = reader["Correo"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al recuperar información del cliente: " + ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }
    }
}
