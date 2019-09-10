using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Documents;
using Word = Microsoft.Office.Interop.Word;
using System.Reflection;
using System.Threading;

namespace Cosmolog
{
    public partial class Form1 : Form
    {
        #region constructors
        public Form1()
        {
            InitializeComponent();
            _myQuantityList = new CQuantityList();
            _myQtyPointerList = new CQtyPointerList();
            _intQtyIndex = 0;
        }
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            // These three had to be manually written to suppress CA2213
            if (_fontItalic != null) _fontItalic.Dispose();
            if (_fontMain != null) _fontMain.Dispose();
            if (_fontSubscript != null) _fontSubscript.Dispose();

            base.Dispose(disposing);
        }
        #endregion
        #region members
        private string _strBasePath;
        private string _strLogPath;
        private CQuantityList _myQuantityList;
        private CQuantityList _StaticQuantityList;
        private CQuantityList _ComputationalQuantityList;
        private CQtyPointerList _myQtyPointerList;
        private CSymbolList _mySymbolList;
        private CMatrix _myMatrix;
        private Font _fontMain;
        private Font _fontItalic;
        private Font _fontSubscript;
        private int _intQtyIndex;
        private int _intPageIndex;
        private int _intColumns;
        private int _intMaxFactors;
        const int _cintPageSize = 25;
        const int _cintBold = 1;
        const int _cintItalic = 2;
        const int _cintSubscript = 4;
        const int _cintSuperscript = 8;
        #endregion
        #region event handlers
        private void Form1_Load(object sender, EventArgs e)
        {
            _strBasePath = "C:\\Cosmolog\\";
            _strLogPath = _strBasePath + "Log.txt";
            _myQuantityList = new CQuantityList();
            _StaticQuantityList = new CQuantityList();
            _ComputationalQuantityList = new CQuantityList();
            _mySymbolList = new CSymbolList();
            _myMatrix = new CMatrix();
            _fontMain = new Font("Times New Roman", 12, System.Drawing.FontStyle.Regular);
            _fontItalic = new Font("Times New Roman", 12, System.Drawing.FontStyle.Italic);
            _fontSubscript = new Font(_fontMain.Name, 6, System.Drawing.FontStyle.Regular);
            rtExpression.Font = _fontMain;
            _intQtyIndex = 0;
            _intPageIndex = 0;
            _intColumns = 4;
            _intMaxFactors = 2;
            this.txtColumns.Text = _intColumns.ToString();
            this.txtMaxFactors.Text = _intMaxFactors.ToString();
            this.updownPage.Maximum = 1;
            this.updownPage.Value = 1;
        }

        // Read a list of quantities
        private void btnReadQuantities_Click(object sender, EventArgs e)
        {
            ReadQuantities(ref _myQuantityList, "Read Quantities from File");
            RepaintQuantityDisplay(0,0, true);
            RefreshSymbolCombo();
        }

        // Make sure every quantity in the list has an inverse.
        // Either find it or create it.
        private void btnSetInverses_Click(object sender, EventArgs e)
        {
            int intCount = _myQuantityList.Count;
            int i;
            for (i = 0; i < intCount; ++i)
            {
                CQuantity thisQuantity = _myQuantityList[i];
                if (!thisQuantity.IsInteger && thisQuantity.InverseQty == null)
                {
                    CQuantity qtyInverse = thisQuantity.Inverse("X_" + _myQuantityList.Count.ToString());
                    int intInverse = _myQuantityList.FindEquivalent(qtyInverse);

                    if (intInverse == i)
                    {
                        // 1 is the inverse of 1
                        thisQuantity.SetInverse(ref thisQuantity);
                    }
                    else if (intInverse > -1)
                    {
                        // inverse found, so pair up
                        qtyInverse = _myQuantityList[intInverse];
                        thisQuantity.SetInverse(ref qtyInverse);
                    }
                    else
                    {
                        // inverse not found, so add
                        qtyInverse.Symbol = thisQuantity.SymbolInverse();
                        _myQuantityList.Add(qtyInverse);
                        thisQuantity.SetInverse(ref qtyInverse);
                    }
                }
            }
            MakeComputationalList();
            RepaintQuantityDisplay(0,0, true);
            RefreshSymbolCombo();
        }
        private void MakeComputationalList()
        {
            
            // Establish the Computational List from the complete quantity list
            _ComputationalQuantityList.Clear();
            int i;
            for (i = 0; i < _myQuantityList.Count; ++i)
            {
                CQuantity thisQuantity = _myQuantityList[i];
                if (thisQuantity.IsComputational || thisQuantity.IsQED)
                {
                    CQuantity thatQuantity = new CQuantity(thisQuantity);
                    thatQuantity.ResetIndex();
                    // thatQuantity.ResetInverse();
                    _ComputationalQuantityList.Add(thatQuantity);
                }
            }
            int intQEDPlus = _ComputationalQuantityList.FindSimpleMatch("π+");
            int intQEDMinus = _ComputationalQuantityList.FindSimpleMatch("π-");
            if (intQEDPlus > -1 && intQEDMinus > -1)
            {
                CQuantity qtyPiQED = _ComputationalQuantityList[intQEDPlus];
                CQuantity qtyPiQEDInverse = _ComputationalQuantityList[intQEDMinus];
                CQuantity qtyFour = new CQuantity(_ComputationalQuantityList, true, "4", 4, "Four");
                CFactor factorFour = new CFactor(qtyFour);
                CFactor factorPiQED = new CFactor(qtyPiQED);
                CFactor factorPiQEDInverse = new CFactor(qtyPiQEDInverse);
                _ComputationalQuantityList.Add(qtyFour);
                for (i = 0; i < _ComputationalQuantityList.Count; ++i)
                {
                    CQuantity thisQuantity = _ComputationalQuantityList[i];
                    string strLabel = thisQuantity.SimpleLabel;
                    CExpression thisExpression;
                    switch (strLabel)
                    {
                        case "π4q":
                            thisExpression = new CExpression();
                            thisExpression.Numerator.Add(new CFactor(factorFour));
                            thisExpression.Numerator.Add(new CFactor(factorPiQED));
                            thisQuantity.ExpressionList.Add(thisExpression);
                            break;
                        case "π4q2":
                            thisExpression = new CExpression();
                            thisExpression.Numerator.Add(new CFactor(factorFour));
                            thisExpression.Numerator.Add(new CFactor(factorPiQED));
                            thisExpression.RatioPower.Set(2);
                            thisQuantity.ExpressionList.Add(thisExpression);
                            break;
                        case "π4q3":
                            thisExpression = new CExpression();
                            thisExpression.Numerator.Add(new CFactor(factorFour));
                            thisExpression.Numerator.Add(new CFactor(factorPiQED));
                            thisExpression.RatioPower.Set(3);
                            thisQuantity.ExpressionList.Add(thisExpression);
                            break;
                        case "π4q-":
                            thisExpression = new CExpression();
                            thisExpression.Numerator.Add(new CFactor(factorPiQEDInverse));
                            thisExpression.Denominator.Add(new CFactor(factorFour));
                            thisQuantity.ExpressionList.Add(thisExpression);
                            break;
                        case "π4q2-":
                            thisExpression = new CExpression();
                            thisExpression.Numerator.Add(new CFactor(factorPiQEDInverse));
                            thisExpression.Denominator.Add(new CFactor(factorFour));
                            thisExpression.RatioPower.Set(2);
                            thisQuantity.ExpressionList.Add(thisExpression);
                            break;
                        case "π4q3-":
                            thisExpression = new CExpression();
                            thisExpression.Numerator.Add(new CFactor(factorPiQEDInverse));
                            thisExpression.Denominator.Add(new CFactor(factorFour));
                            thisExpression.RatioPower.Set(3);
                            thisQuantity.ExpressionList.Add(thisExpression);
                            break;
                    }
                }
            }
        }

        // Fill matrix with products and quotients
        private void btnMakeMatrix_Click(object sender, EventArgs e)
        {
            int intX;
            int intY;
            CMatrixCell myMatrixCell;
            int i;

            // Fill matrix with all combinations of X * Y
            for (intX = 0; intX < _myQuantityList.Count; ++intX)
            {
                for (intY = 0; intY < _myQuantityList.Count - intX; ++intY)
                {
                    if (!(_myQuantityList[intX].IsInteger && _myQuantityList[intY].IsInteger))
                    {
                        myMatrixCell = new CMatrixCell(_myQuantityList[intX], _myQuantityList[intY], true);
                        _myMatrix.Add(myMatrixCell);
                    }
                }
            }
            // Fill matrix with all permutations of X/Y
            for (intX = 0; intX < _myQuantityList.Count; ++intX)
            {
                for (intY = 0; intY < _myQuantityList.Count; ++intY)
                {
                    if (!(_myQuantityList[intX].IsInteger && _myQuantityList[intY].IsInteger))
                    {
                        myMatrixCell = new CMatrixCell(_myQuantityList[intX], _myQuantityList[intY], false);
                        _myMatrix.Add(myMatrixCell);
                    }
                }
            }
            // Associate any matrix cell with its equivalent quantity if any.
            for (i = 0; i < _myMatrix.Count; ++i)
            {
                myMatrixCell = _myMatrix[i];
                int intIndexQty = _myQuantityList.FindEquivalent(myMatrixCell.Number);
                if (intIndexQty > -1)
                    myMatrixCell.PtrQty = new CQtyPointer(_myQuantityList[intIndexQty]);
            }
            // Sort matrix cells in value order.
            _myMatrix.Sort();

            // WriteMatrix();   // perform diagnostic dump of matrix prior to processing

            // At this Point any unassigned cells will be sorted together by value, forming groups.
            // A group that is more than tautological is considered to be a quantity.
            int intCount = 0;
            int intFirst = -1;
            CMatrixCell firstCell = new CMatrixCell();  // an empty cell is by definition unassigned
            bool bIsGroup = false;
            for (i = 0; i < _myMatrix.Count; ++i)
            {
                CMatrixCell thisCell = _myMatrix[i];
                if (thisCell.IsAssigned)
                {
                    // An assigned cell terminates the previous group
                    if (intCount > 0)
                        bIsGroup = true;
                }
                else
                {
                    // An unassigned cell either starts a new group or continues an existing group
                    if (intCount == 0)
                    {
                        // start new group
                        intFirst = i;
                        intCount = 1;
                        firstCell = thisCell;
                    }
                    else if (thisCell.IsEquivalent(firstCell))
                    {
                        // this cell continues an existing group
                        intCount += 1;
                    }
                    else
                    {
                        // This cell, by not matching, terminates the previous group
                        bIsGroup = true;
                    }
                }
                if (bIsGroup)
                {
                    // This is a group that potentially defines a new quantity
                    bIsGroup = false;
                    _myMatrix.QualifyGroup(intFirst, intCount, _myQuantityList);
                    if (thisCell.IsAssigned)
                    {
                        // no group
                        firstCell = new CMatrixCell();
                        intFirst = -1;
                        intCount = 0;
                    }
                    else
                    {
                        // possible new group
                        firstCell = thisCell;
                        intFirst = i;
                        intCount = 1;
                    }
                }
            }
            // Assign formulas to each quantity
            for (i = 0; i < _myMatrix.Count; ++i)
            {
                CMatrixCell thisCell = _myMatrix[i];
                if (thisCell.IsAssigned)
                {
                    CQuantity thisQuantity = thisCell.PtrQty.Qty;
                    if (thisCell.IsMatrixQuantity())
                    {
                        CExpression thisExpression = new CExpression(thisCell, true);
                        thisQuantity.ExpressionList.Add(thisExpression);
                    }
                }
            }
            // Suppress duplicates in generated expression lists per quantity
            for (i = 0; i < _myQuantityList.Count; ++i)
            {
                CQuantity thisQuantity = _myQuantityList[i];
                thisQuantity.ExpressionList.SuppressDupes();
                thisQuantity.ExpressionList.SuppressTautology(thisQuantity);
            }
            RepaintQuantityDisplay(0,0, true);
            RefreshSymbolCombo();
        }

        private void btnExtend_Click(object sender, EventArgs e)
        {
            _myQuantityList.ExtendPrecision();
            RepaintQuantityDisplay(_intQtyIndex,0,true);
        }

        private void btnWriteExpressions_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlgOutputFile = new SaveFileDialog();
            dlgOutputFile.Title = "Save Expressions to File";
            dlgOutputFile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dlgOutputFile.AddExtension = true;
            dlgOutputFile.InitialDirectory = _strBasePath;
            if (dlgOutputFile.ShowDialog() == DialogResult.OK)
            {
                string strOutputPath = dlgOutputFile.FileName;
                CQtyPointerList thisQtyPointerList = new CQtyPointerList(ref _myQuantityList);
                thisQtyPointerList.WriteExpressions(strOutputPath, _strLogPath);
            }
        }

        private void btnWriteMatrix_Click(object sender, EventArgs e)
        {
            WriteMatrix();
        }

        private void WriteMatrix()
        {
            SaveFileDialog dlgOutputFile = new SaveFileDialog();
            dlgOutputFile.Title = "Save Matrix to File";
            dlgOutputFile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dlgOutputFile.AddExtension = true;
            dlgOutputFile.InitialDirectory = _strBasePath;
            if (dlgOutputFile.ShowDialog() == DialogResult.OK)
            {
                string strOutputPath = dlgOutputFile.FileName;
                _myMatrix.WriteMatrix(strOutputPath, _strLogPath);
            }
        }


        private void btnWriteValues_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlgOutputFile = new SaveFileDialog();
            dlgOutputFile.Title = "Save Values to File";
            dlgOutputFile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dlgOutputFile.AddExtension = true;
            dlgOutputFile.InitialDirectory = _strBasePath;
            if (dlgOutputFile.ShowDialog() == DialogResult.OK)
            {
                string strOutputPath = dlgOutputFile.FileName;
                CQtyPointerList thisQtyPointerList = new CQtyPointerList(ref _myQuantityList);
                thisQtyPointerList.WriteValues(strOutputPath, _strLogPath);
            }
        }

        private void btnAllToWord_Click(object sender, EventArgs e)
        {
            int intWordIndex = 0;
            if (txtWordIndex.Text == "")
                intWordIndex = 0;
            else
                intWordIndex = System.Convert.ToInt32(this.txtWordIndex.Text) - 1; ;
            AllToWord(intWordIndex);
        }

        private void btnOneToWord_Click(object sender, EventArgs e)
        {
            OneToWord(_intQtyIndex, false);
        }
        // Use Substitution to expand every expression list associated with each quantity.
        private void btnSubstitute_Click(object sender, EventArgs e)
        {
            DialogResult thisResult = MessageBox.Show("This can take a very long time. Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo);
            _intMaxFactors = System.Convert.ToInt32(this.txtMaxFactors.Text);
            if (thisResult == DialogResult.Yes)
            {
                for (int i = 0; i < _myQtyPointerList.Count; ++i)
                {
                    CQuantity thisQuantity = _myQtyPointerList[i].Qty;
                    this.txtSubstIndex.Text = i.ToString();
                    int intStart = 0;
                    string strRichLine = thisQuantity.Symbol.SimpleLabel;
                    this.rtSubstSymbol.Clear();
                    this.rtSubstSymbol.AppendText(strRichLine);
                    FormatRichTextSymbol(rtSubstSymbol, thisQuantity, ref intStart);
                    txtSubstIndex.Refresh();
                    rtSubstSymbol.Refresh();
                    Application.DoEvents();
                    thisQuantity.CalculateSubstitution(_intMaxFactors, true);
                    //thisQuantity.CalculateSubstitution(_intMaxFactors, false);
                }
                for (int j = 0; j < _myQtyPointerList.Count; ++j)
                {
                    CQuantity thisQuantity = _myQtyPointerList[j].Qty;
                    thisQuantity.ApplyCandidateExpressionList();
                }
                RepaintQuantityDisplay(_intQtyIndex,0,true);
            }
        }

        private void updownQty_ValueChanged(object sender, EventArgs e)
        {
            int intPtr = System.Convert.ToInt32(this.updownQty.Value);
            RepaintQuantityDisplay(intPtr - 1,0,true);
        }

        private void btnSubstituteOne_Click(object sender, EventArgs e)
        {
            this.rtExpression.Clear();
            CQuantity thisQuantity = _myQtyPointerList[_intQtyIndex].Qty;
            _intMaxFactors = System.Convert.ToInt32(this.txtMaxFactors.Text);
            thisQuantity.CalculateSubstitution(_intMaxFactors, true);
            thisQuantity.CalculateSubstitution(_intMaxFactors, false);
            thisQuantity.ApplyCandidateExpressionList();
            RepaintQuantityDisplay(_intQtyIndex,0,true);
        }

        // Where a calculated quantity matches the value in the static file,
        // substitute the symbol from the static file
        private void btnMatchSymbols_Click(object sender, EventArgs e)
        {
            if (_StaticQuantityList == null || _StaticQuantityList.Count == 0)
            {
                if (_StaticQuantityList == null)
                    _StaticQuantityList = new CQuantityList();
                ReadQuantities(ref this._StaticQuantityList, "Read Match Quantities from File");
            }

            for (int q = 0; q < _myQuantityList.Count; ++q)
            {
                CQuantity thisQuantity = _myQuantityList[q];
                int s = _StaticQuantityList.FindEquivalent(thisQuantity);
                if (s > -1)
                {
                    CQuantity thatQuantity = _StaticQuantityList[s];
                    thisQuantity.Symbol = thatQuantity.Symbol;
                }
            }
            RepaintQuantityDisplay(_intQtyIndex,0,true);
            RefreshSymbolCombo();
        }
        private void rtInverse_Click(object sender, EventArgs e)
        {
            int intPtr = System.Convert.ToInt32(this.updownQty.Value);
            --intPtr;
            CQuantity thisQuantity = _myQtyPointerList[intPtr].Qty;
            CQuantity inverseQty = thisQuantity.InverseQty;
            int intInverseIndex = inverseQty.Index;
            if (intInverseIndex > -1)
            {
                int intQtyIndex = _myQtyPointerList.FindByQtyIndex(intInverseIndex);
                if (intQtyIndex > -1)
                    RepaintQuantityDisplay(intQtyIndex,0,true);
            }
        }

        //Cull all quantities that begin with 'X' in both quantity list and expressions
        private void btnCullAllX_Click(object sender, EventArgs e)
        {
            int q;
            for (q = 0; q < _myQtyPointerList.Count; ++q)
            {
                CQuantity thisQuantity = _myQtyPointerList[q].Qty;
                thisQuantity.ExpressionList.CullX();
            }
            for (q = _myQuantityList.Count - 1; q > -1; --q)
            {
                CQuantity thisQuantity = _myQuantityList[q];
                if (thisQuantity.ContainsX())
                    _myQuantityList.RemoveAt(q);
            }
            _myQuantityList.RepairIndices();
            _myQtyPointerList = new CQtyPointerList(ref _myQuantityList);
            RepaintQuantityDisplay(0,0,true);
            RefreshSymbolCombo();
        }
        // Delete this quantity and its inverse from the quantity list and all expressions
        private void btnDeleteQuantity_Click(object sender, EventArgs e)
        {
            CQuantity thisQuantity = _myQtyPointerList[_intQtyIndex].Qty;
            CQuantity inverseQuantity = thisQuantity.InverseQty;
            CSymbol thisSymbol = thisQuantity.Symbol;
            CSymbol inverseSymbol = null;
            if (inverseQuantity != null)
                inverseSymbol = inverseQuantity.Symbol;
            int q;
            for (q = 0; q < _myQtyPointerList.Count; ++q)
            {
                CQuantity cullQuantity = _myQtyPointerList[q].Qty;
                cullQuantity.ExpressionList.CullSymbol(thisSymbol);
                if (inverseSymbol != null)
                    cullQuantity.ExpressionList.CullSymbol(inverseSymbol);
            }
            if (inverseQuantity != null)
                _myQuantityList.RemoveAt(inverseQuantity.Index);
            _myQuantityList.RemoveAt(thisQuantity.Index);
            _myQuantityList.RepairIndices();
            _myQtyPointerList = new CQtyPointerList(ref _myQuantityList);
            if (_intQtyIndex > 0)
                --_intQtyIndex;
            RepaintQuantityDisplay(_intQtyIndex,0,true);
            RefreshSymbolCombo();
        }
        #endregion
        #region methods
        private void RefreshSymbolCombo()
        {
            string[] stringArray = new string[_myQuantityList.Count];
            _myQuantityList.MakeStringArray(ref stringArray);
            this.cboSymbol.Items.Clear();
            this.cboSymbol.Items.AddRange(stringArray);
        }
        private void ReadQuantities(ref CQuantityList theQuantityList, string strTitle)
        {
            OpenFileDialog dlgInputFile = new OpenFileDialog();
            dlgInputFile.Title = strTitle;
            dlgInputFile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dlgInputFile.InitialDirectory = _strBasePath;
            if (dlgInputFile.ShowDialog() == DialogResult.OK)
            {
                string strQtyPath = dlgInputFile.FileName;
                theQuantityList.ReadValues(strQtyPath, _strLogPath);
                if (_mySymbolList.Count == 0)
                {
                    dlgInputFile.Title = "Read Descriptions from File";
                    if (dlgInputFile.ShowDialog() == DialogResult.OK)
                    {
                        string strSymbolPath = dlgInputFile.FileName;
                        _mySymbolList.ReadSymbols(strSymbolPath, _strLogPath);
                    }
                }
                theQuantityList.SetAllDescriptions(_mySymbolList);
            }
        }

        private void RepaintQuantityBox()
        {
            rtExpression.Clear();
            // Make an index into the QuantityList sorted by value.
            _myQtyPointerList = new CQtyPointerList(ref _myQuantityList);

            for (int i = 0; i < _myQtyPointerList.Count; ++i)
            {
                CQuantity thisQuantity = _myQtyPointerList[i].Qty;
                int intStart = rtExpression.Text.Length;
                string strRichLine = thisQuantity.ToRichString();
                rtExpression.AppendText(strRichLine);
                FormatRichTextSymbol(rtExpression, thisQuantity, ref intStart);
            }
            txtQtyCount.Text = _myQuantityList.Count.ToString();
        }
        private void ClearQuantityDisplay()
        {
            this.rtSymbol.Clear();
            this.rtInverse.Clear();
            this.txtValue.Clear();
            this.txtLog.Clear();
            this.txtDescription.Clear();
            this.rtExpression.Clear();
            this.chkInteger.Checked = false;
            this.txtWordIndex.Clear();
            this.rtWordSymbol.Clear();
            this.txtExpressionCount.Clear();
            this.txtPrintableCount.Clear();
        }
        private void RepaintQuantityDisplay(int intQtyIndex, int intPageIndex, bool bSuppressDupes)
        {
            ClearQuantityDisplay();
            if (intQtyIndex >= _myQuantityList.Count)
                intQtyIndex = _myQuantityList.Count - 1;
            if (_myQuantityList.Count > 0 && intQtyIndex < 0)
                intQtyIndex = 0;
            _intQtyIndex = intQtyIndex;
            bool bHideSense = this.chkHideTilda.Checked;
            if (intQtyIndex >= 0)
            {
                if (_myQtyPointerList.Count != _myQuantityList.Count)
                {
                    _myQtyPointerList = new CQtyPointerList(ref _myQuantityList);
                    txtQtyCount.Text = _myQuantityList.Count.ToString();
                }
                CQuantity thisQuantity = _myQtyPointerList[intQtyIndex].Qty;
                this.chkConstant.Checked = thisQuantity.IsExact;
                this.chkComp.Checked = thisQuantity.IsComputational;
                this.chkQED.Checked = thisQuantity.IsQED;
                this.chkSuppressed.Checked = thisQuantity.IsSuppressed;
                this.chkInteger.Checked = thisQuantity.IsInteger;
                int intStart = 0;
                string strRichLine = thisQuantity.Symbol.SimpleLabel;
                this.rtSymbol.AppendText(strRichLine);
                FormatRichTextSymbol(rtSymbol, thisQuantity, ref intStart);
                if (thisQuantity.InverseQty != null)
                {
                    CQuantity inverseQuantity = thisQuantity.InverseQty;
                    intStart = 0;
                    strRichLine = inverseQuantity.Symbol.SimpleLabel;
                    this.rtInverse.AppendText(strRichLine);
                    FormatRichTextSymbol(rtInverse, inverseQuantity, ref intStart);
                }
                txtValue.Text = thisQuantity.Number.ToString();
                txtLog.Text = thisQuantity.Number.LogToString();
                txtDescription.Text = thisQuantity.Description;
                txtUnit.Text = thisQuantity.Unit;
                CExpressionList thisExpressionList = new CExpressionList(thisQuantity.ExpressionList, bHideSense);
                if (bSuppressDupes)
                    thisExpressionList.SuppressDupes();
                thisExpressionList.Sort();
                txtExpressionCount.Text = thisExpressionList.Count.ToString();
                int intPrintableCount = thisExpressionList.CountPrintable();
                txtPrintableCount.Text = intPrintableCount.ToString();

                while (intPageIndex * _cintPageSize > thisExpressionList.Count)
                    --intPageIndex;
                if (intPageIndex < 0)
                    intPageIndex = 0;
                _intPageIndex = intPageIndex;
                int intPageLimit = thisExpressionList.Count / _cintPageSize + 1;
                this.updownPage.Maximum = intPageLimit;
                this.updownPage.Value = intPageIndex + 1;
                this.txtPageCount.Text = intPageLimit.ToString();
                for (int j = intPageIndex * _cintPageSize; j < thisExpressionList.Count && j < (intPageIndex+1) * _cintPageSize; ++j)
                {
                    CExpression thisExpression = thisExpressionList[j];
                    string strLine = " = " + thisExpression.ValueToString() + " " + thisExpression.LogToString() + " " + thisExpression.ToString() + "\r\n";
                    FormatRichTextExpression(rtExpression, strLine);
                }
                thisExpressionList.Clear(); // Assist the garbage collector
            }
            this.updownQty.Maximum = _myQuantityList.Count;
            this.updownQty.Value = intQtyIndex + 1;
            this.txtQtyCount.Text = _myQuantityList.Count.ToString();
        }
        #endregion
        #region Rich Text
        private void FormatRichTextExpression(System.Windows.Forms.RichTextBox thisRichTextBox, string strLine)
        {
            string strSkip = "= \t\r\n()*/^";
            string strDigit = "-+.0123456789";
            string strNumber = strDigit + "E";
            int intStart = rtExpression.Text.Length;
            int intStartWhiteSpace = intStart;
            rtExpression.AppendText(strLine);
            int intLimit = rtExpression.Text.Length;
            int intStartThisPass;

            while (intStart < intLimit)
            {
                intStartThisPass = intStart;
                // look for white space
                while (intStart < intLimit && strSkip.Contains(rtExpression.Text[intStart]))
                    ++intStart;
                if (intStartWhiteSpace < intStart)
                {
                    // found white space
                    intStartWhiteSpace = FormatRichTextWhiteSpace(rtExpression, intStartWhiteSpace, intStart);
                }
                // look for a number
                if (intStart < intLimit && strDigit.Contains(rtExpression.Text[intStart]))
                {
                    while (intStart < intLimit && strNumber.Contains(rtExpression.Text[intStart]))
                        ++intStart;
                }
                if (intStartWhiteSpace < intStart)
                {
                    // found number
                    intStartWhiteSpace = FormatRichTextWhiteSpace(rtExpression, intStartWhiteSpace, intStart);
                }
                else if (intStart < intLimit)
                {
                    
                    // found symbol
                    intStartWhiteSpace = intStart;
                    while (intStartWhiteSpace < intLimit && !strSkip.Contains(rtExpression.Text[intStartWhiteSpace]))
                        ++intStartWhiteSpace;
                    string strSymbol = rtExpression.Text.Substring(intStart, intStartWhiteSpace - intStart);
                    int intThatIndex = _myQuantityList.FindSimpleMatch(strSymbol);
                    if (intThatIndex > -1)
                    {
                        CQuantity thatQuantity = _myQuantityList[intThatIndex];
                        intStartWhiteSpace = FormatRichTextSymbol(rtExpression, thatQuantity, ref intStart);
                    }
                    else
                    {
                        intStart = intStartWhiteSpace;
                        break; // should not get here!
                    }
                }
                if (intStartThisPass == intStart)
                {
                    break;  //!!! should not get here!
                }
            }
        }
        private int FormatRichTextSymbol(System.Windows.Forms.RichTextBox thisRichTextBox, CQuantity thisQuantity, ref int intStart)
        {
            int intSelectionLen = thisQuantity.SymbolMainLen;
            thisRichTextBox.Select(intStart, intSelectionLen);
            thisRichTextBox.SelectionFont = (thisQuantity.IsInteger ? _fontMain : _fontItalic);
            thisRichTextBox.SelectionColor = (thisQuantity.IsInteger ? Color.Black : Color.Blue);
            intStart += intSelectionLen;
            if (thisQuantity.SymbolSubscriptLen > 0)
            {
                intSelectionLen = thisQuantity.SymbolSubscriptLen;
                thisRichTextBox.Select(intStart, intSelectionLen);
                thisRichTextBox.SelectionFont = _fontSubscript;
                thisRichTextBox.SelectionColor = (thisQuantity.IsInteger ? Color.Black : Color.Blue);
                intStart += intSelectionLen;
            }
            return intStart;
        }
        private int FormatRichTextWhiteSpace(System.Windows.Forms.RichTextBox thisRichTextBox, int intStart, int intEnd)
        {
            int intSelectionLen = intEnd - intStart;
            thisRichTextBox.Select(intStart, intSelectionLen);
            thisRichTextBox.SelectionFont = _fontMain;
            thisRichTextBox.SelectionColor = Color.Black;
            return intEnd;
        }
        #endregion
        #region Word
        private void ValuesToWord()
        {
            bool bHideTilde = this.chkHideTilda.Checked;
            object oMissing = System.Reflection.Missing.Value;
            object oEndOfDoc = "\\endofdoc"; // endofdoc is a predefined bookmark


            //Start Word and create a new document.
            Word._Application oWord;
            Word._Document oDoc;
            oWord = new Word.Application();

            oWord.Visible = true;
            oDoc = oWord.Documents.Add(ref oMissing, ref oMissing,
                ref oMissing, ref oMissing);


            string strPath;
            strPath = @"C:\Cosmolog\Values.docx";
            object objPath = strPath;
            oDoc.SaveAs(ref objPath, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);

            int q;
            for (q = 0; q < _myQtyPointerList.Count; q++)
            {
                int intWordIndex = q + 1;
                this.txtWordIndex.Text = intWordIndex.ToString();
                CQuantity thisQuantity = _myQtyPointerList[q].Qty;
                if (thisQuantity.IsComputational || thisQuantity.IsSuppressed || thisQuantity.IsInteger)
                {
                }
                else
                {
                    int intStart = 0;
                    string strRichLine = thisQuantity.Symbol.SimpleLabel;
                    this.rtWordSymbol.Clear();
                    this.rtWordSymbol.AppendText(strRichLine);
                    FormatRichTextSymbol(rtWordSymbol, thisQuantity, ref intStart);

                    Word.Paragraph oPara4;
                    oPara4 = oDoc.Content.Paragraphs.Add(ref oMissing);
                    intStart = oPara4.Range.Start;
                    string strDescription = thisQuantity.Description;
                    int intPos = strDescription.IndexOfAny(new char[] { '_', '^' });
                    while (intPos > -1)
                    {
                        string strLeft = strDescription.Substring(0, intPos);
                        FormatWordText(strLeft, _cintBold, ref intStart, ref oDoc);
                        string strToken = strDescription.Substring(intPos + 1, 1);
                        if (strDescription[intPos] == '_')
                            FormatWordText(strToken, _cintBold | _cintSubscript, ref intStart, ref oDoc);
                        else
                            FormatWordText(strToken, _cintBold | _cintSuperscript, ref intStart, ref oDoc);
                        strDescription = strDescription.Substring(intPos + 2);
                        intPos = strDescription.IndexOfAny(new char[] { '_', '^' });
                    }
                    if (strDescription.Length > 0)
                        FormatWordText(strDescription, _cintBold, ref intStart, ref oDoc);

                    FormatWordText(", ", _cintBold, ref intStart, ref oDoc);
                    FormatBoldWordSymbol(thisQuantity, ref intStart, ref oDoc);

                    oPara4 = oDoc.Content.Paragraphs.Add(ref oMissing);
                    intStart = oPara4.Range.Start;
                    string strLead = "Value " + thisQuantity.Number.ToString11() + " ";
                    FormatWordText(strLead, 0, ref intStart, ref oDoc);
                    FormatWordUnitOfMeasure(thisQuantity.Unit, ref intStart, ref oDoc);
                    strLead = "\t\tLog " + thisQuantity.Number.LogToString11() + " ";
                    FormatWordText(strLead, 0, ref intStart, ref oDoc);
                }
            }
            object objSaveChanges = true;
            oDoc.Close(ref objSaveChanges, ref oMissing, ref oMissing);
            oWord.Quit(ref oMissing, ref oMissing, ref oMissing);
            oDoc = null;
            oWord = null;
            GC.Collect(); // force final cleanup
        }
        private void AllToWord(int intWordStart)
        {
            int q;

            for (q = intWordStart; q < _myQtyPointerList.Count; q++)
            {
                int intWordIndex = q + 1;
                this.txtWordIndex.Text = intWordIndex.ToString();
                OneToWord(q, true);
            }
        }
        private void OneToWord(int intQtyIndex, bool bAll)
        {
            bool bHideSense = this.chkHideTilda.Checked;
            object oMissing = System.Reflection.Missing.Value;
            object oEndOfDoc = "\\endofdoc"; // endofdoc is a predefined bookmark

            CQuantity thisQuantity = _myQtyPointerList[intQtyIndex].Qty;
            if (bAll && (thisQuantity.IsComputational || thisQuantity.IsSuppressed || thisQuantity.IsInteger))
            {
            }
            else
            {
                int intStart = 0;
                string strRichLine = thisQuantity.Symbol.SimpleLabel;
                this.rtWordSymbol.Clear();
                this.rtWordSymbol.AppendText(strRichLine);
                FormatRichTextSymbol(rtWordSymbol, thisQuantity, ref intStart);

                //Start Word and create a new document.
                Word._Application oWord;
                Word._Document oDoc;
                oWord = new Word.Application();

                oWord.Visible = true;
                oDoc = oWord.Documents.Add(ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing);


                string strPath;
                strPath = @"C:\Cosmolog\Formulas\" + thisQuantity.Description + ".docx";
                object objPath = strPath;
                oDoc.SaveAs(ref objPath, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                Word.Paragraph oPara4;
                oPara4 = oDoc.Content.Paragraphs.Add(ref oMissing);
                intStart = oPara4.Range.Start;

                // format the Description
                string strDescription = thisQuantity.Description;
                int intPos = strDescription.IndexOfAny(new char[] { '_', '^' });
                while (intPos > -1)
                {
                    string strLeft = strDescription.Substring(0, intPos);
                    FormatWordText(strLeft, _cintBold, ref intStart, ref oDoc);
                    string strToken = strDescription.Substring(intPos + 1, 1);
                    if (strDescription[intPos] == '_')
                        FormatWordText(strToken, _cintBold | _cintSubscript, ref intStart, ref oDoc);
                    else
                        FormatWordText(strToken, _cintBold | _cintSuperscript, ref intStart, ref oDoc);
                    strDescription = strDescription.Substring(intPos + 2);
                    intPos = strDescription.IndexOfAny(new char[] { '_', '^' });
                }
                if (strDescription.Length > 0)
                    FormatWordText(strDescription, _cintBold, ref intStart, ref oDoc);

                // format the value, unit of measure and log
                oPara4 = oDoc.Content.Paragraphs.Add(ref oMissing);
                intStart = oPara4.Range.Start;
                string strLead = "Value " + thisQuantity.Number.ToString11() + " ";
                FormatWordText(strLead, 0, ref intStart, ref oDoc);
                FormatWordUnitOfMeasure(thisQuantity.Unit, ref intStart, ref oDoc);
                strLead = "\t\tLog " + thisQuantity.Number.LogToString11() + " ";
                FormatWordText(strLead, 0, ref intStart, ref oDoc);

                // format the symbol and expressions in a table
                int intColCount = System.Convert.ToInt32(this.txtColumns.Text);
                int intRowCount = (thisQuantity.ExpressionList.CountPrintable() + intColCount - 1) / intColCount;
                Word.Table oTable = CreateTable(ref oWord, ref oDoc, intColCount + 1, intRowCount);

                int e, intRow, intCol;
                intRow = 1;
                intStart = oTable.Cell(intRow, 1).Range.Start;
                FormatWordSymbol(thisQuantity, ref intStart, ref oDoc);

                CExpressionList thisExpressionList = new CExpressionList(thisQuantity.ExpressionList, bHideSense);
                thisExpressionList.Sort();
                intCol = 2;
                for (e = 0; e < thisExpressionList.Count; ++e)
                {
                    CExpression thisExpression = thisExpressionList[e];
                    if ((!bHideSense || !thisExpression.ContainsSense()) && !thisExpression.ContainsSuppressed())
                    {
                        if (intCol > intColCount + 1)
                        {
                            ++intRow;
                            intCol = 2;
                        }
                        intStart = oTable.Cell(intRow, intCol).Range.Start;
                        FormatWordText("=", 0, ref intStart, ref oDoc);
                        FormatWordExpression(thisExpressionList[e], ref intStart, ref oDoc);
                        ++intCol;
                    }
                }
                thisExpressionList.Clear();
                thisExpressionList = null;  // Assist the garbage collector
                object objSaveChanges = true;
                oDoc.Close(ref objSaveChanges, ref oMissing, ref oMissing);
                oWord.Quit(ref oMissing, ref oMissing, ref oMissing);
                oDoc = null;
                oWord = null;
                GC.Collect(); // force final cleanup
            }
        }
        // Create a word table to handle the symbol and its expressions
        private Word.Table CreateTable(ref Word._Application oWord, ref Word._Document oDoc, int intColCount, int intRowCount)
        {
            object oMissing = System.Reflection.Missing.Value;
            object oEndOfDoc = "\\endofdoc"; // endofdoc is a predefined bookmark
            //Insert a 5 x n table, fill it with data
            Word.Table oTable;
            Word.Range wrdRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            oTable = oDoc.Tables.Add(wrdRng, intRowCount, intColCount, ref oMissing, ref oMissing);
            float fltPageWidth = (float)6.5;
            float fltSymbolWidth = (float).7;
            float fltExpressionWidth = (fltPageWidth - fltSymbolWidth) / (intColCount - 1);
            oTable.Columns[1].Width = oWord.InchesToPoints(fltSymbolWidth);
            for (int intCol = 2; intCol < intColCount + 1; ++intCol)
            {
                oTable.Columns[intCol].Width = oWord.InchesToPoints(fltExpressionWidth);
            }
            oTable.Range.ParagraphFormat.SpaceAfter = 0;
            return oTable;
        }
        private void FormatWordExpression(CExpression thisExpression, ref int intStart, ref Word._Document oDoc)
        {
            bool bParens = false;
            bool bPower = false;
            int i = 0;

            bPower = (thisExpression.Power != 1 || thisExpression.Root != 1);
            if (bPower)
                FormatWordText("(", 0, ref intStart, ref oDoc);

            if (thisExpression.Numerator.Count == 0)
                FormatWordText("1", 0, ref intStart, ref oDoc);
            else
            {
                for (i = 0; i < thisExpression.Numerator.Count; ++i)
                {
                    CFactor thisFactor = thisExpression.Numerator[i];
                    if (!FormatCompWordFactor(thisFactor, ref intStart, ref oDoc))
                        FormatWordFactor(thisFactor, ref intStart, ref oDoc);
                }
            }
            if (thisExpression.Denominator.Count > 0 && thisExpression.Denominator[0].Qty.Log != 0)
            {
                FormatWordText("/", 0, ref intStart, ref oDoc);
                bParens = (thisExpression.Denominator.Count > 1);
                if (bParens)
                    FormatWordText("(", 0, ref intStart, ref oDoc);
                for (i = 0; i < thisExpression.Denominator.Count; ++i)
                {
                    CFactor thisFactor = thisExpression.Denominator[i];
                    if (!FormatCompWordFactor(thisFactor, ref intStart, ref oDoc))
                        FormatWordFactor(thisFactor, ref intStart, ref oDoc);
                }
                if (bParens)
                    FormatWordText(")", 0, ref intStart, ref oDoc);
            }
            if (bPower)
            {
                FormatWordText(")", 0, ref intStart, ref oDoc);
                string strPower = thisExpression.PowerToString();
                FormatWordText(strPower, _cintSuperscript, ref intStart, ref oDoc);
            }
        }
        // If the factor is a computational quantity then expand it as an expression and return true
        // else return false.
        private bool FormatCompWordFactor(CFactor thisFactor, ref int intStart, ref Word._Document oDoc)
        {
            bool bIsCompFactor = false;
            int intQtyIndex = _ComputationalQuantityList.FindMatch(thisFactor.Label);
            if (intQtyIndex > -1)
            {
                CQuantity qtyComp = _ComputationalQuantityList[intQtyIndex];
                if (qtyComp.IsComputational && qtyComp.ExpressionList.Count > 0)
                {
                    CExpression expressionComp = qtyComp.ExpressionList[0];
                    FormatWordExpression(expressionComp, ref intStart, ref oDoc);
                    bIsCompFactor = true;
                }
            }
            return bIsCompFactor;
        }
        private void FormatBoldWordSymbol(CQuantity thisQuantity, ref int intStart, ref Word._Document oDoc)
        {
            if (thisQuantity.IsInteger)
            {
                FormatWordText(thisQuantity.Symbol.SimpleLabel, _cintBold, ref intStart, ref oDoc);
            }
            else
            {
                FormatWordText(thisQuantity.Symbol.Main, _cintBold | _cintItalic, ref intStart, ref oDoc);
                if (thisQuantity.Symbol.Subscript != "")
                {
                    FormatWordText(thisQuantity.Symbol.Subscript, _cintBold | _cintSubscript, ref intStart, ref oDoc);
                }
            }
        }
        private void FormatWordSymbol(CQuantity thisQuantity, ref int intStart, ref Word._Document oDoc)
        {
            if (thisQuantity.IsInteger)
            {
                FormatWordText(thisQuantity.Symbol.SimpleLabel, 0, ref intStart, ref oDoc);
            }
            else
            {
                FormatWordText(thisQuantity.Symbol.Main, _cintItalic, ref intStart, ref oDoc);
                if (thisQuantity.Symbol.Subscript != "")
                {
                    FormatWordText(thisQuantity.Symbol.Subscript, _cintSubscript, ref intStart, ref oDoc);
                }
            }
        }
        // format symbol and include exponent if found
        private void FormatWordFactor(CFactor thisFactor, ref int intStart, ref Word._Document oDoc)
        {
            FormatWordSymbol(thisFactor.Qty, ref intStart, ref oDoc);
            string strPower = thisFactor.PowerToString();
            if (strPower != "")
            {
                FormatWordText(strPower, _cintSuperscript, ref intStart, ref oDoc);
            }
        }
        private void FormatWordUnitOfMeasure(string strUnit, ref int intStart, ref Word._Document oDoc)
        {
            int intPos;
            int intEnd;
            string strSegment;
            string strPower;
            string strDigits = "-01234567890/";

            intPos = strUnit.IndexOf('^');
            while (intPos > -1)
            {
                strSegment = strUnit.Substring(0, intPos);
                FormatWordText(strSegment, 0, ref intStart, ref oDoc);
                intEnd = intPos + 1;
                while (intEnd < strUnit.Length && strDigits.IndexOf(strUnit[intEnd]) > -1)
                    ++intEnd;
                strPower = strUnit.Substring(intPos + 1, intEnd - intPos - 1);
                FormatWordText(strPower, _cintSuperscript, ref intStart, ref oDoc);
                strUnit = strUnit.Substring(intEnd);
                intPos = strUnit.IndexOf('^');
            }
            if (strUnit.Length > 0)
                FormatWordText(strUnit, 0, ref intStart, ref oDoc);
        }

        private void FormatWordText(string strText, int intFontSpec, ref int intStart, ref Word._Document oDoc)
        {
            Object objStart, objEnd;
            int intEnd;
            Word.Range rngInsert;

            intEnd = intStart;
            objStart = intStart;
            objEnd = intEnd;
            rngInsert = oDoc.Range(ref objStart, ref objEnd);
            rngInsert.Text = strText;
            rngInsert.Font.Name = "Times New Roman";
            rngInsert.Font.Size = 12;
            rngInsert.Font.Bold = ((intFontSpec & _cintBold) == _cintBold ? 1 : 0);
            rngInsert.Font.Italic = ((intFontSpec & _cintItalic) == _cintItalic ? 1 : 0); ;
            rngInsert.Font.Subscript = ((intFontSpec & _cintSubscript) == _cintSubscript ? 1 : 0);
            rngInsert.Font.Superscript = ((intFontSpec & _cintSuperscript) == _cintSuperscript ? 1 : 0);
            intStart = rngInsert.End;
        }

        #endregion

        private void cboSymbol_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object objSelection = this.cboSymbol.SelectedItem;
            string strSelection = objSelection.ToString();
            int intPos = strSelection.IndexOf('\t');
            if (intPos > -1)
            {
                string strLabel = strSelection.Substring(0, intPos);
                int intIndex = _myQtyPointerList.FindByQtyLabel(strLabel);
                if (intIndex > -1)
                    RepaintQuantityDisplay(intIndex,0,true);
            }
        }

        private void updownPage_ValueChanged(object sender, EventArgs e)
        {
            int intPtr = System.Convert.ToInt32(this.updownPage.Value);
            if (intPtr-1 != _intPageIndex)
                RepaintQuantityDisplay(_intQtyIndex, intPtr - 1, true);
        }

        private void chkHideTilda_CheckedChanged(object sender, EventArgs e)
        {
            RepaintQuantityDisplay(_intQtyIndex, 0, true);
        }

        private void btnValuesToWord_Click(object sender, EventArgs e)
        {
            ValuesToWord();
        }

        private void btnSuppressTautology_Click(object sender, EventArgs e)
        {
            CQuantity thisQuantity = _myQtyPointerList[_intQtyIndex].Qty;
            thisQuantity.ExpressionList.SuppressTautology(thisQuantity);
            RepaintQuantityDisplay(_intQtyIndex, 0, true);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            int intThisValue = 135;
            int intThatValue = 3;
            CPrimeFactorList thisList = new CPrimeFactorList(intThisValue);
            thisList.CalcLowestCommon(ref intThatValue);
            intThisValue = thisList.Value;
        }

        private void btnMakeOne_Click(object sender, EventArgs e)
        {
            int intIndex = this._myQuantityList.FindMatch("1");
            if (intIndex > -1)
            {
                CQuantity qtyOne = _myQuantityList[intIndex];
                qtyOne.ExpressionList.Clear();
                for (intIndex = 0; intIndex < _myQtyPointerList.Count && _myQtyPointerList[intIndex].Label != "1"; ++intIndex)
                {
                    CQuantity qtyThis = _myQtyPointerList[intIndex].Qty;
                    if (!qtyThis.IsComputational && !qtyThis.IsInteger && !qtyThis.IsSuppressed)
                    {
                        CMatrixCell cellThis = new CMatrixCell(qtyThis, qtyThis.InverseQty, true);
                        CExpression exprThis = new CExpression(cellThis, false);
                        qtyOne.ExpressionList.Add(exprThis);
                    }
                }
            }
            RepaintQuantityDisplay(_intQtyIndex, 0, false);
        }
    }
}
