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
    public partial class Form2 : Form
    {
        private SqlConnection cn;
        private int currentLivro;
        private int currentLivroAdded = 0;
        private static string connectionString;
        Cliente c;
        public Form2(string conection,Cliente c = null )
        {
            InitializeComponent();
            this.c = c;
            connectionString = conection;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (!verifySGBDConnection())
                return;

            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = " SELECT li.ISBN, li.titulo, li.ano, li.id_editora, ed.nome_editora , li.categoria,le.cota, le.estado,le.numero_exemplar " +
                              " FROM Biblioteca.Livros_Exemplares as le JOIN Biblioteca.Livro as li ON li.ISBN = le.ISBN " +
                              "                                         JOIN Biblioteca.Editora as ed ON li.id_editora = ed.id_editora " +
                              "where le.numero_exemplar in (select le.numero_exemplar from BIBLIOTECA.Emprestimo as e join BIBLIOTECA.Livros_Exemplares le on e.n_emprestimo = le.n_emprestimo where e.data_chegada IS NOT null)";
            cmd.Connection = cn;
            SqlDataReader reader = cmd.ExecuteReader();
            listBox1.Items.Clear();

            while (reader.Read())
            {
                Livro l = new Livro();
                l.ISBN = (String)reader["ISBN"];
                l.Titulo = (String)reader["titulo"];
                l.Ano = (int)reader["ano"];
                l.Id_editora = (int)reader["id_editora"];
                if(reader["categoria"].ToString() !="")
                    l.Categoria = (String)reader["categoria"];
                //l.CountTilulos = (int)reader["countTitulos"];
                l.Nome_editora = (String)reader["nome_editora"];
                l.Estado = (String)reader["estado"];
                l.Cota = (String)reader["cota"];
                l.Numero_exemplar = (int)reader["numero_exemplar"];
                listBox1.Items.Add(l);
            }
            listBox1.Sorted = true;
            cn.Close();
            buttonRemove.Visible = false;
        }

        public static SqlConnection getSGBDConnection()
        {
            return new SqlConnection(connectionString);
            //return new SqlConnection("Data Source = tcp:mednat.ieeta.pt\\SQLSERVER,8101; Initial Catalog = p1g2; uid = p1g2;" + "password = Sqlgang.99");
        }
        private bool verifySGBDConnection()
        {
            if (cn == null)
                cn = getSGBDConnection();

            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn.State == ConnectionState.Open;
        }


        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (!verifySGBDConnection())
                return;

            if (textSearch.Text == "")
            {
                Form2_Load(sender, e);
            }
            else
            {
                SqlCommand cmd = new SqlCommand();
                
                cmd.CommandText = " SELECT li.ISBN, li.titulo, li.ano, li.id_editora, ed.nome_editora , li.categoria,le.cota, le.estado,le.numero_exemplar " +
                              " FROM Biblioteca.Livros_Exemplares as le JOIN Biblioteca.Livro as li ON li.ISBN = le.ISBN " +
                              "                                         JOIN Biblioteca.Editora as ed ON li.id_editora = ed.id_editora " +
                              "where le.numero_exemplar in (select le.numero_exemplar from BIBLIOTECA.Emprestimo as e join BIBLIOTECA.Livros_Exemplares le on e.n_emprestimo = le.n_emprestimo where e.data_chegada IS NOT null)" +
                              "and li.titulo like @varsearch";



                cmd.Parameters.AddWithValue("@varSearch", "%"+textSearch.Text+"%");
                cmd.Connection = cn;
                SqlDataReader reader = cmd.ExecuteReader();
                listBox1.Items.Clear();

                while (reader.Read())
                {
                    Livro l = new Livro();
                    l.ISBN = (String)reader["ISBN"];
                    l.Titulo = (String)reader["titulo"];
                    l.Ano = (int)reader["ano"];
                    l.Id_editora = (int)reader["id_editora"];
                    if (reader["categoria"].ToString() != "")
                        l.Categoria = (String)reader["categoria"];
                    //l.CountTilulos = (int)reader["countTitulos"];
                    //l.CountTilulos = (int)reader["countTitulos"];
                    l.Nome_editora = (String)reader["nome_editora"];
                    l.Estado = (String)reader["estado"];
                    l.Cota = (String)reader["cota"];
                    l.Numero_exemplar = (int)reader["numero_exemplar"];
                    listBox1.Items.Add(l);
                }
                cn.Close();
                listBox1.Sorted = true;
                currentLivro = 0;
                //ShowLivro();
                //ShowButtons();
            }
        }

        public void ShowLivro()
        {
            currentLivro = listBox1.SelectedIndex;
            if (listBox1.Items.Count == 0 | currentLivro < 0) return;
            Livro livro = new Livro();
            livro = (Livro)listBox1.Items[currentLivro];
            textCota.Text = livro.Cota.ToString();
            textCategoria.Text = livro.Categoria;
            textEditora.Text = livro.Nome_editora.ToString();
            textBoxEstado.Text = livro.Estado.ToString();
            textBoxID.Text = livro.Numero_exemplar.ToString();
        }
        
        public void ShowLivro2()
        {
            currentLivro = listBox2.SelectedIndex;
            if (listBox1.Items.Count == 0 | currentLivro < 0) return;
            Livro livro = new Livro();
            livro = (Livro)listBox2.Items[currentLivro];
            textCota.Text = livro.Cota.ToString();
            textCategoria.Text = livro.Categoria;
            textEditora.Text = livro.Nome_editora.ToString();
            textBoxEstado.Text = livro.Estado.ToString();
            textBoxID.Text = livro.Numero_exemplar.ToString();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0 | listBox1.SelectedIndex == -1) return;

            currentLivro = listBox1.SelectedIndex;
            if (currentLivro < 0)
            {
                MessageBox.Show("Please select a Book");
                return;
            }
            Livro livro = new Livro();
            livro = (Livro)listBox1.Items[currentLivro];
            //if (livro.CountTilulos > 0)
            //{
                listBox2.Items.Add(livro);
            listBox1.Items.Remove(livro);

                //livro.CountTilulos--;
                ShowLivro();
            //}
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0  | listBox2.SelectedIndex == -1) return;
            currentLivroAdded = listBox2.SelectedIndex;
            Livro livro = new Livro();
            livro = (Livro)listBox2.Items[currentLivroAdded];
            //livro.CountTilulos++;
            listBox2.Items.Remove(livro);
            listBox1.Items.Add(livro);
            listBox1.Sorted = true;
            ShowLivro();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            int rows = 0;
            if (!verifySGBDConnection())
                return;
            SqlCommand cmd = new SqlCommand("BIBLIOTECA.FazerEmprestimo", cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();

            string livros_ids ="";

            foreach (var item in listBox2.Items)
            {
                Livro livro = new Livro();
                livro = (Livro) item; //Falta fazer um ciclo for aqui ou na query
                livros_ids += livro.Numero_exemplar + ";";

            }
            cmd.Parameters.Add("@numeros_exemplares", SqlDbType.VarChar).Value = livros_ids;
            cmd.Parameters.Add("@id_funcionario", SqlDbType.Int).Value = "101"; //Falta ver qual o funcionario
            cmd.Parameters.Add("@id_cliente", SqlDbType.Int).Value = c.Id;

            cmd.Connection = cn;
            try
            {
                rows = cmd.ExecuteNonQuery();
                MessageBox.Show("Requesitados com Sucesso!","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possivel Requesitar estes livros!","Erro",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            finally
            {
                cn.Close();
            }

            this.Close();  //Fechar o Form dos Emprestimo
            Form2_Load(sender, e);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonAdd.Visible = true;
            buttonRemove.Visible = false;
            ShowLivro();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex == -1)
            {
                buttonAdd.Visible = true;
                buttonRemove.Visible = false;
            }
            else
            {
                buttonAdd.Visible = false;
                buttonRemove.Visible = true;
            }
            ShowLivro2();
        }
    }
}
