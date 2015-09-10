using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NLog;

namespace FacturaElectronica.Repository
{
    public class Authentication
    {
        const string DEFAULT_URLWSAAWSDL = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms?WSDL";
        const string DEFAULT_SERVICIO = "wsfe";
        const string DEFAULT_CERTSIGNER = "C:\\factura_electronica\\openssl-0.9.8k_X64\\bin\\CSRJavierProgramadorAFIPExportado.pfx";
        const string keyPath = "C:\\factura_electronica\\openssl-0.9.8k_X64\\bin\\jpernias";
        const bool DEFAULT_VERBOSE = true;
        LoginTicket TicketRespuesta { get; set; }
        string LoginTicketRespuesta { get; set; }
        long Cuit { get; set; }

        public WSFE.FEAuthRequest AuthRequest
        {
            get
            {
                WSFE.FEAuthRequest authRequest = null;

                try
                {
                    XmlDocument XmlLoginTicketResponse = new XmlDocument();
                    XmlLoginTicketResponse.LoadXml(LoginTicketRespuesta);

                    authRequest = new WSFE.FEAuthRequest();

                    authRequest.Cuit = Cuit;
                    authRequest.Sign = XmlLoginTicketResponse.SelectSingleNode("//sign").InnerText;
                    authRequest.Token = XmlLoginTicketResponse.SelectSingleNode("//token").InnerText;

                }
                catch (Exception excepcionAlAnalizarLoginTicketResponse)
                {
                    throw new Exception("***Error ANALIZANDO el LoginTicketResponse : " + excepcionAlAnalizarLoginTicketResponse.Message);
                }

                return authRequest;
            }
        }

        public Authentication(string cuit, string url, string pathCertificado, Logger logger, string key)
        {
            string strIdServicioNegocio = DEFAULT_SERVICIO;

            Cuit = Convert.ToInt64(cuit);
            TicketRespuesta = new LoginTicket(logger);
            LoginTicketRespuesta = TicketRespuesta.ObtenerLoginTicketResponse(strIdServicioNegocio, url, pathCertificado, key);
        }
    }
}
