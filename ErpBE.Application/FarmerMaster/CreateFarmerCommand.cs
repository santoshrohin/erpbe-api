using MediatR;

namespace ErpBE.Application.FarmerMaster
{
    public class CreateFarmerCommand : IRequest<int>
    {
        public string FarmerName { get; set; }
        public string FarmerCode { get; set; }
        public string FarmerAddress { get; set; }
        public int BranchId { get; set; }
        public int LineId { get; set; }
    }
}
