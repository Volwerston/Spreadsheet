using SpreadsheetEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spreadsheet_JPeven
{
    public partial class Form1 : Form
    {
        private static int num_rows = 50;
        private static int num_cols = 26;
        private Spreadsheet mySpread = new Spreadsheet(num_rows, num_cols);

        public Form1()
        {
            InitializeComponent();
        }

        private void SpreadsheetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int row_index = (sender as Cell).RowIndex;
            int col_index = (sender as Cell).ColumnIndex;
            dataGridView2.Rows[row_index].Cells[col_index].Value = (sender as CellHelper).chValue;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            for (int i = 65; i <= 90; i++) //This will convert i to an ASCII value for the 
            {
                dataGridView2.Columns.Add(Convert.ToChar(i).ToString(), Convert.ToChar(i).ToString()); //the .ToString() took me FOREVER to figure out
                                                                                                       //now the name, and shown value are identical
            }
            for (int i = 0; i < 50; i++)//This will likely need to change later on in the project
            {
                dataGridView2.Rows.Add();
            }
            for (int i = 0; i < this.dataGridView2.Rows.Count; i++) //This will add the headers, and is limited by the count, not hardcoded to 50
            {
                dataGridView2.Rows[i].HeaderCell.Value = (i + 1).ToString(); //(i+1) so it doesn't go 0-49
            }

            mySpread.PropertyChanged += SpreadsheetPropertyChanged;

            /*for (int i = 0; i < 50; i++)                           //subscribing
            {
                for (int j = 0; j < 26; j++)
                {
                    mySpread.cell_array[i, j].PropertyChanged += SpreadsheetPropertyChanged;
                }
            }*/
        }

        private void dataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            string msg = String.Format("Editing Cell at ({0}, {1})", e.ColumnIndex, e.RowIndex);
            dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = mySpread.cell_array[e.RowIndex, e.ColumnIndex].cText;
            this.Text = msg;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            int ranNum = rand.Next();

            for (int i = 0; i < 50; i++)                            //I think for now I have to do this part first BEFORE the Col A, which needs to be fixed later on
            {
                for (int j = 1; j < 2; j++)                         // I <3 off by one errors
                {
                    mySpread.cell_array[i, j].cText = "This is cell B" + (i + 1);
                }
            }

            for (int i = 0; i < 50; i++)                            //Filling 50 random cells with "This was Hard!"
            {
                int randomrow = rand.Next() % mySpread.RowCount;
                int randomcol = rand.Next() % (mySpread.ColumnCount - 2) + 2;
                mySpread.cell_array[randomrow, randomcol].cText = "I <3 321";
            }
            for (int i = 0; i < 50; i++)
            {
                mySpread.cell_array[i, 0].cText = "=B" + (i + 1);
            }
        }

        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string msg = String.Format("Finished Editing Cell at ({0}, {1})", e.ColumnIndex, e.RowIndex);
            mySpread.cell_array[e.RowIndex, e.ColumnIndex].cText = (string)dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = mySpread.cell_array[e.RowIndex, e.ColumnIndex].chValue;
            this.Text = msg;
        }
    }
}
