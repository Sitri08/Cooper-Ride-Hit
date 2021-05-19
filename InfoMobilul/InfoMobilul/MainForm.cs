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
    public partial class MainForm : Form
    {
        string[] liniiDecriptate = new string[0];
        int IdCurent;
        public MainForm()
        {
            InitializeComponent();
            IncarcaUtilizator();
            Statistici();
        }

        private void IncarcaUtilizator()
        {
            
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT Prenume, Id, Sex FROM Utilizatori WHERE Id = (SELECT IdUtilizator FROM Logari WHERE Id = (SELECT MAX(Id) FROM Logari))"
                );
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            label1.Text = "Bine ai venit, " + Convert.ToString(reader[0]);
            IdCurent = Convert.ToInt32(reader[1]);
            if (Convert.ToString(reader[2]) == "F") Feminizeaza();
            reader.Close();

            //MessageBox.Show("1");

            con.Close();
        }

        private void Feminizeaza()
        {
            label1.ForeColor = Color.LightGreen;
            label2.ForeColor = Color.LightGreen;
            label3.ForeColor = Color.LightGreen;
            chart1.Palette = chart2.Palette = chart3.Palette = chart4.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.BrightPastel;
            chart5.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Statistici();
        }

        private void Statistici()
        {
            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart3.Series[0].Points.Clear();
            chart4.Series[0].Points.Clear();
            chart5.Series[0].Points.Clear();

            Decripteaza();
            IncarcaTabelScoruri();
            IncarcaGrafice();
        }

        private void Decripteaza()
        {
            //0123456789012345678901234567890123
            //RRRZZzzRRRRLLllRRRRAAaaAAaaRRRRS00
            //Downloads\Mini.txt

            //extrage text, imparte text in linii (inregistrari)
            string filePath = Application.ExecutablePath;
            filePath = filePath.Remove(filePath.Length - 26) + @"\Resources\Mini.txt";
            string text = System.IO.File.ReadAllText(filePath);
            if (text == "") return;
            string[] linii = ImparteLinii(text);

            //decodeaza inregistrari
            foreach (string linie in linii)
            {
                Array.Resize(ref liniiDecriptate, liniiDecriptate.Length + 1);
                liniiDecriptate[liniiDecriptate.Length - 1] = DecripteazaLinie(linie);
            }
        }

        private string[] ImparteLinii(string x)
        {
            string[] linii = { "" };
            string y = "";
            int i = 0;

            for (bool ok = true; i < x.Length && ok; i++)
            {
                y += x[i];
                if (x[i] == '0' && x[i + 1] == '0') //sfarsitul primei inregistrari
                {
                    y += x[i];
                    i++;
                    linii[0] = y;
                    y = "";
                    ok = false;
                }
            }

            for (; i < x.Length; i++)
            {
                y += x[i];
                if (x[i] == '0' && x[i + 1] == '0') //sfarsitul unei inregistrari
                {
                    y += x[i];
                    i++;
                    Array.Resize(ref linii, linii.Length + 1);
                    linii[linii.Length - 1] = y;
                    y = "";
                }
            }

            return linii;
        }

        private string DecripteazaLinie(string x)
        {
            //ZLAAS
            //01234567890123456789012345678901234
            //RRRZZzzRRRRLLllRRRRAAaaAAaaRRRRSS00
            //2726048884261587750426211475582600
            int[] indices = { 3, 5, 11, 13, 19, 21, 23, 25 }; //31
            string y = "";

            //decripteaza data
            foreach (int index in indices)
            {
                y += CautaCifra("" + x[index] + x[index + 1]);
            }
            //decripteaza scor
            for (int i = 31, ok = 1; ok != 0; i += 2)
            {
                if (x[i] == '0' && x[i + 1] == '0' && x[i-1] != '2') ok = 0;
                else
                {
                    y += CautaCifra("" + x[i] + x[i + 1]);
                }
            }

            return y;
        }

        private string CautaCifra(string x)
        {
            switch (x)
            {
                case "26":
                    return "0";
                case "21":
                    return "1";
                case "04": //04
                    return "2";
                case "69": //20 > 69
                    return "3";
                case "16":
                    return "4";
                case "03": //03
                    return "5";
                case "19":
                    return "6";
                case "08": //08
                    return "7";
                case "15":
                    return "8";
                case "14":
                    return "9";
                case "12":
                    return "-";
            }

            return "0";
        }

        private void IncarcaTabelScoruri()
        {
            foreach(var linie in liniiDecriptate)
            {
                int zile, luni, ani;
                zile = Convert.ToInt32("" + linie[0] + linie[1]);
                luni = Convert.ToInt32("" + linie[2] + linie[3]);
                ani = Convert.ToInt32("" + linie[4] + linie[5] + linie[6] + linie[7]);
                double scor;
                scor = Convert.ToDouble(linie.Remove(0, 8)) / 100;
                DateTime data = new DateTime(ani, luni, zile); //Convert.ToDateTime("" + zile + "-" + luni + "-" + ani);

                if (!ExistaInregistrare(scor, data))
                    Inregistreaza(scor, data);
            }
        }

        private bool ExistaInregistrare(double scor, DateTime data)
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT Data FROM Scoruri WHERE Scor = '{0}'",
                scor
                );

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                if (Convert.ToDateTime(reader[0]).Day.Equals(data.Day) &&
                    Convert.ToDateTime(reader[0]).Month.Equals(data.Month) &&
                    Convert.ToDateTime(reader[0]).Year.Equals(data.Year))
                    return true;
            reader.Close();

            con.Close();

            return false;
        }

        private void Inregistreaza(double scor, DateTime data)
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT IdUtilizator FROM Logari WHERE DataLogare = (SELECT MAX(DataLogare) FROM Logari)"
                );
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int IdUtilizator = Convert.ToInt32(reader[0]);
            reader.Close();

            cmd.CommandText = string.Format(
                "INSERT INTO Scoruri(IdUtilizator, Scor, Data) " +
                "VALUES ('{0}', '{1}', '{2}')",
                IdUtilizator, scor, data
                );
            cmd.ExecuteNonQuery();

            con.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            SettingsForm formSet = new SettingsForm();
            formSet.Show();
            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void IncarcaGrafice()
        {
            IncarcaGraficScoruriIndividuale();
            IncarcaGraficActivitateGlobala();
            IncarcaScorMaxim();
            IncarcaGraficInregistrari();
            IncarcaGraficVarste();
            IncarcaGraficSexe();
        }

        private void IncarcaGraficScoruriIndividuale()
        {
            
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT * FROM Scoruri WHERE IdUtilizator = {0} ORDER BY Data",
                IdCurent
                );

            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                double scor = Convert.ToDouble(reader[2]);
                DateTime data = Convert.ToDateTime(reader[3]);
                chart1.Series[0].Points.AddXY(Convert.ToString(data).Remove(10), scor);
            }

            con.Close();
        }

        private void IncarcaGraficActivitateGlobala()
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT COUNT (Id), DataLogare FROM Logari GROUP BY DataLogare ORDER BY DataLogare"
                );

            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                int numar = Convert.ToInt32(reader[0]);
                string data = Convert.ToString(reader[1]);
                data = data.Remove(10);
                //MessageBox.Show(data + " " + numar);
                chart2.Series[0].Points.AddXY(data, numar);
            }


            con.Close();
        }

        private void IncarcaScorMaxim()
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT MAX(Scor) FROM Scoruri"
                );
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            double scorMax = Convert.ToDouble(reader[0]);
            reader.Close();

            cmd.CommandText = string.Format(
                "SELECT Prenume, Nume FROM Utilizatori WHERE Id IN (SELECT IdUtilizator FROM Scoruri WHERE Scor = '{0}')",
                scorMax
                );

            reader = cmd.ExecuteReader();
            reader.Read();
            string prenume = Convert.ToString(reader[0]);
            string nume = Convert.ToString(reader[1]);
            reader.Close();

            label2.Text = "Scor maxim: " + scorMax + "\n(" + prenume + ' ' + nume + ")";

            cmd.CommandText = string.Format(
                "SELECT MAX(Scor) FROM Scoruri WHERE IdUtilizator = {0}",
                IdCurent
                );

            reader = cmd.ExecuteReader();
            reader.Read();
            label2.Text += "\n\n" + "Scorul meu maxim: " + Convert.ToString(reader[0]);
            reader.Close();

            con.Close();
        }

        private void IncarcaGraficInregistrari()
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT COUNT(Id), DataInregistrare FROM Utilizatori GROUP BY DataInregistrare"
                );

            int[] luni = new int[13];

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int NumarId = Convert.ToInt32(reader[0]);
                DateTime data = Convert.ToDateTime(reader[1]);
                luni[data.Month] += NumarId;


                //chart3.Series[0].Points.AddXY(Convert.ToString(data).Remove(10), NumarId);
            }

            string[] numeLuni = { "", "Jan-19", "Feb-19", "Mar-19", "Apr-19", "May-19", "Jun-19", "Jul-19", "Aug-19", "Sep-18", "Oct-18", "Nov-18", "Dec-18" };

            for (int i = 9; i <= 12; i++) if (luni[i] != 0) chart3.Series[0].Points.AddXY(numeLuni[i], luni[i]);
            for (int i = 1; i <= 8; i++) if (luni[i] != 0) chart3.Series[0].Points.AddXY(numeLuni[i], luni[i]);

            con.Close();
        }

        private void IncarcaGraficVarste()
        {
            chart4.Series[0].Points.AddXY(1, 0);

            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT DataNasterii FROM Utilizatori"
                );

            int[] varste = new int[100];
            for (int i = 0; i < 100; i++) varste[i] = 0;
            int maxx = 0;

            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                DateTime data = Convert.ToDateTime(reader[0]);
                int ani = (Convert.ToInt32(DateTime.Now.Subtract(data).TotalDays)) / 365;
                varste[ani]++;
                if (ani > maxx) maxx = ani;
            }
            reader.Close();

            for (int i = 0; i <= maxx; i++) if (varste[i] != 0) chart4.Series[0].Points.AddXY(i, varste[i]);

            cmd.CommandText = "SELECT CAST(AVG(CAST(DataNasterii AS INT)) AS DATETIME) FROM Utilizatori";
            reader = cmd.ExecuteReader();
            reader.Read();
            double varstaMedie = Math.Floor(DateTime.Now.Subtract(Convert.ToDateTime(reader[0])).TotalDays / 365 * 100) / 100;
            reader.Close();

            label4.Text = "Varsta medie: " + varstaMedie;

            con.Close();
        }

        public void IncarcaGraficSexe()
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT COUNT(Id), Sex FROM Utilizatori GROUP BY Sex"
                );

            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                chart5.Series[0].Points.AddXY(Convert.ToString(reader[1]), Convert.ToInt32(reader[0]));
            }

            con.Close();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            string caleJoc = Application.ExecutablePath;
            caleJoc = caleJoc.Remove(caleJoc.Length - 26) + @"\Resources\InfoMobilul.exe";
            System.Diagnostics.Process.Start(caleJoc);
            //Button1_Click(button1, new EventArgs());
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

        private void Button3_MouseEnter(object sender, EventArgs e)
        {
            button3.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;

        }

        private void Button3_MouseLeave(object sender, EventArgs e)
        {
            button3.BackgroundImage = InfoMobilul.Properties.Resources.blue_yellow;

        }

        private void Button4_MouseEnter(object sender, EventArgs e)
        {
            button4.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;

        }

        private void Button4_MouseLeave(object sender, EventArgs e)
        {
            button4.BackgroundImage = InfoMobilul.Properties.Resources.blue_yellow;

        }

        private void Button5_Click(object sender, EventArgs e)
        {
            string conStr = Application.ExecutablePath;
            conStr = conStr.Remove(conStr.Length - 25) + "InfoMobilul.mdf";
            conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + conStr + @";Integrated Security=True";

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = String.Format(
                "SELECT AVG(Scor) FROM Scoruri WHERE IdUtilizator IN (SELECT Id FROM Utilizatori WHERE Sex = 'M')"
                );
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            double scorMediuM = Convert.ToDouble(reader[0]);
            reader.Close();

            cmd.CommandText = String.Format(
               "SELECT AVG(Scor) FROM Scoruri WHERE IdUtilizator IN (SELECT Id FROM Utilizatori WHERE Sex = 'F')"
               );
            reader = cmd.ExecuteReader();
            reader.Read();
            double scorMediuF = Convert.ToDouble(reader[0]);
            reader.Close();

            cmd.CommandText = String.Format(
               "SELECT AVG(Scor) FROM Scoruri"
               );
            reader = cmd.ExecuteReader();
            reader.Read();
            double scorMediu = Convert.ToDouble(reader[0]);
            reader.Close();

            scorMediu = Math.Floor(scorMediu * 100) / 100;
            scorMediuF = Math.Floor(scorMediuF * 100) / 100;
            scorMediuM = Math.Floor(scorMediuM * 100) / 100;

            label3.Text = "Medie barbati: " + scorMediuM + "\nMedie femei: " + scorMediuF + "\nMedie: " + scorMediu;

            con.Close();
        }

        private void Button5_MouseEnter(object sender, EventArgs e)
        {
            button5.BackgroundImage = InfoMobilul.Properties.Resources.red_yellow;
        }

        private void Button5_ContextMenuStripChanged(object sender, EventArgs e)
        {
            button5.BackgroundImage = InfoMobilul.Properties.Resources.blue_yellow;

        }

        private void Button5_MouseLeave(object sender, EventArgs e)
        {
            button5.BackgroundImage = InfoMobilul.Properties.Resources.blue_yellow;
        }
    }
}
