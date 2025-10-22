namespace ErpBE.Domain.CommonDto
{
    public class LineQueryParameters : QueryParameters
    {
        public string? LineName { get; set; }
        
        public override void Validate()
        {
            base.Validate();
            
            // Add any line-specific validation here
        }
    }
}
