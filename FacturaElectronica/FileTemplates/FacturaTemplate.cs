using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace FacturaElectronica.FileTemplates
{
    [DelimitedRecord("|")]
    public class FacturaTemplate
    {
        public string Comprobante;

        public string TipoComprobante;

        public int PuntoDeVenta;

        [FieldTrim(TrimMode.Both)]
        public string TipoDocumento;

        [FieldTrim(TrimMode.Both)]
        public long Cuit;

        [FieldConverter(ConverterKind.Date, "dd/MM/yyyy"), FieldTrim(TrimMode.Both)]
        public DateTime FechaFactura;

        [FieldTrim(TrimMode.Both)]
        public string Concepto;

        /*FECHAS PARA SERVICIOS - PARA CONCEPTOS VIENEN PERO HAY QUE IGNORARLAS*/
        [FieldConverter(ConverterKind.Date, "dd/MM/yyyy"), FieldTrim(TrimMode.Both)]
        public DateTime FechaDesde;

        [FieldConverter(ConverterKind.Date, "dd/MM/yyyy"), FieldTrim(TrimMode.Both)]
        public DateTime FechaHasta;

        [FieldConverter(ConverterKind.Date, "dd/MM/yyyy"), FieldTrim(TrimMode.Both)]
        public DateTime FechaVencimiento;

        [FieldTrim(TrimMode.Both)]
        public string FormaPago;

        [FieldTrim(TrimMode.Both)]
        public string ComprobanteRelacionado;

        [FieldDelimiter("_ITEM|"), FieldTrim(FileHelpers.TrimMode.Both, "|_ITEM|")]
        public string[] Items;

        public List<FacturaItemsTemplate> GetItems()
        {
            List<FacturaItemsTemplate> items = new List<FacturaItemsTemplate>();

            foreach (string item in Items)
            {
                int index = 1;

                if (!string.IsNullOrWhiteSpace(item))
                {
                    object[] fields = item.Split('|');
                    FacturaItemsTemplate facturaItem = new FacturaItemsTemplate
                    {
                        ItemOrder = index,
                        Codigo = fields[0].ToString(),
                        Descripcion = fields[1].ToString(),
                        Cantidad = fields[2].ToString(),
                        Unitario = Math.Round(Convert.ToDouble(fields[3]), 2),
                        TasaIVA = Math.Round(Convert.ToDouble(fields[4]), 2),
                        PorcentajeDescuento = Math.Round(Convert.ToDouble(fields[5]), 2)
                    };

                    items.Add(facturaItem);
                    index++;
                }
            }

            return items;
        }

    }

    public class FacturaItemsTemplate
    {
        public int ItemOrder { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string Cantidad { get; set; }
        public double Unitario { get; set; }
        public double TasaIVA { get; set; }
        public double PorcentajeDescuento { get; set; }
    }
}
