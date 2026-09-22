namespace OpenIMSBackend.Core.Model.DTOs
{
    /// <summary>Transports a single string-value, so that an endpoint which only exchanges one value does not need a type of its own.</summary>
    public class StringValueDTO
    {
        public string Value { get; set; }
    }
}
