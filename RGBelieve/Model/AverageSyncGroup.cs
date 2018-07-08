using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Newtonsoft.Json;
using RGB.NET.Core;
using RGB.NET.Groups;

namespace RGBelieve.Model
{
    public class AverageSyncGroup : AbstractBindable
    {
        #region Properties & Fields

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "(unnamed)" : Name;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    OnPropertyChanged(nameof(DisplayName));
            }
        }

        private ObservableCollection<SyncLed> _syncLed;
        public ObservableCollection<SyncLed> SyncLed
        {
            get => _syncLed;
            set => SetProperty(ref _syncLed, value);
        }

        private ObservableCollection<SyncLed> _leds = new ObservableCollection<SyncLed>();
        public ObservableCollection<SyncLed> Leds
        {
            get => _leds;
            set => SetProperty(ref _leds, value);
        }

        [JsonIgnore]
        public ListLedGroup LedGroup { get; set; }

        [JsonIgnore]
        public NotifyCollectionChangedEventHandler LedsChangedEventHandler { get; set; }

        #endregion
    }
}
