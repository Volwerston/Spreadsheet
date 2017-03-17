using SpreadsheetEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Jackson Peven, 11382715

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
            //These if statements will allow additions to more types of notification changes as updates
            //happen, making the code more reusable in the future
            if(e.PropertyName == "CellColor")
            {
                Color newColor;
                Color weirdHack;
                newColor = Color.FromArgb((sender as CellHelper).BGColor);

                if (newColor.A == 0 && newColor.R == 0 && newColor.G == 0 && newColor.B == 0)
                    dataGridView2.Rows[row_index].Cells[col_index].Style.BackColor = Color.White;
                //weirdHack = Color.FromArgb(255, newColor.R, newColor.G, newColor.B);
                else
                {
                    weirdHack = Color.FromArgb(255, newColor.R, newColor.G, newColor.B);

                    dataGridView2.Rows[row_index].Cells[col_index].Style.BackColor = weirdHack;
                }
            }
            if(e.PropertyName == "CellValue")
            {
                dataGridView2.Rows[row_index].Cells[col_index].Value = (sender as CellHelper).chValue;
            }

            //Undo stack
            if (mySpread.undoCount() == 0)
            {
                undoToolStripMenuItem.Enabled = false;
                undoToolStripMenuItem.Text = "Undo...";
            }
            else
            {
                undoToolStripMenuItem.Enabled = true;
                undoToolStripMenuItem.Text = "Undo " + mySpread.undoTop().BText + " change";
            }
            
            //Redo stack
            if (mySpread.redoCount() == 0)
            {
                redoToolStripMenuItem.Enabled = false;
                redoToolStripMenuItem.Text = "Redo...";
            }
            else
            {
                redoToolStripMenuItem.Enabled = true;
                redoToolStripMenuItem.Text = "Redo " + mySpread.redoTop().BText + " change";
            }
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
            redoToolStripMenuItem.Text = "Redo...";
            undoToolStripMenuItem.Text = "Undo...";
            undoToolStripMenuItem.Enabled = false; //initially nothing can be done
            redoToolStripMenuItem.Enabled = false;
        }

        private void dataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = mySpread.cell_array[e.RowIndex, e.ColumnIndex].cText;
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
            /*if ((string)dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == "")
            {
                mySpread.cell_array[e.RowIndex, e.ColumnIndex].cText = 
            }
            else
            */
            TextRestore textRestore = new TextRestore(mySpread.cell_array[e.RowIndex, e.ColumnIndex].cText, (string)dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, e.RowIndex, e.ColumnIndex);
            textRestore.BText = "Cell Text";
            mySpread.cell_array[e.RowIndex, e.ColumnIndex].cText = (string)dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = mySpread.cell_array[e.RowIndex, e.ColumnIndex].chValue;
            mySpread.undoPush(textRestore);
            undoToolStripMenuItem.Enabled = true;
            undoToolStripMenuItem.Text = "Undo Cell Text Change";
            //this.Text = msg;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mySpread.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mySpread.Redo();
        }

        private void chooseBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog CDialogue = new ColorDialog();
            Color selected = Color.White;
            int[] origColor = new int[dataGridView2.SelectedCells.Count];
            int[] newColor = new int[dataGridView2.SelectedCells.Count];
            int[] newRow = new int[dataGridView2.SelectedCells.Count];
            int[] newCol = new int[dataGridView2.SelectedCells.Count];
            //Cell[] cellArr = new Cell[dataGridView2.SelectedCells.Count];

            if (CDialogue.ShowDialog() == DialogResult.OK)
            {
                selected = CDialogue.Color;
            }
            int i = 0;
            int test = selected.ToArgb();
            Color test2 = Color.FromArgb(test);
            foreach(DataGridViewTextBoxCell c in dataGridView2.SelectedCells)
            {
                origColor[i] = c.Style.BackColor.ToArgb();
                newColor[i] = CDialogue.Color.ToArgb();
                newRow[i] = c.RowIndex;
                newCol[i] = c.ColumnIndex;
                //cellArr[i] = mySpread.cell_array[c.RowIndex, c.ColumnIndex];

                mySpread.cell_array[c.RowIndex, c.ColumnIndex].BGColor = newColor[i];
                //this updates in the Logic layer which in turn will notify the UI
                i++;
            }
            Restore colRestore = new ColorRestore(origColor, newColor, newRow,newCol);
            colRestore.BText = "Background Color";
            undoToolStripMenuItem.Enabled = true;
            undoToolStripMenuItem.Text = "Undo Background Color Change";
            mySpread.undoPush(colRestore);
        }

        /* The code for Save/Load is taken from my PA3. The instructions say to keep the work in the logic layer, so what I think I'll do is
         * open a stream for reading/writing and then pass that the to another function in the logic layer to deal with the actual saving and loading
         * */

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();

            if (SFD.ShowDialog() == DialogResult.OK)     //Making sure everything is alright.
            {
                StreamWriter myStream = new StreamWriter(File.Create(SFD.FileName));    //Creating a streamWriter that will in turn create a new file from the 
                //myStream.Write(textBox1.Text);                                      //Takes everything in the textbox and writes it to a file that was just created
                //myStream.Dispose();                                                 //Disposing the streamWriter because we don't need it anymore
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();

            if (OFD.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader myStream = new StreamReader(OFD.FileName))
                {
                    //LoadText(myStream);//.ReadToEnd();
                }
            }
        }
    }
}
