using System;
using System.Collections.Generic;
using Q42.HueApi.Streaming;
using Q42.HueApi.Streaming.Models;
using System.Threading.Tasks;
using RGBelieve.Configuration;
using System.Linq;

namespace RGBelieve.Hue
{
    class StreamingSetup
    {
        public static StreamingGroup StreamingGroup { get; set; }
        public static StreamingHueClient StreamingHueClient { get; set; }
        public static List<EntertainmentLayer> Layers { get; set; }
        private static int BPM { get; set; } = 120;
        public static Ref<TimeSpan?> WaitTime { get; set; } = TimeSpan.FromMilliseconds(500);

        private static string _groupId;

        public static async Task<StreamingGroup> SetupAndReturnGroup()
        {

            string ip = ApplicationManager.Instance.Settings.HueBridgeIP;
            string key = ApplicationManager.Instance.Settings.HueKey;
            string entertainmentKey = ApplicationManager.Instance.Settings.HueEntertainmentKey;
            bool useSimulator = ApplicationManager.Instance.Settings.HueUseSimulator;

            StreamingGroup = null;
            Layers = null;

            StreamingHueClient = new StreamingHueClient(ip, key, entertainmentKey);

            var allEntertainmentGroups = await StreamingHueClient.LocalHueClient.GetEntertainmentGroups();
            var entertainmentGroup = allEntertainmentGroups.FirstOrDefault();

            if (entertainmentGroup == null)
                throw new Exception("No Entertainment Group found.");

            var stream = new StreamingGroup(entertainmentGroup.Locations);
            stream.IsForSimulator = useSimulator;

            await StreamingHueClient.Connect(entertainmentGroup.Id, simulator: useSimulator);

            StreamingHueClient.AutoUpdate(stream, 50);

            StreamingGroup = stream;
            var baseLayer = stream.GetNewLayer(isBaseLayer: true);
            var effectLayer = stream.GetNewLayer(isBaseLayer: false);
            Layers = new List<EntertainmentLayer>() { baseLayer, effectLayer };

            baseLayer.AutoCalculateEffectUpdate();
            effectLayer.AutoCalculateEffectUpdate();

            return stream;
        }


        public static void Disconnect()
        {
            StreamingHueClient.LocalHueClient.SetStreamingAsync(_groupId, active: false);
        }

        public async static Task<string> IsStreamingActive()
        {
            var bridgeInfo = await StreamingHueClient.LocalHueClient.GetBridgeAsync();

            return bridgeInfo.IsStreamingActive ? "Streaming is active" : "Streaming is not active";

        }
    }
}
