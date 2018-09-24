using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    internal class RepModel
    {
        public IModel Select(FbConnection conn/*, FbTransaction trans, string code*/)
        {
            //string query = "SELECT Name FROM Dic_org WHERE modelcode = UPPER(@modelcode)";

            string query = "SELECT id, Name, EGRPOU FROM Dic_org";

            IModel ret = null;

            try
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand(query, conn/*, trans*/))
                {
                    
                    //cmd.Parameters.AddWithValue("modelcode", code.ToUpper());
                    using (FbDataReader reader = cmd.ExecuteReader())
                    {
                        
                        if (reader.Read())
                        {
                            ret = new Model
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                EGRPOU = reader.GetString(2)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Ошибка SQL запроса. {0}", ex.Message));
            }

            return ret;
        }
    }
}
