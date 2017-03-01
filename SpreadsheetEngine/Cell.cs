using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel; //for INotifyProperty

namespace SpreadsheetEngine
{
    public abstract class Cell : INotifyPropertyChanged
    {
        private int m_rowIndex;
        private int m_columnIndex;
        protected string cellText;
        protected string cellValue;
        public List<CellHelper> referencedBy = new List<CellHelper>();
        public List<CellHelper> references = new List<CellHelper>();

        public event PropertyChangedEventHandler PropertyChanged; //= delegate { };

        protected Cell(int rowIndex, int columnIndex) //protected constructor
        {
            m_rowIndex = rowIndex;
            m_columnIndex = columnIndex;
            cellText = "";
            cellValue = cellText; //This may need to change

        }

        public int RowIndex //only a getter, not a setter
        {
            get { return m_rowIndex; }
        }

        public int ColumnIndex //same, only a getter, not a setter
        {
            get { return m_columnIndex; }
        }
        public string cText
        {
            get { return cellText; }
            set
            {                     //I don't totally understand "value yet"...
                if (value == cellText) { return; } //The value entered is the same as the one currently in it

                //the value has been changed and the INotifyProperty needs to be activated
                cellText = value;
                NotifyPropertyChanged("CellText");
            }
        }

        protected string cValue    //now everyone can see/access cellValue, but only those inheriting from Cell can set it
        {
            get { return cellValue; }
            set { cellValue = value; } //this will only be called by a function inside Cell, because when Spreadsheet changes a cell value, it will be routed through CellHelper
        }

        public void NotifyPropertyChanged(string change) //I wrote this function in case in the future there are more than one kinds of property changes
                                                          //right now it's just cell text but I'm preparing for future iterations
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

        public void clearReferences()
        {
            this.references = new List<CellHelper>();
        }
    }

    public class CellHelper : Cell //This class will inherit from class and will allow Spreadsheet to call properties in Cell that it wouldn't be able to otherwise
    {
        public CellHelper(int row, int col) : base(row, col) { } //C# is weird. Wasn't able to instantiate object of Cell type so I had to create a helper object
        public string chValue
        {
            get { return cellValue; }
            set
            {
                cellValue = value;
                //NotifyPropertyChanged("Value updated");
                //cellText = cellValue; //took me about an our of debugging to realize I wasn't connecting the text with the value
            }
        }
    }

}