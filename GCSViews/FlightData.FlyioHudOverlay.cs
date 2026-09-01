using MissionPlanner.Controls;
using System;
using System.Drawing;

namespace MissionPlanner.GCSViews
{
    public partial class FlightData
    {
        private readonly object flyioLocationSync = new object();
        private string flyioLocationName = "Place unavailable";
        private string flyioLocationDistance = string.Empty;

        private void InitializeFlyioHudOverlay()
        {
            // Retain the flight symbology and tapes, but replace duplicate bottom telemetry
            // with one deliberate Fly.io footer row.
            hud1.displayspeedtext = false;
            hud1.displaygps = false;
            hud1.batteryon = false;
            hud1.batteryon2 = false;
            hud1.displayekf = false;
            hud1.displayvibe = false;
            hud1.displayprearm = false;
            hud1.OnCustomPaint -= DrawFlyioHudOverlay;
            hud1.OnCustomPaint += DrawFlyioHudOverlay;
        }

        /// <summary>
        /// Provides a thread-safe integration point for a future cached, asynchronous
        /// reverse-geocoding provider. This method performs no lookup or network access.
        /// </summary>
        internal void UpdateFlyioHudLocation(string placeName, string distanceText)
        {
            lock (flyioLocationSync)
            {
                flyioLocationName = string.IsNullOrWhiteSpace(placeName) ? "Place unavailable" : placeName.Trim();
                flyioLocationDistance = distanceText == null ? string.Empty : distanceText.Trim();
            }

            if (!hud1.IsDisposed)
                hud1.BeginInvokeIfRequired(() => hud1.Invalidate());
        }

        private void DrawFlyioHudOverlay(HUD hud)
        {
            if (hud.Width < 260 || hud.Height < 180)
                return;

            string place;
            string distance;
            lock (flyioLocationSync)
            {
                place = flyioLocationName;
                distance = flyioLocationDistance;
            }

            CurrentState state = MainV2.comPort.MAV.cs;
            float scale = Math.Max(0.72F, Math.Min(1.25F, hud.Width / 520F));
            float margin = 9F * scale;
            float locationX = Math.Max(hud.Width * 0.18F, 48F * scale);
            float locationY = hud.Height * 0.55F;
            float cardHeight = 43F * scale;
            float speedY = locationY + cardHeight + 2F * scale;
            float footerHeight = 25F * scale;
            float footerY = hud.Height - footerHeight - margin;

            using (var footerBacking = new SolidBrush(Color.FromArgb(190, 8, 18, 30)))
            using (var border = new Pen(Color.FromArgb(120, 255, 255, 255), Math.Max(1F, scale)))
            using (var primary = new SolidBrush(Color.White))
            using (var secondary = new SolidBrush(Color.FromArgb(220, 220, 229, 238)))
            using (var healthy = new SolidBrush(Color.FromArgb(255, 85, 214, 135)))
            using (var warning = new SolidBrush(Color.FromArgb(255, 255, 190, 72)))
            using (var danger = new SolidBrush(Color.FromArgb(255, 235, 87, 87)))
            using (var amber = new SolidBrush(Color.FromArgb(255, 255, 152, 0)))
            {
                float pinX = locationX + 10F * scale;
                float pinY = locationY + 14F * scale;
                hud.DrawLine(border, pinX, pinY - 4F * scale, pinX, pinY + 5F * scale);
                hud.DrawLine(border, pinX - 4F * scale, pinY, pinX + 4F * scale, pinY);
                hud.DrawString(place.ToUpperInvariant(), 11F * scale, primary,
                    locationX + 23F * scale, locationY + 5F * scale);
                hud.DrawString(string.IsNullOrEmpty(distance) ? "---" : distance, 8F * scale, secondary,
                    locationX + 23F * scale, locationY + 25F * scale);

                string speedValue = state.groundspeed.ToString("0");
                hud.DrawString(speedValue, 32F * scale, amber, locationX + 4F * scale, speedY);
                hud.DrawString(CurrentState.SpeedUnit, 10F * scale, primary,
                    locationX + 14F * scale + speedValue.Length * 17F * scale, speedY + 24F * scale);

                var footer = new RectangleF(margin, footerY, hud.Width - margin * 2F, footerHeight);
                hud.FillRectangle(footerBacking, footer);
                hud.DrawLine(border, footer.Left, footer.Top, footer.Right, footer.Top);

                float firstLine = footerY + 6F * scale;
                hud.DrawString("AS " + state.airspeed.ToString("0.0") + " " + CurrentState.SpeedUnit,
                    7F * scale, secondary, footer.Left + 7F * scale, firstLine);
                hud.DrawString("GS " + state.groundspeed.ToString("0.0") + " " + CurrentState.SpeedUnit,
                    7F * scale, primary, footer.Left + footer.Width * 0.20F, firstLine);

                hud.DrawString("EKF", 7F * scale, state.ekfstatus < 1F ? healthy : warning,
                    footer.Left + footer.Width * 0.40F, firstLine);
                float vibe = Math.Max(state.vibex, Math.Max(state.vibey, state.vibez));
                hud.DrawString("VIBE", 7F * scale, vibe < 30F ? healthy : warning,
                    footer.Left + footer.Width * 0.48F, firstLine);

                SolidBrush gpsBrush = state.gpsstatus >= 3 ? healthy : (state.gpsstatus >= 2 ? warning : danger);
                hud.DrawString("GPS: " + FormatGpsFix(state.gpsstatus), 7F * scale, gpsBrush,
                    footer.Left + footer.Width * 0.57F, firstLine);

                SolidBrush batteryBrush = state.battery_remaining >= 25 ? healthy :
                    (state.battery_remaining >= 10 ? warning : danger);
                hud.DrawString("BAT " + Math.Max(0, state.battery_remaining).ToString("0") + "%", 7F * scale,
                    batteryBrush, footer.Left + footer.Width * 0.82F, firstLine);
            }
        }

        private static string FormatGpsFix(float status)
        {
            if (status >= 6) return "RTK Fixed";
            if (status >= 5) return "RTK Float";
            if (status >= 3) return "3D Fix";
            if (status >= 2) return "2D Fix";
            return "No Fix";
        }
    }
}
