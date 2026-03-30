using System.Drawing.Drawing2D;

namespace Pos.WinFormsUI.Forms
{
    public class GradientInfoPanelBuilder
    {
        private class GradientPanelState
        {
            public List<(string Header, string Value)> Fields { get; set; } = [];
            public int RecordCount { get; set; }
            public Color Start { get; set; }
            public Color End { get; set; }
        }

        public static void Build(
            Panel panel,
            List<(string Header, string Value)> fields,
            int recordCount,
            Color gradientStart,
            Color gradientEnd)
        {
            if (panel == null || fields == null) return;

            var state = new GradientPanelState
            {
                Fields = fields,
                RecordCount = recordCount,
                Start = gradientStart,
                End = gradientEnd
            };

            panel.Tag = state;

            panel.Resize -= PanelResize;
            panel.Resize += PanelResize;

            Render(panel, state);
        }

        private static void PanelResize(object? sender, EventArgs e)
        {
            if (sender is Panel panel && panel.Tag is GradientPanelState state)
            {
                Render(panel, state);
            }
        }

        private static void Render(Panel panel, GradientPanelState state)
        {
            var fields = state.Fields;

            panel.Controls.Clear();
            panel.BackColor = Color.Transparent;

            panel.Paint -= OnPanelPaint;
            panel.Paint += OnPanelPaint;

            int count = Math.Min(state.RecordCount, fields.Count);
            if (count == 0) return;

            int slotWidth = panel.Width / count;
            int padding = 20;

            int headerH = 18;
            int gap = 6;

            for (int i = 0; i < count; i++)
            {
                int slotX = i * slotWidth;
                int labelLeft = slotX + padding;
                int labelWidth = slotWidth - padding * 2;

                ContentAlignment valueAlign = ContentAlignment.TopLeft;

                // Divider
                if (i > 0)
                {
                    var divider = new Panel
                    {
                        BackColor = Color.FromArgb(80, Color.White),
                        Width = 1,
                        Height = (int)(panel.Height * 0.55),
                        Left = slotX,
                        Top = (panel.Height - (int)(panel.Height * 0.55)) / 2
                    };

                    panel.Controls.Add(divider);
                }

                var valueFont = new Font("Segoe UI", 13F, FontStyle.Bold);

                Size measuredSize = TextRenderer.MeasureText(
                    fields[i].Value,
                    valueFont,
                    new Size(labelWidth, 0),
                    TextFormatFlags.WordBreak);

                int valueHeight = measuredSize.Height;

                int totalHeight = headerH + gap + valueHeight;
                int blockTop = (panel.Height - totalHeight) / 2;

                // Header
                var lblHeader = new Label
                {
                    Text = fields[i].Header.ToUpper(),
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(220, 255, 255, 255),
                    Width = labelWidth,
                    Height = headerH,
                    Left = labelLeft,
                    Top = blockTop,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent
                };

                // Value
                var lblValue = new Label
                {
                    Text = fields[i].Value,
                    Font = valueFont,
                    ForeColor = Color.White,
                    Width = labelWidth,
                    Height = valueHeight,
                    Left = labelLeft,
                    Top = blockTop + headerH + gap,
                    TextAlign = valueAlign,
                    BackColor = Color.Transparent,
                    AutoSize = false
                };

                panel.Controls.Add(lblHeader);
                panel.Controls.Add(lblValue);
            }
        }

        private static void OnPanelPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel || panel.Tag is not GradientPanelState state)
                return;

            using var brush = new LinearGradientBrush(
                panel.ClientRectangle,
                state.Start,
                state.End,
                LinearGradientMode.Vertical);

            e.Graphics.FillRectangle(brush, panel.ClientRectangle);
        }
    }
}