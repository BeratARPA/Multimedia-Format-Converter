using System.Windows.Forms;

namespace WindowsFormsAppUI.Helpers
{
    public class DataGridViewProgressColumn : DataGridViewColumn
    {
        public DataGridViewProgressColumn()
        {
            this.CellTemplate = new DataGridViewProgressCell();
        }
    }
}
