using System;
using Newtonsoft.Json;

namespace cosmos_client;

public class Item
{
    [JsonProperty("id")]
    public string ? Id {get;set;}
    public string ? Description {get;set;}
    public decimal? Price {get;set;}
    public string ? Name {get;set;}
}
