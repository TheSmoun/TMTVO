using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class GridModule : Module
    {
        private static GridModule instance = null;

        public List<GridItem> GridPositions { get; private set; }

        public GridModule() : base("GridPositions")
        {
            this.GridPositions = new List<GridItem>();
            instance = this;
        }

        public GridItem FindDriver(Driver driver)
        {
            return GridPositions.Find(g => g.CarIndex == driver.CarIndex);
        }

        public GridItem FindDriver(LiveStandingsItem driver)
        {
            return GridPositions.Find(g => g.CarIndex == driver.Driver.CarIndex);
        }

        public GridItem FindDriver(int carIndex)
        {
            return GridPositions.Find(g => g.CarIndex == carIndex);
        }

        public static GridItem FindDriverStatic(Driver driver)
        {
            return instance.GridPositions.Find(g => g.CarIndex == driver.CarIndex);
        }

        public static GridItem FindDriverStatic(LiveStandingsItem driver)
        {
            return instance.GridPositions.Find(g => g.CarIndex == driver.Driver.CarIndex);
        }

        public static GridItem FindDriverStatic(int carIndex)
        {
            return instance.GridPositions.Find(g => g.CarIndex == carIndex);
        }

        public static GridItem GetLeader()
        {
            return instance.GridPositions.Find(g => g.Position == 1);
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            List<Dictionary<string, object>> qualiResults = rootNode.GetMapList("QualifyResultsInfo.Results");
            if (qualiResults == null)
                return;

            foreach (Dictionary<string, object> qualiResult in qualiResults)
            {
                string i = qualiResult.GetDictValue("CarIdx");
                if (i == null)
                    continue;

                int carIdx = int.Parse(i);

                GridItem item = GridPositions.Find(g => g.CarIndex == carIdx);
                if (item == null)
                {
                    item = new GridItem();
                    GridPositions.Add(item);
                }

                item.Update(qualiResult, api, this);
            }
        }

        public override void Reset()
        {
            this.GridPositions.Clear();
        }

        public GridItem this[int carIndex]
        {
            get
            {
                return GridPositions.Find(g => g.CarIndex == carIndex);
            }
        }
    }
}
