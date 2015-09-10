using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturaElectronica.Repository
{
    public class Commons
    {
        List<WSFE.IvaTipo> alicuotas_iva;
        WSFE.Service Servicio { get; set; }
        WSFE.FEAuthRequest AuthRequest { get; set; }

        public Commons(WSFE.Service servicio, WSFE.FEAuthRequest authRequest)
        {
            Servicio = servicio;
            AuthRequest = authRequest;

            alicuotas_iva = ((WSFE.IvaTipoResponse)Servicio.FEParamGetTiposIva(AuthRequest)).ResultGet.ToList();
        }

        public int? GetTipoComprobanteId(string comprobante, string tipoCompro)
        {
            int? tipoComprobanteId = null;
            string descripcion = string.Empty;


            try
            {
                switch (comprobante)
                {
                    case "FCV":
                        if (tipoCompro.Equals("A"))
                        {
                            descripcion = "Factura A";
                        }
                        else
                        {
                            descripcion = "Factura B";
                        }
                        break;
                    case "NCV":
                        if (tipoCompro.Equals("A"))
                        {
                            descripcion = "Nota de Crédito A";
                        }
                        else
                        {
                            descripcion = "Nota de Crédito B";
                        }
                        break;
                    case "NDV":
                        if (tipoCompro.Equals("A"))
                        {
                            descripcion = "Nota de Débito A";
                        }
                        else
                        {
                            descripcion = "Nota de Débito B";
                        }
                        break;
                }

                WSFE.CbteTipoResponse tipo = Servicio.FEParamGetTiposCbte(AuthRequest);
                tipoComprobanteId = tipo.ResultGet.Where(x => x.Desc.Contains(descripcion)).SingleOrDefault().Id;
            }
            catch (Exception ex)
            {

            }

            return tipoComprobanteId;
        }

        public int? GetPuntoDeVentaId(string comprobante)
        {
            int? punto_de_venta = null;
            int punto_de_venta_aux = 0;
            try
            {
                string[] numero_comprobante_partido = comprobante.Split('-');
                if (int.TryParse(numero_comprobante_partido[0], out punto_de_venta_aux))
                {
                    punto_de_venta = punto_de_venta_aux;
                }

            }
            catch (Exception ex)
            {
                //TODO: MANEJO EXCEPCION
            }

            return punto_de_venta;
        }

        public int? GetNumeroComprobanteId(string comprobante)
        {
            int? punto_de_venta = null;
            int punto_de_venta_aux = 0;
            try
            {
                string[] numero_comprobante_partido = comprobante.Split('-');
                if (int.TryParse(numero_comprobante_partido[1], out punto_de_venta_aux))
                {
                    punto_de_venta = punto_de_venta_aux;
                }

            }
            catch (Exception ex)
            {
                //TODO: MANEJO EXCEPCION
            }

            return punto_de_venta;
        }

        public int? GetTipoConceptoId(string concepto)
        {
            int? tipoConceptoId = null;
            string descripcion = string.Empty;


            try
            {
                switch (concepto)
                {
                    case "P":
                        descripcion = "Producto";
                        break;
                    case "S":
                        descripcion = "Servicios";
                        break;
                }

                WSFE.ConceptoTipoResponse tipo = Servicio.FEParamGetTiposConcepto(AuthRequest);
                tipoConceptoId = tipo.ResultGet.Where(x => x.Desc.Equals(descripcion)).SingleOrDefault().Id;
            }
            catch (Exception ex)
            {

            }

            return tipoConceptoId;
        }

        public int? GetTipoDocumentoId(string tipoDocumento)
        {
            int? tipoDocumentoId = null;
            string descripcion = string.Empty;


            try
            {
                WSFE.DocTipoResponse tipo = Servicio.FEParamGetTiposDoc(AuthRequest);
                tipoDocumentoId = tipo.ResultGet.Where(x => x.Desc.Equals(tipoDocumento)).SingleOrDefault().Id;
            }
            catch (Exception ex)
            {

            }

            return tipoDocumentoId;
        }

        public int GetComprobanteProximoAAtutorizar(int punto_venta, int comprobante_tipo)
        {
            int numero_comprobante = 0;
            try
            {
                WSFE.FERecuperaLastCbteResponse tipo = Servicio.FECompUltimoAutorizado(AuthRequest, punto_venta, comprobante_tipo);
                numero_comprobante = tipo.CbteNro;
            }
            catch (Exception ex)
            {

            }

            return numero_comprobante;
        }

        public int GetTipoIVAId(double iva)
        {
            int ivaId = 0;
            try
            {
                string descripcion = string.Format("{0}%", iva.ToString());
                ivaId = Convert.ToInt16(alicuotas_iva.Where(x => x.Desc.Equals(descripcion)).SingleOrDefault().Id);
            }
            catch (Exception ex)
            {

            }

            return ivaId;
        }
    }
}
