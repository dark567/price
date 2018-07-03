using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.Common;
using System.IO;
using System.Windows.Input;
using FirebirdSql.Data.FirebirdClient;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using System.Collections;
using DevExpress.XtraPrinting;
using System.Globalization;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;


//using lcpi.data.oledb;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        FbConnection fb; //fb ссылается на соединение с нашей базой данных, по-этому она должна быть доступна всем методам нашего класса
        public string path_db;
        ToolStripLabel infolabel;
        ToolStripLabel datelabel;
        ToolStripLabel timelabel;
        ToolStripLabel typelabel;
        Timer timer;
        string[] prioritet; //приоритетность столбцоы


        //Создаем схему для работы с конфигом
        static MyConfig cfg = new MyConfig();
        //имя файла с настройками 
        static public string cfgName = "Config.xml";
        //имя таблицы с которой будем работать
        static string TableConfigName = "ConfigTable";

        public Form1()
        {
            InitializeComponent();
            SetMyCustomFormat();
            StatusLable();

        }

        public int k = 0;

        #region //работа с xml
        //функция загрузки настроек 
        static void LoadConfig()
        {
            //проверяем если файла не существует создаем его с настройкой по умолчанию 
            if (File.Exists(cfgName) == false)
                //создаем пустышку  item Элемент "item" не существует в текущем контексте. 

                cfg.WriteXml(cfgName);
            //чистим буфер если в нем что то есть 
            cfg.Clear();
            //если файл сущствует загружаем его для чтения
            cfg.ReadXml(cfgName);
        }

        //функция чтения настроек из конфига 
        static public string GetParam(string ParamName)
        {
            //загружаем настройки 
            LoadConfig();

            //создаем условия отбора параметров 
            string Filtr = "Code = '" + ParamName + "'";

            //выбираем из таблицы все параметры 
            DataRow[] FindParam = cfg.Tables[TableConfigName].Select(Filtr);

            //перебираем результат и записываем в параметр 1 значение 
            foreach (DataRow item in FindParam)
            {
                //если параметр есть возвращаем его 
                return item["Code_Terra"].ToString();

            }

            //возвращаем пустой  результат  если такого параметра нет 
            return "";

        }

        static public string GetParam_up(string ParamName)
        {
            //загружаем настройки 
            LoadConfig();

            //создаем условия отбора параметров 
            string Filtr = "Code_Terra = '" + ParamName + "'";

            //выбираем из таблицы все параметры 
            DataRow[] FindParam = cfg.Tables[TableConfigName].Select(Filtr);

            //перебираем результат и записываем в параметр 1 значение 
            foreach (DataRow item in FindParam)
            {
                //если параметр есть возвращаем его 
                return item["Code"].ToString();

            }

            //возвращаем пустой  результат  если такого параметра нет 
            return "";

        }

        //функция записи настроек в конфиг 
        static public void SetParam(string ParamName, string ParamValue, bool Append)
        {
            //загружаем настройки 
            LoadConfig();
            //создаем условия отбора параметров 
            string Filtr = "Code = '" + ParamName + "'";

            //выбираем из таблицы все параметры 
            DataRow[] FindParam = cfg.Tables[TableConfigName].Select(Filtr);

            //перебираем результат и записываем
            foreach (DataRow item in FindParam)
            {
                //записываем значение
                item["Code_Terra"] = ParamValue;
                //завершаем цикл 
                break;
            }

            //если пользователь хочет дописывать параметр в файл 
            if (Append)
            {
                //если мы его не нашли значит результат фильтра будет пустым
                if (FindParam.Length == 0)
                {
                    //обьявим строку 
                    DataRow NewParam;

                    //создаем новую строку в таблице с настройками 
                    NewParam = cfg.Tables[TableConfigName].NewRow();

                    //добавляем в нее параметры с указанием имен столбцов
                    NewParam["Code"] = ParamName;
                    NewParam["Code_Terra"] = ParamValue;

                    //добавляем данные в таблицу
                    cfg.Tables[TableConfigName].Rows.Add(NewParam);
                }
            }

            //сохраняем настройки в файл 
            cfg.WriteXml(cfgName);
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            k = 0;
            button2_Click(sender, e); 

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

        private void button1_Click(object sender, EventArgs e)
        {
            //string connectstr = @"User=SYSDBA;Password=masterkey;Database=localhost:ldb;
            //       Port=3053;Dialect=3; Charset=NONE;Role=;Connection lifetime=15;Pooling=true;
            //       MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            // формируем connection string для последующего соединения с нашей базой данных
            FbConnectionStringBuilder fb_con = new FbConnectionStringBuilder();
            fb_con.Charset = "WIN1251"; //используемая кодировка
            fb_con.UserID = "SYSDBA"; //логин
            fb_con.Password = "masterkey"; //пароль
            fb_con.Database = db_puth.Value; //путь к файлу базы данных
            fb_con.ServerType = 0; //указываем тип сервера (0 - "полноценный Firebird" (classic или super server), 1 - встроенный (embedded))
            fb = new FbConnection(fb_con.ToString()); //передаем нашу строку подключения объекту класса FbConnection

            Properties.Settings s = new Properties.Settings();

            // FbConnection conn = new FbConnection(connectstr);
         
            //FbConnection conn = new FbConnection(s.ConnectionString);

            try
            {
                //создаем подключение
                //conn.Open(); //убрал пока используем ini
                fb.Open(); //открываем БД

                // MessageBox.Show("Работает");

                MessageBox.Show(string.Format("Вы выбрали период с {0} до {1}", dateTimePicker1.Value.ToLongDateString(), dateTimePicker2.Value.ToLongDateString()), "Информация");
                File.AppendAllText(Application.StartupPath + @"\Event.log",string.Format("Вы выбрали период с {0} до {1}", dateTimePicker1.Value.ToLongDateString(), dateTimePicker2.Value.ToLongDateString()) + "\n");


                FbCommand fbcommand = fb.CreateCommand();
                fbcommand.CommandType = CommandType.Text;
                fbcommand.Connection = fb;

                                       // fbcommand.CommandText = "select /*S.ID,*/"+
                                       // "S.ID as CODE,"+
                                       // "S.DATE_TIME,"+
                                       // "(select max(CA1.DATE_TIME) " +
                                       // "from JOR_CASH CA1 " +
                                       // "where CA1.CHECK_ID = S.ID and " +
                                       // "CA1.SUM_ > 0) as PAYED_DATE," +
                                       // // "S.DATE_TO, "+
                                       // "S.NUM," +
                                       // //"S.SUBDIVISION_ID, "+
                                       // "DS.NAME as SUB,"+
                                       //// "S.WORK_PLACE_ID, W.NAME as WORKPLACE, S.CLIENT_ID, "+
                                       // "C.CODE_NAME as CLIENT,"+
                                       // "case when C.BIRTH_DATE is null then(select(extract(year from current_date) - C.BIRTH_YEAR) " +
                                       // "from RDB$DATABASE) " +
                                       // "else (select R_YEAR " +
                                       // "from GET_DATE_DIFF(C.BIRTH_DATE, current_date)) " +
                                       // "end as AGE_ID," +
                                       //  "(select RESULT " +
                                       // "from translate('' || C.SEX)) as SEX_ID, " +
                                       // //"S.MANAGER_ID, "+
                                       // "M.CODE_NAME as MANAGER," +
                                       //// "S.EMPLOYEE_ID, E.CODE_NAME as EMPLOYEE, S.AGENT_ID, A.CODE_NAME as AGENT, S.PAYER_ORG_ID, "+
                                       // "O.CODE_NAME as ORG,"+
                                       // //"S.DESCR, S.COLOR_ID," +
                                       // //"S.AGREEMENT_DOC," +
                                       // "S.SUM_BASE," +
                                       // "S.SUM_," +
                                       // "(select sum(CA.SUM_) " +
                                       // "from JOR_CASH CA " +
                                       // "where CA.CHECK_ID = S.ID and " +
                                       // "CA.SUM_ > 0) as PAYED_SUM," +
                                       // //"S.DIAGNOSIS_ID, DIA.NAME as DIAGNOSIS, S.MENSTRPHASE_ID," +
                                       // //"C.SEX as SEX_ID_ID, "+
                                       // //"S.PREG_WEEK_FROM_ID, S.PREG_WEEK_TO_ID, S.DOC_EMPLOYEE_ID, "+
                                       // //"DOC.CODE_NAME as DOC_EMPLOYEE," +
                                       // //"S.LAST_MENSTR_DAY_ID, S.CYCLE_LENGTH_ID, S.DISCONT_DESCR, "+
                                       // //"ADDD.CODE_NAME as ADD_EMPLOYEE," +
                                       // //"(select GCC.COLOR " +
                                       //  //"from GET_COLOR_FOR_CHECK(S.ID) GCC) as COLOR, "+
                                       // "S.IS_DONE," +
                                       // "S.IS_FISCAL " +
                                       // //"S.FISCAL_PRINT_TIME " +
                                       // //"(select ESS.COLOR_EMAIL_SEND " +
                                       // //"from GET_EMAIL_SEND_STATUS(S.ID) ESS) as COLOR_EMAIL_SEND " +
                                       // "from JOR_CHECKS S " +
                                       // "left join DIC_SUBDIVISIONS DS on DS.ID = S.SUBDIVISION_ID " +
                                       // "left join DIC_WORK_PLACES W on W.ID = S.WORK_PLACE_ID " +
                                       // "left join DIC_CLIENTS C on C.ID = S.CLIENT_ID " +
                                       // "left join DIC_EMPLOYEE M on M.ID = S.MANAGER_ID " +
                                       // "left join DIC_EMPLOYEE E on E.ID = S.EMPLOYEE_ID " +
                                       // "left join DIC_EMPLOYEE ADDD on ADDD.ID = S.ADD_EMPLOYEE_ID " +
                                       // "left join DIC_EMPLOYEE DOC on DOC.ID = S.DOC_EMPLOYEE_ID " +
                                       // "left join DIC_CLIENTS A on A.ID = S.AGENT_ID " +
                                       // "left join DIC_ORG O on O.ID = S.PAYER_ORG_ID " +
                                       // "left join DIC_DIAGNOSIS DIA on DIA.ID = S.DIAGNOSIS_ID " +
                                       // "where S.DATE_TIME >= cast('" + dateTimePicker1.Value.ToString("dd.MM.yyyy") + "' as date) and " +
                                       // "      S.DATE_TIME < cast('" + dateTimePicker2.Value.ToString("dd.MM.yyyy") + "' as date) + 1 " +
                                       // "order by DATE_TIME";


                fbcommand.CommandText = "select C.DATE_TIME, C.NUM, C.CLIENT_ID, DC.SURNAME, DC.NAME, DC.SECNAME, DC.BIRTH_DATE, "+
                                         "case when DC.BIRTH_DATE is null then(select(extract(year from current_date) - DC.BIRTH_YEAR) " +
                                         "from RDB$DATABASE) " +
                                         "else (select R_YEAR " +
                                         "from GET_DATE_DIFF(DC.BIRTH_DATE, current_date)) " +
                                         "end as AGE_ID, (select min(date_time) from JOR_CHECKS CDC where CDC.client_id = C.CLIENT_ID) as FIRSTVISIT," +
                                         "DC.SEX, DC.CELLPHONE, DC.EMAIL," +
                                        "C.SUBDIVISION_ID, DS.NAME, C.MANAGER_ID, DE.CODE_NAME as MANAGER, C.EMPLOYEE_ID, EM.CODE_NAME, C.WORK_PLACE_ID," +
                                        "WP.NAME, C.AGENT_ID, A.CODE_NAME as AGENT, C.PAYER_ORG_ID, PO.CODE_NAME, PO.EGRPOU, D.GOODS_ID,"+
                                        "case " +
                                        "when(exists(select first 1 1 " +
                                        "from JOR_CHECKS_DT CDT " +
                                        "where CDT.ID = D.COMPLEX_ID)) then(select first 1 CDT1.GOODS_ID " +
                                        "from JOR_CHECKS_DT CDT1 " +
                                        "where CDT1.ID = D.COMPLEX_ID) " +
                                        "else null " +
                                        "end as COMPLEX_ID, " +
                                        "case " +
                                        "when(exists(select first 1 1 " +
                                        "from JOR_CHECKS_DT CDT " +
                                        "where CDT.ID = D.COMPLEX_ID)) then(select first 1 dcg.name " +
                                        "from JOR_CHECKS_DT CDT1 " +
                                        "join dic_goods dcg on dcg.id = CDT1.GOODS_ID " +
                                        "where CDT1.ID = D.COMPLEX_ID) " +
                                        "else null " +
                                        "end as COMPLEX_NAME," +
                                        "case " +
                                        "when(exists(select first 1 1 from dic_goods dg " +
                                        "join DIC_GOODS_GRP G on G.ID = dg.GRP_ID " +
                                        "join DIC_CALCULATIONS dc on dc.hd_id = dg.id " +
                                        "join dic_goods dg1 on dg1.id = dc.goods_id " +
                                        "join DIC_GOODS_GRP G1 on G1.ID = dg1.GRP_ID " +
                                        "left join DIC_GOODS_LAB_NORMS dgln on dgln.goods_id = dg.id " +
                                        "where dgln.id is null and dg.id = D.GOODS_ID)) then 1 else 0 end as COMPLEX_PR, " +
                                        "G.NAME as GOODS_NAME, " +
                                        "G.PRICE_OUT as PRICE_OUT, " +
                                        //"(select first 1 PRICE_OUT_DISC " +
                                        //"from dic_goods dcd " +
                                        //"join DIC_PRICE_LIST DPL on DPL.GOOD_ID = dcd.id " +
                                        //"where dcd.ID = D.GOODS_ID), " + 
                                        "G.CODE, D.UNIT_ID, U.NAME, D.CNT," +
                                        "D.PRICE_BASE," +
                                        "D.PRICE," +
                                        "D.SUM_BASE," +
                                        "D.SUM_," +
                                        "D.IS_REFUSE, CS.SUM_, C.IS_FISCAL, CS.IS_FISCAL, CS.NUM," +
                                        "case " +
                                        "when(exists(select first 1 1 " +
                                        "from JOR_CHECKS_DT CDT " +
                                        "where CDT.ID = D.COMPLEX_ID)) then(select first 1 CDT1.GOODS_ID " +
                                        "from JOR_CHECKS_DT CDT1 " +
                                        "where CDT1.ID = D.COMPLEX_ID) " +
                                        "else null " +
                                        "end, " +
                                        "AD.ID, ADC.ID, cast(ADC.VAL as FULL_NAME), D.ORG_EXEC_ID, EO.NAME as ORG_EXEC_NAME, C.disconts_card " +
                                        "from JOR_CHECKS C " +
                                        "inner join JOR_CHECKS_DT D on D.HD_ID = C.ID " +
                                        "inner join DIC_CLIENTS DC on DC.ID = C.CLIENT_ID " +
                                        "inner join DIC_SUBDIVISIONS DS on DS.ID = C.SUBDIVISION_ID " +
                                        "inner join DIC_EMPLOYEE DE on DE.ID = C.MANAGER_ID " +
                                        "left join DIC_EMPLOYEE EM on EM.ID = C.EMPLOYEE_ID " +
                                        "left join DIC_WORK_PLACES WP on WP.ID = C.WORK_PLACE_ID " +
                                        "left join DIC_CLIENTS A on A.ID = C.AGENT_ID " +
                                        "left join DIC_ORG PO on PO.ID = C.PAYER_ORG_ID " +
                                        "left join DIC_ORG EO on EO.ID = D.ORG_EXEC_ID " +
                                        "inner join DIC_GOODS G on G.ID = D.GOODS_ID " +
                                        "left join DIC_UNITS U on U.ID = D.UNIT_ID " +
                                        "left join JOR_CASH CS on CS.CHECK_ID = C.ID " +
                                        "left join CALC_COMPLEX_DT_SUM(D.ID) CDS on 1 = 1 " +
                                        "left join CALC_COMPLEX_DT_BASE_SUM(D.ID) CDBS on 1 = 1 " +
                                        "left join DIC_ADDRESS AD on AD.ID = DS.ADDRESS_ID " +
                                        "left join DIC_DICS ADC on ADC.ID = AD.CITY_ID " +
                                        "where C.DATE_TIME >= cast('" + dateTimePicker1.Value.ToString("dd.MM.yyyy") + "' as date) and " +
                                        "C.DATE_TIME < cast('" + dateTimePicker2.Value.ToString("dd.MM.yyyy") + "' as date) + 1 ";

                //  MessageBox.Show(fbcommand.CommandText);
                FbDataAdapter FBAdapter = new FbDataAdapter(fbcommand.CommandText, fbcommand.Connection);
                DataSet fbds = new DataSet("first_tab_DS");
                FBAdapter.Fill(fbds, "dic_clients");

                int count = fbds.Tables[0].Rows.Count; /*количество записей*/

                FbDatabaseInfo fb_inf = new FbDatabaseInfo(fb); //информация о БД
                                                                //пока у объекта БД не был вызван метод Open() - никакой информации о БД не получить, будет только ошибка
                //MessageBox.Show("connect Info: " + fb_inf.ServerClass + "; " + fb_inf.ServerVersion); //выводим тип и версию используемого сервера Firebird

                typelabel.Text = "connect Info: " + fb_inf.ServerClass + "; " + fb_inf.ServerVersion + "; Rows: " + count;
                statusStrip1.Items.Add(typelabel);

                //Form1.dataGridView1.DataSource = fbds.Tables["dic_clients"].DefaultView;

                gridControl1.DataSource = fbds.Tables["dic_clients"].DefaultView;

                int check = 1;

                foreach (ToolStripMenuItem mainItem in menuStrip1.Items)
                {
                    if (mainItem.Text == "Вид")
                    {
                        check = 0;
                       // MessageBox.Show(mainItem.Text);
                    }
                }

                if (check == 1)
                {
                    //добавить вид...
                    ToolStripMenuItem newMenuItem = new ToolStripMenuItem("Вид");

                    //ToolStripMenuItem newItem = new ToolStripMenuItem("Создать") { Checked = true, CheckOnClick = true };
                    //newMenuItem.DropDownItems.Add(newItem);
                    //newItem.CheckedChanged += saveItem_Click;
                    //ToolStripMenuItem saveItem = new ToolStripMenuItem("Сохранить") { Checked = true, CheckOnClick = true };
                    //saveItem.CheckedChanged += saveItem_Click;
                    //newMenuItem.DropDownItems.Add(saveItem);

                    prioritet = new string[fbds.Tables[0].Columns.Count];

                    for (int i = 0; i < fbds.Tables[0].Columns.Count; i++)
                        {

                        if (fbds.Tables[0].Columns[i].ColumnName != "BULB_NUM_CODE_ID")
                          {
                            //MessageBox.Show(fbds.Tables[0].Columns[i].ColumnName);
                            ToolStripMenuItem newItemasd = new ToolStripMenuItem(fbds.Tables[0].Columns[i].ColumnName) { Checked = true, CheckOnClick = true };

                            newMenuItem.DropDownItems.Add(newItemasd);
                            newItemasd.CheckedChanged += saveItem_Click;

                            prioritet[i] = fbds.Tables[0].Columns[i].ColumnName;
                           

                        }
                       }

                    ToolStripMenuItem newItem = new ToolStripMenuItem("Убрать всё") { Checked = true, CheckOnClick = true };
                    newMenuItem.DropDownItems.Add(newItem);
                    newItem.CheckedChanged += saveItem_Click;

                    menuStrip1.Items.Insert(1, newMenuItem);
                }
                //

                fb.Close();

            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\Event.log", "Не работает. Ошибка: " + ex.Message);
                MessageBox.Show("Не работает. Ошибка: " + ex.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (k == 0)
            {
                panel1.Height = 30;
                k = 1;
                gridControl1.Location = new System.Drawing.Point(3, panel1.Location.Y + panel1.Height+1);
                //gridControl1.Location = new System.Drawing.Point(3, 104);
                gridControl1.Height = this.Height - (panel1.Location.Y + panel1.Height) -48;
               // gridControl1.Height = 615;
            }
            else
            {
                panel1.Height = 95;
                k = 0;
                gridControl1.Location = new System.Drawing.Point(3, panel1.Location.Y + panel1.Height+1);
                //gridControl1.Location = new System.Drawing.Point(3, 171);
                gridControl1.Height = this.Height - (panel1.Location.Y + panel1.Height) - 48;
                //gridControl1.Height = 550;
            }
        }

        public void SetMyCustomFormat()
        {
            // Set the Format type and the CustomFormat string.
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd.MM.yyyy";

            dateTimePicker2.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.CustomFormat = "dd.MM.yyyy";
        }

        private void печатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!gridControl1.IsPrintingAvailable)
            {
                File.AppendAllText(Application.StartupPath + @"\Event.log", "Error - DevExpress dll printing не найдена");
                MessageBox.Show("DevExpress dll printing не найдена", "Error");
                return;
            }
            gridControl1.ShowPrintPreview();
        }

        private void экспортToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Get its CSV export options.

                string fileName = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Export.csv";
                DevExpress.XtraPrinting.CsvExportOptions exportMode = new DevExpress.XtraPrinting.CsvExportOptions();
                exportMode.TextExportMode = DevExpress.XtraPrinting.TextExportMode.Text;
                exportMode.Encoding = Encoding.Unicode;
                exportMode.Separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToString();

                gridControl1.ExportToCsv(fileName, exportMode);

                File.AppendAllText(Application.StartupPath + @"\Event.log", "Импорт удачно:" + fileName);
                MessageBox.Show("Импорт удачно:" + fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Импорт НЕудачно " + ex.Message);
            }
        }



        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            экспортToolStripMenuItem_Click(sender, e);
            //try
            //{
            //    gridControl1.ExportToCsv("Export.csv");
            //    File.AppendAllText(Application.StartupPath + @"\Event.log", "Импорт удачно:" + Application.StartupPath + @"\Export.csv");
            //    MessageBox.Show("Импорт удачно:" + Application.StartupPath + @"\Export.csv");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Импорт НЕудачно " + ex.Message);
            //}
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Экспорт Пока не разработан");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Экспорт НЕудачно " + ex.Message);
            }
        }

        private void OnUserNameMessage(string path_db)
        {
            if (string.IsNullOrEmpty(path_db))
                this.Text = "Экспорт Импорт";
            else
                this.Text = "Экспорт Импорт - (" + path_db + ")";
        }

        private void StatusLable()
        {
           // toolStripStatusLabel1.Text = "Текущие дата и время";

            infolabel = new ToolStripLabel();
            infolabel.Text = "Текущие дата и время:";

            datelabel = new ToolStripLabel();
            timelabel = new ToolStripLabel();

            typelabel = new ToolStripLabel();

            statusStrip1.Items.Add(infolabel);
            statusStrip1.Items.Add(datelabel);
            statusStrip1.Items.Add(timelabel);

            timer = new Timer() { Interval = 1000 };
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            datelabel.Text = DateTime.Now.ToLongDateString();
            timelabel.Text = DateTime.Now.ToLongTimeString();
        }



        private void button2_Click(object sender, EventArgs e)
        {
            if (k == 0)
            {
                panel1.Height = 30;
                k = 1;
                gridSplitContainer1.Location = new System.Drawing.Point(3, panel1.Location.Y + panel1.Height + 1);
                //gridControl1.Location = new System.Drawing.Point(3, 104);
                gridSplitContainer1.Height = this.Height - (panel1.Location.Y + panel1.Height) - 46 - 10;
                // gridControl1.Height = 615;
            }
            else
            {
                panel1.Height = 95;
                k = 0;
                gridSplitContainer1.Location = new System.Drawing.Point(3, panel1.Location.Y + panel1.Height + 1);
                //gridControl1.Location = new System.Drawing.Point(3, 171);
                gridSplitContainer1.Height = this.Height - (panel1.Location.Y + panel1.Height) - 46 - 10;
                //gridControl1.Height = 550;
            }
        }

        private void gridView1_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            MessageBox.Show("Редактирование не предусмотрено.", "Error");
            
            return;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        public void gridColumn_update(string name, int length, Color color)
        {
            GridColumn unbColumn_ = gridView1.Columns.ColumnByFieldName(name);
            unbColumn_.AppearanceCell.BackColor = color;
            unbColumn_.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
           // unbColumn_.DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
            unbColumn_.Width = length;
        }

        private void gridControl1_DataSourceChanged(object sender, EventArgs e)
        {

            gridControl1.ForceInitialize();
            #region
            // if(gridView1.Columns.ColumnByFieldName("CODE") == null)
            // { 
            // //Create an unbound column.
            // GridColumn unbColumn = gridView1.Columns.AddField("CODE");
            // unbColumn.VisibleIndex = gridView1.Columns.Count;
            // unbColumn.UnboundType = DevExpress.Data.UnboundColumnType.String;
            // // Disable editing.
            // unbColumn.OptionsColumn.AllowEdit = false;
            // // Specify format settings.
            // //unbColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            // unbColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
            // unbColumn.DisplayFormat.FormatString = "c";
            // // Customize the appearance settings.
            // unbColumn.AppearanceCell.BackColor = Color.LemonChiffon;
            // unbColumn.Width = 50;
            //}
            #endregion

            foreach (GridColumn Column in this.gridView1.Columns)
            {
                // MessageBox.Show(Column.FieldName);
                if (Column.FieldName == "DATE_TIME" || Column.FieldName == "FIRSTVISIT")
                {
                    GridColumn unbColumn_ = gridView1.Columns.ColumnByFieldName(Column.FieldName);
                    unbColumn_.AppearanceCell.BackColor = Color.LemonChiffon;
                    unbColumn_.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    unbColumn_.DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
                    unbColumn_.Width = 100;
                }
                else if (Column.FieldName == "BIRTH_DATE")
                {
                    GridColumn unbColumn_ = gridView1.Columns.ColumnByFieldName(Column.FieldName);
                    unbColumn_.AppearanceCell.BackColor = Color.LemonChiffon;
                    unbColumn_.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    unbColumn_.DisplayFormat.FormatString = "dd.MM.yyyy";
                    unbColumn_.Width = 100;
                }
                else if (Column.FieldName == "SURNAME" || Column.FieldName == "NAME" || Column.FieldName == "SECNAME" || Column.FieldName == "EMAIL" || Column.FieldName == "GOODS_NAME")
                {
                    gridColumn_update(Column.FieldName, 75, Color.Bisque);
                }
                else if (Column.FieldName == "COMPLEX_ID" || Column.FieldName == "COMPLEX_NAME" || Column.FieldName == "COMPLEX_PR") 
                {
                    gridColumn_update(Column.FieldName, 85, Color.Azure);
                }
                else
                {
                    gridColumn_update(Column.FieldName, 50, Color.LemonChiffon);
                }

                //RowStyleEventArgs k = e as RowStyleEventArgs;
               


               
                    //string SUM_ = View.GetRowCellDisplayText(e.RowHandle, View.Columns["SUM_"]);
                    ////MessageBox.Show(category);
                    //if (category == "1")
                    //{
                    //    RowStyleEventArgs k = i as RowStyleEventArgs;
                    //    e.Appearance.BackColor = Color.Crimson;
                    //    //  e.Appearance.BackColor2 = Color.SeaShell;
                    //}

                }


            //if (k.RowHandle >= 0)
            //{

            //    string category = View.GetRowCellDisplayText(k.RowHandle, View.Columns["COMPLEX_ID"]);
            //    //MessageBox.Show(category);
            //    if (category == "")
            //    {
            //        k.Appearance.BackColor = Color.Salmon;
            //        k.Appearance.BackColor2 = Color.SeaShell;
            //    }
            //}

            //if (!(control is TextBox)) //Если control не TextBox, то переходим к следующей итерации.

            //    continue;

            //if (control.Name.Substring(6) == dataColumn.ColumnName)
            //{
            //    что - нибудь делаем...
            //// }

            //GridView View = sender as GridView;
            //for (int i = 0; i < gridView1.DataRowCount; i++)
            //{
            //    if (gridView1.GetRowCellValue(i, "SUM_").ToString() != "0")
            //    {
            //        MessageBox.Show($"{i}");

            //        RowStyleEventArgs k = e as RowStyleEventArgs;
                    
            //        if (k.RowHandle >= 0)
            //        {

            //            string category = View.GetRowCellDisplayText(i, View.Columns["SUM_"]);
            //            //MessageBox.Show(category);
            //            if (category == "")
            //            {
            //                k.Appearance.BackColor = Color.Salmon;
            //                k.Appearance.BackColor2 = Color.SeaShell;
            //            }
            //            //}
            //            //  Your code here
            //        }
            //    }
            //}

            // gridView1_RowStyle(sender, e);



        }

        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    string priority = View.GetRowCellDisplayText(e.RowHandle, View.Columns["SUM_"]);
            //    if (priority != "0")
            //    {
            //        e.Appearance.BackColor = Color.FromArgb(150, Color.LightCoral);
            //        e.Appearance.BackColor2 = Color.White;
            //    }
            //}

        }

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.RowHandle == view.FocusedRowHandle) return;
            //if (e.Column.FieldName != "COMPLEX_ID" || e.Column.FieldName != "COMPLEX_NAME") return;
            if (e.Column.FieldName == "COMPLEX_ID" || e.Column.FieldName == "COMPLEX_NAME" || e.Column.FieldName == "COMPLEX_PR")
            {
                // Fill a cell's background if its value is greater than null. 
                if (Convert.ToString(e.CellValue) != "")
                    e.Appearance.BackColor = Color.FromArgb(60, Color.Salmon);
            }



            if (e.RowHandle >= 0)
            {
                GridView View = sender as GridView;

                string categoryName = View.GetRowCellDisplayText(e.RowHandle, View.Columns["COMPLEX_NAME"]);
                string category = View.GetRowCellDisplayText(e.RowHandle, View.Columns["COMPLEX_PR"]);
                string SUM_ = View.GetRowCellDisplayText(e.RowHandle, View.Columns["SUM_"]);
                //MessageBox.Show(category);
                if (category == "1")
                {
                    e.Appearance.BackColor = Color.Crimson;
                   // e.Appearance.BackColor2 = Color.SeaShell;
                }
                if (categoryName != "" && category == "0" && int.Parse(SUM_) != 0)
                {
                    e.Appearance.BackColor = Color.Blue;
                    //  e.Appearance.BackColor2 = Color.SeaShell;
                }

            }
        }

        //private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        //{
        //    GridView view = sender as GridView;
        //    if (e.Column != view.Columns["Name"])
        //        return;
        //    if (Convert.ToString(view.GetRowCellValue(e.RowHandle, view.Columns["COMPLEX_NAME"])) == "")
        //    {
        //        e.Appearance.BackColor = Color.Blue;
        //    }
        //}




        void saveItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show((sender as ToolStripMenuItem).Name);

            //gridView1.Columns[(sender as ToolStripMenuItem).Text].Visible = false;

            ToolStripMenuItem miClicked = (ToolStripMenuItem)sender;
            // Поиск элемента, на которой выполнен щелчок, по его имени.

            if (miClicked.Text != "Убрать всё" & miClicked.Text != "Показать всё")
            {
                if (miClicked.Checked == true)
                {
                    gridView1.Columns[miClicked.Text].Visible = true;
                    
                    gridView1.Columns[miClicked.Text].VisibleIndex = getTotalValue_pr(miClicked.Text);

                    int count = 0;
                    foreach (string i in prioritet)
                    {
                        if (i == miClicked.Text)
                        {
                            count += 1;
                            gridView1.Columns[miClicked.Text].VisibleIndex = count;
                        }
                    }
                }
                else
                {
                    gridView1.Columns[miClicked.Text].Visible = false;
                }
            }
            else
            {
                foreach (ToolStripMenuItem mainItem in menuStrip1.Items)
                {
                    Boolean change = false;
                    if (miClicked.Text == "Убрать всё") change = false;
                    if (miClicked.Text == "Показать всё") change = true;

                    if (mainItem.Text == "Вид")
                    {
                        foreach (ToolStripItem menuItem in mainItem.DropDownItems)
                        {
                            //mainItem.Checked = false;
                            if (menuItem.Text == "Убрать всё") menuItem.Text = "Показать всё"; //работает
                            else if (menuItem.Text == "Показать всё") menuItem.Text = "Убрать всё";
                            else
                            {
                                gridView1.Columns[menuItem.Text].Visible = change;
                                foreach (ToolStripMenuItem Item in mainItem.DropDownItems)
                                {
                                    Item.Checked = change;
                                }
                            }
                        }
                    }
                }

                int count = 0; // не работает
                foreach (string i in prioritet)
                {
                    if (i == miClicked.Text)
                    {
                        count += 1;
                        gridView1.Columns[miClicked.Text].VisibleIndex = count;
                    }
                }
            }
        }

        int getTotalValue_pr(string column)
        {
            int count = 0;
            int rezult = 0;

            foreach (string i in prioritet)
            {
                count += 1;
                if (i == column)
                {
                    rezult = count;
                } 
               
                //MessageBox.Show(string.Format("Element id:{0}\nName:{1}\t", count, i), "Информация");
            }
            //MessageBox.Show(column+":"+Convert.ToString(rezult));
            return rezult;

        }

        string getTotalValue(GridView view, int listSourceRowIndex)
        {
            //decimal unitPrice = Convert.ToDecimal(view.GetListSourceRowCellValue(listSourceRowIndex, "UnitPrice"));
            //decimal quantity = Convert.ToDecimal(view.GetListSourceRowCellValue(listSourceRowIndex, "Quantity"));
            //decimal discount = Convert.ToDecimal(view.GetListSourceRowCellValue(listSourceRowIndex, "Discount"));
            //return unitPrice * quantity * (1 - discount);

            string zultcount = GetParam_up(Convert.ToString(view.GetListSourceRowCellValue(listSourceRowIndex, "ID_GOODS")));

           if (string.IsNullOrEmpty(zultcount))
                return "#"+Convert.ToString(view.GetListSourceRowCellValue(listSourceRowIndex, "ID_GOODS"));
           else
                return zultcount;

        }

        private void gridView1_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e) //WHAT IS IT
        {
            GridView view = sender as GridView;
            if (e.Column.FieldName == "CODE" && e.IsGetData) e.Value = 
              getTotalValue(view, e.ListSourceRowIndex);
      
        }

        //private void соответствияToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    Form Сonf = new Сonformity();
        //    Сonf.ShowDialog();

        //    //SetParam("221", "229", true);
        //    //SetParam("201", "230", true);
        //    //SetParam("432", "227", true);
        //    //SetParam("191", "224", true);
        //    //SetParam("461", "234", true);


        //    //MessageBox.Show(GetParam_up("226"));
        //}

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //int count = 0;
            //foreach (string i in prioritet)
            //{
            //    count += 1;
            //    MessageBox.Show(string.Format("Element id:{0}\nName:{1}\t", count,i), "Информация");
            //}

            MessageBox.Show("Infa");
        }

        private void отображатьЗаголовкиПриПечатиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem miClicked = (ToolStripMenuItem)sender;
            if (miClicked.Checked == true) gridView1.OptionsPrint.PrintHeader = true;
            else gridView1.OptionsPrint.PrintHeader = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                button1_Click(sender, e);
            }
        }

        private void экспортXlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Get its CSV export options.

                string fileName = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Export.xls";
                XlsExportOptions exportMode = new XlsExportOptions();
                exportMode.TextExportMode = DevExpress.XtraPrinting.TextExportMode.Text;
                //exportMode.Encoding = Encoding.Unicode;
               // exportMode.Separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToString();

                gridControl1.ExportToXls(fileName, exportMode);

                File.AppendAllText(Application.StartupPath + @"\Event.log", "Импорт удачно:" + fileName);
               // MessageBox.Show("Импорт удачно:" + fileName);



                System.Diagnostics.Process.Start(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Импорт НЕудачно " + ex.Message);
            }
        }
    }
}
