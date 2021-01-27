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
using System.Drawing.Printing;
using System.IO;

namespace kurs_shop
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\Database1.mdf';Integrated Security=True";
        private string sql = "SELECT * FROM Products";
        private SqlDataAdapter adapter { get; set; }
        private DataSet dataSet { get; set; }
        private SqlCommandBuilder commandBuilder { get; set; }
        private List<string> listOfProducts { get; set; }
        private DataTable purchases { get; set; }
        public Form1()
        {
            InitializeComponent();
            purchases = new DataTable();
            listOfProducts = new List<string>();
            dataSet = new DataSet("productsSet");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    adapter = new SqlDataAdapter(sql, connection);
                    dataSet.Tables.Add(new DataTable("products"));
                    adapter.Fill(dataSet.Tables["products"]);
                    dataGridView1.DataSource = dataSet.Tables["products"];
                }
                for(int i=0; i<dataSet.Tables["products"].Rows.Count; i++)
                    listOfProducts.Add(dataSet.Tables["products"].Rows[i]["name"].ToString());
                products.DataSource = listOfProducts;
                purchases.Columns.Add("Name", typeof(String));
                purchases.Columns.Add("Quantity", typeof(String));
                purchases.Columns.Add("PPO", typeof(String));
                purchases.Columns.Add("Amount", typeof(String));
            }
            catch(Exception ex)
            {
                MessageBox.Show("No Connection." + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void save_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    adapter = new SqlDataAdapter(sql, connection);
                    commandBuilder = new SqlCommandBuilder(adapter);
                    adapter.Update(dataSet,"products");
                    MessageBox.Show("Table saved!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data base didn't saved! Error code: " + ex.Message, "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void add_Click(object sender, EventArgs e)
        {
            DataRow row = dataSet.Tables[0].NewRow();
            dataSet.Tables["products"].Rows.Add(row);
        }

        private void delete_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                    dataGridView1.Rows.Remove(row);
                save_Click(null, null);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Wrong action! Code error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void check_Click(object sender, EventArgs e)
        {
            paper.Text = "Cash register receipt";
            DataRow TRow = purchases.NewRow();
            float quanty = Convert.ToSingle(quantity.Text.ToString());
            foreach (DataRow row in dataSet.Tables["products"].Rows)
                if (row["name"].ToString() == products.SelectedItem.ToString())
                {
                    TRow["Name"] = row["name"].ToString();
                    TRow["Quantity"] = quanty.ToString();
                    TRow["PPO"] = row["price"].ToString();
                    TRow["Amount"] = (quanty * Convert.ToSingle(row["price"])).ToString();
                }
            purchases.Rows.Add(TRow);
            basket.DataSource = purchases;
            finish.Text= "Date..........................." + DateTime.Now.ToShortDateString() + "\n" +
                         "Time..........................." + DateTime.Now.ToLongTimeString() + "\n" +
                         "Thank you for your purchase!";
            print.Enabled = paper.Text != string.Empty;
        }

        private void quantity_TextChanged(object sender, EventArgs e)
        {
            if (quantity.Text != string.Empty)
                check.Enabled = true;
        }

        private void quantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',' && new string((quantity.Text + e.KeyChar).ToCharArray())[0] == ',')
            {
                e.Handled = true;
                return;
            }
            if (Char.IsControl(e.KeyChar))
                return;
            if (e.KeyChar == '.')
                e.KeyChar = ',';
            if (e.KeyChar.ToString() == string.Empty || int.TryParse(e.KeyChar.ToString(), out _) || e.KeyChar == ',')
                return;
            e.Handled = true;
        }
        
        private void print_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
            for(int i=0; i < purchases.Rows.Count; i++)
                for (int j = 0; j < dataSet.Tables[0].Rows.Count; j++)
                    if (purchases.Rows[i]["Name"].Equals(dataSet.Tables[0].Rows[j]["Name"]))
                        dataSet.Tables[0].Rows[j]["quantity"] = Convert.ToSingle(dataSet.Tables[0].Rows[j]["quantity"]) - Convert.ToSingle(purchases.Rows[i]["quantity"]);
            save_Click(null, null);
            purchases.Clear();
            MessageBox.Show("Check printed successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            DataGridView b = basket;
            b.RowHeadersVisible = false;
            b.Size = new Size(430,204);
            Bitmap bmp = new Bitmap(b.Size.Width, b.Size.Height);
            b.DrawToBitmap(bmp, new Rectangle(0,0, b.Size.Width, b.Size.Height));
            e.Graphics.DrawString(paper.Text.ToString(), new Font("Arial", 14), Brushes.Black, 0, 15);
            e.Graphics.DrawImage(bmp, 0, 50);
            e.Graphics.DrawString(finish.Text.ToString(), new Font("Arial", 14), Brushes.Black, 0, b.Size.Height+70);
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.FormattedValue.ToString() == string.Empty)
                return;
            switch (e.ColumnIndex)
            {
                case 0:
                    if (!int.TryParse(e.FormattedValue.ToString(), out _))
                    {
                        e.Cancel = true;
                        return;
                    }
                    break;
                case 3:
                case 4:
                    if (!float.TryParse(e.FormattedValue.ToString(), out _))
                    {
                        e.Cancel = true;
                        return;
                    }
                    break;
                case 5:
                    if (!DateTime.TryParse(e.FormattedValue.ToString(), out _))
                    {
                        e.Cancel = true;
                        return;
                    }
                    break;
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if(e.Exception != null && e.Context == DataGridViewDataErrorContexts.Commit)
                MessageBox.Show("Incorrect data! Please reenter! Error code:"+e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        private void quastion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The application <Vegetable Shop> is a database of products in the store.\n Here you can add new types of products and also change their price.\n To add a new product, click on the <Add> button. To delete <Delete>.\n You can also collect your own shopping cart and print a receipt.\n The work was performed by Ilya Shevtsov and Artem Shershakov.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
