namespace ErpBE.Domain.CommonDto
{
    public class BranchQueryParameters : QueryParameters
    {
        public string? BranchName { get; set; }
        
        public override void Validate()
        {
            base.Validate();
            
            // Add any branch-specific validation here
        }
    }
}
