using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacturaElectronica.FileTemplates;
using FacturaElectronica.WSFE;
using NLog;

namespace FacturaElectronica.Repository
{
    public class FECAE
    {
        Logger LoggerManager { get; set; }

        public FECAE(Logger logger)
        {
            LoggerManager = logger;
        }

        public WSFE.FECAEResponse GetCAERequest(FacturaTemplate factura, WSFE.Service servicio, WSFE.FEAuthRequest authRequest)
        {

            if (factura != null)
            {
                try
                {
                    servicio.FECAESolicitarCompleted += servicio_FECAESolicitarCompleted;

                    WSFE.FECAERequest request = new WSFE.FECAERequest();

                    request.FeCabReq = GetCabecera(factura, servicio, authRequest);

                    request.FeDetReq = GetDetalles(factura, servicio, authRequest);
                    WSFE.FECAEResponse fecae = servicio.FECAESolicitar(authRequest, request);
                    return fecae;
                }
                catch (Exception ex)
                {
                }
            }

            return null;
        }

        private FECAECabRequest GetCabecera(FacturaTemplate factura, WSFE.Service servicio, WSFE.FEAuthRequest authRequest)
        {

            WSFE.FECAECabRequest cabecera = new FECAECabRequest();
            try
            {

                Commons commonUtilities = new Commons(servicio, authRequest);
                int? id = commonUtilities.GetTipoComprobanteId(factura.Comprobante, factura.TipoComprobante);
                cabecera.CbteTipo = id.Value;
                cabecera.CantReg = 1; //factura.Items.Count(); //Siempre enviamos uno solo. Como mejora a futuro se habla para procesamiento por lotes
                cabecera.PtoVta = commonUtilities.GetPuntoDeVentaId(factura.ComprobanteRelacionado).Value;
            }
            catch (Exception ex)
            {
                LoggerManager.Error("Ha ocurrido un error al generar la cabecera", ex);
            }
            return cabecera;
        }

        private WSFE.FECAEDetRequest[] GetDetalles(FacturaTemplate factura, WSFE.Service servicio, WSFE.FEAuthRequest authRequest)
        {
            WSFE.FECAEDetRequest[] fedetreq = new WSFE.FECAEDetRequest[1];

            try
            {
                WSFE.FECAEDetRequest detalle = new FECAEDetRequest();
                List<WSFE.AlicIva> alicuota_iva = new List<AlicIva>();

                Commons commonsUtilities = new Commons(servicio, authRequest);

                int tipo_comprobante = commonsUtilities.GetTipoComprobanteId(factura.Comprobante, factura.TipoComprobante).Value;
                int ptoVta = factura.PuntoDeVenta;
                int conceptoTipo = commonsUtilities.GetTipoConceptoId(factura.Concepto).Value;
                int documetoTipo = commonsUtilities.GetTipoDocumentoId(factura.TipoDocumento).Value;

                detalle.CbteDesde = commonsUtilities.GetComprobanteProximoAAtutorizar(ptoVta, tipo_comprobante) + 1; //numero de comprobante desde
                detalle.CbteHasta = detalle.CbteDesde; //numero de comprobante hasta
                detalle.CbteFch = factura.FechaFactura.ToString("yyyyMMdd");
                detalle.Concepto = conceptoTipo;
                detalle.DocTipo = documetoTipo;
                detalle.DocNro = factura.Cuit;
                detalle.MonId = "PES";//Siempre pesos
                detalle.MonCotiz = 1;//Cotizacion de la moneda es siempre 1 porque es pesos

                if (factura.Concepto.Equals("S"))
                {
                    detalle.FchServDesde = factura.FechaDesde.ToString("yyyyMMdd");
                    detalle.FchServHasta = factura.FechaHasta.ToString("yyyyMMdd");
                    detalle.FchVtoPago = factura.FechaVencimiento.ToString("yyyyMMdd");
                }

                double importeSinIVA = 0;
                double totalIVA = 0;

                //Sumamos y calculamos
                //Como observacion hay que aclarar que el tema de los decimales es bastante complicado y pueden existir 
                //situaciones donde la AFIP rechace las solicitudes por redondeos que se efectuan de distinta manera
                foreach (FacturaItemsTemplate item in factura.GetItems())
                {
                    importeSinIVA += item.Unitario;
                    totalIVA += (item.Unitario * (item.TasaIVA / 100));

                    WSFE.AlicIva iva = new AlicIva();
                    iva.BaseImp = item.Unitario;
                    iva.Importe = (item.Unitario * (item.TasaIVA / 100));
                    iva.Id = commonsUtilities.GetTipoIVAId(item.TasaIVA);
                    alicuota_iva.Add(iva);
                }

                detalle.ImpIVA = Math.Round(totalIVA, 2);
                detalle.ImpNeto = Math.Round(importeSinIVA, 2);
                detalle.ImpTotal = detalle.ImpIVA + detalle.ImpNeto;
                detalle.ImpTrib = 0.0;
                detalle.Iva = alicuota_iva.GroupBy(x => x.Id).Select(x => new WSFE.AlicIva
                {
                    Id = x.Key,
                    Importe = Math.Round(x.Sum(i => i.Importe), 2),
                    BaseImp = Math.Round(x.Sum(i => i.BaseImp), 2)
                }).ToArray();
                fedetreq[0] = detalle;
            }
            catch (Exception ex)
            {
                LoggerManager.Error("Ha ocurrido un error al generar el detalle", ex);
            }

            return fedetreq;
        }

        private void servicio_FECAESolicitarCompleted(object sender, WSFE.FECAESolicitarCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
