# Cosmolog
Cosmolog (c) 2015-2019 Richard E. Knouse.
This is a Visual Studio 2017 C# solution.

This tool was used to generate the physical quantities and list of formulas used in Orest Bedrij's three
volume set: "'1' The Encyclopedia of Physical Laws." The entire list was marshalled through the MS Word
libary to produce a series of MS Word files, ready for publication and mathematically guaranteed to be
letter perfect.

The entire flow is controlled via the dialog form. This facilitates iteration of calculation, automatic
generation of new formulas and quantities and the resolution and elimination of duplicates. The flow starts
with reading of initialization files that include symbols and values. Any subset of the CODATA (Committee on
Data for Science and Technology of the International Council for Science) standard can
be used as a starting point. For practical purposes, a symbol has an ANSI form for internal manipulation and
a Unicode form for external publication. This way the ToString() method of any class can be read in a
dubugger. 

A single iteration recalculates the value of each quantity as an average of the computed values of each
expression associated with that quantity. Each iteration extends the precision of each quantity so that
successive iterations converge to the point that the truncation error associated with the fixed size of the
floating point buffer begins to dominate. Multiplicative error accumulation is minimized by performing all
multiplication as addition of logs. Conversion between log and value is performed on a just in time basis.
See the CNumber class.

Duplicate and equivalent expressions are suppressed by resolving each expression into a standard format.
Standard format means that the expression has at most one numerator and one denominator. The factors in 
numerator and denominator are sorted in a fixed order. Number factors are resolved to ratios of integers.
Ratios of factors that resolve to the value '1' are collapsed.

The form displays each quantity and a list of its expressions. Related quantities can be navigated to reveal
systems of quantity relationships even across disciplines. The calculated quantites can be stored as a text
file for later analysis. Expressions associated with a quantity can be published into a Word document.

One interesting result of this work is that spherical assumptions about a field in the near vicinity of a
subatomic particle break down completely. There is no such thing as a quantum spherical particle in the
classical sense. Indeed, Pi is only a valid concept when dealing with aggregate phenomena at the clasical
mechanical scale. Orest Bedrij proposes a new Quantization Constant to resolve the anomaly. I leave it to
the theoretical and experimental physicists to sort this one out.