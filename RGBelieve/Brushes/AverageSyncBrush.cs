using System.Collections.Generic;
using System.ComponentModel;
using RGB.NET.Core;
using RGBelieve.Helper;
using RGBelieve.Model;

namespace RGBelieve.Brushes
{
    public class AverageSyncBrush : AbstractBrush
    {
        #region Properties & Fields

        private readonly AverageSyncGroup _averageSyncGroup;

        private List<Led> _syncLed;

        #endregion

        #region Constructors

        public AverageSyncBrush(AverageSyncGroup averageSyncGroup)
        {
            this._averageSyncGroup = averageSyncGroup;
            this._syncLed = new List<Led>();

            for (int i = 0; i < averageSyncGroup.SyncLed.Count; i++)
            {
                averageSyncGroup.SyncLed[i].PropertyChanged += SyncGroupOnPropertyChanged;
                _syncLed.Add(averageSyncGroup.SyncLed[i]?.GetLed());
            }
        }

        #endregion

        #region Methods

        private void SyncGroupOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SyncGroup.SyncLed))
            {
                for (int i = 0; i < _averageSyncGroup.SyncLed.Count; i++)
                {
                    _syncLed[i] = _averageSyncGroup.SyncLed[i]?.GetLed();
                }
            }
        }

        protected override Color GetColorAtPoint(Rectangle rectangle, BrushRenderTarget renderTarget)
        {
            //if (renderTarget.Led == _syncLed)
            //    return Color.Transparent;

            if (_syncLed == null)
                return Color.Transparent;

            Color average = _syncLed[0];

            for (int i = 1; i < _syncLed.Count; i++)
            {
                average = ColorUtils.colorMixer(average, _syncLed[i]);
            }

            return average;
        }

        #endregion
    }
}
