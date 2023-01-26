﻿namespace MoleImperator;

public class Garden
{
    public readonly Tile[] Tiles = new Tile[204];

    public Garden()
    {
        for (int i = 0; i < Tiles.Length; i++)
        {
            Tiles[i] = new Tile(i+1);
        }
    }
    
}

public enum PlantType: uint
{
    None = 0,
    Unknown = 9998,
    Weeds = 99999,
    Salad = 2,
    Carrot = 6,
    Cucumber = 12,
    Radish = 14,
    Strawberry = 3,
    Tomato = 5,
    Onion = 9,
    Spinach = 36,
    Marigold = 49,
    Potato = 22,
    Garlic = 35,
    Pepper = 15,
    Broccoli = 33,
    Asparagus = 20,
    Cauliflower = 32,
    Aubergine = 7,
    Zucchini = 16
}

public class PlantTypeData
{
    public static readonly Dictionary<PlantType, PlantTypeData> Data = GetData();

    private static Dictionary<PlantType, PlantTypeData> GetData()
    {
        var dict = new Dictionary<PlantType, PlantTypeData>()
        {
            { PlantType.Salad , new PlantTypeData{RefPrice = 0.05f, InGameName = "Salat", GrowTimeSeconds = 14*60, Offspring = 2}},
            { PlantType.Carrot , new PlantTypeData{RefPrice = 0.05f, InGameName = "Karotte", GrowTimeSeconds = 10*60, Offspring = 2}},
            { PlantType.Cucumber , new PlantTypeData{RefPrice = 0.05f, InGameName = "Gurke", GrowTimeSeconds = 40*60, Offspring = 4}},
            { PlantType.Radish , new PlantTypeData{RefPrice = 0.05f, InGameName = "Radieschen", GrowTimeSeconds = 50*60, Offspring = 3}},
        };
        return dict;
    }

    public static Dictionary<string, PlantType> Types = GetTypeMap();

    private static Dictionary<string, PlantType> GetTypeMap()
    {
        var dict = new Dictionary<string, PlantType>();

        foreach (var (type, data) in Data)
        {
            dict[data.InGameName] = type;
        }
        
        return dict;
    }

    private PlantTypeData()
    {
        
    }

    public float RefPrice { get; init; }
    public string InGameName { get; init; }
    public float GrowTimeSeconds { get; init; }
    public float Offspring { get; init; }
    public int XSize { get; init; } = 1;
    public int YSize { get; init; } = 1;

}

public class Tile
{
    public Tile(int tileId)
    {
        TileId = tileId;
    }

    public PlantType Type { get; private set; } = PlantType.Unknown;
    public PlantTypeData Data => PlantTypeData.Data[Type];
    public DateTime FinishedAt { get; private set; } = DateTime.MinValue;
    public bool IsFinished => DateTime.Now > FinishedAt;
    
    public int TileId { get; }
    public string Selector => $"#gardenTile{TileId}";

    public void Update(PlantType type, DateTime finishedAt)
    {
        Type = type;
        FinishedAt = finishedAt;
    }
}