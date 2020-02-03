using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signature_XMLdesign
{
    class Program
    {
        static void Main(string[] args)
        {

            //string inFile = "C:\\Users\\parena\\Desktop\\tituloFirmado_SIN_FIRMA.xml";
            //string outFile = "C:\\Users\\parena\\Desktop\\tituloFirmado_CON_XMLDesign.xml";

            string inFile = "C:\\Users\\parena\\Desktop\\test_SIN_FIRMA.xml";
            string outFile = "C:\\Users\\parena\\Desktop\\test_CON_XMLDesign.xml";

            SignXML sign = new SignXML();
            sign.Firma_XML_Design(inFile,outFile);

            //Console.ReadKey();
        }
    }
}
