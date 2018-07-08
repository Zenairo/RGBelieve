using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using RGB.NET.Core;
using RGB.NET.Groups;
using RGB.NET.Devices.Asus;
using RGB.NET.Devices.Corsair;
using RGBelieve.Brushes;
using RGBelieve.Configuration;
using RGBelieve.Helper;
using RGBelieve.Model;
using RGBelieve.UI;
using System.Collections.ObjectModel;
using RGBelieve.Hue;
using System.Threading.Tasks;

namespace RGBelieve
{
    class ApplicationManager
    {
        #region Constants

        private const string DEVICEPROVIDER_DIRECTORY = "DeviceProvider";

        #endregion

        #region Properties & Fields

        public static ApplicationManager Instance { get; } = new ApplicationManager();

        private MainWindow _mainWindow;

        public Settings Settings { get; set; }
        public TimerUpdateTrigger UpdateTrigger { get; set; }
        public HueEntertainment HueEntertainment { get; private set; }

        #endregion

        #region Commands

        private ActionCommand _openConfiguration;
        public ActionCommand OpenConfigurationCommand => _openConfiguration ?? (_openConfiguration = new ActionCommand(OpenConfiguration));

        private ActionCommand _exitCommand;
        public ActionCommand ExitCommand => _exitCommand ?? (_exitCommand = new ActionCommand(Exit));

        #endregion

        #region Constructors

        private ApplicationManager() { }

        #endregion

        #region Methods

        public void Initialize()
        {
            RGBSurface surface = RGBSurface.Instance;
            LoadDeviceProviders();
            surface.AlignDevices();

            surface.Devices.OfType<AsusMainboardRGBDevice>().First().UpdateMode = DeviceUpdateMode.SyncBack;

            UpdateTrigger = new TimerUpdateTrigger { UpdateFrequency = 1.0 / 30.0 }; //MathHelper.Clamp(Settings.UpdateRate, 1, 100) };
            surface.RegisterUpdateTrigger(UpdateTrigger);
            UpdateTrigger.Start();

            PremadeSyncGroups(); //Create your sync groups here!

            HueEntertainment = HueEntertainment.Instance;
            Task.Factory.StartNew(() => HueEntertainment.Initialize());
        }

        private void LoadDeviceProviders()
        {
            try
            {
                RGBSurface.Instance.LoadDevices(AsusDeviceProvider.Instance, RGBDeviceType.Mainboard);
                RGBSurface.Instance.LoadDevices(CorsairDeviceProvider.Instance);
            }
            catch { }

        }

        public void AddSyncGroup(SyncGroup syncGroup)
        {
            Settings.SyncGroups.Add(syncGroup);
            RegisterSyncGroup(syncGroup);
        }

        private void RegisterSyncGroup(SyncGroup syncGroup)
        {
            syncGroup.LedGroup = new ListLedGroup(syncGroup.Leds.GetLeds()) { Brush = new SyncBrush(syncGroup) };
            syncGroup.LedsChangedEventHandler = (sender, args) => UpdateLedGroup(syncGroup.LedGroup, args);
            syncGroup.Leds.CollectionChanged += syncGroup.LedsChangedEventHandler;
        }

        public void RemoveSyncGroup(SyncGroup syncGroup)
        {
            Settings.SyncGroups.Remove(syncGroup);
            syncGroup.Leds.CollectionChanged -= syncGroup.LedsChangedEventHandler;
            syncGroup.LedGroup.Detach();
            syncGroup.LedGroup = null;
        }

        public void AddAverageSyncGroup(AverageSyncGroup averageSyncGroup)
        {
            Settings.AverageSyncGroups.Add(averageSyncGroup);
            RegisterAverageSyncGroup(averageSyncGroup);
        }

        private void RegisterAverageSyncGroup(AverageSyncGroup averageSyncGroup)
        {
            averageSyncGroup.LedGroup = new ListLedGroup(averageSyncGroup.Leds.GetLeds()) { Brush = new AverageSyncBrush(averageSyncGroup) };
            averageSyncGroup.LedsChangedEventHandler = (sender, args) => UpdateLedGroup(averageSyncGroup.LedGroup, args);
            averageSyncGroup.Leds.CollectionChanged += averageSyncGroup.LedsChangedEventHandler;
        }

        public void RemoveAverageSyncGroup(AverageSyncGroup averageSyncGroup)
        {
            Settings.AverageSyncGroups.Remove(averageSyncGroup);
            averageSyncGroup.Leds.CollectionChanged -= averageSyncGroup.LedsChangedEventHandler;
            averageSyncGroup.LedGroup.Detach();
            averageSyncGroup.LedGroup = null;
        }

        private void UpdateLedGroup(ListLedGroup group, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                List<Led> leds = group.GetLeds().ToList();
                group.RemoveLeds(leds);
            }
            else
            {
                if (args.NewItems != null)
                    group.AddLeds(args.NewItems.Cast<SyncLed>().GetLeds());

                if (args.OldItems != null)
                    group.RemoveLeds(args.OldItems.Cast<SyncLed>().GetLeds());
            }
        }

        private void PremadeSyncGroups()
        {
            AsusMainboardRGBDevice auraMB = RGBSurface.Instance.Devices.OfType<AsusMainboardRGBDevice>().First();

            CorsairKeyboardRGBDevice corsairKeyboard = RGBSurface.Instance.Devices.OfType<CorsairKeyboardRGBDevice>().First();
            double keyboardWidth = corsairKeyboard.Max(x => x.LedRectangle.Location.X);

            CorsairMousepadRGBDevice corsairMousepad = RGBSurface.Instance.Devices.OfType<CorsairMousepadRGBDevice>().First();
            CorsairHeadsetStandRGBDevice corsairHeadsetStand = RGBSurface.Instance.Devices.OfType<CorsairHeadsetStandRGBDevice>().First();

            List<CorsairCustomRGBDevice> corsairCustomDevices = RGBSurface.Instance.Devices.OfType<CorsairCustomRGBDevice>().ToList();
            CorsairCustomRGBDevice stripOne = corsairCustomDevices[0];
            CorsairCustomRGBDevice stripTwo = corsairCustomDevices[1];
            CorsairCustomRGBDevice stripThree = corsairCustomDevices[2];
            CorsairCustomRGBDevice stripFour = corsairCustomDevices[3];
            CorsairCustomRGBDevice fanOne = corsairCustomDevices[4];
            CorsairCustomRGBDevice fanTwo = corsairCustomDevices[5];
            CorsairCustomRGBDevice fanThree = corsairCustomDevices[6];
            CorsairCustomRGBDevice fanFour = corsairCustomDevices[7];
            CorsairCustomRGBDevice fanFive = corsairCustomDevices[8];
            CorsairCustomRGBDevice fanSix = corsairCustomDevices[9];

            //CorsairCustomRGBDevice stripOne = surface.Devices.OfType<CorsairCustomRGBDevice>().

            //backIO = 0
            SyncLed backIO = new SyncLed(auraMB.ElementAt(0));
            List<Led> backIOTargets = new List<Led>();
            backIOTargets.AddRange(corsairKeyboard.Where(x => x.LedRectangle.Location.X <= keyboardWidth / 4).ToList());
            backIOTargets.AddRange(corsairMousepad.Take(4));
            backIOTargets.Add(corsairHeadsetStand.ElementAt(8));
            backIOTargets.AddRange(stripOne);
            backIOTargets.AddRange(fanOne);
            backIOTargets.AddRange(fanTwo);
            SyncGroup syncBackIO = new SyncGroup { SyncLed = backIO, Leds = new ObservableCollection<SyncLed>(backIOTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterSyncGroup(syncBackIO);

            //pch = 1
            SyncLed pch = new SyncLed(auraMB.ElementAt(1));
            List<Led> pchTargets = new List<Led>();
            pchTargets.AddRange(corsairKeyboard.Where(x => x.LedRectangle.Location.X > keyboardWidth / 4 && x.LedRectangle.Location.X <= keyboardWidth / 2).ToList());
            pchTargets.AddRange(corsairMousepad.Skip(4).Take(3));
            pchTargets.Add(corsairHeadsetStand.ElementAt(6));
            pchTargets.AddRange(stripThree);
            pchTargets.AddRange(fanFour);
            SyncGroup syncPch = new SyncGroup { SyncLed = pch, Leds = new ObservableCollection<SyncLed>(pchTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterSyncGroup(syncPch);


            //headerOne = 2
            SyncLed headerOne = new SyncLed(auraMB.ElementAt(2));
            List<Led> headerOneTargets = new List<Led>();
            headerOneTargets.AddRange(corsairKeyboard.Where(x => x.LedRectangle.Location.X > keyboardWidth / 2 && x.LedRectangle.Location.X <= keyboardWidth / 4 * 3).ToList());
            headerOneTargets.AddRange(corsairMousepad.Skip(8).Take(3));
            headerOneTargets.Add(corsairHeadsetStand.ElementAt(4));
            headerOneTargets.AddRange(stripTwo);
            headerOneTargets.AddRange(fanThree);
            SyncGroup syncHeaderOne = new SyncGroup { SyncLed = headerOne, Leds = new ObservableCollection<SyncLed>(headerOneTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterSyncGroup(syncHeaderOne);


            //headerTwo = 3
            SyncLed headerTwo = new SyncLed(auraMB.ElementAt(3));
            List<Led> headerTwoTargets = new List<Led>();
            headerTwoTargets.AddRange(corsairKeyboard.Where(x => x.LedRectangle.Location.X > keyboardWidth / 4 * 3).ToList());
            headerTwoTargets.AddRange(corsairMousepad.Skip(11).Take(4));
            headerTwoTargets.Add(corsairHeadsetStand.ElementAt(2));
            headerTwoTargets.AddRange(stripFour);
            headerTwoTargets.AddRange(fanFive);
            headerTwoTargets.AddRange(fanSix);
            SyncGroup syncHeaderTwo = new SyncGroup { SyncLed = headerTwo, Leds = new ObservableCollection<SyncLed>(headerTwoTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterSyncGroup(syncHeaderTwo);

            //average backIO and pch
            List<SyncLed> avgBackPch = new List<SyncLed>() { backIO, pch };
            List<Led> avgBackPchTargets = new List<Led>() { corsairHeadsetStand.ElementAt(7) };
            AverageSyncGroup syncAvgBackPch = new AverageSyncGroup { SyncLed = new ObservableCollection<SyncLed>(avgBackPch.ToList()), Leds = new ObservableCollection<SyncLed>(avgBackPchTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterAverageSyncGroup(syncAvgBackPch);

            //average backIO and headerTwo
            List<SyncLed> avgBackTwo = new List<SyncLed>() { backIO, headerTwo };
            List<Led> avgBackTwoTargets = new List<Led>() { corsairHeadsetStand.ElementAt(1) };
            AverageSyncGroup syncAvgBackTwo = new AverageSyncGroup { SyncLed = new ObservableCollection<SyncLed>(avgBackTwo.ToList()), Leds = new ObservableCollection<SyncLed>(avgBackTwoTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterAverageSyncGroup(syncAvgBackTwo);

            //average pch and headerOne
            List<SyncLed> avgPchOne = new List<SyncLed>() { pch, headerOne };
            List<Led> avgPchOneTargets = new List<Led>() { corsairHeadsetStand.ElementAt(5), corsairMousepad.ElementAt(7) };
            AverageSyncGroup syncAvgPchOne = new AverageSyncGroup { SyncLed = new ObservableCollection<SyncLed>(avgPchOne.ToList()), Leds = new ObservableCollection<SyncLed>(avgPchOneTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterAverageSyncGroup(syncAvgPchOne);

            //average headerOne and headerTwo
            List<SyncLed> avgOneTwo = new List<SyncLed>() { headerOne, headerTwo };
            List<Led> avgOneTwoTargets = new List<Led>() { corsairHeadsetStand.ElementAt(3) };
            AverageSyncGroup syncAvgOneTwo = new AverageSyncGroup { SyncLed = new ObservableCollection<SyncLed>(avgOneTwo.ToList()), Leds = new ObservableCollection<SyncLed>(avgOneTwoTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterAverageSyncGroup(syncAvgOneTwo);

            //average backIO and pch and headerOne and headerTwo
            List<SyncLed> avgAll = new List<SyncLed>() { backIO, pch, headerOne, headerTwo };
            List<Led> avgAllTargets = new List<Led>() { corsairHeadsetStand.ElementAt(0) };
            AverageSyncGroup syncAvgAll = new AverageSyncGroup { SyncLed = new ObservableCollection<SyncLed>(avgAll.ToList()), Leds = new ObservableCollection<SyncLed>(avgAllTargets.Select(x => new SyncLed(x)).ToList()) };
            RegisterAverageSyncGroup(syncAvgAll);
        }

        private void OpenConfiguration()
        {
            if (_mainWindow == null) _mainWindow = new MainWindow();
            _mainWindow.Show();
        }

        private void Exit()
        {
            try { RGBSurface.Instance?.Dispose(); } catch { /* goodbye */ }

            try { HueEntertainment.Instance?.Dispose(); } catch { /* goodbye hue */ }

            Application.Current.Shutdown();
        }

        #endregion
    }
}
