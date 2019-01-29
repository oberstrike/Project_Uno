﻿using OpenQA.Selenium.Firefox;
using SQLiteApplication.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteApplication.Updaters
{
    class FarmassistUpdater : IUpdater
    {
        public void Update(Village village)
        {
            var dic = (Dictionary<string,object>)village.Driver.ExecuteScript("return Accountmanager.farm.current_units");
            var units = new Dictionary<Unit, double>();


            foreach(var kvp in dic)
            {
                var name = kvp.Key;
                var count = kvp.Value;
                Unit unit = (Unit) Enum.Parse(typeof(Unit), name.ToUpper());
                units.Add(unit, double.Parse((string) count));

            }
            village.Units = units;
        }
        
    }
}
