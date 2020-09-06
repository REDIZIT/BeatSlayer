using ProjectManagement;

namespace BeatSlayerServer.Dtos.Mapping
{
    public class DifficultyData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stars { get; set; }
        public float CubesSpeed { get; set; }

        public DifficultyData() { }

        public DifficultyData(DifficultyInfo original)
        {
            Id = original.id;
            Name = original.name;
            Stars = original.stars;
            CubesSpeed = 1; // Unknown
        }
    }
}
