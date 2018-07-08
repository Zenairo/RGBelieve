using Q42.HueApi.ColorConverters;
using Q42.HueApi.Streaming.Extensions;
using Q42.HueApi.Streaming.Models;
using RGB.NET.Core;
using RGB.NET.Devices.Asus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RGBelieve.Hue
{
    class HueEntertainment
    {

        public static HueEntertainment Instance { get; } = new HueEntertainment();

        public static EntertainmentLayer EffectLayer { get; set; }

        private static IEnumerable<Led> syncLeds;

        public async Task Initialize()
        {
            await Connect();

            syncLeds = RGBSurface.Instance.Devices.OfType<AsusMainboardRGBDevice>().First();

            ApplicationManager.Instance.UpdateTrigger.Update += (s,e) => { Update(); };
        }

        public async Task Connect()
        {
            try
            {
                await StreamingSetup.SetupAndReturnGroup();
                EffectLayer = StreamingSetup.Layers.Last();
            }
            catch
            {
                /* borked */
            }
        }

        private void Update()
        {
            var lights = EffectLayer.OrderBy(x => new Guid());

            int i = 0;
            foreach (EntertainmentLight light in lights)
            {
                int colInt = i++ % syncLeds.Count();
                light.SetState(new RGBColor(syncLeds.ElementAt(colInt).Color.R, syncLeds.ElementAt(colInt).Color.G, syncLeds.ElementAt(colInt).Color.B), 1);
            }
        }

        public void Dispose()
        {
            StreamingSetup.Disconnect();
            EffectLayer = null;
            syncLeds = null;
        }
    }
}
