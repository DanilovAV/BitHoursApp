﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BitHoursApp.Updater.Core
{
    public interface IApplicationInfoLoader
    {
        Task<IEnumerable<ApplicationInfo>> LoadApplicationInfoAsync(string pathToUpdatingData);
    }
    
    public class HttpApplicationInfoLoader : IApplicationInfoLoader
    {
        private const string xsdResource = "BitHoursApp.Updater.Core.UpdaterDataScheme.xsd";

        public async Task<IEnumerable<ApplicationInfo>> LoadApplicationInfoAsync(string pathToUpdatingData)
        {            
            //Get string from HTTP 
            var appInfoString = await GetApplicationInfoString(pathToUpdatingData);
     
            //Parse HTTP response in XmlDocument
            var xmlDocument = await GetXmlDocumentAsync(appInfoString);
         
            //Deserialize xml and get data
            return GetApplicationInfo(xmlDocument);            
        }

        protected virtual async Task<string> GetApplicationInfoString(string pathToUpdatingData)
        {
            String xmlString = null;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    Uri requestUri = new Uri(pathToUpdatingData);
                    xmlString = await httpClient.GetStringAsync(requestUri);
                }
                catch (ArgumentNullException)
                {
                    throw new UpdaterException(UpdaterError.InvalidAppInfoPath);
                }
                catch (UriFormatException)
                {
                    throw new UpdaterException(UpdaterError.InvalidAppInfoPath);
                }
                catch (Exception)
                {
                    throw new UpdaterException(UpdaterError.AppInfoLoadingFailed);                 
                }
            }

            return xmlString;
        }

        protected virtual Task<XmlDocument> GetXmlDocumentAsync(string xml)
        {
            return Task<XmlDocument>.Run(() =>
            {
                var assembly = Assembly.GetExecutingAssembly();

                XmlSchema xs = null;
            
                using (Stream stream = assembly.GetManifestResourceStream(xsdResource))
                {
                    xs = XmlSchema.Read(stream, (s, e) =>
                    {
                        if (e.Severity == XmlSeverityType.Error)
                            throw new Exception("Invalid xsd format");
                    });
                }

                XmlSchemaSet xss = new XmlSchemaSet();
                xss.Add(xs);

                XmlDocument xdoc = new XmlDocument();
                xdoc.Schemas.Add(xss);

                try
                {
                    xdoc.LoadXml(xml);
                    xdoc.Validate(null);
                }
                catch (XmlException)
                {
                    throw new UpdaterException(UpdaterError.InvalidXmlFormat);                                    
                }
                catch (XmlSchemaValidationException)
                {
                    throw new UpdaterException(UpdaterError.InvalidXmlFormat);                                    
                }
                catch (Exception)
                {
                    throw new UpdaterException(UpdaterError.Unexpected);                                    
                }

                return xdoc;
            });
        }

        protected virtual IEnumerable<ApplicationInfo> GetApplicationInfo(XmlDocument xmlDocument)
        {
            using (var stream = new MemoryStream())
            {
                XmlWriterSettings xwSettings = new XmlWriterSettings();
                xwSettings.Encoding = Encoding.UTF8;

                using (XmlWriter xw = XmlWriter.Create(stream, xwSettings))
                {
                    xmlDocument.WriteContentTo(xw);
                    xw.Flush();

                    stream.Position = 0;

                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ApplicationsInfo));

                        var applicationsInfo = (ApplicationsInfo)serializer.Deserialize(stream);
                        return applicationsInfo.ApplicationInfo;
                    }
                    catch (Exception)
                    {
                        throw new UpdaterException(UpdaterError.DeserializingFailed);                                     
                    }
                }
            }
        }
    }
}