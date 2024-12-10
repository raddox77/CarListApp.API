using System;

namespace CarListApp.API;

public class Car
{
    public int Id { get; set; }
    public string Make { get; set; }  = null!;  
    public string Model { get; set; } = null!;
    public string Vin { get; set; } = null!;

}
