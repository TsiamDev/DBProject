using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

namespace ProjectDB2019
{
    public partial class Form1 : Form
    {
        string connectionString = "server=localhost;port=3306;database=publishing_house_db;uid=root;password=";
        List<TabPage> tabs;
        string email;
        List<string> old_editor;
        public Form1()
        {
            InitializeComponent();
            //Hold tabs
            tabs = new List<TabPage>();
            //Disable irrelevant tab pages
            foreach (TabPage tp in tabControl1.TabPages)
            {
                tabs.Add(tp);
                tp.Parent = null;
                if (tp.Text == "Log In")
                {
                    tp.Parent = tabControl1;
                }
            }

            //Change Datetime format
            CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "yyyy'/'MM'/'dd";
            ci.DateTimeFormat.LongTimePattern = "hh':'mm':'ss";
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;


        }

        private void Log_in_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        //execute stored procedure
                        MySqlCommand cmd = new MySqlCommand("check_user_pwd", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Username_tb.Text;
                        cmd.Parameters.Add("@password", MySqlDbType.VarChar).Value = Password_tb.Text;
                        cmd.Parameters.Add("@result", MySqlDbType.VarChar);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@found_e", MySqlDbType.VarChar);
                        cmd.Parameters["@found_e"].Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        //MessageBox.Show("Command Executed");

                        //read out variable value
                        string t = (string)cmd.Parameters["@result"].Value;
                        email = (string)cmd.Parameters["@found_e"].Value;
                        //Dissable log in tab and enable appropriate tab
                        if (t != "not found")
                        {
                            foreach (TabPage tp in tabs)
                            {
                                if (tp.Text == t)
                                {
                                    tp.Parent = tabControl1;
                                }
                                if (tp.Text == "Log In")
                                {
                                    tp.Parent = null;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(t);
                        }
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        //editor button
        private void button1_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        //execute stored procedure
                        MySqlCommand cmd = new MySqlCommand("get_article", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = email;

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        dataGridView1.DataSource = dt;

                        for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                        {
                            DataGridViewComboBoxCell cbc = new DataGridViewComboBoxCell();
                            cbc.Items.Add("approved");
                            cbc.Items.Add("rejected");
                            cbc.Items.Add("to change");
                            cbc.Value = dataGridView1.Rows[i].Cells[2].Value;
                            dataGridView1.Rows[i].Cells[2] = cbc;
                        }

                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            string str;
            str = dataGridView1.Rows[dataGridView1.CurrentCellAddress.Y].Cells[dataGridView1.CurrentCellAddress.X].Value.ToString();
            //MessageBox.Show(str);
        }

        private void ed_button_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        string sql = @"update article set article.to_check = @check, 
                                article.start_page = @start_page, 
                                article.approved_date = @a_date 
                                where article.article_path = @a_path;";
                        MySqlCommand cmd = new MySqlCommand(sql, connection);
                        for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@check", MySqlDbType.Enum).Value = dataGridView1.Rows[i].Cells[2].Value;
                            cmd.Parameters.Add("@start_page", MySqlDbType.Int64).Value = dataGridView1.Rows[i].Cells[3].Value;
                            cmd.Parameters.Add("@a_path", MySqlDbType.VarChar).Value = dataGridView1.Rows[i].Cells[0].Value;
                            if (dataGridView1.Rows[i].Cells[2].Value.ToString() == "approved")
                            {
                                cmd.Parameters.Add("@a_date", MySqlDbType.DateTime).Value = DateTime.Now.ToString();
                            }
                            else
                            {
                                cmd.Parameters.Add("@a_date", MySqlDbType.DateTime).Value = null;
                            }
                            //MessageBox.Show("cmd " + cmd.ExecuteNonQuery().ToString());
                            cmd.ExecuteNonQuery();
                        }

                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void insert_article_button_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        string sql = @"insert into article values(@a_path,@title,@summary,@published_at,
                                    @belongs_to_cat,@a_num,@to_check,@approved_date,@pages_num,@start_page,@supl_path);";
                        MySqlCommand cmd = new MySqlCommand(sql, connection);
                        cmd.Parameters.Add("@a_path", MySqlDbType.VarChar).Value = a_path_tb.Text;
                        cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = title_tb.Text;
                        cmd.Parameters.Add("@summary", MySqlDbType.Text).Value = summary_tb.Text;
                        cmd.Parameters.Add("@published_at", MySqlDbType.Int64).Value = published_at_tb.Text;
                        cmd.Parameters.Add("@belongs_to_cat", MySqlDbType.Int64).Value = belongs_to_cat_tb.Text;
                        cmd.Parameters.Add("@a_num", MySqlDbType.Int64).Value = article_num_tb.Text;
                        cmd.Parameters.Add("@to_check", MySqlDbType.Enum).Value = "approved";
                        cmd.Parameters.Add("@approved_date", MySqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@pages_num", MySqlDbType.Int64).Value = pages_num_tb.Text;
                        cmd.Parameters.Add("@start_page", MySqlDbType.Int64).Value = start_page_tb.Text;
                        cmd.Parameters.Add("@supl_path", MySqlDbType.VarChar).Value = ed_supl_path_tb.Text;

                        //MessageBox.Show(cmd.ExecuteNonQuery().ToString());
                        cmd.ExecuteNonQuery();
                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void insert_category_button_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        string sql = @"insert into category values(@code,@name,@description,@parent_cat);";
                        MySqlCommand cmd = new MySqlCommand(sql, connection);
                        cmd.Parameters.Add("@code", MySqlDbType.Int64).Value = 0;
                        cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = name_tb.Text;
                        cmd.Parameters.Add("@description", MySqlDbType.Text).Value = description_tb.Text;
                        if ((string)ed_cat_cb.SelectedItem == "null")
                        {
                            cmd.Parameters.Add("@parent_cat", MySqlDbType.Int64).Value = null;
                        }
                        else
                        {
                            cmd.Parameters.Add("@parent_cat", MySqlDbType.Int64).Value = ed_cat_cb.SelectedItem.ToString();
                        }


                        cmd.ExecuteNonQuery();
                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;

                }
            }
        }

        //editor category combobox
        private void ed_cat_cb_DropDown(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        MySqlCommand cmd = new MySqlCommand("get_cat_code", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());

                        //update combobox options
                        int end = ed_cat_cb.Items.Count;
                        for (int i = 0; i < end; i++)
                        {
                            ed_cat_cb.Items.RemoveAt(0);
                        }
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ed_cat_cb.Items.Add(dt.Rows[i][0].ToString());
                        }
                        ed_cat_cb.Items.Add("null");
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;

                }
            }
        }

        private void admin_sal_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        MySqlCommand cmd = new MySqlCommand("get_sal_info", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@months", MySqlDbType.Int64).Value = admin_sal_tb.Text;

                        MySqlDataReader dr = cmd.ExecuteReader();
                        DataTable d = new DataTable();
                        while (dr.HasRows)
                        {
                            d.Load(dr);
                        }
                        admin_dataGridView.DataSource = d;
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;

                }
            }
        }

        private void admin_ref_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        MySqlCommand cmd = new MySqlCommand("get_leaf_info", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        admin_lf_dataGridView.DataSource = dt;

                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }


            }
        }

        private void admin_sub_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        string sql = @"update leaf set leaf.lf_not_sold = @lf_not_sold where leaf.leaf_num = @leaf_num;";
                        MySqlCommand cmd2 = new MySqlCommand(sql, connection);
                        for (int i = 0; i < admin_lf_dataGridView.RowCount - 1; i++)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.Parameters.Add("@lf_not_sold", MySqlDbType.Int64).Value = admin_lf_dataGridView.Rows[i].Cells[5].Value;
                            cmd2.Parameters.Add("@leaf_num", MySqlDbType.Int64).Value = admin_lf_dataGridView.Rows[i].Cells[1].Value;
                            //MessageBox.Show("cmd2 " + cmd2.ExecuteNonQuery().ToString());
                            cmd2.ExecuteNonQuery();
                        }

                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void publisher_ref_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        MySqlCommand cmd = new MySqlCommand("get_paper_info", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@owner", MySqlDbType.VarChar).Value = email;

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        publisher_dataGridView.DataSource = dt;

                        old_editor = new List<string>();
                        for (int i = 0; i < publisher_dataGridView.RowCount - 1; i++)
                        {
                            old_editor.Add(publisher_dataGridView.Rows[i].Cells[3].Value.ToString());
                        }

                            transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }


            }
        }

        private void publisher_sub_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        MySqlCommand c = new MySqlCommand("add_editor", connection);
                        c.CommandType = CommandType.StoredProcedure;
                        for (int i = 0; i < publisher_dataGridView.RowCount - 1; i++)
                        {
                            c.Parameters.Add("@email", MySqlDbType.VarChar).Value = publisher_dataGridView.Rows[i].Cells[3].Value;
                            c.Parameters.Add("@old_editor", MySqlDbType.VarChar).Value = old_editor[i];
                            c.Parameters.Add("@papern", MySqlDbType.VarChar).Value = publisher_dataGridView.Rows[i].Cells[0].Value;
                            c.ExecuteNonQuery();
                        }

                        string sql = @"update paper set 
                                paper.owner = @owner, paper.publish_frequency = @publ_freq, 
                                paper.supervisor = @super where paper.name = @paper_name;";
                        MySqlCommand cmd = new MySqlCommand(sql, connection);
                        for (int i = 0; i < publisher_dataGridView.RowCount - 1; i++)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@paper_name", MySqlDbType.VarChar).Value = publisher_dataGridView.Rows[i].Cells[0].Value;
                            cmd.Parameters.Add("@owner", MySqlDbType.VarChar).Value = publisher_dataGridView.Rows[i].Cells[1].Value;
                            cmd.Parameters.Add("@publ_freq", MySqlDbType.Enum).Value = publisher_dataGridView.Rows[i].Cells[2].Value;
                            cmd.Parameters.Add("@super", MySqlDbType.VarChar).Value = publisher_dataGridView.Rows[i].Cells[3].Value;
                            //MessageBox.Show("cmd " + cmd.ExecuteNonQuery().ToString());
                            cmd.ExecuteNonQuery();
                        }

                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void publisher_lf_ref_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        MySqlCommand cmd = new MySqlCommand("get_leaf_printed", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        publisher_lf_dataGridView.DataSource = dt;

                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }


            }
        }

        private void publisher_lf_sub_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        string sql = @"update leaf set leaf.lf_printed = @lf_printed where leaf.leaf_num = @leaf_num;";
                        MySqlCommand cmd = new MySqlCommand(sql, connection);
                        for (int i = 0; i < publisher_lf_dataGridView.RowCount - 1; i++)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@leaf_num", MySqlDbType.Int64).Value = publisher_lf_dataGridView.Rows[i].Cells[0].Value;
                            cmd.Parameters.Add("@lf_printed", MySqlDbType.Int64).Value = publisher_lf_dataGridView.Rows[i].Cells[1].Value;
                            //MessageBox.Show("cmd " + cmd.ExecuteNonQuery().ToString());
                            cmd.ExecuteNonQuery();
                        }

                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void publisher_printed_rf_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        MySqlCommand cmd = new MySqlCommand("get_leaf_sales", connection);
                        cmd.CommandType = CommandType.StoredProcedure;

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        publisher_printed_dataGridView.DataSource = dt;
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        private void jour_article_sub_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        string sql = @"insert into article values(@a_path,@title,@summary,@published_at,
                                    @belongs_to_cat,@a_num,@to_check,@approved_date,@pages_num,@start_page,@supl_path);";
                        MySqlCommand cmd = new MySqlCommand(sql, connection);
                        cmd.Parameters.Add("@a_path", MySqlDbType.VarChar).Value = article_path_tb.Text;
                        cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = jour_title_tb.Text;
                        cmd.Parameters.Add("@summary", MySqlDbType.Text).Value = jour_sum_tb.Text;
                        cmd.Parameters.Add("@published_at", MySqlDbType.Int64).Value = jour_publ_at_tb.Text;
                        cmd.Parameters.Add("@belongs_to_cat", MySqlDbType.Int64).Value = jour_bel_to_cat_tb.Text;
                        cmd.Parameters.Add("@a_num", MySqlDbType.Int64).Value = jour_a_num_tb.Text;
                        cmd.Parameters.Add("@to_check", MySqlDbType.Enum).Value = null;
                        cmd.Parameters.Add("@approved_date", MySqlDbType.DateTime).Value = null;
                        cmd.Parameters.Add("@pages_num", MySqlDbType.Int64).Value = jour_pages_num_tb.Text;
                        cmd.Parameters.Add("@start_page", MySqlDbType.Int64).Value = jour_start_page_tb.Text;
                        cmd.Parameters.Add("@supl_path", MySqlDbType.VarChar).Value = jour_supl_path_tb.Text;
                        //MessageBox.Show("cmd " + cmd.ExecuteNonQuery().ToString());
                        cmd.ExecuteNonQuery();
                        /*
                        sql = @"insert into is_submitted_by values(@email,@art_path,@sub_date);";
                        MySqlCommand c = new MySqlCommand(sql, connection);
                        c.Parameters.Add("@email", MySqlDbType.VarChar).Value = email;
                        c.Parameters.Add("@art_path", MySqlDbType.VarChar).Value = article_path_tb.Text;
                        c.Parameters.Add("@sub_date", MySqlDbType.DateTime).Value = DateTime.Now;
                        c.ExecuteNonQuery();
                        */
                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        string sql = @"insert into is_submitted_by values(@email,@art_path,@sub_date);";
                        MySqlCommand c = new MySqlCommand(sql, connection);
                        c.Parameters.Add("@email", MySqlDbType.VarChar).Value = email;
                        c.Parameters.Add("@art_path", MySqlDbType.VarChar).Value = article_path_tb.Text;
                        c.Parameters.Add("@sub_date", MySqlDbType.DateTime).Value = DateTime.Now;
                        c.ExecuteNonQuery();

                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void jour_ref_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        MySqlCommand cmd = new MySqlCommand("get_submitted_article", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = email;

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        jour_dataGridView.DataSource = dt;

                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void jour_sub_art_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        // Passing an existing transaction to the context
                        context.Database.UseTransaction(transaction);

                        string sql = @"update article set article.title=@title,article.summary=@summary,
                                        article.published_at=@published_at,article.belongs_to_cat=@belongs_to_cat,
                                        article.article_num=@a_num,article.to_check=@to_check,article.approved_date=@approved_date,
                                        article.pages_num=@pages_num,article.start_page=@start_page,article.supl_path=@supl_path
                                        where article.article_path=@a_path;";
                        MySqlCommand cmd = new MySqlCommand(sql, connection);
                        for (int i = 0; i < jour_edit_art_dataGridView.RowCount - 1; i++)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@a_path", MySqlDbType.VarChar).Value = jour_edit_art_dataGridView.Rows[i].Cells[0].Value;
                            cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = jour_edit_art_dataGridView.Rows[i].Cells[1].Value;
                            cmd.Parameters.Add("@summary", MySqlDbType.Text).Value = jour_edit_art_dataGridView.Rows[i].Cells[2].Value;
                            cmd.Parameters.Add("@published_at", MySqlDbType.Int64).Value = jour_edit_art_dataGridView.Rows[i].Cells[3].Value;
                            cmd.Parameters.Add("@belongs_to_cat", MySqlDbType.Int64).Value = jour_edit_art_dataGridView.Rows[i].Cells[4].Value;
                            cmd.Parameters.Add("@a_num", MySqlDbType.Int64).Value = jour_edit_art_dataGridView.Rows[i].Cells[5].Value;
                            cmd.Parameters.Add("@to_check", MySqlDbType.Enum).Value = null;
                            cmd.Parameters.Add("@approved_date", MySqlDbType.DateTime).Value = null;
                            cmd.Parameters.Add("@pages_num", MySqlDbType.Int64).Value = jour_edit_art_dataGridView.Rows[i].Cells[8].Value;
                            cmd.Parameters.Add("@start_page", MySqlDbType.Int64).Value = jour_edit_art_dataGridView.Rows[i].Cells[9].Value;
                            cmd.Parameters.Add("@supl_path", MySqlDbType.VarChar).Value = jour_edit_art_dataGridView.Rows[i].Cells[10].Value;
                            //MessageBox.Show("cmd " + cmd.ExecuteNonQuery());
                            cmd.ExecuteNonQuery();
                        }
                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void jour_ref_art_bt_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // DbConnection that is already opened
                    using (Publishing_house_db context = new Publishing_house_db(connection, false))
                    {

                        // Interception/SQL logging
                        context.Database.Log = (string message) => { Console.WriteLine(message); };

                        MySqlCommand cmd = new MySqlCommand("get_submitted_article", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = email;

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        jour_edit_art_dataGridView.DataSource = dt;
                    }
                }
                catch
                {
                    throw;
                }


            }
        }
    }
}

