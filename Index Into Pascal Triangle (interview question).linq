<Query Kind="Program">
  <NuGetReference>Microsoft.Solver.Foundation</NuGetReference>
  <Namespace>Microsoft.SolverFoundation.Common</Namespace>
</Query>

// Find value of element of Pascal's Triangle given (row, col) index (0-based)
//
// For instance, row = 4 and col = 2 would be:
//
//     1      <== row #0
//    1 1     <== row #1
//   1 2 1    <== row #2
//  1 3 3 1   <== row #3
// 1 4 6 4 1  <== row #4, col #2 = 6
//
void Main()
{
	PascalIndex(4, 2).ToString().Dump();
	PascalIndex(10, 6).ToString().Dump(); // <== PascalIndex_Recursive can only get this far
	PascalIndex(104, 66).ToString().Dump();
	PascalIndex(200, 66).ToString().Dump();
	PascalIndex(2000, 666).ToString().Dump();
	PascalIndex(30000, 2222).ToString().Dump();
}

// Faster implementation able to handle arbitrarily large numbers (using Rational
// from Microsoft.SolverFoundation.Common assembly—https://www.nuget.org/packages/Microsoft.Solver.Foundation/)
public Rational PascalIndex(int row, int col)
{
	// Uses the "multiplicative formula" of Binomial Coefficents (see 
	// https://en.wikipedia.org/wiki/Binomial_coefficient#Multiplicative_formula)
	return Enumerable.Range(1, Math.Min(col, row - col))
		.Select(i => Rational.Get((BigInteger)(row + 1 - i), (BigInteger)i))
		.Aggregate((Rational)1, (x, acc) => x * acc);
}

// This recursive implementation quickly surpasses the int data type. (This was my initial
// solution.)
public int PascalIndex_Recursive(int row, int col)
{
	if (col == 0 || col == row)
		return 1;
		
	var value1 = PascalIndex_Recursive(row - 1, col - 1);
	var value2 = PascalIndex_Recursive(row - 1, col);
	
	return (value1 + value2);
}

