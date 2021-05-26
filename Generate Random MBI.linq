<Query Kind="Statements" />

var random = new Random();
var cValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
var nValues = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
var aValues = new[] { 'A', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'T', 'U', 'V', 'W', 'X', 'Y' };
var anValues = aValues.Concat(nValues.Select(x => (char)(x + 48))).ToArray();

Func<string> c = () => (cValues)[random.Next(cValues.Length)].ToString();
Func<string> n = () => (nValues)[random.Next(nValues.Length)].ToString();
Func<string> a = () => (aValues)[random.Next(aValues.Length)].ToString();
Func<string> an = () => (anValues)[random.Next(anValues.Length)].ToString();

var mbi = $"{c()}{a()}{an()}{n()}{a()}{an()}{n()}{a()}{a()}{n()}{n()}";

Console.WriteLine(mbi);
