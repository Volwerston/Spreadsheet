using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEngine
{
    public class Restore
    {
        public string BText; //This is the text that will be shown on the button
    }

    public class TextRestore: Restore
    {
        private string origText;
        private string newText;
        int rIndex;
        int cIndex;

        public TextRestore(string oText, string nText, int rowIndex, int colIndex)
        {
            origText = oText;
            newText = nText;
            rIndex = rowIndex;
            cIndex = colIndex;
        }

        public string orText
        {
            get { return origText; }
        }

        public string neText
        {
            get { return newText; }
        }

        public int rowIndex
        {
            get { return rIndex; }
        }

        public int colIndex
        {
            get { return cIndex; }
        }

    }

    public class ColorRestore : Restore
    {
        int[] oldColor;
        int[] newColor;
        int[] rIndexArr;
        int[] cIndexArr;
        
        public ColorRestore(int[] oColor, int[] nColor, int[] rowIndexArray, int[] colIndexArray)
        {
            oldColor = oColor;
            newColor = nColor;
            rIndexArr = rowIndexArray;
            cIndexArr = colIndexArray;
        }

        public int[] oColor
        {
            get { return oldColor; }
        }

        public int[] nColor
        {
            get { return newColor; }
        }

        public int[] colIndexArray
        {
            get { return cIndexArr; }

        }
        public int[] rowIndexArray
        {
            get { return rIndexArr; }

        }


    }

}
