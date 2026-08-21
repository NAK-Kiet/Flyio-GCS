namespace MissionPlanner.GCSViews.ConfigurationView
{
    public sealed class ConfigRadioAccelCombined : ConfigCombinedTabPage
    {
        public ConfigRadioAccelCombined()
        {
            if (MainV2.DisplayConfiguration.displayRadioCalibration)
                AddPage("Radio Calibration", new ConfigRadioInput());

            if (MainV2.DisplayConfiguration.displayAccelCalibration)
                AddPage("Accel Calibration", new ConfigAccelerometerCalibration());
        }
    }
}
