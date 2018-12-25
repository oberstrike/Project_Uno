﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using SQLiteApplication.Web;

namespace SQLiteApplication.Tools
{
    public class MarketUpdater : IUpdater { 
    
        public void Update(FirefoxDriver driver, Village village)
        {
            var available = driver.FindElement(By.Id("market_merchant_available_count")).Text;
            var total = driver.FindElement(By.Id("market_merchant_total_count")).Text;
            village.TManager.AvailableTraders = int.Parse(available);
            village.TManager.TotalTraders = int.Parse(total);
        }
    }
}
