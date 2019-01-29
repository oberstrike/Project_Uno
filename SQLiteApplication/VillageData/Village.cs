﻿using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using SQLiteApplication.Page;
using SQLiteApplication.Tools;
using SQLiteApplication.UserData;
using SQLiteApplication.VillageData;
using SQLiteApplication.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLiteApplication
{
    public sealed class Village
    {
        public Village(double id, double serverId, IWebDriver driver, User user )
        {
            Id = id;
            ServerId = serverId;
            pathCreator = new PathCreator(serverId.ToString(), id.ToString());
            Driver = driver;
            MyUser = user;
            Pages =  new List<AbstractPage>() { };
        }

        public static List<string> buildOrder = new List<string>() { "wood","stone","iron"};


        #region PROPERTIES    
        public User MyUser { get; set; }
        public List<AbstractPage> Pages { get; set; } 
        public ICollection<Building> Buildings { get; set; }
        public IWebDriver Driver { get; set; }
        public Dictionary<string, int> MaxBuildings { get; set; }
        public double Id { get; set; }
        public double ServerId { get; set; }
        public ResourcesManager RManager { get; set; }
        public TradeManager TManager { get; set; } = new TradeManager();
        public IList<KeyValuePair<string, TimeSpan>> BuildingsInQueue { get; set; }
        public ICollection<TroupMovement> OutcomingTroops { get; set; }
        public Dictionary<string, object> Technologies { get; set; }
        public Dictionary<Unit, double> Units { get; set; }
        public IList<string> GetAttackedVillages()
        {
            return OutcomingTroops.Select(x => x.TargetId).ToList();
        }
        public PathCreator pathCreator { get; set; }
        public string Csrf { get; internal set; }
        public string Coordinates { get; set; }
        public string Name { get; set; }
        public string[] FarmingVillages { get; set; }
        #endregion

        #region METHODS
        public bool CanConsume(double wood, double stone, double iron, double population)
        {
            return RManager.CanConsume(wood, stone, iron, population);
        }

        public Building GetNextBuilding()
        {
            var buildings = GetBuildingsInBuildOrder().OrderBy(each => !each.IsBuildeable);
            if (buildings.Count() > 0)
                return buildings.First();
            else
                return null;
        }

        public bool CanConsume(Unit unit)
        {
            return CanConsume(unit.GetWood(), unit.GetStone(), unit.GetIron(), unit.GetNeededPopulation());
        }

        public bool CanConsume(Building building)
        {
            return CanConsume(building.Wood, building.Stone, building.Iron, building.NeededPopulation);
        }
        public Building GetBuilding(string name)
        {
            return (from building in Buildings
                    where building.Name.Equals(name)
                    select building).First();
        }

        public void Update()
        {
            foreach (AbstractPage page in Pages)
            {
                page.Update();
                Client.Sleep();
            }
        }

        public void Build(string name)
        {
            Build(GetBuilding(name));
        }
        public void Build(Building building)
        {
            int queueCount = BuildingsInQueue.Count();
            int maxQueueCount = MyUser.IsPremium ? 4 : 2;

            if(queueCount < maxQueueCount)
            {
                MainPage page = Pages.Where(each => each is MainPage).First() as MainPage;
                Driver.GoTo(page.URL);
                page.Build(building);
                Client.Sleep();
                Client.Print(DateTime.Now + " " + building.Name + " wird in " + Id + " ausgebaut");
            }

        }

        public void Train(Dictionary<Unit, double> units)
        {
            BarracksPage page = (BarracksPage)Pages.Where(each => each is BarracksPage).First();

            foreach (KeyValuePair<Unit, double> kvp in units)
            {
                string name = kvp.Key.GetName();
                page.Train(kvp.Key, kvp.Value);
            }
        }

        public bool SendRessourceToVillage(Dictionary<string, double> resources, Village village)
        {
            MarketPage page = Pages.Where(each => each is MarketPage).First() as MarketPage;
            double wood = 0;
            double stone = 0;
            double iron = 0;

            if(resources.ContainsKey("Wood"))
                wood = resources["Wood"];
            if (resources.ContainsKey("Stone"))
                stone = resources["Stone"];
            if (resources.ContainsKey("Iron"))
                iron = resources["Iron"];
            var traders = Math.Round((wood + stone + iron)/1000 + 0.5);

            if(wood < RManager.Wood && stone < RManager.Stone && iron < RManager.Iron && TManager.AvailableTraders >= traders)
            {
                return page.SendRessource(wood, stone, iron, village.Coordinates);
            }
            return false;
        }

        public IEnumerable<Building> GetBuildingsInBuildOrder()
        {
            return Buildings.Where(each => {
                return buildOrder.Contains(each.Name) && (each.TimeToCanBuild != TimeSpan.Zero || each.IsBuildeable);
            }).Select(each =>
            {
                return each;
            });

        }

        #endregion

        #region OPERATORS
        public override bool Equals(object obj)
        {
            Village village = obj as Village;
            if (village != null)
                return village.Id == this.Id;

            return false;
        }
        public override string ToString()
        {
            return $"Wood: {RManager.Wood}, Stone:  {RManager.Stone}, Iron: {RManager.Iron}, {Buildings} " +
                $"\nWood Production: {RManager.WoodProduction}, Stone Production: {RManager.StoneProduction}, Iron Production: {RManager.IronProduction}";
        }
        public override int GetHashCode()
        {
            return 2108858624 + Id.GetHashCode();
        }
        public static bool operator ==(Village village1, Village village2)
        {
            return EqualityComparer<Village>.Default.Equals(village1, village2);
        }
        public static bool operator !=(Village village1, Village village2)
        {
            return !(village1 == village2);
        }
        #endregion

    }
}
