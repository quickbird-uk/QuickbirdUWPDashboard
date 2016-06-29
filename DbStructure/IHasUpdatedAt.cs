using System;
using System.ComponentModel.DataAnnotations;

namespace DbStructure
{
    public interface IHasUpdatedAt
    {
        [Required]
        DateTimeOffset UpdatedAt { get; } 
    }
}
