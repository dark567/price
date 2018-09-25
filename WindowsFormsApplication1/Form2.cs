﻿using FirebirdSql.Data.FirebirdClient;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        FbConnection fb; //fb ссылается на соединение с нашей базой данных, по-этому она должна быть доступна всем методам нашего класса
        public string path_db;
        bool status = false;

        public Form2()
        {
            InitializeComponent();
        }

        private void label8_Click(object sender, EventArgs e)
        {
            SaveFileDialog OPF = new SaveFileDialog();
            OPF.Filter = "Файлы csv|*.csv";

            if (OPF.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = OPF.FileName;
                status = true;
            }
            else
                status = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // формируем connection string для последующего соединения с нашей базой данных
            FbConnectionStringBuilder fb_con = new FbConnectionStringBuilder();
            fb_con.Charset = "UTF8"; //используемая кодировка
            fb_con.UserID = "SYSDBA"; //логин
            fb_con.Password = "masterkey"; //пароль
            fb_con.Database = db_puth.Value; //путь к файлу базы данных
            fb_con.ServerType = 0; //указываем тип сервера (0 - "полноценный Firebird" (classic или super server), 1 - встроенный (embedded))
            fb = new FbConnection(fb_con.ToString()); //передаем нашу строку подключения объекту класса FbConnection

            #region example how fill
            // RepModel r = new RepModel();
            //MessageBox.Show(r.Select(fb).Name);

            //ORGTabel(fb);
            //if (!string.IsNullOrEmpty(textBox2.Text))
            //{
            //    using (var w = new StreamWriter(textBox2.Text))
            //    {
            //        foreach (DataRow dataRow in ORGTabel(fb).AsEnumerable().ToList())
            //        {
            //            var first = dataRow[0].ToString(); // 
            //            var second = dataRow[1].ToString(); //
            //            var third = dataRow[2].ToString(); //
            //            var line = string.Format($"{first};{second};{third}");
            //            w.WriteLine(line);
            //            w.Flush();
            //        }
            //    }
            //}
            //else
            //{
            //    label8_Click(sender, e);
            //    using (var w = new StreamWriter(textBox2.Text))
            //    {
            //        foreach (DataRow dataRow in ORGTabel(fb).AsEnumerable().ToList())
            //        {
            //            var first = dataRow[0].ToString(); // 
            //            var second = dataRow[1].ToString(); //
            //            var third = dataRow[2].ToString(); //
            //            var line = string.Format($"{first};{second};{third}");
            //            w.WriteLine(line);
            //            w.Flush();
            //        }
            //    }
            //}
            #endregion

            try
            {
                PriceTabel(fb, null, null, null, null);

                if (!string.IsNullOrEmpty(textBox2.Text))
                    Fillcsv();
                
                else
                {
                    label8_Click(sender, e);
                    if (!string.IsNullOrEmpty(textBox2.Text))
                        Fillcsv();
                }
                if (status)
                    MessageBox.Show("выгружено");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Выгрузка неудачно {ex.Message}");
            }
        }

        private void Fillcsv()
        {
            using (var w = new StreamWriter(textBox2.Text))
            {
                int ColumnsCount = PriceTabel(fb, null, null, null, null).Columns.Count;
                string line = "";

                foreach (DataColumn column in PriceTabel(fb, null, null, null, null).Columns)
                {
                    w.Write($"{column.ColumnName};");
                }
                w.Write("\n");

                foreach (DataRow dataRow in PriceTabel(fb, null, null, null, null).AsEnumerable().ToList())
                {
                    line = "";
                    for (int i = 0; i < ColumnsCount; i++)
                    {
                        string first = dataRow[i].ToString().Replace(@",", ".") + ";";  
                        line = line + first;
                    }
                     w.WriteLine(line);
                    
                    w.Flush();
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                //Создание объекта, для работы с файлом
                INIManager manager = new INIManager(Application.StartupPath + "\\set.ini");
                //Получить значение по ключу name из секции main
                path_db = manager.GetPrivateString("connection", "db");
                db_puth.Value = path_db;

                File.AppendAllText(Application.StartupPath + @"\Event.log", "путь к db:" + db_puth.Value + "\n");
                //Записать значение по ключу age в секции main
                // manager.WritePrivateString("main", "age", "21");

                OnUserNameMessage(path_db);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ini не прочтен" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnUserNameMessage(string path_db)
        {
            if (string.IsNullOrEmpty(path_db))
                this.Text = "Экспорт Прайсов";
            else
                this.Text = "Экспорт Прайсов - (" + path_db + ")";
        }

        public DataTable ORGTabel(FbConnection conex)
        {
            string query = "SELECT id, Name, EGRPOU FROM Dic_org";
            FbCommand comando = new FbCommand(query, conex);
            try
            {
                conex.Open();
                FbDataAdapter datareader = new FbDataAdapter(comando);
                DataTable usuarios = new DataTable();
                datareader.Fill(usuarios);
                return usuarios;
            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                conex.Close();
            }
        }

        public DataTable PriceTabel(FbConnection conn, string GRP_ID, string FILTER_, string REFRESH_ID, string IN_ORG_ID)
        {
            if (string.IsNullOrEmpty(GRP_ID)) { GRP_ID = "null"; }
            if (string.IsNullOrEmpty(FILTER_)) { FILTER_ = "null"; }
            if (string.IsNullOrEmpty(REFRESH_ID)) { REFRESH_ID = "null"; }
            if (string.IsNullOrEmpty(IN_ORG_ID)) { IN_ORG_ID = "null"; }

            //string query = $"SELECT ID, CODE, NAME, UNIT FROM Z$DIC_PRICE_S({GRP_ID}, {FILTER_}, {REFRESH_ID}, {IN_ORG_ID})";
            string query = $"select GOOD_ID, ORG_ID, TYPE_BONUS_ID, TYPE_PRICE_ID, PRICE_OUT_DISC from DIC_PRICE_LIST order by GOOD_ID";

              // MessageBox.Show($"{query}");
            FbCommand cmd = new FbCommand(query, conn);

            try
            {
                conn.Open();
                FbDataAdapter datareader = new FbDataAdapter(cmd);
                DataTable usuarios = new DataTable();

                datareader.Fill(usuarios);
                return usuarios;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void label7_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }
    }
}
