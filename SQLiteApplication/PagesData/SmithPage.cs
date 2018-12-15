﻿using OpenQA.Selenium.Firefox;
using SQLiteApplication.Tools;
using SQLiteApplication.VillageData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteApplication.PagesData
{
    public class SmithPage : AbstractBuildingPage
    {

        public SmithPage(Village village, FirefoxDriver driver) : base(village, driver)
        {

        }

        public override List<AbstractUpdater> Updaters => new List<AbstractUpdater>() { new SmithUpdater() };

        public override BuildingEnum MyBuilding => BuildingEnum.SMITH;

        public override string Url()
        {
            return base.Url() + "&screen=smith";
        }

        public void ResearchTech(string techName, int villageId)
        {
            GoTo();
            Dictionary<string, object> tech = (Dictionary<string, object>)PageVillage.Technologies[techName];
            double wood = (long)tech["wood"];
            double stone = (long)tech["stone"];
            double iron = (long)tech["iron"];
            if (PageVillage.CanConsume(wood, stone, iron, 0))
            {
                try
                {
                    bool t = (bool)tech["can_research"];
                    if (t)
                    {
                        Driver.ExecuteScript("BuildingSmith.research('" + techName + "');");
                    }
                }
                catch
                {

                }

            }

        }

    }
}
