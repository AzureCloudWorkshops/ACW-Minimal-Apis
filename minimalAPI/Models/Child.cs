namespace minimalAPI.Models 
{
    public class Child
    {
          public int Id { get; set; }
          public string Name { get; set; }
          public bool IsGood { get; set; }
          public bool checkedTwice { get; set; }
          public virtual Gift wantedItem { get; set; }
          public bool presentReadyForDelivery { get; set; }
    }
}