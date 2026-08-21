namespace MissionPlanner.GCSViews.ConfigurationView
{
    public sealed class ConfigMotorServoCombined : ConfigCombinedTabPage
    {
        public ConfigMotorServoCombined()
        {
            // ConfigMotorTest.Activate creates its dynamic controls, so invoke it once per instance.
            if (MainV2.DisplayConfiguration.displayMotorTest)
                AddPage("Motor Test", new ConfigMotorTest(), true);

            if (MainV2.DisplayConfiguration.displayServoOutput)
                AddPage("Servo Output", new ConfigRadioOutput());
        }
    }
}
