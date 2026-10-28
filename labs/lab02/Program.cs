using SportEvents.Lab02;
var registration = new SportRegistration("S-1001", "Іваненко");
registration.AddLine("100 м", 3, 250m);
registration.AddLine("200 м", 12, 90m);
Console.WriteLine(registration.CalculateTotal(false));
Console.WriteLine(registration.CalculateTotal(true));
Console.WriteLine(registration.IsValid());
Console.WriteLine(registration.TryChangeStatus(1));
Console.Write(registration.BuildReport());
