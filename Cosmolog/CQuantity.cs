using System;

// this is an element of the list of quantities
namespace Cosmolog
{
    class CQuantity : IComparable<CQuantity>
    {
        #region constructors
        // Deep copy. Used in CQuantity::CalculateSubstitution and in deep copy constructors.
        public CQuantity(CQuantity thatQuantity)
        {
            Init(thatQuantity.QuantityList);
            _symbol = new CSymbol(thatQuantity._symbol);
            _number = new CNumber(thatQuantity._number);
            _uncertainty = new CNumber(thatQuantity._uncertainty);
            _lowfence = new CNumber(thatQuantity._lowfence);
            _highfence = new CNumber(thatQuantity._highfence);
            _intIndex = thatQuantity._intIndex;
            _inverseQty = thatQuantity.InverseQty;
            _expressionList = new CExpressionList(thatQuantity._expressionList);
            _isExact = thatQuantity._isExact;
            _isComputational = thatQuantity._isComputational;
            _isQED = thatQuantity._isQED;
            _isSuppressed = thatQuantity._isSuppressed;
            _isInteger = thatQuantity.IsInteger;
        }
        // this constructor initializes from a line of QuantityValues.txt
        public CQuantity(CQuantityList thatQuantityList, string strLine)
        {
            Init(thatQuantityList);
            string strThatLabel = ParseLine(ref strLine);
            string strToken = ParseLine(ref strLine);
            string strExactFlag = ParseLine(ref strLine);
            _symbol = new CSymbol(strThatLabel);
            if (strToken == "")
            {
                _number = new CNumber();
            }
            else
            {
                _number = new CNumber(strToken, true);
                _uncertainty = new CNumber(_number.Uncertainty);
                _lowfence = new CNumber(_number.Value - _uncertainty.Value, true);
                _highfence = new CNumber(_number.Value + _uncertainty.Value, true);
            }
            _isExact = strExactFlag.ToUpper() == "X";
            _isComputational = strExactFlag.ToUpper() == "C";
            _isQED = strExactFlag.ToUpper() == "Q";
            _isSuppressed = strExactFlag.ToUpper() == "S";
            _isInteger = _number.IsInteger;
        }
        // construct from either value or log
        public CQuantity(CQuantityList thatQuantityList, string strThatLabel, double dblToken, bool bIsValue)
        {
            Init(thatQuantityList);
            _symbol = new CSymbol(strThatLabel);
            _number = new CNumber(dblToken, bIsValue);
            _isInteger = _number.IsInteger;
        }
        // construct from either value or log, constant or not
        public CQuantity(CQuantityList thatQuantityList, string strThatLabel, double dblToken, bool bIsValue, bool bIsConstant)
        {
            Init(thatQuantityList);
            _isExact = bIsConstant;
            _symbol = new CSymbol(strThatLabel);
            _number = new CNumber(dblToken, bIsValue);
            _isInteger = _number.IsInteger;
        }
        // construct integer quantity from label and value
        public CQuantity(CQuantityList thatQuantityList, bool bIsInteger, string strThatLabel, double dblToken)
        {
            Init(thatQuantityList);
            _isExact = true;
            _isInteger = true;
            _symbol = new CSymbol(strThatLabel);
            _number = new CNumber(dblToken, true);
        }
        // construct integer quantity from label, value and description
        public CQuantity(CQuantityList thatQuantityList, bool bIsInteger, string strThatLabel, double dblToken, string strDescription)
        {
            Init(thatQuantityList);
            _isExact = true;
            _isInteger = true;
            _symbol = new CSymbol(strThatLabel);
            _number = new CNumber(dblToken, true);
            _symbol.Description = strDescription;
        }

        private void Init(CQuantityList thatQuantityList)
        {
            _intIndex = -1;
            _isExact = false;
            _isComputational = false;
            _isQED = false;
            _isSuppressed = false;
            _isInteger = false;
            _uncertainty = new CNumber(0);
            _lowfence = new CNumber();
            _highfence = new CNumber();
            _candidate = new CNumber();
            _expressionList = new CExpressionList();
            _candidateExpressionList = new CExpressionList();
            _inverseQty = null;
            _quantityList = thatQuantityList;
        }

        #endregion
        #region members
        CSymbol _symbol;
        CNumber _number;        // current value and log of quantity
        CNumber _candidate;     // candidate value and log of quantity
        CNumber _uncertainty;   // +- if known
        CNumber _lowfence;      // original number - uncertainty
        CNumber _highfence;     // original number + uncertainty
        CQuantityList _quantityList;    // Expression normalization requires pointer to the quantity list.
        int _intIndex;          // where I am in the list
        bool _isExact;         // exact value. Do not recompute
        bool _isQED;            // quantization constant
        bool _isComputational; // used for computation but not for substitution or printout
        bool _isSuppressed;    // suppressed in printout
        bool _isInteger;
        CExpressionList _expressionList;    // list of formula expressions that are equivalent to this quantity      
        CExpressionList _candidateExpressionList;
        CQuantity _inverseQty;
        #endregion
        #region properties
        public CQuantityList QuantityList
        {
            get { return _quantityList; }
        }
        public CExpressionList ExpressionList
        {
            get { return _expressionList; }
        }
        public CExpressionList CandidateExpressionList
        {
            get { return _candidateExpressionList; }
        }
        public string Label
        {
            get { return _symbol.Label; }
            set { _symbol = new CSymbol(value); }
        }
        public string SimpleLabel
        {
            get { return _symbol.SimpleLabel; }
        }
        public string InverseLabel
        {
            get { return _inverseQty == null ? "" : _inverseQty.Label; }
        }
        public CSymbol Symbol
        {
            get { return _symbol; }
            set { _symbol = new CSymbol(value); }

        }
        public string SymbolMain
        {
            get { return _symbol.Main; }
        }
        public string SymbolSub
        {
            get { return _symbol.Subscript; }
        }
        public string StrNumber
        {
            get { return _number.ToString(); }
        }
        public int SymbolMainLen
        {
            get { return _symbol.Main.Length; }
        }
        public int SymbolSubscriptLen
        {
            get { return _symbol.Subscript.Length; }
        }
        public string Description
        {
            get { return _symbol.Description; }
            set { _symbol.Description = value; }
        }
        public string Unit
        {
            get { return _symbol.Unit; }
            set { _symbol.Unit = value; }
        }
        public int Index
        {
            get { return _intIndex; }
            set
            {
                // index can only be set once
                if (_intIndex == -1)
                    _intIndex = value;
                else
                    throw new ArgumentException("Attempt to modify value intIndex:", _symbol.ToString());
            }
        }

        public double Value
        {
            get { return (_number.Value); }
            set
            {
                if (_isExact)
                    throw new ArgumentException("Attempt to modify value of constant:", _symbol.ToString());
                else
                    _number.Value = value;
            }
        }

        public double Log
        {
            get { return (_number.Log); }
            set
            {
                if (_isExact)
                    throw new ArgumentException("Attempt to modify log of constant:", _symbol.ToString());
                else
                    _number.Log = value;
            }
        }

        public double ValueCandidate
        {
            get { return (_candidate.Value); }
            set
            {
                if (_isExact)
                    throw new ArgumentException("Attempt to modify candidate value of constant:", _symbol.ToString());
                else
                    _candidate.Value = value;
            }
        }

        public double LogCandidate
        {
            get { return (_candidate.Log); }
            set
            {
                if (_isExact)
                    throw new ArgumentException("Attempt to modify candidate log of constant:", _symbol.ToString());
                else
                    _candidate.Log = value;
            }
        }

        public bool IsExact
        {
            get { return (_isExact); }
            set { _isExact = value; }
        }
        public bool IsQED
        {
            get { return (_isQED); }
            set { _isQED = value; }
        }
        public bool IsSuppressed
        {
            get { return (_isSuppressed); }
            set { _isSuppressed = value; }
        }
        public bool IsComputational
        {
            get { return (_isComputational); }
            set { _isComputational = value; }
        }
        public bool IsInteger
        {
            get { return (_isInteger); }
        }

        public CNumber Number
        {
            get { return _number; }
        }
        public CNumber Candidate
        {
            get { return (_candidate); }
            set { _candidate = value; }
        }
        public CQuantity InverseQty
        {
            get { return _inverseQty; }
        }
        #endregion
        #region methods
        public void SetInteger()
        {
            _isInteger = true;
        }
        public void ResetIndex()
        {
            _intIndex = -1;
        }
        public void ResetInverse()
        {
            _inverseQty = null;
        }
        public CNumber MultiplyIntegers(CQuantity thatQuantity)
        {
            CNumber numberProduct = null;
            if (_isInteger && thatQuantity._isInteger)
            {
                double dblValue = this.Value * thatQuantity.Value;
                numberProduct = new CNumber(dblValue, true);
            }
            return numberProduct;
        }
        public void SetInverse(ref CQuantity thatQuantity)
        {
            if (_inverseQty == null)
            {
                // do not allow inverse integers
                if (!_isInteger)
                {
                    _inverseQty = thatQuantity;
                    if (this != thatQuantity)
                        thatQuantity._inverseQty = this;
                }
            }
            else
                throw new ArgumentException("Attempt to modify value _inverseQty:", _symbol.ToString());
        }
        public bool ContainsX()
        {
            return _symbol.ContainsX();
        }
        // true if suffix is + or - indicating sense of the log of the underlying value
        public bool ContainsSense()
        {
            return _symbol.ContainsSense();
        }
        public bool IsSymbolMatch(CSymbol thatSymbol)
        {
            return _symbol.IsMatch(thatSymbol);
        }
        // Look up my symbol in the symbol list. If found then copy the description and unit.
        public void SetDescription(CSymbolList thatSymbolList)
        {
            int s;
            s = thatSymbolList.FindMatch(_symbol.Label);
            if (s > -1)
            {
                CSymbol thatSymbol = thatSymbolList[s];
                _symbol.Description = thatSymbol.Description;
                _symbol.Unit = thatSymbol.Unit;
            }
        }
        // Substitute every term of each expression to form potential new expressions
        public void CalculateSubstitution(int intMaxFactors, bool bFirstPass)
        {
            int intLimit = _expressionList.Count;
            double dblLogThis = this.Log;
            CExpressionList thisExpressionList;
            // Integers are never substituted.
            if (!this._isInteger)
            {
                // On first pass initialize from the established expression list.
                // On subsequent passes build on previous candidate list.
                if (bFirstPass)
                {
                    thisExpressionList = new CExpressionList(_expressionList);
                    _candidateExpressionList = new CExpressionList(_expressionList);
                }
                else
                    thisExpressionList = new CExpressionList(_candidateExpressionList);
                // Limit this to only the expressions that are already in the list.
                intLimit = thisExpressionList.Count;
                // for each expression in this quantity's expression list
                for (int i = 0; i < intLimit; ++i)
                {
                    CExpression baseExpression = thisExpressionList[i];
                    CNumber baseNumber = new CNumber(baseExpression.Log, false);
                    int intNumeratorLimit = baseExpression.Numerator.Count;
                    int intDenominatorLimit = baseExpression.Denominator.Count;
                    // for each factor in the numerator
                    for (int intFactorIndex = 0; intFactorIndex < intNumeratorLimit; ++intFactorIndex)
                    {
                        CFactor baseFactor = baseExpression.Numerator[intFactorIndex];
                        CQuantity baseQuantity = new CQuantity(baseFactor.Qty);
                        // Integers are never substituted.
                        // Computational intermediates are never substituted. They are expanded on printout.
                        if (!baseQuantity._isInteger /* && !baseQuantity._isComputational*/)
                        {
                            // for each expression associated with the numerator factor
                            for (int k = 0; k < baseQuantity.ExpressionList.Count; ++k)
                            {
                                CExpression insertExpression = new CExpression(baseQuantity.ExpressionList[k]);
                                if (!insertExpression.ContainsSymbol(this.Symbol))
                                {
                                    CExpression newExpression = new CExpression(baseExpression);
                                    newExpression.SubstituteIntoNumerator(intFactorIndex, insertExpression);
                                    CNumber newNumber = new CNumber(newExpression.Log, false);
                                    if (!baseNumber.IsEquivalent(newNumber))
                                    {
                                        // !!! error
                                        double dblLogTest = newExpression.Log; // testing only
                                    }
                                    newExpression.Normalize();
                                    if (newExpression.SymbolCount <= intMaxFactors)
                                        _candidateExpressionList.Add(newExpression);
                                }
                            }
                        }
                    }
                    // for each factor in the denominator
                    for (int intFactorIndex = 0; intFactorIndex < intDenominatorLimit; ++intFactorIndex)
                    {
                        CFactor baseFactor = baseExpression.Denominator[intFactorIndex];
                        CQuantity baseQuantity = new CQuantity(baseFactor.Qty);
                        // Integers are never substituted.
                        // Computational intermediates are never substituted. They are expanded on printout.
                        if (!baseQuantity._isInteger/* && !baseQuantity._isComputational*/)
                        {
                            for (int k = 0; k < baseQuantity.ExpressionList.Count; ++k)
                            {
                                // for each expression associated with the denominator factor
                                CExpression insertExpression = new CExpression(baseQuantity.ExpressionList[k]);
                                if (!insertExpression.ContainsSymbol(this.Symbol))
                                {
                                    CExpression newExpression = new CExpression(baseExpression);
                                    newExpression.SubstituteIntoDenominator(intFactorIndex, insertExpression);
                                    CNumber newNumber = new CNumber(newExpression.Log, false);
                                    if (!baseNumber.IsEquivalent(newNumber))
                                    {
                                        // !!! error
                                        double dblLogTest = newExpression.Log; // testing only
                                    }
                                    newExpression.Normalize();
                                    if (newExpression.SymbolCount <= intMaxFactors)
                                        _candidateExpressionList.Add(newExpression);
                                }
                            }
                        }
                    }
                    _candidateExpressionList.SuppressDupes();
                    _candidateExpressionList.SuppressTautology(this);
                }
                GC.Collect();
            }
        }
        // candidate has been calculated so replace the primary with the candidate.
        public void ApplyCandidateExpressionList()
        {
            _expressionList.Clear();        // Give the garbage collector a heads up.
            _expressionList = _candidateExpressionList;
            _candidateExpressionList = new CExpressionList();
            GC.Collect();
        }
        // parse off the first segment of a string delimited by either space or tab
        private string ParseLine(ref string strLine)
        {
            strLine = strLine.Trim();
            string strNibble = "";
            int intPos = strLine.IndexOfAny(new char[]{ ' ', '\t'});

            if (intPos > -1)
            {
                strNibble = strLine.Substring(0, intPos);
                strLine = strLine.Substring(intPos + 1);
            }
            else
            {
                strNibble = strLine;
                strLine = "";
            }
            return strNibble;
        }
        // Return a new CQuantity = 1/value
        // This cannot be applied to integers
        public CQuantity Inverse(string strLabel)
        {
            if (this._isInteger)
            {
                // !!!panic. We do not want to define inverses to integers
                bool bPanic = this._isInteger;
            }
            double dblInverse = -(_number.Log);
            CQuantity qty = new CQuantity(_quantityList, strLabel, dblInverse, false, _isExact);
            qty._isComputational = this._isComputational;
            qty._isQED = this._isQED;
            qty._isSuppressed = this._isSuppressed;
            return qty;
        }
        // symbol.SymbolInverse = ~symbol and vice versa
        public CSymbol SymbolInverse()
        {
            string strSense;
            if (_number.Log >= 0)
                strSense = "-";
            else
                strSense = "+";
            return _symbol.SymbolInverse(strSense);
        }

        public void ApplyUncertainty(double dblUncertainty)
        {
            _uncertainty = new CNumber(dblUncertainty);
            if (_uncertainty.Value > 0)
            {
                _lowfence = new CNumber(_number.Value - _uncertainty.Value, true);
                _highfence = new CNumber(_number.Value + _uncertainty.Value, true);
            }
        }
        // set the number from the candidate
        public void ApplyCandidate()
        {
            if (_candidate.IsFromValue)
                _number.Value = _candidate.Value;
            else
                _number.Log = _candidate.Log;
        }

        // Calculate a new candidate value for this quantity as
        // an average of its expressions. Perform all calculations
        // using the log to limit propagation of truncation errors.
        public void CalcCandidate()
        {
            if (!_isExact && _expressionList.Count > 0)
            {
                _candidate.Log = _expressionList.AverageLog();
                // keep the calculated value within the fences
                if (_uncertainty.Value != 0)
                {
                    if (_candidate.Log < _lowfence.Log)
                        _candidate.Log = _lowfence.Log;
                    else if (_candidate.Log > _highfence.Log)
                        _candidate.Log = _highfence.Log;
                }
            }
            else
            {
                _candidate = new CNumber(_number);
            }
        }

        // This comparison returns true if log is within standard difference
        // of otherquantity.log.
        public bool IsEquivalent(CQuantity otherQuantity)
        {
            return _number.IsEquivalent(otherQuantity.Number);
        }

        // This comparison returns true if log is within standard difference
        // of otherNumber.log.
        public bool IsEquivalent(CNumber otherNumber)
        {
            return _number.IsEquivalent(otherNumber);
        }

        // return a string that can be formatted into RichText
        public string ToRichString()
        {
            string strReturn = _symbol.SimpleLabel + "\t " + _number.ToString() + "\t " + _number.LogToString() + "\r\n";
            return strReturn;
        }

        #endregion
        #region wrappers
        // return a string equivalent to a line in QuantityValues.txt
        public override string ToString()
        {
            string strReturn = (_symbol == null ? "null" : _symbol.ToString()) + "\t "
                + (_number == null ? "null" : _number.ToString());
            if (_isExact)
                strReturn += " C";
            return strReturn;
        }

        // This comparison establishes standard symbol order which is used
        // to resolve and simplify expressions.
        public int CompareTo(CQuantity otherQuantity)
        {
            // Compare case sensitive
            return _symbol.CompareTo(otherQuantity.Symbol);
        }

        #endregion
    }
}
