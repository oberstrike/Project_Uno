﻿using System;
using System.Collections.Generic;
using System.Linq;
using TWLibrary.Page;
using TWLibrary.VillageData;

namespace TWLibrary.Tools
{
    public interface IPlugin
    {

    }

    public interface IVillagePlugin : IPlugin
    {
        void Compute(Village village);
    }

    public interface IVillagesPlugin : IPlugin
    {
        void Compute(List<Village> villages);
    }


    public class PluginCouldntLoadException : Exception
    {
        public PluginCouldntLoadException(Type type) : base($"Das Plugin {type.Name} konnte nicht getsartet werden.")
        {

        }

    }
    [Obsolete("Idee")]
    public class ConquerPlugin : IVillagesPlugin
    {
        public void Compute(List<Village> villages)
        {
            List<Village> villagesWithAG = new List<Village>();

            foreach (Village village in villages)
            {
                double value = village.Units[Unit.SNOB];
                if (value > 0)
                    villagesWithAG.Add(village);
            }

            if (villagesWithAG.Count() > 0)
            {



            }


        }



        public class TradingPlugin : IVillagesPlugin
        {
            public void Compute(List<Village> villages)
            {
                foreach (Village village in villages)
                {
                    if (village.Pages.Where(each => each is MarketPage).Count() == 0)
                        throw new PluginCouldntLoadException(GetType());
                    village.RManager.GetMissingRessourcesForBuildings(village.GetBuildingsInBuildOrder());

                }

                IEnumerable<ResourcesManager> managers = villages.Select(each => each.RManager);

                string[] ressis = { "Wood", "Iron", "Stone" };
                foreach (string res in ressis)
                {
                    List<ResourcesManager> managersWithMore = new List<ResourcesManager>();
                    List<ResourcesManager> managersWithLess = new List<ResourcesManager>();

                    foreach (ResourcesManager manager in managers)
                    {
                        double value = (double)manager.GetType().GetProperty("Unused" + res).GetValue(manager);

                        if (value > 0)
                            managersWithMore.Add(manager);
                        else
                            managersWithLess.Add(manager);
                    }

                    if (managersWithMore.Count > 0)
                    {

                        double ueberschuss = managersWithMore.Sum(each => (double)each.GetType().GetProperty("Unused" + res).GetValue(each));
                        double fehlende = managersWithLess.Sum(each => (double)each.GetType().GetProperty("Unused" + res).GetValue(each));

                        if (managersWithLess.Count == 0)
                            break;


                        managersWithLess.Shuffle();
                        foreach (ResourcesManager manager in managersWithLess)
                        {
                            double dorfBenoetigteMaterialien = Math.Abs(Math.Round((double)manager.GetType().GetProperty("Unused" + res).GetValue(manager)));
                            if (dorfBenoetigteMaterialien == 0)
                                break;


                            if (managersWithMore.Count > 0)
                            {
                                foreach (ResourcesManager managerWithMore in managersWithMore)
                                {
                                    double dorfUebrigeMaterialien = Math.Round((double)managerWithMore.GetType().GetProperty("Unused" + res).GetValue(managerWithMore));
                                    Dictionary<string, double> resToSend = new Dictionary<string, double>();

                                    if (dorfBenoetigteMaterialien < dorfUebrigeMaterialien)
                                    {
                                        dorfUebrigeMaterialien = dorfBenoetigteMaterialien;
                                    }
                                    ueberschuss -= dorfUebrigeMaterialien;
                                    fehlende += dorfUebrigeMaterialien;
                                    resToSend.Add(res, dorfUebrigeMaterialien);

                                    bool succesfull = managerWithMore.MyVillage.SendRessourceToVillage(resToSend, manager.MyVillage);
                                    if (succesfull)
                                    {

                                        if (ueberschuss == 0 | fehlende == 0)
                                            break;
                                    }


                                }
                            }
                        }
                    }

                }


            }
        }
    }
}
