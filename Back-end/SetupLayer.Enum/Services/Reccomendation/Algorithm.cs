using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.Reccomendation
{
    public enum Algorithm
    {
        [EnumMember(Value = "WeightedEuclideanDistance")]
        WeightedEuclideanDistance,
        [EnumMember(Value = "CosineSimilarity")]
        CosineSimilarity
    }
}
