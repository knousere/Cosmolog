// This class contains an integer ratio that represents a rational number
// It is used in normalizing and simplifying expressions that contain integers
namespace Cosmolog
{
    class CRatio
    {
        #region constructors
        public CRatio()
        {
            Init();
        }
        public CRatio(int intThatNumerator)
        {
            Init();
            _numerator = intThatNumerator;
        }
        public CRatio(int intThatNumerator, int intThatDenominator)
        {
            Init();
            _numerator = intThatNumerator;
            _denominator = intThatDenominator;
            Normalize();
        }
        public CRatio(CRatio thatRatio)
        {
            _numerator = thatRatio.Numerator;
            _denominator = thatRatio.Denominator;
            Normalize();
        }
        private void Init()
        {
            _numerator = 0;
            _denominator = 1;
            _isValid = true;
        }
        #endregion
        #region members
        protected int _numerator;
        protected int _denominator;
        protected bool _isValid; // true unless denominator is 0 
        #endregion
        #region properties
        public int Numerator
        {
            get { return _numerator; }
        }
        public int Denominator
        {
            get { return _denominator; }
        }
        public bool IsValid
        {
            get { return _isValid; }
        }
        #endregion
        #region methods
        public void Reset()
        {
            Init();
        }
        public void Set(int intThat)
        {
            _numerator = intThat;
            _denominator = 1;
            _isValid = true;
        }
        public void Set(CRatio thatRatio)
        {
            _numerator = thatRatio.Numerator;
            _denominator = thatRatio.Denominator;
            _isValid = thatRatio.IsValid;
            Normalize();
        }
        public void SetNumerator(int intThat)
        {
            _numerator = intThat;
        }
        // Reduce numerator and denominator by lowest common
        public void Normalize()
        {
            NormalizeSign();
            if (_numerator == _denominator)
            {
                if (_numerator != 1)
                    Set(1);
            }
            else
            {
                CPrimeFactorList factorListNumerator = new CPrimeFactorList(_numerator);
                factorListNumerator.CalcLowestCommon(ref _denominator);
                _numerator = factorListNumerator.Value;
            }
        }
        // denominator should always be positive
        public void NormalizeSign()
        {
            if (_denominator < 0)
            {
                _numerator *= -1;
                _denominator *= -1;
            }
            else if (_denominator == 0)
            {
                // panic, dividing by zero!
                _isValid = false;
            }
        }
        public void FlipSign()
        {
            _numerator *= -1;
        }
        // Return true if it divides evenly
        public static bool DivideInt(ref int intTarget, int intDivisor)
        {
            bool bRet = false;
            if (intDivisor == 0)
            {
                // panic, dividing by zero
                bRet = false;
            }
            else
            {
                int intTrial = intTarget / intDivisor;
                if (intTrial * intDivisor == intTarget)
                {
                    bRet = true;
                    intTarget = intTrial;
                }
            }
            return bRet;
        }
        public void ApplyMultiplication(CRatio thatRatio)
        {
            // Ignore overflow cases because they will not come up in this application.
            _numerator *= thatRatio.Numerator;
            _denominator *= thatRatio.Denominator;
            NormalizeSign();
        }
        public void ApplyDivision(CRatio thatRatio)
        {
            CRatio ratioDivision = new CRatio(thatRatio.Denominator, thatRatio.Numerator);
            ApplyMultiplication(ratioDivision);
        }
        // Apply power to the extent that it works out to an integer ratio.
        // This CRatio is always an integer, so the denominator is always 1 in this application.
        // Return true if it works out evenly (set ratioPower = 1/1)
        // else return false and set ratioPower to the unapplied portion.
        // Ignore overflow cases because they will not come up in this application.
        public bool ApplyPower(ref CRatio ratioPower)
        {
            bool bSuccess = false;
            ratioPower.NormalizeSign();
            if (this._numerator > 256 || ratioPower.Numerator > 16)
            {
                // Avoid overflow
                bSuccess = false;
            }
            if (ratioPower.Numerator == 0)
            {
                Set(1);
                bSuccess = true;
            }
            else if (ratioPower.Numerator > 0)
            {
                bSuccess = false;
                // Apply power numerator
                int intOrigNumerator = _numerator;
                for (int intPower = ratioPower.Numerator - 1; intPower > 0; --intPower)
                {
                    bSuccess = true;
                    _numerator *= intOrigNumerator;
                }
                ratioPower.SetNumerator(1);
                // Apply power denominator.
                if (ratioPower.Denominator != 1)
                {
                    if (_numerator == 1)
                    {
                        bSuccess = true;
                        ratioPower.Set(1);
                    }
                    else
                    {
                        // calculate root
                        CPrimeFactorList thisPrimeFactorList = new CPrimeFactorList(_numerator);
                        bSuccess = thisPrimeFactorList.ApplyRoot(ratioPower.Denominator);
                        if (bSuccess)
                        {
                            _numerator = thisPrimeFactorList.Value;
                            ratioPower.Set(1);  // power = 1/1
                        }
                    }
                }
            }
            else // negative numerator
            {
                bSuccess = false;
            }

            return bSuccess;
        }
        public void NumeratorAdd(int intValue)
        {
            _numerator += intValue;
        }
        public void NumeratorSubtract(int intValue)
        {
            _numerator -= intValue;
        }
        #endregion
        #region wrappers
        public override string ToString()
        {
            return _denominator != 1 ?
                "(" + _numerator.ToString() + "/" + _denominator.ToString() + ")" :
                _numerator.ToString();
        }
        public  bool Equals(CRatio thatRatio)
        {
            return (this._numerator == thatRatio._numerator && this._denominator == thatRatio._denominator);
        }
        public bool Equals(int intThat)
        {
            return (this._numerator == intThat && this._denominator == 1);

        }
        #endregion
    }
}
