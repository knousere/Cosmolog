using System;
using System.Collections.Generic;
using System.IO;

// The list of quantities. There is only one master list in this project attached to the main form. 
// Do not implement Sort because the index of each element must remain constant.
namespace Cosmolog
{
    class CQuantityList : List<CQuantity>
    {
        #region constructors
        public CQuantityList() : base()
        {
        }
        #endregion
        #region methods
        // Called in the process of normalizing expressions and factorlists
        public CQuantity ReplaceIntegerQuantity(CNumber thatNumber)
        {
            CQuantity thisQuantity = null;
            int intIndex = this.FindEquivalent(thatNumber);
            if (intIndex > -1)
            {
                thisQuantity = this[intIndex];
            }
            else
            {
                thisQuantity = new CQuantity(this, true, thatNumber.Value.ToString(), thatNumber.Value);
                this.Add(thisQuantity);
            }
            return thisQuantity;
        }
        public void RepairIndices()
        {
            int q;
            for (q = 0; q < this.Count; ++q)
            {
                CQuantity thisQuantity = this[q];
                thisQuantity.ResetIndex();
                thisQuantity.ResetInverse();
            }
            for (q = 0; q < this.Count; ++q)
            {
                CQuantity thisQuantity = this[q];
                if (thisQuantity.Index == -1)
                {
                    thisQuantity.Index = q;
                    double dblInverseLog = -thisQuantity.Log;
                    CNumber inverseNumber = new CNumber(dblInverseLog, false);
                    int intInverseIndex = this.FindEquivalent(inverseNumber);
                    if (intInverseIndex > -1)
                    {
                        CQuantity inverseQty = this[intInverseIndex];
                        if (inverseQty.Index == -1)
                        {
                            inverseQty.Index = intInverseIndex;
                        }
                        thisQuantity.SetInverse(ref inverseQty);
                    }
                }
                thisQuantity.ExpressionList.Sort();
            }
        }
        // called by Form1.RefreshSymbolCombo 
        public void MakeStringArray(ref string[] stringArray)
        {
            for (int q = 0; q < this.Count; ++q)
            {
                CQuantity thisQuantity = this[q];
                string strEntry = thisQuantity.SimpleLabel + "\t " + thisQuantity.Description;
                stringArray[q] = strEntry;
            }
        }
        public void SetAllDescriptions(CSymbolList thatSymbolList)
        {
            for (int i = 0; i < Count; ++i)
            {
                CQuantity thisQuantity = this[i];
                thisQuantity.SetDescription(thatSymbolList);
            }
        }
        // return index of equivalent if found, else -1
        public int FindEquivalent(CQuantity thatQuantity)
        {
            int intIndex = -1;
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].IsEquivalent(thatQuantity))
                {
                    intIndex = i;
                    break;
                }
            }
            return intIndex;
        }
        // return index of equivalent if found, else -1
        public int FindEquivalent(CNumber thatNumber)
        {
            int intIndex = -1;
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].IsEquivalent(thatNumber))
                {
                    intIndex = i;
                    break;
                }
            }
            return intIndex;
        }
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
        public int FindSimpleMatch(string strMatch)
        {
            int intFound = -1;
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].SimpleLabel == strMatch)
                {
                    intFound = i;
                    break;
                }
            }
            return intFound;
        }
        public void AddUnique(CQuantity thisQuantity)
        {
            int intFound = FindMatch(thisQuantity.Label);
            if (intFound == -1)
                Add(thisQuantity);
        }
        // read values from a file
        public bool ReadValues(string strPath, string strLogPath)
        {
            bool bSuccess = true;
            StreamReader srValues = null;
            this.Clear();
            Add(new CQuantity(this, true, "1", 1, "One"));
            Add(new CQuantity(this, true, "2", 2, "Two"));
            Add(new CQuantity(this, true, "4", 4, "Four"));
            try
            {
                srValues = File.OpenText(strPath);
                while (srValues.Peek() >= 0)
                {
                    string strLine = srValues.ReadLine();
                    CQuantity thisQuantity = new CQuantity(this, strLine);
                    Add(thisQuantity);
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
                if (srValues != null)
                    srValues.Close();
            }
            return bSuccess;
        }
        // Use the average of associated expressions to extend the precision of each quantity.
        public void ExtendPrecision()
        {
            for (int i = 0; i < this.Count; ++i)
                this[i].CalcCandidate();
            for (int i = 0; i < this.Count; ++i)
                this[i].ApplyCandidate();
        }
        #endregion
        #region wrappers
        public new void Add(CQuantity thisQuantity)
        {
            thisQuantity.Index = this.Count;
            base.Add(thisQuantity);
        }

        #endregion
    }
}
