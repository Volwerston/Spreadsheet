using System.Collections.Generic;
using System.ComponentModel; 

namespace SpreadsheetEngine
{
    public abstract class Cell : INotifyPropertyChanged
    {
        protected string cellText;
        public List<CellHelper> referencedBy = new List<CellHelper>();
        public List<CellHelper> references = new List<CellHelper>();
        private int color;

        public event PropertyChangedEventHandler PropertyChanged; 

        protected Cell(int rowIndex, int columnIndex)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            cellText = "";
            cValue = cellText; 

        }

        public int RowIndex { get; }

        public int ColumnIndex { get; }

        public string cText
        {
            get => cellText;
            set
            {                     
                cellText = value;
                NotifyPropertyChanged("CellText");
            }
        }

        protected string cValue { get; set; }

        public int BGColor
        {
            get => color;
            set {
                color = value;
                NotifyPropertyChanged("CellColor");
            }
        }

        public void NotifyPropertyChanged(string change)                 
        {
            PropertyChanged(this, new PropertyChangedEventArgs(change));
        }

        public void removeReference(CellHelper c)
        {
            references.Remove(c);
        }

        public void addReference(CellHelper c)
        {
            references.Add(c);
        }

        public void removeReferenceBy(CellHelper c)
        {
            referencedBy.Remove(c);
        }

        public void addReferenceBy(CellHelper c)
        {
            referencedBy.Add(c);
        }
    }

    public class CellHelper : Cell 
    {
        public CellHelper(int row, int col) : base(row, col) { }

        public string chValue
        {
            get => cValue;
            set => cValue = value;
        }

        public List<CellHelper> clearReferences()
        {
            var cellList = references;
            foreach (var c in references)
            {
                c.removeReferenceBy(this);
            }
            references = new List<CellHelper>();
            return cellList;
        }
    }

}