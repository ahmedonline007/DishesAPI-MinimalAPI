using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DishesAPI.Entities
{
    public class Dish
    {
        public Dish()
        {

        }
        [SetsRequiredMembers]
        public Dish(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}
