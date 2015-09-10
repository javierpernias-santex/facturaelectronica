using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FacturaElectronica.FileTemplates;
using FacturaElectronica.Repository;
using FacturaElectronica.Utils;
using FileHelpers;
using NLog;
using NLog.Targets;

namespace FacturaElectronica
{
    class Program
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            logger.Info("Comienzo Proceso Obtencion de CAE");

            logger.Info("Leyendo configuracion de aplicacion");

            var location = ConfigurationManager.AppSettings["FileLocation"];
            var pattern = ConfigurationManager.AppSettings["FilePattern"];
            var patternWrite = ConfigurationManager.AppSettings["FilePatternWrite"];
            var cuit = ConfigurationManager.AppSettings["EmpresaCuit"];
            var modo_ejecucion = ConfigurationManager.AppSettings["modo_ejecucion"];
            var key = ConfigurationManager.AppSettings["certificado_key"];
            var url_web_service_autenticacion = "";
            var url_web_service_wsfe = "";
            var path_certificado = ConfigurationManager.AppSettings["path_certificado"];

            if (modo_ejecucion.Equals("test"))
            {
                logger.Info("Modo Ejecucion: Testing");
                url_web_service_autenticacion = ConfigurationManager.AppSettings["url_homologacion_login"];
                url_web_service_wsfe = ConfigurationManager.AppSettings["url_homologacion_wsfe"];
            }
            else
            {
                logger.Info("Modo Ejecucion: Produccion");
                url_web_service_autenticacion = ConfigurationManager.AppSettings["url_produccion_login"];
                url_web_service_wsfe = ConfigurationManager.AppSettings["url_produccion_wsfe"];
            }


            var filename = Paths.getFile(pattern, location);

            logger.Info("Buscando archivos de factura");
            if (filename.Any())
            {
                try
                {
                    logger.Info("Se encontraron {0} archivos para procesar", filename.Count);
                    //Seteamos los datos para iniciar las comunicaciones
                    WSFE.Service servicio = new WSFE.Service();
                    servicio.Url = url_web_service_wsfe;
                    Authentication auth = new Authentication(cuit, url_web_service_autenticacion, path_certificado, logger, key);


                    var engine = new FileHelperAsyncEngine(typeof(FacturaTemplate));
                    var records = new List<FacturaTemplate>();
                    var sw = new Stopwatch();
                    sw.Start();
                    //Read File
                    foreach (string filePath in filename)
                    {
                        logger.Info("Leyendo archivo {0}", filePath);
                        using (var fileStream = File.OpenRead(filePath))
                        using (var bs = new BufferedStream(fileStream))
                        using (TextReader textReader = new StreamReader(bs))
                        {
                            engine.BeginReadStream(textReader);
                            object record;

                            do
                            {
                                record = engine.ReadNext();

                                if (record != null) records.Add((FacturaTemplate)record);
                            } while (record != null);
                        }

                        //Start Processing       
                        if (records.Any())
                        {
                            FECAE solicitarCAE = new FECAE(logger);
                            foreach (FacturaTemplate factura in records)
                            {
                                logger.Info("Procesando Comprobante N°{0}", factura.ComprobanteRelacionado);
                                WSFE.FECAEResponse response = solicitarCAE.GetCAERequest(factura, servicio, auth.AuthRequest);

                                if (response.Errors != null && response.Errors.Any())
                                {
                                    logger.Info("Errores reportados por web service de la AFIP");
                                    foreach (WSFE.Err error in response.Errors)
                                    {
                                        logger.Error("Codigo: {0}\nMensaje: {1}", error.Code, error.Msg);
                                    }
                                }

                                if (response.FeCabResp.Resultado.Equals("R"))
                                {

                                    foreach (WSFE.FECAEDetResponse r in response.FeDetResp)
                                    {
                                        if (r.Observaciones != null)
                                        {
                                            logger.Info("Factura rechaza con observaciones");
                                            foreach (WSFE.Obs observacion in r.Observaciones)
                                            {
                                                logger.Error("Codigo: {0}\nMensaje: {1}", observacion.Code, observacion.Msg);
                                            }
                                        }
                                        else
                                        {
                                            logger.Info("Factura rechaza sin observaciones");
                                        }
                                    }
                                }
                                else
                                {
                                    if (response.FeCabResp.Resultado.Equals("A"))
                                    {
                                        logger.Info("Factura aprobada con CAE N°: {0} con vencimiento {1}", response.FeDetResp.First().CAE, response.FeDetResp.First().CAEFchVto);
                                        var writeEngine = new FileHelperEngine(typeof(FacturaTemplateWrite));

                                        var filenameWrite = string.Format("{0}\\{1}", location, string.Format(patternWrite, Path.GetFileNameWithoutExtension(filePath)));
                                        List<FacturaTemplateWrite> items = new List<FacturaTemplateWrite>();
                                        items.Add(new FacturaTemplateWrite
                                        {
                                            nro_comprobante_electronico = string.Format("{0}-{1}", response.FeCabResp.PtoVta.ToString().PadLeft(4, '0'), response.FeDetResp.First().CbteDesde.ToString().PadLeft(8, '0')),
                                            cae = response.FeDetResp.First().CAE,
                                            cae_vencimiento = response.FeDetResp.First().CAEFchVto
                                        });

                                        writeEngine.WriteFile(filenameWrite, items);

                                        //Renombramos el archivo

                                        string newFileName = Path.GetFileName(filePath);

                                        string newFullPath = string.Format("{1}\\PROCESADO_{0}", newFileName, location);
                                        File.Move(filePath, newFullPath);
                                    }
                                }
                            }
                            records.Clear();
                        }
                    }
                }
                catch (FileHelpersException ex)
                {
                    logger.Error("Error al leer/parsear el archivo", ex);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            }
            else
            {
                logger.Error("No se encontro archivo para procesar");
            }

            logger.Info("Fin procesamiento");
        }
    }
}
