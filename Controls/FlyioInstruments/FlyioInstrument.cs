using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MissionPlanner.Controls.FlyioInstruments
{
    internal enum FlyioInstrumentKind
    {
        Airspeed,
        Attitude,
        Altimeter,
        TurnCoordinator,
        Heading,
        VerticalSpeed
    }

    internal sealed class FlyioInstrument : Control
    {
        private const float DegreesToRadians = (float)(Math.PI / 180.0);
        private float primaryValue;
        private float secondaryValue;
        private float scaleMaximum = 60F;
        private string unit = string.Empty;

        internal FlyioInstrument(FlyioInstrumentKind kind)
        {
            Kind = kind;
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(7, 23, 41);
            MinimumSize = new Size(82, 82);
            Margin = new Padding(2);
        }

        internal FlyioInstrumentKind Kind { get; }

        internal float ScaleMaximum
        {
            get => scaleMaximum;
            set
            {
                float next = Math.Max(1F, value);
                if (Math.Abs(scaleMaximum - next) < 0.001F)
                    return;
                scaleMaximum = next;
                Invalidate();
            }
        }

        internal void SetValues(float primary, float secondary, string displayUnit)
        {
            if (Math.Abs(primaryValue - primary) < 0.001F &&
                Math.Abs(secondaryValue - secondary) < 0.001F && unit == displayUnit)
                return;

            primaryValue = primary;
            secondaryValue = secondary;
            unit = displayUnit ?? string.Empty;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (float.IsNaN(primaryValue) || float.IsNaN(secondaryValue) ||
                float.IsInfinity(primaryValue) || float.IsInfinity(secondaryValue))
            {
                e.Graphics.Clear(BackColor);
                TextRenderer.DrawText(e.Graphics, KindTitle() + "\r\nUnavailable", Font, ClientRectangle,
                    Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle card = Rectangle.Inflate(ClientRectangle, -1, -1);
            using (var cardBrush = new SolidBrush(Color.FromArgb(7, 23, 41)))
            using (var cardPen = new Pen(Color.FromArgb(23, 58, 94)))
            {
                e.Graphics.FillRectangle(cardBrush, card);
                e.Graphics.DrawRectangle(cardPen, card);
            }

            float titleHeight = Math.Max(14F, Math.Min(19F, ClientSize.Height * 0.105F));
            float size = Math.Min(ClientSize.Width - 10F, ClientSize.Height - titleHeight - 7F);
            if (size < 64F)
                return;

            float top = titleHeight + Math.Max(2F, (ClientSize.Height - titleHeight - size) / 2F);
            var face = new RectangleF((ClientSize.Width - size) / 2F, top, size, size);
            DrawTitle(e.Graphics, KindTitle(), new RectangleF(3, 1, ClientSize.Width - 6, titleHeight));
            DrawFace(e.Graphics, face);
        }

        private void DrawFace(Graphics graphics, RectangleF face)
        {
            using (var faceBrush = new SolidBrush(Color.FromArgb(8, 12, 18)))
            using (var bezelPen = new Pen(Color.FromArgb(35, 42, 49), Math.Max(5F, face.Width * 0.05F)))
            using (var rimPen = new Pen(Color.FromArgb(88, 101, 115), Math.Max(1F, face.Width * 0.009F)))
            {
                graphics.FillEllipse(faceBrush, face);
                graphics.DrawEllipse(bezelPen, RectangleF.Inflate(face, -2F, -2F));
                graphics.DrawEllipse(rimPen, face);
            }

            switch (Kind)
            {
                case FlyioInstrumentKind.Airspeed: DrawAirspeed(graphics, face); break;
                case FlyioInstrumentKind.Attitude: DrawAttitude(graphics, face); break;
                case FlyioInstrumentKind.Altimeter: DrawAltimeter(graphics, face); break;
                case FlyioInstrumentKind.TurnCoordinator: DrawTurn(graphics, face); break;
                case FlyioInstrumentKind.Heading: DrawHeading(graphics, face); break;
                case FlyioInstrumentKind.VerticalSpeed: DrawVerticalSpeed(graphics, face); break;
                default: DrawRadial(graphics, face, primaryValue, 0F, 60F, "0"); break;
            }
        }

        private void DrawAirspeed(Graphics graphics, RectangleF face)
        {
            PointF center = Center(face);
            float radius = face.Width * 0.39F;
            using (var tickPen = new Pen(Color.White, 1F))
            using (var textBrush = new SolidBrush(Color.White))
            using (var valueBrush = new SolidBrush(Color.White))
            using (var labelFont = CreateFont(Math.Max(6F, face.Width * 0.052F), FontStyle.Regular))
            using (var valueFont = CreateFont(Math.Max(12F, face.Width * 0.145F), FontStyle.Bold))
            using (var unitFont = CreateFont(Math.Max(6F, face.Width * 0.058F), FontStyle.Regular))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                float maximum = Math.Max(1F, ScaleMaximum);
                for (int index = 0; index <= 30; index++)
                {
                    float angle = 135F + index * 9F;
                    DrawTick(graphics, center, radius, angle, index % 5 == 0 ? 0.14F : 0.07F, tickPen);
                    if (index % 5 == 0)
                    {
                        PointF label = Polar(center, radius * 0.69F, angle);
                        graphics.DrawString((maximum * index / 30F).ToString("0"), labelFont, textBrush,
                            new RectangleF(label.X - 15, label.Y - 7, 30, 14), format);
                    }
                }
                float ratio = Math.Max(0F, Math.Min(1F, primaryValue / maximum));
                DrawNeedle(graphics, center, radius * 0.82F, 135F + ratio * 270F, Color.White);
                graphics.DrawString(primaryValue.ToString("0"), valueFont, valueBrush,
                    new RectangleF(face.Left, center.Y - face.Height * 0.13F, face.Width, face.Height * 0.18F), format);
                graphics.DrawString(unit, unitFont, textBrush,
                    new RectangleF(face.Left, center.Y + face.Height * 0.05F, face.Width, face.Height * 0.10F), format);
            }
        }

        private void DrawTitle(Graphics graphics, string text, RectangleF bounds)
        {
            using (var font = CreateFont(Math.Max(7F, Math.Min(9F, Width / 35F)), FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(242, 245, 248)))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                graphics.DrawString(text, font, brush, bounds, format);
        }

        private void DrawRadial(Graphics graphics, RectangleF face, float value, float minimum, float maximum, string format)
        {
            PointF center = Center(face);
            float radius = face.Width * 0.39F;
            using (var majorPen = new Pen(Color.White, Math.Max(1F, face.Width * 0.012F)))
            using (var minorPen = new Pen(Color.FromArgb(170, 205, 214, 226), 1F))
            using (var textBrush = new SolidBrush(Color.White))
            using (var valueBrush = new SolidBrush(Color.FromArgb(0, 173, 238)))
            using (var labelFont = CreateFont(Math.Max(6F, face.Width * 0.055F), FontStyle.Regular))
            using (var valueFont = CreateFont(Math.Max(9F, face.Width * 0.09F), FontStyle.Bold))
            using (var formatCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int index = 0; index <= 40; index++)
                {
                    float angle = 135F + index * 270F / 40F;
                    bool major = index % 5 == 0;
                    DrawTick(graphics, center, radius, angle, major ? 0.13F : 0.07F, major ? majorPen : minorPen);
                    if (major)
                    {
                        float tickValue = minimum + (maximum - minimum) * index / 40F;
                        PointF label = Polar(center, radius * 0.70F, angle);
                        graphics.DrawString(tickValue.ToString(format), labelFont, textBrush,
                            new RectangleF(label.X - 20, label.Y - 8, 40, 16), formatCenter);
                    }
                }

                float ratio = maximum <= minimum ? 0 : Math.Max(0, Math.Min(1, (value - minimum) / (maximum - minimum)));
                DrawNeedle(graphics, center, radius * 0.82F, 135F + ratio * 270F, Color.FromArgb(0, 173, 238));
                graphics.DrawString(value.ToString(format) + (string.IsNullOrEmpty(unit) ? "" : " " + unit),
                    valueFont, valueBrush, new RectangleF(face.Left, face.Top + face.Height * 0.62F, face.Width, face.Height * 0.17F), formatCenter);
            }
        }

        private void DrawAttitude(Graphics graphics, RectangleF face)
        {
            GraphicsState state = graphics.Save();
            using (var clipPath = new GraphicsPath())
            {
                clipPath.AddEllipse(face);
                graphics.SetClip(clipPath);
            }
            PointF center = Center(face);
            graphics.TranslateTransform(center.X, center.Y);
            graphics.RotateTransform(-primaryValue);
            float pitchOffset = Math.Max(-face.Height, Math.Min(face.Height, secondaryValue * face.Height / 45F));
            using (var sky = new SolidBrush(Color.FromArgb(62, 142, 208)))
            using (var ground = new SolidBrush(Color.FromArgb(139, 90, 50)))
            using (var whitePen = new Pen(Color.White, Math.Max(1.4F, face.Width * 0.012F)))
            using (var ladderPen = new Pen(Color.FromArgb(220, 255, 255, 255), 1F))
            {
                graphics.FillRectangle(sky, -face.Width, -face.Height * 2, face.Width * 2, face.Height * 2 + pitchOffset);
                graphics.FillRectangle(ground, -face.Width, pitchOffset, face.Width * 2, face.Height * 2);
                graphics.DrawLine(whitePen, -face.Width, pitchOffset, face.Width, pitchOffset);
                for (int pitch = -20; pitch <= 20; pitch += 10)
                {
                    if (pitch == 0) continue;
                    float y = pitchOffset - pitch * face.Height / 45F;
                    graphics.DrawLine(ladderPen, -face.Width * 0.18F, y, face.Width * 0.18F, y);
                }
            }
            graphics.Restore(state);
            using (var rollPen = new Pen(Color.White, Math.Max(1F, face.Width * 0.01F)))
            {
                float rollRadius = face.Width * 0.39F;
                foreach (int angle in new[] { -60, -30, -20, -10, 0, 10, 20, 30, 60 })
                    DrawTick(graphics, center, rollRadius, -90F + angle,
                        angle % 30 == 0 ? 0.10F : 0.06F, rollPen);
                PointF top = Polar(center, rollRadius * 0.84F, -90F);
                PointF left = new PointF(top.X - face.Width * 0.025F, top.Y - face.Height * 0.04F);
                PointF right = new PointF(top.X + face.Width * 0.025F, top.Y - face.Height * 0.04F);
                graphics.DrawPolygon(rollPen, new[] { top, left, right });
            }
            using (var aircraftPen = new Pen(Color.White, Math.Max(2F, face.Width * 0.018F)))
            {
                graphics.DrawLine(aircraftPen, center.X - face.Width * 0.22F, center.Y, center.X - face.Width * 0.05F, center.Y);
                graphics.DrawLine(aircraftPen, center.X + face.Width * 0.05F, center.Y, center.X + face.Width * 0.22F, center.Y);
                graphics.DrawEllipse(aircraftPen, center.X - 3, center.Y - 3, 6, 6);
            }
        }

        private void DrawAltimeter(Graphics graphics, RectangleF face)
        {
            PointF center = Center(face);
            float radius = face.Width * 0.39F;
            using (var majorPen = new Pen(Color.White, Math.Max(1F, face.Width * 0.012F)))
            using (var minorPen = new Pen(Color.FromArgb(190, 205, 214, 226), 1F))
            using (var textBrush = new SolidBrush(Color.White))
            using (var labelFont = CreateFont(Math.Max(6F, face.Width * 0.06F), FontStyle.Regular))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int tick = 0; tick < 50; tick++)
                {
                    float angle = -90F + tick * 7.2F;
                    bool major = tick % 5 == 0;
                    DrawTick(graphics, center, radius, angle, major ? 0.14F : 0.07F, major ? majorPen : minorPen);
                    if (major)
                    {
                        PointF label = Polar(center, radius * 0.70F, angle);
                        graphics.DrawString((tick / 5).ToString(), labelFont, textBrush,
                            new RectangleF(label.X - 12F, label.Y - 9F, 24F, 18F), format);
                    }
                }
                float hundreds = ((primaryValue % 1000F) + 1000F) % 1000F;
                DrawNeedle(graphics, center, radius * 0.82F, -90F + hundreds / 1000F * 360F, Color.White);
            }
            string digits = Math.Max(0, (int)Math.Round(primaryValue)).ToString("000000");
            RectangleF drum = new RectangleF(face.Left + face.Width * 0.18F, face.Top + face.Height * 0.40F,
                face.Width * 0.64F, face.Height * 0.20F);
            using (var drumBrush = new SolidBrush(Color.FromArgb(5, 9, 15)))
            using (var borderPen = new Pen(Color.FromArgb(150, 160, 170), 1F))
            using (var digitBrush = new SolidBrush(Color.White))
            using (var font = new Font("Consolas", Math.Max(9F, face.Width * 0.095F), FontStyle.Bold, GraphicsUnit.Point))
            using (var unitFont = CreateFont(Math.Max(6F, face.Width * 0.055F), FontStyle.Regular))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                graphics.FillRectangle(drumBrush, drum);
                graphics.DrawRectangle(borderPen, drum.X, drum.Y, drum.Width, drum.Height);
                graphics.DrawString(digits, font, digitBrush, drum, format);
                graphics.DrawString(unit, unitFont, digitBrush,
                    new RectangleF(face.Left, drum.Bottom + 2F, face.Width, face.Height * 0.10F), format);
            }
        }

        private void DrawHeading(Graphics graphics, RectangleF face)
        {
            PointF center = Center(face);
            float radius = face.Width * 0.39F;
            using (var pen = new Pen(Color.White, 1F))
            using (var brush = new SolidBrush(Color.White))
            using (var cyan = new SolidBrush(Color.FromArgb(0, 173, 238)))
            using (var font = CreateFont(Math.Max(7F, face.Width * 0.075F), FontStyle.Bold))
            using (var valueFont = CreateFont(Math.Max(11F, face.Width * 0.13F), FontStyle.Bold))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                string[] cardinals = { "N", "E", "S", "W" };
                for (int degrees = 0; degrees < 360; degrees += 5)
                {
                    float angle = degrees - primaryValue - 90F;
                    DrawTick(graphics, center, radius, angle, degrees % 30 == 0 ? 0.12F : 0.06F, pen);
                    if (degrees % 90 == 0)
                    {
                        PointF label = Polar(center, radius * 0.72F, angle);
                        graphics.DrawString(cardinals[degrees / 90], font, brush,
                            new RectangleF(label.X - 12, label.Y - 10, 24, 20), format);
                    }
                    else if (degrees % 30 == 0)
                    {
                        PointF label = Polar(center, radius * 0.72F, angle);
                        graphics.DrawString((degrees / 10).ToString("00"), font, brush,
                            new RectangleF(label.X - 13, label.Y - 9, 26, 18), format);
                    }
                }
                using (var centerBrush = new SolidBrush(Color.FromArgb(220, 12, 24, 32)))
                    graphics.FillEllipse(centerBrush, center.X - face.Width * 0.23F, center.Y - face.Height * 0.12F,
                        face.Width * 0.46F, face.Height * 0.24F);
                graphics.DrawString(((int)Math.Round((primaryValue + 360F) % 360F)).ToString("000") + "°",
                    valueFont, cyan, new RectangleF(face.Left, center.Y - face.Height * 0.09F, face.Width, face.Height * 0.18F), format);
                using (var lubber = new Pen(Color.White, Math.Max(2F, face.Width * 0.014F)))
                    graphics.DrawLine(lubber, center.X, face.Top + face.Height * 0.08F,
                        center.X, face.Top + face.Height * 0.17F);
                DrawAircraftSymbol(graphics, center, face.Width, false);
            }
        }

        private void DrawTurn(Graphics graphics, RectangleF face)
        {
            PointF center = Center(face);
            // CurrentState.turnrate is the supported real turn-rate source. A standard
            // three-degree/second rate is 0.0524 rad/s; no synthetic slip ball is drawn.
            float indicatedTurn = Math.Max(-30F, Math.Min(30F, primaryValue / 0.0524F * 20F));
            using (var white = new Pen(Color.White, Math.Max(2F, face.Width * 0.015F)))
            using (var textBrush = new SolidBrush(Color.FromArgb(205, 214, 226)))
            using (var font = CreateFont(Math.Max(6F, face.Width * 0.06F), FontStyle.Regular))
            using (var format = new StringFormat { Alignment = StringAlignment.Center })
            {
                GraphicsState state = graphics.Save();
                graphics.TranslateTransform(center.X, center.Y);
                graphics.RotateTransform(indicatedTurn);
                DrawAircraftSymbol(graphics, PointF.Empty, face.Width, true);
                graphics.Restore(state);
                float referenceY = center.Y - face.Height * 0.19F;
                graphics.DrawLine(white, center.X - face.Width * 0.27F, referenceY,
                    center.X - face.Width * 0.27F, referenceY + face.Height * 0.09F);
                graphics.DrawLine(white, center.X + face.Width * 0.27F, referenceY,
                    center.X + face.Width * 0.27F, referenceY + face.Height * 0.09F);
                graphics.DrawString("L        2 MIN TURN        R", font, textBrush,
                    new RectangleF(face.Left, face.Top + face.Height * 0.72F, face.Width, 18), format);
            }
        }

        private void DrawVerticalSpeed(Graphics graphics, RectangleF face)
        {
            PointF center = Center(face);
            float radius = face.Width * 0.39F;
            using (var tickPen = new Pen(Color.White, 1F))
            using (var labelBrush = new SolidBrush(Color.FromArgb(205, 214, 226)))
            using (var valueBrush = new SolidBrush(Color.FromArgb(0, 173, 238)))
            using (var labelFont = CreateFont(Math.Max(6F, face.Width * 0.052F), FontStyle.Regular))
            using (var valueFont = CreateFont(Math.Max(11F, face.Width * 0.13F), FontStyle.Bold))
            using (var unitFont = CreateFont(Math.Max(6F, face.Width * 0.055F), FontStyle.Regular))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                float maximum = Math.Max(1F, ScaleMaximum);
                for (int index = -20; index <= 20; index++)
                {
                    float scaleValue = maximum * index / 20F;
                    float angle = 180F - scaleValue / maximum * 135F;
                    DrawTick(graphics, center, radius, angle, index % 4 == 0 ? 0.14F : 0.07F, tickPen);
                    if (index % 4 == 0)
                    {
                        PointF label = Polar(center, radius * 0.70F, angle);
                        graphics.DrawString(Math.Abs(scaleValue).ToString("0"), labelFont, labelBrush,
                            new RectangleF(label.X - 12F, label.Y - 8F, 24F, 16F), format);
                    }
                }
                float limitedValue = Math.Max(-maximum, Math.Min(maximum, primaryValue));
                DrawNeedle(graphics, center, radius * 0.82F, 180F - limitedValue / maximum * 135F, Color.White);
                string value = primaryValue > 0 ? "+" + primaryValue.ToString("0.0") : primaryValue.ToString("0.0");
                graphics.DrawString(value, valueFont, valueBrush,
                    new RectangleF(face.Left, center.Y - face.Height * 0.14F, face.Width, face.Height * 0.17F), format);
                graphics.DrawString(unit, unitFont, labelBrush,
                    new RectangleF(face.Left, center.Y + face.Height * 0.03F, face.Width, face.Height * 0.12F), format);
            }
        }

        private static void DrawTick(Graphics graphics, PointF center, float radius, float angle, float lengthRatio, Pen pen)
        {
            PointF outer = Polar(center, radius, angle);
            PointF inner = Polar(center, radius * (1F - lengthRatio), angle);
            graphics.DrawLine(pen, inner, outer);
        }

        private static void DrawNeedle(Graphics graphics, PointF center, float radius, float angle, Color color)
        {
            using (var pen = new Pen(color, Math.Max(2F, radius * 0.025F)))
            using (var hub = new SolidBrush(color))
            {
                graphics.DrawLine(pen, center, Polar(center, radius, angle));
                graphics.FillEllipse(hub, center.X - 4, center.Y - 4, 8, 8);
            }
        }

        private static void DrawAircraftSymbol(Graphics graphics, PointF center, float size, bool turnProfile)
        {
            float wing = size * (turnProfile ? 0.27F : 0.20F);
            float body = size * (turnProfile ? 0.15F : 0.13F);
            float tail = size * 0.075F;
            using (var pen = new Pen(Color.White, Math.Max(1.5F, size * 0.012F))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            })
            {
                graphics.DrawLine(pen, center.X - wing, center.Y, center.X + wing, center.Y);
                graphics.DrawLine(pen, center.X, center.Y - body, center.X, center.Y + body);
                graphics.DrawLine(pen, center.X - tail, center.Y + body * 0.72F,
                    center.X + tail, center.Y + body * 0.72F);
                graphics.DrawLine(pen, center.X - wing, center.Y, center.X - body * 0.35F, center.Y - body * 0.18F);
                graphics.DrawLine(pen, center.X + wing, center.Y, center.X + body * 0.35F, center.Y - body * 0.18F);
            }
        }

        private string KindTitle()
        {
            switch (Kind)
            {
                case FlyioInstrumentKind.Airspeed: return "AIRSPEED";
                case FlyioInstrumentKind.Attitude: return "ATTITUDE";
                case FlyioInstrumentKind.Altimeter: return "ALTIMETER";
                case FlyioInstrumentKind.TurnCoordinator: return "TURN COORDINATOR";
                case FlyioInstrumentKind.Heading: return "HEADING";
                default: return "VERTICAL SPEED";
            }
        }

        private static PointF Center(RectangleF rectangle) =>
            new PointF(rectangle.Left + rectangle.Width / 2F, rectangle.Top + rectangle.Height / 2F);

        private static PointF Polar(PointF center, float radius, float angle)
        {
            float radians = angle * DegreesToRadians;
            return new PointF(center.X + (float)Math.Cos(radians) * radius,
                center.Y + (float)Math.Sin(radians) * radius);
        }

        private static Font CreateFont(float size, FontStyle style)
        {
            try { return new Font("Kantumruy Pro", size, style, GraphicsUnit.Point); }
            catch { return new Font("Segoe UI", size, style, GraphicsUnit.Point); }
        }
    }
}
