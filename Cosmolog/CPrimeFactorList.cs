using System.Collections.Generic;
// (c)Richard E. Knouse 2015-1019

namespace Cosmolog
{
    // This class reduces an integer to its prime factors.
    // It is used to normalize expressions that contain integers.
    class CPrimeFactorList : List<CPrimeFactor>
    {
        #region constructors
        public CPrimeFactorList(int intValue)
        {
            _value = intValue;
            _remainder = _value;
            int i = 0;
            int intCount = 0;
            int intRemainder = _remainder;
            while (i < 6 && intRemainder >= _primes[i])
            {
                intCount = 0;
                while (intRemainder == (intRemainder / _primes[i]) * _primes[i])
                {
                    ++intCount;
                    intRemainder /= _primes[i];
                }
                if (intCount > 0)
                    this.Add(new CPrimeFactor(_primes[i], intCount));
                ++i;
            }
            _remainder = intRemainder;
        }
        #endregion
        #region members
        private int[] _primes = { 2, 3, 5, 7, 11, 13 };
        protected int _value;
        protected int _remainder;
        #endregion
        #region properties
        public int Value
        {
            get { return _value; }
        }
        public int Remainder
        {
            get { return _remainder; }
        }
        #endregion
        #region methods
        public int Find(int intPrime)
        {
            int i;
            for (i = Count - 1; i >= 0; --i)
            {
                if (this[i].Prime == intPrime)
                    break;
            }
            return i;
        }
        // Return true and apply root to value if the result works out to integer
        // else return false.
        public bool ApplyRoot(int intRoot)
        {
            bool bSuccess = false;
            if (intRoot == 0)
            {
                // panic, zero root
                bSuccess = false;
            }
            else if (intRoot == 1)
                bSuccess = true;
            else if (intRoot < 0)
            {
                // panic, negative root
                bSuccess = false;
            }
            else if (_remainder == 1)
            {
                // general case
                bSuccess = true;    // innocent until proven guilty
                for (int i = 0; i < Count; ++i)
                {
                    CPrimeFactor thisFactor = this[i];
                    int intTrial = thisFactor.Count;
                    bSuccess = CRatio.DivideInt(ref intTrial, intRoot);
                    if (bSuccess)
                    {
                        thisFactor.SetCount(intTrial);
                    }
                    else
                        break;  // guilty
                }
                if (bSuccess)
                    Recalc();
            }
            return bSuccess;
        }
        // Recalculate value from factors.
        // Called after factors have been modified.
        public void Recalc()
        {
            int intTarget = _remainder;
            for (int i = 0; i < Count; ++i)
            {
                CPrimeFactor thisFactor = this[i];
                for (int j = 0; j < thisFactor.Count; ++j)
                {
                    intTarget *= thisFactor.Prime;
                }
            }
            _value = intTarget;
        }
        // Reduce this._value vs. intThatValue
        public void CalcLowestCommon(ref int intThatValue)
        {
            CPrimeFactorList thatList = new CPrimeFactorList(intThatValue);
            CalcLowestCommon(ref thatList);
            intThatValue = thatList.Value;
        }
        // Reduce this vs thatList
        public void CalcLowestCommon(ref CPrimeFactorList thatList)
        {
            int intThis = 0;
            int intThat = 0;
            for (intThis = 0; intThis < Count; ++intThis)
            {
                CPrimeFactor thisPrimeFactor = this[intThis];
                intThat = thatList.Find(thisPrimeFactor.Prime);
                if (intThat >= 0)
                {
                    CPrimeFactor thatPrimeFactor = thatList[intThat];
                    while (thisPrimeFactor.Count > 0 && thatPrimeFactor.Count > 0)
                    {
                        thisPrimeFactor.Decrement();
                        thatPrimeFactor.Decrement();
                    }
                }
            }
            if (_remainder == thatList._remainder)
            {
                _remainder = 1;
                thatList._remainder = 1;
            }
            CleanUp();
            thatList.CleanUp();
            Recalc();
            thatList.Recalc();
        }
        public void CleanUp()
        {
            for (int i = Count - 1; i >= 0; --i)
            {
                if (this[i].Count < 1)
                    this.RemoveAt(i);
            }
        }
        #endregion
    }
}
