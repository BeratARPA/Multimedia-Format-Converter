using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsAppUI.Helpers
{
    public class DataGridViewProgressCell : DataGridViewTextBoxCell
    {
        public DataGridViewProgressCell()
        {
            this.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            if (value is int progressValue)
            {
                var progressBarBounds = new Rectangle(cellBounds.X + 2, cellBounds.Y + 2, cellBounds.Width - 4, cellBounds.Height - 4);
                var progressBarValue = Math.Max(0, Math.Min(progressValue, 100));

                ProgressBarRenderer.DrawHorizontalBar(graphics, progressBarBounds);
                var progressBarChunk = new Rectangle(progressBarBounds.X, progressBarBounds.Y, progressBarBounds.Width * progressBarValue / 100, progressBarBounds.Height);
                ProgressBarRenderer.DrawHorizontalChunks(graphics, progressBarChunk);
            }
        }
    }
}
