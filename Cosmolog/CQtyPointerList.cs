using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Cosmolog
{
    // This is meant to act as a sorted index into CQuantityList ordered by value.
    class CQtyPointerList : List<CQtyPointer>
    {
        #region constructors
        public CQtyPointerList() : base()
        {
            _isDirty = false;
        }

        // This constructor makes a deep copy
        public CQtyPointerList(ref CQtyPointerList thatQtyPointerList) : base()
        {
            int i;
            for (i = 0; i < thatQtyPointerList.Count; ++i)
            {
                CQtyPointer thisQtyPointer = new CQtyPointer(thatQtyPointerList[i]);
                Add(thisQtyPointer);
            }
            _isDirty = thatQtyPointerList.IsDirty;
        }

        // This constructor makes an index that points into a Quantity List
        public CQtyPointerList(ref CQuantityList thatQuantityList) : base()
        {
            int i;
            for (i = 0; i < thatQuantityList.Count; ++i)
            {
                CQtyPointer thisQtyPointer = new CQtyPointer(thatQuantityList[i]);
                Add(thisQtyPointer);
            }
            Sort();
        }
        #endregion
        #region members
        protected bool _isDirty; // true if terms have been added since last sort
        #endregion
        #region properties
        bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }
        #endregion
        #region wrappers
        public new void Add(CQtyPointer thisQtyPointer)
        {
            base.Add(thisQtyPointer);
            _isDirty = true;
        }
        // If match remove one occurance and return true, else return false.
        // Adjust the dirty flag.
        public bool Remove(string strLabel)
        {
            int i;
            bool bRemoved = false;

            for (i = 0; i < this.Count; ++i)
            {
                if (this[i].Label == strLabel)
                {
                    this.RemoveAt(i);
                    bRemoved = true;
                    break;
                }
            }
            if (bRemoved)
                _isDirty = Count > 1;

            return bRemoved;
        }
        // Sort the list and reset the dirty flag
        public new void Sort()
        {
            if (_isDirty && Count > 1)
            {
                base.Sort();
            }
            _isDirty = false;
        }

        // Output a string of all the labels delimited by ","
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(1000);
            if (Count > 0)
            {
                Sort();
                sb.Append(this[0].ToString());
                for (int i = 1; i < Count; ++i)
                {
                    sb.Append(", ");
                    sb.Append(this[i].ToString());
                }
            }
            return sb.ToString();
        }
        #endregion
        #region methods
        public int FindByQtyLabel(string strLabel)
        {
            int intIndex = -1;
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].Qty.SimpleLabel == strLabel)
                {
                    intIndex = i;
                    break;
                }
            }
            return intIndex;
        }
        public int FindByQtyIndex(int intQtyIndex)
        {
            int intIndex = -1;
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].Qty.Index == intQtyIndex)
                {
                    intIndex = i;
                    break;
                }
            }
            return intIndex;
        }
        public bool WriteExpressions(string strPath, string strLogPath)
        {
            bool bSuccess = true;
            StreamWriter swExpressions = null;
            string strLine;
            try
            {
                swExpressions = new StreamWriter(strPath);
                for (int i = 0; i < this.Count; ++i)
                {
                    CQuantity thisQuantity = this[i].Qty;
                    strLine = thisQuantity.SimpleLabel + "\t= ";
                    CExpressionList thisExpressionList = thisQuantity.ExpressionList;
                    thisExpressionList.SuppressDupes();
                    if (thisExpressionList.Count > 0)
                        strLine += thisExpressionList[0].ToString();
                    swExpressions.WriteLine(strLine);
                    for (int j = 1; j < thisExpressionList.Count; ++j)
                    {
                        strLine = "\t= " + thisExpressionList[j].ToString();
                        swExpressions.WriteLine(strLine);
                    }
                }
            }
            catch (Exception e)
            {
                bSuccess = false;
                string strLogEntry = "\r\n Error writing expression file:" + e.Message;
                File.AppendAllText(strLogPath, strLogEntry);
            }
            finally
            {
                if (swExpressions != null)
                    swExpressions.Close();
            }

            return bSuccess;
        }
        public bool WriteValues(string strPath, string strLogPath)
        {
            bool bSuccess = true;
            StreamWriter swValues = null;
            string strLine;
            try
            {
                swValues = new StreamWriter(strPath);
                for (int i = 0; i < this.Count; ++i)
                {
                    CQuantity thisQuantity = this[i].Qty;
                    strLine = thisQuantity.ToString() + "\t" + thisQuantity.Log.ToString();
                    swValues.WriteLine(strLine);
                }
            }
            catch (Exception e)
            {
                bSuccess = false;
                string strLogEntry = "\r\n Error writing value file:" + e.Message;
                File.AppendAllText(strLogPath, strLogEntry);
            }
            finally
            {
                if (swValues != null)
                    swValues.Close();
            }

            return bSuccess;
        }
        #endregion
    }
}
