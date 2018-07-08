using System.Collections.Generic;
using RGBelieve.Model;

namespace RGBelieve.Configuration
{
    public class Settings
    {
        #region Constants

        public const int CURRENT_VERSION = 1;

        #endregion

        #region Properties & Fields

        public int Version { get; set; } = 0;

        public double UpdateRate { get; set; } = 30.0;

        public List<SyncGroup> SyncGroups { get; set; } = new List<SyncGroup>();

        public List<AverageSyncGroup> AverageSyncGroups { get; set; } = new List<AverageSyncGroup>();

        public string HueBridgeIP { get; set; } = "192.168.50.3";

        public string HueKey { get; set; } = "q7GBsM1FPCKmpL0VK0xYhq1wzPoySz0QBJTLGsZ7";

        public string HueEntertainmentKey { get; set; } = "07ED912E9BC343A20985A7DCF53D1D47";

        public bool HueUseSimulator { get; set; } = false;

        #endregion
    }
}
