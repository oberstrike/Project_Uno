﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using SQLiteApplication.Tools;
using SQLiteApplication.Web;

namespace SQLiteApplication.PagesData
{
    class AttackPage : Page
    {
        public AttackPage(Village village, FirefoxDriver driver) : base(village, driver)
        {
        }

        public override List<Updater> Updaters => throw new NotImplementedException();

        public void Attack(Dictionary<string, double> units, string target, int villageId)
        {
            GoTo(Driver, target);

            foreach (KeyValuePair<string, double> kvp in units)
            {
                Driver.FindElement(By.Id("unit_input_" + kvp.Key)).SendKeys(kvp.Value.ToString());
                Client.Sleep();
            }
            Driver.FindElement(By.Id("target_attack")).Click();
            Client.Sleep();
            Driver.FindElement(By.Id("troop_confirm_go")).Click();

        }

        public override string Url()
        {
            return base.Url() + "&screen=place&target=";
        }
    }
}
