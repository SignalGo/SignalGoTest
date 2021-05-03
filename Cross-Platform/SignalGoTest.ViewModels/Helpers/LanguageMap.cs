extern alias SignalGoCodeGenerator;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using SignalGoTest.ViewModels.Models;
using SignalGoCodeGenerator.SignalGo.CodeGenerator.Helpers;
using SignalGoCodeGenerator.SignalGo.CodeGenerator.Models;
using SignalGoCodeGenerator.SignalGo.CodeGenerator.LanguageMaps;
using SignalGo.Shared.Models.ServiceReference;

namespace SignalGoTest.ViewModels.Helpers
{
    public class LanguageMap : LanguageMapBase
    {
        private static LanguageMap _Current = null;
        public static LanguageMap Current
        {
            get
            {
                if (_Current == null)
                    GetCurrent = _Current = new LanguageMap();
                return _Current;
            }
        }

        public override string DownloadService(string servicePath, AddReferenceConfigInfo config)
        {
            string fullFilePath = "";
            if (config.ServiceType == 0)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(config.ServiceUrl);
                webRequest.ContentType = "SignalGo Service Reference";
                webRequest.Headers.Add("servicenamespace", config.ServiceNameSpace);
                webRequest.Headers.Add("selectedLanguage", config.LanguageType.ToString());
                WebResponse response = webRequest.GetResponse();
                if (response.ContentLength <= 0)
                    throw new Exception("Url ContentLength is not set!");
                else if (response.Headers["Service-Type"] == null || response.Headers["Service-Type"] != "SignalGoServiceType")
                    throw new Exception("Url file type is not support!");
                Stream stream = response.GetResponseStream();

                using (MemoryStream streamWriter = new MemoryStream())
                {
                    streamWriter.SetLength(0);
                    byte[] bytes = new byte[1024 * 10];
                    while (streamWriter.Length != response.ContentLength)
                    {
                        int readCount = stream.Read(bytes, 0, bytes.Length);
                        if (readCount <= 0)
                            break;
                        streamWriter.Write(bytes, 0, readCount);
                    }
                    string json = Encoding.UTF8.GetString(streamWriter.ToArray());
                    //var namespaceReferenceInfo = (NamespaceReferenceInfo)JsonConvert.DeserializeObject(json, typeof(NamespaceReferenceInfo), new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new DataExchangeConverter(LimitExchangeType.IncomingCall) { Server = null, Client = null, IsEnabledReferenceResolver = true, IsEnabledReferenceResolverForArray = true } }, Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });
                    var namespaceReferenceInfo = (SignalGoCodeGenerator.SignalGo.Shared.Models.ServiceReference.NamespaceReferenceInfo)JsonConvert.DeserializeObject(json, typeof(SignalGoCodeGenerator.SignalGo.Shared.Models.ServiceReference.NamespaceReferenceInfo), new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });

                    //csharp
                    if (config.LanguageType == 0)
                    {
                        fullFilePath = Path.Combine(servicePath, "Reference.cs");
                        File.WriteAllText(fullFilePath, CsharpLanguageMap.CalculateMapData(namespaceReferenceInfo, config), Encoding.UTF8);
                    }
                    //angular
                    else if (config.LanguageType == 1)
                    {
                        //string oldPath = Path.Combine(servicePath, "OldAngular");
                        //string newPath = Path.Combine(servicePath, "NewAngular");
                        //fullFilePath = Path.Combine(oldPath, "Reference.ts");
                        //if (!Directory.Exists(oldPath))
                        //    Directory.CreateDirectory(oldPath);
                        //TypeScriptLanguageMap typeScriptLanguageMap = new TypeScriptLanguageMap();
                        //File.WriteAllText(fullFilePath, typeScriptLanguageMap.CalculateMapData(oldPath, namespaceReferenceInfo, config.ServiceNameSpace), Encoding.UTF8);

                        AngularTypeScriptLanguageMap angularTypeScriptLanguageMap = new AngularTypeScriptLanguageMap();
                        angularTypeScriptLanguageMap.CalculateMapData(servicePath, namespaceReferenceInfo, config.ServiceNameSpace);

                    }
                    //blazor
                    else if (config.LanguageType == 2)
                    {
                        fullFilePath = Path.Combine(servicePath, "Reference.cs");
                        File.WriteAllText(fullFilePath, BlazorLanguageMap.CalculateMapData(namespaceReferenceInfo, config.ServiceNameSpace, config), Encoding.UTF8);
                    }
                    //java android
                    else if (config.LanguageType == 3)
                    {
                        JavaAndroidLanguageMap javaAndroidLanguageMap = new JavaAndroidLanguageMap();
                        javaAndroidLanguageMap.CalculateMapData(servicePath, namespaceReferenceInfo, config.ServiceNameSpace);
                    }
                    //swift
                    else if (config.LanguageType == 4)
                    {
                        SwiftLanguageMap swiftLanguageMap = new SwiftLanguageMap();
                        swiftLanguageMap.CalculateMapData(servicePath, namespaceReferenceInfo, config.ServiceNameSpace);
                    }
                    //flutter
                    else if (config.LanguageType == 5)
                    {
                        DartFlutterLanguageMap flutterLanguageMap = new DartFlutterLanguageMap();
                        flutterLanguageMap.CalculateMapData(servicePath, namespaceReferenceInfo, config.ServiceNameSpace);
                    }
                }
            }
            else
            {
                if (config.LanguageType > 0)
                    throw new NotSupportedException("this language for this type not supported now!");
                XMLToCsharp2 xmlCsharp = new XMLToCsharp2();
                xmlCsharp.Generate(config.ServiceUrl);
                string csharpCode = xmlCsharp.GeneratesharpCode(config.ServiceNameSpace);
                fullFilePath = Path.Combine(servicePath, "Reference.cs");
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"namespace {config.ServiceNameSpace}");
                builder.AppendLine("{");
                builder.AppendLine(csharpCode);
                builder.AppendLine("}");
                File.WriteAllText(fullFilePath, builder.ToString(), Encoding.UTF8);
            }
            return fullFilePath;
        }

        public override ProjectInfoBase GetActiveProject()
        {
            return new ProjectInfo() {   ProjectItemsInfoBase  = new ProjectItemsInfo() {  } };
        }

        public override List<ProjectItemInfoBase> GetAllProjectItemsWithoutServices(ProjectItemsInfoBase projectBase)
        {
            ProjectItemsInfo project = projectBase as ProjectItemsInfo;
            List<ProjectItemInfoBase> result = new List<ProjectItemInfoBase>();
            return result;
        }

        public override string GetAutoGeneratedText()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            return $@"// AUTO GENERATED
//     This code was generated by signalgo test add refenreces.
//     Runtime Version : {assembly.GetName().Version.ToString()}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//     to download signalgo vsix for visual studio go https://marketplace.visualstudio.com/items?itemName=AliVisualStudio.SignalGoExtension
//     support and use signalgo go https://github.com/SignalGo/SignalGo-full-net
// AUTO GENERATED";
        }
    }
}
