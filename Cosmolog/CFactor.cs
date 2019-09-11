using System;

// One term in a factor list of the form:
// q^(p/r)
// 'q' represents a quantity that may be a constant. In addition, the entire factor can be raised to power/root.
// In most cases power and root both equal 1 and are therefore not displayed.
namespace Cosmolog
{
    class CFactor : IComparable<CFactor>
    {
        #region constructors

        public CFactor(CQuantity thatQuantity)
        {
            Init();
            this._qty = thatQuantity;
        }

        // this constructor makes a shallow copy that points to the same quantity
        public CFactor(CFactor thatFactor)
        {
            Init();
            this._qty = thatFactor.Qty;
            this._ratioPower = new CRatio(thatFactor.RatioPower); // ratio must be a deep copy to avoid side effects
        }

        private void Init()
        {
            _ratioPower = new CRatio(1);
        }
        #endregion
        #region members
        protected CQuantity _qty;        // attached quantity
        protected CRatio _ratioPower;    // power of this term
        #endregion
        #region properties
        // This is called when an expression is evaluated.
        public double Log
        {
            get { return _qty == null ? 0 : _qty.Log * _ratioPower.Numerator / _ratioPower.Denominator; }
        }
        public string Label
        {
            get { return _qty == null ? "qty.null" : _qty.Label; }
            set { if (_qty != null) _qty.Label = value; }
        }

        public CQuantity Qty
        {
            get { return _qty; }
            set { _qty = value; }
        }
        public CRatio RatioPower
        {
            get { return _ratioPower; }
            set { _ratioPower = value; }
        }
        public int Power
        {
            get { return _ratioPower.Numerator; }
        }
        public int Root
        {
            get { return _ratioPower.Denominator; }
        }
        public bool IsConstant
        {
            get { return _qty.IsExact; }
        }
        public bool IsInteger
        {
            get { return _qty.IsInteger; }
        }
        public bool IsAssigned
        {
            get { return _qty != null; }
        }
        public bool IsSimplePower
        {
            get { return _ratioPower.Numerator == 1 && _ratioPower.Denominator == 1; }
        }
        public bool IsSquareRoot
        {
            get { return _ratioPower.Numerator == 1 && _ratioPower.Denominator == 2; }
        }

        #endregion
        #region methods
        public bool ContainsX()
        {
            return _qty.ContainsX();
        }
        // contains symbol that indicates sense of one or many
        public bool ContainsSense()
        {
            return _qty.ContainsSense();
        }
        public bool IsSymbolMatch(CSymbol thatSymbol)
        {
            return _qty.IsSymbolMatch(thatSymbol);
        }
        public string PowerToString()
        {
            return (_ratioPower.Numerator == 1 && _ratioPower.Denominator == 1)? "":
                _ratioPower.Denominator == 1? _ratioPower.Numerator.ToString():
                _ratioPower.Numerator.ToString() + "/" + _ratioPower.Denominator.ToString();
        }
        public string ToSimpleString()
        {
            return (_qty == null ? "qty.null" :
                _ratioPower.Denominator != 1? _qty.SimpleLabel + "^(" + _ratioPower.Numerator.ToString() + "/" + _ratioPower.Denominator.ToString() + ")":
                _ratioPower.Numerator != 1? _qty.SimpleLabel + "^" + _ratioPower.Numerator.ToString():
                _qty.SimpleLabel);
        }
        // reverse the sign of the power
        public void FlipPower()
        {
            _ratioPower.FlipSign();
        }
        // Raise this factor to p/r
        public void ApplyPower(CRatio thatPower)
        {
            _ratioPower.ApplyMultiplication(thatPower);
        }
        // Simplify integer to an integer power.
        // Resultant power is either 1 or 1/n.
        public void ApplyPowerToInteger()
        {
            if (Qty.IsInteger)
            {
                int intValue = (int)Qty.Number.Value;
                CRatio thisRatio = new CRatio(intValue);
                bool bSuccess = thisRatio.ApplyPower(ref _ratioPower);
                CNumber numberToPower = new CNumber(thisRatio);
                CQuantityList theQuantityList = this.Qty.QuantityList;
                // Because the calculated quantity value has changed, the factor qty must be replaced.
                this.Qty = theQuantityList.ReplaceIntegerQuantity(numberToPower);
            }
        }
        public void ResetPower()
        {
            _ratioPower.SetNumerator(1);
        }
        // Multiply integers.
        // Called by CFactoryList.Normalize().
        public CNumber MultiplyIntegers(CFactor thatFactor)
        {
            CNumber numberProduct = null;
            if (this.Qty.IsInteger && thatFactor.Qty.IsInteger)
            {
                double dblValue = this.Qty.Value * thatFactor.Qty.Value;
                numberProduct = new CNumber(dblValue, true);
            }
            return numberProduct;
        }
        // Multiply like quantities by adding powers.
        // This is usually applied to adjacent factors in FactorList when normalizing.
        public bool MultiplyLike(CFactor thatFactor)
        {
            bool bSuccess = false;
            if (this.Label == thatFactor.Label)
            {
                if (this.Root == 1 && thatFactor.Root == 1)
                    _ratioPower.NumeratorAdd(thatFactor.Power);
                else
                {
                    if (this.Root == thatFactor.Root)
                        _ratioPower.NumeratorAdd(thatFactor.Power);
                    else
                    {
                        // Cross multiply
                        int intRootM = _ratioPower.Denominator * thatFactor.Root;
                        int intPowerM = _ratioPower.Numerator * thatFactor.Root + thatFactor.Power * _ratioPower.Denominator;
                        _ratioPower = new CRatio(intRootM, intPowerM);
                    }
                    _ratioPower.Normalize();
                }
                bSuccess = true;
            }
            return bSuccess;
        }

        // Divide like quantities by subtracting powers.
        // This is usually applied to matching numerator and denominator factors in an Expression when normalizing.
        public bool DivideLike(CFactor thatFactor)
        {
            bool bSuccess = false;
            if (this.Label == thatFactor.Label)
            {
                if (this.Root == 1 && thatFactor.Root == 1)
                    _ratioPower.NumeratorSubtract(thatFactor.Power);
                else
                {
                    if (this.Root == thatFactor.Root)
                        _ratioPower.NumeratorSubtract(thatFactor.Power);
                    else
                    {
                        // Cross multiply
                        int intRootM = _ratioPower.Denominator * thatFactor.Root;
                        int intPowerM = _ratioPower.Numerator * thatFactor.Root - thatFactor.Power * _ratioPower.Denominator;
                        _ratioPower = new CRatio(intRootM, intPowerM);
                    }
                    _ratioPower.Normalize();
                }
                bSuccess = true;
            }
            return bSuccess;
        }

        #endregion
        #region wrappers
        public int CompareTo(CFactor otherFactor)
        {
            // Sort by label. Integers sort low.
            if (this.IsInteger && !otherFactor.IsInteger)
                return -1;
            else if (!this.IsInteger && otherFactor.IsInteger)
                return 1;
            else
                // Sort case sensitive.
                return (String.Compare(this.Label, otherFactor.Label, false));
        }

        public override string ToString()
        {
            return ToSimpleString();
        }
        #endregion
    }
}
