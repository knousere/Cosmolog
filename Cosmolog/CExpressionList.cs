using System.Collections.Generic;

namespace Cosmolog
{
    class CExpressionList : List<CExpression>
    {
        #region constructors
        public CExpressionList() : base()
        {
        }
        // Construct a deep copy
        public CExpressionList(CExpressionList thatExpressionList) : base()
        {
            for (int i = 0; i < thatExpressionList.Count; ++i)
            {
                CExpression newExpression = new CExpression(thatExpressionList[i]);
                Add(newExpression);
            }
        }
        // Construct shallow copy for purposes of display and printing
        public CExpressionList(CExpressionList thatExpressionList, bool bNotSense)
            : base()
        {
            for (int i = 0; i < thatExpressionList.Count; ++i)
            {
                CExpression thisExpression = thatExpressionList[i];
                if (!bNotSense || !thisExpression.ContainsSense())
                    Add(thisExpression);
            }
        }
        #endregion
        #region methods
        public int CountNotSense()
        {
            int intCount = 0;
            for (int i = 0; i < Count; ++i)
            {
                if (!(this[i].ContainsSense()))
                    ++intCount;
            }
            return intCount;
        }
        public int CountPrintable()
        {
            int intCount = 0;
            for (int i = 0; i < Count; ++i)
            {
                if (!(this[i].ContainsSuppressed()) && !(this[i].ContainsComputational()))
                    ++intCount;
            }
            return intCount;
        }
        public double AverageLog()
        {
            double dblSum = 0;
            double dblAvg = 0;
            if (Count > 0)
            {
                for (int i = 0; i < Count; ++i)
                {
                    dblSum += this[i].Log;
                }
                dblAvg = dblSum / Count;
            }
            return dblAvg;
        }
        public void SuppressDupes()
        {
            for (int i = this.Count - 1; i > 0; --i)
            {
                this[i].Normalize();
            }
            Sort();
            for (int i = this.Count - 1; i > 0; --i)
            {
                if (this[i].CompareTo(this[i - 1]) == 0)
                    this.RemoveAt(i);
            }
        }
        // use to remove x = x or x = xabc where abc = 1
        public void SuppressTautology(CQuantity thatQuantity)
        {
            CQuantity InverseQuantity = thatQuantity.InverseQty;
            for (int i = this.Count - 1; i > 0; --i)
            {
                CExpression thisExpression = this[i];
                if (thisExpression.Numerator.Find(thatQuantity.Label) > -1)
                    this.RemoveAt(i);
                else if (thisExpression.Denominator.Find(thatQuantity.Label) > -1)
                    this.RemoveAt(i);
                else if (InverseQuantity != null && thisExpression.Numerator.Find(InverseQuantity.Label) > -1)
                    this.RemoveAt(i);
                else if (InverseQuantity != null && thisExpression.Denominator.Find(InverseQuantity.Label) > -1
                    && (thisExpression.Numerator.Count > 1 || (thisExpression.Numerator.Count !=0 && thisExpression.Numerator[0].Qty.Value != 1)))
                    this.RemoveAt(i);
                else
                {
                    // We don't want large integers in expressions
                    bool bRemoved = false;
                    int intIndex = thisExpression.Numerator.FindInteger();
                    if (intIndex >= 0)
                    {
                        if (thisExpression.Numerator[intIndex].Qty.Value > 256)
                        {
                            this.RemoveAt(i);
                            bRemoved = true;
                        }
                    }
                    if (!bRemoved)
                    {
                        intIndex = thisExpression.Denominator.FindInteger();
                        if (intIndex >= 0)
                        {
                            if (thisExpression.Denominator[intIndex].Qty.Value > 256)
                            {
                                this.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }
        // remove any expression that contains 'X' or '~X'
        public void CullX()
        {
            int intLimit = this.Count;
            for (int i = intLimit - 1; i > -1; --i)
            {
                CExpression thisExpression = this[i];
                if (thisExpression.ContainsX())
                    this.RemoveAt(i);
            }
        }
        // remove any expression that contains that Symbol
        public void CullSymbol(CSymbol thatSymbol)
        {
            int intLimit = this.Count;
            for (int i = intLimit - 1; i > -1; --i)
            {
                CExpression thisExpression = this[i];
                if (thisExpression.ContainsSymbol(thatSymbol))
                    this.RemoveAt(i);
            }
        }
        #endregion
    }
}
