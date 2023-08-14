using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprimirProduccion
{
    public class FG
    {
        public enum CXN { ONBORDING };

        public static void startFG()
        {
            try
            {
                string[] commandLineArgs = Environment.GetCommandLineArgs();
                string lineaCxn = commandLineArgs[1];

                string[] Cxn = lineaCxn.Split('#');

                VG._ConStrNSCONBORDING = Cxn[0];
            }
            catch (Exception ex)
            {                
                EscribeLog("VL".ToUpper());
                VG._ConStrNSCONBORDING = @"data source=NSCTHETA\SQLNSC;Initial Catalog=dbNsc_Onbording;User ID=NSCinterno;Password=NSC$1stem@s";
            }                
        }

        public static void EscribeLog(string psMensaje)
        {
            psMensaje = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + psMensaje;
            Console.Write("\n" + psMensaje);                   

            using (StreamWriter sw = File.AppendText("..\\LOGS\\RespaldosAutomaticos\\Task_RespaldoAutomaticos_" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
            {
                sw.WriteLine(psMensaje);

                VG._MensajeBitacora += "<p>" + psMensaje + "</p>";
            }
        }

        public static void ShowError(string psError)
        {
            EscribeLog("¡¡ERROR!! \n\nDETALLES: " + psError.ToUpper());
        }

        public static string GeneraCadenaAleatoria(int piLong)
        {
            System.Random obj = new System.Random();
            string sCadena = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_";
            string strNombreAleatorio = string.Empty;
            for (int i = 0; i < piLong; i++)
            {
                strNombreAleatorio += sCadena[obj.Next(sCadena.Length)].ToString();
            }
            return strNombreAleatorio;
        }

        public static DataSet QueryDBDS(string pStoreProcedure, System.Data.SqlClient.SqlParameter[] p = null, int piCmdTimeout = 0, bool pbGuardaBitacora = false, object btnBoton = null, string pstrDescripcion = "", CXN cxn = CXN.ONBORDING)
        {
            string lsCS = "", SMTP = "";

            switch (cxn)
            {
                case CXN.ONBORDING:
                    lsCS = VG._ConStrNSCONBORDING;
                    break;
            }

            DataSet ds; Exception lEx = null;
            int lintIdBitacora = 0; int liExito = 0;
            string lsBitacora = pStoreProcedure;

            //Establece conexion con la base de datos
            using (var loCnn = new System.Data.SqlClient.SqlConnection(lsCS))
            {
                if (pbGuardaBitacora)
                {     //Arma la cadena de bitacora, ejemplo spNombreStore @P1=1, @P2='Hola', .... @PN=valorN
                    if (p != null)
                    {
                        lsBitacora += " ";
                        for (int lix = 0; lix < p.Length; lix++)
                        {
                            lsBitacora += p[lix].ParameterName + "='";
                            lsBitacora += (p[lix].DbType == DbType.DateTime) ? ((DateTime)p[lix].Value).ToString("yyyyMMdd HH:mm:ss.fff") : p[lix].Value.ToString() + "',";
                        }
                    }
                    //Hace insert de la bitacora 
                    using (var lcmdBita = new System.Data.SqlClient.SqlCommand("spAuditoriaSistema", loCnn))
                    {
                        lcmdBita.CommandType = CommandType.StoredProcedure;
                        lcmdBita.Parameters.AddRange(new System.Data.SqlClient.SqlParameter[] {
                            new System.Data.SqlClient.SqlParameter("@Opcion", 5),
                            new System.Data.SqlClient.SqlParameter("@IdSesion", -1),
                            new System.Data.SqlClient.SqlParameter("@IdPantalla", -1),
                            new System.Data.SqlClient.SqlParameter("@Pantalla", -1 == -1 ? "Auto" : ""),
                            new System.Data.SqlClient.SqlParameter("@IdBoton", -1),
                            new System.Data.SqlClient.SqlParameter("@Query", lsBitacora),
                            new System.Data.SqlClient.SqlParameter("@Descripcion", pstrDescripcion)
                        });
                        lcmdBita.Parameters.Add("@IdBitacora", SqlDbType.Int).Direction = ParameterDirection.Output;
                        loCnn.Open();
                        lcmdBita.ExecuteNonQuery();
                        lintIdBitacora = (int)(lcmdBita.Parameters["@IdBitacora"].Value);
                    }
                }

                //Se hace la ejecucion del store procedure orignal y se guardan los resultados en el dataset ds
                using (var lcmd = new System.Data.SqlClient.SqlCommand(pStoreProcedure, loCnn))
                {
                    lcmd.CommandType = CommandType.StoredProcedure;
                    if (p != null)
                        lcmd.Parameters.AddRange(p);

                    using (var ladpter = new System.Data.SqlClient.SqlDataAdapter(lcmd))
                    {
                        ds = new DataSet();
                        try
                        {
                            ladpter.Fill(ds);
                            liExito = 1;        //Se marca exitosa la ejecucion
                        }
                        catch (Exception Ex)
                        {
                            liExito = 0;        //Se marca con error la ejecucion
                            lEx = Ex;
                        }
                    }
                }

                if (pbGuardaBitacora)
                {     //Se actualiza el estatus de ejecucion del proceso, su duracion y el mensaje de error si aplica.
                    using (var lcmdBita = new System.Data.SqlClient.SqlCommand("spAuditoriaSistema", loCnn))
                    {
                        lcmdBita.CommandType = CommandType.StoredProcedure;
                        lcmdBita.Parameters.AddRange(new System.Data.SqlClient.SqlParameter[] {
                            new System.Data.SqlClient.SqlParameter("@Opcion", 6),
                            new System.Data.SqlClient.SqlParameter("@IdBitacora", lintIdBitacora),
                            new System.Data.SqlClient.SqlParameter("@Exito", liExito),
                            new System.Data.SqlClient.SqlParameter("@Error", (liExito==0) ? lEx.Message : "")
                        });
                        lcmdBita.ExecuteNonQuery();
                    }
                }
                loCnn.Close();
            }

            if (liExito == 0)
            {
                lEx.HelpLink = lintIdBitacora.ToString();
                throw lEx;
            }

            return ds;
        }
    }
}
