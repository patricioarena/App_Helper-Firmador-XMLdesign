using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Signature_XMLdesign
{
    public class SignXML
    {
        public SignXML()
        {
        }

        public void Firma_XML_Design(string inFile, string outFile)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;

                xmlDoc.Load(inFile);
                X509Certificate2 aCert = SelectCertificate();

                SignXml(xmlDoc, aCert);
                xmlDoc.Save(outFile); // Guardar automaticamente en el escritorio

                Console.WriteLine($"XML file signed. file saved in { outFile }");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // Sign an XML file. 
        // This document cannot be verified unless the verifying 
        // code has the key with which it was signed.
        private static void SignXml(XmlDocument xmlDoc, X509Certificate2 x509Certificate2)
        {
            RSACryptoServiceProvider encryptProvider = (RSACryptoServiceProvider)x509Certificate2.PrivateKey;

            // Check arguments. 
            if (xmlDoc == null)
                throw new ArgumentException("xmlDoc");
            if (encryptProvider == null)
                throw new ArgumentException("Key");

            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(xmlDoc);

            // Add the key to the SignedXml document.
            signedXml.SigningKey = encryptProvider;


            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate).
            KeyInfo keyInfo = new KeyInfo();

            KeyInfoX509Data clause = new KeyInfoX509Data();
            keyInfo.AddClause(new RSAKeyValue((RSA)encryptProvider));

            clause.AddIssuerSerial(x509Certificate2.IssuerName.Name, x509Certificate2.SerialNumber);
            clause.AddSubjectName(x509Certificate2.Subject);
            clause.AddCertificate(x509Certificate2);

            keyInfo.AddClause(clause);
            signedXml.KeyInfo = keyInfo;

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save 
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        }

        public static X509Certificate2 SelectCertificate(string message = null, string title = null)
        {
            X509Certificate2 cert = null;

            try
            {
                // Open the store of personal certificates.
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                if (string.IsNullOrEmpty(message))
                {
                    message = "Seleccione un certificado.";
                }

                if (string.IsNullOrEmpty(title))
                {
                    title = "Firmar archivo";
                }
                //A X509Certificate2UI window always on top
                IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
                X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, title, message, X509SelectionFlag.SingleSelection, windowHandle);

                if (scollection != null && scollection.Count == 1)
                {
                    cert = scollection[0];

                    if (cert.HasPrivateKey == false)
                    {
                        throw new Exception("El certificado no tiene asociada una clave privada.");
                    }
                }

                store.Close();
            }
            catch (Exception ex)
            {
                // Thx @rasputino
                throw new Exception("No se ha podido obtener la clave privada.", ex);
            }

            return cert;
        }

        private JObject verificarFirma_XMLdesign(XmlDocument xDoc)
        {
            XmlDocument document = xDoc;
            document.PreserveWhitespace = true;

            SignedXml signedXml = new SignedXml(document);
            XmlNodeList nodeList = document.GetElementsByTagName("Signature");

            // Load the signature node.
            signedXml.LoadXml((XmlElement)nodeList[0]);
            // Check the signature and return the result.
            bool IsValid = signedXml.CheckSignature();

            var x509data = signedXml.Signature.KeyInfo.OfType<KeyInfoX509Data>().First();
            X509Certificate2 x509Certificate2 = x509data.Certificates[0] as X509Certificate2;

            string Message = "La verificación de la firma no ha sido satisfactoria";
            if (IsValid)
                Message = "Verificación de la firma satisfactoria";

            JObject jObject = new JObject();
            jObject.Add("Subject", x509Certificate2.Subject);
            jObject.Add("IsValid", IsValid);
            jObject.Add("Message", Message);

            return jObject;
        }
    }
}