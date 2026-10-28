using SportEvents.Lab02;
var z = new sportentry("S-1001","Іваненко");
z.Add("100 м",3,250m);
z.Add("200 м",12,90m);
Console.WriteLine(z.ProcessData(false));
Console.WriteLine(z.ProcessData(true));
Console.WriteLine(z.Ck());
Console.WriteLine(z.Chg(1));
Console.Write(z.Rep());
