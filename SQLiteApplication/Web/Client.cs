﻿using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SQLiteApplication.Tools;
using SQLiteApplication.UserData;
using SQLiteApplication.VillageData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SQLiteApplication.Web
{
    public class Client : Updater
    {
        public static void Sleep()
        {
            Thread.Sleep((new Random().Next(1, 5) * 1000) + 245);
        }

        private readonly List<Updater> _updaters = new List<Updater>()
        {
            new MainUpdater(), new MarketUpdater(), new MovementUpdater(), new SmithUpdater(), new TroopUpdater()
        };

        private IJavaScriptExecutor _executor;
        private Farmmanager _farmmanager;
        private bool _isConnected;
        private bool _isLoggedIn;
        private PathCreator _creator;
        private string _csrf;
        private readonly List<string> urls = new List<string>() { "https://www.die-staemme.de/" };
        public Process TorProcess { get; set; }
        private FirefoxOptions options;
        public List<Updater> Updaters => _updaters;

        public FirefoxDriver Driver { get; set; }

        public Client(Configuration configuration)
        {
            Config = configuration;
            options = new FirefoxOptions();

#if DEBUG

#else
            options.AddArgument("--headless");

#endif

            if (configuration.TorBrowserPath != null)
            {
                ConfigureAdvancedBrowser();

            }
        }

        private void ConfigureAdvancedBrowser()
        {
            Process[] localIds = Process.GetProcessesByName("tor");

            if (localIds.Length == 0)
            {

                TorProcess = new Process();
                TorProcess.StartInfo.FileName = Config.TorBrowserPath;
                TorProcess.StartInfo.Arguments = " - n";
                TorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                TorProcess.Start();
                Sleep();
            }
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("network.proxy.type", 1);
            profile.SetPreference("network.proxy.socks", "127.0.0.1");
            profile.SetPreference("network.proxy.socks_port", 9150);
            options.Profile = profile;
        }

        public void Connect()
        {
            try
            {
                options.SetLoggingPreference(LogType.Driver, LogLevel.Debug);
                Driver = new FirefoxDriver(options);
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                Executor = Driver;
                Driver.Navigate().GoToUrl(urls[0]);
                IsConnected = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Verbindung fehlgeschlagen.");
                Console.WriteLine(e.Message);
            }
        }

        public void Login()
        {
            if (IsConnected)
            {
                if (!Driver.Url.Equals(urls[0]))
                {
                    Driver.Navigate().GoToUrl(urls[0]);
                }
                bool contains = Driver.PageSource.Contains(Config.User.Name);
                if (!contains)
                {
                    Driver.FindElement(By.Id("user")).SendKeys(Config.User.Name);
                    Driver.FindElement(By.Id("password")).SendKeys(Config.User.Password);
                    Driver.FindElement(By.ClassName("btn-login")).Click();
                    Sleep();
                }


                Driver.FindElements(By.ClassName("world_button_active")).Where(each => each.Text.Contains(Config.User.Server.ToString())).First().Click();
                Sleep();
                if (Driver.Url != urls[0])
                {
                    WebDriverWait driverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
                    Config.Village.Id = GetVillageId();
                    Update(this);
                }
                IsLoggedIn = true;


            }

        }

        public List<Building> GetBuildings(Dictionary<string, object> keyValuePairs)
        {

            List<Building> newBuildings = new List<Building>();
            foreach (string key in keyValuePairs.Keys)
            {
                Dictionary<string, object> dictionary = (Dictionary<string, object>)keyValuePairs[key];

                string text = null;
                DateTime? dateTime = null;
                if (dictionary.ContainsKey("error"))
                {
                    text = (string)dictionary["error"];
                    if (text != null)
                    {
                        if (text.Length > 1 && text.Contains("Genug") && text.Contains("um"))
                        {
                            string date = text.Split(' ')[4];

                            dateTime = DateTime.Parse(date);
                            DateTime nowTime = DateTime.Now;

                            if (dateTime.Value.Ticks < nowTime.Ticks)
                            {
                                dateTime = dateTime.Value.AddDays(1).AddMinutes(2);
                            }
                        }
                    }

                }
                try
                {
                    newBuildings.Add(new BuildingBuilder()
                                .WithName(key)
                                .WithWood((long)dictionary["wood"])
                                .WithStone((long)dictionary["stone"])
                                .WithIron((long)dictionary["iron"])
                                .WithLevel(double.Parse((string)dictionary["level"]))
                                .WithPopulation((long)dictionary["pop"])
                                .WithMaxLevel((long)dictionary["max_level"])
                                .WithTargetLevel(Config.Village.MaxBuildings[key])
                                .WithBuildeable(text == null)
                                .Build());

                }
                catch
                {
                    Console.WriteLine(key + " wurde nicht gefunden"); Thread.Sleep((new Random().Next(1, 5) * 1000) + 245);
                }



            }
            newBuildings.ForEach(x => x.TimeToCanBuild = Config.Village.GetTimeToBuild(x));

            return newBuildings;
        }
        public void Logout()
        {
            Driver.Navigate().GoToUrl(Creator.GetLogout(Csrf));
            IsLoggedIn = false;
        }

        public void Close()
        {
            Driver.Close();
        }


        public void Build(Building building)
        {
            if (IsConnected && IsLoggedIn)
            {
                Sleep();
                GoTo(Creator.GetMain());
                if (Config.Village.CanConsume(building.Wood, building.Stone, building.Iron, building.NeededPopulation))
                {
                    Executor.ExecuteScript($"BuildingMain.build(\"{building.Name}\")");
                    Sleep();
                }
            }
        }
        #region Properties
        public Farmmanager Farmmanager { get => _farmmanager; set => _farmmanager = value; }
        public bool IsConnected { get => _isConnected; set => _isConnected = value; }
        public bool IsLoggedIn { get => _isLoggedIn; set => _isLoggedIn = value; }
        public PathCreator Creator { get => _creator; set => _creator = value; }
        public string Csrf { get => _csrf; set => _csrf = value; }
        public Configuration Config { get; set; }
        public IJavaScriptExecutor Executor { get => _executor; set => _executor = value; }

        #endregion


        public void Attack(Dictionary<string, double> units, string target)
        {
            Driver.Navigate().GoToUrl(Creator.GetAttackLink(target));
            foreach (KeyValuePair<string, double> kvp in units)
            {
                Driver.FindElement(By.Id("unit_input_" + kvp.Key)).SendKeys(kvp.Value.ToString());
                Sleep();
            }
            Driver.FindElement(By.Id("target_attack")).Click();
            Sleep();
            Driver.FindElement(By.Id("troop_confirm_go")).Click();

        }

        public void GoTo(string url)
        {
            if (Driver.Url != url)
            {
                Sleep();
                Driver.Navigate().GoToUrl(url);
            }
        }
        public double GetVillageId()
        {
            double id = 0;
            if (IsLoggedIn && IsConnected)
            {
                Sleep();
                object output = (Executor.ExecuteScript("return TribalWars.getGameData().village.id"));

                if (typeof(string).Equals(output.GetType()))
                {
                    double.TryParse((string)output, out id);
                }
                else
                {
                    id = (long)output;
                }
            }
            Creator = new PathCreator(Config.User.Server.ToString(), id.ToString());
            return id;

        }
        //Missing Incomings




        public void SendRessource(int wood, int stone, int iron, string targetId)
        {
            GoTo(Creator.GetMarketModeSend());

            IWebElement woodInput = Driver.FindElement(By.XPath("//input[@name='wood']"));
            IWebElement stoneInput = Driver.FindElement(By.XPath("//input[@name='stone']"));
            IWebElement ironInput = Driver.FindElement(By.XPath("//input[@name='iron']"));
            IWebElement targetInput = Driver.FindElement(By.XPath("//input[@placeholder='123|456']"));

            if (Config.Village.CanConsume(wood, stone, iron, 0))
            {
                woodInput.SendKeys(wood.ToString());
                stoneInput.SendKeys(stone.ToString());
                ironInput.SendKeys(iron.ToString());
                targetInput.SendKeys(targetId);
            }
        }

        public void ResearchTech(string techName)
        {
            GoTo(Creator.GetSmith());
            Dictionary<string, object> tech = (Dictionary<string, object>)Config.Village.Technologies[techName];
            double wood = (long)tech["wood"];
            double stone = (long)tech["stone"];
            double iron = (long)tech["iron"];
            if (Config.Village.CanConsume(wood, stone, iron, 0))
            {
                try
                {
                    bool t = (bool)tech["can_research"];
                    if (t)
                    {
                        Executor.ExecuteScript("BuildingSmith.research('" + techName + "');");
                    }
                }
                catch
                {

                }

            }

        }

        public void TrainUnits(Dictionary<string, double> units)
        {

            if (units.ContainsKey("spears") || units.ContainsKey("sword") || units.ContainsKey("axe"))
            {
                TrainUnitsInBarracks(units);
            }

            if (units.ContainsKey("spy") || units.ContainsKey("light") || units.ContainsKey("heavy"))
            {
                TrainUnitsInStable(units);
            }



        }

        private void TrainUnitsInBarracks(Dictionary<string, double> units)
        {
            GoTo(Creator.GetBarracks());

            IWebElement spearsInput = null;
            IWebElement swordInput = null;
            IWebElement axeInput = null;

            try
            {
                spearsInput = Driver.FindElementByXPath("//input[@id='spear_0']");
                swordInput = Driver.FindElementByXPath("//input[@id='sword_0']");
                axeInput = Driver.FindElementByXPath("//input[@id='axe_0']");
            }
            catch
            {

            }
            IWebElement trainBtn = Driver.FindElementByCssSelector(".btn.btn-recruit");


            if (units.ContainsKey("spears"))
            {
                double count = units["spears"];
                if (IsTrainable(count, "spears") && spearsInput != null)
                {
                    spearsInput.SendKeys(count.ToString());
                }

            }
            if (units.ContainsKey("sword"))
            {
                double count = units["sword"];
                if (IsTrainable(count, "sword") && swordInput != null)
                {

                    swordInput.SendKeys(units["sword"].ToString());
                }

            }
            if (units.ContainsKey("axe"))
            {
                {
                    double count = units["axe"];
                    if (IsTrainable(count, "axe") && axeInput != null)
                    {

                        axeInput.SendKeys(units["axe"].ToString());
                    }
                }
            }

            trainBtn.Click();
            Sleep();
        }

        private void TrainUnitsInStable(Dictionary<string, double> units)
        {
            GoTo(Creator.GetStable());
            IWebElement spysInput = null;
            IWebElement lightInput = null;
            IWebElement heavyInput = null;


            try
            {
                spysInput = Driver.FindElementByXPath("//input[@id='spy_0']");
                lightInput = Driver.FindElementByXPath("//input[@id='light_0']");
                heavyInput = Driver.FindElementByXPath("//input[@id='heavy_0']");

            }
            catch
            {

            }
            IWebElement trainBtn = Driver.FindElementByCssSelector(".btn.btn-recruit");


            if (units.ContainsKey("spy"))
            {
                double count = units["spy"];
                if (IsTrainable(count, "spy") && spysInput != null)
                {
                    spysInput.SendKeys(count.ToString());
                }

            }
            if (units.ContainsKey("light"))
            {
                double count = units["light"];
                if (IsTrainable(count, "light") && lightInput != null)
                {

                    lightInput.SendKeys(units["light"].ToString());
                }

            }
            if (units.ContainsKey("heavy"))
            {
                {
                    double count = units["axe"];
                    if (IsTrainable(count, "axe") && heavyInput != null)
                    {

                        heavyInput.SendKeys(units["axe"].ToString());
                    }
                }
            }
            trainBtn.Click();
            Sleep();

        }

        private bool IsTrainable(double count, string name)
        {
            return Config.Village.CanConsume(Village.unit_Prices[name]["wood"] * count, Village.unit_Prices[name]["iron"] * count, Village.unit_Prices[name]["stone"] * count, Village.unit_Prices[name]["population"] * count);

        }

        public virtual void Update(Client client)
        {
            foreach (Updater updater in Updaters)
            {
                updater.Update(this);
                Sleep();
            }
        }
    }
}
