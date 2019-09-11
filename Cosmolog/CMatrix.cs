using System;
using System.Collections.Generic;
using System.IO;
// (c)Richard E. Knouse 2015-1019

namespace Cosmolog
{
    class CMatrix : List<CMatrixCell> 
    {
        #region constructors
        public CMatrix()
        {
        }
        #endregion
        #region members
        #endregion
        #region methods
        public bool WriteMatrix(string strPath, string strLogPath)
        {
            bool bSuccess = true;
            StreamWriter swMatrix = null;
            string strLine;
            try
            {
                swMatrix = new StreamWriter(strPath);
                for (int i = 0; i < this.Count; ++i)
                {
                    CMatrixCell thisCell = this[i];
                    strLine = thisCell.DumpToString();
                    swMatrix.WriteLine(strLine);
                }
            }
            catch (Exception e)
            {
                bSuccess = false;
                string strLogEntry = "\r\n Error writing matrix file:" + e.Message;
                File.AppendAllText(strLogPath, strLogEntry);
            }
            finally
            {
                if (swMatrix != null)
                    swMatrix.Close();
            }

            return bSuccess;
        }
        // This is a candidate group to define a quantity.
        // 1. Determine if the quantity already exists.
        // 2. If new then determine if it has enough members and sufficient diversity
        //    to define a new quantity.
        // 3. Assign the quantity to every matrix cell in this group.
        public void QualifyGroup(int intFirst, int intCount, CQuantityList myQuantityList)
        {
            CStringList labelList = new CStringList();

            bool bGoodQuantity = true;
            bool bIsExact = false;      // set flag if this is new quantity
            for (int i = intFirst; i < intFirst + intCount; ++i)
            {
                CMatrixCell myMatrixCell = this[i];
                if (myMatrixCell.IsExact)
                    bIsExact = true;
                CQuantity qtyX = myMatrixCell.PtrX.Qty;
                CQuantity qtyY = myMatrixCell.PtrY.Qty;
                if (!qtyX.IsInteger)
                    labelList.Add(qtyX.Label);
                if (!qtyY.IsInteger)
                    labelList.Add(qtyY.Label);
            }

            if (labelList.Count > 1)
            {
                // This is a qualifying quantity
                double dblLog = this[intFirst].Number.Log;
                int intQtyIndex = myQuantityList.FindEquivalent(this[intFirst].Number);
                bool bNewQuantity = (intQtyIndex < 0);
                bGoodQuantity = true;
                CQuantity thisQuantity;
                if (bNewQuantity)
                {
                    string strLabel = "X_" + myQuantityList.Count.ToString();
                    thisQuantity = new CQuantity(myQuantityList, strLabel, dblLog, false);
                    thisQuantity.IsExact = bIsExact;
                    myQuantityList.Add(thisQuantity);
                }
                else
                {
                    thisQuantity = myQuantityList[intQtyIndex];
                }

                CSymbol symbolInverse = thisQuantity.SymbolInverse();
                CQuantity inverseQuantity = thisQuantity.Inverse(symbolInverse.Label);
                int intInverse = myQuantityList.FindEquivalent(inverseQuantity);
                bool bNewInverse = (intInverse < 0);
                if (bNewInverse)
                {
                    myQuantityList.Add(inverseQuantity);
                    inverseQuantity.SetInverse(ref thisQuantity);
                }
                else
                {
                    // inverse quantity already exists in the list
                    inverseQuantity = myQuantityList[intInverse];
                    if (bNewQuantity)
                    {
                        if (inverseQuantity.IsInteger)
                        {
                            // never add inverses to integers
                            bGoodQuantity = false;
                            myQuantityList.RemoveAt(thisQuantity.Index);
                        }
                        else
                        {
                            thisQuantity.Symbol = inverseQuantity.SymbolInverse();
                            thisQuantity.SetInverse(ref inverseQuantity);
                            thisQuantity.IsExact = inverseQuantity.IsExact;
                        }
                    }
                }

                if (bGoodQuantity)
                {
                    // assign the quantity to every matrix cell in the group
                    for (int i = intFirst; i < intFirst + intCount; ++i)
                    {
                        CMatrixCell myMatrixCell = this[i];
                        myMatrixCell.PtrQty = new CQtyPointer(thisQuantity);
                    }
                }
            }
        }
        #endregion
    }
}
