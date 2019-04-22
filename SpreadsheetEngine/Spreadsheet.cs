using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel; //for INotifyProperty
using System.Xml;           //maybe?
using System.IO;
using System.Drawing;

//Jackson Peven, 11382715

namespace SpreadsheetEngine
{
    public class Spreadsheet : INotifyPropertyChanged
    {
        public CellHelper[,] cell_array;
        private Stack<Restore> undoStack = new Stack<Restore>();
        private Stack<Restore> redoStack = new Stack<Restore>();

        public event PropertyChangedEventHandler PropertyChanged;

        public Spreadsheet(int rows, int col)
        {
            cell_array = new CellHelper[rows, col];
            RowCount = rows;
            ColumnCount = col;
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
            if(e.PropertyName == "CellColor")
            {
                NotifyPropertyChanged((sender as CellHelper), new PropertyChangedEventArgs("CellColor"));
                return;
            }
            if (e.PropertyName == "CellText")
            {
                
                if ((sender as Cell).cText == "" || (sender as Cell).cText == null)
                {
                    (sender as CellHelper).chValue = "";
                    var previously_references = (sender as CellHelper).clearReferences();
                    foreach(CellHelper c in previously_references)
                    {
                        UpdateCellValue(c);
                    }
                    if ((sender as CellHelper).referencedBy.Count != 0) 
                    {
                        foreach (CellHelper c in (sender as CellHelper).referencedBy)
                        {
                            UpdateCellValue(c);
                        }
                    }
                }
                else if ((sender as Cell).cText[0] == '=')                                    
                {
                    if (badRef((sender as CellHelper)))                                     
                    {
                        (sender as CellHelper).chValue = "!(bad reference)";
                        return;
                    }
                    if (selfRef((sender as CellHelper)))
                    {
                        (sender as CellHelper).chValue = "!(self reference)";
                        return;
                    }
                    if (circularRef((sender as CellHelper), new HashSet<CellHelper>()))
                    {
                        (sender as CellHelper).chValue = "!(circular reference)";
                        return;
                    }
                    
                    var tree = new ExpTree((sender as CellHelper).cText.Substring(1));   
                    var referencedCells = tree.GetVariables();               

                    var previously_references = (sender as CellHelper).clearReferences();
                    
                    foreach (var c_name in referencedCells)
                    {
                        var req_col = c_name.Substring(0, 1);    
                        var req_row = c_name.Substring(1);     
                        var colInt = Convert.ToChar(req_col) - 65;                
                        var rowInt = Convert.ToInt32(req_row) - 1;              

                        double cellVal = 0;
                        try
                        {
                            cellVal = Convert.ToDouble(cell_array[rowInt, colInt].chValue);
                        }
                        catch (FormatException)
                        {
                            cellVal = 0;
                        }

                        tree.SetVar(c_name, cellVal);                          

                        (sender as CellHelper).addReference(cell_array[rowInt, colInt]);   
                        cell_array[rowInt, colInt].addReferenceBy((sender as CellHelper));    
                    }

                    (sender as CellHelper).chValue = Convert.ToString(tree.Eval());

                    foreach (var c in (sender as CellHelper).referencedBy)
                    {
                        UpdateCellValue(c);
                    }
                }
                else
                {
                    var previously_references = (sender as CellHelper).clearReferences();
                    
                    (sender as CellHelper).chValue = (sender as Cell).cText;

                    if ((sender as CellHelper).referencedBy.Count != 0) 
                    {
                        foreach (CellHelper c in (sender as CellHelper).referencedBy)
                        {
                            UpdateCellValue(c);
                        }
                    }
                    foreach (CellHelper c in previously_references)
                    {
                        UpdateCellValue(c);
                    }
                }
                
                NotifyPropertyChanged((sender as CellHelper), new PropertyChangedEventArgs("CellValue"));
            }
        }

        private void UpdateCellValue(CellHelper c)
         {
            if (string.IsNullOrEmpty(c.cText))
            {
                c.chValue = c.cText;
            }
            else
            {
                var tree = new ExpTree(c.cText.Substring(1));   
                var referencedCells = tree.GetVariables();   

                foreach (var c_name in referencedCells)
                {
                    var req_col = c_name.Substring(0, 1);    
                    var req_row = c_name.Substring(1);     
                    var colInt = Convert.ToChar(req_col) - 65;              
                    var rowInt = Convert.ToInt32(req_row) - 1;               

                    double cellVal;
                    if (string.IsNullOrEmpty(cell_array[rowInt, colInt].chValue) ||
                        cell_array[rowInt, colInt].chValue == "!(bad reference)" ||
                        cell_array[rowInt, colInt].chValue == "!(self reference)" ||
                        cell_array[rowInt, colInt].chValue == "!(circular reference)")
                    {
                        cellVal = 0;
                    }
                    else
                    {
                        cellVal = Convert.ToDouble(cell_array[rowInt, colInt].chValue);
                    }
                        
                    tree.SetVar(c_name, cellVal);         
                }

                c.chValue = Convert.ToString(tree.Eval());
                foreach (var c2 in c.referencedBy)
                {
                    UpdateCellValue(c2);
                }
            }
            NotifyPropertyChanged(c, new PropertyChangedEventArgs("CellValue"));
        }

        private bool selfRef(CellHelper c)                              
        {                                                               
            if (string.IsNullOrEmpty(c.cText))
            {
                return false;
            }

            if (c.cText[0] != '=')                                 
            {
                return false;
            }

            var tree = new ExpTree(c.cText.Substring(1));    
            var nameList = tree.GetVariables();                

            var cellList = new List<CellHelper>();
            foreach (var n in nameList)                             
            {                                                           
                cellList.Add(stringToCell(n));  
            }

            return cellList.Contains(c);
        }

        private bool badRef(CellHelper c)
        {
            if (string.IsNullOrEmpty(c.cText))
            {
                return false;
            }

            if (c.cText[0] != '=')
            {
                return false;
            }

            var tree = new ExpTree(c.cText.Substring(1));    
            var nameList = tree.GetVariables();

            var cellList = new List<CellHelper>();
            foreach (var n in nameList)
            {
                cellList.Add(stringToCell(n));
            }

            return cellList.Contains(null);
        }

        private bool circularRef(CellHelper c, ISet<CellHelper> hs)
        {
            if (string.IsNullOrEmpty(c.cText))
            {
                return false;
            }

            if(c.cText[0] != '=')
            {
                return false;
            }

            var tree = new ExpTree(c.cText.Substring(1));    
            var nameList = tree.GetVariables();

            if(hs.Contains(c))                                 
            {
                return true;
            }

            hs.Add(c);                                          

            var cellList = new List<CellHelper>();
            foreach(var n in nameList)
            {
                cellList.Add(stringToCell(n));
            }

            foreach(var ce in cellList)
            {
                if (string.IsNullOrEmpty(ce?.cText)) continue;

                var new_hs = new HashSet<CellHelper>(hs);
 
                if (circularRef(ce, new_hs))                      
                {
                    return true;
                }
            }

            return false;
        }

        private CellHelper stringToCell(string name)
        {
            int colInt, rowInt;
            try
            {
                var reqCol = name.Substring(0, 1); 
                var reqRow = name.Substring(1);   
                colInt = Convert.ToChar(reqCol) - 65;  
                rowInt = Convert.ToInt32(reqRow) - 1;   
            }
            catch
            {
                return null;
            }

            if (colInt > 26 || rowInt > 50)
                return null;

            return cell_array[rowInt, colInt];
        }

        public void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Cell GetCell(int R, int C)
        {
            if (R < RowCount && C < ColumnCount)
                return cell_array[R, C]; //if this is out of range, it will just return null so it should be okay
            else
                return null;
        }

        public void Redo()
        {
            Restore item = redoStack.Pop();
            if(item is ColorRestore)
            {
                ColorRestore newRestore = new ColorRestore((item as ColorRestore).nColor, (item as ColorRestore).oColor, (item as ColorRestore).rowIndexArray, (item as ColorRestore).colIndexArray);
                newRestore.BText = "Background Color";
                undoStack.Push(newRestore);
                for (int i = 0; i < (item as ColorRestore).rowIndexArray.Length; i++)
                {
                    cell_array[(item as ColorRestore).rowIndexArray[i], (item as ColorRestore).colIndexArray[i]].BGColor = (item as ColorRestore).oColor[i];
                }
            }
            else if(item is TextRestore)
            {
                TextRestore newRestore = new TextRestore((item as TextRestore).neText, (item as TextRestore).orText, (item as TextRestore).rowIndex, (item as TextRestore).colIndex);
                newRestore.BText = "Cell Text";
                undoStack.Push(newRestore);
                cell_array[(item as TextRestore).rowIndex, (item as TextRestore).colIndex].cText = (item as TextRestore).orText;
            }
            //this will be for future undo/redo actions
        }

        public void Undo()
        {
            Restore item = undoStack.Pop();
            if (item is ColorRestore)
            {
                ColorRestore newRestore = new ColorRestore((item as ColorRestore).nColor, (item as ColorRestore).oColor, (item as ColorRestore).rowIndexArray, (item as ColorRestore).colIndexArray);
                newRestore.BText = "Background Color";
                redoStack.Push(newRestore);
                for (int i = 0; i < (item as ColorRestore).rowIndexArray.Length; i++)
                {
                    cell_array[(item as ColorRestore).rowIndexArray[i], (item as ColorRestore).colIndexArray[i]].BGColor = (item as ColorRestore).oColor[i];
                }
            }
            else if (item is TextRestore)
            {
                TextRestore newRestore = new TextRestore((item as TextRestore).neText, (item as TextRestore).orText, (item as TextRestore).rowIndex, (item as TextRestore).colIndex);
                newRestore.BText = "Cell Text";
                redoStack.Push(newRestore);
                cell_array[(item as TextRestore).rowIndex, (item as TextRestore).colIndex].cText = (item as TextRestore).orText;
            }
            //this will be for future undo/redo actions
        }

        public void saveToXML(FileStream fs) //maybe this needs to return an XML document...
        {
            var xml = new XmlDocument();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;
            

            using (XmlWriter xWriter = XmlWriter.Create(fs, settings))
            {
                //xWriter.WriteStartDocument(); 
                xWriter.WriteStartElement("Spreadsheet"); //This will create the "root"
                foreach(Cell c in cell_array)
                {
                    if(c.cText != "" || c.BGColor != 0)    //it has been edited
                    {
                        string cellName = Convert.ToString(Convert.ToChar(c.ColumnIndex + 65)) + Convert.ToString(c.RowIndex+1);
                        xWriter.WriteStartElement("cell");
                        xWriter.WriteAttributeString("name", cellName);

                        xWriter.WriteStartElement("bgcolor");
                        xWriter.WriteString(ColorTranslator.ToHtml(Color.FromArgb(c.BGColor)));
                        xWriter.WriteEndElement();          //for bgcolor

                        xWriter.WriteStartElement("text");
                        xWriter.WriteString(c.cText);
                        xWriter.WriteEndElement();          //for cText
                        
                        xWriter.WriteEndElement();          //for cell
                    }
                }

                xWriter.WriteEndElement(); // for spreadsheet
                                           //xWriter.WriteEndDocument();

                //return xWriter.ToString();
                /*xml.Load(xWriter.ToString());
                xWriter.Close();*/
            }
        }

        public void loadFromXML(FileStream xml)
        {
            string cname = "";
            int bgcolor = 0;
            string text =  "";


            clearSpreadsheet();
            undoStack.Clear();
            redoStack.Clear();

            //var xmlNode = new XmlNode("spreadsheet");
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(xml, settings);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "cell")
                        {
                            reader.MoveToNextAttribute();
                            cname = reader.Value;
                            break;
                        }
                        else if (reader.Name == "bgcolor")
                        {
                            reader.Read();
                            if(reader.Value != "#000000")
                                bgcolor = ColorTranslator.FromHtml(reader.Value).ToArgb();
                        }
                        else if (reader.Name == "text")
                        {
                            reader.Read();
                            text = reader.Value;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "cell")
                        {
                            addCell(cname, text, bgcolor);
                            cname = "";
                            text = "";
                            bgcolor = 0;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /* This function will be called from load cell where it will create and add a cell from the given parameters
         * */
        private void addCell(string name, string text, int bgcolor)
        {
            string req_col = name.Substring(0, 1);     //to get the required column we need the celltext for the first value "=A6" -> "A"
            string req_row = name.Substring(1);     //This will take the rest of the information, there's no length so it could read it "=A15" -> "15
            int colInt = Convert.ToChar(req_col) - 65;                //gets the index based on the character
            int rowInt = Convert.ToInt32(req_row) - 1;

            if(text != "")
                cell_array[rowInt, colInt].cText = text;
            if(bgcolor != 0)
                cell_array[rowInt, colInt].BGColor = bgcolor;
        }

        private void clearSpreadsheet()
            //This function will be called on a load action and will clear the current spreadsheet out. Should only be called by load
        {
            for(int i = 0; i < RowCount; i++)
            {
                for(int j = 0; j < ColumnCount; j++)
                {
                    cell_array[i, j].cText = ""; //this might not trigger the inotify property to update the datagridview value in the GUI
                    cell_array[i, j].BGColor = 0;
                }
            }
        }


        public Restore redoTop()
        {
            return redoStack.Peek();
        }
        public Restore undoTop()
        {
            return undoStack.Peek();
        }
        public void redoPush(Restore r)
        {
            redoStack.Push(r);
        }
        public void undoPush(Restore r)
        {
            undoStack.Push(r);
        }
        public void redoPop()
        {
            redoStack.Pop();
        }
        public void undoPop()
        {
            undoStack.Pop();
        }
        public int undoCount()
        {
            return undoStack.Count;
        }
        public int redoCount()
        {
            return redoStack.Count;
        }
    }
}
