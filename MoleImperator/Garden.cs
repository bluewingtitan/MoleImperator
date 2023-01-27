namespace MoleImperator;

public class Garden
{
    public readonly Tile[] Tiles = new Tile[204];

    public readonly Dictionary<PlantType, int> SeedAmounts = new Dictionary<PlantType, int>();

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
    Weeds_S = 10000,
    Weeds_M = 10001,
    Weeds_L = 10002,
    Weeds_XL = 10003,
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
            { PlantType.Salad , new PlantTypeData{Type = PlantType.Salad,RefPrice = 0.08f, InGameName = "Salat", GrowTimeSeconds = 14*60, Offspring = 2}},
            { PlantType.Carrot , new PlantTypeData{Type = PlantType.Carrot,RefPrice = 0.07f, InGameName = "Karotte", GrowTimeSeconds = 10*60, Offspring = 2}},
            { PlantType.Cucumber , new PlantTypeData{Type = PlantType.Cucumber,RefPrice = 0.15f, InGameName = "Gurke", GrowTimeSeconds = 40*60, Offspring = 4}},
            { PlantType.Radish , new PlantTypeData{Type = PlantType.Radish,RefPrice = 0.20f, InGameName = "Radieschen", GrowTimeSeconds = 50*60, Offspring = 3}},
            { PlantType.Tomato , new PlantTypeData{Type = PlantType.Tomato,RefPrice = 0.49f, InGameName = "Tomate", GrowTimeSeconds = 2*60*60 + 20*60, Offspring = 4}},
            { PlantType.Strawberry , new PlantTypeData{Type = PlantType.Strawberry,RefPrice = 0.45f, InGameName = "Erdbeere", GrowTimeSeconds = 2*60*60, Offspring = 4}},
            { PlantType.Onion , new PlantTypeData{Type = PlantType.Onion,RefPrice = 0.45f, InGameName = "Zwiebel", GrowTimeSeconds = 8*60*60, Offspring = 4}},
            { PlantType.Spinach , new PlantTypeData{Type = PlantType.Spinach,RefPrice = 0.45f, InGameName = "Spinat", GrowTimeSeconds = 9*60*60 + 20*60, Offspring = 4}},
            
            
            { PlantType.Weeds_S , new PlantTypeData{Type = PlantType.Weeds_S,RefPrice = 2.5f, InGameName = "Unkraut", GrowTimeSeconds = -1, Offspring = 0}},
            { PlantType.Weeds_M , new PlantTypeData{Type = PlantType.Weeds_M,RefPrice = 50f, InGameName = "Stein", GrowTimeSeconds = -1, Offspring = 0}},
            { PlantType.Weeds_L , new PlantTypeData{Type = PlantType.Weeds_L,RefPrice = 250f, InGameName = "Baumstumpf", GrowTimeSeconds = -1, Offspring = 0}},
            { PlantType.Weeds_XL , new PlantTypeData{Type = PlantType.Weeds_XL,RefPrice = 500f, InGameName = "Maulwurf", GrowTimeSeconds = -1, Offspring = 0}},
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

    public PlantType Type { get; init; }
    public string JarSelector => $"#regal_{(uint)Type}";
    public string SeedAmountSelector => $"#regal_{(uint)Type} .anz";
    public float RefPrice { get; init; }
    public string InGameName { get; init; }
    public float GrowTimeSeconds { get; init; }
    public float Offspring { get; init; }
    public int XSize { get; init; } = 1;
    public int YSize { get; init; } = 1;

}

public enum WimpAnswer
{
    Deny,
    Wait,
    Accept
}

public class Wimp
{
    public float Payment { get; init; }
    public List<WimpRequest> Requests = new List<WimpRequest>();
    public float TotalMarketPrice => Requests.Sum(e => e.MarketPrice);
}

public struct WimpRequest
{
    public PlantType Type { get; init; }
    public int Amount { get; init; }
    public float MarketPrice => Data.RefPrice * Amount;
    public PlantTypeData Data => PlantTypeData.Data[Type];
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