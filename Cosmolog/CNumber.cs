using System;
using System.Text;


// This is used by CQuantity to store current and candidate values.
// It has properties that can be read or written as number or log(number)
// It is designed to minimize calls to Log() and Exp() math functions.
namespace Cosmolog
{
    class CNumber : IComparable<CNumber>
    {
        #region constructors
        public CNumber()
        {
            Init();
        }

        public CNumber(double dblToken)
        {
            InitFromNumber(dblToken, true);
        }

        // Initialize from either value or log
        public CNumber(double dblToken, bool bIsValue)
        {
            InitFromNumber(dblToken, bIsValue);
        }

        public CNumber(string strToken)
        {
            InitFromString(strToken, true);
        }

        public CNumber(string strToken, bool bIsValue)
        {
            InitFromString(strToken, bIsValue);
        }

        public CNumber(CRatio thatRatio)
        {
            double dblValue = thatRatio.Numerator;
            dblValue /= thatRatio.Denominator;
            InitFromNumber(dblValue, true);
        }
        public CNumber(CNumber thatNumber)
        {
            if (thatNumber.IsFromValue)
                InitFromNumber(thatNumber.Value, true);
            else
                InitFromNumber(thatNumber.Log, false);
        }

        private void Init()
        {
            _value = 0;
            _log = double.NegativeInfinity;
            _uncertainty = 0;
            _isFromValue = true;
            _isDirty = false;
            _isValid = false;
        }

        // Initialize from either value or log
        public void InitFromNumber(double dblToken, bool bIsValue)
        {
            Init();
            if (bIsValue)
            {
                _value = dblToken;
                _log = Math.Log(dblToken);
                _isFromValue = true;
            }
            else
            {
                _log = dblToken;
                _value = Math.Exp(dblToken);
                _isFromValue = false;
            }
            _isValid = true;
            _isDirty = false;
        }
        // Initialize from string representation of value in scientific notation
        // with optional embedded uncertainty.
        // E.G. 1.234(56)e-78 = 1.234e-78 +- 0.056e-78
        private void InitFromString(string strToken, bool bIsValue)
        {
            Init();
            int intLeft = strToken.IndexOf('(');
            if (intLeft > -1)
            {
                // the string has the uncertainty notation
                int intRight = strToken.IndexOf(')');
                int intPoint = strToken.IndexOf('.');
                string strUncertainty = strToken.Substring(intLeft + 1, intRight - intLeft - 1);
                string strNumber = strToken.Remove(intLeft, intRight - intLeft + 1);
                if (bIsValue)
                {
                    _isValid = double.TryParse(strNumber, out _value);
                    if (IsValid)
                    {
                        _log = Math.Log(_value);
                        _isFromValue = true;
                    }
                }
                else
                {
                    _isValid = double.TryParse(strNumber, out _log);
                    if (IsValid)
                    {
                        _value = Math.Exp(_log);
                        _isFromValue = false;
                    }
                }

                // uncertainty only makes sense if initializing from a value, not a log
                if (bIsValue && _isValid)
                {
                    StringBuilder sb = new StringBuilder(strNumber);
                    int i;
                    int j;
                    for (i = 0; i < intLeft - strUncertainty.Length; ++i)
                    {
                        if (sb[i] != '.')
                            sb[i] = '0';
                    }
                    for (j = 0; j < strUncertainty.Length; ++j)
                    {
                        sb[i + j] = strUncertainty[j];
                    }
                    strNumber = sb.ToString();
                    _isValid = double.TryParse(strNumber, out _uncertainty);
                }
            }
            else
            {
                _isValid = double.TryParse(strToken, out _value);
                _uncertainty = 0;
            }
            _isDirty = true;
        }

        #endregion
        #region members
        protected double _value;
        protected double _log;
        protected double _uncertainty;
        protected bool _isFromValue;  // true if set from value, false if set from log
        protected bool _isDirty;     // true if either value or log is not in sync
        protected bool _isValid;
        protected const double _epsilon = 1.0e-5; // difference for defining equivalence
        // Actual calculated data showed a gap between 10^-4 and 10^-7 in the matrix
        #endregion
        #region properties
        public bool IsInteger
        {
            get { return (this.Value < 65537 && this.Value == Math.Floor(this.Value)); }
        }

        public double Value
        {
            get
            {
                if (!_isFromValue)
                {
                    if (_isDirty)
                        _value = Math.Exp(_log);
                    _isDirty = false;
                }
                return (_value);
            }
            set
            {
                _value = value;
                _isFromValue = true;
                _isDirty = true;
            }
        }

        public double Log
        {
            get
            {
                if (_isFromValue)
                {
                    if (_isDirty)
                        _log = Math.Log(_value);
                    _isDirty = false;
                }
                return (_log);
            }
            set
            {
                _log = value;
                _isFromValue = false;
                _isDirty = true;
            }
        }

        public bool IsFromValue
        {
            get { return _isFromValue; }
        }

        public bool IsValid
        {
            get { return _isValid; }
        }

        public double Uncertainty
        {
            get { return _uncertainty; }
        }
        #endregion
        #region methods
        public CNumber Product(CNumber thatNumber)
        {
            CNumber numberReturn;
            if (thatNumber.IsValid)
            {
                double dblLog = this.Log + thatNumber.Log;
                numberReturn = new CNumber(dblLog, false);
            }
            else
                numberReturn = new CNumber();   // invalid

            return numberReturn;
        }
        public CNumber Quotient(CNumber thatNumber)
        {
            CNumber numberReturn;
            if (thatNumber.IsValid)
            {
                double dblLog = this.Log - thatNumber.Log;
                numberReturn = new CNumber(dblLog, false);
            }
            else
                numberReturn = new CNumber();   // invalid

            return numberReturn;
        }
        public CNumber Power(double dblPower)
        {
            double dblLog = this.Log * dblPower;
            CNumber numberReturn = new CNumber(dblLog, false);
            return numberReturn;
        }
        public CNumber Root(double dblRoot)
        {
            CNumber numberReturn;
            if (dblRoot != 0)
            {
                double dblLog = this.Log / dblRoot;
                numberReturn = new CNumber(dblLog, false);
            }
            else
                numberReturn = new CNumber();   // invalid

            return numberReturn;
        }
        // This comparison returns true if log is within standard difference
        // of other log.
        public bool IsEquivalent(CNumber otherNumber)
        {
            double dblDifference = Math.Abs(this.Log - otherNumber.Log);
            return dblDifference < _epsilon;
        }
        #endregion
        #region wrappers
        // used in sort()
        public int CompareTo(CNumber thatNumber)
        {
            return this.Log.CompareTo(thatNumber.Log);
        }
        public override string ToString()
        {
            return Value.ToString("E14");
        }
        public string LogToString()
        {
            return Log.ToString("E14");
        }
        public string ToString11()
        {
            return Value.ToString("E11");
        }
        public string LogToString11()
        {
            return Log.ToString("E11");
        }

        #endregion
    }
}
