using System;
// (c)Richard E. Knouse 2015-1019

// name/index pair that points to element of CQuantityList
namespace Cosmolog
{
    class CQtyPointer : IComparable<CQtyPointer>
    {
        #region constructors

        public CQtyPointer(CQuantity thisQuantity)
        {
            this.Qty = thisQuantity;
        }

        public CQtyPointer(CQtyPointer thisQtyPointer)
        {
            this.Qty = thisQtyPointer.Qty;
        }

        private void Init()
        {
            _isAssigned = false;
        }
        #endregion
        #region members
        protected bool _isAssigned;
        protected CQuantity _qty;        // index of the attached quantity
        #endregion
        #region properties
        public string Label
        {
            get { return _qty.Label; }
            set { if (_isAssigned) _qty.Label = value; }
        }

        public CQuantity Qty
        {
            get { return _qty; }
            set { _qty = value; _isAssigned = true; }
        }

        public bool IsConstant
        {
            get { return _qty.IsExact; }
        }
        
        #endregion
        #region methods

        #endregion
        #region wrappers
        public int CompareTo(CQtyPointer otherPointer)
        {
            double dblDelta = _qty.Log - otherPointer.Qty.Log;
            if (_qty.IsEquivalent(otherPointer.Qty))
                return 0;
            else if (dblDelta < 0)
                return -1;
            else
                return 1;
        }

        public override string ToString()
        {
            return (_qty == null? "qty.null": _qty.Label);
        }

        #endregion
    }
}
