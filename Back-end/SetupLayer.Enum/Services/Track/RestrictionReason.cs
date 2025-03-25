using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.Track
{
    public enum RestrictionReason
    {
        [EnumMember(Value = "Pending Approval")] // chờ content manager duyệt
        Pending,
        [EnumMember(Value = "No Restriction")] // ko bị cấm => KQ duyệt = Accept => IsPlayable = true
        None,
        // 4 LÝ DO BỊ CẤM
        [EnumMember(Value = "Restricted by Market")]
        Market,
        [EnumMember(Value = "Restricted by Product")]
        Product,
        [EnumMember(Value = "Restricted by Explicit")]
        Explicit,
        [EnumMember(Value = "Restricted by Other")]
        Other,
    }
}
