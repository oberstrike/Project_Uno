﻿using OpenQA.Selenium;
using SQLiteApplication.Tools;
using SQLiteApplication.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SQLiteApplication.Web
{
    [Obsolete("Veraltet")]
    public class AdvancedClient
    {
        /*    public AdvancedClient(Configuration configuration) : base(configuration)
            {
                Updaters.RemoveAll(x => x is TroopUpdater);
                Updaters.Add(new AdvancedTroopUpdater());
            }

            public override void Update(Client client)
            {
                var hasFarmmanager = (bool) Executor.ExecuteScript("return TribalWars.getGameData().features.FarmAssistent.active");
                if (hasFarmmanager)
                {
                    base.Update(client);
                }
                else
                {
                    Console.WriteLine("Farmassistent wird benötigt");
                    throw new Exception("Bot kann nicht gestartet.");
                }

            }

            internal class AdvancedTroopUpdater : Updater
            {
                public void Update(Client client)
                {
                    Sleep();
                    client.Driver.Navigate().GoToUrl(client.Creator.GetFarmAssist());
                    var units = (Dictionary<string, object>)client.Executor.ExecuteScript("return Accountmanager.farm.current_units");
                    Dictionary<string, double> nUnits = new Dictionary<string, double>();
                    foreach (var values in units)
                    {

                        nUnits.Add(values.Key, double.Parse((string)values.Value));
                    }
                    client.Config.Village.SetUnits(nUnits);

          */
    }
}
