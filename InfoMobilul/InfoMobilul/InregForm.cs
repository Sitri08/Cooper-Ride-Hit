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

namespace InfoMobilul
{
    public partial class InregForm : Form
    {
        string capchaC = "";
        public InregForm()
        {
            InitializeComponent();
            IncarcaCapcha();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Form1 formInc = new Form1();
            formInc.Show();
            this.Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //campuri goale
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "" || textBox4.Text == "" || textBox5.Text == "" || textBox6.Text == "")
            {
                MessageBox.Show("Toate campurile sunt obligatorii.");
                return;
            }
            if(checkBox1.Checked == checkBox2.Checked && checkBox3.Checked == false)
            {
                MessageBox.Show("Toate campurile sunt obligatorii.");
                return;
            }

            //parole identice
            if(textBox4.Text != textBox5.Text)
            {
                MessageBox.Show("Parolele nu coincid.");
                return;
            }

            //capcha corect
            if(capchaC.IndexOf(textBox6.Text) == -1)
            {
                MessageBox.Show("Capcha gresit.");
                return;
            }

            //email valid
            if(textBox3.Text.LastIndexOf('@') >= textBox3.Text.LastIndexOf('.'))
            {
                MessageBox.Show("Email invalid.");
                return;
            }

            if(!InregistreazaUtilizator())
            {
                MessageBox.Show("Emailul folosit este deja asociat unui cont.");
                return;
            }
            Form1 forminc = new Form1();
            forminc.Show();
            this.Close();
            MessageBox.Show("Inregistrat cu succes!");
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                checkBox2.Checked = false;
                checkBox3.Checked = false;
            }
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                checkBox3.Checked = false;
            }
        }

        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                checkBox2.Checked = false;
                checkBox1.Checked = false;
            }
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            IncarcaCapcha();
        }

        private void IncarcaCapcha()
        {
            string filePath = Application.ExecutablePath;
            filePath = filePath.Remove(filePath.Length - 25) + @"\Resources\Logare";
            string[] capchas = System.IO.Directory.GetFiles(filePath);
            string capcha = capchas[DateTime.Now.Ticks % capchas.Length];
            do capcha = capchas[DateTime.Now.Ticks % capchas.Length];
            while (Image.FromFile(capcha) == pictureBox1.Image);
            pictureBox1.Image = Image.FromFile(capcha);
            capchaC = capcha;
        }

        private bool InregistreazaUtilizator()
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            string sex = "Nespecificat";
            if (checkBox1.Checked) sex = "M";
            else if (checkBox2.Checked) sex = "F";

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.CommandText = @"SELECT * FROM Utilizatori WHERE Email = '" + textBox3.Text + '\'';
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) return false;
            reader.Close();

            cmd.CommandText = String.Format(
                "INSERT INTO Utilizatori(Email, Parola, Nume, Prenume, Sex, DataNasterii, DataInregistrare) " +
                "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                textBox3.Text, textBox4.Text, textBox1.Text, textBox2.Text, sex, Convert.ToString(dateTimePicker1.Value), Convert.ToString(DateTime.Now)
                );
            cmd.ExecuteNonQuery();

            con.Close();

            return true;
        }

        private void Button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;
        }

        private void Button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackgroundImage = InfoMobilul.Properties.Resources.blue_yellow;

        }

        private void Button2_MouseEnter(object sender, EventArgs e)
        {
            button2.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;

        }

        private void Button2_MouseLeave(object sender, EventArgs e)
        {
            button2.BackgroundImage = InfoMobilul.Properties.Resources.blue_yellow;

        }
    }
}
