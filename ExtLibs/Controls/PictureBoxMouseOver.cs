using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.Controls
{
    public class PictureBoxMouseOver: PictureBox
    {
        public Image ImageNormal { get; set; }
        public Image ImageOver { get; set; }

        public Color HoverColor { get; set; }
        public Color SelectedColor { get; set; }
        public Color PressedColor { get; set; }
        public Color DisabledColor { get; set; }

        bool mouseover = false;
        bool mousedown = false;
        bool _selected = false;
        public bool selected { get { return _selected; } set { _selected = value; ChangePicture(); this.Invalidate(); } }

        Image hoverImage;
        Image selectedImage;
        Image pressedImage;
        Image disabledImage;

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            mouseover = true;

            ChangePicture();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            mouseover = false;
            mousedown = false;

            ChangePicture();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                mousedown = true;
                ChangePicture();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mousedown = false;
            ChangePicture();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            ChangePicture();
        }

        public void RefreshStateImages()
        {
            DisposeStateImages();
            if (ImageNormal != null)
            {
                hoverImage = Tint(ImageNormal, HoverColor);
                selectedImage = Tint(ImageNormal, SelectedColor);
                pressedImage = Tint(ImageNormal, PressedColor);
                disabledImage = Tint(ImageNormal, DisabledColor);
            }
            ChangePicture();
        }

        void ChangePicture()
        {
            if (!Enabled && disabledImage != null)
                Image = disabledImage;
            else if (mousedown && pressedImage != null)
                Image = pressedImage;
            else if (selected && selectedImage != null)
                Image = selectedImage;
            else if (mouseover && hoverImage != null)
                Image = hoverImage;
            else if (mouseover || selected)
                Image = ImageOver;
            else
                Image = ImageNormal;

            this.Invalidate();
        }

        private static Image Tint(Image source, Color color)
        {
            if (color.IsEmpty)
                return null;

            var original = new Bitmap(source);
            var tinted = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (var y = 0; y < original.Height; y++)
            {
                for (var x = 0; x < original.Width; x++)
                {
                    var pixel = original.GetPixel(x, y);
                    var luminance = (54 * pixel.R + 183 * pixel.G + 19 * pixel.B + 128) >> 8;
                    tinted.SetPixel(x, y, Color.FromArgb(pixel.A,
                        (color.R * luminance + 127) / 255,
                        (color.G * luminance + 127) / 255,
                        (color.B * luminance + 127) / 255));
                }
            }
            original.Dispose();
            return tinted;
        }

        private void DisposeStateImages()
        {
            if (hoverImage != null) hoverImage.Dispose();
            if (selectedImage != null) selectedImage.Dispose();
            if (pressedImage != null) pressedImage.Dispose();
            if (disabledImage != null) disabledImage.Dispose();
            hoverImage = selectedImage = pressedImage = disabledImage = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeStateImages();
            base.Dispose(disposing);
        }

    }
}
