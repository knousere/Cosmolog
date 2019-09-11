using System;
// (c)Richard E. Knouse 2015-1019

namespace Cosmolog
{
    // Mathematical expression made up of factors and divisors, all raised to a power.
    // Every such expression can be normalized to the form:
    // (( n1 * n2 * n3 ...)/( d1 * d2 * d3 ...))^(p/r)
    // where n1, ... are factors in the numerator
    // and d1, ... are factors in the denominator
    // Both numerator and denominator are automatically sorted.
    // In addition, the entire expression can be raised to power/root.
    // By default, power and root are both 1 and therefore not displayed.

    class CExpression : IComparable<CExpression>
    {
        #region constructors
        public CExpression()
        {
            Init();
        }

        // deep copy
        public CExpression(CExpression thatExpression)
        {
            Init();
            _numerator = new CFactorList(thatExpression.Numerator);
            _denominator = new CFactorList(thatExpression.Denominator);
            _power = new CRatio(thatExpression.RatioPower);
            _log = thatExpression.Log;
            _symbolCount = thatExpression.SymbolCount;
        }

        // initialize with a mathematical expression
        public CExpression(string strExpression)
        {
            _isValid = Parse(strExpression);
            // check bValid property after construction
            Normalize();
        }

        public CExpression(CMatrixCell thatMatrixCell, bool bNormalize)
        {
            Init();

            _numerator.Add(new CFactor(thatMatrixCell.PtrX));
            if (thatMatrixCell.IsProduct)
                _numerator.Add(new CFactor(thatMatrixCell.PtrY));
            else
                _denominator.Add(new CFactor(thatMatrixCell.PtrY));
            if (bNormalize)
                Normalize();
        }

        private void Init()
        {
            _power = new CRatio(1);
            _numerator = new CFactorList();
            _denominator = new CFactorList();
            _log = 0;
            _isLogValid = false;
            _isValid = true;
            _symbolCount = 0;
        }
        #endregion
        #region members
        protected CRatio _power;
        protected CFactorList _numerator;
        protected CFactorList _denominator;
        protected bool _isLogValid;  // true if log has been calculated
        protected bool _isValid;     // true if expression evaluates successfully
        protected double _log;   // natural log of the evaluated expression
        protected int _symbolCount;  // how many unique symbols are included
        #endregion
        #region properties
        public CFactorList Numerator
        {
            get { return _numerator; }
            set { _numerator = new CFactorList(value); }
        }

        public CFactorList Denominator
        {
            get { return _denominator; }
            set { _denominator = new CFactorList(value); }
        }
        public CRatio RatioPower
        {
            get { return _power; }
        }
        public int Power
        {
            get { return _power.Numerator; }
        }
        public int Root
        {
            get { return _power.Denominator; }
        }
        public bool IsValid
        {
            get { return _isValid; }
        }
        public int PowerCount
        {
            get { return _numerator.CountIntegersToPower() + _denominator.CountIntegersToPower(); }
        }
        public int SymbolCount
        {
            get { return _symbolCount; }
        }
        // evaluate as value
        public double Value
        {
            get { return Math.Exp(Log); }
        }
        // Evaluate as log of value
        // Recalculate if necessary.
        public double Log
        {
            get    
            {
                int i;

                //if (!_isLogValid)
                //{
                    _log = 0;
                    for (i = 0; i < _numerator.Count; ++i)
                    {
                        _log += _numerator[i].Log;
                    }
                    for (i = 0; i < _denominator.Count; ++i)
                    {
                        _log -= _denominator[i].Log;
                    }
                //}
                _isLogValid = true;
                return _log;
            }
        }
        #endregion
        #region methods
        // return true if any term of the expression begins with 'X' or '~X'
        public bool ContainsX()
        {
            return _numerator.ContainsX() || _denominator.ContainsX();
        }
        // true if contains a computational factor in either numerator or denominator
        public bool ContainsComputational()
        {
            return _numerator.ContainsComputational() || _denominator.ContainsComputational();
        }
        // true if contains a Suppressed factor in either numerator or denominator
        public bool ContainsSuppressed()
        {
            return _numerator.ContainsSuppressed() || _denominator.ContainsSuppressed();
        }
        // return true if any term of the expression begins with '~'
        public bool ContainsSense()
        {
            return _numerator.ContainsSense() || _denominator.ContainsSense();
        }
        // return true if any term of the expression contains a quantity with that symbol
        public bool ContainsSymbol(CSymbol thatSymbol)
        {
            return _numerator.ContainsSymbol(thatSymbol) || _denominator.ContainsSymbol(thatSymbol);
        }
        // Parse the expression and load the numerator and denominator.
        // Return true if successful.
        public bool Parse(string strExpression)
        {
            // !!!ToDo: finish this
            return true;
        }
        // Substitute an expression for a factor in the numerator.
        public void SubstituteIntoNumerator(int intIndex, CExpression thatExpression)
        {
            string strNumerator = _numerator.ToString();
            CFactor oldFactor = _numerator[intIndex];
            CExpression insertExpression = new CExpression(thatExpression);
            insertExpression.DistributePower();
            insertExpression.RatioPower.Set(oldFactor.RatioPower);
            insertExpression.DistributePower();
            _numerator.RemoveAt(intIndex);
            _numerator.Append(insertExpression.Numerator);
            _denominator.Append(insertExpression.Denominator);
            _isLogValid = false;
            Normalize();
        }
        // Substitute an expression for a factor in the denominator.
        public void SubstituteIntoDenominator(int intIndex, CExpression thatExpression)
        {
            CFactor oldFactor = _denominator[intIndex];
            CExpression insertExpression = new CExpression(thatExpression);
            insertExpression.DistributePower();
            insertExpression.RatioPower.Set(oldFactor.RatioPower);
            insertExpression.DistributePower();
            _denominator.RemoveAt(intIndex);
            _denominator.Append(insertExpression.Numerator);
            _numerator.Append(insertExpression.Denominator);
            _isLogValid = false;
            Normalize();
        }
        // Apply the power of this expression to each term
        // and set the power of the overall expression to 1/1.
        // This guarantees that equivalent expressions will compare equal.
        public void DistributePower()
        {
            if (_power.Numerator != 1 || _power.Denominator != 1)
            {
                for (int i = 0; i < _numerator.Count; ++i)
                {
                    //if (i < _numerator.Count)
                        _numerator[i].ApplyPower(_power);
                }
                for (int j = 0; j < _denominator.Count; ++j)
                {
                    //if (j < _denominator.Count)
                        _denominator[j].ApplyPower(_power);
                }
            }
            _power.Set(1);
        }
        public void Normalize()
        {
            int intNumIndex, intDenomIndex = 0;
            _numerator.Normalize();
            _denominator.Normalize();
            if (_numerator.Count > 0 && _denominator.Count > 0)
            {
                for (intNumIndex = _numerator.Count - 1; intNumIndex > -1; --intNumIndex)
                {
                    // if dupe is found in denominator then adjust both numerator and denominator
                    CFactor numeratorFactor = _numerator[intNumIndex];
                    intDenomIndex = _denominator.Find(numeratorFactor.Label);
                    if (intDenomIndex > -1) 
                    {
                        CFactor denominatorFactor = _denominator[intDenomIndex];
                        numeratorFactor.DivideLike(denominatorFactor);
                        _denominator.RemoveAt(intDenomIndex);
                        _isLogValid = false;
                    }
                    // if inverse is found in denominator then adjust both numerator and denominator
                    intDenomIndex = _denominator.Find(numeratorFactor.Qty.InverseLabel);
                    if (intDenomIndex > -1)
                    {
                        CFactor denominatorFactor = _denominator[intDenomIndex];
                        CQuantity inverseQty = denominatorFactor.Qty.InverseQty;
                        denominatorFactor.Qty = inverseQty;
                        numeratorFactor.MultiplyLike(denominatorFactor);
                        _denominator.RemoveAt(intDenomIndex);
                        _isLogValid = false;
                    }
                }
            }
            // if numerator factor has negative power swap it to denominator
            for (intNumIndex = _numerator.Count - 1; intNumIndex > -1; --intNumIndex)
            {
                CFactor numeratorFactor = _numerator[intNumIndex];
                if (numeratorFactor.Power == 0)
                    _numerator.RemoveAt(intNumIndex);
                else if (numeratorFactor.Power < 0)
                {
                    _numerator.RemoveAt(intNumIndex);
                    numeratorFactor.FlipPower();
                    _denominator.Add(numeratorFactor);
                    _isLogValid = false;
                }
            }
            // if denominator factor has negative power swap it to numerator
            for (intDenomIndex = _denominator.Count - 1; intDenomIndex > -1; --intDenomIndex)
            {
                CFactor denominatorFactor = _denominator[intDenomIndex];
                if (denominatorFactor.Power == 0)
                    _denominator.RemoveAt(intDenomIndex);
                else if (denominatorFactor.Power < 0)
                {
                    _denominator.RemoveAt(intDenomIndex);
                    denominatorFactor.FlipPower();
                    _numerator.Add(denominatorFactor);
                    _isLogValid = false;
                }
            }
            // net out integers between numerator and denominator
            int intNumeratorIndex = _numerator.FindInteger();
            int intDenominatorIndex = _denominator.FindInteger();
            if (intNumeratorIndex > -1 && intDenominatorIndex > -1)
            {
                CFactor numeratorFactor = _numerator[intNumeratorIndex];
                CFactor denominatorFactor = _denominator[intDenominatorIndex];
                if (numeratorFactor.RatioPower.Equals(1) && denominatorFactor.RatioPower.Equals(1))
                {
                    int intNumeratorValue = (int)numeratorFactor.Qty.Value;
                    int intDenominatorValue = (int)denominatorFactor.Qty.Value;

                    // Putting both integers into a CRatio has the effect of reducing them
                    CRatio ratioThis = new CRatio(intNumeratorValue, intDenominatorValue);
                    double dblNumerator = (double)ratioThis.Numerator;
                    double dblDenominator = (double)ratioThis.Denominator;
                    CNumber numberNumerator = new CNumber(dblNumerator);
                    CNumber numberDenominator = new CNumber(dblDenominator);
                    CQuantityList theQuantityList = numeratorFactor.Qty.QuantityList;

                    numeratorFactor.Qty = theQuantityList.ReplaceIntegerQuantity(numberNumerator);
                    denominatorFactor.Qty = theQuantityList.ReplaceIntegerQuantity(numberDenominator);
                }
            }
            _numerator.Normalize();
            _denominator.Normalize();
            SetSymbolCount(); 
        }

        //return the power as a string which may be empty
        public string PowerToString()
        {
            return (_power.Numerator == 1 && _power.Denominator == 1) ? "" :
                _power.Denominator == 1 ? _power.Numerator.ToString() :
                _power.Numerator.ToString() + "/" + _power.Denominator.ToString();
        }

        // return the evaluated value as a formatted string
        public string ValueToString()
        {
            CNumber thisNumber = new CNumber(Value, true);
            return thisNumber.ToString();
        }

        // return the evaluated log as a formatted string
        public string LogToString()
        {
            CNumber thisNumber = new CNumber(Log, false);
            return thisNumber.LogToString();
        }

        public void SetPower(int intPower)
        {
            _power.Set(intPower);
        }
        public void SetSymbolCount()
        {
            CStringList thisStringList = new CStringList();
            int i;
            CQuantity thisQuantity;
            for (i = 0; i < _numerator.Count; ++i)
            {
                thisQuantity = _numerator[i].Qty;
                if (!thisQuantity.IsInteger)
                    thisStringList.AddUnique(thisQuantity.Label);
            }
            for (i = 0; i < _denominator.Count; ++i)
            {
                thisQuantity = _denominator[i].Qty;
                if (!thisQuantity.IsInteger)
                    thisStringList.AddUnique(thisQuantity.Label);
            }
            _symbolCount = thisStringList.Count;
        }
        #endregion
        #region wrappers
        // return mathematical expression in the crudest form
        public override string ToString()
        {
            string strReturn;
            string strNumerator = _numerator.ToSimpleString();
            string strDenominator = _denominator.ToSimpleString();

            if (strNumerator == "")
                strNumerator = "1";

            if (strDenominator == "" || strDenominator == "1")
                strReturn = strNumerator;
            else
            {
                if (_denominator.Count > 1)
                    strDenominator = "(" + strDenominator + ")";
                strReturn = strNumerator + "/" + strDenominator;
                if (_power.Numerator != 1 || _power.Denominator != 1)
                    strReturn = "(" + strReturn + ")";
            }
            if (_power.Denominator > 1)
            {
                if (_numerator.Count > 1 || _denominator.Count > 0)
                    strReturn = "(" + strReturn + ")";
                strReturn = strReturn + "^(" + _power.Numerator.ToString() + "/" + _power.Denominator.ToString() + ")";
            }
            else if (_power.Numerator != 1)
            {
                if (_numerator.Count > 1 || _denominator.Count > 0)
                    strReturn = "(" + strReturn + ")";
                strReturn = strReturn + "^" + _power.Numerator.ToString();
            }

            return strReturn;
        }
        // Implicitly called by Sort()
        public int CompareTo(CExpression otherExpression)
        {
            // Compare case sensitive
            return String.Compare(this.ToString(), otherExpression.ToString(), false);
        }

        #endregion
    }
}
