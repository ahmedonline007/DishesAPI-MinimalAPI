using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DishesAPI.Entities
{
    public class Ingredient
    {
        public Ingredient()
        {
        }
        [SetsRequiredMembers]
        public Ingredient(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(200)]
        public required string Name { get; set; }
        public ICollection<Dish> Dishes { get; set; } = new HashSet<Dish>();


    }
}
