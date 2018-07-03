using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Сonformity : Form
    {
        public Сonformity()
        {
            InitializeComponent();
        }

        private void Сonformity_Load(object sender, EventArgs e)
        {
            DataTable dt = new DataTable("Table");
            DataColumn Code = new DataColumn("Code", Type.GetType("System.String"));
            dt.Columns.Add(Code);
            DataColumn Code_Terra = new DataColumn("Code_Terra", Type.GetType("System.String"));
            dt.Columns.Add(Code_Terra);
            //DataColumn int_result = new DataColumn("int_result", Type.GetType("System.Int32"), "int1+int2");
            //dt.Columns.Add(int_result);

            //Создаем первый датагридвью
            //DataGridView firstDGV = new DataGridView();
            GridControl firstDGV = new DevExpress.XtraGrid.GridControl();
            firstDGV.Location = new Point(0, 0);
            firstDGV.Dock = DockStyle.Fill;
            //firstDGV.Enabled = false; // редактирование
            //firstDGV.Size = new Size(200, 200);
            // Оключаем генерацию колонок, чтобы показывались только те что нужны
            //firstDGV.AutoGenerateColumns = false;

            firstDGV.ForceInitialize();

            //Добавялем отображаемые колонки
            //DataGridViewTextBoxColumn int1Column = new DataGridViewTextBoxColumn();
            GridView view = firstDGV.MainView as GridView;
           // view.Name = "View1";
            view.Name = "gridView1";

            GridColumn columnCODE = new GridColumn();
            columnCODE.Caption = "Code";
            columnCODE.FieldName = "Code";
            columnCODE.OptionsColumn.AllowEdit = false;
            columnCODE.UnboundType = DevExpress.Data.UnboundColumnType.String;
            //columnTotal.UnboundExpression = "[UnitPrice]*[Quantity]*(1-[Discount])";
            view.Columns.Add(columnCODE);
            columnCODE.VisibleIndex = view.VisibleColumns.Count;
            columnCODE.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
            //columnCODE.DisplayFormat.FormatString = "TTL={0:c2}";
            columnCODE.AppearanceCell.BackColor = Color.LemonChiffon;
            view.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gridView1_RowUpdated);
            firstDGV.EditorKeyUp += new System.Windows.Forms.KeyEventHandler(this.gridControl1_EditorKeyUp);
            view.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridview1_KeyDown);



            GridColumn columnCode_Terra = new GridColumn();
            columnCode_Terra.Caption = "Code_Terra";
            columnCode_Terra.FieldName = "Code_Terra";
            columnCode_Terra.OptionsColumn.AllowEdit = true;
            columnCode_Terra.UnboundType = DevExpress.Data.UnboundColumnType.String;
            //columnTotal.UnboundExpression = "[UnitPrice]*[Quantity]*(1-[Discount])";
            view.Columns.Add(columnCode_Terra);
            columnCode_Terra.VisibleIndex = view.VisibleColumns.Count;
            columnCode_Terra.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
            //columnCode_Terra.DisplayFormat.FormatString = "TTL={0:c2}";
            columnCode_Terra.AppearanceCell.BackColor = Color.LemonChiffon;

            //Добавляем на форму
            this.Controls.Add(firstDGV);

            ArrayList objectList = new ArrayList() { "151q", "151", "111", "012", "013", "011", "014", "055", "073", "072", "074", "071", "056", "251", "326obm", "322",
                                                      "911b", "911", "911Lg", "496", "913", "321", "217q", "217", "8444", "882q", "882Lg", "882", "873q", "873Lg",
                                                      "873", "868q", "868Lg", "868", "866q", "866Lg", "866", "806q", "806Lg", "806", "859q", "859Lg", "859", "858q",
                                                      "858Lg", "858", "856q", "856Lg", "856", "853q", "853Lg", "853", "852q", "852Lg", "852", "851q", "851Lg", "851", "845q",
                                                      "845Lg", "845", "844q", "844Lg", "844", "839q", "839Lg", "839", "835q", "835Lg", "835", "833q", "833Lg", "833", "831q",
                                                      "831Lg", "831", "826q", "826Lg", "826", "818q", "818Lg", "818", "816q", "816Lg", "816", "811q", "811Lg", "811", "212",
                                                      "213q", "213", "211", "HIV339", "HCV350", "VHB320", "915", "253", "254", "252", "248", "245", "234", "247", "239", "241",
                                                      "235", "246", "233", "249", "5468", "244", "243", "238", "237", "236", "232", "218", "216", "121", "029", "028", "027", "053",
                                                      "052", "054", "051", "214", "221", "223Lg", "223", "222", "201", "181", "171", "525", "487", "484", "484%", "327", "442", "441",
                                                      "522", "431", "432", "324", "324%", "323", "323%", "225", "523", "485", "485%", "524", "326l", "371", "526", "451", "451%",
                                                      "481", "499", "527", "528", "529", "521", "191", "465", "464", "462", "467", "463", "466", "461", "557", "488", "482", "141", "325",
                                                      "498", "86AZF", "84AZF", "255AZF", "254AZF", "134AZF", "127AZF", "486", "486%", "497", "326"};
            foreach (object o in objectList)
            {
                //Console.WriteLine(Convert.ToString(o));
                dt.Rows.Add(new Object[] { o, Form1.GetParam(Convert.ToString(o))});
            }

            //List<string> codes = new List<string>() { "151q", "151", "111", "012", "013", "011", "014", "055", "073", "072", "074", "071", "056", "251", "326obm", "322",
            //                                          "911b", "911", "911Lg", "496", "913", "321", "217q", "217", "8444", "882q", "882Lg", "882", "873q", "873Lg",
            //                                          "873", "868q", "868Lg", "868", "866q", "866Lg", "866", "806q", "806Lg", "806", "859q", "859Lg", "859", "858q",
            //                                          "858Lg", "858", "856q", "856Lg", "856", "853q", "853Lg", "853", "852q", "852Lg", "852", "851q", "851Lg", "851", "845q",
            //                                          "845Lg", "845", "844q", "844Lg", "844", "839q", "839Lg", "839", "835q", "835Lg", "835", "833q", "833Lg", "833", "831q",
            //                                          "831Lg", "831", "826q", "826Lg", "826", "818q", "818Lg", "818", "816q", "816Lg", "816", "811q", "811Lg", "811", "212",
            //                                          "213q", "213", "211", "HIV339", "HCV350", "VHB320", "915", "253", "254", "252", "248", "245", "234", "247", "239", "241",
            //                                          "235", "246", "233", "249", "5468", "244", "243", "238", "237", "236", "232", "218", "216", "121", "029", "028", "027", "053",
            //                                          "052", "054", "051", "214", "221", "223Lg", "223", "222", "201", "181", "171", "525", "487", "484", "484%", "327", "442", "441",
            //                                          "522", "431", "432", "324", "324%", "323", "323%", "225", "523", "485", "485%", "524", "326l", "371", "526", "451", "451%",
            //                                          "481", "499", "527", "528", "529", "521", "191", "465", "464", "462", "467", "463", "466", "461", "557", "488", "482", "141", "325",
            //                                          "498", "86AZF", "84AZF", "255AZF", "254AZF", "134AZF", "127AZF", "486", "486%", "497", "326"};

            //foreach (string s in codes)
            //{
            // dt.Rows.Add(new Object[] { s, Form1.GetParam(s) });
            //}
      
            firstDGV.DataSource = dt.DefaultView;

            if (string.IsNullOrEmpty(Convert.ToString(objectList.Count)))
                this.Text = "Соответствия";
            else
                this.Text = "Соответствия - (Общее число элементов коллекции: " + Convert.ToString(objectList.Count) + ")";
        }

        private void gridView1_RowUpdated(object sender, RowObjectEventArgs e) //изменение соответствий
        {

            GridView View = sender as GridView;
            // MessageBox.Show(Convert.ToString(View.FocusedRowHandle), "Error");

            string cellValue_code = View.GetRowCellValue(View.FocusedRowHandle, "Code").ToString();
            string cellValue_code_terra = View.GetRowCellValue(View.FocusedRowHandle, "Code_Terra").ToString();
            //MessageBox.Show(cellValue_code+":"+cellValue_code_terra);

            Form1.SetParam(cellValue_code, cellValue_code_terra, true);
            //if (cellValue == "Germany")
            //    e.Cancel = true;

        }

        private void gridControl1_EditorKeyUp(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(Convert.ToString(e.KeyValue), "Error");
            //if (e.KeyValue == 222)
            //{
            //    MessageBox.Show(Convert.ToString(e.KeyValue), "Error");
            //    e.Handled = true;
            //    return;
            //}
           // base.OnKeyPress(e);

            if (e.KeyCode == Keys.Enter)
            {
              MessageBox.Show(Convert.ToString(e), "Error");           
            }
         
        }

        private void gridview1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            

           // if (e.KeyCode == Keys.Enter)
           // {
                //MessageBox.Show(Convert.ToString(e), "Enter");
                //GridView View = sender as GridView;
                //string cellValue_code = View.GetRowCellValue(View.FocusedRowHandle, "Code").ToString();
                //string cellValue_code_terra = View.GetRowCellValue(View.FocusedRowHandle, "Code_Terra").ToString(); //????!!!!
                //MessageBox.Show(cellValue_code+":"+cellValue_code_terra);

          //  }
                if (e.KeyCode == System.Windows.Forms.Keys.Enter)
                {
                GridView View = sender as GridView;
                if (View.FocusedColumn.Name != "Code_Terra") return;
                //XtraMessageBox.Show("Key Down " + View.EditingValue);
                    string cellValue_code = View.GetRowCellValue(View.FocusedRowHandle, "Code").ToString();
                    string cellValue_code_terra = View.EditingValue.ToString(); //!!!!!!
                    Form1.SetParam(cellValue_code, cellValue_code_terra, true);

                }

            
        }



        }
}
