using System;
using System.Web.Http;
using System.Linq;
using CloudConfiguation.DB;
using CloudConfiguration.WebAPI.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudConfiguration.WebAPI.Models;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CloudConfiguration.WebAPI.Tests
{
    [TestClass]
    public class ConfigControllerTests
    {
        [TestMethod]
        public void ListTests()
        {
            using (var dbContext = new DatabaseContext())
            {
                dbContext.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                var isSQlError = false;
                try
                {
                    // dbContext.Database.BeginTransaction();
                    // Arrange 
                    var configRecord = this.EnsureRecordInTblconfiguration("UnitTesting");

                    // act
                    var configController = new ConfigurationController();
                    var getList = configController.List();

                    // assert
                    Assert.IsTrue(getList.ResponseStatus);

                    var newAddedRecord = getList.configurations.SingleOrDefault(f => f.Name == "UnitTesting");
                    Assert.IsNotNull(newAddedRecord);
                    Assert.AreEqual(configRecord.Id, newAddedRecord.Id);
                    Assert.AreEqual(configRecord.Version, newAddedRecord.Version);
                    Assert.AreEqual(configRecord.TemplatePath, newAddedRecord.TemplatePath);
                    Assert.AreEqual(configRecord.IsProduction, newAddedRecord.IsProduction);
                    Assert.AreEqual(configRecord.IsActive, newAddedRecord.IsActive);
                    Assert.AreEqual(configRecord.CreatedDate.Date, newAddedRecord.CreatedDate.Date);


                }
                catch (SqlException ex)
                {
                    isSQlError = true;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (!isSQlError)
                    {
                        var addedConfigRecord = dbContext.Configuration.FirstOrDefault(f => f.Name == "UnitTesting");
                        dbContext.Configuration.Remove(addedConfigRecord);
                        dbContext.SaveChanges();
                    }
                }

            }
        }

        [TestMethod]
        public void InsertUserBuilderMasterTests()
        {
            using (var dbContext = new DatabaseContext())
            {
                var isSQlError = false;
                try
                {
                    // Arrange 
                    var userbuild = new UserBuildMasterModel();
                    userbuild.UserId = 1;
                    userbuild.SessionId = "3334455";
                    userbuild.Template = "Unittesting -template2\\template.xml";
                    userbuild.BuildFile = "Unittesting -buildfiled2";
                    userbuild.CreatedDate = System.DateTime.Now;

                    // act
                    var configController = new ConfigurationController();
                    var getList = configController.InsertUserBuild(userbuild);

                    // assert
                    Assert.IsTrue(getList.ResponseStatus);

                    var buildRecord = dbContext.UserBuildMaster.Where(u => u.BuildFile == userbuild.BuildFile);
                    Assert.IsNotNull(buildRecord);
                    Assert.AreEqual(1, buildRecord.Count());

                    var record = buildRecord.First();
                    Assert.AreEqual(record.BuildFile, userbuild.BuildFile);
                    Assert.AreEqual(record.Template, userbuild.Template);
                    Assert.AreEqual(record.CreatedDate.Date, userbuild.CreatedDate.Date);
                    Assert.AreEqual(record.UserId, userbuild.UserId);
                }
                catch (SqlException ex)
                {
                    isSQlError = true;
                    // Handle the SQL Exception as you wish
                    Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (!isSQlError)
                    {
                        var userRecord = dbContext.UserBuildMaster.FirstOrDefault(u => u.BuildFile == "Unittesting -buildfiled2");
                        dbContext.UserBuildMaster.Remove(userRecord);
                        dbContext.SaveChanges();
                    }
                }
            }
        }

        [TestMethod]
        public void GetTemplateTests()
        {
            using (var dbContext = new DatabaseContext())
            {
                var isSQlError = false;
                var strConfigName = "TemplateUnitTesting";
                try
                {
                    // dbContext.Database.BeginTransaction();
                    // Arrange 
                    var configRecord = this.EnsureRecordInTblconfiguration(strConfigName);
                    var templateModel = new TemplateModel();
                    templateModel.templateId = configRecord.Id;
                    templateModel.templateName = configRecord.Name;
                    templateModel.isProduction = 2;

                    // act
                    var configController = new ConfigurationController();
                    var template = configController.GetTemplate(templateModel);

                    // assert
                    Assert.IsNotNull(template.templateContent);
                    Assert.IsTrue(template.ResponseStatus);
                    Assert.IsNull(template.Exception);
                }
                catch (SqlException ex)
                {
                    isSQlError = true;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (!isSQlError)
                    {
                        var addedConfigRecord = dbContext.Configuration.FirstOrDefault(f => f.Name == strConfigName);
                        if (addedConfigRecord != null)
                        {
                            dbContext.Configuration.Remove(addedConfigRecord);
                            dbContext.SaveChanges();
                        }
                    }
                }

            }
        }

        [TestMethod]
        public void SetUserBuild()
        {
            using (var dbContext = new DatabaseContext())
            {
                var isSQlError = false;
                var strConfigName = "TemplateUnitTesting";
                try
                {
                    // Arrange 
                    var configRecord = this.EnsureRecordInTblconfiguration(strConfigName);

                    var build = new BuildModel();
                    build.Template = configRecord.Name;
                    build.UserId = 2;
                    build.isProduction = configRecord.IsProduction;
                    build.programId = configRecord.Id;
                    // /TCPMPConfig/RDMClientProxy/Connections
                    // INSTALLPATH\\SignedIX\\SDVM\\TCPMP\\tcpmpconfig.xml
                    var writeXMLString = "{\"element\":\"Connection\",\"file\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\TCPMP\\\\tcpmpconfig.xml\"" +
                        ",\"xpath\":\"/TCPMPConfig/RDMClientProxy/Connections\",\"attributes\":[{\"attr\":[\"priority\",\"address\",\"portNumber\"]}" +
                        ",{\"attr\":[\"1\",\"192.168.2.51\",\"2111\"]}]}";
                    build.writeXMLTable = JsonConvert.DeserializeObject<WriteXMLTableModel>(writeXMLString);

                    var jsonString = "[{\"type\":\"copy\",\"index\":0,\"build\":{\"from\":\"LIBRARYPATH\\\\T7\\\\Firmware\",\"to\":\"INSTALLPATH\\\\Firmware\"}}" +
                        ",{\"type\":\"copy\",\"index\":1,\"build\":{\"from\":\"LIBRARYPATH\\\\T7\\\\Startup\",\"to\":\"INSTALLPATH\\\\Startup\"}}" +
                        ",{\"type\":\"copy\",\"index\":1,\"build\":{\"from\":\"LIBRARYPATH\\\\T7\\\\SignedIX\",\"to\":\"INSTALLPATH\\\\SignedIX\"}}" +
                        ",{\"type\":\"delete\",\"index\":2,\"build\":{\"path\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\testdelete.xml\"}}" +
                        ",{\"type\":\"writeXml\",\"index\":62,\"build\":{\"file\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\IXPayMain\\\\App.config\",\"value\":\"BlackOnWhiteqWXGA\",\"toCreate\":false,\"xpath\":\"/configuration/Display/add[@value='displayTheme']\"}}" +
                        ",{\"type\":\"writeXml\",\"index\":63,\"build\":{\"file\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\IXPayMain\\\\App.config\",\"value\":\"qXGA\",\"toCreate\":false,\"xpath\":\"/configuration/Display/add[@value='displayType']\"}}" +
                        ",{\"type\":\"move\",\"index\":525,\"build\":{\"from\":\"INSTALLPATH\\\\ServiceUtility\\\\STARTUP\",\"to\":\"INSTALLPATH\\\\STARTUP\"}}" +
                        ",{\"type\":\"writeXml\",\"index\":118,\"build\":{\"file\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\IXPayMain\\\\App.config\",\"value\":\"False\",\"toCreate\":true,\"xpath\":\"/configuration/Keypad/add[@value='showTouchKeypadButton']\"}}" +
                    //+ ",{\"type\":\"copy\",\"index\":121,\"build\":{\"from\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\IXPayMain\\\\App.config\",\"to\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\IXPayMain\\\\App.config\"}}" +
                        "]";
                    build.build = JsonConvert.DeserializeObject<List<MainBuildModel>>(jsonString);

                    // act
                    var configController = new ConfigurationController();
                    var template = configController.SetUserBuild(build);

                    // assert
                    Assert.IsTrue(template.ResponseStatus);
                    Assert.IsNull(template.Exception);
                    Assert.IsFalse(template.copy.Any(a=>a.Status == false));
                    Assert.IsFalse(template.writeXML.Any(a=>a.Status == false));
                }
                catch (SqlException ex)
                {
                    isSQlError = true;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (!isSQlError)
                    {
                        var addedConfigRecord = dbContext.Configuration.FirstOrDefault(f => f.Name == strConfigName);
                        dbContext.Configuration.Remove(addedConfigRecord);
                        dbContext.SaveChanges();
                    }
                }

            }
        }



        //[TestMethod]
        //public void XmlNodeTests()
        //{
        //    using (var dbContext = new DatabaseContext())
        //    {
        //        var isSQlError = false;
        //        var strConfigName = "TemplateUnitTesting";
        //        try
        //        {
        //            // Arrange 
        //            var configRecord = this.EnsureRecordInTblconfiguration(strConfigName);

        //            var build = new BuildModel();
        //            build.Template = configRecord.Name;
        //            build.UserId = 2;
        //            build.isProduction = configRecord.IsProduction;
        //            build.programId = configRecord.Id;
        //            // /TCPMPConfig/RDMClientProxy/Connections
        //            // INSTALLPATH\\SignedIX\\SDVM\\TCPMP\\tcpmpconfig.xml
        //            var writeXMLString = "{\"element\":\"Connection\",\"file\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\TCPMP\\\\tcpmpconfig.xml\"" +
        //                ",\"xpath\":\"/TCPMPConfig/RDMClientProxy/Connections\",\"attributes\":[{\"attr\":[\"priority\",\"address\",\"portNumber\"]}" +
        //                ",{\"attr\":[\"1\",\"192.168.2.51\",\"2111\"]}]}";
        //            build.writeXMLTable = JsonConvert.DeserializeObject<WriteXMLTableModel>(writeXMLString);

        //            var jsonString = "[" +
        //                ",{\"type\":\"writeXml\",\"index\":62,\"build\":{\"file\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\IXPayMain\\\\App.config\",\"value\":\"BlackOnWhiteqWXGA\",\"toCreate\":false,\"xpath\":\"/configuration/Display/add[@key='displayTheme']/@value\"}}" +
        //                ",{\"type\":\"writeXml\",\"index\":63,\"build\":{\"file\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\IXPayMain\\\\App.config\",\"value\":\"qXGA\",\"toCreate\":false,\"xpath\":\"/configuration/Display/add[@key='displayType']/@value\"}}" +
        //                ",{\"type\":\"writeXml\",\"index\":118,\"build\":{\"file\":\"INSTALLPATH\\\\SignedIX\\\\SDVM\\\\IXPayMain\\\\App.config\",\"value\":\"False\",\"toCreate\":true,\"xpath\":\"/configuration/Keypad/add[@key='showTouchKeypadButton']/@value\"}}" +
        //                "]";
        //            build.build = JsonConvert.DeserializeObject<List<MainBuildModel>>(jsonString);

        //            // act
        //            var configController = new ConfigurationController();
        //            var template = configController.SetUserBuild(build);

        //            // assert
        //            Assert.IsTrue(template.ResponseStatus);
        //            Assert.IsNull(template.Exception);


        //        }
        //        catch (SqlException ex)
        //        {
        //            isSQlError = true;
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //        finally
        //        {
        //            if (!isSQlError)
        //            {
        //                var addedConfigRecord = dbContext.Configuration.FirstOrDefault(f => f.Name == strConfigName);
        //                dbContext.Configuration.Remove(addedConfigRecord);
        //                dbContext.SaveChanges();
        //            }
        //        }

        //    }
        //}
        private TblConfiguration EnsureRecordInTblconfiguration(string prefix)
        {
            using (var dbContext = new DatabaseContext())
            {
                var toAdd = false;
                var configRecord = dbContext.Configuration.SingleOrDefault(f => f.Name == prefix);
                if (configRecord == null)
                {
                    toAdd = true;
                    configRecord = new TblConfiguration();
                }
                configRecord.Name = prefix;
                configRecord.Version = prefix + "-Version";
                configRecord.TemplatePath = prefix;
                configRecord.IsProduction = 2;
                configRecord.IsActive = true;
                configRecord.CreatedDate = System.DateTime.Now;

                if (toAdd)
                {
                    dbContext.Configuration.Add(configRecord);
                }

                dbContext.SaveChanges();

                return configRecord;
            }
        }
    }
}
