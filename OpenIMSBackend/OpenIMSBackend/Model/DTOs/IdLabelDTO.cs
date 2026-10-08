namespace OpenIMSBackend.Core.Model.DTOs
{
    /// <summary>Identifies an object by its id and describes it by a human-readable label, for example as an entry of a selection-list.</summary>
    public class IdLabelDTO
    {
        public string Id { get; set; }
        public string Label { get; set; }

        public IdLabelDTO(string id, string label)
        {
            this.Id = id;
            this.Label = label;
        }
    }
}
