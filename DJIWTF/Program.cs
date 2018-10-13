using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DJIWTF
{
    class Program
    {
        private enum FlyDirections
        {
            Up,
            Down,
            Left,
            Right,
            Forward,
            Back
        }
        static void Main(string[] args)
        {
            Console.Title = "DJI WTF";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Program prog = new Program();
            Console.WriteLine("[n] normal startup\n[tm] test middleware\n[mc] manual control\n[ct] config test\n[cto] config test online\n");
            String msg = Console.ReadLine();
            if (msg.ToLower().Equals("n"))
            {
                prog.MainMenu();
            }else if (msg.ToLower().Equals("tm"))
            {
                prog.TestMiddlewareConnection();
            }else if (msg.ToLower().Equals("mc"))
            {
                prog.ManualControl();
            }else if (msg.ToLower().Equals("ct"))
            {
                prog.TestConfig();
            }else if (msg.ToLower().Equals("cto"))
            {
                prog.TestConfigOnline();
            }
            
        }

        public void TestConfig()
        {
            BasicControl bc = new BasicControl(@"C:\temp\djiwtf\", 250, false);
            bc.TestConfig();
            Console.Write("Press any key...");
            Console.ReadKey();
        }

        public void TestConfigOnline()
        {
            BasicControl bc = new BasicControl(@"C:\temp\djiwtf\", 250, true);
            bc.TestConfigOnline();
            Console.Write("Press any key...");
            Console.ReadKey();
        }

        public void TestMiddlewareConnection()
        {
            Console.Title = "DJI WTF - Middleware Test";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            MiddlewareConnector con = new MiddlewareConnector("https://dronevision.azurewebsites.net");
            string runId = "123test";
            string flightData = "no flight data";
            JsonConfigurationResponse configData = new JsonConfigurationResponse();
            configData.Id = "";
            configData.Data = null;
            JsonIdName device = new JsonIdName();
            device.Id = "";
            JsonIdName storage = new JsonIdName();
            storage.Id = "";

            try
            {
                Console.WriteLine("Test Get Devices");
                string devices = con.GetDevices();
                Console.WriteLine("Test Get Devices successful");
                Console.WriteLine("Response Content: \n" + devices);
                device = JsonConvert.DeserializeObject<List<JsonIdName>>(devices)[0];
            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                Console.WriteLine("Test Get Storage");
                string storages = con.GetStorages();
                Console.WriteLine("Test Get Storage successful");
                Console.WriteLine("Response Content: " + storages);
                storage = JsonConvert.DeserializeObject<List<JsonIdName>>(storages)[0];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                Console.WriteLine("Test Get Storage");
                string configurations = con.GetConfigurations(device.Id, storage.Id);
                Console.WriteLine("Test Get Storage successful");
                Console.WriteLine("Response Content: " + configurations);
                configData = JsonConvert.DeserializeObject<List<JsonConfigurationResponse>>(configurations)[0];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                Console.WriteLine("Test Post Run");
                string run = con.PostRun(device.Id, storage.Id, configData.Id);
                Console.WriteLine("Test Post Run successful");
                Console.WriteLine("Response Content: " + run);
                runId = run;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                Console.WriteLine("Test Patch Run");
                con.PatchRun(runId);
                Console.WriteLine("Test Patch Run successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                Console.WriteLine("Test Get Sequence");
                string getRunData = con.GetSequence(runId);
                Console.WriteLine("Test Get Sequence successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            try
            {
                Console.WriteLine("Test Post Image");
                con.PostImage(runId, "test", "image/jpg", @"C:\temp\djiwtf\dog.jpg");
                Console.WriteLine("Test Post Image successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.Write("\n\nPress any Key...");
            Console.ReadKey();
        }

        public void MainMenu()
        {
            Console.Title = "DJI WTF - Standard Operation";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            String msg = "";
            MiddlewareConnector con = new MiddlewareConnector("https://dronevision.azurewebsites.net");
            JsonIdName device = new JsonIdName();
            device.Id = "";
            device.Name = "";
            bool bDevice = false;
            JsonIdName storage = new JsonIdName();
            storage.Id = "";
            storage.Name = "";
            bool bStorage = false;
            JsonConfigurationResponse config = new JsonConfigurationResponse();
            config.Id = "";
            config.Data = new DroneConfiguration();
            bool bConfig = false;


            while (true)
            {
                try
                {
                    Console.WriteLine("\n\n#####\tDRONE VISION\t#####\n\n[init]\n[run]\n[quit]\n");
                    msg = Console.ReadLine();
                    if (msg.ToLower().Equals("init"))
                    {
                        Console.WriteLine("Init...");
                        bDevice = false;
                        bStorage = false;
                        bConfig = false;
                        Console.WriteLine("Load Devices");
                        List<JsonIdName> deviceList = JsonConvert.DeserializeObject<List<JsonIdName>>(con.GetDevices());
                        Console.WriteLine("Load Storages");
                        List<JsonIdName> storageList = JsonConvert.DeserializeObject<List<JsonIdName>>(con.GetStorages());
                        if (deviceList.Count == 1)
                        {
                            device = deviceList[0];
                            Console.WriteLine("One device found; set device ID: " + device.Id);
                            bDevice = true;
                        }
                        else if (deviceList.Count > 1)
                        {
                            Console.WriteLine("Haut den typen, der noch ein weiteres DEVICE hinzugefuegt hat!!!");
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("Error while initialize system; no device found");
                            continue;
                        }
                        if (storageList.Count == 1)
                        {
                            storage = storageList[0];
                            Console.WriteLine("One storage found; set storage ID: " + storage.Id);
                            bStorage = true;
                        }
                        else if (storageList.Count > 1)
                        {
                            Console.WriteLine("Haut den typen, der noch ein weiteren STORAGE hinzugefuegt hat!!!");
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("Error while initialize system; no storage found");
                            continue;
                        }
                        if (bDevice && bStorage)
                        {
                            Console.WriteLine("Load Configurations");
                            List<JsonConfigurationResponse> configurationList = JsonConvert.DeserializeObject<List<JsonConfigurationResponse>>(con.GetConfigurations(device.Id, storage.Id));
                            if (configurationList.Count == 1)
                            {
                                config = configurationList[0];
                                Console.WriteLine("One configuration found; set configuration ID: " + config.Id);
                                bConfig = true;
                            }
                            else if (configurationList.Count > 1)
                            {
                                Console.WriteLine("Haut den typen, der noch ein weitere CONFIGURATION hinzugefuegt hat!!!");
                                continue;
                            }
                            else
                            {
                                Console.WriteLine("Error while initialize system; no configuration found");
                                continue;
                            }
                            if (bConfig)
                            {
                                Console.WriteLine("Init done successful");
                            }
                        }

                    }
                    else if (msg.ToLower().Equals("run"))
                    {
                        if (bConfig && bDevice && bStorage)
                        {
                            string runId = con.PostRun(device.Id, storage.Id, config.Id);
                            BasicControl control = new BasicControl(@"C:\temp\djiwtf\", 250, true);
                            control.SetConfiguration(config.Data);
                            control.ExecuteScript(runId);
                            //Task script = control.ExecuteScriptAsync(runId);
                            //while (!script.IsCompleted)
                            //{
                            //    Console.WriteLine("Script is running!");
                            //    Thread.Sleep(1000);
                            //}
                            DirectoryInfo d = new DirectoryInfo(@"C:\temp\djiwtf");
                            FileInfo[] files = d.GetFiles("*.jpg");
                            foreach(FileInfo f in files)
                            {
                                con.PostImage(runId, f.Name, "image/jpg", f.FullName);
                            }
                            con.PatchRun(runId);
                        }
                        else
                        {
                            Console.WriteLine("Cannot start run; configuration not done (execute init)");
                        }
                    }
                    else if (msg.ToLower().Equals("quit"))
                    {
                        break;
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                
            }
            Console.WriteLine("Shutdown Software");
        }

        public void ManualControl()
        {
            Console.Title = "DJI WTF - Manual Operation";
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.WriteLine("ATTENTION: MANUAL OPERATION");
            BasicControl bc = new BasicControl(@"C:\temp\djiwtf\", 250, false);
            bc.ManualControl();
        }

    }
}
