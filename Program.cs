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

//****************************************************************************************************************************************************************************************************************************

string pathConfig = "\\\\NSCZETA\\compartido$\\Publico\\video\\ConfiguracionRespaldos\\configuracion.txt";

string _stringConfiguracion = "";
using (StreamReader sr = new StreamReader(pathConfig))
{
    string line;
    while ((line = sr.ReadLine()) != null)
    {
        _stringConfiguracion += line + "; \n";
    }
}

//****************************************************************************************************************************************************************************************************************************

string[] wordsConfiguracion = _stringConfiguracion.Split(';');

string[] _sourceDirectoryPath = wordsConfiguracion[0].Split('*');
string sourceDirectoryPath = _sourceDirectoryPath[1].TrimStart().Trim('\u0009');

string[] _compressedFilePath = wordsConfiguracion[1].Split('*');
string compressedFilePath = _compressedFilePath[1].TrimStart().Trim('\u0009');
string _nombre = "RespaldoAutomatico_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ".zip";
_nombre = _nombre.Replace(":", "_");
compressedFilePath += _nombre;

string[] _SendEmail = wordsConfiguracion[2].Split('*');
string SendEmail = _SendEmail[1].TrimStart().Trim('\u0009');

string[] _BackUpWeekend = wordsConfiguracion[3].Split('*');
string BackUpWeekend = _SendEmail[1].TrimStart().Trim('\u0009');

string[] _TO = wordsConfiguracion[4].Split('*');
string TO = _TO[1].TrimStart().Trim('\u0009');

string[] _CC = wordsConfiguracion[5].Split('*');
string CC = _CC[1].TrimStart().Trim('\u0009');

//****************************************************************************************************************************************************************************************************************************

DateTime today = DateTime.Today;
if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday)
{
    if (!Convert.ToBoolean(BackUpWeekend))
        return;
}

//****************************************************************************************************************************************************************************************************************************

ZipFile.CreateFromDirectory(sourceDirectoryPath, compressedFilePath);

//****************************************************************************************************************************************************************************************************************************

string msj = "Se a respaldado los archivos de NSCMX_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
if (!System.IO.File.Exists(compressedFilePath))
    msj = "No " + msj;

if (Convert.ToBoolean(SendEmail))
{
    MailMessage correo = new MailMessage();
    correo.From = new MailAddress("automatizacion@nscasesores.com", "Notificacion respaldo de NSCMX", System.Text.Encoding.UTF8);
    correo.To.Add(TO);
    if (!string.IsNullOrEmpty(CC) || !string.IsNullOrWhiteSpace(CC))
    {
        correo.CC.Add(CC);
    }
    correo.Subject = "Respaldos automaticos de NSCMX";
    correo.Body = msj;
    correo.IsBodyHtml = true;
    correo.Priority = MailPriority.High;
    SmtpClient smtp = new SmtpClient();
    smtp.UseDefaultCredentials = false;
    smtp.Host = "smtp.office365.com";
    smtp.Port = 587;
    smtp.Credentials = new System.Net.NetworkCredential("automatizacion@nscasesores.com", "nlzwddwbtwfvxvbj");//Cuenta de correo
    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
    smtp.EnableSsl = true;
    smtp.Send(correo);
}

Environment.Exit(0);

/**
NAME TXT: configuracion.txt
 * 
SourceDirectoryPath* C:\CarpetaDePruebas\NSCMXProduccion_20230213
CompressedFilePath* \\NSCZETA\compartido$\Publico\video\Gustavo\respaldo de NSCMX\
SendEmail* false
BackUpWeekend* false
To* desarrollo@nscasesores.com
Cc*
*/