using System.IO.Compression;
using System.IO;
using System.Net.Mail;
using System.Reflection.PortableExecutable;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using ComprimirProduccion;

try
{
    string lsMensaje;

    lsMensaje = "************INICIANDO Versionador NSCMX - MOD.20230731 : " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss") + "************";

    Console.Write(lsMensaje);

    string Carpeta = "..\\LOGS\\RespaldosAutomaticos\\";
    if (!Directory.Exists(Carpeta))
        Directory.CreateDirectory(Carpeta);

    using (StreamWriter sw = File.AppendText("..\\LOGS\\Task_RespaldoAutomaticos_" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
    {
        sw.WriteLine("\n" + lsMensaje);
    }

    FG.startFG();

    FG.EscribeLog("Linea de conexión (L1): " + "Establecida");//VG._ConStrNSCONBORDING);

    FG.EscribeLog("PATH LOG: " + Path.GetFullPath(Carpeta));

    var p = new System.Data.SqlClient.SqlParameter[]{
            new System.Data.SqlClient.SqlParameter("@Opcion", 4)
    };

    FG.QueryDBDS("spVersionadorNSCMX", p);

    FG.EscribeLog("INICIO DE RESPALDO");

    p = new System.Data.SqlClient.SqlParameter[]{
            new System.Data.SqlClient.SqlParameter("@Opcion", 3)
    };

    DataSet ds = FG.QueryDBDS("spVersionadorNSCMX", p);

    string sourceDirectoryPath = (string)ds.Tables[0].Rows[0]["sourceDirectoryPath"];
    string compressedFilePath = (string)ds.Tables[0].Rows[0]["compressedFilePath"];
    string _nombre = "RespaldoAutomatico_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip";
    _nombre = _nombre.Replace(":", "_");
    compressedFilePath += _nombre;

    FG.EscribeLog("LECTURA DE CONFIGURACION");
    FG.EscribeLog("sourceDirectoryPath: ".ToUpper() + sourceDirectoryPath);
    FG.EscribeLog("compressedFilePath: ".ToUpper() + compressedFilePath);

    string sourceDirectoryPathRar = @"C:\RespaldoTemp";
    if (Directory.Exists(sourceDirectoryPathRar))
    {
        FG.EscribeLog("EL DIRECTORIO TEMPORAL YA EXISTE");
        Directory.Delete(sourceDirectoryPathRar, true);
        FG.EscribeLog("DIRECTORIO TEMPORAL ELIMINADO");
    }

    FG.EscribeLog("CREANDO DIRECTORIO TEMPORAL");
    Directory.CreateDirectory(sourceDirectoryPathRar);
    FG.EscribeLog("DIRECTORIO TEMPORAL CREADO");

    sourceDirectoryPathRar = Path.Combine(sourceDirectoryPathRar, _nombre);

    FG.EscribeLog("COMPRIMIENDO PROYECTO");

    ZipFile.CreateFromDirectory(sourceDirectoryPath, sourceDirectoryPathRar);

    FG.EscribeLog("PROYECTO COMPRIMIDO");

    FG.EscribeLog("INICIO DE COPIADO");

    File.Copy(sourceDirectoryPathRar, compressedFilePath, true);

    FG.EscribeLog("FINALIZA COPIADO");

    string msj = "Se a respaldado los archivos " + _nombre;
    int opcion = 1;

    if (!System.IO.File.Exists(compressedFilePath))
    {
        msj = "No " + msj;
        FG.EscribeLog("¡¡ERROR!!: " + msj.ToUpper());
        opcion = 2;
    }

    msj = msj.ToUpper();
    FG.EscribeLog("CREANDO CUERPO DEL CORREO");

    string html = "<!DOCTYPE html>" +
                  "<html>" +
                  "<head>" +
                  "<meta charset=\"utf-8\">" +
                  "<title>" + msj + "</title>" +
                  "</head>" +
                  "<body>" +
                  "<h2>" + msj + "</h2>" +
                  "<br>" +
                  "<h3>Bitacora</h3>" +
                  VG._MensajeBitacora +
                  "</body>" +
                  "</html>";

    p = new System.Data.SqlClient.SqlParameter[]{
        new System.Data.SqlClient.SqlParameter("@Opcion", opcion)
    ,   new System.Data.SqlClient.SqlParameter("@Body", html)

    };

    FG.QueryDBDS("spVersionadorNSCMX", p);

    FG.EscribeLog("EMAIL ENVIADO");
}
catch (Exception ex)
{

    string Error = "<p>DETALLES DEL ERROR: </p>";
    Error += "<p>" + ex.ToString() + "</p>";
    Error += "<p>BITACORA: </p>" + VG._MensajeBitacora;

    FG.EscribeLog("¡¡ERROR!!: " + Error.Replace("</p><p>", Environment.NewLine).Replace("<p>", ""));

    var p = new System.Data.SqlClient.SqlParameter[]{
        new System.Data.SqlClient.SqlParameter("@Opcion", 2)
    ,   new System.Data.SqlClient.SqlParameter("@Body", Error)

    };

    FG.QueryDBDS("spVersionadorNSCMX", p);

    FG.EscribeLog("EMAIL ERROR ENVIADO");
}

FG.EscribeLog("FIN");

//FG.EscribeLog("Presione cualquier tecla para cerrar . . .".ToUpper());
//Console.ReadKey();
Environment.Exit(0);