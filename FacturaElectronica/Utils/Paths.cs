using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FacturaElectronica.Utils
{
    public class Paths
    {
        public static List<string> getFile(string pattern, string location)
        {
            List<string> filename = new List<string>();
            if (!String.IsNullOrEmpty(pattern) && !String.IsNullOrEmpty(location))
            {
                Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                DirectoryInfo directorio = new DirectoryInfo(location);

                foreach (FileInfo archivo in directorio.GetFiles())
                {

                    if (regex.IsMatch(archivo.Name))
                    {
                        filename.Add(Path.Combine(location, archivo.Name));
                    }
                }

            }
            return filename;
        }
    }
}
