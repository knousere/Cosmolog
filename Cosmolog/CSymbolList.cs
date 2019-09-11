using System;
using System.Collections.Generic;
using System.IO;
// (c)Richard E. Knouse 2015-1019

namespace Cosmolog
{
    class CSymbolList : List<CSymbol>
    {
        #region constructors
        public CSymbolList() : base()
        {
        }
        // Construct a deep copy
        public CSymbolList(CSymbolList thatSymbolList) : base()
        {
            for (int i = 0; i < thatSymbolList.Count; ++i)
            {
                CSymbol newSymbol = new CSymbol(thatSymbolList[i]);
                Add(newSymbol);
            }
        }
        #endregion
        #region Methods
        public int FindMatch(string strMatch)
        {
            int intFound = -1;
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].Label == strMatch)
                {
                    intFound = i;
                    break;
                }
            }
            return intFound;
        }

        // Read symbols and descriptions from a file
        public bool ReadSymbols(string strPath, string strLogPath)
        {
            bool bSuccess = true;
            StreamReader srSymbols = null;
            this.Clear();
            try
            {
                srSymbols = File.OpenText(strPath);
                while (srSymbols.Peek() >= 0)
                {
                    string strLine = srSymbols.ReadLine();
                    int intPos = strLine.IndexOf('\t');
                    string strSymbol = "";
                    string strDescription = "";
                    string strUnit = "";

                    if (intPos > -1)
                    {
                        strSymbol = strLine.Substring(0, intPos);
                        strDescription = strLine.Substring(intPos + 1);
                        intPos = strDescription.IndexOf('\t');
                        if (intPos > -1)
                        {
                            strUnit = strDescription.Substring(intPos + 1);
                            strDescription = strDescription.Substring(0, intPos);
                        }
                    }
                    else
                    {
                        strSymbol = strLine;
                    }
                    CSymbol thisSymbol = new CSymbol(strSymbol, "", strDescription, strUnit);
                    Add(thisSymbol);
                }
            }
            catch (Exception e)
            {
                bSuccess = false;
                string strLogEntry = " Error reading quantity file:" + e.Message;
                File.AppendAllText(strLogPath, strLogEntry);
            }
            finally
            {
                if (srSymbols != null)
                    srSymbols.Close();
            }
            return bSuccess;
        }

        #endregion
    }
}
