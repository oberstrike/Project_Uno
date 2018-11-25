﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteApplication.Web
{
    public class PathCreator
    {
        public string ServerId { get; set; }
        public string VillageId { get; set; }

        private string _basePath;

        public PathCreator(string pServerId, string pVillageId)
        {
            ServerId = pServerId;
            VillageId = pVillageId;
            _basePath = $"https://de{ServerId}.die-staemme.de/game.php?";

        }

        public string GetMain()
        {
            return $"{_basePath}village={ServerId}&screen=main";
        }

        public string GetPlace()
        {
            return $"{_basePath}village={VillageId}&screen=place&mode=command";
        }

        public string GetLogout(string csrf)
        {
            return $"{_basePath}village={VillageId}&screen=&action=logout&h={csrf}";
        }

        public string GetAttackLink(string target)
        {
            return $"{_basePath}village={VillageId}&screen=place&target={target}";
        }

        public string GetFarmAssist()
        {
            return $"https://de{ServerId}.die-staemme.de/game.php?village={VillageId}&screen=am_farm";
        }

        public string GetBuildingOverview()
        {
            return $"https://de{ServerId}.die-staemme.de/game.php?village={VillageId}&screen=overview_villages";
        }

        public string GetMarketModeSend()
        {
            return $"https://de{ServerId}.die-staemme.de/game.php?village={VillageId}&screen=market&mode=send";
        }

        public string GetBarracks()
        {
            return $"https://de{ServerId}.die-staemme.de/game.php?village={VillageId}&screen=barracks";
        }

        internal string GetStable()
        {
            return $"https://de{ServerId}.die-staemme.de/game.php?village={VillageId}&screen=stable";
        }

        internal string GetSmith()
        {
            return $"https://de{ServerId}.die-staemme.de/game.php?village={VillageId}&screen=smith";

        }
    }
}
