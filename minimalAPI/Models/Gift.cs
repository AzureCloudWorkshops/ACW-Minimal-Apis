namespace minimalAPI.Models 
{
    public class Gift
    {
          public int Id { get; set; }
          public string Name { get; set; }
          public bool shouldBeBuilt { get; set; }
          public bool startedBuilding { get; set; }
          public bool isCompleted { get; set; }
    }
}