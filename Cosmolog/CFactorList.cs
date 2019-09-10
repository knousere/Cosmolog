using System.Collections.Generic;
using System.Text;

namespace Cosmolog
{
    // This is meant to act as a sorted index into CQuantityList ordered by label.
    // The primary difference between this class and CQtypointerList is that
    // this class sorts in label order to facilitate normalizing Expressions.
    // This class can serve as a numerator or denominator of an expression.
    class CFactorList : List<CFactor>
    {
        #region constructors
        public CFactorList() : base()
        {
            _isDirty = false;
        }

        // This constructor makes a deep copy
        public CFactorList(CFactorList thatFactorList) : base()
        {
            int i;
            for (i = 0; i < thatFactorList.Count; ++i)
            {
                CFactor thisFactor = new CFactor(thatFactorList[i]);
                Add(thisFactor);
            }
            _isDirty = thatFactorList.IsDirty;
        }

        // This constructor makes an index that points into a Quantity List
        public CFactorList(ref CQuantityList thatQuantityList) : base()
        {
            int i;
            for (i = 0; i < thatQuantityList.Count; ++i)
            {
                CFactor thisFactor = new CFactor(thatQuantityList[i]);
                Add(thisFactor);
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
        #region methods
        public bool ContainsX()
        {
            bool bFound = false;
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].ContainsX())
                {
                    bFound = true;
                    break;
                }
            }
            return bFound;
        }
        // true if contains pi QED
        public bool ContainsQED()
        {
            bool bFound = false;
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].Qty.IsQED)
                {
                    bFound = true;
                    break;
                }
            }
            return bFound;
        }
        // true if contains suppressed element Pi4q
        public bool ContainsSuppressed()
        {
            bool bFound = false;
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].Qty.IsSuppressed)
                {
                    bFound = true;
                    break;
                }
            }
            return bFound;
        }
        // true if contains a computational factor
        public bool ContainsComputational()
        {
            bool bFound = false;
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].Qty.IsComputational)
                {
                    bFound = true;
                    break;
                }
            }
            return bFound;
        }
        // true if contains symbol that indicates sense of one or many
        public bool ContainsSense()
        {
            bool bFound = false;
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].ContainsSense())
                {
                    bFound = true;
                    break;
                }
            }
            return bFound;
        }
        public bool ContainsSymbol(CSymbol thatSymbol)
        {
            bool bFound = false;
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].IsSymbolMatch(thatSymbol))
                {
                    bFound = true;
                    break;
                }
            }
            return bFound;
        }
        #endregion
        #region wrappers
        public new void Add(CFactor thisFactor)
        {
            base.Add(thisFactor);
            _isDirty = true;
        }
        // If match remove one occurance and return true, else return false.
        // Dirty flag is not changed because sort order is not affected.
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

            return bRemoved;
        }
        // return index if found else -1
        public int Find(string strLabel)
        {
            int i;

            for (i = this.Count - 1; i >= 0; --i)
            {
                if (this[i].Label == strLabel)
                {
                    break;
                }
            }
            return i;
        }
        // return index of first integer found else -1
        public int FindInteger()
        {
            int i;

            for (i = this.Count - 1; i >= 0; --i)
            {
                if (this[i].IsInteger)
                {
                    break;
                }
            }
            return i;
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

        // Output a string of all the labels delimited by "*"
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(1000);
            if (Count > 0)
            {
                // Sort by label. Constants sort low.
                Sort();
                sb.Append(this[0].ToString());
                for (int i = 1; i < Count; ++i)
                {
                    sb.Append("*");
                    sb.Append(this[i].ToString());
                }
            }
            else
            {
                sb.Append("1");
            }
            return sb.ToString();
        }
        #endregion
        #region methods
        public void Append(CFactorList thatFactorList)
        {
            for (int i = 0; i < thatFactorList.Count; ++i)
            {
                Add(thatFactorList[i]);
            }
        }

        // Sort quantities and then combine dupes by adding powers.
        public void Normalize()
        {
            Sort();
            int i;
            // combine like factors
            for (i = this.Count - 1; i > 0; --i)
            {
                if (this[i].Label == this[i - 1].Label)
                {
                    // n^x * n^y = n^xy
                    this[i - 1].MultiplyLike(this[i]);
                    this.RemoveAt(i);
                }
            }
            // Simplify integer to a power
            for (i = this.Count - 1; i > -1; --i)
            {
                CFactor thisFactor = this[i];
                if (thisFactor.Qty.IsInteger && thisFactor.Power != 1)
                {
                    thisFactor.ApplyPowerToInteger();
                }
            }
            // combine integer factors
            for (i = this.Count - 1; i > 0; --i)
            {
                CFactor thisFactor = this[i];
                CFactor nextFactor = this[i-1];
                if (thisFactor.IsInteger && nextFactor.IsInteger)
                {
                    // !!! ToDo: deal with root != 1
                    CNumber numberProduct = nextFactor.MultiplyIntegers(thisFactor);
                    CQuantityList theQuantityList = thisFactor.Qty.QuantityList;
                    // Because the calculated quantity value has changed, the factor qty must be replaced.
                    nextFactor.Qty = thisFactor.Qty.QuantityList.ReplaceIntegerQuantity(numberProduct);
                    this.RemoveAt(i);
                }
            }
            // combine inverses
            for (i = this.Count - 1; i > 0; --i)
            {
                CFactor thisFactor = this[i];
                if (thisFactor.Power != 0)
                {
                    CQuantity thisQuantity = thisFactor.Qty;
                    if (thisQuantity.InverseQty != null)
                    {
                        CQuantity inverseQty = thisQuantity.InverseQty;
                        int intInverse = this.Find(inverseQty.Label);
                        if (intInverse > -1)
                        {
                            // n^x * 1/n^y = n^x/n^y = n^(x-y)
                            CFactor inverseFactor = this[intInverse];
                            thisFactor.Qty = inverseFactor.Qty;
                            inverseFactor.DivideLike(thisFactor);
                            this.RemoveAt(i);
                        }
                    }
                }
            }
            // drop factors that equal 1
            for (i = this.Count - 1; i > -1; --i)
            {
                CFactor thisFactor = this[i];
                // x^0 = 1 and log(1) = 0
                // n*1 = n
                if (thisFactor.Power == 0 || thisFactor.Log == 0)
                    this.RemoveAt(i);
            }
        }
        public int CountIntegersToPower()
        {
            int intCount = 0;
            // Count any integers to power
            for (int i = this.Count - 1; i > -1; --i)
            {
                CFactor thisFactor = this[i];
                if (thisFactor.IsInteger && thisFactor.Power != 1 && thisFactor.Power != 0)
                {
                    ++intCount;
                }
            }
            return intCount;
        }
        // Output a string of all the simple labels delimited by "*"
        public string ToSimpleString()
        {
            StringBuilder sb = new StringBuilder(1000);
            if (Count > 0)
            {
                // Sort by label. Constants sort low.
                Sort();
                sb.Append(this[0].ToSimpleString());
                for (int i = 1; i < Count; ++i)
                {
                    sb.Append("*");
                    sb.Append(this[i].ToSimpleString());
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}
