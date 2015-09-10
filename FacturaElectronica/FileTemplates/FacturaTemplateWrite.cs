using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace FacturaElectronica.FileTemplates
{
    [DelimitedRecord("|")]
    public class FacturaTemplateWrite
    {
        public string nro_comprobante_electronico;
        public string cae;
        public string cae_vencimiento;
    }
}
