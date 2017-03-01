using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel; //for INotifyProperty

namespace SpreadsheetEngine
{
    public class Spreadsheet : INotifyPropertyChanged
    {
        public CellHelper[,] cell_array;
        private int m_rows;
        private int m_col;

        public event PropertyChangedEventHandler PropertyChanged;

        public Spreadsheet(int rows, int col)
        {
            cell_array = new CellHelper[rows, col];
            m_rows = rows;
            m_col = col;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    cell_array[i, j] = new CellHelper(i, j);
                    cell_array[i, j].PropertyChanged += NotifyCellPropertyChanged;
                }
            }
        }

        private void NotifyCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((sender as Cell).cText.ToString().Length > 0)     //This means that there is something in the cell
            {
                if ((sender as Cell).cText[0] != '=')        //no need to do equation processing because it doesn't start with '='
                {
                    (sender as CellHelper).chValue = (sender as Cell).cText;
                    if ((sender as CellHelper).referencedBy.Count != 0) //some stuff references this
                    {
                        foreach (CellHelper c in (sender as CellHelper).referencedBy)
                        {
                            UpdateCellValue(c);
                        }
                    }
                }

                //THERE'S SOME ERROR WITH REMOVING REFERENCES TO THE CELLS. THE TEST I TRIED WAS A1 = 44, A2 = A1, A1 = 22
                //AND THAT UPDATED FINE. THE ISSUE WAS THEN CHANGING A2 = 33, SO IT'S NOT REFERENCING A1, BUT THEN WHEN I
                //CHANGED A1 IT STILL CHANGED

                else if((sender as Cell).cText[0] == '=')                                    //Text is an equation
                {
                    ExpTree tree = new ExpTree((sender as CellHelper).cText.Substring(1));    //create an expression tree
                    List<string> referencedCells = tree.GetVariables();                //This list contains all referenced cells. So "=A1+B2*3" would have ["A1","B2"]
                   
                    foreach (Cell c in (sender as CellHelper).references) //I might not reference you anymore
                    {
                        c.removeReferenceBy((sender as CellHelper));
                    }

                    (sender as CellHelper).references = new List<CellHelper>(); //clears reference list

                    //UpdateCellValue((sender as CellHelper));

                    foreach (string c_name in referencedCells)
                    {
                        string req_col = c_name.Substring(0, 1);     //to get the required column we need the celltext for the first value "=A6" -> "A"
                        string req_row = c_name.Substring(1);     //This will take the rest of the information, there's no length so it could read it "=A15" -> "15
                        int colInt = Convert.ToChar(req_col) - 65;                //gets the index based on the character
                        int rowInt = Convert.ToInt32(req_row) - 1;                //sets the index (and subtracts on so it's (0,49) instead of (1,50), matching the indexes

                        double cellVal = 0;
                        try
                        {
                           cellVal = Convert.ToDouble(cell_array[rowInt, colInt].chValue);
                        }
                        catch(FormatException err)
                        {
                            cellVal = 0;
                        }
                           

                        tree.SetVar(c_name, cellVal);                           //now the tree knows what A2 is

                        (sender as CellHelper).addReference(cell_array[rowInt, colInt]);      //We're telling this cell what it references
                        cell_array[rowInt, colInt].addReferenceBy((sender as CellHelper));    //The cell we're referencing now knows we're referencing them
                    }

                    (sender as CellHelper).chValue = Convert.ToString(tree.Eval());

                    foreach (CellHelper c in (sender as CellHelper).referencedBy)
                    {
                        UpdateCellValue(c);
                    }


                    //will need to set the value of all referenced values in that equation
                    //String[] vars = tree.Vars() that will return all "B1", "C3", etc that the expression tree needs
                    //for each of those strings, tree.setvar(...);




                    /*string req_col = (sender as Cell).cText.Substring(1, 1);     //to get the required column we need the celltext for the first value "=A6" -> "A"
                    string req_row = (sender as Cell).cText.Substring(2);     //This will take the rest of the information, there's no length so it could read it "=A15" -> "15
                    int colInt = Convert.ToChar(req_col) - 65;                //gets the index based on the character
                    int rowInt = Convert.ToInt32(req_row) - 1;                //sets the index (and subtracts on so it's (0,49) instead of (1,50), matching the indexes
                    //(sender as CellHelper).chValue = tree.Eval();
                    (sender as CellHelper).chValue = cell_array[rowInt, colInt].chValue;
                    //updated Dependencies*/
                }
            }
            else                                //I'm not totally sure when this would be triggered, error on input maybe?
            {
                (sender as CellHelper).chValue = (sender as Cell).cText;
                (sender as Cell).cText = "ERROR";
            }
            NotifyPropertyChanged((sender as CellHelper), new PropertyChangedEventArgs("Cell updated"));
        }

        //INotifyPropertyChanged

        private void UpdateCellValue(CellHelper c)
            //Will be called when a cell that cell c references is updated, or when a cell itself is updated.
            //Will create a new expression tree based on the text, and get the cell values
            //from the spreadsheet.
        {
            ExpTree tree = new ExpTree(c.cText.Substring(1));    //create an expression tree
            List<string> referencedCells = tree.GetVariables();                //This list contains all referenced cells. So "=A1+B2*3" would have ["A1","B2"]

            foreach (string c_name in referencedCells)
            {
                string req_col = c_name.Substring(0, 1);     //to get the required column we need the celltext for the first value "=A6" -> "A"
                string req_row = c_name.Substring(1);     //This will take the rest of the information, there's no length so it could read it "=A15" -> "15
                int colInt = Convert.ToChar(req_col) - 65;                //gets the index based on the character
                int rowInt = Convert.ToInt32(req_row) - 1;                //sets the index (and subtracts on so it's (0,49) instead of (1,50), matching the indexes

                double cellVal = Convert.ToDouble(cell_array[rowInt, colInt].chValue);
                tree.SetVar(c_name, cellVal);                           //now the tree knows what A2 is

                /*(sender as CellHelper).addReference(cell_array[rowInt, colInt]);      //We're telling this cell what it references
                cell_array[rowInt, colInt].addReferenceBy((sender as CellHelper));    //The cell we're referencing now knows we're referencing them*/
            }

            c.chValue = Convert.ToString(tree.Eval());
            NotifyPropertyChanged(c, new PropertyChangedEventArgs("Cell updated"));
        }

        public void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(sender, e);
        }

        public int ColumnCount
        {
            get { return m_col; }
        }

        public int RowCount
        {
            get { return m_rows; }
        }

        public Cell GetCell(int R, int C)
        {
            if (R < RowCount && C < ColumnCount)
                return cell_array[R, C]; //if this is out of range, it will just return null so it should be okay
            else
                return null;
        }




        /*
         * void updateCell(cell c){
         * 
         *  updateTableDelete(c); lookup variables from the old tree and remove yourself from each cell
         * }
         * UpdateCellValue(c); //will clear the tree/value
         * 
         * UpdateTableAdd(c)
         * foreach depented decll "dep"
         *  updateCell(dep);
         * 
         * 
         * 
         * 
         */

    }
}
