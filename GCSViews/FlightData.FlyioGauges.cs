using MissionPlanner.Controls.FlyioInstruments;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    public partial class FlightData
    {
        private TableLayoutPanel flyioSixPackLayout;
        private FlyioInstrument flyioAirspeed;
        private FlyioInstrument flyioAttitude;
        private FlyioInstrument flyioAltimeter;
        private FlyioInstrument flyioTurn;
        private FlyioInstrument flyioHeading;
        private FlyioInstrument flyioVerticalSpeed;

        private void InitializeFlyioSixPack()
        {
            Gspeed.Visible = false;
            Galt.Visible = false;
            Gheading.Visible = false;
            Gvspeed.Visible = false;

            flyioSixPackLayout = new TableLayoutPanel
            {
                Name = "flyioSixPackLayout",
                Dock = DockStyle.Fill,
                BackColor = FlyioDataBackground,
                Padding = new Padding(3),
                Margin = Padding.Empty,
                ColumnCount = 3,
                RowCount = 2
            };
            for (int column = 0; column < 3; column++)
                flyioSixPackLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            for (int row = 0; row < 2; row++)
                flyioSixPackLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            flyioAirspeed = NewFlyioInstrument(FlyioInstrumentKind.Airspeed);
            flyioAttitude = NewFlyioInstrument(FlyioInstrumentKind.Attitude);
            flyioAltimeter = NewFlyioInstrument(FlyioInstrumentKind.Altimeter);
            flyioTurn = NewFlyioInstrument(FlyioInstrumentKind.TurnCoordinator);
            flyioHeading = NewFlyioInstrument(FlyioInstrumentKind.Heading);
            flyioVerticalSpeed = NewFlyioInstrument(FlyioInstrumentKind.VerticalSpeed);
            flyioAirspeed.ScaleMaximum = Math.Max(1F, Gspeed.MaxValue);
            flyioVerticalSpeed.ScaleMaximum = Math.Max(1F,
                Math.Max(Math.Abs(Gvspeed.MinValue), Math.Abs(Gvspeed.MaxValue)));

            flyioSixPackLayout.Controls.Add(flyioAirspeed, 0, 0);
            flyioSixPackLayout.Controls.Add(flyioAttitude, 1, 0);
            flyioSixPackLayout.Controls.Add(flyioAltimeter, 2, 0);
            flyioSixPackLayout.Controls.Add(flyioTurn, 0, 1);
            flyioSixPackLayout.Controls.Add(flyioHeading, 1, 1);
            flyioSixPackLayout.Controls.Add(flyioVerticalSpeed, 2, 1);
            tabGauges.Controls.Add(flyioSixPackLayout);
            flyioSixPackLayout.BringToFront();
            UpdateFlyioSixPack(MainV2.comPort.MAV.cs);
        }

        private static FlyioInstrument NewFlyioInstrument(FlyioInstrumentKind kind)
        {
            return new FlyioInstrument(kind) { Dock = DockStyle.Fill, Margin = new Padding(2) };
        }

        private void UpdateFlyioSixPack(CurrentState state)
        {
            if (flyioSixPackLayout == null || flyioSixPackLayout.IsDisposed)
                return;

            flyioAirspeed.SetValues(state.airspeed, state.groundspeed, CurrentState.SpeedUnit);
            flyioAttitude.SetValues(state.roll, state.pitch, "deg");
            flyioAltimeter.SetValues(state.alt, 0F, CurrentState.AltUnit);
            flyioTurn.SetValues(state.turnrate, state.roll, "rad/s");
            flyioHeading.SetValues(state.yaw, state.nav_bearing, "deg");
            flyioVerticalSpeed.SetValues(state.verticalspeed, 0F, CurrentState.SpeedUnit);
        }
    }
}
