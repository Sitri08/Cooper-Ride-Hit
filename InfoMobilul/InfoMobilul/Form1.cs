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

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;


namespace InfoMobilul
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        

        private void Button2_Click(object sender, EventArgs e)
        {
            if (UtilizatorCorect())
            {
                AdaugaLogare();
                MainForm FormPrinc = new MainForm();
                FormPrinc.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Date gresite. Va rugam incercati din nou.");
                textBox1.Text = textBox2.Text = "";
            }
        }

        private bool UtilizatorCorect()
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format("SELECT * FROM Utilizatori WHERE Email = '{0}' AND Parola = '{1}'", textBox2.Text, textBox1.Text);
            int rez = Convert.ToInt32(cmd.ExecuteScalar());
            if (rez >= 1) return true;

            con.Close();

            return false;
        }
        
        private void AdaugaLogare()
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.CommandText = string.Format(
                "SELECT Id FROM Utilizatori WHERE Email = '{0}'",
                textBox2.Text
                );
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            string IdUtilizator = Convert.ToString(reader[0]);
            reader.Close();

            cmd.CommandText = String.Format(
                "INSERT INTO Logari(IdUtilizator, DataLogare) VALUES('{0}', '{1}')",
                IdUtilizator, Convert.ToString(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                );
            cmd.ExecuteNonQuery();

            con.Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            InregForm formInreg = new InregForm();
            formInreg.Show();
            this.Hide();
        }

        private void Button2_MouseHover(object sender, EventArgs e)
        {
            button2.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;
            button2.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void Button2_MouseLeave(object sender, EventArgs e)
        {
            button2.BackgroundImage = InfoMobilul.Properties.Resources.blue_yellow;
            button2.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void Button3_MouseHover(object sender, EventArgs e)
        {
            button3.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;
        }

        private void Button3_MouseEnter(object sender, EventArgs e)
        {
            button3.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;
        }

        private void Button3_MouseLeave(object sender, EventArgs e)
        {
            button3.BackgroundImage = InfoMobilul.Properties.Resources.blue_yellow;
        }

        private void Button2_MouseEnter(object sender, EventArgs e)
        {
            button2.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;
            button2.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //IncarcaDate();
        }

        int operatie;
        private void Button1_Click(object sender, EventArgs e)
        {
            button1.Text = operatie + "";
        }

        private void apasa_buton(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            if (button.Text == "button4") operatie = 1;
            else operatie = 2;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            apasa_buton(sender, e);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            apasa_buton(sender, e);
        }
    }
}
