﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Biblioteca
{
    public partial class Form1 : Form
    {
        private SqlConnection cn;
        private int currentPessoa;
        private bool adding;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cn = getSGBDConnection();

            if (!verifySGBDConnection())
                return;

            SqlCommand cmd = new SqlCommand("SELECT * FROM Biblioteca.Pessoa", cn);
            SqlDataReader reader = cmd.ExecuteReader();
            listBox1.Items.Clear();

            while (reader.Read())
            {
                Cliente c = new Cliente();
                c.Id = (int) reader["id"];
                c.First_name = reader["first_name"].ToString();
                c.Last_name = reader["last_name"].ToString();
                c.Data_nascimento = (DateTime) reader["data_nascimento"];
                c.Telefone = (decimal) reader["telefone"];
                listBox1.Items.Add(c);
            }

            cn.Close();


            currentPessoa = 0;
            ShowPessoa();
            ShowButtons();
        }

        private SqlConnection getSGBDConnection()
        {
            return new SqlConnection("data source= localhost;integrated security=true;initial catalog=Biblioteca");
            //return new SqlConnection("Data Source = tcp:mednat.ieeta.pt\\SQLSERVER,8101; Initial Catalog = p1g2; uid = p1g2;" + "password = Sqlgang.99");
            //return new SqlConnection("data source= localhost;integrated security=true;");// initial catalog=Biblioteca");
        }
        private bool verifySGBDConnection()
        {
            if (cn == null)
                cn = getSGBDConnection();

            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn.State == ConnectionState.Open;
        }

        public void ShowPessoa()
        {
            if (listBox1.Items.Count == 0 | currentPessoa < 0) return;
            Cliente cliente = new Cliente();
            cliente = (Cliente)listBox1.Items[currentPessoa];
            textID.Text = cliente.Id.ToString();
            textFirstName.Text = cliente.First_name;
            textLastName.Text = cliente.Last_name;
            string[] str = cliente.Data_nascimento.ToString().Split(' '); //Para tirar as Horas à data
            textDataNascimento.Text = str[0];
            textTelefone.Text = cliente.Telefone.ToString();
        }

        private bool SavePessoa()
        {
            Cliente cliente = new Cliente();
            try
            {
                //cliente.Id = int.Parse(textID.Text);
                cliente.First_name = textFirstName.Text;
                cliente.Last_name = textLastName.Text;
                cliente.Data_nascimento = DateTime.Parse(textDataNascimento.Text);
                cliente.Telefone = decimal.Parse(textTelefone.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            if (adding)
            {
                cliente = SubmitPessoa(cliente);
                listBox1.Items.Add(cliente);
            }
            else
            {
                UpdatePessoa(cliente);
                listBox1.Items[currentPessoa] = cliente;
            }
            return true;
        }

        private Cliente SubmitPessoa(Cliente c)
        {
            if (!verifySGBDConnection())
                return null;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "INSERT Biblioteca.Pessoa (first_name, last_name, data_nascimento, telefone) output INSERTED.ID VALUES ( @first_name, @last_name, @data_nascimento, @telefone); ";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@first_name", c.First_name);
            cmd.Parameters.AddWithValue("@last_name", c.Last_name);
            cmd.Parameters.AddWithValue("@data_nascimento", c.Data_nascimento);
            cmd.Parameters.AddWithValue("@telefone", c.Telefone);
            cmd.Connection = cn;

            try
            {
                int id = (int)cmd.ExecuteScalar();
                c.Id = id;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update contact in database. \n ERROR MESSAGE: \n" + ex.Message);
            }
            finally
            {
                cn.Close();
            }
            return c;
        }

        private void UpdatePessoa(Cliente c)
        {
            int rows = 0;

            if (!verifySGBDConnection())
                return;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();

            //cmd.CommandText = "CREATE PROC BIBLIOTECA.EditCliente (@pessoaId int, @first_Name varchar(100), @last_Name varchar(100), @nas_Data date = null, @telemovel decimal(9,0) = null, @morada varchar(255), @mail varchar(100), @nif decimal(9,0)) " +
            //    " as " +
            //    "	    begin Transaction " +
            //    "	    update BIBLIOTECA.Pessoa SET first_name=@first_Name, last_name=@last_Name, data_nascimento=@nas_Data, telefone=@telemovel WHERE id=@pessoaId; " +
            //    "     update BIBLIOTECA.Cliente SET mail=@mail, morada=@morada, nif= @nif WHERE id_cliente = @pessoaId; " +
            //    "     if @@ERROR !=0 " +
            //    "         rollback tran " +
            //    "     else " +
            //    "         commit tran " +
            //    "     go " +

            cmd.CommandText = " EXECUTE Biblioteca.EditCliente @id, @first_name, @last_name, @data_nascimento, @telefone, @morada, @mail, @nif; ";
                //"UPDATE Biblioteca.Pessoa " + "SET first_name = @first_name, " + "    last_name = @last_name, " + "    data_nascimento = @data_nascimento, " + "    telefone = @telefone " + " WHERE id = @id";

            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.Parameters.AddWithValue("@first_name", c.First_name);
            cmd.Parameters.AddWithValue("@last_name", c.Last_name);
            cmd.Parameters.AddWithValue("@data_nascimento", c.Data_nascimento);
            cmd.Parameters.AddWithValue("@telefone", c.Telefone);
            cmd.Parameters.AddWithValue("@morada", "ola");
            cmd.Parameters.AddWithValue("@mail", "ole");
            cmd.Parameters.AddWithValue("@nif", (int) 273363620);
            cmd.Connection = cn;

            try
            {
                rows = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update contact in database. \n ERROR MESSAGE: \n" + ex.Message);
            }
            finally
            {
                if (rows == 1)
                    MessageBox.Show("Update OK");
                else
                    MessageBox.Show("Update NOT OK");

                cn.Close();
            }
        }

        private void RemovePessoa(int id)
        {
            if (!verifySGBDConnection())
                return;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "DELETE Biblioteca.Pessoa WHERE id=@id ";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Connection = cn;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete contact in database. \n ERROR MESSAGE: \n" + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }
       

        public void ClearFields()
        {
            textID.Text = "";
            textFirstName.Text = "";
            textLastName.Text = "";
            textDataNascimento.Text = "";
            textTelefone.Text = "";
        }

        public void ShowButtons()
        {
            LockControls();
            buttonAdd.Visible = true;
            buttonEdit.Visible = true;
            buttonRemove.Visible = true;
            buttonOk.Visible = false;
            buttonCancel.Visible = false;

        }

        public void HideButtons()
        {
            UnlockControls();
            buttonAdd.Visible = false;
            buttonEdit.Visible = false;
            buttonRemove.Visible = false;
            buttonOk.Visible = true;
            buttonCancel.Visible = true;
        }

        public void LockControls()
        {
            textID.ReadOnly = true;
            textFirstName.ReadOnly = true;
            textLastName.ReadOnly = true;
            textDataNascimento.ReadOnly = true;
            textTelefone.ReadOnly = true;
        }

        public void UnlockControls()
        {
            textID.ReadOnly = false;
            textFirstName.ReadOnly = false;
            textLastName.ReadOnly = false;
            textDataNascimento.ReadOnly = false;
            textTelefone.ReadOnly = false;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            adding = true;
            ClearFields();
            HideButtons();
            listBox1.Enabled = false;
        }


        private void buttonOk_Click(object sender, EventArgs e)
        {
            try
            {
                SavePessoa();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            listBox1.Enabled = true;
            int idx = listBox1.FindString(textID.Text);
            listBox1.SelectedIndex = idx;
            ShowButtons();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            currentPessoa = listBox1.SelectedIndex;
            if (currentPessoa < 0)
            {
                MessageBox.Show("Please select a contact to edit");
                return;
            }
            adding = false;
            HideButtons();
            listBox1.Enabled = false;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                try
                {
                    RemovePessoa(((Pessoa)listBox1.SelectedItem).Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                if (currentPessoa == listBox1.Items.Count)
                    currentPessoa = listBox1.Items.Count - 1;
                if (currentPessoa == -1)
                {
                    ClearFields();
                    MessageBox.Show("There are no more contacts");
                }
                else
                {
                    ShowPessoa();
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            listBox1.Enabled = true;
            if (listBox1.Items.Count > 0)
            {
                currentPessoa = listBox1.SelectedIndex;
                if (currentPessoa < 0)
                    currentPessoa = 0;
                ShowPessoa();
            }
            else
            {
                ClearFields();
                LockControls();
            }
            ShowButtons();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                currentPessoa = listBox1.SelectedIndex;
                ShowPessoa();
            }
        }

        private void Search_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textID_TextChanged(object sender, EventArgs e)
        {

        }
        private void textFirstName_TextChanged(object sender, EventArgs e)
        {

        }

        private void textLastName_TextChanged(object sender, EventArgs e)
        {

        }

        private void textTelefone_TextChanged(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
          
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {

            if (!verifySGBDConnection())
                return;

            if (textSearch.Text == "")
            {
                Form1_Load(sender, e);
            }
            else
            {

                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM Biblioteca.Pessoa WHERE first_name = @varSearch";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@varSearch", textSearch.Text);
                cmd.Connection = cn;
                SqlDataReader reader = cmd.ExecuteReader();
                listBox1.Items.Clear();

                while (reader.Read())
                {
                    Cliente c = new Cliente();
                    c.Id = (int)reader["id"];
                    c.First_name = reader["first_name"].ToString();
                    c.Last_name = reader["last_name"].ToString();
                    c.Data_nascimento = (DateTime)reader["data_nascimento"];
                    c.Telefone = (decimal)reader["telefone"];
                    listBox1.Items.Add(c);
                }


                cn.Close();


                currentPessoa = 0;
                ShowPessoa();
                ShowButtons();
            }
        }

        private void buttonEmprestimo_Click(object sender, EventArgs e)
        {
            var form2 = new Form2();
            form2.Show();

            //currentPessoa = 0;
            //ShowPessoa();
            //ShowButtons();
        }

        private void buttonHistorico_Click(object sender, EventArgs e)
        {
            var form3 = new Form3();
            form3.Show();
        }

        private void buttonInventartio_Click(object sender, EventArgs e)
        {
            var form4 = new Form4();
            form4.Show();
        }
    }
}
